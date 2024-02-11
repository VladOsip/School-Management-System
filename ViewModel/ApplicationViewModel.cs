using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using EasySchool.Model;
using EasySchool.View;
using EasySchool.View.Utilities;
using EasySchool.ViewModel.Utilities;

namespace EasySchool.ViewModel
{
    /// <summary>
    /// The application main page's view model. 
    /// Manages navigation between different application screens after login.
    /// </summary>
    public class ApplicationViewModel : BaseViewModel
    {
        #region Fields
        private int _connectedPersonID;

        private ICommand _changeScreenCommand;
        private ICommand _updateScreensCommand;
        private ICommand _logoutCommand;

        private List<IScreenViewModel> _screenViewModels;
        private IScreenViewModel _currentScreenViewModel;
        #endregion

        #region Properties / Commands
        public ICommand ChangeScreenCommand
        {
            get
            {
                if (_changeScreenCommand == null)
                {
                    _changeScreenCommand = new RelayCommand(
                        p => ChangeViewModel((IScreenViewModel)p),
                        p => p is IScreenViewModel);
                }

                return _changeScreenCommand;
            }
        }

        public ICommand RefreshScreensCommand
        {
            get
            {
                if (_updateScreensCommand == null)
                {
                    _updateScreensCommand = new RelayCommand(p => RefreshViewModels());
                }

                return _updateScreensCommand;
            }
        }

        public ICommand LogoutCommand
        {
            get
            {
                if (_logoutCommand == null)
                {
                    _logoutCommand = new RelayCommand(
                        p => LogoutUser(p as IClosableScreen),
                        p => p is IClosableScreen);
                }

                return _logoutCommand;
            }
        }

        public List<IScreenViewModel> ScreensViewModels
        {
            get
            {
                if (_screenViewModels == null)
                    _screenViewModels = new List<IScreenViewModel>();

                return _screenViewModels;
            }
        }

        public IScreenViewModel CurrentScreenViewModel
        {
            get
            {
                return _currentScreenViewModel;
            }
            set
            {
                if (_currentScreenViewModel != value)
                {
                    _currentScreenViewModel = value;
                    OnPropertyChanged("CurrentScreenViewModel");
                }
            }
        }
        #endregion

        #region The constructor
        public ApplicationViewModel(Person connectedPerson, IMessageBoxService messageBoxService)
            : base(messageBoxService)
        {
            _connectedPersonID = connectedPerson.personID;

            // Create a list of all possible screens
            List<IScreenViewModel> allScreens = new List<IScreenViewModel>();

            // These screens are ones that are loaded with others
            allScreens.Add(new SchoolInfoViewModel(connectedPerson));
            allScreens.Add(new ContactsInfoViewModel(connectedPerson));
            allScreens.Add(new WeeklyScheduleViewModel(connectedPerson));
            allScreens.Add(new CalenderViewModel(connectedPerson));
            allScreens.Add(new StudentGradesViewModel(connectedPerson, messageBoxService));
            allScreens.Add(new MessagesDisplayViewModel(connectedPerson));

            // These are independent screens
            allScreens.Add(new CreateMessageViewModel(connectedPerson, RefreshScreensCommand, messageBoxService));
            allScreens.Add(new LessonSummaryViewModel(connectedPerson, RefreshScreensCommand, messageBoxService));
            allScreens.Add(new GradesReportViewModel(connectedPerson, RefreshScreensCommand, messageBoxService));
            allScreens.Add(new LessonManagementViewModel(connectedPerson, RefreshScreensCommand, messageBoxService));
            allScreens.Add(new CourseManagementViewModel(connectedPerson, RefreshScreensCommand, messageBoxService));
            allScreens.Add(new RoomManagementViewModel(connectedPerson, RefreshScreensCommand, messageBoxService));
            allScreens.Add(new ClassManagementViewModel(connectedPerson, RefreshScreensCommand, messageBoxService));
            allScreens.Add(new EventManagementViewModel(connectedPerson, RefreshScreensCommand, messageBoxService));
            allScreens.Add(new UserCreationViewModel(connectedPerson, RefreshScreensCommand, messageBoxService));
            allScreens.Add(new UserUpdateViewModel(connectedPerson, RefreshScreensCommand, messageBoxService));
            allScreens.Add(new SchoolManagementViewModel(connectedPerson, RefreshScreensCommand, messageBoxService));

            // Use only the screens that are relevent to the current user
            foreach (IScreenViewModel screen in allScreens)
            {
                if (screen.HasRequiredPermissions)
                {
                    screen.Initialize(connectedPerson);
                    ScreensViewModels.Add(screen);
                }
            }

            // Set the starting screen
            CurrentScreenViewModel = ScreensViewModels[0];
        }
        #endregion

        #region Methods
        /// <summary>
        /// Update the current view model (logic) to a given 'ViewModel'
        /// </summary>
        /// <param name="viewModel">The new ViewModel</param>
        private void ChangeViewModel(IScreenViewModel viewModel)
        {
            // Make sure the collection of View Models contains this viewModel
            if (!ScreensViewModels.Contains(viewModel))
            {
                ScreensViewModels.Add(viewModel);
            }

            // Use the selected view model
            CurrentScreenViewModel = ScreensViewModels.FirstOrDefault(vm => vm == viewModel);
        }

        /// <summary>
        /// React to a data update by refreshing
        /// </summary>
        private void RefreshViewModels()
        {
            // reload the screens
            SchoolEntities schoolData = new SchoolEntities();
            Person connectedPerson = schoolData.Persons.Find(_connectedPersonID);

            foreach (IScreenViewModel viewModel in ScreensViewModels)
            {
                viewModel.Initialize(connectedPerson);
            }
        }

        /// <summary>
        /// Closes the application and returns to the login menu
        /// </summary>
        /// <param name="thisApplicationScreen">ICloseableScreen object to close the application</param>
        private void LogoutUser(IClosableScreen thisApplicationScreen)
        {
            // Launch the login window
            LoginWindow appLoginWindow = new LoginWindow();
            LoginViewModel context = new LoginViewModel(_messageBoxService);
            appLoginWindow.DataContext = context;
            appLoginWindow.Show();

            // Close this Window 
            thisApplicationScreen.CloseScreen();
        }
        #endregion
    }
}
