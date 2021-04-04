using Klyte.TransportLinesManager.Extensions;
using System.Collections.Generic;

namespace Klyte.TransportLinesManager.Utils
{
    internal class TLMDepotUtils
    {
        public static List<ushort> GetAllDepotsFromCity(ref TransportSystemDefinition tsd)
        {
            var saida = new List<ushort>();
            BuildingManager bm = BuildingManager.instance;
            FastList<ushort> buildings = bm.GetServiceBuildings(ItemClass.Service.PublicTransport);
            foreach (ushort i in buildings)
            {
                if ((bm.m_buildings.m_buffer[i].m_flags & Building.Flags.Untouchable) == 0 && bm.m_buildings.m_buffer[i].Info.m_buildingAI is DepotAI buildingAI && buildingAI.m_maxVehicleCount > 0 && tsd.IsFromSystem(buildingAI))
                {
                    saida.Add(i);
                }
            }
            return saida;
        }
        public static List<ushort> GetAllDepotsFromCity()
        {
            var saida = new List<ushort>();
            BuildingManager bm = BuildingManager.instance;
            FastList<ushort> buildings = bm.GetServiceBuildings(ItemClass.Service.PublicTransport);
            foreach (ushort i in buildings)
            {
                BuildingAI buildingAi = bm.m_buildings.m_buffer[i].Info.m_buildingAI;
                if ((bm.m_buildings.m_buffer[i].m_flags & Building.Flags.Untouchable) != 0 && ((buildingAi is DepotAI depotAi && depotAi.m_maxVehicleCount > 0) || (buildingAi is ShelterAI)))
                {
                    saida.Add(i);
                }
            }
            return saida;
        }

    }

}

