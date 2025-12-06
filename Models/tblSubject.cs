using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.Models
{
    [Table("tblSubject")]
    public class tblSubject
    {
        [Key]
        public int SubjectID { get; set; }
        public string? SubjectName { get; set; }
        public int NumberOfLesson { get; set; }
        public string? Semester { get; set; }
        public bool IsActive { get; set; }
        public int? DepartmentID { get; set; }
        
        [ForeignKey("DepartmentID")]
        public virtual tblDepartment? department { get; set; }
        public virtual ICollection<tblTeacherSubject> teacherSubject { get; set; } = new List<tblTeacherSubject>();
    }
}