# CrestApps RetsSdk

.NET Core 3 based client library for interacting with a RETS server to pull real estate listings, photos, and other data made available from an MLS system.

Although this library has been tested with version 1.7.2 using digest authentication, it should work for versions 1.5, 1.7, 1.7.1, 1.7.2 and 1.8 using basic or digest authentication.

## Getting Started
 - Install-Package CrestApps.RetsSdk
 - Have Fun :)

## Example of what can you do with the library

The RetsConnector project is a console app with an example of how to connect to the RETS server.

But here is some code example of how to connect

```cs
// 1) Register HttpClientFactory
services.AddHttpClient();

// 2) Register the IRetsRequester
services.AddTransient<IRetsRequester, RetsWebRequester>();

// 3) Register the IRetsSession
services.AddTransient<IRetsSession, RetsSession>();

// 4) Register the IRetsClient
services.AddTransient<IRetsClient, RetsClient>();

// 5) Register the ILogger
services.AddLogging();

// 6) Register the connection options
services.AddOptions<ConnectionOptions>()
        .Configure((opts) =>
        {
            opts.UserAgent = hostContext.Configuration["Rets:UserAgent"];
            opts.RetsServerVersion = RetsVersion.Make(hostContext.Configuration["Rets:ServerVersion"]);
            opts.LoginUrl = hostContext.Configuration["Rets:LoginUrl"];
            opts.Username = hostContext.Configuration["Rets:Username"];
            opts.Password = hostContext.Configuration["Rets:Password"];
            opts.UserAgentPassward = hostContext.Configuration["Rets:UserAgentPassward"];
            opts.Type = Enum.Parse<AuthenticationType>(hostContext.Configuration["Rets:Type"], true);
            opts.Timeout = TimeSpan.FromHours(1);
        });

// Create out IoC container
IServiceProvider container = Service.BuildServiceProvider();

// Get instance of the IRetsClient from the IoC
IRetsClient client = container.GetService<IRetsClient>();

// The first request we make to the RETS server is to login
await client.Connect();


// To get to know the RETS server, we can scan the entire system to get a list of our resources, classes, object.....
RetsSystem system = await client.GetSystemMetadata();

// We can also get a list of all available resources
RetsResourceCollection resources = await client.GetResourcesMetadata();

// We can also get all available classes for a given resorce. Assuming your RETS server has a resource called "Property"
RetsClassCollection classes = await client.GetClassesMetadata("Property");


// We can also get a list of all available objects on a given property.
RetsObjectCollection objects = await client.GetObjectMetadata("Property");


// We can also get a list of all available field for the given resource and class. Assuming your RETS server has a resource called "Property" with a class called "Listing"
RetsFieldCollection fields = await client.GetTableMetadata("Property", "Listing");

// We can also get all available lookup values on the given resource
IEnumerable<RetsLookupTypeCollection> lookupTypes = await client.GetLookupValues("Property");

// We can also get a list of all available lookup types for a give resource and lookuptype aka fieldName
RetsLookupTypeCollection lookupType = await client.GetLookupValues("Property", "FieldName");

// We can perform a search against the RETS server
SearchRequest searchRequest = new SearchRequest("Property", "Listing");

// Add ad many parameers to search for. Assuming the class "Listing" on the "Property" resource has a field called "matrix_unique_id" which is numeric type
// we say give me all properties where matrix_unique_id >= 0
searchRequest.ParameterGroup.AddParameter(new QueryParameter("matrix_unique_id", "0+"));

// If you like to return specific columns, you can do so like this
searchRequest.AddColumn("matrix_unique_id");
searchRequest.AddColumn("SomeOtherColumnName");

// This performs the search against the server
SearchResult result = await client.Search(searchRequest);

// we can iterate over the results like so
foreach (SearchResultRow row in result.GetRows())
{
    // Each row has multiple columns, lets loop over them
    foreach (var columnName in result.GetColumns())
    {
        // Lets get the cell value for a given column from the current row
        SearchResultCellValue value = row.Get(columnName);

        // you can get the value trimmed like so
        string a = value.GetTrimmed();
        string b = value.NullOrValue();


        // Assuming you know the type of the returned data, you can cast the values like so
        // Of cource you must know that the column is int type befor eyou call this
        //int castedToIntValue = value.Get<int>(); 
        //you can also do something like
        //int? castedToIntValue = value.GetNullable<int>(); 
        //DateTime? castedToIntValue = value.GetNullable<DateTime>();

        // you can also check if the value is restricted like this
        bool restrictedValue = value.IsRestricted;

        // you can check if this value is a primary key
        bool primaryKey = value.IsPrimaryKeyValue;

        bool c = value.IsNullOrEmpty();
        bool d = value.IsNullOrWhiteSpace();
        bool e = value.IsNull();
    }
}


// Also you can extract only all value for a given column like this
IEnumerable<SearchResultCellValue> createdAtCells = result.Pluck("CreatedAt");

// you can also result cast the values of a given field like this
// this will result an IEnumerable<> of all values found in the CreatedAt column
IEnumerable<DateTime> createdAtvalues = result.Pluck<DateTime>("CreatedAt");

// We can also download photos
// This will return all photos for property with the primarykey 1234
IEnumerable<FileObject> files = await client.GetObject("Property", "Photo", new PhotoId(1234), false);

// Here is how we can iterate over the fields
foreach (FileObject file in files)
{
    var filePath = $"{file.ContentId}/{file.ObjectId}{file.Extension}";

    using (FileStream output = File.Create("../../../Downloads/" + filePath))
    {
        // file.Content has the stream object where you can write it your storage
        file.Content.CopyTo(output);

        // IMPORTANT: Make sure you dispose the stram after finising using it
        file.Dispose();
    }
}

// you can get a specific image for a given primary key like so
IEnumerable<FileObject> files2 = await client.GetObject("Property", "Photo", new PhotoId(1234, 1), false);


// you can get also get images for multiple primary keys at the same time like this
List<PhotoId> photoIds = new List<PhotoId>() { new PhotoId(1234), new PhotoId(5678), new PhotoId(2255) };

IEnumerable<FileObject> files3 = await client.GetObject("Property", "Photo", photoIds, false);


// When you are trying to download lots of images you must be very careful. If you send the server too many ids at the same time
// the server may return 404, 414, 500 or something along these lines because the request is too long.
// Also, the server may take long time to respond which will cause the HTTP request to timeout.
// So solve for the timeout issue we can increase the HTTP timeout from ConnectionOptions() object.


// However, there is a better more reliable solution to this problem which is the ability to batch the request into multiple requests
// Assume we want to download images for 1000 properties. Assume that each property on average has 15 photos, this will result in downloading 1000 x 15 = 15,000 images
// We can split the 1000 properties into a smaller number like 100. This will make 10 (i.e 1000/100) request to the RETS server then return you an object of 15,000 images
// You may want to still be careful because there will be 15,000 stored in the memory, so make sure you're not going to run out of memory
// Anyhow, batching can easily be done like this

IEnumerable<FileObject> batchedFiles = await client.GetObject("Property", "Photo", photoIds, batchSize: 100);

// Finally we can disconect
await client.Disconnect();

// The above code requires us to First connect, then Disconnect when we are done. Not too bad, but we can simplify the call by using
// a method called RoundTrip() which will first connect, execute out code, then disconnect

// to save some code you can do call RoundTrip() which will connect, call out method, then discconnect();
IEnumerable<FileObject> files4 = await client.RoundTrip(async () =>
{
    // Each batch will cause a round trip. In other words, each batch will connect, download a batch, then disconnect.
    return await client.GetObject("Property", "Photo", photoIds, batchSize: 20);
});
```

## License

"Laravel Code Generator" is an open-sourced software licensed under the <a href="https://opensource.org/licenses/MIT" target="_blank" title="MIT license">MIT license</a>
