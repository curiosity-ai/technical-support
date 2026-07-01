[tools: Curiosity.ChatAITools.UID("SupporTGraphLookup1111")]
[tools: Curiosity.ChatAITools.DisplayName("Support Graph Lookup")]
[tools: Curiosity.ChatAITools.Description("Get a single support case by its id (e.g. 'SC-36556'), including the device, status and full conversation.")]
[tools: Curiosity.ChatAITools.Icon("fi-rr-search")]
[tools: Curiosity.ChatAITools.AccessMode("AllUsers")]

// Companion to find-similar-support-cases.cs. Gives the assistant precise graph
// lookups so a support worker can pull a known case, device or part on demand.
// Enable for the AI Assistant by default in the workspace AI tool / assistant
// settings (there is no per-tool "default on" directive).
public class SupportGraphTool
{
    private const int MAX_RELATED = 25;
    private const int MAX_MESSAGES = 12;

    [Tool("Get a single support case by its id (e.g. 'SC-36556'), including the device, status and full conversation.")]
    public static async Task<string> GetSupportCase(ToolScope scope,
          [Parameter("The support case id, e.g. 'SC-36556'", required: true)] string caseId)
    {
        if (string.IsNullOrWhiteSpace(caseId)) return new { error = "A case id is required." }.ToJson();

        var caseNode = scope.Graph.Query().StartAt(N.SupportCase.Type, caseId.Trim()).AsEnumerable().FirstOrDefault();
        if (caseNode is null) return new { error = $"No support case found with id '{caseId}'." }.ToJson();

        string device = null;
        foreach (var deviceNode in scope.Graph.Query().StartAt(caseNode.UID).Out(N.Device.Type, E.ForDevice).AsEnumerable())
        {
            device = deviceNode.GetString(N.Device.Name);
            break;
        }

        var conversation = new List<object>();
        foreach (var messageNode in scope.Graph.Query().StartAt(caseNode.UID).Out(N.SupportCaseMessage.Type, E.HasMessage).Take(MAX_MESSAGES).AsEnumerable())
        {
            conversation.Add(new
            {
                author = messageNode.GetString(N.SupportCaseMessage.Author),
                message = messageNode.GetString(N.SupportCaseMessage.Message)
            });
        }

        scope.SetToolCallDisplayName($"Case {caseId}");

        return new
        {
            id = caseNode.GetString(N.SupportCase.Id),
            summary = caseNode.GetString(N.SupportCase.SupportCaseSummary),
            status = caseNode.GetString(N.SupportCase.Status),
            device,
            conversation
        }.ToJson();
    }

    [Tool("Get a device by its exact name, with the parts it contains and its support cases. Use to understand a device before answering.")]
    public static async Task<string> GetDevice(ToolScope scope,
          [Parameter("The exact device name, e.g. 'Samsung Galaxy A53 5G'", required: true)] string deviceName)
    {
        if (string.IsNullOrWhiteSpace(deviceName)) return new { error = "A device name is required." }.ToJson();

        var deviceNode = scope.Graph.Query().StartAt(N.Device.Type, deviceName.Trim()).AsEnumerable().FirstOrDefault();
        if (deviceNode is null) return new { error = $"No device found named '{deviceName}'." }.ToJson();

        var parts = new List<string>();
        foreach (var partNode in scope.Graph.Query().StartAt(deviceNode.UID).Out(N.Part.Type, E.HasPart).Take(MAX_RELATED).AsEnumerable())
        {
            parts.Add(partNode.GetString(N.Part.Name));
        }

        var cases = new List<object>();
        foreach (var caseNode in scope.Graph.Query().StartAt(deviceNode.UID).Out(N.SupportCase.Type, E.HasSupportCase).Take(MAX_RELATED).AsEnumerable())
        {
            cases.Add(new
            {
                id = caseNode.GetString(N.SupportCase.Id),
                summary = caseNode.GetString(N.SupportCase.SupportCaseSummary),
                status = caseNode.GetString(N.SupportCase.Status)
            });
        }

        scope.SetToolCallDisplayName($"Device {deviceName}");

        return new
        {
            name = deviceNode.GetString(N.Device.Name),
            parts,
            cases
        }.ToJson();
    }

    [Tool("Get a part by its exact name, with its manufacturer and the devices that use it.")]
    public static async Task<string> GetPart(ToolScope scope,
          [Parameter("The exact part name", required: true)] string partName)
    {
        if (string.IsNullOrWhiteSpace(partName)) return new { error = "A part name is required." }.ToJson();

        var partNode = scope.Graph.Query().StartAt(N.Part.Type, partName.Trim()).AsEnumerable().FirstOrDefault();
        if (partNode is null) return new { error = $"No part found named '{partName}'." }.ToJson();

        string manufacturer = null;
        foreach (var manufacturerNode in scope.Graph.Query().StartAt(partNode.UID).Out(N.Manufacturer.Type, E.HasManufacturer).AsEnumerable())
        {
            manufacturer = manufacturerNode.GetString(N.Manufacturer.Name);
            break;
        }

        var devices = new List<string>();
        foreach (var deviceNode in scope.Graph.Query().StartAt(partNode.UID).Out(N.Device.Type, E.PartOf).Take(MAX_RELATED).AsEnumerable())
        {
            devices.Add(deviceNode.GetString(N.Device.Name));
        }

        scope.SetToolCallDisplayName($"Part {partName}");

        return new
        {
            name = partNode.GetString(N.Part.Name),
            manufacturer,
            devices
        }.ToJson();
    }
}

return new SupportGraphTool();

