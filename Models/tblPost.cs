using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.Models
{
    [Table("tblPost")]
    public class tblPost
    {
        [Key]
        public long PostID { get; set; }
        public string? Title { get; set; }
        public string? Abstract { get; set; }
        public string? Contents { get; set; }
        public string? Images { get; set; }
        public string? Link { get; set; }
        public string? Anthor { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public int PostOrder { get; set; }
    }
}