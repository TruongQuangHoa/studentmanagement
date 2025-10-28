using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.Models
{
    [Table("tblClass")]
    public class tblClass
    {
        [Key]
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public int GradeID { get; set; }
        public int NumberOfStudent { get; set; }
        public string SchoolYear { get; set; }
        public bool IsActive { get; set; }
        public int? CohortID { get; set; }

        [ForeignKey("GradeID")]
        public virtual tblGrade? grade { get; set; }
        
        [ForeignKey("CohortID")]
        public virtual tblCohort ? cohort { get; set; }
    }
}