using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ABC_Retail.Models
{
    public class Customer : ITableEntity
    {
        [Key]
        public int Customer_Id { get; set; }  // Unique identifier for the customer

        public string? Customer_Name { get; set; }  // Name of the customer

        public string? Email { get; set; }  // Customer's email address

        public string? Password { get; set; }  // Customer's password

        // ITableEntity implementation
        public string? PartitionKey { get; set; }  // Partition key for table storage

        public string? RowKey { get; set; }  // Row key for table storage

        public ETag ETag { get; set; }  // Entity tag for concurrency control

        public DateTimeOffset? Timestamp { get; set; }  // Timestamp for the entity
    }
}