using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Reflection;
using ColossalFramework.Plugins;
using System.IO;
using ColossalFramework.Threading;
using System.Runtime.CompilerServices;
using ColossalFramework.Math;
using ColossalFramework.Globalization;

namespace TransportLinesManager 
{
	public class TransportLinesManagerMod :  IUserMod, ILoadingExtension{

		public static string version = "1.2.0";

		public string Name { 
			get { 
				
				return "Transport Lines Manager "+version; 
			} 
		}
		
		public string Description { 
			get { return "A shortcut to manage all city's public transports lines."; } 
		}
		public void OnCreated (ILoading loading){

		}
		
		public void OnLevelLoaded (LoadMode mode){

			if (TransportListInterface.instance == null) {
				TransportListInterface.instance = new TransportListInterface ();
			}
//			Log.debug ("LEVELLOAD");
		}
		
		public void OnLevelUnloading (){	
			TransportListInterface.instance.destroy();
//			Log.debug ("LEVELUNLOAD");
		}

		public void OnReleased (){

		}
		
	}
	
	public class TransportLinesManagerThreadMod : ThreadingExtensionBase
	{					
		public override void OnCreated (IThreading threading)
		{			
			if (TransportListInterface.instance != null) {
				TransportListInterface.instance.destroy ();
			}
		}
		
		public override void OnUpdate (float realTimeDelta, float simulationTimeDelta)
		{
			if (TransportListInterface.instance != null) {
				TransportListInterface.instance.init ();
			} 
		}	 
	}

	public class UIButtonLineInfo : UIButton {
		public ushort lineID;
	}

	public class UIRadialChartAge: UIRadialChart{
		public void AddSlice (Color32 innerColor, Color32 outterColor)
		{
			SliceSettings slice = new UIRadialChart.SliceSettings ();
			slice.outterColor = outterColor;
			slice.innerColor = innerColor;
			this.m_Slices.Add (slice);
			this.Invalidate ();
		}
	}

}
