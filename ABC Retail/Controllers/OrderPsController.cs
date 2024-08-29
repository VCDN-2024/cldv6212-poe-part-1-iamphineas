using ABC_Retail.Models;
using ABC_Retail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class OrderPsController : Controller
    {
        private readonly TableStorageService _tableStorageService;
        private readonly QueueService _queueService;

        public OrderPsController(TableStorageService tableStorageService, QueueService queueService)
        {
            _tableStorageService = tableStorageService;
            _queueService = queueService;
        }

        // Action to display all orders
        public async Task<IActionResult> Index()
        {
            var orderProcesses = await _tableStorageService.GetAllOrderProcessesAsync();
            return View(orderProcesses);
        }

        // Action to display the form to register a new order
        public async Task<IActionResult> Register()
        {
            var customers = await _tableStorageService.GetAllCustomersAsync();
            var products = await _tableStorageService.GetAllProductsAsync();

            if (customers == null || customers.Count == 0)
            {
                ModelState.AddModelError("", "No customers found. Please add customers first.");
                return View();
            }

            if (products == null || products.Count == 0)
            {
                ModelState.AddModelError("", "No products found. Please add products first.");
                return View();
            }

            ViewData["Customers"] = customers;
            ViewData["Products"] = products;

            return View();
        }

        // Action to handle the form submission and register the order
        [HttpPost]
        public async Task<IActionResult> Register(OrderProcess orderProcess)
        {
            if (ModelState.IsValid)
            {
                orderProcess.OrderProcess_Date = DateTime.SpecifyKind(orderProcess.OrderProcess_Date, DateTimeKind.Utc);
                orderProcess.PartitionKey = "OrdersPartition";
                orderProcess.RowKey = Guid.NewGuid().ToString();
                await _tableStorageService.AddOrderProcessAsync(orderProcess);

                string message = $"New order by Customer {orderProcess.Customer_Id} for Product {orderProcess.Product_Id} at {orderProcess.Shipping_Location} on {orderProcess.OrderProcess_Date}";
                await _queueService.SendMessageAsync(message);

                return RedirectToAction("Index");
            }

            foreach (var error in ModelState)
            {
                Console.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }

            var customers = await _tableStorageService.GetAllCustomersAsync();
            var products = await _tableStorageService.GetAllProductsAsync();
            ViewData["Customers"] = customers;
            ViewData["Products"] = products;

            return View(orderProcess);
        }

        // Action to display the form to edit an existing order
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var orderProcess = await _tableStorageService.GetOrderProcessAsync(partitionKey, rowKey);
            if (orderProcess == null)
            {
                return NotFound();
            }

            var customers = await _tableStorageService.GetAllCustomersAsync();
            var products = await _tableStorageService.GetAllProductsAsync();

            ViewData["Customers"] = customers;
            ViewData["Products"] = products;

            return View(orderProcess);
        }

        // Action to handle the form submission and update an existing order
        [HttpPost]
        public async Task<IActionResult> Edit(OrderProcess orderProcess)
        {
            if (ModelState.IsValid)
            {
                orderProcess.OrderProcess_Date = DateTime.SpecifyKind(orderProcess.OrderProcess_Date, DateTimeKind.Utc);
                await _tableStorageService.UpdateOrderProcessAsync(orderProcess);
                return RedirectToAction("Index");
            }

            var customers = await _tableStorageService.GetAllCustomersAsync();
            var products = await _tableStorageService.GetAllProductsAsync();
            ViewData["Customers"] = customers;
            ViewData["Products"] = products;

            return View(orderProcess);
        }

        // Action to handle the request to delete an order
        [HttpPost]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            await _tableStorageService.DeleteOrderProcessAsync(partitionKey, rowKey);
            return RedirectToAction("Index");
        }
    }
}
