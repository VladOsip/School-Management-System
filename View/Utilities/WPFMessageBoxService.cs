using System.Windows;
using EasySchool.ViewModel.Utilities;

namespace EasySchool.View.Utilities
{
    /// <summary>
    /// A message box.
    /// </summary>
    public class WPFMessageBoxService : IMessageBoxService
    {
       // default message type is INFORMATION
        public bool ShowMessage(string text, string caption, MessageType messageType, MessagePurpose purpose = MessagePurpose.INFORMATION)
        {
            // Match an image to a message type
            MessageBoxImage messagePurpose;
            switch (purpose)
            {
                case MessagePurpose.ERROR:
                    messagePurpose = MessageBoxImage.Error;
                    break;
                case MessagePurpose.INFORMATION:
                default:
                    messagePurpose = MessageBoxImage.Information;
                    break;
                case MessagePurpose.DEBUG:
                    messagePurpose = MessageBoxImage.Asterisk;
                    break;
            }

            // Match an MessageBoxButton to a message type
            MessageBoxButton messageButtons;
            switch (messageType)
            {
                case MessageType.YES_NO_MESSAGE:
                    messageButtons = MessageBoxButton.YesNo;
                    break;
                case MessageType.OK_MESSAGE:
                default:
                    messageButtons = MessageBoxButton.OK;
                    break;
                case MessageType.ACCEPT_CANCEL_MESSAGE:
                    messageButtons = MessageBoxButton.OKCancel;
                    break;
            }

            // Display the message box and return the user's response.
            MessageBoxResult result = MessageBox.Show(text, caption, messageButtons, messagePurpose, MessageBoxResult.OK , MessageBoxOptions.RtlReading);
            if (result == MessageBoxResult.Yes || result == MessageBoxResult.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}