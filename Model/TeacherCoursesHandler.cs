using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySchool.Model
{
    /// <summary>
    /// Teacher courses information assistant method
    /// </summary>
    public static class TeacherCoursesHandler
    {
        /// <summary>
        /// Configure an <ID, NAME> dictionary for all courses that a given teacher can teach.
        /// </summary>
        /// <param name="teacher">Selected teacher/param>
        /// <param name="searchForHomeroomCourses">Is he a homeroom teacher</param>
        /// <returns>A dictionary made of all of the courses a given teacher can teach</returns>
        public static Dictionary<int, string> GetTeacherCoursesNames(Teacher teacher, bool searchForHomeroomCourses)
        {
            Dictionary<int, string> courseList = new Dictionary<int, string>();

            if (teacher != null)
            {
                // check all possible courses
                if (teacher.firstCourseID != null)
                {
                    courseList.Add(teacher.firstCourseID.Value, teacher.FirstCourse.courseName);
                }
                if (teacher.secondCourseID != null)
                {
                    courseList.Add(teacher.secondCourseID.Value, teacher.SecondCourse.courseName);
                }
                if (teacher.thirdCourseID != null)
                {
                    courseList.Add(teacher.thirdCourseID.Value, teacher.ThirdCourse.courseName);
                }
                if (teacher.fourthCourseID != null)
                {
                    courseList.Add(teacher.fourthCourseID.Value, teacher.FourthCourse.courseName);
                }

                // Homeroom teachers can also teach their homeroom class any homeroom course
                if (searchForHomeroomCourses && teacher.classID != null)
                {
                    SchoolEntities schoolData = new SchoolEntities();
                    foreach (Course course in schoolData.Courses.Where(course => course.isHomeroomTeacherOnly))
                    {
                        // Make sure the course wasn't added already
                        if (!courseList.ContainsKey(course.courseID))
                        {
                            courseList.Add(course.courseID, course.courseName);
                        }
                    }
                }
            }

            return courseList;
        }

    }
}
