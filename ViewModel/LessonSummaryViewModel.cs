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
    /// allow teachers to report missing students and update relevant parties
    /// </summary>
    public class LessonSummaryViewModel : BaseViewModel, IScreenViewModel
    {
        #region Structures
        /// <summary>
        /// A sub-structure that represents information for a given student's missing status
        /// </summary>
        public class StudentAtLesson
        {
            public int StudentID { get; set; }
            public string Name { get; set; }
            public bool WasMissing { get; set; }
            public string Notes { get; set; }
        }
        #endregion

        #region Fields

        private SchoolEntities _schoolData;

        private ICommand _refreshDataCommand;
        private ICommand _reportLessonCommand;

        private DateTime _selectedDate;
        private ObservableDictionary<int, string> _courses;
        private ObservableDictionary<int, string> _classes;
        private int _selectedCourse;
        private int _selectedClass;

        private ObservableCollection<StudentAtLesson> _studentsInLesson;

        private const int NOT_ASSIGNED = -1;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "Lesson Summary"; } }

        // Commands

        /// <summary>
        /// Send lesson report
        /// </summary>
        public ICommand ReportLessonCommand
        {
            get
            {
                if (_reportLessonCommand == null)
                {
                    _reportLessonCommand = new RelayCommand(p => ReportLesson());
                }
                return _reportLessonCommand;
            }
        }

        // Business Logic Properties 
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
                if (_selectedClass != value)
                {
                    _selectedClass = value;
                    OnPropertyChanged("SelectedClass");

                    if (_selectedClass != NOT_ASSIGNED)
                    {
                        FindClassStudents(_selectedClass);
                    }
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

        public DateTime SelectedDate
        { 
            get
            {
                return _selectedDate;
            }
            set
            {
                // note: Only date, no time
                if (_selectedDate != value.Date)
                {
                    _selectedDate = value.Date;
                    OnPropertyChanged("SelectedDate");
                }
            }
        }

        public ObservableCollection<StudentAtLesson> StudentsInLesson
        {
            get 
            {
                return _studentsInLesson;
            }
            set
            {
                if (_studentsInLesson != value)
                {
                    _studentsInLesson = value;
                    OnPropertyChanged("StudentsInLesson");
                }
            }
        }
        #endregion

        #region The Constructor
        public LessonSummaryViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
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
            StudentsInLesson = new ObservableCollection<StudentAtLesson>();
            _schoolData = new SchoolEntities();

            SelectedDate = DateTime.Now;
            SelectedCourse = NOT_ASSIGNED;
            SelectedClass = NOT_ASSIGNED;

            // Get teacher's data
            Teacher _teacherInformation = ConnectedPerson.Teacher;
            if (_teacherInformation != null)
            {
                // classes depend on the selected course
                GatherTeacherCourses(_teacherInformation);
            }
            else
            {
                _messageBoxService.ShowMessage("Teacher permissions error", "Error", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }

        /// <summary>
        /// Report each missing student
        /// </summary>
        private void ReportLesson()
        {
            // Check if there is a selected course/lesson to make a report for
            if (SelectedClass != NOT_ASSIGNED && SelectedCourse != NOT_ASSIGNED)
            {
                bool didSendAnyReport = false;

                // Create report template
                string reportTemplate =
                    "The Teacher " + ConnectedPerson.firstName + " " + ConnectedPerson.lastName +
                    " Wrote you notes at " + SelectedDate.ToString("dd/MM/yy") +
                    " for the course " + Courses[SelectedCourse] + ":\n";

                // iterate over all students to find reported students
                foreach (StudentAtLesson student in StudentsInLesson)
                {
                    if (student.WasMissing || student.Notes != string.Empty)
                    {
                        // Get full student information
                        Student studentInfo = _schoolData.Students.Find(student.StudentID);

                        // Generate a report
                        string report = reportTemplate;

                        if (student.WasMissing)
                        {
                            report += "You missed a lesson\n";

                            // increase counter of missed lessons
                            studentInfo.absencesCounter++;
                        }
                        if (student.Notes != string.Empty)
                        {
                            report += student.Notes + "\n";
                        }

                        // Report to the student
                        MessagesHandler.CreateMessage("You recieved a report for a lesson", report,
                                                        MessageRecipientsTypes.Person, ConnectedPerson.Teacher.teacherID, student.StudentID);

                        // If the student has any listed parents, report to them too
                        if (studentInfo.parentID.HasValue)
                        {
                            string parentReport = "Your child " + student.Name + " Reicved the following report:\n" + report;
                            MessagesHandler.CreateMessage("Your child recieved a report", parentReport,
                                                            MessageRecipientsTypes.Person,
                                                            ConnectedPerson.Teacher.teacherID, studentInfo.parentID.Value);
                        }

                        didSendAnyReport = true;
                    }
                }

                // Save changes and refresh
                if (didSendAnyReport)
                {
                    _schoolData.SaveChanges();
                    _refreshDataCommand.Execute(null);
                }

                // inform the teacher of the action's success
                string resultMessage = "A report was filed for the course " + Courses[SelectedCourse] + " for class " + Classes[SelectedClass];
                _messageBoxService.ShowMessage("Lesson report was filed", resultMessage, MessageType.OK_MESSAGE);
            }
            else
            {
                // No class or course selected. Cannot send a report
                _messageBoxService.ShowMessage("Course or Class were not selected", "Report failed",
                                                MessageType.OK_MESSAGE, MessagePurpose.ERROR);
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
        /// List all students from a specific class
        /// </summary>
        /// <param name="classID">The ID of the class</param>
        private void FindClassStudents(int classID)
        {
            StudentsInLesson =
                new ObservableCollection<StudentAtLesson>(_schoolData.Classes.Find(classID).Students.Where(student => !student.Person.User.isDisabled)
                .Select(student => new StudentAtLesson()
                {
                    StudentID = student.studentID,
                    Name = student.Person.firstName + " " + student.Person.lastName,
                    WasMissing = false,
                    Notes = string.Empty
                }));
        }

        /// <summary>
        /// List all classes of a given teacher for a specific course
        /// </summary>
        /// <param name="teacherID">The ID of the teacher</param>
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

                // For some reason the selections are not updated properly in the view unless called again
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
