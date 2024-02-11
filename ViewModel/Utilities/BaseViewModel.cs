using System;
using System.Diagnostics;
using System.ComponentModel;

namespace EasySchool.ViewModel.Utilities
{
    /// <summary>
    /// A base view model that fires Property Changed events as needed
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {

        #region Fields
        protected IMessageBoxService _messageBoxService;
        #endregion

        #region Constructors
        public BaseViewModel(IMessageBoxService messageBoxService = null)
        {
            _messageBoxService = messageBoxService;
        }
        #endregion

        #region Debugging Tools

        /// <summary>
        /// Checks for a public property with a given name to make sure it exists
        /// To be removed upon release
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public virtual void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches an existing public instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// Is false by default, but it may be overwritten by unit tests to return true via getters
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Raises the PropertyChange event for the property specified
        /// </summary>
        /// <param name="propertyName">The Propety Name to update (case-sensitive)</param>
        public virtual void RaisePropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion 
    }
}
