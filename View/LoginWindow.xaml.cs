using System.Security;
using System.Windows;
using EasySchool.View.Utilities;

namespace EasySchool.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window, IHavePassword, IClosableScreen
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The secure password a given login page
        /// </summary>
        public SecureString SecurePassword => PasswordText.SecurePassword;

        /// <summary>
        /// Confirm secure password for this login page - doesn't actually exists so resending password
        /// </summary>
        public SecureString ConfirmationSecurePassword => PasswordText.SecurePassword;

        /// <summary>
        /// Close this window
        /// </summary>
        public void CloseScreen()
        {
            base.Close();
        }
    }
}
