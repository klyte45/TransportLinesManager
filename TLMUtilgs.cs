using UnityEngine;
using System.Collections;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using System.Reflection;
using ColossalFramework.Plugins;
using System.IO;
using ColossalFramework.Threading;
using System.Runtime.CompilerServices;
using ColossalFramework.Math;
using ColossalFramework.Globalization;

namespace Klyte.TransportLinesManager
{

	public class TLMUtils
	{	
		
		public static  void createUIElement<T> (ref T uiItem, Transform parent) where T : Component
		{			
			GameObject container = new GameObject ();
			container.transform.parent = parent;
			uiItem = container.AddComponent<T> ();	
		}

		public static  void uiTextFieldDefaults (UITextField uiItem)
		{
			uiItem.selectionSprite = "EmptySprite";
			uiItem.useOutline = true;
			uiItem.hoveredBgSprite = "TextFieldPanelHovered";
			uiItem.focusedBgSprite = "TextFieldPanel";
			uiItem.builtinKeyNavigation = true;
			uiItem.submitOnFocusLost = true;
		}
		
		public static   Color contrastColor (Color color)
		{
			int d = 0;
			
			// Counting the perceptive luminance - human eye favors green color... 
			double a = (0.299 * color.r + 0.587 * color.g + 0.114 * color.b);
			
			if (a > 0.5)
				d = 0; // bright colors - black font
			else
				d = 1; // dark colors - white font
			
			return  new Color (d, d, d, 1);
		}

		public static  float calcBezierLenght (Vector3 a, Vector3 b, Vector3 c, Vector3 d, float precision)
		{
			
			Vector3 aa = (-a + 3 * (b - c) + d);
			Vector3 bb = 3 * (a + c) - 6 * b;
			Vector3 cc = 3 * (b - a);
			
			int len = (int)(1.0f / precision);
			float[] arcLengths = new float[len + 1];
			arcLengths [0] = 0;
			
			Vector3 ov = a;
			Vector3 v;
			float clen = 0.0f;
			for (int i = 1; i <= len; i++) {
				float t = (i * precision);
				v = ((aa * t + (bb)) * t + cc) * t + a;
				clen += (ov - v).magnitude;
				arcLengths [i] = clen;
				ov = v;
			}
			return clen;
			
		}

		public static void createDragHandle (UIComponent parent, UIComponent target)
		{
			createDragHandle (parent, target, -1);
		}

		public static void createDragHandle (UIComponent parent, UIComponent target, float height)
		{
			UIDragHandle dh = null;
			createUIElement<UIDragHandle> (ref dh, parent.transform);		
			dh.target = target;
			dh.relativePosition = new Vector3 (0, 0);
			dh.width = parent.width;
			dh.height = height < 0 ? parent.height : height;
			dh.name = "DragHandle";
			dh.Start ();
		}
		
		public static   void initButton (UIButton button, bool isCheck, string baseSprite)
		{
			string sprite = baseSprite;//"ButtonMenu";
			string spriteHov = baseSprite + "Hovered";
			button.normalBgSprite = sprite;
			button.disabledBgSprite = sprite + "Disabled";
			button.hoveredBgSprite = spriteHov;
			button.focusedBgSprite = spriteHov;
			button.pressedBgSprite = isCheck ? sprite + "Pressed" : spriteHov;
			button.textColor = new Color32 (255, 255, 255, 255);			
		}
		
		public static   void initButtonSameSprite (UIButton button, string baseSprite)
		{
			string sprite = baseSprite;//"ButtonMenu";
			button.normalBgSprite = sprite;
			button.disabledBgSprite = sprite;
			button.hoveredBgSprite = sprite;
			button.focusedBgSprite = sprite;
			button.pressedBgSprite = sprite;
			button.textColor = new Color32 (255, 255, 255, 255);			
		}

		public static   void initButtonFg (UIButton button, bool isCheck, string baseSprite)
		{
			string sprite = baseSprite;//"ButtonMenu";
			string spriteHov = baseSprite + "Hovered";
			button.normalFgSprite = sprite;
			button.disabledFgSprite = sprite;
			button.hoveredFgSprite = spriteHov;
			button.focusedFgSprite = spriteHov;
			button.pressedFgSprite = isCheck ? sprite + "Pressed" : spriteHov;
			button.textColor = new Color32 (255, 255, 255, 255);			
		}
		
		public static void copySpritesEvents (UIButton source, UIButton target)
		{			
			target.disabledBgSprite = source.disabledBgSprite;
			target.focusedBgSprite = source.focusedBgSprite;
			target.hoveredBgSprite = source.hoveredBgSprite;
			target.normalBgSprite = source.normalBgSprite;
			target.pressedBgSprite = source.pressedBgSprite;
			
			target.disabledFgSprite = source.disabledFgSprite;
			target.focusedFgSprite = source.focusedFgSprite;
			target.hoveredFgSprite = source.hoveredFgSprite;
			target.normalFgSprite = source.normalFgSprite;
			target.pressedFgSprite = source.pressedFgSprite;
			
		}
		
