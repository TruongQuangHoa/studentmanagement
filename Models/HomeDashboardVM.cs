namespace StudentManagement.Models
{
    // Dùng chung cho cả thông tin cá nhân học sinh và dữ liệu Slideshow
    public class HomeDashboardVM
    {
        // Thông tin cá nhân (Mocked)
        public string FullName { get; set; }
        
        // 1. Dữ liệu cho Slideshow
        public IEnumerable<tblPost> FeaturedPosts { get; set; }

        // 2. Dữ liệu cho Widget Hỗ trợ
        public List<SupportContact> SupportContacts { get; set; } = new List<SupportContact>();
    }
    
    // Class phụ cho Danh sách Hỗ trợ
    public class SupportContact
    {
        public string Title { get; set; }
        public string ContactInfo { get; set; }
        public string IconClass { get; set; }
        public string BadgeClass { get; set; }
    }
}