using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ABC_Retail.Models
{
    public class OrderProcess : ITableEntity
    {
        [Key]
        public int OrderProcess_Id { get; set; }  // Unique identifier for the order process

        public string? PartitionKey { get; set; }  // Partition key for table storage
        public string? RowKey { get; set; }  // Row key for table storage
        public DateTimeOffset? Timestamp { get; set; }  // Timestamp for the entity
        public ETag ETag { get; set; }  // Entity tag for concurrency control

        [Required(ErrorMessage = "Please select a customer.")]
        public int Customer_Id { get; set; }  // FK to the Customer placing the order

        [Required(ErrorMessage = "Please select a product.")]
        public int Product_Id { get; set; }  // FK to the Product being ordered

        [Required(ErrorMessage = "Please select the date.")]
        public DateTime OrderProcess_Date { get; set; }  // Date when the order was processed

        [Required(ErrorMessage = "Please enter the shipping location.")]
        public string? Shipping_Location { get; set; }  // Shipping location for the order

        // These properties are not stored in Table Storage but can be used for display purposes
        public string? Product_Name { get; set; }
        public string? Customer_Name { get; set; }
    }
}
