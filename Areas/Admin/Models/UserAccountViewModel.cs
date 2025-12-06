namespace StudentManagement.Models
{
    public class UserAccountViewModel
    {
        public string Id { get; set; }          
        public string UserName { get; set; }    
        public string Email { get; set; }
        public bool IsLocked { get; set; }     
        public IList<string> Roles { get; set; }
    }
}