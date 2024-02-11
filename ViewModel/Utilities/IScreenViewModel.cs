using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySchool.Model;

namespace EasySchool.ViewModel.Utilities
{
    /// <summary>
    /// Required interface for the actual screens of the application
    /// </summary>
    public interface IScreenViewModel
    {
        #region Properties
        string ScreenName { get; }
        Person ConnectedPerson { get; }
        bool HasRequiredPermissions { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Initialize the ViewModel properties
        /// </summary>
        /// <param name="connectedPerson">Info about the connected user</param>
        void Initialize(Person connectedPerson);
        #endregion
    }
}
