using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
// using StudentManagement.Ultilities;
using StudentManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
// using OfficeOpenXml;
using System.Globalization;
// using QRCoder;
using System.Drawing;
using System.IO.Compression;
using System.Drawing.Drawing2D;

namespace StudentManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StudentController : Controller
    {
        private readonly DataContext _context;

        public StudentController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? classID = null)
        {
            // ViewBag.IsAdmin = Functions.IsAdmin(HttpContext);
            // ViewBag.IsTeacher = Functions.IsTeacher(HttpContext);

            // Lấy tất cả học sinh, bao gồm cả học sinh chưa có lớp
            var stList = _context.Students
                .Include(s => s.studentclass)
                    .ThenInclude(st => st._class)
                        .ThenInclude(l => l.grade)
                .AsQueryable();

            if (classID.HasValue)
            {
                // Lọc những học sinh có lớp active phù hợp classId
                stList = stList.Where(s => s.studentclass.Any(st => st.ClassID == classID.Value && st.IsActive));
                ViewBag.SelectedClass = classID.Value.ToString();
            }
            else
            {
                ViewBag.SelectedClass = "";
            }
            LoadData();
            return View(stList.OrderBy(h => h.ID).ToList());
        }

        // public IActionResult ExportToExcel(int? classId)
        // {
        //     try
        //     {
        //         var query = _context.QLHocSinhs
        //             .Include(h => h.HocSinhLopHocs)
        //                 .ThenInclude(hsl => hsl.lopHoc)
        //             .Where(h => h.HocSinhLopHocs.Any(hsl => hsl.IsActive))
        //             .AsQueryable();

        //         string fileLabel = "toantruong";

        //         if (classId.HasValue && classId.Value > 0)
        //         {
        //             query = query.Where(h => h.HocSinhLopHocs.Any(hsl => hsl.ClassID == classId.Value && hsl.IsActive));
        //             var className = _context.QLLopHocs
        //                 .Where(l => l.ClassID == classId)
        //                 .Select(l => l.ClassName)
        //                 .FirstOrDefault();
        //             fileLabel = className?.Replace(" ", "_") ?? $"Lop_{classId}";
        //         }

        //         var rawData = query.ToList();

        //         if (!rawData.Any())
        //             return Content("Không có học sinh nào để xuất Excel.");

        //         var data = rawData.Select((h, index) => new
        //         {
        //             STT = index + 1,
        //             StudentID = h.StudentID,
        //             FullName = h.FullName,
        //             Birth = h.Birth?.ToString("dd/MM/yyyy") ?? "Không có dữ liệu",
        //             Gender = h.Gender,
        //             Address = h.Address,
        //             ClassName = h.HocSinhLopHocs.FirstOrDefault(hsl => hsl.IsActive)?.lopHoc?.ClassName ?? "Chưa có lớp",
        //             Nation = h.Nation,
        //             Religion = h.Religion,
        //             NumberPhone = h.NumberPhone
        //         }).ToList();

        //         var stream = new MemoryStream();
        //         using (var package = new ExcelPackage(stream))
        //         {
        //             var sheet = package.Workbook.Worksheets.Add($"DanhSach_{fileLabel}");
        //             sheet.Cells.LoadFromCollection(data, true);
        //             sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        //             package.Save();
        //         }

        //         stream.Position = 0;
        //         var fileName = $"DanhSachHocSinh_{fileLabel}.xlsx";
        //         return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        //     }
        //     catch (Exception ex)
        //     {
        //         System.Diagnostics.Debug.WriteLine($"Lỗi khi xuất Excel: {ex.Message}");
        //         return Content("Đã xảy ra lỗi khi xuất file Excel.");
        //     }
        // }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var st = _context.Students.Find(id);
            if (st == null)
                return NotFound();
            return View(st);
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var delStudent = _context.Students.Find(id);
            if (delStudent == null)
                return NotFound();

            var classRelations = _context.StudentClasses.Where(st => st.StudentID == delStudent.StudentID);
            _context.StudentClasses.RemoveRange(classRelations);

            _context.Students.Remove(delStudent);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            LoadData();
            return View();
        }
        [HttpPost]
        public IActionResult Create(tblStudent st, int? ClassID)
        {
            // Kiểm tra xem studentID đã tồn tại chưa
            if (_context.Students.Any(t => t.StudentID == st.StudentID))
            {
                // Thêm lỗi vào ModelState
                ModelState.AddModelError("StudentID", "Mã học sinh này đã tồn tại trong hệ thống.");
            }

            if (ModelState.IsValid)
            {
                _context.Students.Add(st);
                if (ClassID.HasValue && ClassID > 0)
                {
                    var sc = new tblStudentClass
                    {
                        StudentID = st.StudentID,
                        ClassID = ClassID.Value,
                        IsActive = true
                    };
                    _context.StudentClasses.Add(sc);
                }

                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            LoadData();
            return View(st);
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var student = _context.Students
                 .Include(s => s.studentclass)
                 .FirstOrDefault(s => s.ID == id);

            if (student == null)
                return NotFound();

            var currentClass = student.studentclass.FirstOrDefault(st => st.IsActive);
            if (currentClass != null)
            {
                ViewBag.CurrentClassID = currentClass.ClassID;
            }

            LoadData();
            return View(student);
        }
        [HttpPost]
        public IActionResult Edit(tblStudent st, int? ClassID)
        {
            // Kiểm tra trùng lặp StudentID, nhưng cho phép chính bản ghi đang chỉnh sửa
            var existingStudent = _context.Students.AsNoTracking().FirstOrDefault(t => t.StudentID == st.StudentID);
            
            // Nếu bạn dùng ID là khóa chính, ta kiểm tra StudentID trùng với học sinh khác (ID khác)
            if (existingStudent != null && existingStudent.ID != st.ID)
            {
                ModelState.AddModelError("StudentID", "Mã học sinh này đã tồn tại cho một học sinh khác.");
            }

            if (ModelState.IsValid)
            {
                // Cập nhật thông tin học sinh
                _context.Update(st);

                // Xử lý quan hệ lớp học
                var currentRelation = _context.StudentClasses
                    .FirstOrDefault(sc => sc.StudentID == st.StudentID && sc.IsActive);

                if (ClassID.HasValue && ClassID > 0)
                {
                    if (currentRelation != null)
                    {
                        // Vô hiệu hóa mối quan hệ hiện tại
                        currentRelation.IsActive = false;
                        _context.Update(currentRelation);
                    }

                    // Tạo mối quan hệ active mới
                    var newRelation = new tblStudentClass
                    {
                        StudentID = st.StudentID,
                        ClassID = ClassID.Value,
                        IsActive = true
                    };
                    _context.StudentClasses.Add(newRelation);
                }
                else if (currentRelation != null)
                {
                    // Xóa lớp nếu không có lớp nào được chọn
                    currentRelation.IsActive = false;
                    _context.Update(currentRelation);
                }

                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            LoadData();
            return View(st);
        }

        // public async Task<IActionResult> FileUpload(IFormFile UploadedFile)
        // {
        //     var newStudentList = new List<QLHocSinh>();
        //     var newClassRelations = new List<QLHocSinhLopHoc>();

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
        //                     var studentId = workSheet.Cells[row, 2].Value?.ToString()?.Trim();
        //                     var fullName = workSheet.Cells[row, 3].Value?.ToString()?.Trim();

        //                     DateTime? birth = null;
        //                     var birthCell = workSheet.Cells[row, 4];
        //                     if (birthCell.Value != null)
        //                     {
        //                         if (birthCell.Value is DateTime date)
        //                         {
        //                             birth = date;
        //                         }
        //                         else if (DateTime.TryParseExact(birthCell.Text,
        //                                 new[] { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" },
        //                                 CultureInfo.InvariantCulture,
        //                                 DateTimeStyles.None,
        //                                 out var parsed))
        //                         {
        //                             birth = parsed;
        //                         }
        //                     }

        //                     var gender = workSheet.Cells[row, 5].Value?.ToString()?.Trim();
        //                     var address = workSheet.Cells[row, 6].Value?.ToString()?.Trim();

        //                     string hamlet = null, commune = null, district = null, province = null;
        //                     if (!string.IsNullOrEmpty(address))
        //                     {
        //                         var parts = address.Split(',').Select(p => p.Trim()).ToArray();
        //                         if (parts.Length >= 4)
        //                         {
        //                             hamlet = parts[0];
        //                             commune = parts[1];
        //                             district = parts[2];
        //                             province = parts[3];
        //                         }
        //                     }

        //                     var className = workSheet.Cells[row, 7].Value?.ToString()?.Trim();
        //                     var nation = workSheet.Cells[row, 8].Value?.ToString()?.Trim();
        //                     var religion = workSheet.Cells[row, 9].Value?.ToString()?.Trim();
        //                     var numberPhone = workSheet.Cells[row, 10].Value?.ToString()?.Trim();

        //                     // Find class
        //                     var lop = await _context.QLLopHocs.FirstOrDefaultAsync(l => l.ClassName == className);
        //                     if (lop == null) continue;

        //                     // Find student by StudentID
        //                     var existingStudent = await _context.QLHocSinhs
        //                         .Include(s => s.HocSinhLopHocs)
        //                         .FirstOrDefaultAsync(s => s.StudentID == studentId);

        //                     if (existingStudent != null)
        //                     {
        //                         // Update student info
        //                         existingStudent.FullName = fullName;
        //                         existingStudent.Birth = birth;
        //                         existingStudent.Gender = gender;
        //                         existingStudent.Hamlet = hamlet;
        //                         existingStudent.Commune = commune;
        //                         existingStudent.Province = province;
        //                         existingStudent.Address = address;
        //                         existingStudent.Nationality = "Việt Nam";
        //                         existingStudent.Nation = nation;
        //                         existingStudent.Religion = religion;
        //                         existingStudent.NumberPhone = numberPhone;

        //                         // Update class relationship
        //                         var existingRelation = existingStudent.HocSinhLopHocs
        //                             .FirstOrDefault(hsl => hsl.IsActive);

        //                         if (existingRelation != null && existingRelation.ClassID != lop.ClassID)
        //                         {
        //                             // Deactivate current relation
        //                             existingRelation.IsActive = false;
        //                             _context.Update(existingRelation);

        //                             // Create new relation
        //                             var newRelation = new QLHocSinhLopHoc
        //                             {
        //                                 StudentID = existingStudent.StudentID,
        //                                 ClassID = lop.ClassID,
        //                                 IsActive = true
        //                             };
        //                             newClassRelations.Add(newRelation);
        //                         }
        //                         else if (existingRelation == null)
        //                         {
        //                             // Create new relation if none exists
        //                             var newRelation = new QLHocSinhLopHoc
        //                             {
        //                                 StudentID = existingStudent.StudentID,
        //                                 ClassID = lop.ClassID,
        //                                 IsActive = true
        //                             };
        //                             newClassRelations.Add(newRelation);
        //                         }
        //                     }
        //                     else
        //                     {
        //                         var newStudent = new QLHocSinh
        //                         {
        //                             StudentID = studentId,
        //                             FullName = fullName,
        //                             Birth = birth,
        //                             Gender = gender,
        //                             Hamlet = hamlet,
        //                             Commune = commune,
        //                             Province = province,
        //                             Address = address,
        //                             Nationality = "Việt Nam",
        //                             Nation = nation,
        //                             Religion = religion,
        //                             StatusStudent = "Đang học",
        //                             NumberPhone = numberPhone,
        //                             Images = null,
        //                         };

        //                         newStudentList.Add(newStudent);

        //                         // Create class relationship
        //                         var newRelation = new QLHocSinhLopHoc
        //                         {
        //                             StudentID = studentId,
        //                             ClassID = lop.ClassID,
        //                             IsActive = true
        //                         };
        //                         newClassRelations.Add(newRelation);
        //                     }
        //                 }

        //                 if (newStudentList.Count > 0)
        //                 {
        //                     await _context.QLHocSinhs.AddRangeAsync(newStudentList);
        //                 }

        //                 if (newClassRelations.Count > 0)
        //                 {
        //                     await _context.QLHocSinhLopHocs.AddRangeAsync(newClassRelations);
        //                 }

        //                 await _context.SaveChangesAsync();
        //             }
        //         }
        //     }
        //     return RedirectToAction("Index");
        // }

        public void LoadData()
        {
            var clList = _context.Classes
                .Include(cl => cl.grade)
                .Include(cl => cl.cohort)
                .Where(cl => cl.IsActive == true)
                .Select(cl => new
                {
                    cl.ClassID,
                    ThongTin = cl.ClassName +
                               " | Khối: " + (cl.grade != null ? cl.grade.GradeName : "N/A") +
                               " | Khóa học: " + (cl.cohort != null
                                                  ? (cl.cohort.StartYear + "-" + cl.cohort.EndYear)
                                                  : "N/A")
                })
                .ToList();

            ViewBag.clist = new SelectList(clList, "ClassID", "Info");
        }

        // public IActionResult ThongKe()
        // {
        //     try
        //     {
        //         var now = DateTime.Now;

        //         var totalStudents = _context.QLHocSinhs
        //             .Count(s => s.HocSinhLopHocs.Any(hsl => hsl.IsActive));

        //         var allEthnicStudents = _context.QLHocSinhs
        //             .Count(s => !string.IsNullOrEmpty(s.Nation) && s.HocSinhLopHocs.Any(hsl => hsl.IsActive));

        //         var religionStats = _context.QLHocSinhs
        //             .Where(s => !string.IsNullOrEmpty(s.Religion) && s.HocSinhLopHocs.Any(hsl => hsl.IsActive))
        //             .GroupBy(s => s.Religion)
        //             .ToDictionary(g => g.Key, g => g.Count());

        //         var statusStats = _context.QLHocSinhs
        //             .Where(s => !string.IsNullOrEmpty(s.StatusStudent) && s.HocSinhLopHocs.Any(hsl => hsl.IsActive))
        //             .GroupBy(s => s.StatusStudent)
        //             .ToDictionary(g => g.Key, g => g.Count());

        //         var genderStats = _context.QLHocSinhs
        //             .Where(s => !string.IsNullOrEmpty(s.Gender) && s.HocSinhLopHocs.Any(hsl => hsl.IsActive))
        //             .GroupBy(s => s.Gender)
        //             .ToDictionary(g => g.Key, g => g.Count());

        //         var ageStats = _context.QLHocSinhs
        //             .Where(s => s.Birth != null && s.HocSinhLopHocs.Any(hsl => hsl.IsActive))
        //             .AsEnumerable()
        //             .GroupBy(s =>
        //             {
        //                 var age = now.Year - s.Birth.Value.Year;
        //                 if (s.Birth.Value.Date > now.AddYears(-age)) age--;
        //                 return (age / 5) * 5;
        //             })
        //             .Select(g => new { AgeRange = $"{g.Key}-{g.Key + 3}", Count = g.Count() })
        //             .OrderBy(g => g.AgeRange)
        //             .ToDictionary(g => g.AgeRange, g => g.Count);

        //         var provinceStats = _context.QLHocSinhs
        //             .Where(s => !string.IsNullOrEmpty(s.Province) && s.HocSinhLopHocs.Any(hsl => hsl.IsActive))
        //             .GroupBy(s => s.Province)
        //             .Select(g => new { Province = g.Key, Count = g.Count() })
        //             .OrderByDescending(g => g.Count)
        //             .ToDictionary(g => g.Province, g => g.Count);

        //         var districtStats = _context.QLHocSinhs
        //             .Where(s => !string.IsNullOrEmpty(s.Commune) && s.HocSinhLopHocs.Any(hsl => hsl.IsActive))
        //             .GroupBy(s => s.Commune)
        //             .Select(g => new { Commune = g.Key, Count = g.Count() })
        //             .OrderByDescending(g => g.Count)
        //             .ToDictionary(g => g.Commune, g => g.Count);

        //         ViewBag.TotalStudents = totalStudents;
        //         ViewBag.MinorityStudents = allEthnicStudents;
        //         ViewBag.Religions = religionStats;
        //         ViewBag.StudyStatus = statusStats;
        //         ViewBag.GenderStats = genderStats;
        //         ViewBag.AgeStats = ageStats;
        //         ViewBag.ProvinceStats = provinceStats;
        //         ViewBag.CommuneStats = districtStats;

        //         return View();
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, "Đã xảy ra lỗi khi thống kê.");
        //     }
        // }

        // [HttpGet]
        // public IActionResult StudentListPartial(
        //     string filter,
        //     string religionFilter = null,
        //     string statusFilter = null,
        //     string genderFilter = null,
        //     string provinceFilter = null,
        //     string districtFilter = null,
        //     string ageRangeFilter = null)
        // {
        //     try
        //     {
        //         IQueryable<QLHocSinh> studentsQuery = _context.QLHocSinhs
        //             .Where(s => s.HocSinhLopHocs.Any(hsl => hsl.IsActive));

        //         switch (filter?.ToLower())
        //         {
        //             case "all":
        //                 break;

        //             case "minority":
        //                 studentsQuery = studentsQuery.Where(s => !string.IsNullOrEmpty(s.Nation));
        //                 break;

        //             case "religion":
        //                 if (!string.IsNullOrEmpty(religionFilter))
        //                     studentsQuery = studentsQuery.Where(s => s.Religion == religionFilter);
        //                 break;

        //             case "status":
        //                 if (!string.IsNullOrEmpty(statusFilter))
        //                     studentsQuery = studentsQuery.Where(s => s.StatusStudent == statusFilter);
        //                 break;

        //             case "gender":
        //                 if (!string.IsNullOrEmpty(genderFilter))
        //                     studentsQuery = studentsQuery.Where(s => s.Gender == genderFilter);
        //                 break;

        //             case "province":
        //                 if (!string.IsNullOrEmpty(provinceFilter))
        //                     studentsQuery = studentsQuery.Where(s => s.Province == provinceFilter);
        //                 break;

        //             case "district":
        //                 if (!string.IsNullOrEmpty(districtFilter))
        //                     studentsQuery = studentsQuery.Where(s => s.Commune == districtFilter);
        //                 break;

        //             case "age":
        //                 if (!string.IsNullOrEmpty(ageRangeFilter))
        //                 {
        //                     var parts = ageRangeFilter.Split('-');
        //                     if (parts.Length == 2 && int.TryParse(parts[0], out int minAge) && int.TryParse(parts[1], out int maxAge))
        //                     {
        //                         var minBirth = DateTime.Now.AddYears(-maxAge - 1);
        //                         var maxBirth = DateTime.Now.AddYears(-minAge);
        //                         studentsQuery = studentsQuery.Where(s => s.Birth != null &&
        //                             s.Birth >= minBirth && s.Birth <= maxBirth);
        //                     }
        //                 }
        //                 break;

        //             default:
        //                 return BadRequest("Filter không hợp lệ");
        //         }

        //         var students = studentsQuery.Select(s => new
        //         {
        //             s.StudentID,
        //             s.FullName,
        //             Birth = s.Birth.HasValue ? s.Birth.Value.ToString("dd/MM/yyyy") : "Không rõ",
        //             s.Nation,
        //             s.Religion,
        //             s.StatusStudent,
        //             s.Gender,
        //             s.Province,
        //             s.Commune
        //         }).ToList();

        //         return Json(students);
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, "Đã xảy ra lỗi khi truy vấn danh sách học sinh.");
        //     }
        // }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound();
            student.IsActive = !student.IsActive;
            _context.Update(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // public IActionResult StudentCard(int? classId, string searchTerm)
        // {
        //     var students = _context.QLHocSinhs
        //         .Include(s => s.HocSinhLopHocs)
        //             .ThenInclude(hsl => hsl.lopHoc)
        //                 .ThenInclude(l => l.KhoaHoc)
        //         .Where(s => s.IsActive && s.HocSinhLopHocs.Any(hsl => hsl.IsActive))
        //         .AsQueryable();

        //     if (!string.IsNullOrWhiteSpace(searchTerm))
        //     {
        //         students = students.Where(s =>
        //             s.StudentID.Contains(searchTerm) ||
        //             s.FullName.Contains(searchTerm));

        //         if (classId.HasValue && classId.Value >= 0)
        //         {
        //             students = students.Where(s => s.HocSinhLopHocs.Any(hsl => hsl.ClassID == classId.Value && hsl.IsActive));
        //         }
        //     }
        //     else if (classId.HasValue && classId.Value > 0)
        //     {
        //         students = students.Where(s => s.HocSinhLopHocs.Any(hsl => hsl.ClassID == classId.Value && hsl.IsActive));
        //     }

        //     var classList = _context.QLLopHocs
        //         .Include(l => l.KhoaHoc)
        //         .Where(l => l.IsActive)
        //         .AsEnumerable() // để GroupBy dùng các giá trị đã load
        //         .GroupBy(l => new
        //         {
        //             l.ClassName,
        //             StartYear = l.KhoaHoc != null ? l.KhoaHoc.StartYear : (int?)null,
        //             EndYear = l.KhoaHoc != null ? l.KhoaHoc.EndYear : (int?)null
        //         })
        //         .Select(g => new SelectListItem
        //         {
        //             Value = g.First().ClassID.ToString(), // lấy ClassID đầu tiên trong nhóm
        //             Text = g.Key.ClassName + (g.Key.StartYear != null ? " | " + g.Key.StartYear + "-" + g.Key.EndYear : "")
        //         })
        //         .OrderBy(x => x.Text)
        //         .ToList();


        //     ViewBag.ClassList = classList;
        //     ViewBag.SelectedClassId = classId?.ToString() ?? "";


        //     ViewBag.ClassList = classList;
        //     ViewBag.SelectedClassId = classId?.ToString() ?? "";
        //     ViewBag.SearchTerm = searchTerm;

        //     return View(students.ToList());
        // }

        // public IActionResult ExportClassCardsToImages(int? classId, string searchTerm)
        // {
        //     var studentsQuery = _context.QLHocSinhs
        //         .Include(s => s.HocSinhLopHocs)
        //             .ThenInclude(hsl => hsl.lopHoc)
        //                 .ThenInclude(l => l.KhoaHoc)
        //         .Where(s => s.HocSinhLopHocs.Any(hsl => hsl.IsActive))
        //         .AsQueryable();

        //     if (!string.IsNullOrEmpty(searchTerm))
        //     {
        //         var exactStudent = studentsQuery.FirstOrDefault(s => s.StudentID.ToLower() == searchTerm.ToLower());

        //         if (exactStudent != null)
        //         {
        //             studentsQuery = studentsQuery.Where(s => s.ID == exactStudent.ID);
        //         }
        //         else
        //         {
        //             studentsQuery = studentsQuery.Where(s =>
        //                 s.StudentID.ToLower().Contains(searchTerm.ToLower()) ||
        //                 s.FullName.ToLower().Contains(searchTerm.ToLower()));
        //         }
        //     }
        //     else if (classId.HasValue)
        //     {
        //         studentsQuery = studentsQuery.Where(s => s.HocSinhLopHocs.Any(hsl => hsl.ClassID == classId && hsl.IsActive));
        //     }

        //     var students = studentsQuery.ToList();
        //     if (!students.Any()) return NotFound("Không có học sinh để xuất thẻ.");

        //     int cardWidth = 540;
        //     int cardHeight = 320;
        //     using var zipStream = new MemoryStream();
        //     using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        //     {
        //         foreach (var student in students)
        //         {
        //             var currentClass = student.HocSinhLopHocs.FirstOrDefault(hsl => hsl.IsActive)?.lopHoc;

        //             using var bitmap = new Bitmap(cardWidth, cardHeight);
        //             using var graphics = Graphics.FromImage(bitmap);

        //             // Fill background and border
        //             var gradientBrush = new LinearGradientBrush(
        //                 new Point(0, 0),
        //                 new Point(cardWidth, cardHeight),
        //                 ColorTranslator.FromHtml("#F0F8FF"),
        //                 ColorTranslator.FromHtml("#D9EDFF"));
        //             graphics.FillRectangle(gradientBrush, 0, 0, cardWidth, cardHeight);
        //             using var borderPen = new Pen(ColorTranslator.FromHtml("#0056B3"), 2.5f);
        //             graphics.DrawRectangle(borderPen, 1, 1, cardWidth - 3, cardHeight - 3);

        //             // Logo
        //             string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "admin", "assets", "img", "logo.jpg");
        //             if (System.IO.File.Exists(logoPath))
        //             {
        //                 using var logoOriginal = Image.FromFile(logoPath);
        //                 using var logo = new Bitmap(55, 55);
        //                 using (var gLogo = Graphics.FromImage(logo))
        //                 {
        //                     gLogo.Clear(Color.Transparent);
        //                     gLogo.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //                     gLogo.DrawImage(logoOriginal, 0, 0, 55, 55);
        //                 }
        //                 logo.MakeTransparent(Color.White);
        //                 graphics.DrawImage(logo, 20, 20, 55, 55);
        //             }

        //             // Header text
        //             using var fontSchoolHeader = new Font("Arial", 16, FontStyle.Bold);
        //             var brushSchoolHeader = new SolidBrush(ColorTranslator.FromHtml("#003366"));
        //             var text1 = "BỘ GIÁO DỤC VÀ ĐÀO TẠO";
        //             var text2 = "TRƯỜNG THPT ANH SƠN I";
        //             var sizeText1 = graphics.MeasureString(text1, fontSchoolHeader);
        //             var sizeText2 = graphics.MeasureString(text2, fontSchoolHeader);
        //             float startY = 25f;
        //             graphics.DrawString(text1, fontSchoolHeader, brushSchoolHeader, new PointF((cardWidth - sizeText1.Width) / 2, startY));
        //             graphics.DrawString(text2, fontSchoolHeader, brushSchoolHeader, new PointF((cardWidth - sizeText2.Width) / 2, startY + sizeText1.Height - 4));

        //             // QR Code
        //             string qrDataString = $"Mã HS: {student.StudentID}, Họ tên: {student.FullName}, Ngày sinh: {student.Birth?.ToString("dd/MM/yyyy")}, Lớp: {currentClass?.ClassName}, Dân tộc: {student.Nation}, Tôn giáo: {student.Religion}, SĐT: {student.NumberPhone}";
        //             using var qrGenerator = new QRCodeGenerator();
        //             using var qrCodeData = qrGenerator.CreateQrCode(qrDataString, QRCodeGenerator.ECCLevel.Q);
        //             using var qrCode = new PngByteQRCode(qrCodeData);
        //             byte[] qrCodeBytes = qrCode.GetGraphic(3);
        //             using var msQr = new MemoryStream(qrCodeBytes);
        //             using var qrImage = Image.FromStream(msQr);
        //             var qrRect = new Rectangle(cardWidth - 75, 20, 55, 55);
        //             graphics.FillRectangle(Brushes.White, qrRect);
        //             graphics.DrawRectangle(new Pen(ColorTranslator.FromHtml("#0056B3")), qrRect);
        //             graphics.DrawImage(qrImage, qrRect.X + 2, qrRect.Y + 2, qrRect.Width - 4, qrRect.Height - 4);

        //             // Separator line
        //             using var fontSeparator = new Font("Arial", 12, FontStyle.Bold);
        //             string sepText = new string('-', 80);
        //             var sepSize = graphics.MeasureString(sepText, fontSeparator);
        //             graphics.DrawString(sepText, fontSeparator, new SolidBrush(ColorTranslator.FromHtml("#64B5F6")), new PointF((cardWidth - sepSize.Width) / 2, 85));

        //             // Title
        //             using var fontTitle = new Font("Arial", 18, FontStyle.Bold);
        //             var titleSize = graphics.MeasureString("THẺ HỌC SINH", fontTitle);
        //             graphics.DrawString("THẺ HOC SINH", fontTitle, new SolidBrush(ColorTranslator.FromHtml("#004D40")), new PointF((cardWidth - titleSize.Width) / 2, 100));

        //             // Photo
        //             var photoRect = new Rectangle(40, 145, 100, 120);
        //             graphics.FillRectangle(Brushes.White, photoRect);
        //             graphics.DrawRectangle(new Pen(ColorTranslator.FromHtml("#42A5F5")), photoRect);

        //             if (!string.IsNullOrEmpty(student.Images))
        //             {
        //                 var relativePath = student.Images.TrimStart('/');
        //                 var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

        //                 if (System.IO.File.Exists(imagePath))
        //                 {
        //                     try
        //                     {
        //                         using var photo = Image.FromFile(imagePath);
        //                         graphics.DrawImage(photo, photoRect);
        //                     }
        //                     catch
        //                     {
        //                         DrawPlaceholder(graphics, photoRect, "Ảnh lỗi");
        //                     }
        //                 }
        //                 else
        //                 {
        //                     DrawPlaceholder(graphics, photoRect, "Chưa có ảnh");
        //                 }
        //             }
        //             else
        //             {
        //                 DrawPlaceholder(graphics, photoRect, "Ảnh");
        //             }

        //             using var fontId = new Font("Arial", 11, FontStyle.Bold);
        //             graphics.DrawString($"MHS: {student.StudentID}", fontId, Brushes.Black, new PointF(photoRect.X, photoRect.Bottom + 10));

        //             float infoX = photoRect.Right + 40;
        //             float infoY = photoRect.Y + 10;
        //             float lineHeight = 34;

        //             using var fontLabel = new Font("Arial", 14, FontStyle.Bold);
        //             using var fontValue = new Font("Arial", 14);

        //             // Get cohort info from current class
        //             string khoaText = "";
        //             if (currentClass?.KhoaHoc != null)
        //             {
        //                 var khoaHoc = currentClass.KhoaHoc;
        //                 khoaText = $"Khóa: {khoaHoc.Cohort} ({khoaHoc.StartYear} - {khoaHoc.EndYear})";
        //             }
        //             else
        //             {
        //                 khoaText = "Khóa: Không xác định";
        //             }

        //             DrawDetail(graphics, "Họ tên:", student.FullName ?? "", infoX, infoY, fontLabel, fontValue); infoY += lineHeight;
        //             DrawDetail(graphics, "Ngày sinh:", student.Birth?.ToString("dd/MM/yyyy") ?? "", infoX, infoY, fontLabel, fontValue); infoY += lineHeight;
        //             DrawDetail(graphics, "Lớp:", currentClass?.ClassName ?? "", infoX, infoY, fontLabel, fontValue); infoY += lineHeight;
        //             DrawDetail(graphics, "Khóa:", khoaText, infoX, infoY, fontLabel, fontValue); infoY += lineHeight;

        //             string entryPath;
        //             if (!string.IsNullOrEmpty(searchTerm) && students.Count == 1)
        //             {
        //                 entryPath = $"TheHocSinh_{student.StudentID}_{student.FullName}.png";
        //             }
        //             else if (!classId.HasValue)
        //             {
        //                 string classFolder = $"Lop_{currentClass?.ClassName ?? "KhongXacDinh"}";
        //                 entryPath = $"{classFolder}/TheHocSinh_{student.StudentID}.png";
        //             }
        //             else
        //             {
        //                 entryPath = $"TheHocSinh_{student.StudentID}.png";
        //             }

        //             var entry = archive.CreateEntry(entryPath);
        //             using var entryStream = entry.Open();
        //             bitmap.Save(entryStream, System.Drawing.Imaging.ImageFormat.Png);
        //         }
        //     }

        //     zipStream.Position = 0;
        //     string fileName;

        //     if (!string.IsNullOrEmpty(searchTerm) && students.Count == 1)
        //     {
        //         var student = students[0];
        //         fileName = $"TheHocSinh_{student.StudentID}_{student.FullName}.zip";
        //     }
        //     else if (classId.HasValue)
        //     {
        //         var className = _context.QLLopHocs
        //             .Where(l => l.ClassID == classId)
        //             .Select(l => l.ClassName)
        //             .FirstOrDefault() ?? $"Lop_{classId}";

        //         foreach (var c in Path.GetInvalidFileNameChars())
        //             className = className.Replace(c, '_');

        //         fileName = $"TheHocSinh_Lop_{className}.zip";
        //     }
        //     else
        //     {
        //         fileName = "TheHocSinhTatCa.zip";
        //     }

        //     return File(zipStream.ToArray(), "application/zip", fileName);
        // }

        // private void DrawPlaceholder(Graphics graphics, Rectangle rect, string text)
        // {
        //     graphics.FillRectangle(Brushes.LightGray, rect);
        //     using var font = new Font("Arial", 12);
        //     var textSize = graphics.MeasureString(text, font);
        //     graphics.DrawString(text, font, Brushes.Gray,
        //         rect.X + (rect.Width - textSize.Width) / 2,
        //         rect.Y + (rect.Height - textSize.Height) / 2);
        // }

        // private void DrawDetail(Graphics graphics, string label, string value, float x, float y, Font labelFont, Font valueFont)
        // {
        //     if (!string.IsNullOrEmpty(label))
        //     {
        //         graphics.DrawString(label, labelFont, Brushes.Black, new PointF(x, y));
        //         x += graphics.MeasureString(label, labelFont).Width;
        //     }
        //     graphics.DrawString(value, valueFont, Brushes.Black, new PointF(x, y));
        // }

        // public async Task<IActionResult> Search(string query)
        // {
        //     if (string.IsNullOrEmpty(query))
        //     {
        //         var allStudents = await _context.QLHocSinhs
        //             .Where(s => s.HocSinhLopHocs.Any(hsl => hsl.IsActive))
        //             .ToListAsync();
        //         return View(allStudents);
        //     }

        //     var results = await _context.QLHocSinhs
        //         .Include(s => s.HocSinhLopHocs)
        //             .ThenInclude(hsl => hsl.lopHoc)
        //         .Where(s => (s.StudentID.ToLower().Contains(query.ToLower()) ||
        //                    s.FullName.ToLower().Contains(query.ToLower())) &&
        //                    s.HocSinhLopHocs.Any(hsl => hsl.IsActive))
        //         .ToListAsync();

        //     return View(results);
        // }
    }
}