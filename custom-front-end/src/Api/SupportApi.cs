using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mosaik;
using Mosaik.Components;
using Mosaik.Helpers;
using Mosaik.Schema;
using Mosaik.Views;
using Tesserae;
using TNT;
using UID;
using static Tesserae.UI;
using static Mosaik.UI;
using static TNT.T;
using static Transpose.Core.dom;
using API  = Mosaik.API;
using Node = Mosaik.Schema.Node;


namespace TechnicalSupport.FrontEnd
{
    // The wire contract, front-end half, and the one place this project calls its own code endpoints.
    //
    // Every request and response class here is declared a second time, identically, in the endpoint that
    // reads or returns it (custom-endpoints/code/endpoints/...). The two declarations are the wire format: keep
    // them in step. DTOs are flat - no inheritance, no nested generics beyond arrays.

    public sealed class SupportChatContext
    {
        public string Topic { get; set; }
    }

    public sealed class SupportChatMessageRequest
    {
        public string                  Message           { get; set; }
        public UID128                  ChatUID           { get; set; }
        public UID128                  ReplaceMessageUID { get; set; }   // set by "Edit" and "Regenerate", null for a new message
        public UID128[]                Tools             { get; set; }
        public UID128[]                Agents            { get; set; }
        public UID128[]                Skills            { get; set; }
        public UID128[]                Attachments       { get; set; }
        public SupportChatContext      Context           { get; set; }
    }

    public sealed class SupportChatSetContextRequest
    {
        public UID128             ChatUID { get; set; }
        public SupportChatContext Context { get; set; }
    }

    public sealed class SimilarCase
    {
        public UID128 UID   { get; set; }
        public float  Score { get; set; }
    }

    internal static class SupportApi
    {
        // One method per endpoint, and nothing else in the project calls API.Endpoints directly. The route is the
        // generated constant, never a literal. readOnlyCompatible mirrors the endpoint's own ReadOnly marking.

        internal static Task<UID128> PostChatMessageAsync(SupportChatMessageRequest request)
            => API.Endpoints.CallAsync<UID128>(Endpoints.SupportChat.PostMessage, request);

        internal static Task<SupportChatContext> GetChatContextAsync(UID128 chatUID)
            => API.Endpoints.CallAsync<SupportChatContext>(Endpoints.SupportChat.GetContext, chatUID, readOnlyCompatible: true);

        internal static Task SetChatContextAsync(UID128 chatUID, SupportChatContext context)
            => API.Endpoints.CallAsync<string>(Endpoints.SupportChat.SetContext, new SupportChatSetContextRequest { ChatUID = chatUID, Context = context });

        /// <summary>The cases most similar to <paramref name="caseUID"/> for the same device, best first.</summary>
        internal static Task<SimilarCase[]> SuggestSimilarCasesAsync(UID128 caseUID)
            => API.Endpoints.CallAsync<SimilarCase[]>(Endpoints.SuggestSimilarCases, caseUID, readOnlyCompatible: true);
    }
}
