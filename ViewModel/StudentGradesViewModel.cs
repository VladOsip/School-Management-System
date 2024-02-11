using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using EasySchool.Model;
using EasySchool.ViewModel.Utilities;

namespace EasySchool.ViewModel
{
    /// <summary>
    /// Student's grades summary page.
    /// </summary>
    public class StudentGradesViewModel : BaseViewModel, IScreenViewModel
    {
        #region Structures
        /// <summary>
        /// A sub-structure that represents information for a given grade
        /// </summary>
        public struct GradeData
        {
            public int CourseID { get; set; }
            public int TeacherID { get; set; }
            public string CourseName { get; set; }
            public int Score { get; set; }
            public string TeacherNotes { get; set; }
        }
        #endregion

        #region Fields
        private List<Student> _students;
        private List<GradeData> _grades;

        private Student _currentStudent;
        private GradeData _selectedGrade;

        private double _averageGrade;
        private int _absences;
        private string _homeroomTeacher;

        private string _appealText;

        private ICommand _changeStudentCommand;
        private ICommand _appealGradeCommand;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "Student Summary"; } }

        // Commands

        /// <summary>
        /// Changes what student is being viewed
        /// </summary>
        public ICommand ChangeStudentCommand
        {
            get
            {
                if (_changeStudentCommand == null)
                {
                    _changeStudentCommand = new RelayCommand(
                        p => ChangeStudent((Student)p),
                        p => p is Student);
                }

                return _changeStudentCommand;
            }
        }

        /// <summary>
        /// Challenge an issued grade
        /// </summary>
        public ICommand AppealGradeCommand
        {
            get
            {
                if (_appealGradeCommand == null)
                {
                    _appealGradeCommand = new RelayCommand(p => AppealGrade());
                }

                return _appealGradeCommand;
            }
        }

        // Business Logic Properties

        public List<Student> Students
        {
            get
            {
                return _students;
            }
            set
            {
                if (_students != value)
                {
                    _students = value;
                    OnPropertyChanged("Students");
                }
            }
        }

        public Student CurrentStudent
        {
            get
            {
                return _currentStudent;
            }
            set
            {
                if (_currentStudent != value)
                {
                    _currentStudent = value;
                    OnPropertyChanged("CurrentStudent");

                    // Show student grades
                    Grades = _currentStudent.Grades.Select(score =>
                        new GradeData() 
                        { 
                            CourseID = score.courseID, 
                            TeacherID = score.teacherID,
                            CourseName = score.Course.courseName, 
                            Score = score.score, 
                            TeacherNotes = score.notes
                        }).ToList();

                    Absences = _currentStudent.absencesCounter;

                    // If a homeroom teacher exists, show them
                    if (_currentStudent.Class.Teachers.Count > 0)
                    {
                        HomeroomTeacher = "Home Teacher: " +
                                    _currentStudent.Class.Teachers.First().Person.firstName +
                                    " " + _currentStudent.Class.Teachers.First().Person.lastName;
                    }
                    else
                    {
                        HomeroomTeacher = string.Empty;
                    }
                }
            }
        }

        public List<GradeData> Grades
        {
            get
            {
                return _grades;
            }
            set
            {
                if (_grades != value)
                {
                    _grades = value;
                    OnPropertyChanged("Grades");

                    // Update the average grade
                    if (_grades != null && _grades.Count > 0)
                    {
                        AverageGrade =  Math.Round(_grades.Average(x => x.Score), 1);
                    }
                    else
                    {
                        AverageGrade = 0;
                    }
                }
            }
        }
        public GradeData SelectedGrade
        {
            get
            {
                return _selectedGrade;
            }
            set
            {
                // Note: there might be a different student at the same course, so we update every time
                _selectedGrade = value;
                OnPropertyChanged("SelectedGrade");
            }
        }

        public double AverageGrade
        {
            get
            {
                return _averageGrade;
            }
            set
            {
                if (_averageGrade != value)
                {
                    _averageGrade = value;
                    OnPropertyChanged("AverageGrade");
                }
            }
        }

        public int Absences
        {
            get
            {
                return _absences;
            }
            set
            {
                if (_absences != value)
                {
                    _absences = value;
                    OnPropertyChanged("Absences");
                }
            }
        }

        public string HomeroomTeacher
        {
            get
            {
                return _homeroomTeacher;
            }
            set
            {
                if (_homeroomTeacher != value)
                {
                    _homeroomTeacher = value;
                    OnPropertyChanged("HomeroomTeacher");
                }
            }
        }

        public bool CanViewDifferentStudents { get; private set; }

        public bool CanAppealGrades { get; set; }

        public string AppealText
        {
            get
            {
                return _appealText;
            }
            set
            {
                if (_appealText != value)
                {
                    _appealText = value;
                    OnPropertyChanged("AppealText");
                }
            }
        }
        #endregion

        #region The constructor
        public StudentGradesViewModel(Person connectedPerson, IMessageBoxService messageBoxService)
            : base(messageBoxService)
        {
            // The only people who can't view this are teachers who aren't homeroom teachers
            if (!connectedPerson.isTeacher || (connectedPerson.isTeacher && connectedPerson.Teacher.classID != null))
            {
                HasRequiredPermissions = true;
            }

            Students = new List<Student>();
            Grades = new List<GradeData>();
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;

            if (HasRequiredPermissions)
            {
                // Reset student list
                Students.Clear();

                // Initialize lists based on the user's type
                if (ConnectedPerson.isStudent)
                {
                    // A student can see and appeal only their own grades
                    Students.Add(ConnectedPerson.Student);
                    CanViewDifferentStudents = false;
                    CanAppealGrades = true;
                }
                // Homeroom teacher can see the grades of everyone in their class
                else if (ConnectedPerson.isTeacher && ConnectedPerson.Teacher.classID != null)
                {
                    Students.AddRange(ConnectedPerson.Teacher.Class.Students.Where(student => student.Person.User.isDisabled == false));
                    CanViewDifferentStudents = true;
                    CanAppealGrades = false;
                }
                // A parent can see the grades of all of THEIR children
                else if (ConnectedPerson.isParent)
                {
                    Students.AddRange(ConnectedPerson.ChildrenStudents.Where(student => student.Person.User.isDisabled == false));
                    CanViewDifferentStudents = true;
                    CanAppealGrades = false;
                }
                // Management can see the grades of every student
                else if (ConnectedPerson.isPrincipal || ConnectedPerson.isSecretary)
                {
                    SchoolEntities schoolData = new SchoolEntities();
                    Students.AddRange(schoolData.Students.Where(student => student.Person.User.isDisabled == false));
                    CanViewDifferentStudents = true;
                    CanAppealGrades = false;
                }

                CurrentStudent = Students.First();
            }
        }

        /// <summary>
        /// Changes the selected student
        /// </summary>
        /// <param name="newStudent">The student whose grades are yet to be shown</param>
        private void ChangeStudent(Student newStudent)
        {
            // check if student is viable to be shown
            if (Students.Contains(newStudent))
            {
                CurrentStudent = newStudent;
            }
        }

        /// <summary>
        /// Generate an appeal message on behalf of a student
        /// </summary>
        private void AppealGrade()
        {
            // Verify grade selection
            if (SelectedGrade.CourseName != string.Empty)
            {
                // Verify appeal content
                if (AppealText.Count() > 0)
                {
                    // Send message and report it to the student
                    MessagesHandler.CreateMessage("Appeal Message", AppealText, MessageRecipientsTypes.Person, ConnectedPerson.personID, SelectedGrade.TeacherID);
                    _messageBoxService.ShowMessage("Appeal sent successfully in " + SelectedGrade.CourseName, "Appeal sent",
                                                    MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);

                    // Clear text
                    AppealText = string.Empty;
                }
                else
                {
                    _messageBoxService.ShowMessage("Failed to send an appeal", "action failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
            }
            else
            {
                _messageBoxService.ShowMessage("Please choose a course before appealing", "Action failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }
        #endregion
    }
}
