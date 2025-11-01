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
        public DbSet<tblClass> Classes { get; set; }
        public DbSet<tblStudent> Students { get; set; }
        public DbSet<tblStudentClass> StudentClasses { get; set; }

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
        }
    }
}