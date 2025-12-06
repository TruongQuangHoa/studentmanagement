using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.Models
{
    [Table("tblYearSemester")]
    public class tblYearSemester
    {
        [Key]
        public int YearSemesterID { get; set; }
        public string? SemesterName { get; set; }
        public string? SchoolYear { get; set; }
        public bool IsActive { get; set; }
    }
}