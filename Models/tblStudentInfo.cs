using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.Models
{
    [Table("tblStudentInfo")]
    public class tblStudentInfo
    {
        [Key]
        public int ID { get; set; }
        public string StudentID { get; set; }
        public string FullName { get; set; }
        public DateTime? Birth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? Nation { get; set; }
        public string? Religion { get; set; }
        public string? StatusStudent { get; set; }
        public string? NumberPhone { get; set; }
        public bool IsActive { get; set; }
        public string? Images { get; set; }

        public string? Hamlet { get; set; }
        public string? Commune { get; set; }

        public string? Province { get; set; }
        public string? Nationality { get; set; }

       // public virtual ICollection<tblStudent>? HocSinhLopHocs { get; set; }
    }
}