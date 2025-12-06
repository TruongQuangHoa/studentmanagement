using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using StudentManagement.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace StudentManagement.Models
{
    [Table("tblClass")]
    public class tblClass
    {
        [Key]
        public int ClassID { get; set; }

        [Required(ErrorMessage = "Tên lớp không được để trống")]
        public string? ClassName { get; set; }

        [BindNever]  // không bind từ form
        public int GradeID { get; set; }

        [Required(ErrorMessage = "Số lượng tối đa không được để trống")]
        public int MaxStudents { get; set; }

        [BindNever]
        public int CurrentStudents { get; set; }

        [BindNever]
        public string? SchoolYear { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public int? CohortID { get; set; }

        [ForeignKey("GradeID")]
        public virtual tblGrade? grade { get; set; }
        
        [ForeignKey("CohortID")]
        public virtual tblCohort? cohort { get; set; }
        
        // Quan hệ ngược tới học sinh-lớp
        [JsonIgnore] // tránh vòng lặp khi serialize
        public virtual ICollection<tblStudentClass>? studentclass { get; set; }

        // [JsonIgnore] // tránh vòng lặp nếu bạn không cần ngược lại từ lớp → thời khóa biểu
        // public virtual ICollection<QLKhoaBieu>? KhoaBieus { get; set; }
    }
}
