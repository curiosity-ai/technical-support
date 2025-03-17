namespace TechnicalSupport.FrontEnd
{
    // This class is an auto-generated helper for all existing node & edge schema on your graph.
    // You can get an updated version of it by downloading the template project again, and replacing this file
    //
    // You can use as a helper to call methods on the graph, or read contents nodes you retrieve.
    //
    // Examples:
    //
    // - Fetch node from graph
    //   >    var authorNode = await Mosaik.API.Nodes.GetAsync(N.Author.Type, "John Doe");
    //
    // - Read string value from node:
    //   >    var authorName = authorNode .GetString(N.Author.Name);
    //
    // - Run query on graph:
    //   >    var booksFromAuthor = await Mosaik.API.Query.StartAt(N.Author.Type, "John Doe").Out(N.Book.Type, E.AuthorOf).Take(100).GetAsync();
    //     or
    //   >    var booksFromAuthor = await Mosaik.API.Query.StartAt(authorNode.UID).Out(N.Book.Type, E.AuthorOf).Take(100).GetAsync();

    public static class N
    {
        public sealed class Device
        {
            public const string Type = nameof(Device);
            public const string Name = nameof(Name);
        }
        public sealed class Part
        {
            public const string Type = nameof(Part);
            public const string Name = nameof(Name);
        }
        public sealed class Manufacturer
        {
            public const string Type = nameof(Manufacturer);
            public const string Name = nameof(Name);
        }
        public sealed class SupportCase
        {
            public const string Type = nameof(SupportCase);
            public const string Id = nameof(Id);
            public const string Summary = nameof(Summary);
            public const string Content = nameof(Content);
            public const string Status  = nameof(Status);
        }
        public sealed class SupportCaseMessage
        {
            public const string Type = nameof(SupportCaseMessage);
            public const string Id = nameof(Id);
            public const string Author = nameof(Author);
            public const string Message = nameof(Message);
        }
        public sealed class Status
        {
            public const string Type = nameof(Status);
            public const string Value = nameof(Value);
        }
        public sealed class PotentialIdentifiers
        {
            public const string Type = nameof(PotentialIdentifiers);
            public const string Value = nameof(Value);
        }
    }


    public static class E
    {
        public const string HasPart = nameof(HasPart);
        public const string PartOf = nameof(PartOf);
        public const string HasSupportCase = nameof(HasSupportCase);
        public const string ForDevice = nameof(ForDevice);
        public const string HasManufacturer = nameof(HasManufacturer);
        public const string ManufacturerOf = nameof(ManufacturerOf);
        public const string HasStatus = nameof(HasStatus);
        public const string StatusOf = nameof(StatusOf);
        public const string HasMessage = nameof(HasMessage);
        public const string MessageOf = nameof(MessageOf);
    }

}
