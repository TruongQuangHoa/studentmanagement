namespace StudentManagement.Models
{
    public class HomeDashboardVM
    {
        public string FullName { get; set; }
        public IEnumerable<tblPost> FeaturedPosts { get; set; }
        public List<SupportContact> SupportContacts { get; set; } = new List<SupportContact>();
    }

    public class SupportContact
    {
        public string Title { get; set; }
        public string ContactInfo { get; set; }
        public string IconClass { get; set; }
        public string BadgeClass { get; set; }
    }
}