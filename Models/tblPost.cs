// File: tblPost.cs (Trong thư mục Models)

using System;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models
{
    
    public class tblPost 
    {
        [Key]
        public int PostID { get; set; } 
        
        public string Title { get; set; }
        public string Summary { get; set; }
        public string ImageUrl { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFeatured { get; set; }
        public int? PostOrder { get; set; }
        
    }
}