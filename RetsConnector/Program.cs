using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RetsSdk.Models;
using RetsSdk.Models.Enums;
using RetsSdk.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RetsConnector
{
    class Program
    {
        public static IConfigurationRoot Configuration;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            Console.WriteLine("Environment: {0}", environment);

            var services = new ServiceCollection();

            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: true);
            if (environment == "Development")
            {

                builder.AddJsonFile(
                        Path.Combine(AppContext.BaseDirectory, string.Format("..{0}..{0}..{0}", Path.DirectorySeparatorChar), $"appsettings.{environment}.json"),
                        optional: true
                    );
            }
            else
            {
                builder.AddJsonFile($"appsettings.{environment}.json", optional: false);
            }

            Configuration = builder.Build();

            var options = new ConnectionOptions()
            {
                UserAgent = Configuration["Rets:UserAgent"],
                RetsServerVersion = Configuration["Rets:ServerVersion"],
                LoginUrl = Configuration["Rets:LoginUrl"],
                Username = Configuration["Rets:Username"],
                Password = Configuration["Rets:Password"],
                Type = Enum.Parse<AuthenticationType>(Configuration["Rets:Type"], true)
            };

            try
            {
                Task.Run(async () =>
                {


                    var session = new Session(options);

                    //

                    await session.Connect();

                    var files = await session.GetObject("Property", "Photo", new PhotoId(9409920), false);

                    var searchRequest = new SearchRequest("Property", "Listing")
                    {
                        Limit = 10
                    };

                    searchRequest.ParameterGroup.AddParameter(new QueryParameter("matrix_unique_id", "0+"));

                    //await session.Search(searchRequest);

                    foreach (var file in files)
                    {
                        var filePath = $"{file.ContentId}/{file.ObjectId}{file.Extension}";

                        using (FileStream output = File.Create("../../../Downloads/" + filePath))
                        {
                            file.Content.CopyTo(output);
                            file.Dispose();
                        }
                    }

                    var system = await session.GetSystemMetadata();

                    RetsResourceCollection resources = await session.GetResourcesMetadata();  // this allows you to list the resources

                    RetsClassCollection classes = await session.GetClassesMetadata("Property");

                    RetsObjectCollection objects = await session.GetObjectMetadata("Property");


                    RetsLookupTypeCollection lookupValues = await session.GetLookupValues("Property", "*");

                    RetsFieldCollection tables = await session.GetTableMetadata("Property", "Listing");

                    await session.Disconnect();

                    var photos = new List<PhotoId>() { new PhotoId(1), new PhotoId(2) };

                    // to save some code you can do call RoundTrip() which will connect, call out method, then discconnect();
                    var result = await session.RoundTrip(async () =>
                    {
                        // Batch size will allow us to auto break multiple requests into a smaller batched
                        // IF you want to download lots of images, the server may retrn 404 error due to too many params in the request
                        // this will allow you to break the request into multiple request and do the job 
                        return await session.GetObject("Property", "Photo", photos, batchSize: 20);
                    });

                });


                //Console.WriteLine("Hello World!");

                Console.ReadKey();

            }
            catch (Exception e)
            {

            }
        }



    }
}
