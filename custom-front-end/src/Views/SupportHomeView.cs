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
    /// <summary>
    /// The home page: the support backlog. Every row is a support case drawn by its own renderer
    /// (SupportCaseRenderer), so the backlog and the Support Cases page look the same without either
    /// customising its rows.
    /// </summary>
    public sealed class SupportHomeView : IComponent
    {
        private readonly IComponent _content;

        public SupportHomeView(Parameters state)
        {
            _content = Hub("Technical Support Backlog".t(), DefaultRoutes.Home)
               .Section(CreateSearch(), grow: true)
               .S();
        }

        private static IComponent CreateSearch()
        {
            return SearchArea()
               .WithFacets()
               .OnSearch(sr => sr.SetBeforeTypesFacet(N.SupportCase.Type))
               .S();
        }

        public HTMLElement Render() => _content.Render();
    }
}
