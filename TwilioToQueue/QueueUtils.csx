#r "Microsoft.WindowsAzure.Storage"

using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Queue; // Namespace for Queue storage types

static void AddToQueue(string data)
{
    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
        Environment.GetEnvironmentVariable(
            "codecampdemostorage_STORAGE"));
    CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
    CloudQueue queue = queueClient.GetQueueReference("sms-queue");
    queue.CreateIfNotExists();
    CloudQueueMessage message = new CloudQueueMessage(data);
    queue.AddMessage(message);
}