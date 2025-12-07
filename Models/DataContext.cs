using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.Areas.Admin.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace StudentManagement.Models
{
    public class DataContext : IdentityDbContext<IdentityUser>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        public DbSet<tblMenu> Menus { get; set; } = null!;
        public DbSet<AdminMenu> AdminMenus { get; set; } = null!;
        public DbSet<tblCohort> Cohorts { get; set; } = null!;
        public DbSet<tblGrade> Grades { get; set; } = null!;
        public DbSet<tblYearSemester> YearSemesters { get; set; } = null!;
        public DbSet<tblDepartment> Departments { get; set; } = null!;
        public DbSet<tblSubject> Subjects { get; set; } = null!;
        public DbSet<tblTeacher> Teachers { get; set; } = null!;
        public DbSet<tblTeacherSubject> TeacherSubjects { get; set; } = null!;
        public DbSet<tblClass> Classes { get; set; } = null!;
        public DbSet<tblStudent> Students { get; set; } = null!;
        public DbSet<tblStudentClass> StudentClasses { get; set; } = null!;
        public DbSet<tblScore> Scores { get; set; } = null!;
        public DbSet<tblPost> Posts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
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