﻿using EasySchool.Model;
using System.Linq;
using System.Collections.Generic;
using EasySchool.ViewModel.Utilities;
using System;

namespace EasySchool.ViewModel
{
    /// <summary>
    /// The school's about page - display lots of different data about the school
    /// </summary>
    public class SchoolInfoViewModel : BaseViewModel, IScreenViewModel
    {
        #region Properties
        // Base Properties
        public string ScreenName { get { return "About"; } }
        public Person ConnectedPerson { get; private set; }
        public bool HasRequiredPermissions { get; private set; }

        // Business Logic Properties
        public string SchoolName { get; private set; }
        public string SchoolDescription { get; private set; }
        public string SchoolImage { get; private set; }
        public int NumberOfStudents { get; private set; }
        public int NumberOfClasses { get; private set; }
        public float ClassAverageSize { get; private set; }
        public double ScoreAverage { get; private set; }
        #endregion

        #region Constructors
        public SchoolInfoViewModel(Person connectedPerson)
        {
            HasRequiredPermissions = true;
        }
        #endregion

        #region Methods
        public void Initialize(Person connectedPerson)
        {
            ConnectedPerson = connectedPerson;
            SchoolEntities dbContext = new SchoolEntities();

            // School name and description
            var schoolInfo = dbContext.SchoolInfo;
            SchoolName = schoolInfo.Find("schoolName").value;
            SchoolDescription = schoolInfo.Find("schoolDescription").value;

            // Full path to the logo of the school
            string imageSrc = schoolInfo.Find("schoolImage").value;
            SchoolImage = imageSrc.Contains(":\\") ? imageSrc : "/EasySchool;component/Images/" + imageSrc;

            // Generate basic statistics
            var relevantStudentsQuery = dbContext.Students.Where(student => !student.Person.User.isDisabled);
            NumberOfClasses = dbContext.Classes.Count();
            NumberOfStudents = relevantStudentsQuery.Count();
            ClassAverageSize = NumberOfStudents / NumberOfClasses;
            ScoreAverage = CalcAverageGrade(relevantStudentsQuery.ToList());
        }

        /// <summary>
        /// Calculate the average score of the students across the school
        /// </summary>
        /// <param name="students"></param>
        /// <returns></returns>
        private double CalcAverageGrade(List<Student> students)
        {
            // Calculate the average score of each student, then the sum of averages
            double scoresSum = 0;
            int releventStudentsNumber = 0;
            foreach (Student student in students)
            {
                if (student.Grades != null && student.Grades.Count > 0)
                {
                    scoresSum += Math.Round(student.Grades.Average(x => x.score), 1);
                    releventStudentsNumber++;
                }
            }
            return Math.Round(scoresSum / releventStudentsNumber, 1);
        }

        #endregion
    }
}