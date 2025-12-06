using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudentManagement.Models
{
    [Table("tblTeacher")]
    public class tblTeacher
    {
        [Key]
        public int ID { get; set; }
        public string TeacherID { get; set; }
        public string? FullName { get; set; }
        public DateTime? Birth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? StatusTeacher { get; set; }
        public string? CCCD { get; set; }
        public string? Nation { get; set; }
        public string? Religion { get; set; }
        public string? GroupDV { get; set; }
        public string? NumberPhone { get; set; }
        public string? NumberBHXH { get; set; }
        public bool IsActive { get; set; }
        public int? DepartmentID { get; set; }
        public string? Hamlet { get; set; }
        public string? Commune { get; set; }
        public string? Province { get; set; }
        public string? Nationality { get; set; }
        public string? Images { get; set; }
        [ForeignKey(nameof(DepartmentID))]
        public virtual tblDepartment? department { get; set; }
        // Quan hệ N-N với Subject
        [JsonIgnore]
        public virtual ICollection<tblTeacherSubject> teachersubject { get; set; } = new List<tblTeacherSubject>();
    }
}