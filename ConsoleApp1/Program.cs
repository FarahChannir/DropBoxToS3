using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Dropbox.Api;
using Dropbox.Api.Files;
namespace ConsoleApp1
{
    class Program
    {
        const string dropboxAccessToken = "sl.BtbcP3MqnfmDVEejKNqrf5gLUir8MkJ-IVuoi3kL1pDomWobd1KM73E0A4g5Xq5um5TYQ2C1OIh8EKSZ-xTGa93lQ77150cTDTxuYel5MssJhw0KhkWNO3BcU_Zkx5NHf_M48r-VVA9qGYM-RLodTCU";

        static async Task Main()
        {
            const string awsAccessKey = "AKIAY33S6UVX2CRFF3EF";
            const string awsSecretKey = "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";
            const string awsS3BucketName = "drobboxs3";
            const string dropboxRootFolderPath = "/MV";
            const string Appkey = "3brdsrl3z6oe6g4";
            const string Appsecret = "tt7eshhzmid5s8i";
            try
            {
            
                var dbx = new DropboxClient(dropboxAccessToken);

             
                var s3Config = new AmazonS3Config
                {
                    Timeout = TimeSpan.FromMinutes(5),
                    RegionEndpoint = RegionEndpoint.EUNorth1
                };
                var s3Client = new AmazonS3Client(awsAccessKey, awsSecretKey, s3Config);
                var fileTransferUtility = new TransferUtility(s3Client);

                // Specify the Dropbox folder path and the S3 bucket name
                var dropboxFolderPath = dropboxRootFolderPath;
                var s3BucketName = awsS3BucketName;

                // Get a list of files in the Dropbox folder
                var list = await dbx.Files.ListFolderAsync(dropboxFolderPath);

                // Transfer each file from Dropbox to S3
                foreach (var item in list.Entries)
                {
                    if (item is FileMetadata)
                    {
                        var file = item as FileMetadata;
                        // Check if the file exists in the S3 bucket
                        var exists = await DoesFileExistInS3Async(s3BucketName, file.Name, s3Client);

                        if (!exists)
                        {
                            // Download the file from Dropbox
                            var fileContent = await dbx.Files.DownloadAsync(file.PathLower);

                            // Use the file stream in the file content
                            using (var fileStream = await fileContent.GetContentAsStreamAsync())
                            {
                                // Upload the file to S3
                                await fileTransferUtility.UploadAsync(fileStream, s3BucketName, file.Name);
                            }
                        }
                    }
                }
            }
            catch (Exception exp)
            {


            }

        }
        static async Task<bool> DoesFileExistInS3Async(string bucketName, string fileName, AmazonS3Client client)
        {
            // Create an Amazon S3 client using the specified bucket name

            {
                try
                {
                    // Create a GetObject request for the specified file
                    var request = new GetObjectRequest
                    {
                        BucketName = bucketName,
                        Key = fileName
                    };

                    // Attempt to get the object from S3
                    var response = await client.GetObjectAsync(request);

                    // If the response was successful, the file exists
                    return true;
                }
                catch (AmazonS3Exception e)
                {
                    // If the bucket or file was not found, return false
                    if (e.ErrorCode == "NoSuchBucket" || e.ErrorCode == "NoSuchKey")
                    {
                        return false;
                    }

                    // Otherwise, throw the exception
                    throw;
                }
            }
        }

     
    }
}

    