using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DojoActivityCenter.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DojoActivityCenter.Controllers
{
    public class HomeController : Controller
    {
        private DacContext context;
        private PasswordHasher<User> RegisterHasher = new PasswordHasher<User>();
        private PasswordHasher<LoginUser> LoginHasher = new PasswordHasher<LoginUser>();

        public HomeController(DacContext pc)
        {
            context = pc;
        }
        [HttpPost("register")]
        public IActionResult Register(User u)
        {
            if (ModelState.IsValid)
            {
                if (context.Users.Any(us => us.Email == u.Email))
                {
                    ModelState.AddModelError("Email", "Email already registered!");
                    return View("Index");
                }
                else
                {
                    u.Password = RegisterHasher.HashPassword(u, u.Password);
                    context.Users.Add(u);
                    context.SaveChanges();
                    HttpContext.Session.SetInt32("UserId", u.UserId);
                    return Redirect("/dashboard");
                }            
            }
            else{
                return View("Index");
            }
        }

        [HttpPost("login")]
        public IActionResult Login(LoginUser l)
        {
            if (ModelState.IsValid)
            {
                User logging_in_user = context.Users.FirstOrDefault(u => u.Email == l.LoginEmail);
                if (logging_in_user != null)
                {
                    var result = LoginHasher.VerifyHashedPassword(l, logging_in_user.Password, l.LoginPassword);
                    if (result == 0)
                    {
                        ModelState.AddModelError("LoginPassword", "Invalid Password");
                    }
                    else
                    {
                        HttpContext.Session.SetInt32("UserId", logging_in_user.UserId);
                        return Redirect("/dashboard");
                    }
                }
                else
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Email");
                }
            }
            return View("Index");
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            int? UserId = HttpContext.Session.GetInt32("UserId");
            if (UserId == null)
            {
                return Redirect("/");
            }
                List<Party> Parties = context.Parties
                .Include(p => p.Planner)
                .Include(p => p.AttendingUsers)
                .OrderBy(p=> p.PartyDate).ToList();
                for(int i =0; i<Parties.Count; i++)
                {
                    if(Parties[i].PartyDate < DateTime.Now)
                    {
                        Parties.Remove(Parties[i]);
                    }
                }
                ViewBag.Parties = Parties;
                ViewBag.UserId = UserId;
                return View("Dashboard");               
        }
        [Route("")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserId");
            return Redirect("/");
        }

        [HttpGet("party/new")]
        public IActionResult NewParty()
        {
            return View("NewParty");
        }

        [HttpPost("party")]
        public IActionResult CreateParty(Party p)
        {
            int? UserId = HttpContext.Session.GetInt32("UserId");
            if (UserId == null)
            {
                return Redirect("/");
            }
            if(ModelState.IsValid)
            {
                p.PlannerId = (int) UserId;
                context.Parties.Add(p);
                context.SaveChanges(); 
                return Redirect("/dashboard");
            }
            else{
                return View("NewParty", p);
            }
        }

        [HttpGet("delete/{PartyId}")]
        public IActionResult Delete(int PartyId)
        {
            Party p = context.Parties.FirstOrDefault(po => po.PartyId == PartyId);
            context.Parties.Remove(p);
            context.SaveChanges();
            return Redirect("/dashboard");
        }

        [HttpGet("view/{PartyId}")]
        public IActionResult ShowParty(int PartyId)
        {
            Party p = context.Parties
            .Include(po => po.Planner)
            .Include(po => po.AttendingUsers)
            .ThenInclude(po => po.Joiner)
            .FirstOrDefault(po => po.PartyId == PartyId);
            ViewBag.Joins = p.AttendingUsers;
            return View(p);
        }

        [HttpGet("edit/{PartyId}")]
        public IActionResult Edit(int PartyId)
        {
            Party par = context.Parties.FirstOrDefault(p => p.PartyId == PartyId);
            return View(par);
        }

        [HttpGet("join/{PartyId}")]
        public IActionResult Join(int PartyId)
        {
            int? UserId = HttpContext.Session.GetInt32("UserId");
            if (UserId == null)
            {
                return Redirect("/");
            }
            Join j = new Join()
            {
                UserId = (int)UserId,
                PartyId = PartyId
            };
            context.Joins.Add(j);
            context.SaveChanges();
            return Redirect("/dashboard");
        }

        [HttpGet("leave/{PartyId}")]
        public IActionResult Leave(int PartyId)
        {
            int? UserId = HttpContext.Session.GetInt32("UserId");
            if (UserId == null)
            {
                return Redirect("/");
            }
            Join join =context.Joins
            .Where(j=> j.PartyId == PartyId)
            .FirstOrDefault(j => j.UserId == (int) UserId);
            context.Joins.Remove(join);
            context.SaveChanges();
            return Redirect("/dashboard");
        }

        [HttpPost("update/{PartyId}")]
        public IActionResult Update(int PartyId, Party p)
        {
            if(ModelState.IsValid)
            {
                Party par = context.Parties.FirstOrDefault(po => po.PartyId == PartyId);
                par.PartyName = p.PartyName;
                par.PartyDate = p.PartyDate;
                par.Duration = p.Duration;
                par.Description = p.Description;
                par.TimeFormat = p.TimeFormat;
                context.SaveChanges();
                return Redirect("/dashboard");
            }
            else
            {
                return View("Edit", p);
            }
        }
    
    }
}
