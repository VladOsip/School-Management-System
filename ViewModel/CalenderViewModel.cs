using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using EasySchool.Model;
using EasySchool.ViewModel.Utilities;

namespace EasySchool.ViewModel
{
    /// <summary>
    /// Shows the user a calender with all the events they have access to
    /// </summary>
    public class CalenderViewModel : BaseViewModel, IScreenViewModel
    {
        #region Fields
        private SchoolEntities _schoolData;
        private Epical.Calendar.Calendar _calender;

        private ICommand _updateCalenderCommand;
        private ICommand _updateSelectedDayCommand;

        private List<string> _months;
        private string _selectedMonth;

        private Event _selectedEvent;
        private List<Event> _selectedDayEvents;
        #endregion

        #region Properties / Commands
        // Base Properties
        public string ScreenName { get { return "Calendar"; } }
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; }

        /// <summary>
        /// Update the calender to show the current selected month's events
        /// </summary>
        public ICommand UpdateCalenderCommand
        {
            get
            {
                if (_updateCalenderCommand == null)
                {
                    _updateCalenderCommand = new RelayCommand(p => UpdateCalender((Epical.Calendar.Calendar)p),
                                                              p => p is Epical.Calendar.Calendar);
                }
                return _updateCalenderCommand;
            }
        }
        /// <summary>
        /// Display the events of the selected day
        /// </summary>
        public ICommand UpdateSelectedDayCommand
        {
            get
            {
                if (_updateSelectedDayCommand == null)
                {
                    _updateSelectedDayCommand = new RelayCommand(p => GetSelectedDayEvents());
                }
                return _updateSelectedDayCommand;
            }
        }

        // Business Logic Properties
        public List<string> Months
        {
            get
            {
                if (_months == null)
                {
                    _months = new List<string>();
                }

                return _months;
            }
        }

        public string SelectedMonth
        {
            get
            {
                return _selectedMonth;
            }
            set
            {
                if (_selectedMonth != value && Months.Contains(value))
                {
                    _selectedMonth = value;
                    OnPropertyChanged("SelectedMonth");
                }
            }
        }

        public List<Event> SelectedDayEvents
        { 
            get
            {
                return _selectedDayEvents;
            }
            set
            {
                if (_selectedDayEvents != value)
                {
                    _selectedDayEvents = value;
                    OnPropertyChanged("SelectedDayEvents");
                }
            }
        }
        public Event SelectedEvent
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
                    OnPropertyChanged("SelectedEvent");
                }
            }
        }
        #endregion

        #region The constructor
        public CalenderViewModel(Person connectedPerson)
        {
            HasRequiredPermissions = true;

            if (HasRequiredPermissions)
            {
                _schoolData = new SchoolEntities();
                _months = new List<string> 
                    { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
                _selectedDayEvents = new List<Event>();
            }
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;

            // Use the current month as the default.
            // Note that DateTime.Now.Month indexes from 1 to 12 and not 0-11 so we substract
            SelectedMonth = _months[DateTime.Now.Month - 1];
        }

        /// <summary>
        /// Generate a list of events for a given user between two given dates
        /// </summary>
        /// <param name="startingDate">The low-end search date</param>
        /// <param name="endDate">The high-end search date</param>
        /// <returns></returns>
        private List<Event> GetRelevantEvents(DateTime startingDate, DateTime endingDate)
        {
            // Check that the dates align properly, otherwise throw an exception.
            if (startingDate > endingDate)
            {
                throw new ArgumentException("Invalid dates received when searching for events");
            }

            // Create the minimum query to only get events that are within the specified dates without time.
            // Given that LINQ can't directly use the Date property of a Datetime we need to use DBFunctions to compensate.
            var startDate = startingDate.Date;
            var endDate = endingDate.Date;
            var eventsQuery =
                _schoolData.Events.Where(schoolEvent => (DbFunctions.TruncateTime(schoolEvent.eventDate) <= endDate) &&
                                                          DbFunctions.TruncateTime(schoolEvent.eventDate) >= startDate);

            // Use a Hashset to create events that are for the entire school (reminder: if there is no specific recipient or class it counts)
            // Also, events that were created by the current user or are directed at them.
            // The HashSet verifies that all of the events are unique.
            HashSet<Event> userEvents = eventsQuery.Where(schoolEvent =>
                                                                (schoolEvent.recipientClassID == null && schoolEvent.recipientID == null) ||
                                                                schoolEvent.submitterID == ConnectedPerson.personID ||
                                                                schoolEvent.recipientID == ConnectedPerson.personID)
                                                                .ToHashSet();

            // Generate events for a given user based on their permissions.

            if (ConnectedPerson.isSecretary || ConnectedPerson.isPrincipal)
            {
                // Show every school event
                userEvents.UnionWith(eventsQuery.ToHashSet());
            }

            //Homeroom teacher
            else if (ConnectedPerson.isTeacher && ConnectedPerson.Teacher.classID != null)
            {
                // Show an homeroom teacher any event of his own class
                userEvents.UnionWith(eventsQuery.AsEnumerable().Where(schoolEvent =>
                                                        schoolEvent.recipientClassID == ConnectedPerson.Teacher.classID)
                                                        .ToHashSet());
            }

            else if (ConnectedPerson.isParent)
            {
                userEvents.UnionWith(eventsQuery.AsEnumerable().Where(schoolEvent =>
                                                        ConnectedPerson.ChildrenStudents.Any(childStudent =>
                                                                                            childStudent.classID == schoolEvent.recipientClassID))
                                                        .ToHashSet());
            }

            else if (ConnectedPerson.isStudent)
            {

                userEvents.UnionWith(eventsQuery.AsEnumerable().Where(schoolEvent =>
                                                       schoolEvent.recipientClassID == ConnectedPerson.Student.classID)
                                                       .ToHashSet());
            }

            return userEvents.ToList();
        }

        /// <summary>
        /// Update the calender to show data for the selected month
        /// </summary>
        /// <param name="calender">The visual calender</param>
        private void UpdateCalender(Epical.Calendar.Calendar calender)
        {
            DateTime wantedDate = new DateTime(DateTime.Now.Year, Months.IndexOf(SelectedMonth) + 1, 1);
            calender.BuildCalendar(wantedDate);

            // Generate a list of events for the current user
            List<Event> userEvents = GetRelevantEvents(calender.Days[0].DayDate, calender.Days.Last().DayDate);

            // Iterate through the displayed days and add their events 
            foreach (Event currentEvent in userEvents)
            {
                // Generate a number for the current day of the event, will act as an offset
                int addedDays = (currentEvent.eventDate - calender.Days[0].DayDate).Days;

                // Should there be multiple events in the same day, space them out.
                if (calender.Days[addedDays].Notes != string.Empty && calender.Days[addedDays].Notes != null)
                {
                    calender.Days[addedDays].Notes += ", ";
                }
                calender.Days[addedDays].Notes += currentEvent.name;
            }

            // Save the calender to use for determining selected day
            _calender = calender;
        }

        /// <summary>
        /// Get information for the current observed/selected day
        /// </summary>
        private void GetSelectedDayEvents()
        {
            var selectedDay = _calender.SelectedDay;

            if (selectedDay != null)
            {
                SelectedDayEvents = GetRelevantEvents(selectedDay.DayDate, selectedDay.DayDate);
                SelectedEvent = SelectedDayEvents.FirstOrDefault();
            }
            else
            {
                SelectedDayEvents = new List<Event>();
                SelectedEvent = null;
            }
        }
        #endregion
    }
}
