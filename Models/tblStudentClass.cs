using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudentManagement.Models
{
    [Table("tblStudentClass")]
    public class tblStudentClass
    {
        [Key]
        public int StudentClassID { get; set; }
        [Column("StudentID")]
        public string? StudentID { get; set; }
        public int ClassID { get; set; }
        public bool IsActive { get; set; }

        [ForeignKey(nameof(StudentID))]
        public tblStudent? student { get; set; }

        public int? CohortID { get; set; }
        [ForeignKey(nameof(CohortID))]
        public tblCohort? cohort { get; set; }

        // Quan hệ ngược lại lớp
        [JsonIgnore] // chặn vòng lặp
        [ForeignKey(nameof(ClassID))]
        public virtual tblClass? _class { get; set; }
    }
}