using ColossalFramework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/**
   OBS: Mudar a forma de pintar as linhas, criar caminhos comuns entre estações iguais (ajuda no tram)
   ver como fazer linhas de sentido único no tram
*/
namespace Klyte.TransportLinesManager.MapDrawer
{
    public class TLMMapStation
    {

        public string name;
        public Dictionary<ushort, Vector3> stopsWithWorldPos = new Dictionary<ushort, Vector3>();
        public ushort stopId;


        public List<ushort> LinesPassing { get; private set; } = new List<ushort>();
        public int Id { get; private set; }
        private ItemClass.Service service;
        private string districtName;
        private ushort districtId;

        public string ToJson() => $@"{{""name"":""{name}"",""linesPassingCount"":{ LinesPassing.Count},
                ""stops"":{{{string.Join(",", stopsWithWorldPos.Select(x => $@"""{x.Key}"":[{x.Value.x},{x.Value.y},{x.Value.z}]").ToArray())}}},
                ""stopId"":{stopId},""linesPassing"":[{string.Join(",", LinesPassing.Select(x => x.ToString()).ToArray())}],
                ""id"":{Id},""service"":""{service}"",""districtId"":{districtId},""districtName"":""{districtName}""}}";

        public TLMMapStation(string n, Vector2 pos, Vector3 worldPos, Dictionary<ushort, Vector3> stops, int stationId, ItemClass.Service service, ushort stopId)
        {
            name = n;

            stopsWithWorldPos = stops;
            Id = stationId;
            this.stopId = stopId;
            this.service = service;
            DistrictManager dm = Singleton<DistrictManager>.instance;
            districtId = dm.GetDistrict(worldPos);
            districtName = dm.GetDistrictName(districtId);
        }

        public void AddLine(ushort lineId)
        {
            if (!LinesPassing.Contains(lineId))
            {
                LinesPassing.Add(lineId);
            }
        }
        public List<ushort> GetAllStationOffsetPoints() => LinesPassing;
    }

}

