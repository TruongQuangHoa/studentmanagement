// File: tblPost.cs (Trong thư mục Models)

using System;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models
{
    // Cần định nghĩa nó là một Class, không phải DbSet
    public class tblPost 
    {
        [Key] // Đánh dấu khóa chính (nên thêm để EF Core nhận diện)
        public int PostID { get; set; } 
        
        public string Title { get; set; }
        public string Summary { get; set; }
        public string ImageUrl { get; set; }
        
        // Thêm các thuộc tính khác cần thiết cho truy vấn:
        public bool? IsActive { get; set; }
        public bool? IsFeatured { get; set; }
        public int? PostOrder { get; set; }
        
        // Lưu ý: Nếu bạn có thêm thuộc tính PublishDate, hãy thêm nó ở đây
    }
}