namespace EasySchool.ViewModel.Utilities
{
    /// <summary>
    /// Possible message box types.
    /// </summary>
    public enum MessageType
    {
        OK_MESSAGE,
        YES_NO_MESSAGE,
        ACCEPT_CANCEL_MESSAGE
    }
    
    /// <summary>
    /// Intended Purpose of the message box
    /// </summary>
    public enum MessagePurpose
    {
        INFORMATION,
        ERROR,
        DEBUG,
    }

    /// <summary>
    /// Support message boxes.
    /// </summary>
    public interface IMessageBoxService
    {
        /// <summary>
        /// Show a message box
        /// </summary>
        /// <param name="text">Message box text</param>
        /// <param name="caption">Message box title</param>
        /// <param name="messageType">Message box possible actions</param>
        /// <param name="purpose">Message box purpose</param>
        /// <returns>User action</returns>
        bool ShowMessage(string text, string caption, MessageType messageType, MessagePurpose purpose = MessagePurpose.INFORMATION);
    }
}