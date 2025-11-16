using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using StudentManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
// using OfficeOpenXml;
using System.Globalization;

namespace StudentManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TeacherController : Controller
    {
        private readonly DataContext _context;
        public TeacherController(DataContext context)
        {
            _context = context;
        }
        public ActionResult Index()
        {
            var tcList = _context.Teachers.OrderBy(t => t.ID)
             .Include(t => t.department).ToList();
            return View(tcList);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var tc = _context.Teachers.Find(id);
            if (tc == null)
                return NotFound();
            return View(tc);
        }
        [HttpPost]

        public IActionResult Delete(int id)
        {
            var delTeacher = _context.Teachers.Find(id);
            if (delTeacher == null)
                return NotFound();
            _context.Teachers.Remove(delTeacher);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            getData();
            ViewBag.SelectedSubjects = new List<int>();
            return View();
        }

        [HttpPost]
        public IActionResult Create(tblTeacher model, int[] SelectedSubjectIDs)
        {
            // Kiểm tra xem TeacherID đã tồn tại chưa
            if (_context.Teachers.Any(t => t.TeacherID == model.TeacherID))
            {
                // Thêm lỗi vào ModelState
                ModelState.AddModelError("TeacherID", "Mã giáo viên này đã tồn tại trong hệ thống.");
            }

            if (ModelState.IsValid)
            {
                _context.Teachers.Add(model);
                _context.SaveChanges();

                // Thêm các môn học giáo viên dạy
                if (SelectedSubjectIDs != null && SelectedSubjectIDs.Length > 0)
                {
                    foreach (var subId in SelectedSubjectIDs)
                    {
                        _context.TeacherSubjects.Add(new tblTeacherSubject
                        {
                            TeacherID = model.TeacherID,
                            SubjectID = subId
                        });
                    }
                    _context.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            getData();
            ViewBag.SelectedSubjects = SelectedSubjectIDs?.ToList() ?? new List<int>();
            return View(model);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var teacher = _context.Teachers.Find(id);
            if (teacher == null) return NotFound();

            getData();

            // Lấy danh sách môn học giáo viên đang dạy 
            var selectedSubjects = _context.TeacherSubjects
                .Where(x => x.TeacherID == teacher.TeacherID)
                .Select(x => x.SubjectID ?? 0) 
                .ToList();

            ViewBag.SelectedSubjects = selectedSubjects;

            return View(teacher);
        }

        [HttpPost]
        public IActionResult Edit(tblTeacher model, int[] SelectedSubjectIDs)
        {
            // Kiểm tra trùng lặp TeacherID, nhưng cho phép chính bản ghi đang chỉnh sửa
            var existingTeacher = _context.Teachers.AsNoTracking().FirstOrDefault(t => t.TeacherID == model.TeacherID);
            
            // Nếu bạn dùng ID là khóa chính, ta kiểm tra TeacherID trùng với giáo viên khác (ID khác)
            if (existingTeacher != null && existingTeacher.ID != model.ID)
            {
                ModelState.AddModelError("TeacherID", "Mã giáo viên này đã tồn tại cho một giáo viên khác.");
            }

            if (ModelState.IsValid)
            {
                _context.Teachers.Update(model);
                
                // Xóa môn học cũ
                var oldSubjects = _context.TeacherSubjects.Where(x => x.TeacherID == model.TeacherID);
                _context.TeacherSubjects.RemoveRange(oldSubjects);

                // Thêm môn học mới
                if (SelectedSubjectIDs != null && SelectedSubjectIDs.Length > 0)
                {
                    foreach (var subId in SelectedSubjectIDs)
                    {
                        _context.TeacherSubjects.Add(new tblTeacherSubject
                        {
                            TeacherID = model.TeacherID,
                            SubjectID = subId
                        });
                    }
                }
                
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            getData();
            ViewBag.SelectedSubjects = SelectedSubjectIDs?.ToList() ?? new List<int>();
            return View(model);
        }

        private void getData()
        {
            var dpList = _context.Departments.Where(d => d.IsActive == true)
                .Select(dp => new
                {
                    dp.DepartmentID,
                    Info = dp.DepartmentID + " - " + dp.DepartmentName
                }).ToList();
            ViewBag.dpList = new SelectList(dpList, "DepartmentID", "Info");

            var sbList = _context.Subjects
                .Where(sb => sb.IsActive)
                .Select(sb => new
                {
                    sb.SubjectID,
                    Info = sb.SubjectID + " - " + sb.SubjectName
                }).ToList();
            ViewBag.sbList = new SelectList(sbList, "SubjectID", "Info");
        }

        [HttpGet]
        public IActionResult TeacherListPartial(
            string filter,
            string religionFilter = null,
            string statusFilter = null,
            string partyFilter = null,
            string genderFilter = null,
            string departmentFilter = null,
            string ageRangeFilter = null)
        {
            try
            {
                IQueryable<tblTeacher> teachersQuery = _context.Teachers;

                switch (filter.ToLower())
                {
                    case "all":
                        break;
                    case "minority":
                        teachersQuery = teachersQuery.Where(g => !string.IsNullOrEmpty(g.Nation));
                        break;
                    case "religion":
                        if (!string.IsNullOrEmpty(religionFilter))
                        {
                            teachersQuery = teachersQuery.Where(g => g.Religion != null &&
                                g.Religion.ToLower() == religionFilter.ToLower());
                        }
                        break;
                    case "status":
                        if (!string.IsNullOrEmpty(statusFilter))
                        {
                            teachersQuery = teachersQuery.Where(g => g.StatusTeacher != null &&
                                g.StatusTeacher.ToLower() == statusFilter.ToLower());
                        }
                        break;
                    case "party":
                        if (!string.IsNullOrEmpty(partyFilter))
                        {
                            teachersQuery = teachersQuery.Where(g => g.GroupDV != null &&
                                g.GroupDV.ToLower() == partyFilter.ToLower());
                        }
                        break;
                    case "gender":
                        if (!string.IsNullOrEmpty(genderFilter))
                        {
                            teachersQuery = teachersQuery.Where(g => g.Gender != null &&
                                g.Gender.ToLower() == genderFilter.ToLower());
                        }
                        break;
                    case "department":
                        if (!string.IsNullOrEmpty(departmentFilter))
                        {
                            teachersQuery = teachersQuery.Where(g => g.department != null &&
                                g.department.DepartmentName.ToLower() == departmentFilter.ToLower());
                        }
                        break;
                    case "age":
                        if (!string.IsNullOrEmpty(ageRangeFilter))
                        {
                            var range = ageRangeFilter.Split('-');
                            if (range.Length == 2 && int.TryParse(range[0], out int minAge) && int.TryParse(range[1], out int maxAge))
                            {
                                var minBirthDate = DateTime.Now.AddYears(-maxAge - 1);
                                var maxBirthDate = DateTime.Now.AddYears(-minAge);
                                teachersQuery = teachersQuery.Where(g => g.Birth != null &&
                                    g.Birth >= minBirthDate && g.Birth <= maxBirthDate);
                            }
                        }
                        break;
                    default:
                        return BadRequest("Invalid filter");
                }

                var teachers = teachersQuery.Select(g => new
                {
                    g.TeacherID,
                    g.FullName,
                    Birth = g.Birth.HasValue ? g.Birth.Value.ToString("dd/MM/yyyy") : "Không rõ",
                    g.Nation,
                    Religion = g.Religion ?? "Không rõ",
                    StatusTeacher = g.StatusTeacher ?? "Không rõ",
                    GroupDV = g.GroupDV ?? "Không rõ",
                    Gender = g.Gender ?? "Không rõ",
                    Department = g.department != null ? g.department.DepartmentName : "Không rõ"
                }).ToList();

                return Json(teachers);
            }
            catch (Exception ex)
            {

                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
                return NotFound();
            teacher.IsActive = !teacher.IsActive;
            _context.Update(teacher);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // tải excel
        // public ActionResult Export()
        // {
        //     try
        //     {
        //         // Thay đổi từ QLGiaoViens sang QLGiaoViens (hoặc QLTeachers tùy theo tên DbSet của bạn)
        //         // Nếu có mối quan hệ cần include (ví dụ: QLGiaoVien có LopDay), bạn có thể thêm .Include() ở đây
        //         var rawData = _context.QLGiaoViens // Giả định DbSet của giáo viên là QLGiaoViens
        //                             .ToList();

        //         var data = rawData.Select((t, index) => new
        //         {
        //             STT = index + 1,
        //             TeacherID = t.TeacherID,
        //             FullName = t.FullName,
        //             Birth = t.Birth?.ToString("dd/MM/yyyy") ?? "Không có dữ liệu",
        //             Gender = t.Gender,
        //             Address = t.Address,

        //             CCCD = t.CCCD,
        //             Nation = t.Nation,
        //             Religion = t.Religion,
        //             GroupDV = t.GroupDV,
        //             NumberPhone = t.NumberPhone,
        //             NumberBHXH = t.NumberBHXH,

        //         }).ToList();

        //         System.Diagnostics.Debug.WriteLine($"Số lượng bản ghi được lấy: {data.Count}");

        //         // Thiết lập giấy phép EPPlus
        //         // Đảm bảo bạn đã cài đặt EPPlus phiên bản 5 trở lên và cấu hình giấy phép
        //         ExcelPackage.License.SetNonCommercialPersonal("QLGiaoVien");

        //         var stream = new MemoryStream();
        //         using (var package = new ExcelPackage(stream))
        //         {
        //             var sheet = package.Workbook.Worksheets.Add("QLGiaoVien");

        //             // Tải dữ liệu vào sheet
        //             // Tham số thứ hai (true) có nghĩa là dòng đầu tiên sẽ là tiêu đề
        //             sheet.Cells.LoadFromCollection(data, true);

        //             // Tùy chọn: Tự động điều chỉnh độ rộng cột cho dễ đọc
        //             sheet.Cells[sheet.Dimension.Start.Row, sheet.Dimension.Start.Column, sheet.Dimension.End.Row, sheet.Dimension.End.Column].AutoFitColumns();

        //             package.Save(); // Lưu gói Excel vào MemoryStream
        //         }

        //         stream.Position = 0; // Đặt lại vị trí luồng về đầu trước khi trả về file
        //         var fileName = $"Danhsachgiaovien.xlsx";

        //         // Trả về file Excel
        //         // return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        //         // Trả file kèm tên gợi ý (trình duyệt sẽ quyết định có hỏi lưu hay tự tải)
        //         return File(stream,
        //                     "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //                     fileName);
        //     }
        //     catch (Exception ex)
        //     {
        //         // Ghi lại lỗi để kiểm tra. Trong môi trường thực tế, bạn nên sử dụng một hệ thống logging chuyên nghiệp.
        //         System.Diagnostics.Debug.WriteLine($"Lỗi khi xuất Excel: {ex.Message}");
        //         System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");


        //         return Content("Đã xảy ra lỗi khi xuất file Excel. Vui lòng thử lại sau.");
        //     }
        // }

        // upload
        // public async Task<IActionResult> FileUpload(IFormFile UploadedFile)
        // {
        //     var newTeacherList = new List<QLGiaoVien>();

        //     if (UploadedFile != null && UploadedFile.Length > 0)
        //     {
        //         using (var stream = new MemoryStream())
        //         {
        //             await UploadedFile.CopyToAsync(stream);

        //             using (var package = new ExcelPackage(stream))
        //             {
        //                 var workSheet = package.Workbook.Worksheets.First();
        //                 var noOfRow = workSheet.Dimension.End.Row;

        //                 for (int row = 2; row <= noOfRow; row++)
        //                 {
        //                     // Đọc TeacherID là string
        //                     var teacherId = workSheet.Cells[row, 2].Value?.ToString()?.Trim();
        //                     if (string.IsNullOrEmpty(teacherId))
        //                     {
        //                         // Bỏ qua nếu TeacherID rỗng
        //                         continue;
        //                     }

        //                     var fullName = workSheet.Cells[row, 3].Value?.ToString()?.Trim();

        //                     // Parse Birth
        //                     var birthCell = workSheet.Cells[row, 4];
        //                     DateTime? birth = null;

        //                     if (birthCell.Value != null)
        //                     {
        //                         if (birthCell.Value is DateTime date)
        //                         {
        //                             birth = date;
        //                         }
        //                         else if (DateTime.TryParseExact(birthCell.Text,
        //                                     new[] { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" },
        //                                     CultureInfo.InvariantCulture,
        //                                     DateTimeStyles.None,
        //                                     out var parsed))
        //                         {
        //                             birth = parsed;
        //                         }
        //                     }

        //                     var gender = workSheet.Cells[row, 5].Value?.ToString()?.Trim();
        //                     var address = workSheet.Cells[row, 6].Value?.ToString()?.Trim();
        //                     var cccd = workSheet.Cells[row, 7].Value?.ToString()?.Trim();
        //                     var nation = workSheet.Cells[row, 8].Value?.ToString()?.Trim();
        //                     var religion = workSheet.Cells[row, 9].Value?.ToString()?.Trim();
        //                     var groupDV = workSheet.Cells[row, 10].Value?.ToString()?.Trim();
        //                     var numberPhone = workSheet.Cells[row, 11].Value?.ToString()?.Trim();
        //                     var numberBHXH = workSheet.Cells[row, 12].Value?.ToString()?.Trim();
        //                     var email = workSheet.Cells[row, 13].Value?.ToString()?.Trim();

        //                     // Tìm giáo viên theo TeacherID (string)
        //                     var existingTeacher = await _context.QLGiaoViens.FirstOrDefaultAsync(t => t.TeacherID == teacherId);

        //                     if (existingTeacher != null)
        //                     {
        //                         // Cập nhật
        //                         existingTeacher.FullName = fullName;
        //                         existingTeacher.Birth = birth;
        //                         existingTeacher.Gender = gender;
        //                         existingTeacher.Address = address;
        //                         existingTeacher.CCCD = cccd;
        //                         existingTeacher.Nation = nation;
        //                         existingTeacher.Religion = religion;
        //                         existingTeacher.GroupDV = groupDV;
        //                         existingTeacher.NumberPhone = numberPhone;
        //                         existingTeacher.NumberBHXH = numberBHXH;

        //                         existingTeacher.StatusTeacher = "Đang dạy";
        //                         existingTeacher.IsActive = true;
        //                     }
        //                     else
        //                     {
        //                         // Thêm mới
        //                         var newTeacher = new QLGiaoVien
        //                         {
        //                             TeacherID = teacherId,
        //                             FullName = fullName,
        //                             Birth = birth,
        //                             Gender = gender,
        //                             Address = address,
        //                             CCCD = cccd,
        //                             Nation = nation,
        //                             Religion = religion,
        //                             GroupDV = groupDV,
        //                             NumberPhone = numberPhone,
        //                             NumberBHXH = numberBHXH,

        //                             StatusTeacher = "Đang dạy",
        //                             IsActive = true,
        //                         };

        //                         newTeacherList.Add(newTeacher);
        //                     }
        //                 }

        //                 if (newTeacherList.Count > 0)
        //                 {
        //                     await _context.QLGiaoViens.AddRangeAsync(newTeacherList);
        //                 }

        //                 await _context.SaveChangesAsync();
        //             }
        //         }
        //     }

        //     return RedirectToAction("Index");
        // }

        // thống kê
        // public IActionResult ThongKe()
        // {
        //     try
        //     {
        //         var now = DateTime.Now;

        //         // Total teachers count
        //         var totalTeachers = _context.QLGiaoViens.Count();
        //         var totalMinorityTeachers = _context.QLGiaoViens
        //             .Count(g => !string.IsNullOrEmpty(g.Nation));

        //         // Department statistics
        //         var boMonStats = _context.QLGiaoViens
        //             .Where(g => g.BoMon != null)
        //             .GroupBy(g => g.BoMon.department_name)
        //             .Select(g => new { BoMon = g.Key, Count = g.Count() })
        //             .OrderBy(g => g.BoMon)
        //             .ToDictionary(g => g.BoMon ?? "Không xác định", g => g.Count);

        //         // Age statistics (grouped by 5-year intervals)
        //         var ageStats = _context.QLGiaoViens
        //             .Where(g => g.Birth != null)
        //             .AsEnumerable() // Switch to client-side for complex calculations
        //             .GroupBy(g =>
        //             {
        //                 var age = now.Year - g.Birth.Value.Year;
        //                 if (g.Birth.Value.Date > now.AddYears(-age)) age--;
        //                 return (age / 5) * 5; // Group by 5-year intervals
        //             })
        //             .Select(g => new { AgeRange = $"{g.Key}-{g.Key + 4}", Count = g.Count() })
        //             .OrderBy(g => g.AgeRange)
        //             .ToDictionary(g => g.AgeRange, g => g.Count);

        //         // Gender statistics
        //         var genderStats = _context.QLGiaoViens
        //             .GroupBy(g => string.IsNullOrEmpty(g.Gender) ? "Không xác định" : g.Gender)
        //             .Select(g => new { Gender = g.Key, Count = g.Count() })
        //             .OrderByDescending(g => g.Count)
        //             .ToDictionary(g => g.Gender, g => g.Count);

        //         // Religion statistics (with "Other" for less common religions)
        //         var allReligions = _context.QLGiaoViens
        //             .Where(g => !string.IsNullOrEmpty(g.Religion))
        //             .GroupBy(g => g.Religion)
        //             .Select(g => new { Religion = g.Key, Count = g.Count() })
        //             .ToList();

        //         var religionStats = allReligions
        //             .Where(r => r.Count > 2) // Show separately if more than 2 teachers
        //             .OrderByDescending(r => r.Count)
        //             .ToDictionary(r => r.Religion, r => r.Count);

        //         var otherReligionsCount = allReligions
        //             .Where(r => r.Count <= 2)
        //             .Sum(r => r.Count);

        //         if (otherReligionsCount > 0)
        //         {
        //             religionStats.Add("Khác", otherReligionsCount);
        //         }

        //         // Teaching status statistics
        //         var statusStats = _context.QLGiaoViens
        //             .GroupBy(g => string.IsNullOrEmpty(g.StatusTeacher) ? "Không xác định" : g.StatusTeacher)
        //             .Select(g => new { Status = g.Key, Count = g.Count() })
        //             .OrderByDescending(g => g.Count)
        //             .ToDictionary(g => g.Status, g => g.Count);

        //         // Party member statistics
        //         var partyStats = _context.QLGiaoViens
        //             .GroupBy(g => string.IsNullOrEmpty(g.GroupDV) ? "Không xác định" : g.GroupDV)
        //             .Select(g => new { Party = g.Key, Count = g.Count() })
        //             .OrderByDescending(g => g.Count)
        //             .ToDictionary(g => g.Party, g => g.Count);

        //         // Province statistics (top 10 provinces)
        //         var provinceStats = _context.QLGiaoViens
        //             .Where(g => !string.IsNullOrEmpty(g.Province))
        //             .GroupBy(g => g.Province)
        //             .Select(g => new { Province = g.Key, Count = g.Count() })
        //             .OrderByDescending(g => g.Count)
        //             .Take(10)
        //             .ToDictionary(g => g.Province, g => g.Count);

        //         var otherProvincesCount = _context.QLGiaoViens
        //             .Count(g => !string.IsNullOrEmpty(g.Province)) - provinceStats.Values.Sum();

        //         if (otherProvincesCount > 0)
        //         {
        //             provinceStats.Add("Tỉnh/TP khác", otherProvincesCount);
        //         }

        //         // Prepare data for View
        //         ViewBag.TotalTeachers = totalTeachers;
        //         ViewBag.TotalMinorityTeachers = totalMinorityTeachers;
        //         ViewBag.BoMonStats = boMonStats;
        //         ViewBag.AgeStats = ageStats;
        //         ViewBag.GenderStats = genderStats;
        //         ViewBag.ReligionStats = religionStats;
        //         ViewBag.StatusStats = statusStats;
        //         ViewBag.PartyStats = partyStats;
        //         ViewBag.ProvinceStats = provinceStats;

        //         // For dropdown filters
        //         ViewBag.Religions = religionStats;
        //         ViewBag.StatusGV = statusStats;
        //         ViewBag.Genders = genderStats;
        //         ViewBag.Parties = partyStats;

        //         return View();
        //     }
        //     catch (Exception ex)
        //     {

        //         return StatusCode(500, "An error occurred while processing your request.");
        //     }
        // }
    }
}