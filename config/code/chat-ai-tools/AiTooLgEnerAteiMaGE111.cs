[tools: Curiosity.ChatAITools.UID("AiTooLgEnerAteiMaGE111")]
[tools: Curiosity.ChatAITools.DisplayName("Create an Image")]
[tools: Curiosity.ChatAITools.Description("Generate images based on a description.")]
[tools: Curiosity.ChatAITools.Icon("fi-rr-picture")]
[tools: Curiosity.ChatAITools.AccessMode("AllUsers")]

public class ImageGenerationTool
{
    [Tool("Generate images based on a description.")]
    public static async Task<string> GenerateImage(ToolScope scope,
          [Parameter("The description of the image to be generated", required: true)] string imageDescription)
    {
        if (string.IsNullOrWhiteSpace(imageDescription))
        {
            return "[]";
        }
        else
        {
            var tornadoProvider = await scope.GetProviderAsync() as ITornadoProvider;
            var imageResult = await tornadoProvider.TornadoClient.ImageGenerations.CreateImage(new ImageGenerationRequest(prompt: imageDescription,
                                                                                                                          quality: TornadoImageQualities.Hd,
                                                                                                                          responseFormat: TornadoImageResponseFormats.Base64,
                                                                                                                          model: ImageModel.OpenAi.Dalle.V3));
            using var stream = new MemoryStream(Convert.FromBase64String(imageResult.Data.First().Base64));
            var fileUID = await scope.StoreUserFileAsync("generated-image.png", "image/png", "OpenAI", stream);
            scope.AddSnippet(fileUID, imageDescription, renderInline: true);
            return new { success = true }.ToJson();
        }
    }
}

return new ImageGenerationTool();

