
using Microsoft.AspNetCore.Mvc;
using RagBasedChatbot.Data;
using RagBasedChatbot.Models.Nothwind;
using RagBasedChatbot.Helpers;
using Microsoft.EntityFrameworkCore;
namespace RagBasedChatbot.Controllers
{
    public class DatabaseController : Controller
    {
        private readonly NorthwindDbContext _context;
        public int countNewOrders;

        public DatabaseController(NorthwindDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var dataHistory = HttpContext.Session.GetObject<List<int>>("DataHistory")
                ?? new List<int>();
            countNewOrders = dataHistory.Count();
            var orders = _context.Orders.ToList();
            return View((orders, countNewOrders));
        }

        [HttpGet]
        public IActionResult Orders()
        {

            var orders = _context.Orders.ToList();
            return View(orders);
        }


        [HttpGet]
        public IActionResult SendOrder()
        {


            return View();
        }

        [HttpPost]
        public IActionResult SendOrder(IFormCollection form)
        {

            var main_orders = _context.Orders.ToList();
            Order order = new Order();
            order.OrderId = (short)(main_orders[^1].OrderId + 1);

            order.CustomerId = form["customerId"];

            if (short.TryParse(form["employeeId"], out short employeeId))
                order.EmployeeId = employeeId;

            if (DateTime.TryParse(form["orderDate"], out DateTime orderDate))
                order.OrderDate = DateOnly.FromDateTime(orderDate);

            if (DateTime.TryParse(form["requiredDate"], out DateTime requiredDate))
                order.RequiredDate = DateOnly.FromDateTime(requiredDate);

            if (DateTime.TryParse(form["shippedDate"], out DateTime shippedDate))
                order.ShippedDate = DateOnly.FromDateTime(shippedDate);

            if (float.TryParse(form["freight"], out float freight))
                order.Freight = freight;

            if (short.TryParse(form["shipVia"], out short shipVia))
                order.ShipVia = shipVia;

            order.ShipName = form["shipName"];
            order.ShipAddress = form["shipAddress"];
            order.ShipCity = form["shipCity"];
            order.ShipRegion = form["shipRegion"];
            order.ShipPostalCode = form["shipPostalCode"];
            order.ShipCountry = form["shipCountry"];

            _context.Orders.Add(order);
            _context.SaveChanges();

            var dataHistory = HttpContext.Session.GetObject<List<int>>("DataHistory")
                            ?? new List<int>();
            dataHistory.Add(1);

            HttpContext.Session.SetObject("DataHistory", dataHistory);
            return View();


        }
        [HttpPost]
        public IActionResult DeleteOrder(int orderId)
        {

            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
            {
                return NotFound();
            }
            
            _context.OrderDetails.RemoveRange(order.OrderDetails);
            _context.Orders.Remove(order);


            _context.SaveChanges();


            return RedirectToAction("Orders");
        }



        // [HttpGet]
        // public IActionResult SendOrder(List<Order> order)
        // {

        //     var orders = _context.Orders.ToList();
        //     return View(orders);
        // }
    }
}