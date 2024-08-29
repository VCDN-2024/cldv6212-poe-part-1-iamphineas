using ABC_Retail.Models;
using ABC_Retail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABC_Retail.Controllers
{
    public class ProductsController : Controller
    {
        private readonly BlobService _blobService;
        private readonly TableStorageService _tableStorageService;

        public ProductsController(BlobService blobService, TableStorageService tableStorageService)
        {
            _blobService = blobService;
            _tableStorageService = tableStorageService;
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using var stream = file.OpenReadStream();
                try
                {
                    var imageUrl = await _blobService.UploadAsync(stream, file.FileName);
                    product.ImageUrl = imageUrl;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
                    return View(product);
                }
            }

            if (ModelState.IsValid)
            {
                product.PartitionKey = "ProductsPartition";
                product.RowKey = Guid.NewGuid().ToString();
                try
                {
                    await _tableStorageService.AddProductAsync(product);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error adding product: {ex.Message}");
                    return View(product);
                }
                return RedirectToAction("Index");
            }

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(string partitionKey, string rowKey)
        {
            var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product product, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            var existingProduct = await _tableStorageService.GetProductAsync(product.PartitionKey, product.RowKey);
            if (existingProduct == null)
            {
                return NotFound();
            }

            // Update product properties
            existingProduct.Product_Name = product.Product_Name;
            existingProduct.Description = product.Description;
            existingProduct.Location = product.Location;

            if (file != null && file.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    try
                    {
                        await _blobService.DeleteBlobAsync(existingProduct.ImageUrl);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Error deleting existing image: {ex.Message}");
                        return View(product);
                    }
                }

                using var stream = file.OpenReadStream();
                try
                {
                    var imageUrl = await _blobService.UploadAsync(stream, file.FileName);
                    existingProduct.ImageUrl = imageUrl;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error uploading new image: {ex.Message}");
                    return View(product);
                }
            }

            // Save the updated product
            try
            {
                await _tableStorageService.UpdateProductAsync(existingProduct);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating product: {ex.Message}");
                return View(product);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string partitionKey, string rowKey)
        {
            var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);

            if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
            {
                try
                {
                    await _blobService.DeleteBlobAsync(product.ImageUrl);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error deleting image: {ex.Message}");
                    return RedirectToAction("Index");
                }
            }

            try
            {
                await _tableStorageService.DeleteProductAsync(partitionKey, rowKey);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting product: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _tableStorageService.GetAllProductsAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error fetching products: {ex.Message}");
                return View(new List<Product>());
            }
        }
    }
}


