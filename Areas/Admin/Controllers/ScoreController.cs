using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace StudentManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ScoreController : Controller
    {
        private readonly DataContext _context;

        public ScoreController(DataContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Truy vấn điểm kèm học sinh và bảng trung gian
            var query = _context.Scores
                .Include(d => d.subject)
                .Include(d => d.yearSemester)
                .Include(d => d.student)
                    .ThenInclude(sc => sc.studentclass.Where(stc => stc.IsActive))
                        .ThenInclude(stc => stc._class)
                            .ThenInclude(c => c.grade)
            // Nếu bạn cũng cần Cohort
                .Include(d => d.student)
                    .ThenInclude(sc => sc.studentclass.Where(stc => stc.IsActive))
                        .ThenInclude(stc => stc._class)
                            .ThenInclude(kh => kh.cohort)
                .AsQueryable();

            var stlist = await query.OrderBy(d => d.student.StudentID).ToListAsync();

            // Trả trực tiếp List<tblScore> để view nhận đúng model
            return View(stlist);
        }

        public IActionResult Create()
        {
            LoadDropdownLists();
            return View();
        }

        [HttpPost]
        public IActionResult Create(tblScore sr)
        {
            if (ModelState.IsValid)
            {
                // Thêm kiểm tra trùng lặp tại đây
                var existingScore = _context.Scores
                    .FirstOrDefault(s => s.StudentID == sr.StudentID &&
                                         s.SubjectID == sr.SubjectID &&
                                         s.YearSemesterID == sr.YearSemesterID);

                if (existingScore != null)
                {
                    // Tìm thông tin để hiển thị lỗi chi tiết hơn
                    var student = _context.Students.FirstOrDefault(st => st.StudentID == sr.StudentID);
                    var subject = _context.Subjects.FirstOrDefault(sub => sub.SubjectID == sr.SubjectID);
                    var yearSemester = _context.YearSemesters.FirstOrDefault(ys => ys.YearSemesterID == sr.YearSemesterID);

                    string studentName = student?.FullName ?? sr.StudentID;
                    string subjectName = subject?.SubjectName ?? "môn học này";
                    string semesterInfo = yearSemester != null ? $"{yearSemester.SemesterName} - Năm học {yearSemester.SchoolYear}" : "học kỳ này";

                    // Thêm lỗi vào ModelState
                    ModelState.AddModelError("", $"Lỗi: Học sinh **{studentName}** đã có điểm cho **{subjectName}** trong **{semesterInfo}**. Vui lòng nhập lại thông tin để thêm mới điểm.");

                    // Tải lại Dropdown Lists
                    LoadDropdownLists();
                    return View(sr);
                }

                // **THÊM BƯỚC KIỂM TRA MỚI: KIỂM TRA ĐĂNG KÝ LỚP HỌC**
                var studentClassEntry = _context.StudentClasses
                          .FirstOrDefault(sc => sc.StudentID == sr.StudentID &&
                                     sc.YearSemesterID == sr.YearSemesterID &&
                                     sc.IsActive);

                if (studentClassEntry == null)
                {
                    var student = _context.Students.FirstOrDefault(st => st.StudentID == sr.StudentID);
                    var yearSemester = _context.YearSemesters.FirstOrDefault(ys => ys.YearSemesterID == sr.YearSemesterID);
                    string studentName = student?.FullName ?? sr.StudentID;
                    string semesterInfo = yearSemester != null ? $"{yearSemester.SemesterName} - Năm học {yearSemester.SchoolYear}" : "học kỳ được chọn";

                    ModelState.AddModelError("", $"Lỗi: Học sinh **{studentName}** chưa được đăng ký vào bất kỳ lớp học nào trong **{semesterInfo}**. Vui lòng kiểm tra lại đăng ký học sinh - lớp học.");
                    LoadDropdownLists();
                    return View(sr);
                }

                // === BƯỚC 1: TÍNH ĐIỂM TB ĐÁNH GIÁ THƯỜNG XUYÊN (Average_CA_Score) ===
                var caScores = new List<double>();
                if (sr.OralScore1.HasValue) caScores.Add(sr.OralScore1.Value);
                if (sr.OralScore2.HasValue) caScores.Add(sr.OralScore2.Value);
                if (sr.Score15Minute1.HasValue) caScores.Add(sr.Score15Minute1.Value);
                if (sr.Score15Minute2.HasValue) caScores.Add(sr.Score15Minute2.Value);

                if (caScores.Any())
                {
                    sr.Average_CA_Score = Math.Round(caScores.Average(), 1);
                }
                else
                {
                    sr.Average_CA_Score = null;
                }

                // === BƯỚC 2: TÍNH ĐIỂM TRUNG BÌNH MÔN HỌC KỲ (AverageScore) VÀ XẾP LOẠI ===
                // Chỉ tính khi có đủ 3 loại điểm chính (TB ĐGTX, Giữa kỳ, Cuối kỳ)
                if (sr.Average_CA_Score.HasValue && sr.MidtermScore.HasValue && sr.FinalScore.HasValue)
                {
                    // Công thức: (TB_ĐGTX*1 + Giữa kỳ*2 + Cuối kỳ*3) / 6
                    double numerator = (sr.Average_CA_Score.Value * 1) +
                                       (sr.MidtermScore.Value * 2) +
                                       (sr.FinalScore.Value * 3);
                    double avg = numerator / 6;

                    // Làm tròn kết quả cuối cùng (thường là làm tròn đến 1 chữ số thập phân)
                    sr.AverageScore = Math.Round(avg, 1);

                    // Xếp loại Học lực
                    if (sr.AverageScore.Value < 5)
                        sr.AcademicRating = "Yếu";
                    else if (sr.AverageScore.Value < 6.5)
                        sr.AcademicRating = "Trung bình";
                    else if (sr.AverageScore.Value < 8)
                        sr.AcademicRating = "Khá";
                    else if (sr.AverageScore.Value < 9)
                        sr.AcademicRating = "Giỏi";
                    else
                        sr.AcademicRating = "Xuất sắc";
                }
                else
                {
                    // Nếu thiếu 1 trong 3 điểm chính, không tính TB Môn và không xếp loại
                    sr.AverageScore = null;
                    sr.AcademicRating = null;
                }

                sr.CreateDate = DateTime.Now;
                _context.Scores.Add(sr);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            LoadDropdownLists();
            return View(sr);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var score = _context.Scores.Find(id);

            if (score == null)
                return NotFound();

            LoadDropdownLists();
            // Trả về Model tblScore
            return View(score);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Nên thêm để bảo mật
        public IActionResult Edit(tblScore sr)
        {
            if (ModelState.IsValid)
            {
                // === BƯỚC 1: TÍNH ĐIỂM TB ĐÁNH GIÁ THƯỜNG XUYÊN (Average_CA_Score) ===
                // Logic này phải được lặp lại khi Edit để đảm bảo điểm TB luôn đúng
                var caScores = new List<double>();
                if (sr.OralScore1.HasValue) caScores.Add(sr.OralScore1.Value);
                if (sr.OralScore2.HasValue) caScores.Add(sr.OralScore2.Value);
                if (sr.Score15Minute1.HasValue) caScores.Add(sr.Score15Minute1.Value);
                if (sr.Score15Minute2.HasValue) caScores.Add(sr.Score15Minute2.Value);

                if (caScores.Any())
                {
                    sr.Average_CA_Score = Math.Round(caScores.Average(), 1);
                }
                else
                {
                    sr.Average_CA_Score = null;
                }

                // === BƯỚC 2: TÍNH ĐIỂM TRUNG BÌNH MÔN HỌC KỲ (AverageScore) VÀ XẾP LOẠI ===
                // Chỉ tính khi có đủ 3 loại điểm chính (TB ĐGTX, Giữa kỳ, Cuối kỳ)
                if (sr.Average_CA_Score.HasValue && sr.MidtermScore.HasValue && sr.FinalScore.HasValue)
                {
                    // Công thức mới: (TB_ĐGTX*1 + Giữa kỳ*2 + Cuối kỳ*3) / 6
                    double numerator = (sr.Average_CA_Score.Value * 1) +
                                         (sr.MidtermScore.Value * 2) +
                                         (sr.FinalScore.Value * 3);
                    double avg = numerator / 6;

                    // Làm tròn kết quả cuối cùng (1 chữ số thập phân)
                    sr.AverageScore = Math.Round(avg, 1);

                    // Xếp loại Học lực (AcademicRating)
                    if (sr.AverageScore.Value < 5)
                        sr.AcademicRating = "Yếu";
                    else if (sr.AverageScore.Value < 6.5)
                        sr.AcademicRating = "Trung bình";
                    else if (sr.AverageScore.Value < 8)
                        sr.AcademicRating = "Khá";
                    else if (sr.AverageScore.Value < 9)
                        sr.AcademicRating = "Giỏi";
                    else
                        sr.AcademicRating = "Xuất sắc";
                }
                else
                {
                    // Nếu thiếu 1 trong 3 điểm chính, không tính TB Môn và không xếp loại
                    sr.AverageScore = null;
                    sr.AcademicRating = null;
                }

                sr.CreateDate = DateTime.Now; // Cập nhật lại ngày
                _context.Scores.Update(sr); // Đảm bảo gọi Update trên DbContext
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            LoadDropdownLists();
            return View(sr);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var sc = _context.Scores
                             .Include(s => s.student) // Bao gồm thông tin Học sinh
                                                      // Nối tiếp để lấy thông tin Lớp học và Khối lớp
                                .ThenInclude(st => st.studentclass.Where(stc => stc.IsActive)) // Giả định lấy lớp đang hoạt động
                                    .ThenInclude(stc => stc._class)
                                        .ThenInclude(c => c.grade)
                             .Include(s => s.subject)
                             .Include(s => s.yearSemester)
                             .FirstOrDefault(s => s.ScoreID == id);

            if (sc == null)
                return NotFound();

            return View(sc);
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var delScore = _context.Scores.Find(id);
            if (delScore == null)
                return NotFound();

            _context.Scores.Remove(delScore);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        private void LoadDropdownLists()
        {
            var stList = _context.Students.Where(cl => cl.IsActive == true)
            .Select(st => new
            {
                st.StudentID,
                Info = st.StudentID + " - " + st.FullName
            }).ToList();
            ViewBag.stList = new SelectList(stList, "StudentID", "Info");

            var sbList = _context.Subjects
              .Select(sb => new
              {
                  sb.SubjectID,
                  Info = sb.SubjectID + " - " + sb.SubjectName
              }).ToList();
            ViewBag.sbList = new SelectList(sbList, "SubjectID", "Info");
            
            var yearSemesters = _context.YearSemesters
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.SchoolYear)
                .ThenBy(s => s.SemesterName)
                .Select(s => new
                {
                    s.YearSemesterID,
                    Info = s.SemesterName + " - " + s.SchoolYear
                }).ToList();

            ViewBag.YearSemesterList = new SelectList(yearSemesters, "YearSemesterID", "Info");

            ViewBag.ClassList = new SelectList(_context.Classes, "ClassID", "ClassName");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var score = await _context.Scores.FindAsync(id);
            if (score == null)
                return NotFound();

            score.IsActive = !score.IsActive;

            _context.Update(score);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}