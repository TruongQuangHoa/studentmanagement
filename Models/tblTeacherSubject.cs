using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.Models
{
    [Table("tblTeacherSubject")]
    public class tblTeacherSubject
    {
        [Key, Column(Order = 0)]
        public string? TeacherID { get; set; } = null!;

        [Key, Column(Order = 1)]
        public int? SubjectID { get; set; }

        [ForeignKey(nameof(TeacherID))]
        public tblTeacher? teacher { get; set; }

        [ForeignKey(nameof(SubjectID))]
        public tblSubject? subject { get; set; }
    }
}