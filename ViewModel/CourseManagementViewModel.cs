using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using EasySchool.Model;
using EasySchool.ViewModel.Utilities;

namespace EasySchool.ViewModel
{
    /// <summary>
    /// Manages the school's Courses - creating, editing and deleting courses
    /// </summary>
    public class CourseManagementViewModel : BaseViewModel, IScreenViewModel
    {
        #region Structures
        /// <summary>
        /// A sub-structure that represents information for a given class
        /// </summary>
        public class CourseData
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public bool IsHomeroomTeacherOnly { get; set; }
            public List<string> CourseTeachers { get; set; }
            public List<LessonsOfCourse> CourseLessons { get; set; }
        }
        /// <summary>
        /// A sub-structure meant to manage course schedueling
        /// </summary>
        public class LessonsOfCourse
        {
            public int LessonID { get; set; }
            public int ClassID { get; set; }
            public string ClassName { get; set; }
            public string TeacherName { get; set; }
            public string DayFirstLesson { get; set; }
            public string HourFirstLesson { get; set; }
            public string DaySecondLesson { get; set; }
            public string HourSecondLesson { get; set; }
            public string DayThirdLesson { get; set; }
            public string HourThirdLesson { get; set; }
            public string DayFourthLesson { get; set; }
            public string HourFourthLesson { get; set; }
        }
        #endregion

        #region Fields
        private ICommand _refreshDataCommand;
        private ICommand _createNewCourseCommand;
        private ICommand _updateCourseCommand;
        private ICommand _deleteCourseCommand;

        private SchoolEntities _schoolData;

        private CourseData _selectedCourse;
        private string _selectedCourseName;
        private bool _isSelectedCourseHomeroomOnly;

