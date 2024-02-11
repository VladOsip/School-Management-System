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
    /// Creating and sending an event
    /// </summary>
    public class EventManagementViewModel : BaseViewModel, IScreenViewModel
    {
        #region Structures
        private enum EventRecipientsTypes
        {
            Students,
            Classes,
            Everyone,
        }

        private enum ActionOnEvent
        { 
            Created,
            Deleted,
            Updated
        }
        /// <summary>
        /// A sub-structure that represents information for a given event
        /// </summary>
        public class EventData
        {
            public int ID { get; set; }
            public string EventName { get; set; }
            public string EventText { get; set; }
            public string EventLocation { get; set; }
            public string SubmitterName { get; set; }
            public DateTime EventDatetime { get; set; }
        }
        #endregion

        #region Fields
        private ICommand _sendEventCommand;
        private ICommand _updateEventCommand;
        private ICommand _deleteEventCommand;
        private ICommand _refreshDataCommand;

        private SchoolEntities _schoolData;

        private bool _searchingSchoolEvents;
        private bool _searchingClassEvents;
        private bool _searchingStudentEvents;
        private int _selectedSearchChoiceID;

        private ObservableDictionary<int, string> _availableSearchChoices;
        public ObservableCollection<EventData> _eventsTableData;
        public EventData _selectedEvent;

        private ObservableDictionary<int, string> _recipients;
        private int _selectedRecipient;

        private bool _sendingToStudent;
        private bool _sendingToClass;
        private bool _sendingToEveryone;

        private bool _canSendToEveryone;

        private ObservableCollection<string> _possibleEvents;
        private ObservableCollection<string> _possibleLocations;
        private DateTime _eventDatetime;
        private string _eventLocation;
        private string _eventName;
        private string _eventText;

        private int NOT_ASSIGNED = -1;
        private int EVERYONE_OPTION = 0;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "Manage Events"; } }

        // Commands

        /// <summary>
        /// Create an event and send it
        /// </summary>
        public ICommand SendEventCommand
        {
            get
            {
                if (_sendEventCommand == null)
                {
                    _sendEventCommand = new RelayCommand(p => CreateNewEvent());
                }
                return _sendEventCommand;
            }
        }

        /// <summary>
        /// Update a given event
        /// </summary>
        public ICommand UpdateEventCommand
        {
            get
            {
                if (_updateEventCommand == null)
                {
                    _updateEventCommand = new RelayCommand(p => UpdateSelectedEvent());
                }
                return _updateEventCommand;
            }
        }

        /// <summary>
        /// Delete a given event
        /// </summary>
        public ICommand DeleteEventCommand
        {
            get
            {
                if (_deleteEventCommand == null)
                {
                    _deleteEventCommand = new RelayCommand(p => DeleteSelectedEvent());
                }
                return _deleteEventCommand;
            }
        }
         
        // Business Logic Properties

        public bool SearchingSchoolEvents
        {
            get
            {
                return _searchingSchoolEvents;
            }
            set
            {
                if (_searchingSchoolEvents != value)
                {
                    _searchingSchoolEvents = value;
                    OnPropertyChanged("SearchingSchoolEvents");

                    if (value == true)
                    {
                        // Due to how Epical switches the new choice on before changing the other off, causing two options to be on and messing with the selected event
                        // We need to turn the other search options off manually
                        SearchingStudentEvents = false;
                        SearchingClassEvents = false;
                        UseSelectedSearchCategory(EventRecipientsTypes.Everyone);
                    }
                }
            }
        }

        public bool SearchingClassEvents
        {
            get
            {
                return _searchingClassEvents;
            }
            set
            {
                if (_searchingClassEvents != value)
                {
                    _searchingClassEvents = value;
                    OnPropertyChanged("SearchingClassEvents");

                    if (value == true)
                    {
                        // Due to how Epical switches the new choice on before changing the other off, causing two options to be on and messing with the selected event
                        // We need to turn the other search options off manually
                        SearchingStudentEvents = false;
                        SearchingSchoolEvents = false;
                        UseSelectedSearchCategory(EventRecipientsTypes.Classes);
                    }
                }
            }
        }

        public bool SearchingStudentEvents
        {
            get
            {
                return _searchingStudentEvents;
            }
            set
            {
                if (_searchingStudentEvents != value)
                {
                    _searchingStudentEvents = value;
                    OnPropertyChanged("SearchingStudentEvents");

                    if (value == true)
                    {
                        // Due to how Epical switches the new choice on before changing the other off, causing two options to be on and messing with the selected event
                        // We need to turn the other search options off manually
                        SearchingClassEvents = false;
                        SearchingSchoolEvents = false;
                        UseSelectedSearchCategory(EventRecipientsTypes.Students);
                    }
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
                // The ID value might not change when we switched categories (e.g choosing first item in different categories)
                // so update everytime the 'set' is called
                _selectedSearchChoiceID = value;
                UseSelectedSearchChoice();
                OnPropertyChanged("SelectedSearchChoice");
            }
        }

        public ObservableCollection<EventData> EventsTableData
        {
            get
            {
                return _eventsTableData;
            }
            set
            {
                if (_eventsTableData != value)
                {
                    _eventsTableData = value;
                    OnPropertyChanged("EventsTableData");
                }
            }
        }
        public EventData SelectedEvent
        {
            get
            {
                return _selectedEvent;
            }
            set
            {
                if (_selectedEvent != value)
                {
                    _selectedEvent = value;
                    UseSelectedEvent(_selectedEvent);
                    OnPropertyChanged("SelectedEvent");
                }
            }
        }

        public ObservableDictionary<int, string> Recipients
        {
            get
            {
                return _recipients;
            }
            set
            {
                if (_recipients != value)
                {
                    _recipients = value;
                    OnPropertyChanged("Recipients");
                }
            }
        }
        public int SelectedRecipient 
        { 
            get
            {
                return _selectedRecipient;
            }
            set
            {
                if (_selectedRecipient != value)
                {
                    _selectedRecipient = value;
                    OnPropertyChanged("SelectedRecipient");
                }
            }
        }
        public bool CanSendToEveryone
        {
            get
            {
                return _canSendToEveryone;
            }
            set
            {
                if (_canSendToEveryone != value)
                {
                    _canSendToEveryone = value;
                    OnPropertyChanged("CanSendToEveryone");
                }
            }
        }
        public bool SendingToEveryone
        {
            get
            {
                return _sendingToEveryone;
            }
            set
            {
                if (_sendingToEveryone != value)
                {
                    _sendingToEveryone = value;
                    OnPropertyChanged("SendingToEveryone");

                    // Update recipients list if it is changing to this category
                    if (value == true)
                    {
                        UpdateRecipientsList(EventRecipientsTypes.Everyone);
                    }
                }
            }
        }
        public bool SendingToClass
        {
            get
            {
                return _sendingToClass;
            }
            set
            {
                if (_sendingToClass != value)
                {
                    _sendingToClass = value;
                    OnPropertyChanged("SendingToClass");

                    // Update recipients list if it is changing to this category
                    if (value == true)
                    {
                        UpdateRecipientsList(EventRecipientsTypes.Classes);
                    }
                }
            }
        }
        public bool SendingToStudent
        {
            get
            {
                return _sendingToStudent;
            }
            set
            {
                if (_sendingToStudent != value)
                {
                    _sendingToStudent = value;
                    OnPropertyChanged("SendingToStudent");

                    // Update recipients list if it is changing to this category
                    if (value == true)
                    {
                        UpdateRecipientsList(EventRecipientsTypes.Students);
                    }
                }
            }
        }
        public DateTime EventDatetime
        {
            get
            {
                return _eventDatetime;
            }
            set
            {
                if (_eventDatetime != value)
                {
                    _eventDatetime = value;
                    OnPropertyChanged("EventDatetime");
                }
            }
        }

        public ObservableCollection<string> PossibleLocations
        {
            get
            {
                return _possibleLocations;
            }
            set
            {
                if (_possibleLocations != value)
                {
                    _possibleLocations = value;
                    OnPropertyChanged("PossibleLocations");
                }
            }
        }
        public string EventLocation
        {
            get
            {
                return _eventLocation;
            }
            set
            {
                if (_eventLocation != value)
                {
                    _eventLocation = value;
                    OnPropertyChanged("EventLocation");
                }
            }
        }

        public ObservableCollection<string> PossibleEvents 
        { 
            get
            {
                return _possibleEvents;
            }
            set
            {
                if (_possibleEvents != value)
                {
                    _possibleEvents = value;
                    OnPropertyChanged("PossibleEvents");
                }
            }
        }
        public string EventName
        {
            get
            {
                return _eventName;
            }
            set
            {
                if (_eventName != value)
                {
                    _eventName = value;
                    OnPropertyChanged("EventName");
                }
            }
        }

        public string EventText
        { 
            get
            {
                return _eventText;
            }
            set
            {
                if (_eventText != value)
                {
                    _eventText = value;
                    OnPropertyChanged("EventText");
                }
            }
        }
        #endregion

        #region Constructors
        public EventManagementViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
        {
            _refreshDataCommand = refreshDataCommand;
            _messageBoxService = messageBoxService;

            // Set permissions
            if (connectedPerson.isTeacher || connectedPerson.isPrincipal || connectedPerson.isSecretary)
            {
                HasRequiredPermissions = true;

                if (connectedPerson.isPrincipal || connectedPerson.isSecretary)
                {
                    CanSendToEveryone = true;
                }
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
            ResetAll();
        }

        /// <summary>
        /// Reset all fields in the form
        /// </summary>
        private void ResetAll()
        {
            _schoolData = new SchoolEntities();
            EventsTableData = new ObservableCollection<EventData>();
            Recipients = new ObservableDictionary<int, string>();
            AvailableSearchChoices = new ObservableDictionary<int, string>();

            PossibleLocations = new ObservableCollection<string>(_schoolData.Rooms.Select(room => "room " + room.roomName).ToList());
            PossibleEvents =
                new ObservableCollection<string>(new List<string>() { "test", "trip", "vacation","meeting", "lesson change", "ceremony", "event" });

            // Use the student category as the default option due to it having the least permissions
            SearchingStudentEvents = true;
            SearchingClassEvents = false;
            SearchingSchoolEvents = false;
            SendingToStudent = true;
            SendingToClass = false;
            SendingToEveryone = false;

            SelectedSearchChoice = NOT_ASSIGNED;
            SelectedEvent = null;

            // Get tommorow's date by default
            EventDatetime = DateTime.Today.AddDays(1);

            EventLocation = string.Empty;
            EventName = string.Empty;
            EventText = string.Empty;
        }
        /// <summary>
        /// Updates the EventsTableData with the events from SelectedSearchChoice 
        /// </summary>
        private void UseSelectedSearchChoice()
        {
            if (SelectedSearchChoice != NOT_ASSIGNED)
            {
                // Make sure that the events table is clean
                EventsTableData.Clear();

                // Check which category the search choice is from, and fill the EventsTableData with the relevent events
                if (SearchingStudentEvents)
                {
                    // Get the chosen student's events
                    Student student = _schoolData.Students.Find(SelectedSearchChoice);
                    _schoolData.Events.Where(schoolEvent => schoolEvent.recipientID == student.studentID || schoolEvent.recipientClassID == student.classID).ToList().
                        ForEach(schoolEvent => EventsTableData.Add(DbEventToEventData(schoolEvent)));
                }
                else if (SearchingClassEvents)
                {
                    // Get the chosen class's events
                    _schoolData.Events.Where(schoolEvent => schoolEvent.recipientClassID == SelectedSearchChoice).ToList().
                        ForEach(schoolEvent => EventsTableData.Add(DbEventToEventData(schoolEvent)));
                }
                else if (SearchingSchoolEvents)
                {
                    // Get the event for the entire school
                    _schoolData.Events.Where(schoolEvent => schoolEvent.recipientClassID == null && schoolEvent.recipientID == null).ToList().
                        ForEach(schoolEvent => EventsTableData.Add(DbEventToEventData(schoolEvent)));
                }
                else
                {
                    // No known search category, so there are no events to show.
                }
            }
        }

        /// <summary>
        /// Choose a specific event and view its information.
        /// </summary>
        /// <param name="selectedEvent">The event's data</param>
        private void UseSelectedEvent(EventData selectedEvent)
        {
            // Update the properties to match the selected lesson
            if (selectedEvent != null)
            {
                SendingToStudent = SearchingStudentEvents;
                SendingToClass = SearchingClassEvents;
                SendingToEveryone = SearchingSchoolEvents;
                EventDatetime = selectedEvent.EventDatetime;
                EventLocation = selectedEvent.EventLocation;
                EventName = selectedEvent.EventName;
                EventText = selectedEvent.EventText;
            }
            else
            {
                // If no lesson was selected, reset the properties
                SendingToStudent = true;
                SendingToClass = false;
                SendingToEveryone = false;
                EventDatetime = DateTime.Today.AddDays(1);
                EventLocation = string.Empty;
                EventName = string.Empty;
                EventText = string.Empty;
            }
        }

        /// <summary>
        /// Updates the AvailableSearchChoices list with the options per the search category
        /// </summary>
        private void UseSelectedSearchCategory(EventRecipientsTypes searchCategory)
        {
            AvailableSearchChoices = CreateRecipientsList(searchCategory);

            // Use the first AvailableSearchChoices dictioanry option by default
            if (AvailableSearchChoices.Count > 0)
            {
                SelectedSearchChoice = AvailableSearchChoices.First().Key;
            }
        }

        /// <summary>
        /// Converts the Database's event class into a local EventData class
        /// </summary>
        /// <param name="lesson">Database "Lesson" item</param>
        /// <returns>Local LessonData version</returns>
        private EventData DbEventToEventData(Event schoolEvent)
        {
            EventData eventData = new EventData()
            {
                ID = schoolEvent.eventID,
                EventDatetime = schoolEvent.eventDate,
                EventLocation = schoolEvent.location,
                EventName = schoolEvent.name,
                EventText = schoolEvent.description,
                SubmitterName = schoolEvent.Submitter.firstName + " " + schoolEvent.Submitter.lastName
            };

            return eventData;
        }

        /// <summary>
        /// Create a dictionary of recipients using the currently selected category
        /// </summary>
        /// <param name="recipientsType">Recipients category</param>
        /// <returns>An ObservableDictionary containing all relevant recipients, organized in a <RecipientID, RecipientName> format</returns>
        private ObservableDictionary<int, string> CreateRecipientsList(EventRecipientsTypes recipientsType)
        {
            // Create recipient dictioanry
            ObservableDictionary<int, string> recipients = new ObservableDictionary<int, string>();

            // create a list of all relevant recipients.
            switch (recipientsType)
            {
                case EventRecipientsTypes.Everyone:
                    recipients.Add(EVERYONE_OPTION, "Entire School");
                    break;

                case EventRecipientsTypes.Classes:
                    _schoolData.Classes.ToList().ForEach(schoolClass => recipients.Add(schoolClass.classID, schoolClass.className));
                    break;

                case EventRecipientsTypes.Students:
                    _schoolData.Persons.Where(person => !person.User.isDisabled && person.isStudent).ToList()
                        .ForEach(person => recipients.Add(person.personID, person.firstName + " " + person.lastName));
                    break;

                default:
                    throw new ArgumentException("Invalid recipient type!");
            }

            return recipients;
        }

        /// <summary>
        /// Update the recipients list given a selected category
        /// </summary>
        /// <param name="recipientsType">Category of recipients</param>
        private void UpdateRecipientsList(EventRecipientsTypes recipientsType)
        {
            Recipients = CreateRecipientsList(recipientsType);
            SelectedRecipient = (Recipients.Count() > 0) ? Recipients.First().Key : NOT_ASSIGNED;

            // Due to an unknown issue, the property changed notification needs to be raised manually
            OnPropertyChanged("SelectedRecipient");
        }

        /// <summary>
        /// Delete selected event
        /// </summary>
        private void DeleteSelectedEvent()
        {
            // Check for selected room
            if (SelectedEvent != null)
            {
                // Get lesson to be deleted
                Event selectedEvent = _schoolData.Events.Find(SelectedEvent.ID);

                // Confirm deletion with the user
                bool confirmation = _messageBoxService.ShowMessage("Are you sure that you wish to proceed",
                                                                    "Event Deletion", MessageType.ACCEPT_CANCEL_MESSAGE, MessagePurpose.INFORMATION);
                if (confirmation == true)
                {
                    // Remove the lesson
                    _schoolData.Events.Remove(selectedEvent);

                    // Save and report changes
                    _schoolData.SaveChanges();
                    SendMessageAboutEvent(selectedEvent, ActionOnEvent.Deleted);
                    _messageBoxService.ShowMessage("Event deleted successfully",
                            "Event deletion", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                    _refreshDataCommand.Execute(null);
                }
            }
            else
            {
                _messageBoxService.ShowMessage("Please select an event first",
                                               "Event deletion failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }

        /// <summary>
        /// Update current event
        /// </summary>
        private void UpdateSelectedEvent()
        {
            // confirm event selection
            if (SelectedEvent != null)
            {
                // Check input validity
                ValidityResult inputValid = VerifyInput();
                if (inputValid.Valid)
                {
                    // Get the event that is going to be edited
                    Event selectedEvent = _schoolData.Events.Find(SelectedEvent.ID);

                    // Update the event's data
                    selectedEvent.submitterID = ConnectedPerson.personID;
                    selectedEvent.eventDate = EventDatetime;
                    selectedEvent.location = EventLocation;
                    selectedEvent.name = EventName;
                    selectedEvent.description = EventText;
                    SetEventRecipients(selectedEvent);

                    // Update the model
                    _schoolData.SaveChanges();

                    // Report action success
                    SendMessageAboutEvent(selectedEvent, ActionOnEvent.Updated);
                    _messageBoxService.ShowMessage("Event updated successfully", "Event updated", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);

                    // Update data in all screens
                    _refreshDataCommand.Execute(null);
                }
                else
                {
                    _messageBoxService.ShowMessage(inputValid.ErrorReport, "Failed to update event", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
            }
            else
            {
                _messageBoxService.ShowMessage("Please select an event to update",
                                               "Event update failed", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }

        /// <summary>
        /// use the current data to create and send an event
        /// </summary>
        private void CreateNewEvent()
        {
            ValidityResult inputValid = VerifyInput();
            if (inputValid.Valid)
            {
                // New event creation
                Event newEvent = new Event()
                {
                    eventDate = EventDatetime,
                    location = EventLocation,
                    name = EventName,
                    description = EventText,
                    submitterID = ConnectedPerson.personID
                };
                SetEventRecipients(newEvent);

                // Save the event, report success, and update all data
                _schoolData.Events.Add(newEvent);
                _schoolData.SaveChanges();
                SendMessageAboutEvent(newEvent, ActionOnEvent.Created);
                _messageBoxService.ShowMessage("Event created succcessfully and sent to " + Recipients[SelectedRecipient],
                                                "Event sent", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                _refreshDataCommand.Execute(null);
            }
            else
            {
                _messageBoxService.ShowMessage(inputValid.ErrorReport, "Failed event creation", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }

        /// <summary>
        /// set recipients of an event given their categorization
        /// </summary>
        /// <param name="schoolEvent"></param>
        private void SetEventRecipients(Event schoolEvent)
        {
            // Check first if you should sent to everyone
            // meaning no specific recipient
            if (SendingToEveryone)
            {
                schoolEvent.recipientID = null;
                schoolEvent.recipientClassID = null;
            }
            // else, if there is no specific recipient but specific class
            else if (SendingToClass)
            {
                schoolEvent.recipientID = null;
                schoolEvent.recipientClassID = SelectedRecipient;
            }
            // Finally, specific recipient
            else
            {
                schoolEvent.recipientID = SelectedRecipient;
                schoolEvent.recipientClassID = null;
            }
        }

        /// <summary>
        /// Send a message to the relevent recipients about an update in an event
        /// </summary>
        /// <param name="schoolEvent">The event to report</param>
        private void SendMessageAboutEvent(Event schoolEvent, ActionOnEvent action)
        {
            string eventTitle;
            string eventMessage;

            // Create the event message depending on the type of event
            switch (action)
            {
                case ActionOnEvent.Created:
                {
                    eventTitle = "New event created";
                    eventMessage = ConnectedPerson.firstName + " " + ConnectedPerson.lastName 
                        + " created the event '" + schoolEvent.name + "' at " + schoolEvent.eventDate;
                    break;
                }
                case ActionOnEvent.Updated:
                {
                     eventTitle = "Event information updated";
                     eventMessage = ConnectedPerson.firstName + " " + ConnectedPerson.lastName
                         + " Updated the event '" + schoolEvent.name + "' that is set at " + schoolEvent.eventDate;
                     break;
                }
                case ActionOnEvent.Deleted:
                {
                    eventTitle = "Event deleted";
                    eventMessage = ConnectedPerson.firstName + " " + ConnectedPerson.lastName
                        + " deleted the event '" + schoolEvent.name + "' that was set at " + schoolEvent.eventDate;
                    break;
                }
                default:
                {
                    throw new ArgumentException("Invalid ActionOnEvent type");
                }
            }

            // Check the recipients to see who to send to

            // Check if school event
            if (schoolEvent.recipientID == null && schoolEvent.recipientClassID == null)
            {
                MessagesHandler.CreateMessage(eventTitle, eventMessage, MessageRecipientsTypes.Everyone);
            }
            // Check if class event
            else if (schoolEvent.recipientClassID != null)
            {
                MessagesHandler.CreateMessage(eventTitle, eventMessage, MessageRecipientsTypes.Class, null, schoolEvent.recipientClassID.Value);
            }
            // check for specific person
            else
            {
                MessagesHandler.CreateMessage(eventTitle, eventMessage, MessageRecipientsTypes.Person, null, schoolEvent.recipientID.Value);
            }
        }
        
        /// <summary>
        /// Validate input
        /// </summary>
        /// <returns>input validity</returns>
        private ValidityResult VerifyInput()
        {
            ValidityResult result = new ValidityResult() { Valid = true };

            // Check if a recipient was selected
            if (SelectedRecipient == NOT_ASSIGNED)
            {
                result.Valid = false;
                result.ErrorReport = "No recipient chosen";
            }
            else if (EventName == string.Empty)
            {
                result.Valid = false;
                result.ErrorReport = "No title written";
            }
            else if (EventText == string.Empty)
            {
                result.Valid = false;
                result.ErrorReport = "No message written";
            }
            else if (EventDatetime == null)
            {
                result.Valid = false;
                result.ErrorReport = "No date selected";
            }

            return result;
        }
        #endregion
    }
}
