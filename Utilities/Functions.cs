using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudentManagement.Utilities
{
    public class Functions
    {
        public static string TitleSlugGeneration(string type, string? title, long id)
        {
            return type + "-" + SlugGenerator.SlugGenerator.GenerateSlug(title) + "-" + id.ToString() + ".html";
        }

        public static string getCurrentData()
        {
            //Lay ngay thang nam hien tai, dinh dang theo datetime
            //trong bang post luu tai csdl aznews trong sql servet
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}