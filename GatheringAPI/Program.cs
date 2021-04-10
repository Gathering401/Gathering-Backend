using System;
using System.Threading.Tasks;

using Amazon.S3;
using Amazon.S3.Model;
using GatheringAPI;
using GatheringAPI.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace S3CreateAndList
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var s3Client = new AmazonS3Client();

            if (GetBucketName(args, out String bucketName))
            {
                try
                {
                    var createResponse = await s3Client.PutBucketAsync(bucketName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            var host = CreateHostBuilder(args).Build();

            UpdateDatabase(host.Services);

            host.Run();

            var listResponse = await MyListBucketsAsync(s3Client);
            foreach (S3Bucket b in listResponse.Buckets)
            {
                Console.WriteLine(b.BucketName);
            }
        }

        private static Boolean GetBucketName(string[] args, out String bucketName)
        {
            Boolean retval = false;
            bucketName = String.Empty;
            if (args.Length == 0)
            {
                bucketName = String.Empty;
                retval = false;
            }
            else if (args.Length == 1)
            {
                bucketName = args[0];
                retval = true;
            }
            else
            {
                Environment.Exit(1);
            }
            return retval;
        }

        private static async Task<ListBucketsResponse> MyListBucketsAsync(IAmazonS3 s3Client)
        {
            return await s3Client.ListBucketsAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void UpdateDatabase(IServiceProvider services)
        {
            using (var serviceScope = services.CreateScope())
            {
                using (var db = serviceScope.ServiceProvider.GetService<GatheringDbContext>())
                {
                    db.Database.Migrate();
                }
            }
        }
    }
}