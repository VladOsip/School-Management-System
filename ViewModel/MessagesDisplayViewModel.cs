using System;
using System.Collections.Generic;
using System.Linq;
using EasySchool.Model;
using EasySchool.ViewModel.Utilities;

namespace EasySchool.ViewModel
{
    /// <summary>
    /// Viewing incoming messages
    /// </summary>
    public class MessagesDisplayViewModel : BaseViewModel, IScreenViewModel
    {
        #region Structures
        /// <summary>
        /// A sub-structure that represents information for a given lesson
        /// </summary>

        /// <summary>
        /// A sub-structure that represents information for a given message
        /// </summary>
        public class DisplayedMessage
        {
            public string SenderName { get; set; }
            public string RecipientName { get; set; }
            public DateTime MessageDateTime { get; set; }
            public string MessageDate 
            { 
                get
                {
                    return MessageDateTime.ToString("dd/MM/yyyy");
                }
            }
            public string Title { get; set; }
            public string Details { get; set; }
        }
        #endregion

        #region Properties / Commands
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }
        public string ScreenName { get { return "Show Messages"; } }

        // Business Logic Properties
        public List<DisplayedMessage> Messages { get; set; }
        #endregion

        #region Constructors
        public MessagesDisplayViewModel(Person connectedPerson)
        {
            HasRequiredPermissions = true;
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;

            // Reset first
            Messages = new List<DisplayedMessage>();
            var schoolMessages = new SchoolEntities().Messages;

            // Generate a list of all messages for the user, be they general or specific for them
            schoolMessages.Where(message => message.forEveryone).ToList()
                .ForEach(message => Messages.Add(ModelMessageToDisplayedMessage(message, MessageRecipientsTypes.Everyone)));
            schoolMessages.Where(message => message.recipientID == ConnectedPerson.personID).ToList()
                .ForEach(message => Messages.Add(ModelMessageToDisplayedMessage(message, MessageRecipientsTypes.Person)));
          
            // Use the user's permissions for the specifics

            if (ConnectedPerson.isStudent)
            {
                schoolMessages.Where(message => message.recipientClassID == ConnectedPerson.Student.classID)
                    .ToList().ForEach(message => Messages.Add(ModelMessageToDisplayedMessage(message, MessageRecipientsTypes.Class)));
                schoolMessages.Where(message => message.forAllStudents).ToList()
                    .ForEach(message => Messages.Add(ModelMessageToDisplayedMessage(message, MessageRecipientsTypes.Students)));
            }
            if (ConnectedPerson.isParent)
            {
                schoolMessages.AsEnumerable().Where(message => ConnectedPerson.ChildrenStudents.Any(childStudent => 
                                                                                      childStudent.classID == message.recipientClassID))
                    .ToList().ForEach(message => Messages.Add(ModelMessageToDisplayedMessage(message, MessageRecipientsTypes.Class)));
            }
            if (ConnectedPerson.isTeacher)
            {
                schoolMessages.Where(message => message.forAllTeachers).ToList()
                    .ForEach(message => Messages.Add(ModelMessageToDisplayedMessage(message, MessageRecipientsTypes.Teachers)));

                if (ConnectedPerson.Teacher.classID != null)
                {
                    schoolMessages.Where(message => ConnectedPerson.Teacher.classID == message.recipientClassID).ToList()
                        .ForEach(message => Messages.Add(ModelMessageToDisplayedMessage(message, MessageRecipientsTypes.Class)));
                }
            }
            if (ConnectedPerson.isSecretary || ConnectedPerson.isPrincipal)
            {
                schoolMessages.Where(message => message.forAllManagement).ToList()
                    .ForEach(message => Messages.Add(ModelMessageToDisplayedMessage(message, MessageRecipientsTypes.Management)));
            }

            // Order messages by date, from new to old
            Messages = Messages.OrderByDescending(message => message.MessageDateTime).ToList();
        }

        /// <summary>
        /// Converts the Database's "Message" object to a local "DisplayedMessage" object
        /// </summary>
        /// <param name="message">Message object</param>
        /// <param name="messageType">Who recieves the message</param>
        /// <returns> Equivelant "DisplayedMessage" version of "Message"</returns>
        private DisplayedMessage ModelMessageToDisplayedMessage(Message message, MessageRecipientsTypes messageType)
        {
            // Verify input
            if (message == null)
            {
                throw new ArgumentNullException("'Message' cannot be null");
            }

            DisplayedMessage displayedMessage = new DisplayedMessage();

            // Create a message based on the sender
            // Automatic messages are generated by the system as a result of other actions

            if (message.senderID != null)
            {
                displayedMessage.SenderName = message.SenderPerson.firstName + " " + message.SenderPerson.lastName;
            }
            else
            {
                displayedMessage.SenderName = "Automatic message";
            }

            displayedMessage.RecipientName = GetRecipientName(message, messageType);
            displayedMessage.Title = message.title;
            displayedMessage.MessageDateTime = message.date;
            displayedMessage.Details = message.data;

            return displayedMessage;
        }
        
        /// <summary>
        /// Assistant method to generate a specific name for the recipient after verifying their type
        /// </summary>
        /// <param name="message">Message source</param>
        /// <param name="messageType">Type of recipient</param>
        /// <returns>The ecipient's name</returns>
        private static string GetRecipientName(Message message, MessageRecipientsTypes messageType)
        {
            string recipientName;
            switch (messageType)
            {
                case MessageRecipientsTypes.Class:
                    recipientName = "Class " + message.Class.className;
                    break;
                case MessageRecipientsTypes.Everyone:
                    recipientName = "General message";
                    break;
                case MessageRecipientsTypes.Management:
                    recipientName = "Management";
                    break;
                case MessageRecipientsTypes.Students:
                    recipientName = "Students";
                    break;
                case MessageRecipientsTypes.Teachers:
                    recipientName = "Teachers";
                    break;
                case MessageRecipientsTypes.Person:
                default:
                    recipientName = message.ReceiverPerson.firstName + " " + message.ReceiverPerson.lastName;
                    break;
            }

            return recipientName;
        }
        #endregion
    }
}
