using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.Areas.Admin.Models;

namespace StudentManagement.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        public DbSet<tblMenu> Menus { get; set; }
        public DbSet<AdminMenu> AdminMenus { get; set; }
        public DbSet<tblCohort> Cohorts { get; set; }
        public DbSet<tblGrade> Grades { get; set; }
        public DbSet<tblYearSemester> YearSemesters { get; set; }
        public DbSet<tblDepartment> Departments { get; set; }
        public DbSet<tblSubject> Subjects { get; set; }
        public DbSet<tblTeacher> Teachers { get; set; }
        public DbSet<tblTeacherSubject> TeacherSubjects { get; set; }
        public DbSet<tblClass> Classes { get; set; }
        public DbSet<tblStudent> Students { get; set; }
        public DbSet<tblStudentClass> StudentClasses { get; set; }
        public DbSet<tblScore> Scores { get; set; }
        public DbSet<tblPost> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình khóa phụ duy nhất cho StudentID trong tblStudent
            modelBuilder.Entity<tblStudent>()
                .HasAlternateKey(hs => hs.StudentID);

            // Quan hệ tblStudentClass -> tblStudent (N-1)
            modelBuilder.Entity<tblStudentClass>()
            .HasOne(x => x.student)
            .WithMany(x => x.studentclass)
            .HasForeignKey(x => x.StudentID)      // tên cột trong tblStudentClass
            .HasPrincipalKey(x => x.StudentID);   // tên khóa chính trong tblStudent

            // Quan hệ tblTeacherSubject -> tblTeacher
            modelBuilder.Entity<tblTeacherSubject>()
                    .HasKey(x => new { x.TeacherID, x.SubjectID });

            // Quan hệ tblTeacherSubject -> tblTeacher (N-1)
            modelBuilder.Entity<tblTeacherSubject>()
                .HasOne(x => x.teacher)
                .WithMany(x => x.teachersubject)
                .HasForeignKey(x => x.TeacherID)
                .HasPrincipalKey(x => x.TeacherID);

            // Quan hệ tblTeacherSubject -> tblSubject (N-1)
            modelBuilder.Entity<tblTeacherSubject>()
                .HasOne(x => x.subject)
                .WithMany(x => x.teacherSubject)
                .HasForeignKey(x => x.SubjectID)
                .HasPrincipalKey(x => x.SubjectID);

           // Cấu hình quan hệ tblScore -> tblStudent
            modelBuilder.Entity<tblScore>()
                .HasOne(d => d.student)
                .WithMany()
                .HasForeignKey(d => d.StudentID)
                .HasPrincipalKey(hs => hs.StudentID);

            // Gọi 1 lần duy nhất
            base.OnModelCreating(modelBuilder);
        }
    }
}