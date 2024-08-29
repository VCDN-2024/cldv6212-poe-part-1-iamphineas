using ABC_Retail.Models;
using Azure;
using Azure.Data.Tables;

namespace ABC_Retail.Services
{
    public class TableStorageService
    {
        private readonly TableClient _productTableClient;
        private readonly TableClient _customerTableClient;
        private readonly TableClient _orderProcessTableClient;

        public TableStorageService(string connectionString)
        {
            _productTableClient = new TableClient(connectionString, "Products");
            _customerTableClient = new TableClient(connectionString, "Customers");
            _orderProcessTableClient = new TableClient(connectionString, "OrderProcesses");
        }

        // Product methods
        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();
            await foreach (var product in _productTableClient.QueryAsync<Product>())
            {
                products.Add(product);
            }
            return products;
        }

        public async Task AddProductAsync(Product product)
        {
            ValidateEntityKeys(product);
            EnsureUtcDateTime(product);

            try
            {
                await _productTableClient.AddEntityAsync(product);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding product to Table Storage", ex);
            }
        }

        public async Task UpdateProductAsync(Product product)
        {
            ValidateEntityKeys(product);
            EnsureUtcDateTime(product);

            try
            {
                await _productTableClient.UpdateEntityAsync(product, ETag.All);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error updating product in Table Storage", ex);
            }
        }

        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                await _productTableClient.DeleteEntityAsync(partitionKey, rowKey);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error deleting product from Table Storage", ex);
            }
        }

        public async Task<Product?> GetProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _productTableClient.GetEntityAsync<Product>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error retrieving product from Table Storage", ex);
            }
        }

        // Customer methods
        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            var customers = new List<Customer>();
            await foreach (var customer in _customerTableClient.QueryAsync<Customer>())
            {
                customers.Add(customer);
            }
            return customers;
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            ValidateEntityKeys(customer);
            EnsureUtcDateTime(customer);

            try
            {
                await _customerTableClient.AddEntityAsync(customer);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding customer to Table Storage", ex);
            }
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            ValidateEntityKeys(customer);
            EnsureUtcDateTime(customer);

            try
            {
                await _customerTableClient.UpdateEntityAsync(customer, ETag.All);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error updating customer in Table Storage", ex);
            }
        }

        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            try
            {
                await _customerTableClient.DeleteEntityAsync(partitionKey, rowKey);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error deleting customer from Table Storage", ex);
            }
        }

        public async Task<Customer?> GetCustomerAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _customerTableClient.GetEntityAsync<Customer>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error retrieving customer from Table Storage", ex);
            }
        }

        // OrderProcess methods
        public async Task<List<OrderProcess>> GetAllOrderProcessesAsync()
        {
            var orderProcesses = new List<OrderProcess>();
            await foreach (var orderProcess in _orderProcessTableClient.QueryAsync<OrderProcess>())
            {
                orderProcesses.Add(orderProcess);
            }
            return orderProcesses;
        }

        public async Task AddOrderProcessAsync(OrderProcess orderProcess)
        {
            ValidateEntityKeys(orderProcess);
            EnsureUtcDateTime(orderProcess);

            try
            {
                await _orderProcessTableClient.AddEntityAsync(orderProcess);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding order process to Table Storage", ex);
            }
        }

        public async Task UpdateOrderProcessAsync(OrderProcess orderProcess)
        {
            ValidateEntityKeys(orderProcess);
            EnsureUtcDateTime(orderProcess);

            try
            {
                await _orderProcessTableClient.UpdateEntityAsync(orderProcess, ETag.All);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error updating order process in Table Storage", ex);
            }
        }

        public async Task DeleteOrderProcessAsync(string partitionKey, string rowKey)
        {
            try
            {
                await _orderProcessTableClient.DeleteEntityAsync(partitionKey, rowKey);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error deleting order process from Table Storage", ex);
            }
        }

        public async Task<OrderProcess?> GetOrderProcessAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _orderProcessTableClient.GetEntityAsync<OrderProcess>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error retrieving order process from Table Storage", ex);
            }
        }

        private void ValidateEntityKeys(ITableEntity entity)
        {
            if (string.IsNullOrEmpty(entity.PartitionKey) || string.IsNullOrEmpty(entity.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set.");
            }
        }

        private void EnsureUtcDateTime(ITableEntity entity)
        {
            var properties = entity.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                {
                    var value = property.GetValue(entity) as DateTime?;
                    if (value.HasValue && value.Value.Kind == DateTimeKind.Unspecified)
                    {
                        property.SetValue(entity, DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
                    }
                }
            }
        }
    }
}
