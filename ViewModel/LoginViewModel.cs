using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Input;
using EasySchool.Model;
using EasySchool.View;
using EasySchool.View.Utilities;
using EasySchool.ViewModel.Utilities;

namespace EasySchool.ViewModel
{
    /// <summary>
    /// ViewModel for the login process logic.
    /// </summary>
    public class LoginViewModel : BaseViewModel
    {
        #region Fields
        private SchoolEntities _mySchoolModel;
        private ICommand _loginCommand;
        #endregion

        #region Properties
        public string Username { get; set; }
        public ICommand LoginCommand 
        {
            get
            {
                if (_loginCommand == null)
                {
                    _loginCommand = new RelayCommand(
                        p => LoginAttempt(p),
                        p => p is IHavePassword && p is IClosableScreen);
                }

                return _loginCommand;
            }
        }
        #endregion

        #region The constructor
        public LoginViewModel(IMessageBoxService messageBoxService)
            : base(messageBoxService)
        {
            _mySchoolModel = new SchoolEntities();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Use the input username and password to log in
        /// </summary>
        /// <param name="parameter">IHavePassword, ICloseableScreen object that contains a SecureString input password</param>
        private void LoginAttempt(object parameter)
        {
            SecureString password = (parameter as IHavePassword).SecurePassword;
            var validInput = IsLoginValid(Username, password);

            IClosableScreen thisScreen = (parameter as IClosableScreen);
            
            if (validInput.Valid)
            {
                // Unsecure the password to compare it to the DB
                // For the sake of simplicity, the password are not hashed and compared
                // DB holds passwords in plain text for the same reason.
                var unsecuredPassword = password.Unsecure();

                // Search for the user in the Database
                User connectedAccount = _mySchoolModel.Users.SingleOrDefault(user => user.username == Username && user.password == unsecuredPassword);

                // If the user is found, connect and open the application.
                if (connectedAccount != null && !connectedAccount.isDisabled)
                {
                    // If needed, make the user change their password
                    if (connectedAccount.hasToChangePassword)
                    {
                        NewPasswordWindow newPasswordDialog = new NewPasswordWindow();
                        NewPasswordViewModel newPasswordDialogVM = new NewPasswordViewModel(_mySchoolModel, connectedAccount);
                        newPasswordDialog.DataContext = newPasswordDialogVM;
                        newPasswordDialog.ShowDialog();
                    }

                    // Launch the main window of the application using the "connecteduser" account
                    GoToMainApp(thisScreen, connectedAccount.Person.Single());
                }
                // Report incorrect user credentials error.
                else
                {
                    _messageBoxService.ShowMessage("Username/Password are incorrect", "Login Failed!", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
                }
            }
            // Generate a report in the case of a disabled user
            else
            {
                _messageBoxService.ShowMessage(validInput.ErrorReport, "Login Failed!", MessageType.OK_MESSAGE, MessagePurpose.ERROR);
            }
        }

        /// <summary>
        /// Used to launch the main app
        /// </summary>
        private void GoToMainApp(IClosableScreen thisLoginScreen, Person connectedUser)
        {
            ApplicationMainWindow appMainWindow = new ApplicationMainWindow();
            ApplicationViewModel context = new ApplicationViewModel(connectedUser, _messageBoxService);
            appMainWindow.DataContext = context;
            appMainWindow.Show();

            // Close this Login Window 
            thisLoginScreen.CloseScreen();
        }

        /// <summary>
        /// Assistant method to check the validity of the username and password.
        /// </summary>
        /// <param name="username">The Username to check</param>
        /// <param name="password">Secured version of the Password to check</param>
        /// <returns>The validity tests result</returns>
        private ValidityResult IsLoginValid(string username, SecureString password)
        {
            ValidityResult result = new ValidityResult();
            result.Valid = true;

            // Did the user write a username
            if (username == null || username.Length == 0)
            {
                result.ErrorReport = "Please write a username";
                result.Valid = false;
            }
            // Is the username valid
            else if (username.Length < Globals.MINIMUM_USERNAME_LENGTH || username.Length > Globals.MAXIMUM_USERNAME_LENGTH)
            {
                result.ErrorReport = string.Format("Username length needs to be between {0} and {1} Characters",
                                                    Globals.MINIMUM_USERNAME_LENGTH, Globals.MAXIMUM_USERNAME_LENGTH);
                result.Valid = false;
            }
            // Did the user write a password
            else if (password == null || password.Length == 0)
            {
                result.ErrorReport = "Please write a password";
                result.Valid = false;
            }
            // Is the password valid
            else if (password.Length < Globals.MINIMUM_PASSWORD_LENGTH || password.Length > Globals.MAXIMUM_PASSWORD_LENGTH)
            {
                result.ErrorReport = string.Format("Password length needs to be between {0} and {1} Characters",
                                                    Globals.MINIMUM_PASSWORD_LENGTH, Globals.MAXIMUM_PASSWORD_LENGTH);
                result.Valid = false;
            }

            return result;
        }
        #endregion
    }
}
