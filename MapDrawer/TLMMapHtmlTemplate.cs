using Klyte.Commons.i18n;
using Klyte.Commons.Utils;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
/**
   OBS: Mudar a forma de pintar as linhas, criar caminhos comuns entre estações iguais (ajuda no tram)
   ver como fazer linhas de sentido único no tram
*/
namespace Klyte.TransportLinesManager.MapDrawer
{
    public class TLMMapHtmlTemplate
    {

        /// <summary>
        /// The header.<>
        /// 0 = Height
        /// 1 = Width
        /// </summary>
        public string GetHtmlHeader(CityTransportObject cto) => $@"
             <!DOCTYPE html><html><head> <meta charset='UTF-8'> 
             <style>{KlyteResourceLoader.LoadResourceString("MapDrawer.lineDrawBasicCss.css") }</style>
             <script src=""https://code.jquery.com/jquery-3.3.1.min.js"" integrity=""sha256-FgpCb/KJQlLNfOu91ta32o/NMZxltwRo8QtmkMRdAu8="" crossorigin=""anonymous""></script>
             <script>var _infoLines = {cto.ToJson()};</script>
             <script>{KlyteResourceLoader.LoadResourceString("MapDrawer.app.js") }</script>
             </head><body>
             <style id=""styleSelectionLineMap""></style>
             <svg id=""map"">
             <defs>
             <marker orient=""auto"" markerHeight=""6"" markerWidth=""6"" refY=""2.5"" refX=""1"" viewBox=""0 0 10 5"" id=""Triangle1""><path d=""M 0 0 L 10 2.5 L 0 5 z""/></marker>
             <marker orient=""auto"" markerHeight=""6"" markerWidth=""6"" refY=""2.5"" refX=""1"" viewBox=""0 0 10 5"" id=""Triangle2""><path d=""M 10 0 L 0 2.5 L 10 5 z""/></marker>
             </defs>";
        public string GetHtmlFooter(string cityName, DateTime date) => $@"<div id=""linesPanel""><div id=""title"">{cityName}</div><div id=""date"">{date.ToString(CultureInfo.GetCultures(CultureTypes.SpecificCultures).Where(c => c.TwoLetterISOLanguageName == KlyteLocaleManager.CurrentLanguageId).FirstOrDefault())}</div><div id=""content""></div></body></html>";
     

        public string GetResult(CityTransportObject cto, string cityName, DateTime currentTime)
        {
            var document = new StringBuilder(GetHtmlHeader( cto));
            document.Append("</svg><div id=\"stationsContainer\">");
            document.Append("</div>");
            document.Append(GetHtmlFooter(cityName, currentTime));
            return document.ToString();
        }
    }

}

