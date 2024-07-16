using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookGuestAjax.Data;
using BookGuestAjax.Models;
using BookGuestAjax.Services;
using Microsoft.AspNetCore.Identity;
using NuGet.Protocol.Core.Types;
using System.Security.Policy;

namespace BookGuestAjax.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepository _repository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountController(IRepository repository, IPasswordHasher<User> passwordHasher)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public IActionResult Login() => PartialView("_LoginPartial");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_LoginPartial", model);
            }

            var user = await _repository.GetUserByNameAsync(model.Name);
            if (user != null && _passwordHasher.VerifyHashedPassword(user, user.Pwd, model.Password) == PasswordVerificationResult.Success)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                return Json(new { success = true, user = user.Name });
            }

            ViewBag.Error = "Неверный логин или пароль.";
            return PartialView("_LoginPartial", model);
        }

        [HttpGet]
        public IActionResult Register() => PartialView("_RegisterPartial");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_RegisterPartial", model);
            }

            var existingUser = await _repository.GetUserByNameAsync(model.Name);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Пользователь с таким логином уже существует.");
                return PartialView("_RegisterPartial", model);
            }

            var user = new User { Name = model.Name };
            user.Pwd = _passwordHasher.HashPassword(user, model.Password);
            await _repository.AddUserAsync(user);
            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserId");
            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }
    }
}