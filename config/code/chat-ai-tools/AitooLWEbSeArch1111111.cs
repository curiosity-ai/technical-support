[tools: Curiosity.ChatAITools.UID("AitooLWEbSeArch1111111")]
[tools: Curiosity.ChatAITools.DisplayName("Search the Web")]
[tools: Curiosity.ChatAITools.Description("Search for files that the user has access to. To reference results for the user, use the snippetId in brackets, such as [1].")]
[tools: Curiosity.ChatAITools.Icon("fi-rr-globe")]
[tools: Curiosity.ChatAITools.AccessMode("AllUsers")]

public class WebSearchTool
{
    [Tool("Search for files that the user has access to. To reference results for the user, use the snippetId in brackets, such as [1].")]
    public static async Task<string> WebSearch(ToolScope scope,
          [Parameter("The search query", required: true)] string query,
          [Parameter("Number of top results to return. Defaults to 10", required: false)] int limit = 10,
          [Parameter("Number of results to skip, use with limit for pagination. Defaults to 0", required: false)] int skip = 0,
          [Parameter("The two-letter language code for results. Defaults to 'en'", required: false)] string lang = "en",
          [Parameter("The two-letter country code for results. Defaults to 'US'", required: false)] string country = "US")
    {

        if (limit <= 0) limit = 10;

        if (string.IsNullOrWhiteSpace(query))
        {
            return "[]";
        }
        else
        {
            //https://api-dashboard.search.brave.com/app/keys
            const string BraveAPIKey = ""; // << ENTER YOUR BRAVE API KEY HERE

            if (string.IsNullOrWhiteSpace(BraveAPIKey))
            {
                return new { error = "Missing Brave Search API Key" }.ToJson();
            }

            var offset = (int)(skip / limit);
            var url = $"https://api.search.brave.com/res/v1/web/search?q={System.Web.HttpUtility.UrlEncode(query)}&country={System.Web.HttpUtility.UrlEncode(country)}&search_lang={System.Web.HttpUtility.UrlEncode(lang)}&count={limit}&offset={offset}&units=metric";

            using var client = scope.GetHttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
            client.DefaultRequestHeaders.TryAddWithoutValidation("x-subscription-token", BraveAPIKey);
            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            var braveResponse = jsonResponse.FromJson<BraveAPI.WebSearchApiResponse>();

            scope.SetToolCallDisplayName($"Web Search for '{query}'");

            return braveResponse.Web.Results.Select(r =>
            {
                var snippetID = scope.AddSnippet(uid: default, text: r.Description, label: r.Title, labelTitle: "Title");
                return new
                {
                    title = r.Title,
                    url = r.Url.ToString(),
                    description = r.Description,
                    snippetID = snippetID,
                };
            }).ToJson();
        }
    }
}

return new WebSearchTool();

public static class BraveAPI
{
    public class WebSearchApiResponse
    {
        public Query Query { get; set; }
        public Mixed Mixed { get; set; }
        public string Type { get; set; }
        public Web Web { get; set; }
    }

    public class Mixed
    {
        public string Type { get; set; }
        public ResultReference[] Main { get; set; }
        public ResultReference[] Top { get; set; }
        public ResultReference[] Side { get; set; }
    }

    public class ResultReference
    {
        public string Type { get; set; }
        public long? Index { get; set; }
        public bool All { get; set; }
    }

    public class Query
    {
        public string Original { get; set; }
        public bool ShowStrictWarning { get; set; }
        public bool IsNavigational { get; set; }
        public bool IsNewsBreaking { get; set; }
        public bool SpellcheckOff { get; set; }
        public string Country { get; set; }
        public bool BadResults { get; set; }
        public bool ShouldFallback { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string HeaderCountry { get; set; }
        public bool MoreResultsAvailable { get; set; }
        public string State { get; set; }
    }


    public class Web
    {
        public string Type { get; set; }
        public WebResult[] Results { get; set; }
        public bool FamilyFriendly { get; set; }
    }

    public class WebResult
    {
        public string Title { get; set; }
        public Uri Url { get; set; }
        public bool IsSourceLocal { get; set; }
        public bool IsSourceBoth { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? PageAge { get; set; }
        public Profile Profile { get; set; }
        public string Language { get; set; }
        public bool FamilyFriendly { get; set; }
        public string Type { get; set; }
        public string Subtype { get; set; }
        public bool IsLive { get; set; }
        public MetaUrl MetaUrl { get; set; }
        public string Age { get; set; }
        public Thumbnail Thumbnail { get; set; }
    }

    public class Profile
    {
        public string Name { get; set; }
        public Uri Url { get; set; }
        public string LongName { get; set; }
        public Uri Img { get; set; }
    }

    public class Thumbnail
    {
        public Uri Src { get; set; }
        public Uri Original { get; set; }
        public bool Logo { get; set; }
    }

    public class MetaUrl
    {
        public string Scheme { get; set; }
        public string Netloc { get; set; }
        public string Hostname { get; set; }
        public Uri Favicon { get; set; }
        public string Path { get; set; }
    }
}

