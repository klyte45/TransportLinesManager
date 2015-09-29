using ColossalFramework.Globalization;
using System;
using System.Collections.Generic;
namespace Klyte.TransportLinesManager
{
	public class VehiclePrefabs
	{
		//
		// Static Fields
		//
		public static VehiclePrefabs instance;
		
		//
		// Fields
		//		
		private VehicleInfo[] busPrefabData;		
		private VehicleInfo[] metroPrefabData;		
		private VehicleInfo[] trainPrefabData;

		public static void Deinit ()
		{
			VehiclePrefabs.instance = null;
		}
		
		public static void Init ()
		{
			VehiclePrefabs.instance = new VehiclePrefabs ();
			VehiclePrefabs.instance.FindAllPrefabs ();
		}

		private void FindAllPrefabs ()
		{
			List<VehicleInfo> buses = new List<VehicleInfo> ();
			List<VehicleInfo> metros = new List<VehicleInfo> ();
			List<VehicleInfo> trains = new List<VehicleInfo> ();
			uint num = 0u;
			while ((ulong)num < (ulong)((long)PrefabCollection<VehicleInfo>.PrefabCount ()))
			{
				VehicleInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab (num);
				if (prefab != null && !this.IsTrailer (prefab) && prefab.m_class.m_service == ItemClass.Service.PublicTransport && prefab.m_class.m_level == ItemClass.Level.Level1)
				{
					if (prefab.m_class.m_subService == ItemClass.SubService.PublicTransportBus)
					{
						buses.Add (prefab);
					}
					else
					{
						if (prefab.m_class.m_subService == ItemClass.SubService.PublicTransportMetro)
						{
							metros.Add (prefab);
						}
						else
						{
							if (prefab.m_class.m_subService == ItemClass.SubService.PublicTransportTrain)
							{
								trains.Add (prefab);
							}
						}
					}
				}
				num += 1u;
			}
			this.busPrefabData = buses.ToArray ();
			this.metroPrefabData = metros.ToArray ();
			this.trainPrefabData = trains.ToArray ();
		}
		
		public VehicleInfo[] GetPrefab (ItemClass.SubService subService)
		{
			if (subService == ItemClass.SubService.PublicTransportBus)
			{
				return this.busPrefabData;
			}
			if (subService == ItemClass.SubService.PublicTransportMetro)
			{
				return this.metroPrefabData;
			}
			if (subService == ItemClass.SubService.PublicTransportTrain)
			{
				return this.trainPrefabData;
			}
			return null;
		}
		private bool IsTrailer (VehicleInfo prefab)
		{
			string @unchecked = ColossalFramework.Globalization.Locale.GetUnchecked ("VEHICLE_TITLE", prefab.name);
			return @unchecked.StartsWith ("VEHICLE_TITLE") || @unchecked.StartsWith ("Trailer");
		}
	}

	public static class VehicleExtensions{
		public static int CurrentCapacity(this VehicleInfo v){
			var ai = v.GetAI ();
			if (ai.GetType()==typeof(BusAI)) {
				return ((BusAI) ai).m_passengerCapacity;
			}
			if (ai.GetType()==typeof(PassengerTrainAI)) {
				return ((PassengerTrainAI) ai).m_passengerCapacity;
			}
			if (ai.GetType()==typeof(MetroTrainAI)) {
				return ((MetroTrainAI) ai).m_passengerCapacity;
			}
			return 0;
		}
		
		public static void SetCapacity(this VehicleInfo v, int newCapacity){
			var ai = v.GetAI ();
			if (ai.GetType()==typeof(BusAI)) {
				((BusAI) ai).m_passengerCapacity=newCapacity;
			}
			if (ai.GetType()==typeof(PassengerTrainAI)) {
				((PassengerTrainAI) ai).m_passengerCapacity=newCapacity;
			}
			if (ai.GetType()==typeof(MetroTrainAI)) {
				((MetroTrainAI) ai).m_passengerCapacity=newCapacity;
			}
		}
	}
}
