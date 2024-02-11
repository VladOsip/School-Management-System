using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using EasySchool.Model;
using EasySchool.ViewModel.Utilities;

namespace EasySchool.ViewModel
{
    /// <summary>
    /// Allows a teacher to set the grades of his students in a specific class & course
    /// </summary>
    public class GradesReportViewModel : BaseViewModel, IScreenViewModel
    {
        #region Structures
        /// <summary>
        /// A sub-structure that represents information for a given student's grade
        /// </summary>
        public class GradedStudent
        {
            public int StudentID { get; set; }
            public string Name { get; set; }
            public int Score { get; set; }
            public string Notes { get; set; }

            // These properties are used to check for changes
            public int OriginalScore { get; set; }
            public string OriginalNotes { get; set; }
        }
        #endregion

        #region Fields
        private ICommand _refreshDataCommand;
        private ICommand _updateGradesCommand;

        private SchoolEntities _schoolData;

        private ObservableDictionary<int, string> _classes;
        private ObservableDictionary<int, string> _courses;
        private int _selectedClass;
        private int _selectedCourse;

        private ObservableCollection<GradedStudent> _studentGrades;

        private const int NOT_ASSIGNED = -1;
        #endregion

        #region Properties / Commands

        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "Write Grades"; } }

        // Commands

        /// <summary>
        /// Update the grades of selected students
        /// </summary>
        public ICommand UpdateGradesCommand
        {
            get
            {
                if (_updateGradesCommand == null)
                {
                    _updateGradesCommand = new RelayCommand(p => UpdateGrades());
                }
                return _updateGradesCommand;
            }
        }

        // Business Logic 
        public ObservableCollection<GradedStudent> StudentsGrades
        {
            get
            {
                return _studentGrades;
            }
            set
            {
                if (_studentGrades != value)
                {
                    _studentGrades = value;
                    OnPropertyChanged("StudentsGrades");
                }
            }
        }
        public ObservableDictionary<int, string> Courses
        {
            get
            {
                return _courses;
            }
            set
            {
                if (_courses != value)
                {
                    _courses = value;
                    OnPropertyChanged("Courses");
                }
            }
        }
        public int SelectedCourse
        {
            get
            {
                return _selectedCourse;
            }
            set
            {
                if (_selectedCourse != value)
                {
                    _selectedCourse = value;
                    OnPropertyChanged("SelectedCourse");

                    if (_selectedCourse != NOT_ASSIGNED)
                    {
                        FindClassForCourse(ConnectedPerson.Teacher.teacherID, _selectedCourse);
                    }
                }
            }
        }
        public ObservableDictionary<int, string> Classes
        {
            get
            {
                return _classes;
            }
            set
            {
                if (_classes != value)
                {
                    _classes = value;
                    OnPropertyChanged("Classes");
                }
            }
        }
        public int SelectedClass
        {
            get
            {
                return _selectedClass;
            }
            set
            {
                // NOTE: We might use a class after changing courses, so we allow reselection 
                _selectedClass = value;
                OnPropertyChanged("SelectedClass");

                FindClassStudents(_selectedClass);
            }
        }
        #endregion

        #region The constructor
        public GradesReportViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
            : base(messageBoxService)
        {
            _refreshDataCommand = refreshDataCommand;

            // Only teachers have access
            if (connectedPerson.isTeacher)
            {
                HasRequiredPermissions = true;
            }
            else
            {
                HasRequiredPermissions = false;
            }
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;

            // Initalize all lists
            Courses = new ObservableDictionary<int, string>();
            Classes = new ObservableDictionary<int, string>();
            StudentsGrades = new ObservableCollection<GradedStudent>();
            _schoolData = new SchoolEntities();

            SelectedCourse = NOT_ASSIGNED;
            SelectedClass = NOT_ASSIGNED;

            // Get the teacher's data
            Teacher _teacherInformation = ConnectedPerson.Teacher;
            if (_teacherInformation != null)
            {
                // Reminder: classes depend on the selected course
                GatherTeacherCourses(_teacherInformation);
            }
            else
            {
                _messageBoxService.ShowMessage("Teacher permissions error", "Error", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }

        /// <summary>
        /// List all courses that a given teacher teaches
        /// </summary>
        /// <param name="teacher">A given teacher</param>
        private void GatherTeacherCourses(Teacher teacher)
        {
            // Reset the Courses collection to fit the current teacher
            Courses = new ObservableDictionary<int, string>(TeacherCoursesHandler.GetTeacherCoursesNames(teacher, true));

            // Automatically select a course if possible
            if (Courses.Count() > 0)
            {
                SelectedCourse = Courses.First().Key;
            }
            else
            {
                SelectedCourse = NOT_ASSIGNED;
            }

            // Due to an unknown, property changes need to be raised manually
            OnPropertyChanged("SelectedCourse");
        }

        /// <summary>
        /// List all student from a specific class
        /// </summary>
        /// <param name="classID">The ID of the class</param>
        private void FindClassStudents(int classID)
        {
            if (classID != NOT_ASSIGNED)
            {
                StudentsGrades =
                    new ObservableCollection<GradedStudent>(_schoolData.Classes.Find(classID).Students.Where(student => !student.Person.User.isDisabled)
                        .Select(student => DbStudentToGradedStudent(student, SelectedCourse)));
            }
            else
            {
                //clear list of students
                StudentsGrades = new ObservableCollection<GradedStudent>();
            }
        }

        /// <summary>
        /// Converts the Database's "student" class into the local "GradedStudent" class
        /// </summary>
        /// <param name="student">Database Student</param>
        /// <param name="courseID">Wanted course ID</param>
        /// <returns>A GradedStudent version of this student</returns>
        private GradedStudent DbStudentToGradedStudent(Student student, int courseID)
        {
            GradedStudent gradedStudent = new GradedStudent();

            gradedStudent.StudentID = student.studentID;
            gradedStudent.Name = student.Person.firstName + " " + student.Person.lastName;

            // If student has a grade, retrieve it
            Grade currentGrade = _schoolData.Grades.Find(student.studentID, courseID);
            if (currentGrade != null)
            {
                gradedStudent.Score = currentGrade.score;
                gradedStudent.Notes = currentGrade.notes;
            }
            else
            {
                gradedStudent.Score = Globals.GRADE_NO_VALUE;
                gradedStudent.Notes = string.Empty;
            }

            gradedStudent.OriginalScore = gradedStudent.Score;
            gradedStudent.OriginalNotes = gradedStudent.Notes;

            return gradedStudent;
        }

        /// <summary>
        /// Update the grade of each student in a given class and course
        /// </summary>
        private void UpdateGrades()
        {
            // check if a course and class were selected
            if (SelectedClass != NOT_ASSIGNED && SelectedCourse != NOT_ASSIGNED)
            {
                bool didSendAnyReport = false;

                // Create the basic template for the report - lesson date and the reporter information
                string reportTemplate =
                    "Teacher " + ConnectedPerson.firstName + " " + ConnectedPerson.lastName +
                    " Has graded you in " + Courses[SelectedCourse] + ":\n";

                // Go over every student and check for reported students
                foreach (GradedStudent student in StudentsGrades)
                {
                    // If needed, update the grade
                    if (student.Score != student.OriginalScore || student.Notes != student.OriginalNotes)
                    {
                        // Get the student's information
                        Student studentInfo = _schoolData.Students.Find(student.StudentID);

                        // Report the grade
                        Grade grade = new Grade()
                        {
                            studentID = studentInfo.studentID,
                            courseID = SelectedCourse,
                            teacherID = ConnectedPerson.Teacher.teacherID,
                            score = Convert.ToByte(student.Score)
                        };
                        string gradeReport = reportTemplate;
                        gradeReport += "Grade: " + student.Score + "\n";

                        if (student.Notes != string.Empty)
                        {
                            gradeReport += "Notes: " + student.Notes + "\n";
                            grade.notes = student.Notes;
                        }

                        // Save the grade and inform the student
                        _schoolData.Grades.Add(grade);
                        MessagesHandler.CreateMessage("You recieved a grade", gradeReport, MessageRecipientsTypes.Person, null, student.StudentID);

                        // If applicable, inform listed parents as well
                        if (studentInfo.parentID.HasValue)
                        {
                            string parentReport = "Your child " + student.Name + " recieved a grade:\n" + gradeReport;
                            MessagesHandler.CreateMessage("Your child recieved a grade", parentReport, MessageRecipientsTypes.Person, null, studentInfo.parentID.Value);
                        }

                        didSendAnyReport = true;
                    }
                }

                // Check if any student report was required, and update accordingly
                if (didSendAnyReport)
                {
                    _schoolData.SaveChanges();
                    _refreshDataCommand.Execute(null);
                }

                // Report the success of the action to the teacher
                string resultMessage = "Grade for the course " + Courses[SelectedCourse] + " and class " + Classes[SelectedClass] 
                    + " were filled successfully";
                _messageBoxService.ShowMessage("Grades filed", resultMessage, MessageType.OK_MESSAGE);
            }
            else
            {
                // No class or course selected. Cannot send a report
                _messageBoxService.ShowMessage("Missing information", "Failed to update grades",
                                                MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }

        /// <summary>
        /// List all classes that a given teacher teaches for a specific course
        /// </summary>
        /// <param name="teacherID">The teacher's ID</param>
        /// <param name="courseID">The ID of the course</param>
        private void FindClassForCourse(int teacherID, int courseID)
        {
            // Reset the classes collection
            Classes.Clear();

            // Use HashSet to list each class that a teacher teaches only once
            var teacherClasses = _schoolData.Lessons.Where(lesson => lesson.courseID == courseID && lesson.teacherID == teacherID).
                                   Select(lesson => lesson.Class).ToHashSet();
            foreach (Class schoolClass in teacherClasses)
            {
                Classes.Add(schoolClass.classID, schoolClass.className);
            }

            // For homeroom courses, add teacher's homeroom class
            if (_schoolData.Courses.Find(courseID).isHomeroomTeacherOnly)
            {
                // verify relevant information
                Teacher teacherInfo = _schoolData.Teachers.Find(teacherID);
                if (teacherInfo.classID.HasValue && !Classes.ContainsKey(teacherInfo.classID.Value))
                {
                    Classes.Add(teacherInfo.Class.classID, teacherInfo.Class.className);
                }
            }

            // Automatically select a class if possible
            if (Classes.Count() > 0)
            {
                SelectedClass = Classes.First().Key;

                // Due to an unknown, property changes need to be raised manually
                OnPropertyChanged("SelectedClass");
            }
            else
            {
                SelectedClass = NOT_ASSIGNED;
            }
        }
        #endregion


    }
}
