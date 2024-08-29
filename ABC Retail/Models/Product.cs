using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ABC_Retail.Models
{
    public class Product : ITableEntity
    {
        [Key]
        public int Product_Id { get; set; }  // Unique identifier for the product

        public string? Product_Name { get; set; }  // Name of the product

        public string? Description { get; set; }  // Description of the product

        public string? ImageUrl { get; set; }  // URL for the product image

        public string? Location { get; set; }  // Location associated with the product

        // ITableEntity implementation
        public string? PartitionKey { get; set; }  // Partition key for table storage

        public string? RowKey { get; set; }  // Row key for table storage

        public ETag ETag { get; set; }  // Entity tag for concurrency control

        public DateTimeOffset? Timestamp { get; set; }  // Timestamp for the entity
    }
}
