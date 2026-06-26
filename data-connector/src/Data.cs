using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechnicalSupport;

public class DeviceJson
{
    public string Name { get; set; }
}

public class PartJson
{
    public string Name { get; set; }
    public string Manufacturer { get; set; }
    public string[] Devices { get; set; }
}

public class SupportCaseJson
{
    public string Summary { get; set; }
    public string Content { get; set; }
    public string Status { get; set; }
    public string Device { get; set; }
    public DateTimeOffset Time { get; set; }
}