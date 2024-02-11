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
    /// Manages the school's lessons
    /// </summary>
    public class LessonManagementViewModel : BaseViewModel, IScreenViewModel
    {
        #region Structures
        /// <summary>
        /// A sub-structure that represents information for a given lesson
        /// </summary>
        public class LessonData
        {
            public int ID { get; set; }
            public string ClassName { get; set; }
            public string CourseName { get; set; }
            public string TeacherName { get; set; }
            public string RoomName { get; set; }
            public int ClassID { get; set; }
            public int CourseID { get; set; }
            public int TeacherID { get; set; }
            public Nullable<int> RoomID { get; set; }

            public LessonTimes LessonMeetingTimes { get; set; }
        }

        /// <summary>
        /// A sub-structure that represents information for lesson times
        /// </summary>
        public class LessonTimes
        {
            public Nullable<int> DayFirst { get; set; }
            public Nullable<int> HourFirst { get; set; }
            public Nullable<int> DaySecond { get; set; }
            public Nullable<int> HourSecond { get; set; }
            public Nullable<int> DayThird { get; set; }
            public Nullable<int> HourThird { get; set; }
            public Nullable<int> DayFourth { get; set; }
            public Nullable<int> HourFourth { get; set; }
        }

        /// <summary>
        /// Assistant enum with the possible ways to search for courses (e.g searching for all the lessons of specific teacher or class)
        /// </summary>
        private enum SearchCategory
        {
            Classes,
            Courses,
            Teachers
        }
        #endregion

        #region Fields
        private ICommand _refreshDataCommand;
        private ICommand _createLessonCommand;
        private ICommand _updateLessonCommand;
        private ICommand _deleteLessonCommand;

        private SchoolEntities _schoolData;

        private bool _searchingByClass;
        private bool _searchingByCourse;
        private bool _searchingByTeacher;
        private int _selectedSearchChoiceID;
        
        private LessonData _selectedLesson;

        private int _selectedClassID;
        private int _selectedCourseID;
        private int _selectedTeacherID;
        private int _selectedRoomID;

        private ObservableDictionary<int, string> _availableSearchChoices;
        public ObservableCollection<LessonData> _lessonsTableData;

        private ObservableDictionary<int, string> _availableClasses;
        private ObservableDictionary<int, string> _availableCourses;
        private ObservableDictionary<int, string> _availableTeachers;
        private ObservableDictionary<int, string> _availableRooms;
        private ObservableDictionary<int, string> _teacherAvailableCourses;

        private ObservableDictionary<int, string> _availableDays;
        private ObservableDictionary<int, string> _availableHours;

        private int _lessonFirstDay;
        private int _lessonSecondDay;
        private int _lessonThirdDay;
        private int _lessonFourthDay;
        private int _lessonFirstHour;
        private int _lessonSecondHour;
        private int _lessonThirdHour;
        private int _lessonFourthHour;

        private const int NOT_ASSIGNED = -1;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "Manage Lessons"; } }

        // Commands

        /// <summary>
        /// Create a new lesson with the current data
        /// </summary>
        public ICommand CreateNewLessonCommand
        {
            get
            {
                if (_createLessonCommand == null)
                {
                    _createLessonCommand = new RelayCommand(p => CreateNewLesson());
                }
                return _createLessonCommand;
            }
        }

        /// <summary>
        /// Update the currently selected lesson
        /// </summary>
        public ICommand UpdateLessonCommand
        {
            get
            {
                if (_updateLessonCommand == null)
                {
                    _updateLessonCommand = new RelayCommand(p => UpdateSelectedLesson());
                }
                return _updateLessonCommand;
            }
        }

        /// <summary>
        /// Delete the currently selected lesson
        /// </summary>
        public ICommand DeleteLessonCommand
        {
            get
            {
                if (_deleteLessonCommand == null)
                {
                    _deleteLessonCommand = new RelayCommand(p => DeleteSelectedLesson());
                }
                return _deleteLessonCommand;
            }
        }

        // Business Logic Properties
        public ObservableCollection<LessonData> LessonsTableData 
        { 
            get
            {
                return _lessonsTableData;
            }
            set
            {
                if (_lessonsTableData != value)
                {
                    _lessonsTableData = value;
                    OnPropertyChanged("LessonsTableData");
                }
            }
        }
        public LessonData SelectedLesson 
        { 
            get
            {
                return _selectedLesson;
            }
            set
            {
                if (_selectedLesson != value)
                {
                    _selectedLesson = value;
                    UseSelectedLesson(_selectedLesson);
                    OnPropertyChanged("SelectedLesson");
                }
            }
        }

        public bool SearchingByClass
        { 
            get
            {
                return _searchingByClass;
            }
            set
            {
                if (_searchingByClass != value)
                {
                    _searchingByClass = value;
                    OnPropertyChanged("SearchingByClass");

                    if (value == true)
                    {
                        // Manually turn off the other search options before using this category, because Epical switches the new choice on before changing the other off,
                        // causing two options to be on at the same time, and in this case this causes an issue with the selected event
                        SearchingByCourse = false;
                        SearchingByTeacher = false;
                        UseSelectedSearchCategory(SearchCategory.Classes);
                    }
                }
            }
        }
        public bool SearchingByCourse
        {
            get
            {
                return _searchingByCourse;
            }
            set
            {
                if (_searchingByCourse != value)
                {
                    _searchingByCourse = value;
                    OnPropertyChanged("SearchingByCourse");

                    if (value == true)
                    {
                        // Due to how Epical switches the new choice on before changing the other off, causing two options to be on and messing with the selected event
                        // We need to turn the other search options off manually
                        SearchingByClass = false;
                        SearchingByTeacher = false;
                        UseSelectedSearchCategory(SearchCategory.Courses);
                    }
                }
            }
        }
        public bool SearchingByTeacher
        {
            get
            {
                return _searchingByTeacher;
            }
            set
            {
                if (_searchingByTeacher != value)
                {
                    _searchingByTeacher = value;
                    OnPropertyChanged("SearchingByTeacher");

                    if (value == true)
                    {
                        // Due to how Epical switches the new choice on before changing the other off, causing two options to be on and messing with the selected event
                        // We need to turn the other search options off manually
                        SearchingByClass = false;
                        SearchingByCourse = false;
                        UseSelectedSearchCategory(SearchCategory.Teachers);
                    }
                }
            }
        }

        public ObservableDictionary<int, string> AvailableClasses
        {
            get
            {
                return _availableClasses;
            }
            set
            {
                if (_availableClasses != value)
                {
                    _availableClasses = value;
                    OnPropertyChanged("AvailableClasses");
                }
            }
        }
        public int SelectedClass
        {
            get
            {
                return _selectedClassID;
            }
            set
            {
                if (_selectedClassID != value)
                {
                    _selectedClassID = value;
                    OnPropertyChanged("SelectedClass");

                    // Update list of possible courses for the selected teacher&class combination
                    ShowSelectedTeacherCourses();
                }
            }
        }

        public ObservableDictionary<int, string> AvailableCourses
        {
            get
            {
                return _availableCourses;
            }
            set
            {
                if (_availableCourses != value)
                {
                    _availableCourses = value;
                    OnPropertyChanged("AvailableCourses");
                }
            }
        }
        public ObservableDictionary<int, string> TeacherAvailableCourses
        {
            get
            {
                return _teacherAvailableCourses;
            }
            set
            {
                if (_teacherAvailableCourses != value)
                {
                    _teacherAvailableCourses = value;
                    OnPropertyChanged("TeacherAvailableCourses");
                }
            }
        }
        public int SelectedCourse
        {
            get
            {
                return _selectedCourseID;
            }
            set
            {
                if (_selectedCourseID != value)
                {
                    _selectedCourseID = value;
                    OnPropertyChanged("SelectedCourse");
                }
            }
        }

        public ObservableDictionary<int, string> AvailableTeachers
        {
            get
            {
                return _availableTeachers;
            }
            set
            {
                if (_availableTeachers != value)
                {
                    _availableTeachers = value;
                    OnPropertyChanged("AvailableTeachers");
                }
            }
        }
        public int SelectedTeacher
        {
            get
            {
                return _selectedTeacherID;
            }
            set
            {
                if (_selectedTeacherID != value)
                {
                    _selectedTeacherID = value;
                    OnPropertyChanged("SelectedTeacher");

                    // Update list of possible courses for a given teacher/course combination
                    ShowSelectedTeacherCourses();
                }
            }
        }

        public ObservableDictionary<int, string> AvailableSearchChoices
        {
            get
            {
                return _availableSearchChoices;
            }
            set
            {
                if (_availableSearchChoices != value)
                {
                    _availableSearchChoices = value;
                    OnPropertyChanged("AvailableSearchChoices");
                }
            }
        }
        public int SelectedSearchChoice
        {
            get
            {
                return _selectedSearchChoiceID;
            }
            set
            {
                // ID value might not change upon category switch (e.g choosing first item in different categories)
                // Update everytime the 'set' is called
                _selectedSearchChoiceID = value;
                UseSelectedSearchChoice();
                OnPropertyChanged("SelectedSearchChoice");
            }
        }

        public ObservableDictionary<int, string> AvailableRooms
        {
            get
            {
                return _availableRooms;
            }
            set
            {
                if (_availableRooms != value)
                {
                    _availableRooms = value;
                    OnPropertyChanged("AvailableRooms");
                }
            }
        }
        public int SelectedRoom
        {
            get
            {
                return _selectedRoomID;
            }
            set
            {
                if (_selectedRoomID != value)
                {
                    _selectedRoomID = value;
                    OnPropertyChanged("SelectedRoom");
                }
            }
        }

        public ObservableDictionary<int, string> AvailableDays
        {
            get
            {
                return _availableDays;
            }
            set
            {
                if (_availableDays != value)
                {
                    _availableDays = value;
                    OnPropertyChanged("AvailableDays");
                }
            }
        }


        public ObservableDictionary<int, string> AvailableHours
        {
            get
            {
                return _availableHours;
            }
            set
            {
                if (_availableHours != value)
                {
                    _availableHours = value;
                    OnPropertyChanged("AvailableHours");
                }
            }
        }

        public int LessonFirstDay
        { 
            get
            {
                return _lessonFirstDay;
            }
            set
            {
                if (_lessonFirstDay != value)
                {
                    _lessonFirstDay = value;
                    OnPropertyChanged("LessonFirstDay");
                }
            }
        }
        public int LessonSecondDay
        {
            get
            {
                return _lessonSecondDay;
            }
            set
            {
                if (_lessonSecondDay != value)
                {
                    _lessonSecondDay = value;
                    OnPropertyChanged("LessonSecondDay");
                }
            }
        }
        public int LessonThirdDay
        {
            get
            {
                return _lessonThirdDay;
            }
            set
            {
                if (_lessonThirdDay != value)
                {
                    _lessonThirdDay = value;
                    OnPropertyChanged("LessonThirdDay");
                }
            }
        }
        public int LessonFourthDay
        {
            get
            {
                return _lessonFourthDay;
            }
            set
            {
                if (_lessonFourthDay != value)
                {
                    _lessonFourthDay = value;
                    OnPropertyChanged("LessonFourthDay");
                }
            }
        }

        public int LessonFirstHour
        {
            get
            {
                return _lessonFirstHour;
            }
            set
            {
                if (_lessonFirstHour != value)
                {
                    _lessonFirstHour = value;
                    OnPropertyChanged("LessonFirstHour");
                }
            }
        }
        public int LessonSecondHour
        {
            get
            {
                return _lessonSecondHour;
            }
            set
            {
                if (_lessonSecondHour != value)
                {
                    _lessonSecondHour = value;
                    OnPropertyChanged("LessonSecondHour");
                }
            }
        }
        public int LessonThirdHour
        {
            get
            {
                return _lessonThirdHour;
            }
            set
            {
                if (_lessonThirdHour != value)
                {
                    _lessonThirdHour = value;
                    OnPropertyChanged("LessonThirdHour");
                }
            }
        }
        public int LessonFourthHour
        {
            get
            {
                return _lessonFourthHour;
            }
            set
            {
                if (_lessonFourthHour != value)
                {
                    _lessonFourthHour = value;
                    OnPropertyChanged("LessonFourthHour");
                }
            }
        }

        #endregion

        #region The constructor
        public LessonManagementViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
            : base(messageBoxService)
        {
            // only management has access
            HasRequiredPermissions = connectedPerson.isSecretary || connectedPerson.isPrincipal;
            _refreshDataCommand = refreshDataCommand;

            if (HasRequiredPermissions)
            {
                _schoolData = new SchoolEntities();
                AvailableSearchChoices = new ObservableDictionary<int, string>();
                LessonsTableData = new ObservableCollection<LessonData>();
                AvailableClasses = new ObservableDictionary<int, string>();
                AvailableCourses = new ObservableDictionary<int, string>();
                AvailableTeachers = new ObservableDictionary<int, string>();
                AvailableRooms = new ObservableDictionary<int, string>();
                TeacherAvailableCourses = new ObservableDictionary<int, string>();

                // complete days and hours list
                // allow 'no meeting' choice that is implemented with a NOT_ASSIGNED value)
                AvailableDays = new ObservableDictionary<int, string>();
                AvailableDays.Add(NOT_ASSIGNED, "None");
                for (int i=0; i < Globals.DAY_NAMES.Length; i++)
                {
                    // Count days as 1 to N rather than 0 to N-1
                    AvailableDays.Add(i + 1, Globals.DAY_NAMES[i]);
                }
                AvailableHours = new ObservableDictionary<int, string>();
                AvailableHours.Add(NOT_ASSIGNED, "None");
                for (int i = 0; i < Globals.HOUR_NAMES.Length; i++)
                {
                    // Count hours as 1 to N rather than 0 to N-1
                    AvailableHours.Add(i + 1, Globals.HOUR_NAMES[i]);
                }
            }
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;
            ResetData();

            // Generate lists of available classes, courses, teachers
            _schoolData.Classes.ToList().ForEach(schoolClass => AvailableClasses.Add(schoolClass.classID, schoolClass.className));
            _schoolData.Courses.ToList().ForEach(course => AvailableCourses.Add(course.courseID, course.courseName));
            _schoolData.Teachers.Where(teacher => !teacher.Person.User.isDisabled).ToList()
                .ForEach(teacher => AvailableTeachers.Add(teacher.teacherID, teacher.Person.firstName + " " + teacher.Person.lastName));

            // Initialize rooms list
            // Note: rooms are optional so a NOT_ASSIGNED option exists
            AvailableRooms.Add(NOT_ASSIGNED, "None");
            _schoolData.Rooms.ToList().ForEach(room => AvailableRooms.Add(room.roomID, room.roomName));

            SearchingByClass = true;

            // Due to an unknown issue, the property changed notification needs to be raised manually
            OnPropertyChanged("SelectedClass");
            OnPropertyChanged("SelectedCourse");
            OnPropertyChanged("SelectedTeacher");
            OnPropertyChanged("SelectedRoom");
        }

        /// <summary>
        /// Converts the Database lesson format into a local LessonData format
        /// </summary>
        /// <param name="lesson">Database lesson</param>
        /// <returns>Local LessonData version/returns>
        private LessonData ModelLessonToLessonData(Lesson lesson)
        {
            LessonData lessonData = new LessonData();
            
            // Get IDs
            lessonData.ID = lesson.lessonID;
            lessonData.ClassID = lesson.classID;
            lessonData.CourseID = lesson.courseID;
            lessonData.TeacherID = lesson.teacherID;
            lessonData.RoomID = lesson.roomID;

            // Get names
            lessonData.ClassName = lesson.Class.className;
            lessonData.CourseName = lesson.Course.courseName;
            lessonData.TeacherName = lesson.Teacher.Person.firstName + " " + lesson.Teacher.Person.lastName;

            if (lessonData.RoomID != null)
            {
                lessonData.RoomName = lesson.Room.roomName;
            }

            // Get lesson meeting times
            lessonData.LessonMeetingTimes = new LessonTimes()
            {
                DayFirst = lesson.firstLessonDay,
                DaySecond = lesson.secondLessonDay,
                DayThird = lesson.thirdLessonDay,
                DayFourth = lesson.fourthLessonDay,
                HourFirst = lesson.firstLessonHour,
                HourSecond = lesson.secondLessonHour,
                HourThird = lesson.thirdLessonHour,
                HourFourth = lesson.fourthLessonHour
            };

            return lessonData;
        }

        /// <summary>
        /// Create a new lesson with current data
        /// </summary>
        private void CreateNewLesson()
        {
            if (_schoolData.Classes.Find(SelectedClass) == null)
            {
                _messageBoxService.ShowMessage("Please choose a proper class", "Lesson creation failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else if (_schoolData.Courses.Find(SelectedCourse) == null)
            {
                _messageBoxService.ShowMessage("Please choose a proper course", "Lesson creation failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else if (_schoolData.Teachers.Find(SelectedTeacher) == null)
            {
                _messageBoxService.ShowMessage("Please choose a proper teacher", "Lesson creation failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            // There is at least one lesson
            else if (LessonFirstDay == NOT_ASSIGNED || LessonFirstHour == NOT_ASSIGNED)
            {
                _messageBoxService.ShowMessage("Please input a proper lesson (day and hour)",
                                               "Lesson creation failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Create new Lesson
                Lesson newLesson = new Lesson();
                newLesson.classID = SelectedClass;
                newLesson.courseID = SelectedCourse;
                newLesson.teacherID = SelectedTeacher;
                newLesson.roomID = (SelectedRoom != NOT_ASSIGNED) ? (int?)SelectedRoom : null;

                // Get lesson meeting times.
                // Note: At least one meeting is required, so firstLessonDay and firstLessonHour are not nullable
                newLesson.firstLessonDay = Convert.ToByte(LessonFirstDay);
                newLesson.firstLessonHour = Convert.ToByte(LessonFirstHour);
                newLesson.secondLessonDay = (LessonSecondDay != NOT_ASSIGNED) ? Convert.ToByte(LessonSecondDay) : (byte?)null;
                newLesson.secondLessonHour = (LessonSecondHour != NOT_ASSIGNED) ? Convert.ToByte(LessonSecondHour) : (byte?)null;
                newLesson.thirdLessonDay = (LessonThirdDay != NOT_ASSIGNED) ? Convert.ToByte(LessonThirdDay) : (byte?)null;
                newLesson.thirdLessonHour = (LessonThirdHour != NOT_ASSIGNED) ? Convert.ToByte(LessonThirdHour) : (byte?)null;
                newLesson.fourthLessonDay = (LessonFourthDay != NOT_ASSIGNED) ? Convert.ToByte(LessonFourthDay) : (byte?)null;
                newLesson.fourthLessonHour = (LessonFourthHour != NOT_ASSIGNED) ? Convert.ToByte(LessonFourthHour) : (byte?)null;

                // Save and report action
                _schoolData.Lessons.Add(newLesson);
                _schoolData.SaveChanges();
                _messageBoxService.ShowMessage("The lesson has been created successfully", "Lesson created", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                _refreshDataCommand.Execute(null);
            }
        }

        /// <summary>
        /// View a specific lesson's information
        /// </summary>
        /// <param name="selectedLesson">The lesson's data</param>
        private void UseSelectedLesson(LessonData selectedLesson)
        {
            // Use selected lesson to update properties
            if (selectedLesson != null)
            {
                SelectedClass = selectedLesson.ClassID;
                SelectedCourse = selectedLesson.CourseID;
                SelectedTeacher = selectedLesson.TeacherID;
                SelectedRoom = (selectedLesson.RoomID != null) ? selectedLesson.RoomID.Value : NOT_ASSIGNED;

                LessonFirstDay = (selectedLesson.LessonMeetingTimes.DayFirst != null) ? selectedLesson.LessonMeetingTimes.DayFirst.Value : NOT_ASSIGNED;
                LessonSecondDay = (selectedLesson.LessonMeetingTimes.DaySecond != null) ? selectedLesson.LessonMeetingTimes.DaySecond.Value : NOT_ASSIGNED;
                LessonThirdDay = (selectedLesson.LessonMeetingTimes.DayThird != null) ? selectedLesson.LessonMeetingTimes.DayThird.Value : NOT_ASSIGNED;
                LessonFourthDay = (selectedLesson.LessonMeetingTimes.DayFourth != null) ? selectedLesson.LessonMeetingTimes.DayFourth.Value : NOT_ASSIGNED;

                LessonFirstHour = (selectedLesson.LessonMeetingTimes.HourFirst != null) ? selectedLesson.LessonMeetingTimes.HourFirst.Value : NOT_ASSIGNED;
                LessonSecondHour = (selectedLesson.LessonMeetingTimes.HourSecond != null) ? selectedLesson.LessonMeetingTimes.HourSecond.Value : NOT_ASSIGNED;
                LessonThirdHour = (selectedLesson.LessonMeetingTimes.HourThird != null) ? selectedLesson.LessonMeetingTimes.HourThird.Value : NOT_ASSIGNED;
                LessonFourthHour = (selectedLesson.LessonMeetingTimes.HourFourth != null) ? selectedLesson.LessonMeetingTimes.HourFourth.Value : NOT_ASSIGNED;
            }
            else
            {
                // Assuming no selected lesson, reset the properties
                SelectedRoom = NOT_ASSIGNED;

                LessonFirstDay = NOT_ASSIGNED;
                LessonSecondDay = NOT_ASSIGNED;
                LessonThirdDay = NOT_ASSIGNED;
                LessonFourthDay = NOT_ASSIGNED;

                LessonFirstDay = NOT_ASSIGNED;
                LessonSecondDay = NOT_ASSIGNED;
                LessonThirdDay = NOT_ASSIGNED;
                LessonFourthDay = NOT_ASSIGNED;
            }
        }

        /// <summary>
        /// Update selected lesson
        /// </summary>
        private void UpdateSelectedLesson()
        {
            if (SelectedLesson == null)
            {
                _messageBoxService.ShowMessage("Please select a lesson",
                                               "lesson update failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else if (LessonFirstDay == NOT_ASSIGNED || LessonFirstHour == NOT_ASSIGNED)
            {
                _messageBoxService.ShowMessage("Please input a proper lesson (day and hour)",
                                               "Lesson update failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // select lesson to be edited
                Lesson selectedLesson = _schoolData.Lessons.Find(SelectedLesson.ID);

                // Update room data
                selectedLesson.classID = SelectedClass;
                selectedLesson.courseID = SelectedCourse;
                selectedLesson.teacherID = SelectedTeacher;
                selectedLesson.roomID = (SelectedRoom != NOT_ASSIGNED) ? (int?)SelectedRoom : null;

                // Get lesson meeting times.
                // Note: At least one meeting is required, so firstLessonDay and firstLessonHour are not nullable                selectedLesson.firstLessonDay = Convert.ToByte(LessonFirstDay);
                selectedLesson.firstLessonHour = Convert.ToByte(LessonFirstHour);
                selectedLesson.secondLessonDay = (LessonSecondDay != NOT_ASSIGNED) ? Convert.ToByte(LessonSecondDay) : (byte?)null;
                selectedLesson.secondLessonHour = (LessonSecondHour != NOT_ASSIGNED) ? Convert.ToByte(LessonSecondHour) : (byte?)null;
                selectedLesson.thirdLessonDay = (LessonThirdDay != NOT_ASSIGNED) ? Convert.ToByte(LessonThirdDay) : (byte?)null;
                selectedLesson.thirdLessonHour = (LessonThirdHour != NOT_ASSIGNED) ? Convert.ToByte(LessonThirdHour) : (byte?)null;
                selectedLesson.fourthLessonDay = (LessonFourthDay != NOT_ASSIGNED) ? Convert.ToByte(LessonFourthDay) : (byte?)null;
                selectedLesson.fourthLessonHour = (LessonFourthHour != NOT_ASSIGNED) ? Convert.ToByte(LessonFourthHour) : (byte?)null;

                // Save and report action
                _schoolData.SaveChanges();
                _messageBoxService.ShowMessage("Lesson updated successfully", "Lesson updated", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                _refreshDataCommand.Execute(null);
            }
        }

        /// <summary>
        /// Delete selected lesson
        /// </summary>
        private void DeleteSelectedLesson()
        {
            if (SelectedLesson == null)
            {
                _messageBoxService.ShowMessage("Please select a room",
                                               "Deletion failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Lesson to be deleted
                Lesson selectedLesson = _schoolData.Lessons.Find(SelectedLesson.ID);

                // Ask for confirmation, as is the case with all deletions
                bool confirmation = _messageBoxService.ShowMessage("Are you sure that you wish to delete this lesson?",
                                                                    "lesson deletion", MessageType.ACCEPT_CANCEL_MESSAGE, MessagePurpose.INFORMATION);
                if (confirmation == true)
                {
                    // Remove lesson
                    _schoolData.Lessons.Remove(selectedLesson);

                    // Save and report changes
                    _schoolData.SaveChanges();
                    _messageBoxService.ShowMessage("Lesson deletion successfully",
                            "Lesson deleted", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                    _refreshDataCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Updates "AvailableSearchChoices" list with search options
        /// </summary>
        /// <param name="category">Category to search by</param>
        private void UseSelectedSearchCategory(SearchCategory category)
        {
            // Find the right search category
            if (category == SearchCategory.Classes)
            {
                AvailableSearchChoices = AvailableClasses;
            }
            else if (category == SearchCategory.Courses)
            {
                AvailableSearchChoices = AvailableCourses;
            }
            else if (category == SearchCategory.Teachers)
            {
                AvailableSearchChoices = AvailableTeachers;
            }
            else
            {
                throw new ArgumentException("Couldn't use search category - unknown category type");
            }

            SelectedSearchChoice = AvailableSearchChoices.First().Key;
        }

        /// <summary>
        /// Updates "LessonsTableData" with the lessons from "SelectedSearchChoice" 
        /// </summary>
        private void UseSelectedSearchChoice()
        {
            if (SelectedSearchChoice != NOT_ASSIGNED)
            {
                // Clean lessons table before adding items
                LessonsTableData.Clear();

                // Find correct search category and fill "LessonsTableData" with relevent lessons
                if (SearchingByClass)
                {
                    _schoolData.Classes.Find(SelectedSearchChoice).Lessons.ToList().
                        ForEach(lesson => LessonsTableData.Add(ModelLessonToLessonData(lesson)));
                }
                else if (SearchingByCourse)
                {
                    _schoolData.Courses.Find(SelectedSearchChoice).Lessons.ToList().
                        ForEach(lesson => LessonsTableData.Add(ModelLessonToLessonData(lesson)));
                }
                else if (SearchingByTeacher)
                {
                    _schoolData.Teachers.Find(SelectedSearchChoice).Lessons.ToList().
                        ForEach(lesson => LessonsTableData.Add(ModelLessonToLessonData(lesson)));
                }
                else
                {
                    // Unknown search category. Show nothing.
                }
            }
        }

        /// <summary>
        /// Allow choosing only the courses that the selected teacher can teach
        /// </summary>
        private void ShowSelectedTeacherCourses()
        {
            // Reset the teacher's available courses collection
            TeacherAvailableCourses.Clear();

            if (SelectedTeacher != NOT_ASSIGNED)
            {
                // Get the selected teacher's information
                Teacher selectedTeacher = _schoolData.Teachers.Find(SelectedTeacher);
                bool isHomeroomTeacherClass = selectedTeacher.classID == SelectedClass;

                // Update the teacher courses collection with this teaher's courses
                TeacherAvailableCourses =
                    new ObservableDictionary<int, string>(TeacherCoursesHandler.GetTeacherCoursesNames(selectedTeacher, isHomeroomTeacherClass));
            }
            
            // Update the selected course per the new courses collection
            if (TeacherAvailableCourses.Count() > 0)
            {
                SelectedCourse = TeacherAvailableCourses.First().Key;
            }
            else
            {
                SelectedCourse = NOT_ASSIGNED;
            }

            // For some reason, after re-initializing a collection, a related selection in it is not updated properly in the view unless called explicitly
            OnPropertyChanged("SelectedCourse");
        }

        /// <summary>
        /// Clears all ViewModel properties
        /// </summary>
        private void ResetData()
        {
            AvailableSearchChoices.Clear();
            LessonsTableData.Clear();
            AvailableClasses.Clear();
            AvailableCourses.Clear();
            AvailableTeachers.Clear();
            AvailableRooms.Clear();
            SelectedLesson = null;
            SelectedSearchChoice = NOT_ASSIGNED;
            SelectedTeacher = NOT_ASSIGNED;
            SelectedClass = NOT_ASSIGNED;
            LessonFirstDay = NOT_ASSIGNED;
            LessonSecondDay = NOT_ASSIGNED;
            LessonThirdDay = NOT_ASSIGNED;
            LessonFourthDay = NOT_ASSIGNED;
            LessonFirstHour = NOT_ASSIGNED;
            LessonSecondHour = NOT_ASSIGNED;
            LessonThirdHour = NOT_ASSIGNED;
            LessonFourthHour = NOT_ASSIGNED;
        }
        #endregion
    }
}
