using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFGUETSchedule
{
    class SelectedCourse
    {
        public string CourseIndex { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Teacher { get; set; }
        public string CourseType { get; set; }
        public string ExamTime { get; set; }
        public string IsCharged { get; set; }

        public SelectedCourse(string courseIndex,string courseCode,string courseName,
                                string teacher,string courseType, string examTime,string isCharged)
        { 
            CourseIndex = courseIndex;
            CourseCode = courseCode;
            CourseName = courseName;
            Teacher = teacher;
            CourseType = courseType;
            ExamTime = examTime;
            IsCharged = isCharged;
        }
    }
}
