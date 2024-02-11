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
    /// Manages the school's classes
    /// </summary>
    public class ClassManagementViewModel : BaseViewModel, IScreenViewModel
    {
        #region Structs
        /// <summary>
        /// A sub-structure that represents information for a given class
        /// </summary>
        public class ClassData
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string RoomName { get; set; }
            public Nullable<int> RoomID { get; set; }
            public string HomeroomTeacherName { get; set; }
            public Nullable<int> HomeroomTeacherID { get; set; }
            public List<LessonInClass> LessonsInThisClass { get; set; }
            public List<string> StudentsInThisClass { get; set; }
            public int StudentCount { get; set; }
        }

        /// <summary>
        /// A sub-structure meant to manage lesson schedueling
        /// </summary>
        public class LessonInClass
        {
            public int LessonID { get; set; }
            public int ClassID { get; set; }
            public string CourseName { get; set; }
            public string ClassName { get; set; }
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
        private ICommand _deleteClassCommand;
        private ICommand _updateClassCommand;
        private ICommand _createNewClassCommand;

        private SchoolEntities _schoolData;

        private ClassData _selectedClass;
        private string _selectedClassName;
        private int _selectedTeacher;
        private int _selectedRoom;

        public ObservableCollection<ClassData> _classesTableData;
        private ObservableDictionary<int, string> _availableTeachers;
        private ObservableDictionary<int, string> _availableRooms;
        private ObservableCollection<LessonInClass> _lessonsInSelectedClass;
        private ObservableCollection<string> _studentsInSelectedClass;

        // Used to recall previously visited positions
        private Nullable<int> _previousHomeroomTeacher;
        private Nullable<int> _previousRoom;

        private const int NOT_ASSIGNED = -1;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "Manage Classes"; } }

        //Commands

        /// <summary>
        /// Create a new class with given data
        /// </summary>
        public ICommand CreateNewClassCommand
        {
            get
            {
                if (_createNewClassCommand == null)
                {
                    _createNewClassCommand = new RelayCommand(p => CreateNewClass());
                }
                return _createNewClassCommand;
            }
        }

        /// <summary>
        /// Update selected class
        /// </summary>
        public ICommand UpdateClassCommand
        {
            get
            {
                if (_updateClassCommand == null)
                {
                    _updateClassCommand = new RelayCommand(p => UpdateSelectedClass());
                }
                return _updateClassCommand;
            }
        }

        /// <summary>
        /// Delete selected class
        /// </summary>
        public ICommand DeleteClassCommand
        {
            get
            {
                if (_deleteClassCommand == null)
                {
                    _deleteClassCommand = new RelayCommand(p => DeleteSelectedClass());
                }
                return _deleteClassCommand;
            }
        }

        // Business Logic Properties
        public ObservableCollection<ClassData> ClassesTableData 
        { 
            get
            {
                return _classesTableData;
            }
            set
            {
                if (_classesTableData != value)
                {
                    _classesTableData = value;
                    OnPropertyChanged("ClassesTableData");
                }
            }
        }
        public ClassData SelectedClass 
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
                    ViewSelectedClass(_selectedClass);
                    OnPropertyChanged("SelectedClass");
                }
            }
        }
        public string ClassName 
        { 
            get
            {
                return _selectedClassName;
            }
            set
            {
                if (_selectedClassName != value)
                {
                    _selectedClassName = value;
                    OnPropertyChanged("ClassName");
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
                return _selectedTeacher;
            }
            set
            {
                if (_selectedTeacher != value)
                {
                    _selectedTeacher = value;
                    OnPropertyChanged("SelectedTeacher");
                }
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
                return _selectedRoom;
            }
            set
            {
                if (_selectedRoom != value)
                {
                    _selectedRoom = value;
                    OnPropertyChanged("SelectedRoom");
                }
            }
        }

        public ObservableCollection<LessonInClass> LessonsInSelectedClass
        {
            get
            {
                return _lessonsInSelectedClass;
            }
            set
            {
                if (_lessonsInSelectedClass != value)
                {
                    _lessonsInSelectedClass = value;
                    OnPropertyChanged("LessonsInSelectedClass");
                }
            }
        }

        public ObservableCollection<string> StudentsInSelectedClass
        {
            get
            {
                return _studentsInSelectedClass;
            }
            set
            {
                if (_studentsInSelectedClass != value)
                {
                    _studentsInSelectedClass = value;
                    OnPropertyChanged("StudentsInSelectedClass");
                }
            }
        }

        #endregion

        #region The Constructor
        // Only management can see this
        public ClassManagementViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
            : base(messageBoxService)
        {
            HasRequiredPermissions = connectedPerson.isSecretary || connectedPerson.isPrincipal;
            _refreshDataCommand = refreshDataCommand;

            if (HasRequiredPermissions)
            {
                _schoolData = new SchoolEntities();
                AvailableTeachers = new ObservableDictionary<int, string>();
                AvailableRooms = new ObservableDictionary<int, string>();
            }

        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;

            // Generate a list of existing classes
            ClassesTableData = new ObservableCollection<ClassData>(_schoolData.Classes.AsEnumerable().Select(currClass => DbClassToClassData(currClass)).ToList());

            // Generate a list of available classes
            AvailableTeachers.Clear();

            // Not all classes have a teacher, so we add this option
            AvailableTeachers.Add(NOT_ASSIGNED, "No assigned teacher");

            // Generate a list of teachers that are not homeroom teachers 
            _schoolData.Teachers.Where(teacher => teacher.classID == null && !teacher.Person.User.isDisabled).ToList()
                .ForEach(teacher => AvailableTeachers.Add(teacher.teacherID, teacher.Person.firstName + " " + teacher.Person.lastName));

            // Generate a list of free rooms
            AvailableRooms.Clear();

            // Not all classes have an assigned room, so we add this option
            AvailableRooms.Add(NOT_ASSIGNED, "No assigned room");

            // Generate a list of rooms that are not tied to a specific class
            _schoolData.Rooms.Where(room => room.Classes.Count() == 0).ToList()
                .ForEach(room => AvailableRooms.Add(room.roomID, room.roomName));

            // Reset selections
            SelectedTeacher = NOT_ASSIGNED;
            SelectedRoom = NOT_ASSIGNED;

            // The selections are not updated properly in the view unless called again for some reason even after reintializing the view
            OnPropertyChanged("SelectedTeacher");
            OnPropertyChanged("SelectedRoom");
        }

        /// <summary>
        /// Converts the Model's Class object into the local classData sub-struct
        /// </summary>
        /// <param name="room">"Class" from the Model/Database</param>
        /// <returns>A matching classData sub-structure</returns>
        private ClassData DbClassToClassData(Class dbClass)
        {
            ClassData classData = new ClassData();

            // Match the easy data directly
            classData.Name = dbClass.className;
            classData.ID = dbClass.classID;

            // Check if the class has an associated room
            if (dbClass.roomID != null)
            {
                classData.RoomID = dbClass.roomID;
                classData.RoomName = dbClass.Room.roomName;
            }

            // Check if the class has an homeroom teacher
            if (dbClass.Teachers != null && dbClass.Teachers.Count > 0)
            {
                var classTeacher = dbClass.Teachers.Single();
                classData.HomeroomTeacherID = classTeacher.teacherID;
                classData.HomeroomTeacherName = classTeacher.Person.firstName + " " + classTeacher.Person.lastName;
            }

            // Better as a ternary condition?
            if (dbClass.Students != null)
            {
                classData.StudentCount = dbClass.Students.Count();
            }
            else
            {
                classData.StudentCount = 0;
            }

            // Check if the class has students associated with it
            if (dbClass.Students != null && dbClass.Students.Count > 0)
            {
                classData.StudentsInThisClass = dbClass.Students.Where(student => !student.Person.User.isDisabled)
                    .Select(student => student.Person.firstName + " " + student.Person.lastName).ToList();
            }

            // Check if the class has lessons associated with it
            if (dbClass.Lessons != null && dbClass.Lessons.Count > 0)
            {
                classData.LessonsInThisClass = dbClass.Lessons.Select(lesson => new LessonInClass()
                    {
                        LessonID = lesson.lessonID,
                        ClassID = lesson.classID,
                        ClassName = lesson.Class.className,
                        CourseName = lesson.Course.courseName,
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

            return classData;
        }

        /// <summary>
        /// Choose a specific class and view its information.
        /// </summary>
        /// <param name="selectedClass">The class's data</param>
        private void ViewSelectedClass(ClassData selectedClass)
        {
            // Remove previous selections
            ClassName = string.Empty;
            SelectedTeacher = NOT_ASSIGNED;
            SelectedRoom = NOT_ASSIGNED;
            StudentsInSelectedClass = new ObservableCollection<string>();
            LessonsInSelectedClass = new ObservableCollection<LessonInClass>();

            // Remove previous room and homeroom teachers since they are already assigned to the previously selected classes
            if (_previousHomeroomTeacher != null)
            {
                AvailableTeachers.Remove(_previousHomeroomTeacher.Value);
            }

            if (_previousRoom != null)
            {
                AvailableRooms.Remove(_previousRoom.Value);
            }

            // Use the selected class to update all of the properties
            if (selectedClass != null)
            {
                ClassName = selectedClass.Name;

                // If possible, add a teacher assigned to the class as the first in the available teachers list
                if (selectedClass.HomeroomTeacherID != null)
                {
                    AvailableTeachers.Add(selectedClass.HomeroomTeacherID.Value, selectedClass.HomeroomTeacherName);
                    SelectedTeacher = selectedClass.HomeroomTeacherID.Value;
                }
                // If possible, add a room assigned to the class as the first in the available rooms list
                if (selectedClass.RoomID != null)
                {
                    AvailableRooms.Add(selectedClass.RoomID.Value, selectedClass.RoomName);
                    SelectedRoom = selectedClass.RoomID.Value;
                }

                // Save the homeroom teacher and room IDs so they can be removed when we select another class
                _previousHomeroomTeacher = selectedClass.HomeroomTeacherID;
                _previousRoom = selectedClass.RoomID;

                // Generate a list of students for the current class
                if (selectedClass.StudentsInThisClass != null)
                {
                    StudentsInSelectedClass = new ObservableCollection<string>(selectedClass.StudentsInThisClass);
                }

                // Generate a list of lessons for the current class
                if (selectedClass.LessonsInThisClass != null)
                {
                    LessonsInSelectedClass = new ObservableCollection<LessonInClass>(selectedClass.LessonsInThisClass);
                }
            }
        }

        /// <summary>
        /// Create a new class with current data
        /// </summary>
        private void CreateNewClass()
        {
            // If the Class' name is not unique, the assignment fails
            if (_schoolData.Classes.Where(classData => classData.className == ClassName).Any())
            {
                _messageBoxService.ShowMessage("This name is already taken. Please select a different name", "Failed class creation", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            // If the a room already hosts a class, the assignment fails
            else if (SelectedRoom != NOT_ASSIGNED && _schoolData.Classes.Where(currClass => currClass.roomID == SelectedRoom).Any())
            {
                _messageBoxService.ShowMessage("This room already belongs to a different class. Please update its information to proceed",
                                               "Failed class creation", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            // If the teacher is already a homeroom teacher for another class, the assignment fails
            else if (SelectedTeacher != NOT_ASSIGNED && _schoolData.Teachers.Find(SelectedTeacher).classID != null)
            {
                _messageBoxService.ShowMessage("Teacher is already responsible for another class. Please update their information to proceed",
                                               "Failed class creation", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Create a new class
                Class newClass = new Class() { className = ClassName };
                if (SelectedRoom != NOT_ASSIGNED)
                {
                    newClass.roomID = SelectedRoom;
                }
                else
                {
                    newClass.roomID = null;
                }

                _schoolData.Classes.Add(newClass);
                _schoolData.SaveChanges();

                // Update a selected teacher
                if (SelectedTeacher != NOT_ASSIGNED)
                {
                    _schoolData.Teachers.Find(SelectedTeacher).classID = newClass.classID;
                    _schoolData.SaveChanges();
                }

                // Report action success
                _messageBoxService.ShowMessage("The Class " + ClassName + " Was created successfully!", "Class created", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);

                // Update data in all screens
                _refreshDataCommand.Execute(null);
            }
        }

        /// <summary>
        /// Update the currently selected class
        /// </summary>
        private void UpdateSelectedClass()
        {
            // Default check for no selected class
            if (SelectedClass == null)
            {
                _messageBoxService.ShowMessage("Please selected a class first",
                                               "Class update failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Get relevent class, room and teacher information
                Class selectedClassModel = _schoolData.Classes.Find(SelectedClass.ID);
                Room previousRoom = _schoolData.Rooms.Find(_previousRoom);
                Room selectedRoom = _schoolData.Rooms.Find(SelectedRoom);
                Teacher previousTeacher = _schoolData.Teachers.Find(_previousHomeroomTeacher);
                Teacher selectedTeacher = _schoolData.Teachers.Find(SelectedTeacher);

                //If the Class' name is not unique, the assignment fails
                if (_schoolData.Classes.Where(classData => classData.classID != selectedClassModel.classID && classData.className == ClassName).Any())
                {
                    _messageBoxService.ShowMessage("This name is already taken. Please select a different name", "Class update failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
                // If the teacher is already a homeroom teacher for another class, the assignment fails
                else if (SelectedRoom != NOT_ASSIGNED && selectedRoom.Classes != null && selectedRoom.Classes.Single().classID != selectedClassModel.classID)
                {
                    _messageBoxService.ShowMessage("This room already belongs to a different class. Please update its information to proceed",
                                                   "Class update failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
                // Check that the selected homeroom teacher doesn't have another class already
                else if (SelectedTeacher != NOT_ASSIGNED && selectedTeacher.classID != null && selectedTeacher.classID != selectedClassModel.classID)
                {
                    _messageBoxService.ShowMessage("Teacher is already responsible for another class. Please update their information to proceed",
                                                   "Class update failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
                else
                {
                    // Update class data
                    selectedClassModel.roomID = SelectedRoom;
                    selectedClassModel.className = ClassName;

                    // if the homeroom teacher changed, remove the previous one
                    if (previousTeacher != null && previousTeacher.teacherID != SelectedTeacher)
                    {
                        previousTeacher.classID = null;
                        _schoolData.SaveChanges();

                        // note: the previous homeroom property is removed directly
                        this._previousHomeroomTeacher = null;
                    }

                    // Room association is saved in the class so there's no need to save it twice - just remove it.
                    this._previousRoom = null;

                    // Add the class to the selected teacher
                    if (SelectedTeacher != NOT_ASSIGNED)
                    {
                        // Update teacher to use this class's ID
                        selectedTeacher.classID = selectedClassModel.classID; 
                    }

                    // Save and report the changes
                    _schoolData.SaveChanges();
                    _messageBoxService.ShowMessage("Class " + ClassName + " was updated successfully.", "Class updated", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                    _refreshDataCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Delete the selected class
        /// </summary>
        private void DeleteSelectedClass()
        {
            // Default check for no selected class
            if (SelectedClass == null)
            {
                _messageBoxService.ShowMessage("Please selected a class first",
                                               "Failed to delete class", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // find class to delete
                Class selectedClass = _schoolData.Classes.Find(SelectedClass.ID);

                // As is the case for all deletion actions, confirm with the user.
                bool confirmation = _messageBoxService.ShowMessage("Are you certain that you wish to delete " + selectedClass.className + "?",
                                                                    "Deletion confirmation", MessageType.ACCEPT_CANCEL_MESSAGE, MessagePurpose.INFORMATION);
                if (confirmation == true)
                {
                    // deal with the class' potential relevant depndencies (events, students)
                    _schoolData.Students.Where(student => student.classID == selectedClass.classID)
                                            .ToList().ForEach(student => student.classID = null);
                    _schoolData.Events.Where(eventData => eventData.recipientClassID == selectedClass.classID)
                                            .ToList().ForEach(eventData => eventData.recipientClassID = null);

                    // delete all lessons for this class
                    var classLessons = _schoolData.Lessons.Where(lesson => lesson.classID == selectedClass.classID);
                    foreach (var lesson in classLessons)
                    {
                        _schoolData.Lessons.Remove(lesson);
                    }

                    // Class is removed, clear 'previous' properties
                    this._previousHomeroomTeacher = null;
                    this._previousRoom = null;

                    // Delete the class itself
                    _schoolData.Classes.Remove(selectedClass);

                    // Save and report changes
                    _schoolData.SaveChanges();
                    _messageBoxService.ShowMessage("Class " + selectedClass.className + " was deleted successfully",
                            "Class deletion complete", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                    _refreshDataCommand.Execute(null);
                }
            }
        }
        #endregion
    }
}
