using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.Models
{
    [Table("tblCohort")]
    public class tblCohort
    {
        [Key]
        public int CohortID { get; set; }
        public int? CohortName { get; set; } 
        public int? StartYear { get; set; } 
        public int? EndYear { get; set; } 
        public bool IsActive { get; set; } 
    }
    
}