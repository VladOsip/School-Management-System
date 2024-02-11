using EasySchool.Model;
using EasySchool.ViewModel.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySchool.ViewModel
{
    /// <summary>
    /// Displays the weekly schedule of a selected class
    /// </summary>
    public class WeeklyScheduleViewModel : BaseViewModel, IScreenViewModel
    {
        #region Structures

        /// <summary>
        /// A sub-structure that represents information for a given lesson
        /// </summary>
        public class LessonData
        {
            public string CourseName { get; set; }
            public string TeacherName { get; set; }
            public string RoomName { get; set; }
            public string ClassName { get; set; }

            /// <summary>
            /// A sub-structure that represents information for a given lesson using input
            /// </summary>
            public LessonData(Lesson lesson)
            {
                CourseName = lesson.Course.courseName;
                TeacherName = lesson.Teacher.Person.firstName + " " + lesson.Teacher.Person.lastName;
                ClassName = "Class " + lesson.Class.className;

                if (lesson.Room != null)
                {
                    RoomName = "Room " + lesson.Room.roomName;
                }
                else
                {
                    RoomName = String.Empty;
                }
            }
        }

        /// <summary>
        /// A sub-structure that represents information for a given schedule
        /// </summary>
        public class ScheduleData
        {
            public bool IsPersonSchedule { get; set; }
            public bool IsClassSchedule { get; set; }
            public bool IsRoomSchedule { get; set; }
            public int ID { get; set; }
            public string Name { get; set; }
            public LessonData[,] ActualSchedule { get; set; }
        }
        #endregion

        #region Fields
        private LessonData[,] _schedule;
        private ScheduleData _selectedSchedule;
        private List<ScheduleData> _availableSchedules;
        #endregion

        #region Properties

        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "Schedule"; } }

        // Business Logic Properties
        public LessonData[,] Schedule 
        { 
            get
            {
                return _schedule;
            }
            set
            {
                if (_schedule != value)
                {
                    _schedule = value;
                    OnPropertyChanged("Schedule");
                }
            }
        }
        public bool CanViewDifferentSchedules { get; private set; }
        public List<ScheduleData> AvailableSchedules
        {
            get
            {
                return _availableSchedules;
            }
            set
            {
                if (_availableSchedules != value)
                {
                    _availableSchedules = value;
                    OnPropertyChanged("AvailableSchedules");
                }
            }
        }
        public ScheduleData SelectedSchedule 
        { 
            get
            {
                return _selectedSchedule;
            }
            set
            {
                // look for a difference between input and _selectedSchedule
                if  (_selectedSchedule == null && value != null || _selectedSchedule.ID != value.ID ||
                    _selectedSchedule.IsClassSchedule != value.IsClassSchedule ||
                    _selectedSchedule.IsPersonSchedule != value.IsPersonSchedule ||
                    _selectedSchedule.IsRoomSchedule != value.IsRoomSchedule)
                {
                    _selectedSchedule = value;
                    Schedule = _selectedSchedule.ActualSchedule;
                    OnPropertyChanged("SelectedSchedule");
                }
            }
        }
        #endregion

        #region The constructor
        public WeeklyScheduleViewModel(Person connectedPerson)
        {
            HasRequiredPermissions = true;
            AvailableSchedules = new List<ScheduleData>();
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            // Reset data
            ConnectedPerson = connectedPerson;
            Schedule = new LessonData[Globals.DAY_NAMES.Length + 1, Globals.HOUR_NAMES.Length + 1];
            AvailableSchedules.Clear();
            
            // Find currently displayed class and fill the list of classes.
            if (ConnectedPerson.isStudent)
            {
                // A given student's schedule is also his class's schedule
                CanViewDifferentSchedules = false;
                Class studentClass = ConnectedPerson.Student.Class;

                // check if student it assigned to class
                if (studentClass != null)
                {
                    ScheduleData schedule = CreateClassSchedule(studentClass);

                    // Add the schedule to list of schedules
                    AvailableSchedules.Add(schedule);
                }
            }
            else if (ConnectedPerson.isParent)
            {
                // A parent can see his children's schedules
                CanViewDifferentSchedules = true;
                foreach (Student student in ConnectedPerson.ChildrenStudents)
                {
                    if (!student.Person.User.isDisabled && student.classID != null)
                    {
                        ScheduleData schedule = CreateClassSchedule(student.Class);

                        // Change schedule to use the child's name
                        schedule.Name = student.Person.firstName + " " + student.Person.lastName;

                        // Add the schedule to list of schedules
                        AvailableSchedules.Add(schedule);
                    }
                }
            }
            else if (ConnectedPerson.isTeacher)
            {
                // A teacher can just see his own schedule, as well as his classroom's schedule (if he/she has one)
                CanViewDifferentSchedules = true;

                // Add teacher's own schedule
                ScheduleData teacherOwnSchedule = CreateTeacherSchedule(ConnectedPerson.Teacher);
                AvailableSchedules.Add(teacherOwnSchedule);

                // Add an homeroom teacher's class schedule
                if (ConnectedPerson.Teacher.classID != null)
                {
                    ScheduleData homeroomTeacherClassSchedule = CreateClassSchedule(ConnectedPerson.Teacher.Class);
                    AvailableSchedules.Add(homeroomTeacherClassSchedule);
                }
            }
            else if (ConnectedPerson.isPrincipal || ConnectedPerson.isSecretary)
            {
                // Managers can watch all schedules
                CanViewDifferentSchedules = true;

                // all class schedules
                SchoolEntities schoolData = new SchoolEntities();
                foreach (Class schoolClass in schoolData.Classes)
                {
                    ScheduleData schedule = CreateClassSchedule(schoolClass);
                    AvailableSchedules.Add(schedule);
                }

                // all teacher schedules
                foreach (Teacher teacher in schoolData.Teachers)
                {
                    ScheduleData schedule = CreateTeacherSchedule(teacher);
                    AvailableSchedules.Add(schedule);
                }
                
                // all room schedules
                foreach (Room room in schoolData.Rooms)
                {
                    ScheduleData schedule = CreateRoomSchedule(room);
                    AvailableSchedules.Add(schedule);
                }
            }

            SelectedSchedule = AvailableSchedules.First();
        }

        /// <summary>
        /// Assistant method that generates a weekly schedule for a specific class
        /// </summary>
        /// <param name="classData">Class to build from</param>
        /// <returns>schedule information</returns>
        private ScheduleData CreateClassSchedule(Class classData)
        {
            ScheduleData studentSchedule = new ScheduleData()
            {
                ID = classData.classID,
                Name = "Class " + classData.className,
                IsClassSchedule = true,
                IsPersonSchedule = false,
                IsRoomSchedule = false,
                ActualSchedule = new LessonData[Globals.DAY_NAMES.Length + 1, Globals.HOUR_NAMES.Length + 1]
            };
            // Add class lessons to the schedule
            classData.Lessons.ToList().ForEach(lesson => AddLessonToSchedule(lesson, ref studentSchedule));
            return studentSchedule;
        }

        /// <summary>
        /// Assistant method that generates a weekly schedule for a specific room
        /// </summary>
        /// <param name="teacher">The room to build from</param>
        /// <returns>schedule information</returns>
        private ScheduleData CreateRoomSchedule(Room room)
        {
            ScheduleData roomSchedule = new ScheduleData()
            {
                ID = room.roomID,
                Name = "room " + room.roomName,
                IsClassSchedule = false,
                IsPersonSchedule = false,
                IsRoomSchedule = true,
                ActualSchedule = new LessonData[Globals.DAY_NAMES.Length + 1, Globals.HOUR_NAMES.Length + 1]
            };
            // Add the room's lessons to the schedule
            room.Lessons.ToList().ForEach(lesson => AddLessonToSchedule(lesson, ref roomSchedule));
            return roomSchedule;
        }

        /// <summary>
        /// Assistant method that generates a weekly schedule for a specific teacher
        /// </summary>
        /// <param name="teacher">The teacher to build from</param>
        /// <returns>schedule information</returns>
        private ScheduleData CreateTeacherSchedule(Teacher teacher)
        {
            ScheduleData teacherSchedule = new ScheduleData()
            {
                ID = teacher.teacherID,
                Name = teacher.Person.firstName + " " + teacher.Person.lastName,
                IsClassSchedule = false,
                IsPersonSchedule = true,
                IsRoomSchedule = false,
                ActualSchedule = new LessonData[Globals.DAY_NAMES.Length + 1, Globals.HOUR_NAMES.Length + 1]
            };
            // Add class lessons to the schedule
            teacher.Lessons.ToList().ForEach(lesson => AddLessonToSchedule(lesson, ref teacherSchedule));
            return teacherSchedule;
        }

        /// <summary>
        /// Assistant method for adding a lesson to the schedule
        /// </summary>
        /// <param name="lesson"></param>
        /// <param name="schedule"></param>
        private void AddLessonToSchedule(Lesson lesson, ref ScheduleData schedule)
        {
            // Only the first lesson is assured
            // We reduce by 1 due to SQL starting from 1 and the array for the schedule starting from 0
            if (isValidLesson(lesson.firstLessonHour - 1, lesson.firstLessonDay - 1))
            {
                schedule.ActualSchedule[lesson.firstLessonDay - 1, lesson.firstLessonHour - 1] = new LessonData(lesson);
            }
            if(isValidLesson(lesson.secondLessonHour - 1, lesson.secondLessonDay - 1))
            {
                schedule.ActualSchedule[lesson.secondLessonDay.Value - 1, lesson.secondLessonHour.Value - 1] = new LessonData(lesson);
            }
            if (isValidLesson(lesson.thirdLessonHour - 1, lesson.thirdLessonDay - 1))
            {
                schedule.ActualSchedule[lesson.thirdLessonDay.Value - 1, lesson.thirdLessonHour.Value - 1] = new LessonData(lesson);
            }
            if (isValidLesson(lesson.fourthLessonHour - 1, lesson.fourthLessonDay - 1))
            {
                schedule.ActualSchedule[lesson.fourthLessonDay.Value - 1, lesson.fourthLessonHour.Value - 1] = new LessonData(lesson);
            }
        }

        /// <summary>
        /// Assistant method for validating lesson time (day/hour)
        /// </summary>
        /// <param name="lessonDay">Lesson day (expected 0-Globals.DAY_NAMES.length)</param>
        /// <param name="lessonHour">Lesson hour (expected 0-Globals.HOUR_NAMES.length)</param>
        /// <returns></returns>
        private bool isValidLesson(Nullable<int> lessonHour, Nullable<int> lessonDay)
        {
            if ((lessonHour != null && lessonDay != null) &&
                (lessonDay >= 0 && lessonDay < Globals.DAY_NAMES.Length) &&
                (lessonHour >= 0 && lessonHour < Globals.HOUR_NAMES.Length))
            {
                return true;
            }

            return false;
        }
        #endregion
    }
}
