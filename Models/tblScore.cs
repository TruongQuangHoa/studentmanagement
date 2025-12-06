using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.Models
{
    [Table("tblScore")]
    public class tblScore
    {
        [Key]
        public int ScoreID { get; set; }
        public string StudentID { get; set; }
        public int SubjectID { get; set; }
        public int YearSemesterID { get; set; }
        public double? OralScore1 { get; set; }
        public double? OralScore2 { get; set; }
        public double? Score15Minute1 { get; set; }
        public double? Score15Minute2 { get; set; }
        public double? Average_CA_Score { get; set; }
        public double? MidtermScore { get; set; }
        public double? FinalScore { get; set; }
        public double? AverageScore { get; set; }
        public string? AcademicRating { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        [ForeignKey(nameof(StudentID))]
        public tblStudent? student { get; set; }

        [ForeignKey(nameof(SubjectID))]
        public tblSubject? subject { get; set; }

        [ForeignKey(nameof(YearSemesterID))]
        public tblYearSemester? yearSemester { get; set; }

    }
}