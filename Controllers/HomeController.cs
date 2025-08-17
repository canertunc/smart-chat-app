using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RagBasedChatbot.Models;
using RagBasedChatbot.Data;

namespace RagBasedChatbot.Controllers
{
    public class HomeController : Controller
    {

        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var user = _context.Users.ToList();
            var chatMessages = _context.ChatMessages.ToList(); 
            return View((user,chatMessages));
        }

    }
}