using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        public DbSet<tblStudentInfo> StudentInfos { get; set; }
        public DbSet<tblStudent> Students { get; set; }
    }
}