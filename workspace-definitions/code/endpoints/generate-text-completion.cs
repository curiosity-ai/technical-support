[endpoint: Curiosity.Endpoints.Path("generate-text-completion")]
[endpoint: Curiosity.Endpoints.AccessMode("AllUsers")]

var prompts = new List<Mosaik.AI.IChatAIMessage>();
prompts.Add(new Mosaik.AI.ChatAIMessage(Mosaik.AI.ChatAuthorRole.System, "You're an AI assistant, answer only with a pirate language"));
prompts.Add(new Mosaik.AI.ChatAIMessage(Mosaik.AI.ChatAuthorRole.User, "What's to do in Toulouse"));
return await ChatAI.GetCompletionAsync(CurrentUser, prompts);
