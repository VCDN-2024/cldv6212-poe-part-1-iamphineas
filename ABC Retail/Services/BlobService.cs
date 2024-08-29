using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ABC_Retail.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "products";  // Updated container name to match product context

        public BlobService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            try
            {
                await blobClient.UploadAsync(fileStream, overwrite: true);
            }
            catch (RequestFailedException ex) when (ex.Status == 409)
            {
                // Handle the conflict error (e.g., log it, notify the user, etc.)
                throw new InvalidOperationException("A blob with the same name already exists.");
            }

            return blobClient.Uri.ToString();
        }

        public async Task DeleteBlobAsync(string blobUri)
        {
            Uri uri = new Uri(blobUri);
            string blobName = uri.Segments[^1];
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            try
            {
                await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            }
            catch (RequestFailedException ex)
            {
                // Handle potential errors
                throw new InvalidOperationException("Error deleting the blob.", ex);
            }
        }
    }
}
