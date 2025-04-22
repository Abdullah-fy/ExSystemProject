using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;
using System.Threading.Tasks;
using ExSystemProject.Models;
using ExSystemProject.UnitOfWorks;
using ExSystemProject.ViewModels;
using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
public enum approles{
    admin, instructor, student,	 superadmin, supervisor
}
public enum Gender{
    M, F
}
namespace ExSystemProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UnitOfWork _unit;
        public AccountController(UnitOfWork _unit)
        {
            this._unit = _unit;  
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (loginVM == null || !ModelState.IsValid)
            {
                return RedirectToAction("Login", loginVM);
            }
            // this to get Active user by its name
            var user = _unit.userRepo.getActiveByName(loginVM.Username);
            if(user == null)
            {
                ModelState.AddModelError("", "Invalid User Name Or Password");
                return View(loginVM);
            }
            if (!BCrypt.Net.BCrypt.Verify(loginVM.password, user.Upassword))
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(loginVM);
            }

            //if(loginVM.password != user.Upassword)
            //{
            //    ModelState.AddModelError("", "Invalid User Name Or Password");
            //    return View(loginVM);
            //}
            var userAssignment = _unit.userAssignmentRepo.getById(user.UserId);
            //var instructor = _unit.instructorRepo.getById(user.UserId);
            //var student = _unit.studentRepo.getById(user.UserId);
            var student = _unit.studentRepo.getByUserId(user.UserId);
            var instructor = _unit.instructorRepo.getByUserId(user.UserId);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, loginVM.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            string branchId = null;
            string trackId = null;
            if (user.Role == "admin" || user.Role == "supervisor")
            {
                if (userAssignment?.Isactive == true)
                {
                    if (userAssignment.BranchId.HasValue)
                        branchId = userAssignment.BranchId.Value.ToString();
                    if (userAssignment.TrackId.HasValue)
                        trackId = userAssignment.TrackId.Value.ToString();
                }
            }
            else if (user.Role == "instructor" && instructor?.Isactive == true && instructor.TrackId.HasValue)
            {
                trackId = instructor.TrackId.Value.ToString();
                //claims.Add(new Claim("InsId", instructor.InsId.ToString()));
            }
            else if (user.Role == "student" && student?.Isactive == true && student.TrackId.HasValue)
            {
                trackId = student.TrackId.Value.ToString();
            }

            if (branchId != null)
                claims.Add(new Claim("BranchId", branchId));
            if (trackId != null)
                claims.Add(new Claim("TrackId", trackId));
            ClaimsIdentity ci   = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            // to keep login after close the browser
            var authonticatonProperty = new AuthenticationProperties
            {
                IsPersistent = loginVM.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(ci),
                    authonticatonProperty
            );
            switch (user.Role)
            {
                case "superadmin":
                    return RedirectToAction("Index", "Home");
                case "admin":
                    return RedirectToAction("Index", "Home");
                case "instructor":
                    return RedirectToAction("Index", "Home");
                case "student":
                    return RedirectToAction("Index", "Student");
                case "supervisor":
                    return RedirectToAction("Index", "Supervisor");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }
  
        
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize(Roles = "superadmin,admin,instructor")]
        public IActionResult Register()
        {
            RegisterViewModel Rvm = new RegisterViewModel();
            Rvm.Branches = _unit.branchRepo.getAll();
            Rvm.Tracks = _unit.trackRepo.getAll();
            return View(Rvm);
        }

        [HttpPost]
        [Authorize(Roles = "superadmin,admin,instructor")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (model.Role == "admin" || model.Role == "supervisor")
            {
                if (!model.BranchId.HasValue)
                    ModelState.AddModelError("BranchId", "Branch is required for Admin/Supervisor");
            }
            if (model.Role == "instructor" || model.Role == "student")
            {
                if (!model.TrackId.HasValue)
                    ModelState.AddModelError("TrackId", "Track is required for Instructor/Student");
            }
            if (model.Role == "instructor")
            {
                if (!model.Salary.HasValue)
                    ModelState.AddModelError("Salary", "Salary is required for Instructor");
            }
            model.Branches = _unit.branchRepo.getAll();
            model.Tracks = _unit.trackRepo.getAll();
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (_unit.userRepo.getActiveByName(model.Username) != null)
                {
                    ModelState.AddModelError("Username", "Username already exists");
                    return View(model);
                }
                if (_unit.userRepo.GetByEmail(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    return View(model);
                }

                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (currentUserRole != "superadmin")
                {
                    if (model.Role == "superadmin" || model.Role == "admin" || model.Role == "supervisor")
                    {
                        ModelState.AddModelError("", "Only Superadmin can register Superadmin, Admin, or Supervisor");
                        return View(model);
                    }
                    if (currentUserRole == "admin")
                    {
                        var currentBranchId = int.Parse(User.FindFirst("BranchId").Value);
                        if (model.BranchId.HasValue && model.BranchId != currentBranchId)
                        {
                            ModelState.AddModelError("BranchId", "You can only register users in your branch");
                            return View(model);
                        }
                    }
                }

                if (model.BranchId.HasValue && !_unit.branchRepo.Exists(model.BranchId.Value))
                {
                    ModelState.AddModelError("BranchId", "Invalid branch");
                    return View(model);
                }
                if (model.TrackId.HasValue && !_unit.trackRepo.Exists(model.TrackId.Value))
                {
                    ModelState.AddModelError("TrackId", "Invalid track");
                    return View(model);
                }

                string imagePath = null;
                if (model.ProfilePicture != null)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "users");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfilePicture.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(fileStream);
                    }
                    imagePath = "images/users/" + uniqueFileName;
                }

                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    Upassword = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    //Upassword = model.Password,
                    Gender = model.Gender,
                    Role = model.Role,
                    Img = imagePath,
                    Isactive = true
                };
                _unit.userRepo.add(user);
                _unit.save();

                if (model.Role == "admin" || model.Role == "supervisor")
                {
                    var userAssignment = new UserAssignment
                    {
                        UserId = user.UserId,
                        BranchId = model.BranchId,
                        TrackId = model.TrackId,
                        Isactive = true
                    };
                    _unit.userAssignmentRepo.add(userAssignment);
                }
                else if (model.Role == "instructor")
                {
                    var instructor = new Instructor
                    {
                        UserId = user.UserId,
                        TrackId = model.TrackId,
                        Salary = model.Salary.Value,
                        Isactive = true
                    };
                    _unit.instructorRepo.add(instructor);
                }
                else if (model.Role == "student")
                {
                    var student = new Student
                    {
                        UserId = user.UserId,
                        TrackId = model.TrackId,
                        EnrollmentDate = DateTime.Now,
                        Isactive = true
                    };
                    _unit.studentRepo.add(student);
                }

                _unit.save();

                switch (currentUserRole)
                {
                    case "superadmin":
                        return RedirectToAction("Index", "Home");
                    case "admin":
                        return RedirectToAction("Index", "Home");
                    case "instructor":
                        return RedirectToAction("Index", "Home");
                    default:
                        return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View(model);
            }
        }
    }
}
