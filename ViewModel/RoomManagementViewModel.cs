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
    /// Manages the rooms in the school
    /// </summary>
    public class RoomManagementViewModel : BaseViewModel, IScreenViewModel
    {
        #region Structures

        /// <summary>
        /// A sub-structure that represents information for a given room
        /// </summary>
        public class RoomData
        {
            public int ID { get; set; }
            public string Name { get; set; }     
            public string HomeroomClassName { get; set; }
            public Nullable<int> HomeroomClassID { get; set; }
            public List<LessonsInRoom> LessonsInThisRoom { get; set; }
        }

        /// <summary>
        /// A sub-structure that represents information for a lesson in a given room
        /// </summary>
        public class LessonsInRoom
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
        private ICommand _createNewRoomCommand;
        private ICommand _updateRoomCommand;
        private ICommand _deleteRoomCommand;
        private ICommand _refreshDataCommand;

        private SchoolEntities _schoolData;

        private RoomData _selectedRoom;
        private string _selectedRoomName;
        private int _selectedClass;

        public ObservableCollection<RoomData> _roomsTableData;
        private ObservableDictionary<int, string> _availableClasses;
        private ObservableCollection<LessonsInRoom> _lessonsInSelectedRoom;

        private Nullable<int> _previousRoomClass;

        private const int NO_ASSIGNED_CLASS = -1;
        #endregion

        #region Properties / Commands

        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "Manage Rooms"; } }

        // Commands

        /// <summary>
        /// Create a new room with the current data
        /// </summary>
        public ICommand CreateNewRoomCommand
        {
            get
            {
                if (_createNewRoomCommand == null)
                {
                    _createNewRoomCommand = new RelayCommand(p => CreateNewRoom());
                }
                return _createNewRoomCommand;
            }
        }

        /// <summary>
        /// Update the currently selected room
        /// </summary>
        public ICommand UpdateRoomCommand
        {
            get
            {
                if (_updateRoomCommand == null)
                {
                    _updateRoomCommand = new RelayCommand(p => UpdateSelectedRoom());
                }
                return _updateRoomCommand;
            }
        }
        /// <summary>
        /// Delete the currently selected room
        /// </summary>
        public ICommand DeleteRoomCommand
        {
            get
            {
                if (_deleteRoomCommand == null)
                {
                    _deleteRoomCommand = new RelayCommand(p => DeleteSelectedRoom());
                }
                return _deleteRoomCommand;
            }
        }

        // Business Logic Properties
        public ObservableCollection<RoomData> RoomsTableData 
        { 
            get
            {
                return _roomsTableData;
            }
            set
            {
                if (_roomsTableData != value)
                {
                    _roomsTableData = value;
                    OnPropertyChanged("RoomsTableData");
                }
            }
        }

        public RoomData SelectedRoom 
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
                    UseSelectedRoom(_selectedRoom);
                    OnPropertyChanged("SelectedRoom");
                }
            }
        }

        public ObservableCollection<LessonsInRoom> LessonsInSelectedRoom
        {
            get
            {
                return _lessonsInSelectedRoom;
            }
            set
            {
                if (_lessonsInSelectedRoom != value)
                {
                    _lessonsInSelectedRoom = value;
                    OnPropertyChanged("LessonsInSelectedRoom");
                }
            }
        }

        public string RoomName 
        { 
            get
            {
                return _selectedRoomName;
            }
            set
            {
                if (_selectedRoomName != value)
                {
                    _selectedRoomName = value;
                    OnPropertyChanged("RoomName");
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
                return _selectedClass;
            }
            set
            {
                if (_selectedClass != value)
                {
                    _selectedClass = value;
                    OnPropertyChanged("SelectedClass");
                }
            }
        }
        #endregion

        #region The constructor
        public RoomManagementViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
            : base(messageBoxService)
        {
            HasRequiredPermissions = connectedPerson.isSecretary || connectedPerson.isPrincipal;
            _refreshDataCommand = refreshDataCommand;

            if (HasRequiredPermissions)
            {
                _schoolData = new SchoolEntities();
                AvailableClasses = new ObservableDictionary<int, string>();
            }

        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;

            // Generate existing room list
            RoomsTableData = new ObservableCollection<RoomData>(_schoolData.Rooms.AsEnumerable().Select(room => DbRoomToRoomData(room)).ToList());

            // Generate available classes list
            AvailableClasses.Clear();

            // Note: not all rooms are assigned to a specific class
            AvailableClasses.Add(NO_ASSIGNED_CLASS, "No Assigned Class");

            // Generate list of classes without a homeroom
            _schoolData.Classes.Where(schoolClass => schoolClass.roomID == null).ToList()
                .ForEach(schoolClass => AvailableClasses.Add(schoolClass.classID, schoolClass.className));

            SelectedClass = NO_ASSIGNED_CLASS;

            // For some reason the selections are not updated properly in the view unless called again
            OnPropertyChanged("SelectedClass"); 
        }

        /// <summary>
        /// Converts Database "Room" class into a local "RoomData" class
        /// </summary>
        /// <param name="Room">Database room</param>
        /// <returns>Local RoomData version of room</returns>
        private RoomData DbRoomToRoomData(Room room)
        {
            RoomData roomData = new RoomData();
            roomData.ID = room.roomID;
            roomData.Name = room.roomName;

            // Check for homeroom
            if (room.Classes != null && room.Classes.Count > 0)
            {
                roomData.HomeroomClassID = room.Classes.Single().classID;
                roomData.HomeroomClassName = room.Classes.Single().className;
            }

            // Check for associated lessons for room
            if (room.Lessons != null && room.Lessons.Count > 0)
            {
                roomData.LessonsInThisRoom = room.Lessons.Select(lesson => new LessonsInRoom()
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

            return roomData;
        }

        /// <summary>
        /// View info of the selected room
        /// </summary>
        /// <param name="selectedRoom">room data</param>
        private void UseSelectedRoom(RoomData selectedRoom)
        {
            // remove previous selection
            SelectedClass = NO_ASSIGNED_CLASS;
            RoomName = string.Empty;
            LessonsInSelectedRoom = new ObservableCollection<LessonsInRoom>();

            // Remove previously chosen room for available choises (it has been assigned)
            if (_previousRoomClass != null)
            {
                AvailableClasses.Remove(_previousRoomClass.Value);
            }

            // Update properties with select room information
            if (selectedRoom != null)
            {
                RoomName = selectedRoom.Name;

                // Homerooms are added first if they exist
                if (selectedRoom.HomeroomClassID != null)
                {
                    AvailableClasses.Add(selectedRoom.HomeroomClassID.Value, selectedRoom.HomeroomClassName);
                    SelectedClass = selectedRoom.HomeroomClassID.Value;
                }

                // Save room ID so that it may be removed
                _previousRoomClass = selectedRoom.HomeroomClassID;

                // Generate list of lessons for current room
                if (selectedRoom.LessonsInThisRoom != null)
                {
                    LessonsInSelectedRoom = new ObservableCollection<LessonsInRoom>(selectedRoom.LessonsInThisRoom);
                }
            }
        }

        /// <summary>
        /// Create new room
        /// </summary>
        private void CreateNewRoom()
        {
            // Check for no name duplicates
            if (_schoolData.Rooms.Where(room => room.roomName == RoomName).Any())
            {
                _messageBoxService.ShowMessage("Room name is taken. Please choose a different name", "Room creation failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            // check if room is already a homeroom
            else if (SelectedClass != NO_ASSIGNED_CLASS && _schoolData.Classes.Find(SelectedClass).roomID != null)
            {
                _messageBoxService.ShowMessage("Chosen class already has a room, please update previous choices to proceed",
                                               "Room creation failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Creat new room
                Room newRoom = new Room() { roomName = RoomName };
                _schoolData.Rooms.Add(newRoom);
                _schoolData.SaveChanges();

                // Update class if relevant
                if (SelectedClass != NO_ASSIGNED_CLASS)
                {
                    _schoolData.Classes.Find(SelectedClass).roomID = newRoom.roomID;
                    _schoolData.SaveChanges();
                }

                // Report and save action
                _messageBoxService.ShowMessage("The room " + RoomName + " Was created successfully", "Room created successfully", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                _refreshDataCommand.Execute(null);
            }
        }

        /// <summary>
        /// Update selected room
        /// </summary>
        private void UpdateSelectedRoom()
        {
            // Verify room selection
            if (SelectedRoom == null)
            {
                _messageBoxService.ShowMessage("Please select a room first",
                                               "Update failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Find the relevant room and class if applicable
                Room selectedRoom = _schoolData.Rooms.Find(SelectedRoom.ID);
                Class previousClass = _schoolData.Classes.Where(schoolClass => schoolClass.roomID == selectedRoom.roomID).FirstOrDefault();
                Class selectedClass = _schoolData.Classes.Find(SelectedClass);

                // Verify single instance of room name
                if (_schoolData.Rooms.Where(room => room.roomID != selectedRoom.roomID && room.roomName == RoomName).Any())
                {
                    _messageBoxService.ShowMessage("Room name is taken. Please select a different name", "Update failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
                // Check for single instance of homeroom for a class
                else if (SelectedClass != NO_ASSIGNED_CLASS && selectedClass.roomID != null && selectedClass.roomID != selectedRoom.roomID)
                {
                    _messageBoxService.ShowMessage("Selected class already has a homeroom. Please change previous choices",
                                                   "Update failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
                else
                {
                    // Update room data
                    selectedRoom.roomName = RoomName;

                    // Remove room from associated class if applicable
                    if (previousClass != null && previousClass.roomID != SelectedClass)
                    {
                        previousClass.roomID = null;

                        // Clear previous room property
                        this._previousRoomClass = null;
                    }

                    // Add room to the selected class
                    if (SelectedClass != NO_ASSIGNED_CLASS)
                    {
                        // Update the class to use this room's ID
                        selectedClass.roomID = selectedRoom.roomID;
                    }

                    // Update the model
                    _schoolData.SaveChanges();

                    // Report action success
                    _messageBoxService.ShowMessage("The room " + RoomName + " was updated successfully", "Update successful", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);

                    // Update data in all screens
                    _refreshDataCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Delete selected room
        /// </summary>
        private void DeleteSelectedRoom()
        {
            // verify room selection
            if (SelectedRoom == null)
            {
                _messageBoxService.ShowMessage("Please select room first",
                                               "Deletion failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
            else
            {
                // Get information on the room to be deleted
                Room selectedRoom = _schoolData.Rooms.Find(SelectedRoom.ID);

                // Cofirm deletion with the user as always
                bool confirmation = _messageBoxService.ShowMessage("Are you sure that you wish to delete " + selectedRoom.roomName + "?",
                                                                    "Room deletion", MessageType.ACCEPT_CANCEL_MESSAGE, MessagePurpose.INFORMATION);
                if (confirmation == true)
                {
                    // Remove room for associated class if applicable
                    Class previousClass = _schoolData.Classes.Where(schoolClass => schoolClass.roomID == selectedRoom.roomID).FirstOrDefault();
                    if (previousClass != null)
                    {
                        previousClass.roomID = null;
                    }
                    
                    // Clear the previous room property
                    this._previousRoomClass = null;

                    // Remove the room from its associated lessons
                    _schoolData.Lessons.Where(lesson => lesson.roomID == selectedRoom.roomID).ToList().ForEach(lesson => lesson.roomID = null);

                    // Delete the room 
                    _schoolData.Rooms.Remove(selectedRoom);

                    // Save and report changes
                    _schoolData.SaveChanges();
                    _messageBoxService.ShowMessage("The room " + selectedRoom.roomName + " was deleted succesfully",
                            "Deletion complete", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                    _refreshDataCommand.Execute(null);
                }
            }
        }
        #endregion
    }
}
