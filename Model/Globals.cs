using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySchool.Model
{
    /// <summary>
    /// Contains global static variables and conversion/organisation methods
    /// </summary>
    public static class Globals
    {
        #region Constants
        public const int MINIMUM_USERNAME_LENGTH = 3;
        public const int MAXIMUM_USERNAME_LENGTH = 16;
        public const int MINIMUM_PASSWORD_LENGTH = 4;
        public const int MAXIMUM_PASSWORD_LENGTH = 16;

        public const string USER_TYPE_PRINCIPAL = "Principals";
        public const string USER_TYPE_SECRETARIES = "Secrateries";
        public const string USER_TYPE_TEACHERS = "Teachers";
        public const string USER_TYPE_PARENTS = "Parents";
        public const string USER_TYPE_STUDENT = "Students";

        public readonly static string[] DAY_NAMES = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
        public readonly static string[] HOUR_NAMES = { "First", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh", "Eighth", "Ninth", "Tenth" };

        public const int GRADE_NO_VALUE = 0;
        public const int GRADE_MIN_VALUE = 1;
        public const int GRADE_MAX_VALUE = 100;
        #endregion

        #region Assistant Methods

        /// <summary>
        /// Changes the format of school days from int to DAY_NAMES
        /// </summary>
        /// <param name="dayNumber">The school day number</param>
        /// <returns>The corresponding day name as a string</returns>
        public static string ConvertDayNumberToName(Nullable<int> dayNumber)
        {
            string dayName = "None"; ;

            // Check if dayNumber is in valid range
            if (dayNumber != null && dayNumber > 0 && dayNumber < DAY_NAMES.Count())
            {
                dayName = DAY_NAMES[dayNumber.Value];
            }

            return dayName;
        }

        /// <summary>
        /// Changes the format of school hours from int to HOUR_Name
        /// </summary>
        /// <param name="hourNumber">School hour number</param>
        /// <returns>The converted string number</returns>
        public static string ConvertHourNumberToName(Nullable<int> hourNumber)
        {
            string hourName = "None"; ;
            
            // Check if hourNumber is in valid range
            if (hourNumber != null && hourNumber > 0 && hourNumber < HOUR_NAMES.Count())
            {
                hourName = HOUR_NAMES[hourNumber.Value];
            }

            return hourName;
        }
        #endregion
    }
}