        public ObservableCollection<CourseData> _coursesTableData;
        private ObservableCollection<string> _teachersOfSelectedCourse;
        private ObservableCollection<LessonsOfCourse> _lessonsOfSelectedCourse;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "Manage Courses"; } }

        // Commands

        /// <summary>
        /// Create a new course with given data
        /// </summary>
        public ICommand CreateNewCourseCommand
        {
            get
            {
                if (_createNewCourseCommand == null)
                {
                    _createNewCourseCommand = new RelayCommand(p => CreateNewCourse());
                }
                return _createNewCourseCommand;
            }
        }

        /// <summary>
        /// Update selected course
        /// </summary>
        public ICommand UpdateCourseCommand
        {
            get
            {
                if (_updateCourseCommand == null)
                {
                    _updateCourseCommand = new RelayCommand(p => UpdateSelectedCourse());
                }
                return _updateCourseCommand;
            }
        }

        /// <summary>
        /// Delete selected course
        /// </summary>
        public ICommand DeleteCourseCommand
        {
            get
            {
                if (_deleteCourseCommand == null)
                {
                    _deleteCourseCommand = new RelayCommand(p => DeleteSelectedCourse());
                }
                return _deleteCourseCommand;
            }
        }

        // Business Logic Properties
        public CourseData SelectedCourse
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
                    UseSelectedCourse(_selectedCourse);
                    OnPropertyChanged("SelectedCourse");
                }
            }
        }
        public ObservableCollection<CourseData> CoursesTableData 
        { 
            get
            {
                return _coursesTableData;
            }
            set
            {
                if (_coursesTableData != value)
                {
                    _coursesTableData = value;
                    OnPropertyChanged("CoursesTableData");
                }
            }
        }
        public string CourseName 
        { 
            get
            {
                return _selectedCourseName;
            }
            set
            {
                if (_selectedCourseName != value)
                {
                    _selectedCourseName = value;
                    OnPropertyChanged("CourseName");
                }
            }
        }
        public bool IsSelectedCourseHomeroomOnly
        {
            get
            {
                return _isSelectedCourseHomeroomOnly;
            }
            set
            {
                if (_isSelectedCourseHomeroomOnly != value)
                {
                    _isSelectedCourseHomeroomOnly = value;
                    OnPropertyChanged("IsSelectedCourseHomeroomOnly");
                }
            }
        }
        public ObservableCollection<string> TeachersOfSelectedCourse
        {
            get
            {
                return _teachersOfSelectedCourse;
            }
            set
            {
                if (_teachersOfSelectedCourse != value)
                {
                    _teachersOfSelectedCourse = value;
                    OnPropertyChanged("TeachersOfSelectedCourse");
                }
            }
        }
        public ObservableCollection<LessonsOfCourse> LessonsOfSelectedCourse
        {
            get
            {
                return _lessonsOfSelectedCourse;
            }
            set
            {
                if (_lessonsOfSelectedCourse != value)
                {
                    _lessonsOfSelectedCourse = value;
                    OnPropertyChanged("LessonsOfSelectedCourse");
                }
            }
        }
        #endregion

        #region The constructor
        public CourseManagementViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
            : base (messageBoxService)
        {
            HasRequiredPermissions = connectedPerson.isSecretary || connectedPerson.isPrincipal;
            _refreshDataCommand = refreshDataCommand;

            if (HasRequiredPermissions)
            {
                _schoolData = new SchoolEntities();
            }
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;

            // Generate a list of existing courses
            CoursesTableData = new ObservableCollection<CourseData>(_schoolData.Courses.AsEnumerable().Select(course => DbCourseToCourseData(course)).ToList());
        }

        /// <summary>
        /// Choose to view the information of a selected course
        /// </summary>
        /// <param name="selectedCourse">The course's data</param>
        private void UseSelectedCourse(CourseData selectedCourse)
        {
            // clear previous selection
            CourseName = string.Empty;
            IsSelectedCourseHomeroomOnly = false;
            LessonsOfSelectedCourse = new ObservableCollection<LessonsOfCourse>();
            TeachersOfSelectedCourse = new ObservableCollection<string>();

            // change the properties to the selected course
            if (selectedCourse != null)
            {
                CourseName = selectedCourse.Name;
                IsSelectedCourseHomeroomOnly = selectedCourse.IsHomeroomTeacherOnly;

                // Generate a list of relevant teachers
                if (selectedCourse.CourseTeachers != null)
                {
                    TeachersOfSelectedCourse = new ObservableCollection<string>(selectedCourse.CourseTeachers);
                }

                // Generate a list of relevant lessons
                if (selectedCourse.CourseLessons != null)
                {
                    LessonsOfSelectedCourse = new ObservableCollection<LessonsOfCourse>(selectedCourse.CourseLessons);
                }
            }
        }
        /// <summary>
        /// Create a new course with given data
        /// </summary>
        private void CreateNewCourse()
        {
            // If the course's name is not unique, the assignment fails
            if (_schoolData.Courses.Where(courseData => courseData.courseName == CourseName).Any())
            {
                _messageBoxService.ShowMessage("This name is already taken. Please select a different name", "Course creation failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Create a new course
                Course newCourse = new Course() { courseName = CourseName, isHomeroomTeacherOnly = IsSelectedCourseHomeroomOnly };
                _schoolData.Courses.Add(newCourse);

                //save and report to the user
                _schoolData.SaveChanges();
                _messageBoxService.ShowMessage("The course " + CourseName + " Was created successfully", "Course creation complete", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                _refreshDataCommand.Execute(null);
            }
        }
        /// <summary>
        /// Update the currently selected course
        /// </summary>
        private void UpdateSelectedCourse()
        {
            // Fail if no course was selected
            if (SelectedCourse == null)
            {
                _messageBoxService.ShowMessage("Please select a course",
                                               "Course update failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // course to be edited
                Course selectedCourse = _schoolData.Courses.Find(SelectedCourse.ID);

                // If the course's name is not unique, the update fails
                if (_schoolData.Courses.Where(course => course.courseID != selectedCourse.courseID && course.courseName == CourseName).Any())
                {
                    _messageBoxService.ShowMessage("This name is already taken. Please select a different name", "Course update failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
                else
                {
                    // Update the course's data
                    selectedCourse.courseName = CourseName;
                    selectedCourse.isHomeroomTeacherOnly = IsSelectedCourseHomeroomOnly;

                    // Save and report to the user
                    _schoolData.SaveChanges();
                    _messageBoxService.ShowMessage("The Course " + CourseName + " was updated successfully", "Course updated", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                    _refreshDataCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Delete the currently selected course
        /// </summary>
        private void DeleteSelectedCourse()
        {
            // Check that a course was selected
            if (SelectedCourse == null)
            {
                _messageBoxService.ShowMessage("Please select a course",
                                               "Course deletion failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Course to be deleted
                Course selectedCourse = _schoolData.Courses.Find(SelectedCourse.ID);

                // Ask for confirmation when deleting
                bool confirmation = _messageBoxService.ShowMessage("Are you sure that you want to delete " + selectedCourse.courseName + "?",
                                                                    "Course deletion", MessageType.ACCEPT_CANCEL_MESSAGE, MessagePurpose.INFORMATION);
                if (confirmation == true)
                {
                    RemoveCourseFromTeachers(selectedCourse);

                    // delete all lessons from this course
                    var releventLessons = _schoolData.Lessons.Where(lesson => lesson.courseID == selectedCourse.courseID);
                    foreach (var lesson in releventLessons)
                    {
                        _schoolData.Lessons.Remove(lesson);
                    }

                    // Delete the course itself
                    _schoolData.Courses.Remove(selectedCourse);

                    // Save and report changes
                    _schoolData.SaveChanges();
                    _messageBoxService.ShowMessage("The Course " + selectedCourse.courseName + " was deleted successfully",
                            "Course deletion complete", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                    _refreshDataCommand.Execute(null);
                }
            }
        }
        /// <summary>
        /// Converts the Database "Course" object into a local "CourseData" object
        /// </summary>
        /// <param name="room">The Database's Course</param>
        /// <returns>CourseData version of said object</returns>
        private CourseData DbCourseToCourseData(Course dbCourse)
        {
            CourseData courseData = new CourseData();

            // Get the "easy" data first
            courseData.ID = dbCourse.courseID;
            courseData.Name = dbCourse.courseName;
            courseData.IsHomeroomTeacherOnly = dbCourse.isHomeroomTeacherOnly;

            // checks the class for associated lessons
            if (dbCourse.Lessons != null && dbCourse.Lessons.Count > 0)
            {
                courseData.CourseLessons = dbCourse.Lessons.Select(lesson => new LessonsOfCourse()
                {
                    LessonID = lesson.lessonID,
                    ClassID = lesson.classID,
                    ClassName = lesson.Class.className,
                    TeacherName = lesson.Teacher.Person.firstName + " " + lesson.Teacher.Person.lastName,
                    DayFirstLesson = Globals.ConvertDayNumberToName(lesson.firstLessonDay),
                    DaySecondLesson = Globals.ConvertDayNumberToName(lesson.secondLessonDay),
                    DayThirdLesson = Globals.ConvertDayNumberToName(lesson.thirdLessonDay),
                    DayFourthLesson = Globals.ConvertDayNumberToName(lesson.fourthLessonDay),
                    HourFirstLesson = Globals.ConvertHourNumberToName(lesson.firstLessonHour),
                    HourSecondLesson = Globals.ConvertHourNumberToName(lesson.secondLessonHour),
                    HourThirdLesson = Globals.ConvertHourNumberToName(lesson.thirdLessonHour),
                    HourFourthLesson = Globals.ConvertHourNumberToName(lesson.fourthLessonHour)
                }).ToList();
            }

            courseData.CourseTeachers = new List<string>();

            // Check for teachers non-disabled accounts of teachers that teach this course
            // Reminder: Teachers can teach up to four courses so we check them all
            foreach (Teacher teacher in _schoolData.Teachers.Where(teacher => !teacher.Person.User.isDisabled &&
                    (teacher.firstCourseID == courseData.ID || teacher.secondCourseID == courseData.ID ||
                    teacher.thirdCourseID == courseData.ID || teacher.fourthCourseID == courseData.ID)))
            {
                courseData.CourseTeachers.Add(teacher.Person.firstName + " " + teacher.Person.lastName);
            }

            return courseData;
        }

        /// <summary>
        /// Assistant method to remove a course from its teachers without empty spaces
        /// Reminder: teachers can have multiple courses (up to four courses)
        /// </summary>
        /// <param name="selectedCourse">The course to delete</param>
        private static void RemoveCourseFromTeachers(Course selectedCourse)
        {
            // Remove the course from all teachers that this is their first course
            foreach (Teacher teacher in selectedCourse.Teachers)
            {
                if (teacher.fourthCourseID != null)
                {
                    // Switch with the fourth course
                    teacher.firstCourseID = teacher.fourthCourseID;
                    teacher.fourthCourseID = null;
                }
                else if (teacher.thirdCourseID != null)
                {
                    // Switch with the third course
                    teacher.firstCourseID = teacher.thirdCourseID;
                    teacher.thirdCourseID = null;
                }
                else if (teacher.secondCourseID != null)
                {
                    // Switch with the second course
                    teacher.firstCourseID = teacher.secondCourseID;
                    teacher.secondCourseID = null;
                }
                else
                {
                    // this was the only course of the teacher. Delete it (and leave the teacher with no courses)
                    teacher.firstCourseID = null;
                }
            }
            // Remove the course from all teachers that this is their second course
            foreach (Teacher teacher in selectedCourse.Teachers)
            {
                if (teacher.fourthCourseID != null)
                {
                    // Switch with the fourth course
                    teacher.secondCourseID = teacher.fourthCourseID;
                    teacher.fourthCourseID = null;
                }
                else if (teacher.thirdCourseID != null)
                {
                    // Switch with the third course
                    teacher.secondCourseID = teacher.thirdCourseID;
                    teacher.thirdCourseID = null;
                }
                else
                {
                    // The teacher only had this and another course, so remove it directly
                    teacher.firstCourseID = null;
                }
            }
            // Remove the course from all teachers that this is their third course
            foreach (Teacher teacher in selectedCourse.Teachers)
            {
                if (teacher.fourthCourseID != null)
                {
                    // Switch with the fourth course
                    teacher.thirdCourseID = teacher.fourthCourseID;
                    teacher.fourthCourseID = null;
                }
                else
                {
                    teacher.thirdCourseID = null;
                }
            }
            // Remove the course from all teachers that this is their fourth
            foreach (Teacher teacher in selectedCourse.Teachers)
            {
                teacher.fourthCourseID = null;
            }
        }
        #endregion
    }
}
