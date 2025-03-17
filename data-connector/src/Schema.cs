using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Curiosity.Library;
using UID;

namespace TechnicalSupport;

// This class is an auto-generated helper for all existing node & edge schema names on your graph.
// You can get an updated version of it by downloading the template project again.

public static class Schema
{
    public static class Nodes
    {
        [Node]
        public class Device
        {
            [Key] public string Name { get; set; }
        }

        [Node]
        public class Part
        {
            [Key] public string Name { get; set; }
        }

        [Node]
        public class Manufacturer
        {
            [Key] public string Name { get; set; }
        }

        [Node]
        public class SupportCase
        {
            [Key] public string Id { get; set; }
            [Property] public string Summary { get; set; }
            [Property] public string Content { get; set; }
            [Property] public string Status { get; set; }
            [Timestamp] public DateTimeOffset Time { get; set; }
        }
        
        [Node]
        public class SupportCaseMessage
        {
            [Key] public string Id { get; set; }
            [Property] public string Author { get; set; }
            [Property] public string Message { get; set; }
            [Timestamp] public DateTimeOffset Time { get; set; }
        }

        [Node]
        public class Status
        {
            [Key] public string Value { get; set; }
        }
    }

    public static class Edges
    {
        public const string HasPart         = nameof(HasPart);
        public const string PartOf          = nameof(PartOf);
        public const string HasSupportCase  = nameof(HasSupportCase);
        public const string ForDevice       = nameof(ForDevice);
        public const string HasManufacturer = nameof(HasManufacturer);
        public const string ManufacturerOf  = nameof(ManufacturerOf);
        public const string HasStatus       = nameof(HasStatus);
        public const string StatusOf        = nameof(StatusOf);
        public const string HasMessage      = nameof(HasMessage);
        public const string MessageOf       = nameof(MessageOf);
    }
}