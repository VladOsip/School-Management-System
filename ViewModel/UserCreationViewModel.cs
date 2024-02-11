using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using EasySchool.Model;
using EasySchool.ViewModel.Utilities;

namespace EasySchool.ViewModel
{
    public class UserCreationViewModel : BaseViewModel, IScreenViewModel
    {
        #region Fields
        private ICommand _registerUserCommand;
        private ICommand _refreshDataCommand;

        private bool _isNewStudent;
        private bool _isNewTeacher;
        private bool _isNewParent;
        private bool _isNewSecretary;

        private const int FIELD_NOT_SET = -1;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "Create Users"; } }

        // Commands

        /// <summary>
        /// Create a user per the screen's properties
        /// </summary>
        public ICommand RegisterUserCommand
        {
            get
            {
                if (_registerUserCommand == null)
                {
                    _registerUserCommand = new RelayCommand(
                        p => RegisterUser());
                }

                return _registerUserCommand;
            }
        }

        // Business Logic Properties
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime Birthdate { get; set; }
        public bool CanCreateSecretaries { get; private set; }

        public bool IsNewSecretary
        {
            get
            {
                return _isNewSecretary;
            }
            set
            {
                if (_isNewSecretary != value)
                {
                    _isNewSecretary = value;
                    OnPropertyChanged("IsNewSecretary");
                }
            }
        }
        public bool IsNewTeacher
        {
            get
            {
                return _isNewTeacher;
            }
            set
            {
                if (_isNewTeacher != value)
                {
                    _isNewTeacher = value;
                    OnPropertyChanged("IsNewTeacher");
                }
            }
        }
        public bool IsNewParent
        {
            get
            {
                return _isNewParent;
            }
            set
            {
                if (_isNewParent != value)
                {
                    _isNewParent = value;
                    OnPropertyChanged("IsNewParent");
                }
            }
        }
        public bool IsNewStudent 
        { 
            get
            {
                return _isNewStudent;
            }
            set
            {
                if (_isNewStudent != value)
                {
                    _isNewStudent = value;
                    OnPropertyChanged("IsNewStudent");
                }
            }
        }

        public Dictionary<int, string> AvailableClasses { get; set; }
        public Dictionary<int, string> AvailableHomeroomClasses { get; set; }
        public Nullable<int> SelectedClass { get; set; }
        public Dictionary<int, string> AvailableParents { get; set; }
        public Nullable<int> SelectedParent { get; set; }
        
        public Dictionary<int, string> AvailableStudents { get; set; }
        public Nullable<int> SelectedStudent { get; set; }

        public Dictionary<int, string> AvailableCourses { get; set; }
        public Dictionary<int, string> AvailableCoursesMustChoose { get; set; }
        public Nullable<int> SelectedCourse1 { get; set; }
        public Nullable<int> SelectedCourse2 { get; set; }
        public Nullable<int> SelectedCourse3 { get; set; }
        public Nullable<int> SelectedCourse4 { get; set; }
        public Nullable<int> SelectedHomeroomClass { get; set; }
        #endregion

        #region The constructor
        public UserCreationViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
            : base(messageBoxService)
        {
            if (connectedPerson.isSecretary || connectedPerson.isPrincipal)
            {
                HasRequiredPermissions = true;
                _refreshDataCommand = refreshDataCommand;
                
                // Note: only the principal can create managers
                if (connectedPerson.isPrincipal)
                {
                    CanCreateSecretaries = true;
                }
                else
                {
                    CanCreateSecretaries = false;
                }

                AvailableClasses = new Dictionary<int, string>();
                AvailableCourses = new Dictionary<int, string>();
                AvailableParents = new Dictionary<int, string>();
                AvailableHomeroomClasses = new Dictionary<int, string>();
                AvailableCoursesMustChoose = new Dictionary<int, string>();
                AvailableStudents = new Dictionary<int, string>();
            }
            else
            {
                HasRequiredPermissions = false;
                CanCreateSecretaries = false;
            }
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;

            ResetAll();

            if (HasRequiredPermissions)
            {
                SchoolEntities schoolData = new SchoolEntities();

                // Generate list of classes
                schoolData.Classes.ToList().ForEach(currClass => AvailableClasses.Add(currClass.classID, currClass.className));

                AvailableHomeroomClasses.Add(FIELD_NOT_SET, "Not defined");
                schoolData.Classes.Where(currClass => currClass.Teachers.Count() == 0).ToList()
                    .ForEach(currClass => AvailableHomeroomClasses.Add(currClass.classID, currClass.className));

                // Generate list of parents
                AvailableParents.Add(FIELD_NOT_SET, "Not defined");
                schoolData.Persons.Where(p => p.isParent).ToList()
                    .ForEach(parent => AvailableParents.Add(parent.personID, parent.firstName + " " + parent.lastName));

                // Generate list of students
                schoolData.Persons.Where(p => p.isStudent).ToList()
                    .ForEach(student => AvailableStudents.Add(student.personID, student.firstName + " " + student.lastName));

                // Generate list of courses
                schoolData.Courses.Where(course => course.isHomeroomTeacherOnly == false).ToList()
                    .ForEach(course => AvailableCoursesMustChoose.Add(course.courseID, course.courseName));
                AvailableCourses.Add(FIELD_NOT_SET, "Not defined");
                AvailableCoursesMustChoose.ToList().ForEach(course => AvailableCourses.Add(course.Key, course.Value));
            }
        }

        // Clear ViewModel data
        private void ResetAll()
        {
            // Reset lists
            AvailableClasses.Clear();
            AvailableParents.Clear();
            AvailableStudents.Clear();
            AvailableCourses.Clear();
            AvailableCoursesMustChoose.Clear();
            AvailableHomeroomClasses.Clear();

            // Reset properties
            Username = "";
            FirstName = "";
            LastName = "";
            Email = "";
            Phone = "";
            Birthdate = new DateTime();

            IsNewStudent = false;
            IsNewTeacher = false;
            IsNewParent = false;
            IsNewSecretary = false;

            SelectedHomeroomClass = null;
            SelectedParent = null;
            SelectedStudent = null;
            SelectedClass = null;
            SelectedCourse1 = null;
            SelectedCourse2 = null;
            SelectedCourse3 = null;
            SelectedCourse4 = null;
        }

        /// <summary>
        /// Register user with a given input
        /// </summary>
        private void RegisterUser()
        {
            // Check user permissions
            if (HasRequiredPermissions)
            {
                var validInput = VerifyInput();

                // validate input
                if (validInput.Valid)
                {
                    SchoolEntities schoolData = new SchoolEntities();

                    // Create User
                    User newUser = new User()
                    {
                        username = Username,
                        password = "123456",
                        hasToChangePassword = true,
                        isDisabled = false
                    };
                    schoolData.Users.Add(newUser);
                    schoolData.SaveChanges();

                    // Create a Person ffrom the given information
                    Person newPerson = new Person()
                    {
                        userID = newUser.userID,

                        firstName = FirstName,
                        lastName = LastName,
                        phoneNumber = Phone,
                        email = Email,
                        birthdate = Birthdate,
                        
                        isStudent = IsNewStudent,
                        isParent = IsNewParent,
                        isTeacher = IsNewTeacher,
                        isSecretary = IsNewSecretary,
                        isPrincipal = false
                    };
                    schoolData.Persons.Add(newPerson);
                    schoolData.SaveChanges();

                    // additional qualification based on if the user is a student
                    if (IsNewStudent)
                    {
                        Student newStudent = new Student()
                        {
                            studentID = newPerson.personID,
                            classID = SelectedClass.Value,
                            parentID = (SelectedParent != null && SelectedParent != FIELD_NOT_SET) ? SelectedParent : null,
                            absencesCounter = 0
                        };
                        schoolData.Students.Add(newStudent);
                        schoolData.SaveChanges();
                    }

                    // additional qualification based on if the user is a parents
                    if (IsNewParent)
                    {
                        schoolData.Students.Find(SelectedStudent.Value).parentID = newPerson.personID;
                    }

                    // additional qualification based on if the user is a teacher
                    if (IsNewTeacher)
                    {
                        Teacher newTeacher = new Teacher()
                        {
                            teacherID = newPerson.personID,
                            classID = (SelectedHomeroomClass != null && SelectedHomeroomClass != FIELD_NOT_SET) ? SelectedHomeroomClass : null,
                            firstCourseID = SelectedCourse1.Value,
                            secondCourseID = (SelectedCourse2 != null && SelectedCourse2 != FIELD_NOT_SET) ? SelectedCourse2 : null,
                            thirdCourseID = (SelectedCourse3 != null && SelectedCourse3 != FIELD_NOT_SET) ? SelectedCourse3 : null,
                            fourthCourseID = (SelectedCourse4 != null && SelectedCourse4 != FIELD_NOT_SET) ? SelectedCourse4 : null
                        };
                        schoolData.Teachers.Add(newTeacher);
                        schoolData.SaveChanges();
                    }

                    _messageBoxService.ShowMessage("The user " + newPerson.firstName + " " + newPerson.lastName + " has been registered successfully",
                        "Creation successful", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                    _refreshDataCommand.Execute(null);
                }
                else
                {
                    _messageBoxService.ShowMessage(validInput.ErrorReport, "registration failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
            }
        }

        /// <summary>
        /// Validate input
        /// </summary>
        /// <returns>issues for users if they exist</returns>
        private ValidityResult VerifyInput()
        {
            ValidityResult result = new ValidityResult();
            result.Valid = true;

            // Check that a username was written
            if (Username == null || Username.Length == 0)
            {
                result.ErrorReport = "Please input Username";
                result.Valid = false;
            }
            else if (Username.Length < Globals.MINIMUM_USERNAME_LENGTH || Username.Length > Globals.MAXIMUM_USERNAME_LENGTH)
            {
                result.ErrorReport = string.Format("Error - Username needs to be between {0} and {1} characters",
                                                    Globals.MINIMUM_USERNAME_LENGTH, Globals.MAXIMUM_USERNAME_LENGTH);
                result.Valid = false;
            }
            else if (FirstName == null || FirstName.Length == 0)
            {
                result.ErrorReport = "Please input first name";
                result.Valid = false;
            }
            else if (LastName == null || LastName.Length == 0)
            {
                result.ErrorReport = "Please input last name";
                result.Valid = false;
            }
            else if (!IsNewStudent && !IsNewParent && !IsNewTeacher && !IsNewSecretary)
            {
                result.ErrorReport = "Please choose user type";
                result.Valid = false;
            }
            else if (IsNewTeacher && (SelectedCourse1 == null || SelectedCourse1 == FIELD_NOT_SET))
            {
                result.ErrorReport = "Please choose at least one course for the teacher";
                result.Valid = false;
            }
            else if (IsNewParent && (SelectedStudent == null || SelectedStudent == FIELD_NOT_SET))
            {
                result.ErrorReport = "Please choose a child for the parent";
                result.Valid = false;
            }
            else if (IsNewStudent && (SelectedClass == null || SelectedClass == FIELD_NOT_SET))
            {
                result.ErrorReport = "Please choose class for student";
                result.Valid = false;
            }

            return result;
        }
        #endregion
    }
}
