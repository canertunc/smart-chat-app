
using Microsoft.AspNetCore.Mvc;
using RagBasedChatbot.Data;
using RagBasedChatbot.Models.Nothwind;
using RagBasedChatbot.Helpers;
using Microsoft.EntityFrameworkCore;
using RagBasedChatbot.Models;
namespace RagBasedChatbot.Controllers
{
    public class ChatdbController : Controller
    {
        private readonly AppDbContext _context;
        public int countNewOrders;

        public ChatdbController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var ChatMessages = _context.ChatMessages.ToList();
            return View(ChatMessages);
        }


        [HttpGet]
        public IActionResult AddMessage()
        {

            return View();
        }

        [HttpPost]
        public IActionResult AddMessage(IFormCollection form)
        {

            var main_cm = _context.ChatMessages.ToList();
            ChatMessage cm = new ChatMessage();
            cm.MessageId = (short)(main_cm[^1].MessageId + 1);

            cm.QuestionMessage = form["questionMessage"];
            cm.AnswerMessage = form["answerMessage"];

            _context.ChatMessages.Add(cm);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteMessage(int messageId)
        {
            var chatMessage = _context.ChatMessages
                .FirstOrDefault(cm => cm.MessageId == messageId);
            if (chatMessage == null)
            {
                return NotFound();
            }

            _context.ChatMessages.Remove(chatMessage);


            _context.SaveChanges();


            return RedirectToAction("Index");
        }


    }
}