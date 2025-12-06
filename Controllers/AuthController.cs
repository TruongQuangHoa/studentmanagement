using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using StudentManagement.Models;

namespace StudentManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager; // 1. Mới thêm
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;

        // 2. Inject RoleManager vào Constructor
        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, DataContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager; // Gán giá trị
            _configuration = configuration;
            _context = context;
        }

        // --- API 1: Đăng ký User mới ---
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterVM model)
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                return BadRequest("Username and password are required.");

            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "User already exists!" });

            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "User creation failed! Check password requirements." });

            return Ok(new { Status = "Success", Message = "User created successfully!" });
        }

        // --- API 2: Đăng nhập (Có trả về Roles) ---
        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginVM model)
        {
            // 1. Kiểm tra user có tồn tại không
            var user = await _userManager.FindByNameAsync(model.Username);
            
            // 2. Kiểm tra password
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                // 3. Khởi tạo danh sách Claims cơ bản
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                // --- BẮT ĐẦU LOGIC LẤY THÔNG TIN CÁ NHÂN (HỌ TÊN / ẢNH) ---
                string fullName = user.UserName; // Mặc định lấy username nếu không tìm thấy tên thật
                string avatar = "/admin/assets/img/profile-img.jpg"; // Ảnh mặc định

                // Tìm trong bảng Học sinh xem có khớp mã không
                var student = _context.Students.FirstOrDefault(s => s.StudentID == user.UserName);
                if (student != null)
                {
                    // Nếu tìm thấy học sinh, lấy tên thật
                    if (!string.IsNullOrEmpty(student.FullName)) 
                        fullName = student.FullName;
                    
                    // Lấy ảnh (nếu có)
                    if (!string.IsNullOrEmpty(student.Images)) 
                        avatar = student.Images;
                }
                
                // Thêm Claims tùy chỉnh vào Token
                authClaims.Add(new Claim("FullName", fullName));
                authClaims.Add(new Claim("Avatar", avatar));
                // --- KẾT THÚC LOGIC LẤY THÔNG TIN ---

                // 4. Lấy tất cả Role của user và nhét vào Token
                var userRoles = await _userManager.GetRolesAsync(user);
                foreach (var role in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }

                // 5. Tạo chữ ký số (Signature)
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

                // 6. Tạo Token
                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddHours(3), // Token hết hạn sau 3 tiếng
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                // 7. Trả về kết quả
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    roles = userRoles.ToList() // Trả về roles để Frontend biết đường chuyển trang
                });
            }

            // Nếu sai tài khoản hoặc mật khẩu
            return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác!" });
        }

        // --- API 3: Tạo Role Mới (Admin/Student...) ---
        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName)) return BadRequest("Role name required");

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
                return Ok(new { message = $"Role '{roleName}' created successfully!" });
            }
            return BadRequest("Role already exists");
        }

        // --- API 4: Gán Role cho User ---
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] UserRoleVM model)
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.RoleName))
                return BadRequest("Username and RoleName are required.");

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null) return BadRequest("User not found");

            if (!await _roleManager.RoleExistsAsync(model.RoleName)) return BadRequest("Role not found");

            await _userManager.AddToRoleAsync(user, model.RoleName);
            return Ok(new { message = $"User '{model.Username}' assigned to '{model.RoleName}'" });
        }
    }

    // Class phụ để hứng dữ liệu gán quyền
    public class UserRoleVM
    {
        public string? Username { get; set; }
        public string? RoleName { get; set; }
    }
}