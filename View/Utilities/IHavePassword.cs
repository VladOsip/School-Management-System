using System;
using System.Security;
using System.Runtime.InteropServices;

namespace EasySchool.View.Utilities
{
    /// <summary>
    /// Class interface for a secure password
    /// </summary>
    public interface IHavePassword
    {
        /// <summary>
        /// Said secure password
        /// </summary>
        SecureString SecurePassword { get; }

        /// <summary>
        /// A password confirmation option
        /// </summary>
        SecureString ConfirmationSecurePassword { get; }
    }

    /// <summary>
    /// Assistant method for <see cref="SecureString"/>
    /// </summary>
    public static class SecureStringHelper
    {
        /// <summary>
        /// Unsecures a <see cref="SecureString"/> instance into plain text
        /// </summary>
        /// <param name="secureString">Secure string</param>
        /// <returns></returns>
        public static string Unsecure(this SecureString secureString)
        {
            // Check if our secure string exists
            if (secureString == null)
                return string.Empty;

            // Memory pointer for the address of the unsecure string
            var unsecuredString = IntPtr.Zero;

            try
            {
                // Unsecures the password
                unsecuredString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(unsecuredString);
            }
            finally
            {
                // Clean up any memory allocation
                Marshal.ZeroFreeGlobalAllocUnicode(unsecuredString);
            }
        }
    }
}
