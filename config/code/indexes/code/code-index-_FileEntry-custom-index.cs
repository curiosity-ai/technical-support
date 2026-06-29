[indexes: Curiosity.Indexes.CodeIndex("_FileEntry")]
[indexes: Curiosity.Indexes.Name("Custom Index")]


// This is an example of a custom code index. 
// You can use it to perform actions on all data of a given type, in-system or outside
// Notes: 
//   - You should always check if the execution was cancelled using the variable CancellationToken
//   - You can use sync or async code in this block
//   - You can call external systems using an HttpClient instance


var failed = new List<UID128>();

var httpClient = CreateHttpClient();

foreach (var uid in ToIndex)
{
    if (CancellationToken.IsCancellationRequested)
    {
        return ToIndex;
    }

    // Sync example:
    if (!TryIndex(uid))
    {
        failed.Add(uid);
    }

    // Async example:
    //  if (await TryIndexAsync(uid) == false)
    //  {
    //      failed.Add(uid);
    //  }

    // HTTP request example:
    //  if (await TryIndexExternalAsync(uid, httpClient) == false)
    //  {
    //      failed.Add(uid);
    //  }
}

return failed;

bool TryIndex(UID128 uid)
{
    Logger.LogInformation($"Indexing {uid}");
    return true;
}

async Task<bool> TryIndexAsync(UID128 uid)
{
    Logger.LogInformation($"Indexing {uid}");
    return true;
}

async Task<bool> TryIndexExternalAsync(UID128 uid, HttpClient client)
{
    var response = await client.PostAsync($"https://example.com/process?uid={uid}", null);
    return response.IsSuccessStatusCode;
}

