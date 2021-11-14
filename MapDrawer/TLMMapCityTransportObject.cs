using System.Collections.Generic;
using System.Linq;
/**
   OBS: Mudar a forma de pintar as linhas, criar caminhos comuns entre estações iguais (ajuda no tram)
   ver como fazer linhas de sentido único no tram
*/
namespace Klyte.TransportLinesManager.MapDrawer
{
    public class TLMMapCityTransportObject
    {
        public Dictionary<ushort, TLMMapTransportLine> transportLines;
        public List<TLMMapStation> stations;
        public string ToJson() => $@"{{
""transportLines"":{{{string.Join(",\n", transportLines.Select(x => $"\"{x.Key}\":{x.Value.ToJson()}").ToArray())}}},
""stations"":[{string.Join(",\n", stations.Select((x, i) => x.ToJson()).ToArray())}]
}}";
    }

}

