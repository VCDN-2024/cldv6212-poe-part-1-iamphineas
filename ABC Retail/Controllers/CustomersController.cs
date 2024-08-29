using ABC_Retail.Models;
using ABC_Retail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class CustomersController : Controller
    {
        private readonly TableStorageService _tableStorageService;

        public CustomersController(TableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        // Display the list of customers
        public async Task<IActionResult> Index()
        {
            var customers = await _tableStorageService.GetAllCustomersAsync();
            return View(customers);
        }

        // Show the form to create a new customer
        public IActionResult Create()
        {
            return View();
        }

        // Handle the POST request to create a new customer
        [HttpPost]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            customer.PartitionKey = "CustomersPartition";
            customer.RowKey = Guid.NewGuid().ToString();

            await _tableStorageService.AddCustomerAsync(customer);
            return RedirectToAction("Index");
        }

        // Handle the request to delete a customer
        [HttpPost]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            await _tableStorageService.DeleteCustomerAsync(partitionKey, rowKey);
            return RedirectToAction("Index");
        }

        // Display details of a specific customer
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            var customer = await _tableStorageService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // Display the form to edit a customer
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var customer = await _tableStorageService.GetCustomerAsync(partitionKey, rowKey);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // Handle the POST request to update a customer
        [HttpPost]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            await _tableStorageService.UpdateCustomerAsync(customer);
            return RedirectToAction("Index");
        }
    }
}