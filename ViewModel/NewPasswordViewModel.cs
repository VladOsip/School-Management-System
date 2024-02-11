using System;
using System.Security;
using System.Windows.Input;
using EasySchool.ViewModel.Utilities;
using EasySchool.View.Utilities;
using EasySchool.Model;

namespace EasySchool.ViewModel
{
    /// <summary>
    /// Sets the user's password
    /// </summary>
    public class NewPasswordViewModel : BaseViewModel
    {
        #region Fields
        private ICommand _setPasswordCommand;
        private bool _isConfirmationPasswordInvalid;
        private bool _isPasswordInvalid;

        private SchoolEntities _context;
        private User _user;
        #endregion

        #region Properties

        // Commands
        public ICommand SetPasswordCommand
        {
            get
            {
                if (_setPasswordCommand == null)
                {
                    _setPasswordCommand = new RelayCommand(
                        p => SetPassword(p),
                        p => p is IHavePassword);
                }

                return _setPasswordCommand;
            }
        }

        // Business logic
        public bool IsPasswordInvalid
        {
            get
            {
                return _isPasswordInvalid;
            }
            set
            {
                _isPasswordInvalid = value;
                OnPropertyChanged("IsPasswordInvalid");
            }
        }
        
        public bool IsConfirmationPasswordInvalid
        {
            get
            {
                return _isConfirmationPasswordInvalid;
            }
            set
            {
                _isConfirmationPasswordInvalid = value;
                OnPropertyChanged("IsConfirmationPasswordInvalid");
            }
        }
        #endregion

        #region Constructor
        public NewPasswordViewModel(SchoolEntities context, User connectedUser)
        {
            _context = context;
            _user = connectedUser;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Set user password
        /// </summary>
        /// <param name="parameter">The source window of the View that implement IHavePassword and IDialogWindow</param>
        private void SetPassword(object parameter)
        {
            SecureString password = (parameter as IHavePassword).SecurePassword;
            SecureString confirmationPassword = (parameter as IHavePassword).ConfirmationSecurePassword;

            // Reset error flags
            IsPasswordInvalid = false;
            IsConfirmationPasswordInvalid = false;

            // Verify password validity
            if (password.Length < Globals.MINIMUM_PASSWORD_LENGTH || password.Length > Globals.MAXIMUM_PASSWORD_LENGTH)
            {
                IsPasswordInvalid = true;
            }
            // Match confirmation password with the original password
            else if (password.Unsecure() != confirmationPassword.Unsecure())
            {
                IsConfirmationPasswordInvalid = true;
            }
            else
            {
                // Note: The password is saved in plain-text for convenience. The product version of this code would have it be secured
                _user.password = password.Unsecure();
                _user.hasToChangePassword = false;
                _context.SaveChanges();
                (parameter as IDialogWindow).CloseDialogWindow(true);
            }
        }
        #endregion
    }
}
