
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
// using RagBasedChatbot.Data;
// using RagBasedChatbot.Helpers;
// using RagBasedChatbot.Models;

namespace RagBasedChatbot.Controllers
{
    public class HomeController : Controller
    {



        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.ScrollToBottom = true;

            return View();
        }


    }
}
