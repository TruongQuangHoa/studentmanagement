using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.Models
{
    [Table("tblGrade")]
    public class tblGrade
    {
        [Key]
        public int GradeID { get; set; }
        public string? GradeName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}