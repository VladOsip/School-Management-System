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
    /// Creating and sending messages
    /// </summary>
    public class CreateMessageViewModel : BaseViewModel, IScreenViewModel
    {
        #region Fields
        private ICommand _refreshDataCommand;
        private ICommand _sendMessageCommand;

        private SchoolEntities _schoolData;

        private ObservableDictionary<int, string> _recipients;
        private int _selectedRecipient;

        private bool _sendingToEveryone;
        private bool _sendingToManagement;
        private bool _sendingToTeacher;
        private bool _sendingToClass;
        private bool _sendingToParent;
        private bool _sendingToStudent;

        private bool _canSendToClass;
        private bool _canSendToEveryone;

        private string _messageTitle;
        private string _messageText;

        private int NOT_ASSIGNED = -1;
        private int EVERYONE_OPTION = 0;
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "Send Message"; } }

        // Commands

        /// <summary>
        /// Create a new message with the current data and send it
        /// </summary>
        public ICommand SendMessageCommand
        {
            get
            {
                if (_sendMessageCommand == null)
                {
                    _sendMessageCommand = new RelayCommand(p => CreateNewMessage());
                }
                return _sendMessageCommand;
            }
        }

        // Business Logic 
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

                    // If relevant, update the recipients list
                    if (value == true)
                    {
                        UpdateRecipientsList(MessageRecipientsTypes.Everyone);
                    }
                }
            }
        }
        public bool SendingToManagement
        {
            get
            {
                return _sendingToManagement;
            }
            set
            {
                if (_sendingToManagement != value)
                {
                    _sendingToManagement = value;
                    OnPropertyChanged("SendingToManagement");

                    // If relevant, update the recipients list
                    if (value == true)
                    {
                        UpdateRecipientsList(MessageRecipientsTypes.Management);
                    }
                }
            }
        }
        public bool SendingToTeacher
        {
            get
            {
                return _sendingToTeacher;
            }
            set
            {
                if (_sendingToTeacher != value)
                {
                    _sendingToTeacher = value;
                    OnPropertyChanged("SendingToTeacher");

                    // If relevant, update the recipients list
                    if (value == true)
                    {
                        UpdateRecipientsList(MessageRecipientsTypes.Teachers);
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

                    // If relevant, update the recipients list
                    if (value == true)
                    {
                        UpdateRecipientsList(MessageRecipientsTypes.Class);
                    }
                }
            }
        }
        public bool SendingToParent
        {
            get
            {
                return _sendingToParent;
            }
            set
            {
                if (_sendingToParent != value)
                {
                    _sendingToParent = value;

                    // If relevant, update the recipients list
                    if (value == true)
                    {
                        UpdateRecipientsList(MessageRecipientsTypes.Parent);
                    }

                    OnPropertyChanged("SendingToParent");
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

                    // If relevant, update the recipients list
                    if (value == true)
                    {
                        UpdateRecipientsList(MessageRecipientsTypes.Students);
                    }
                }
            }
        }

        public bool CanSendToClass
        {
            get
            {
                return _canSendToClass;
            }
            set
            {
                if (_canSendToClass != value)
                {
                    _canSendToClass = value;
                    OnPropertyChanged("CanSendToClass");
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

        public string MessageTitle
        {
            get
            {
                return _messageTitle;
            }
            set
            {
                if (_messageTitle != value)
                {
                    _messageTitle = value;
                    OnPropertyChanged("MessageTitle");
                }
            }
        }
        public string MessageText
        { 
            get
            {
                return _messageText;
            }
            set
            {
                if (_messageText != value)
                {
                    _messageText = value;
                    OnPropertyChanged("MessageText");
                }
            }
        }
        #endregion

        #region The Constructor
        public CreateMessageViewModel(Person connectedPerson, ICommand refreshDataCommand, IMessageBoxService messageBoxService)
            : base (messageBoxService)
        {
            _refreshDataCommand = refreshDataCommand;
            _schoolData = new SchoolEntities();

            // Set permissions
            HasRequiredPermissions = true;

            if (connectedPerson.isTeacher || connectedPerson.isPrincipal || connectedPerson.isSecretary)
            {
                CanSendToClass = true;
            }

            if (connectedPerson.isPrincipal || connectedPerson.isSecretary)
            {
                CanSendToEveryone = true;
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
        /// Empty the form from selections
        /// </summary>
        public void ResetAll()
        {
            Recipients = new ObservableDictionary<int, string>();

            // uses student permissions by default since they're the lowest
            SendingToStudent = true;
            SendingToClass = false;
            SendingToEveryone = false;
            SendingToTeacher = false;
            SendingToParent = false;
            SendingToManagement = false;

            MessageTitle = string.Empty;
            MessageText = string.Empty;
        }

        /// <summary>
        /// Uses the current category selection and creates the list of recipients accordingly
        /// </summary>
        /// <param name="recipientsType">The category of recipients to use</param>
        private void CreateRecipientsList(MessageRecipientsTypes recipientsType)
        {
            // Reset recipients
            Recipients.Clear();

            // Create students list
            switch (recipientsType)
            {
                case MessageRecipientsTypes.Everyone:
                    Recipients.Add(EVERYONE_OPTION, "Entire school");
                    break;

                case MessageRecipientsTypes.Management:
                    // If the user has relevant permissions, they can send to every managerial person at once
                    if (CanSendToEveryone)
                    {
                        Recipients.Add(EVERYONE_OPTION, "All managers");
                    }
                    // Generate list of all available managers
                    // Reminder: managers = principle + secreteries
                    _schoolData.Persons.Where(person => !person.User.isDisabled && (person.isPrincipal || person.isSecretary)).ToList()
                        .ForEach(person => Recipients.Add(person.personID, person.firstName + " " + person.lastName));
                    break;

                case MessageRecipientsTypes.Teachers:
                    // If the user has relevant permissions, they can send to every teacher at once
                    if (CanSendToEveryone)
                    {
                        Recipients.Add(EVERYONE_OPTION, "All teachers");
                    }
                    // Generate list of all available teachers
                    _schoolData.Persons.Where(person => !person.User.isDisabled && person.isTeacher).ToList()
                        .ForEach(person => Recipients.Add(person.personID, person.firstName + " " + person.lastName));
                    break;

                case MessageRecipientsTypes.Class:
                    // If the user has relevant permissions, they can send to every class person at once
                    _schoolData.Classes.ToList().ForEach(schoolClass => Recipients.Add(schoolClass.classID, schoolClass.className));
                    break;

                case MessageRecipientsTypes.Parent:
                    // Generate list of all available parents
                    _schoolData.Persons.Where(person => !person.User.isDisabled && person.isParent).ToList()
                        .ForEach(person => Recipients.Add(person.personID, person.firstName + " " + person.lastName));
                    break;

                case MessageRecipientsTypes.Students:
                    // If the user has relevant permissions, they can send to every student at once
                    if (CanSendToEveryone)
                    {
                        Recipients.Add(EVERYONE_OPTION, "All students");
                    }
                    // Generate list of all available students
                    _schoolData.Persons.Where(person => !person.User.isDisabled && person.isStudent).ToList()
                        .ForEach(person => Recipients.Add(person.personID, person.firstName + " " + person.lastName));
                    break;

                default:
                    throw new ArgumentException("Invalid recipient type!");
            }
        }

        /// <summary>
        /// Create a new message and send it
        /// </summary>
        private void CreateNewMessage()
        {
            ValidityResult inputValid = IsInputValid();
            if (inputValid.Valid)
            {
                // Create a message based on the recipient's category
                // see if you should send a message to everyone from a specific group
                if (SendingToManagement && SelectedRecipient == EVERYONE_OPTION)
                {
                    MessagesHandler.CreateMessage(MessageTitle, MessageText, MessageRecipientsTypes.Management, ConnectedPerson.personID);
                }
                else if (SendingToTeacher && SelectedRecipient == EVERYONE_OPTION)
                {
                    MessagesHandler.CreateMessage(MessageTitle, MessageText, MessageRecipientsTypes.Teachers, ConnectedPerson.personID);
                }
                else if (SendingToStudent && SelectedRecipient == EVERYONE_OPTION)
                {
                    MessagesHandler.CreateMessage(MessageTitle, MessageText, MessageRecipientsTypes.Students, ConnectedPerson.personID);
                }
                if (SendingToEveryone)
                {
                    MessagesHandler.CreateMessage(MessageTitle, MessageText, MessageRecipientsTypes.Everyone, ConnectedPerson.personID);
                }
                // message handling for a specific class
                else if (SendingToClass)
                {
                    MessagesHandler.CreateMessage(MessageTitle, MessageText, MessageRecipientsTypes.Class, ConnectedPerson.personID, SelectedRecipient);
                }
                // messages fora specific person
                else
                {
                    MessagesHandler.CreateMessage(MessageTitle, MessageText, MessageRecipientsTypes.Person, ConnectedPerson.personID, SelectedRecipient);
                }

                // Update data and report success
                _messageBoxService.ShowMessage("Message created successfully and send to " + Recipients[SelectedRecipient],
                                                "Message sent", MessageType.OK_MESSAGE, MessagePurpose.INFORMATION);
                _refreshDataCommand.Execute(null);
            }
            else
            {
                _messageBoxService.ShowMessage(inputValid.ErrorReport, "Failed to send message", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }

        /// <summary>
        /// update the recipients after a category is selected
        /// </summary>
        /// <param name="recipientsType">The category of recipients to use</param>
        private void UpdateRecipientsList(MessageRecipientsTypes recipientsType)
        {
            // Create the list of recipients for a category
            CreateRecipientsList(recipientsType);

            // Select first recipent (if available)
            SelectedRecipient = (Recipients.Count() > 0) ? Recipients.First().Key : NOT_ASSIGNED;

            // Note: unlesss reinitialized manually, it fails to do so by itself for some reason
            OnPropertyChanged("SelectedRecipient");
        }

        /// <summary>
        /// validity check for the message
        /// </summary>
        /// <returns>Input validity (true/false)</returns>
        private ValidityResult IsInputValid()
        {
            ValidityResult result = new ValidityResult() { Valid = true };

            // Check if a recipient was selected
            if (SelectedRecipient == NOT_ASSIGNED)
            {
                result.Valid = false;
                result.ErrorReport = "Recipient not selected";
            }
            else if (MessageTitle == string.Empty)
            {
                result.Valid = false;
                result.ErrorReport = "No title input";
            }
            else if (MessageText == string.Empty)
            {
                result.Valid = false;
                result.ErrorReport = "No content input";
            }

            return result;
        }
        #endregion
    }
}
