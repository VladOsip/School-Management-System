using EasySchool.Model;
using System.Linq;
using System.Collections.Generic;
using EasySchool.ViewModel.Utilities;
using System;
using System.Collections.ObjectModel;

namespace EasySchool.ViewModel
{
    /// <summary>
    /// The school's contacts page - display contact information for the management and teachers in the school
    /// </summary>
    public class ContactsInfoViewModel : BaseViewModel, IScreenViewModel
    {
        #region Structs
        /// <summary>
        /// A sub-structure that shows relevant information about a teacher
        /// </summary>
        public struct TeacherInfo
        {
            public string Name { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string CoursesNames { get; set; }
        }
        /// <summary>
        /// A sub-structure that shows relevant information about secretary
        /// </summary>
        public struct SecretaryInfo
        {
            public string Name { get; set; }
            public string Phone { get; set; }
        }
        #endregion

        #region Properties
        // Base Properties
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; private set; }
        public string ScreenName { get { return "Contact"; } }

        // Business Logic Properties
        public string PrincipalName { get; private set; }
        public string PrincipalEmail { get; private set; }
        public ObservableCollection<SecretaryInfo> Secretaries { get; set; }
        public ObservableCollection<TeacherInfo> Teachers { get; set; }
        #endregion

        #region The constructor
        public ContactsInfoViewModel(Person connectedPerson)
        {
            HasRequiredPermissions = true;
            Secretaries = new ObservableCollection<SecretaryInfo>();
            Teachers = new ObservableCollection<TeacherInfo>();
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;
            SchoolEntities contactData = new SchoolEntities();

            // Set the school's basic information
            var schoolInfo = contactData.SchoolInfo;

            // Get the principal's information
            var principal = contactData.Persons.FirstOrDefault(person => person.isPrincipal && !person.User.isDisabled);
            if (principal != null)
            {
                PrincipalName = principal.firstName + " " + principal.lastName;
                PrincipalEmail = principal.email;
            }

            // Get the secretaries information
            Secretaries.Clear();
            contactData.Persons.Where(person => person.isSecretary && !person.User.isDisabled).ToList()
                .ForEach(person => Secretaries.Add(new SecretaryInfo() { Name = person.firstName + " " + person.lastName, Phone = person.phoneNumber }));

            // Get the teachers information
            Teachers.Clear();
            contactData.Persons.Where(person => person.isTeacher && !person.User.isDisabled).ToList()
               .ForEach(person => Teachers.Add(new TeacherInfo()
               {
                   Name = person.firstName + " " + person.lastName,
                   CoursesNames = GetTeacherCourseNames(person.Teacher),
                   Email = person.email,
                   Phone = person.phoneNumber
               }));
        }

        /// <summary>
        /// Generate a list of all of the courses that a teacher teaches
        /// </summary>
        /// <param name="teacher">The teacher</param>
        /// <returns>A string containing the names of all of the courses that the teacher teaches</returns>
        private string GetTeacherCourseNames(Teacher teacher)
        {
            string courseNames = string.Empty;

            foreach (string teacherCourseName in TeacherCoursesHandler.GetTeacherCoursesNames(teacher, false).Values)
            {
                courseNames += teacherCourseName + ", ";
            }

            // Remove the last ', ' since it's the end of the list
            courseNames = courseNames.Substring(0, courseNames.Length - 2);

            return courseNames;
        }
        #endregion
    }
}