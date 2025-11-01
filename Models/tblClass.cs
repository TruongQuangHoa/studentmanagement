using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using StudentManagement.Models;

namespace StudentManagement.Models
{
    [Table("tblClass")]
    public class tblClass
    {
        [Key]
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public int GradeID { get; set; }
        public int MaxStudents { get; set; }
        public int CurrentStudents { get; set; }
        public string SchoolYear { get; set; }
        public bool IsActive { get; set; }
        public int? CohortID { get; set; }

        [ForeignKey("GradeID")]
        public virtual tblGrade? grade { get; set; }
        
        [ForeignKey("CohortID")]
        public virtual tblCohort? cohort { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<tblStudentClass>? studentclass { get; set; }
    }
}