		public static string getString (ModoNomenclatura m, int numero)
		{

			switch (m) {
			case ModoNomenclatura.GregoMaiusculo:
				return getStringFromNumber (gregoMaiusculo, numero);
			case ModoNomenclatura.GregoMinusculo:
				return getStringFromNumber (gregoMinusculo, numero);
			case ModoNomenclatura.CirilicoMaiusculo:
				return getStringFromNumber (cirilicoMaiusculo, numero);
			case ModoNomenclatura.CirilicoMinusculo:
				return getStringFromNumber (cirilicoMinusculo, numero);	
			case ModoNomenclatura.LatinoMaiusculo:
				return getStringFromNumber (latinoMaiusculo, numero);
			case ModoNomenclatura.LatinoMinusculo:
				return getStringFromNumber (latinoMinusculo, numero);				
			default:
				return "" + numero;
			}
		}
		
		public static string getStringFromNumber (char[] array, int number)
		{
			int arraySize = array.Length;
			string saida = "";
			while (number > 0) {
				int idx = (number-1) % arraySize;
				saida = "" + array [idx] + saida;
				if(number%arraySize == 0){
					number /= arraySize;
					number--;
				} else {
					number /= arraySize;
				}

			}
			return saida;
		}

		public static void setLineColor(ushort lineIdx , Color color){			
			
			Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineIdx].m_color = color;
			Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineIdx].m_flags |= TransportLine.Flags.CustomColor;
		}

		public static void  setLineName(ushort lineIdx, string name){				
			InstanceID lineIdSelecionado = default(InstanceID);
			lineIdSelecionado.TransportLine = lineIdx;	
			if (name.Length > 0) {
				Singleton<InstanceManager>.instance.SetName (lineIdSelecionado, name);
				Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineIdx].m_flags |= TransportLine.Flags.CustomName;
			} else {				
				Singleton<TransportManager>.instance.m_lines.m_buffer [(int)lineIdx].m_flags &= ~TransportLine.Flags.CustomName;
			}
		}

		public static string calculateAutoName(ushort lineIdx){	
			TransportManager tm = Singleton<TransportManager>.instance;
			TransportLine t =tm.m_lines.m_buffer [(int)lineIdx];
			ItemClass.SubService ss = ItemClass.SubService.None;	
			if (t.Info.m_transportType == TransportInfo.TransportType.Train) {
				ss = ItemClass.SubService.PublicTransportTrain;
			} else if (t.Info.m_transportType == TransportInfo.TransportType.Metro) {
				ss = ItemClass.SubService.PublicTransportMetro;							
			} 
			int stopsCount = t.CountStops (lineIdx);
			string m_autoName = "";
			ushort[] stopBuildings = new ushort[stopsCount];
			MultiMap<ushort,Vector3> bufferToDraw = new MultiMap<ushort,Vector3>();
			int perfectSimetricLineStationsCount = (stopsCount+2)/2;
			bool simetric = t.Info.m_transportType != TransportInfo.TransportType.Bus ;	
			int middle = -1;
			if (simetric) {	
				simetric = CalculateSimmetry (ss, stopsCount, t, out middle);
			}
			if(simetric){
				return getStationName (t.GetStop (middle), ss) + " - " + getStationName (t.GetStop (middle+stopsCount/2), ss);		
			} else {
				DistrictManager dm = Singleton<DistrictManager>.instance;
				byte lastDistrict = 0;
				Vector3 local;
				byte district;
				List<int> districtList = new List<int>();
				NetManager nm = Singleton<NetManager>.instance;
				for (int j = 0; j<stopsCount; j++) {	
					local  = nm.m_nodes.m_buffer [(int)t.GetStop(j)].m_bounds.center;
					district = dm.GetDistrict(local);
					if((district != lastDistrict) && district != 0){
						districtList.Add(district);
					}
					if(district != 0){
						lastDistrict = district;
					}
				}			
				
				local = nm.m_nodes.m_buffer [(int)t.GetStop(0)].m_bounds.center;
				district = dm.GetDistrict(local);
				if((district != lastDistrict) && district != 0){
					districtList.Add(district);
				}
				middle=-1;
				int[] districtArray = districtList.ToArray();
				if(districtArray.Length ==1){
					return  (TransportLinesManagerMod.savedCircularOnSingleDistrict.value?"Circular ":"")+dm.GetDistrictName(districtArray[0]);
				}else if(findSimetry(districtArray,out middle)){
					int firstIdx = middle;
					int lastIdx = middle + districtArray.Length/2;
					
					m_autoName = dm.GetDistrictName(districtArray[firstIdx%districtArray.Length])+" - "+dm.GetDistrictName(districtArray[lastIdx%districtArray.Length]);
					if(lastIdx-firstIdx>1){
						m_autoName += ", via ";
						for(int k = firstIdx+1; k<lastIdx;k++){
							m_autoName += dm.GetDistrictName(districtArray[k%districtArray.Length]);
							if(k+1!=lastIdx){
								m_autoName += ", ";
							}
						}
					}
					return m_autoName;
				} else {
					bool inicio = true;
					foreach(int i in districtArray){
						m_autoName += (inicio ? "" :" - ") + dm.GetDistrictName(i);
						inicio = false;
					}
					return m_autoName;
				}
			}
		}

		static bool CalculateSimmetry (ItemClass.SubService ss, int stopsCount, TransportLine t,  out int middle)
		{
			int j;
			NetManager nm = Singleton<NetManager>.instance;
			BuildingManager bm = Singleton<BuildingManager>.instance;
			middle = -1;
			//try to find the loop
			for (j = -1; j < stopsCount / 2; j++) {
				int offsetL = (j + stopsCount) % stopsCount;
				int offsetH = (j + 2) % stopsCount;
				NetNode nn1 = nm.m_nodes.m_buffer [(int)t.GetStop (offsetL)];
				NetNode nn2 = nm.m_nodes.m_buffer [(int)t.GetStop (offsetH)];
				ushort buildingId1 = bm.FindBuilding (nn1.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
				ushort buildingId2 = bm.FindBuilding (nn2.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
				//					DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"buildingId1="+buildingId1+"|buildingId2="+buildingId2);
				//					DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"offsetL="+offsetL+"|offsetH="+offsetH);
				if (buildingId1 == buildingId2) {
					middle = j + 1;
					break;
				}
			}
			//				DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"middle="+middle);
			if (middle >= 0) {
				for (j = 1; j <= stopsCount / 2; j++) {
					int offsetL = (-j + middle + stopsCount) % stopsCount;
					int offsetH = (j + middle) % stopsCount;
					//						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"offsetL="+offsetL+"|offsetH="+offsetH);
					//						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"t.GetStop (offsetL)="+t.GetStop (offsetH)+"|t.GetStop (offsetH)="+t.GetStop (offsetH));
					NetNode nn1 = nm.m_nodes.m_buffer [(int)t.GetStop (offsetL)];
					NetNode nn2 = nm.m_nodes.m_buffer [(int)t.GetStop (offsetH)];
					ushort buildingId1 = bm.FindBuilding (nn1.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
					ushort buildingId2 = bm.FindBuilding (nn2.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
					//						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"buildingId1="+buildingId1+"|buildingId2="+buildingId2);
					//						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"buildingId1="+buildingId1+"|buildingId2="+buildingId2);
					//						DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"offsetL="+offsetL+"|offsetH="+offsetH);
					if (buildingId1 != buildingId2) {
						return false;
					}
				}
			}
			else {
				return false;
			}
			return true;
		}

		public static string getStationName (uint stopId, ItemClass.SubService ss)
		{
				NetManager nm = Singleton<NetManager>.instance;
				BuildingManager bm = Singleton<BuildingManager>.instance;
				NetNode nn = nm.m_nodes.m_buffer [(int)stopId];
				ushort buildingId;
				if (ss != ItemClass.SubService.None) {
					buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.PublicTransport, ss, Building.Flags.None, Building.Flags.None);
				}
				else {
					buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.PublicTransport, ItemClass.SubService.None, Building.Flags.Active, Building.Flags.None);
					if (buildingId == 0) {
						buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Monument, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
					}
					if (buildingId == 0) {
						buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Beautification, ItemClass.SubService.None, Building.Flags.Active, Building.Flags.None);
					}
					if (buildingId == 0) {
						buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Government, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
					}
					if (buildingId == 0) {
						buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.HealthCare, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
					}
					if (buildingId == 0) {
						buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.FireDepartment, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
					}
					if (buildingId == 0) {
						buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.PoliceDepartment, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
					}
					if (buildingId == 0) {
						buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Tourism, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
					}
					if (buildingId == 0) {
						buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Education, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
					}
					if (buildingId == 0) {
						buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Garbage, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
					}
					if (buildingId == 0) {
						buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Office, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
					}
					if (buildingId == 0) {
						buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Commercial, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
					}
					if (buildingId == 0) {
						buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Industrial, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
					}
					if (buildingId == 0) {
						buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Water, ItemClass.SubService.None, Building.Flags.Active, Building.Flags.None);
					}
					if (buildingId == 0) {
						buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Electricity, ItemClass.SubService.None, Building.Flags.Active, Building.Flags.None);
					}
					if (buildingId == 0) {
						buildingId = bm.FindBuilding (nn.m_position, 100f, ItemClass.Service.Residential, ItemClass.SubService.None, Building.Flags.None, Building.Flags.None);
					}
				}
				if (buildingId > 0) {
					Building b = bm.m_buildings.m_buffer [buildingId];
					InstanceID iid = default(InstanceID);
					iid.Building = buildingId;
					return bm.GetBuildingName (buildingId, iid);
				}
				else {
					Vector3 location = nn.m_position;
					DistrictManager dm = Singleton<DistrictManager>.instance;
					int dId = dm.GetDistrict (location);
					if (dId > 0) {
						District d = dm.m_districts.m_buffer [dId];
						return  "[D] " + dm.GetDistrictName (dId);
					} else {						
						return "[X=" + location.x + "|Y=" + location.y + "|Z=" + location.z + "]";
					}
				}
			
		}

		public static bool findSimetry(int[] array, out int middle){
			middle = -1;
			int size = array.Length;
			if (size == 0)
				return false;
			for (int j = -1; j<size/2; j++) {					
				int offsetL = (j+size)%size;
				int offsetH = (j+2)%size;									
				if(array[offsetL]==array[offsetH]){
					middle = j+1;
					break;
				}
			}
			//			DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning,"middle="+middle);
			if(middle>=0){	
				for (int k = 1; k<=size/2; k++) {
					int offsetL = (-k+middle+size)%size;
					int offsetH = (k+middle)%size;
					if(array[offsetL]!=array[offsetH]){
						return false;
					}
				}				
			}else{
				return false;
			}
			return true;
		}

		public class UIButtonLineInfo : UIButton
		{
			public ushort lineID;
		}

		private static char[] latinoMaiusculo = {
			'A',
			'B',
			'C',
			'D',
			'E',
			'F',
			'G',
			'H',
			'I',
			'J',
			'K',
			'L',
			'M',
			'N',
			'O',
			'P',
			'Q',
			'R',
			'S',
			'T',
			'U',
			'V',
			'W',
			'X',
			'Y',
			'Z'
		};
		private static char[] latinoMinusculo = {
			'a',
			'b',
			'c',
			'd',
			'e',
			'f',
			'g',
			'h',
			'i',
			'j',
			'k',
			'l',
			'm',
			'n',
			'o',
			'p',
			'q',
			'r',
			's',
			't',
			'u',
			'v',
			'w',
			'x',
			'y',
			'z'
		};

		private static char[] gregoMaiusculo = {
			'Α',
			'Β',
			'Γ',
			'Δ',
			'Ε',
			'Ζ',
			'Η',
			'Θ',
			'Ι',
			'Κ',
			'Λ',
			'Μ',
			'Ν',
			'Ξ',
			'Ο',
			'Π',
			'Ρ',
			'Σ',
			'Τ',
			'Υ',
			'Φ',
			'Χ',
			'Ψ',
			'Ω'
		};
		private static char[] gregoMinusculo = {
			'α',
			'β',
			'γ',
			'δ',
			'ε',
			'ζ',
			'η',
			'θ',
			'ι',
			'κ',
			'λ',
			'μ',
			'ν',
			'ξ',
			'ο',
			'π',
			'ρ',
			'σ',
			'τ',
			'υ',
			'φ',
			'χ',
			'ψ',
			'ω'
		};
		private static char[] cirilicoMaiusculo = {
			'А',
			'Б',
			'В',
			'Г',
			'Д',
			'Е',
			'Ё',
			'Ж',
			'З',
			'И',
			'Й',
			'К',
			'Л',
			'М',
			'Н',
			'О',
			'П',
			'Р',
			'С',
			'Т',
			'У',
			'Ф',
			'Х',
			'Ц',
			'Ч',
			'Ш',
			'Щ',
			'Ъ',
			'Ы',
			'Ь',
			'Э',
			'Ю',
			'Я'
		};
		private static char[] cirilicoMinusculo = {
			'а',
			'б',
			'в',
			'г',
			'д',
			'е',
			'ё',
			'ж',
			'з',
			'и',
			'й',
			'к',
			'л',
			'м',
			'н',
			'о',
			'п',
			'р',
			'с',
			'т',
			'у',
			'ф',
			'х',
			'ц',
			'ч',
			'ш',
			'щ',
			'ъ',
			'ы',
			'ь',
			'э',
			'ю',
			'я'
		};	
		
	}
}

