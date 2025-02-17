using BookGuestAjax.Models;
using BookGuestAjax.Services;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using System.Diagnostics;

namespace BookGuestAjax.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository _repository;

        public HomeController(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _repository.GetAllMessagesAsync();
            return View(messages);
        }
        [HttpPost]
        public async Task<IActionResult> AddMessage([FromBody] MessageViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("Please log in to add a message.");
            }

            if (ModelState.IsValid)
            {
                var user = await _repository.GetUserByIdAsync(userId.Value);
                var newMessage = new Message
                {
                    Id_User = userId.Value,
                    Content = model.Content,
                    Email = model.Email,
                    WWW = model.WWW,
                    MessageDate = DateTime.Now,
                    User = user 
                };

                await _repository.AddMessageAsync(newMessage);

                var messages = await _repository.GetAllMessagesAsync();
                var html = RenderMessagesHtml(messages);
                return Content(html, "text/html");
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest("Invalid message data: " + string.Join(", ", errors));
        }
        [HttpGet]
        public async Task<IActionResult> GetUserMessages()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized("Please log in to view your messages.");
            }

            var messages = await _repository.GetMessagesByUserIdAsync(userId.Value);
            var html = RenderMessagesHtml(messages);
            return Content(html, "text/html");
        }

        private string RenderMessagesHtml(IEnumerable<Message> messages)
        {
            var html = string.Empty;
            foreach (var message in messages)
            {
                html += $@"
            <div class='card mb-3'>
                <div class='card-header'>
                    <strong>{message.User.Name}</strong>
                    <span class='float-right'>{message.MessageDate}</span>
                </div>
                <div class='card-body'>
                    <p class='card-text'>{message.Content}</p>
                    <p class='card-text'><small class='text-muted'>{message.Email} {message.WWW}</small></p>
                </div>
            </div>";
            }
            return html;
        }
    }
}