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
        public string GetResult(TLMMapCityTransportObject cto, string cityName, DateTime currentTime) => $@"
             <!DOCTYPE html><html><head> <meta charset='UTF-8'> 
             <style>{KlyteResourceLoader.LoadResourceString("MapDrawer.lineDrawBasicCss.css") }</style>
             <script src=""https://code.jquery.com/jquery-3.6.0.min.js"" integrity=""sha256-/xUj+3OJU5yExlq6GSYGSHk7tPXikynS7ogEvDej/m4="" crossorigin=""anonymous""></script>
             <script>var _infoLines = {cto.ToJson()};</script>
             <script>{KlyteResourceLoader.LoadResourceString("MapDrawer.app.js") }</script>
             </head><body>
             <div id=""mapContainer"">
                    <div id=""mapGroup"">
                        <svg id=""map"">                 
                            <defs>                 
                                <marker orient=""auto"" markerHeight=""6"" markerWidth=""6"" refY=""2.5"" refX=""1"" viewBox=""0 0 10 5"" id=""Triangle1"">
                                    <path d=""M 0 0 L 10 2.5 L 0 5 z"" />
                                </marker>
                                <marker orient=""auto"" markerHeight=""6"" markerWidth=""6"" refY=""2.5"" refX=""1"" viewBox=""0 0 10 5"" id=""Triangle2"">
                                    <path d=""M 10 0 L 0 2.5 L 10 5 z"" />
                                </marker>
                            </defs>
                        </svg>
                        <div id=""stationsContainer""></div>
                    </div>
                </div>
            </div>
            <div id=""linesPanel"">
                <div id=""title"">{cityName}</div>
                <div id=""date"">{currentTime.ToString(CultureInfo.GetCultures(CultureTypes.SpecificCultures).Where(c => c.TwoLetterISOLanguageName == KlyteLocaleManager.CurrentLanguageId).FirstOrDefault())}</div>
                <div id=""content"">
            </div></body></html>";     
    }

}

