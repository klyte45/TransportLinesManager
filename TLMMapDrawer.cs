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
using System.Web;

namespace Klyte.TransportLinesManager
{
	public class TLMMapDrawer
	{

		private static Color almostWhite = new Color (0.9f, 0.9f, 0.9f);

		public static void drawCityMap ()
		{
			CalculateCoords calc = TLMLineUtils.gridPosition81Tiles;
			NetManager nm = NetManager.instance;	
			TLMController controller = TLMController.instance;
			Dictionary<Vector2, Station> stations = new Dictionary<Vector2, Station> ();
			Dictionary<Segment2,Color32> lines = new Dictionary<Segment2,Color32> ();
			MultiMap<Vector2,Vector2> intersects = new MultiMap<Vector2,Vector2> ();
			float nil = 0;
//			List<int> usedX = new List<int> ();
//			List<int> usedY = new List<int> ();
			for (ushort i =0; i < controller.tm.m_lines.m_size; i++) {
				TransportLine t = controller.tm.m_lines.m_buffer [(int)i];
				if (t.Info.m_transportType == TransportInfo.TransportType.Metro || t.Info.m_transportType == TransportInfo.TransportType.Train) {
					int stopsCount = t.CountStops (i);
					if (stopsCount == 0) {
						continue;
					}
					Color color = t.m_color;
					Vector2 ultPos = Vector2.zero;
					Segment2 lastSeg = default(Segment2);
				
					int startStop = 0;
					int finalStop = stopsCount;
					int middle = 0;
					if (TLMUtils.CalculateSimmetry (t.Info.m_stationSubService, stopsCount, t, out middle)) {
						startStop = middle;
						finalStop = middle + stopsCount / 2 + 1;
					}
					for (int j=startStop; j< finalStop; j++) {
//						Debug.Log ("ULT POS:" + ultPos);
						ushort nextStop = t.GetStop (j % stopsCount);
						string name = TLMUtils.getStationName (nextStop, t.Info.m_stationSubService);
						Vector2 pos = calc (nm.m_nodes.m_buffer [nextStop].m_position);
						Vector2 gridAdd = Vector2.zero;
						
						ushort nextNextStop = t.GetStop (j % stopsCount);
						Vector2 gridNextPos = calc (nm.m_nodes.m_buffer [nextNextStop].m_position);
						float angle = -GetAngleOfLineBetweenTwoPoints (pos, gridNextPos);
						CardinalPoint cardinal = CardinalPoint.getCardinalPoint (angle);

						bool intersectStation = false;
						int countIterations = 0;
						while (stations.Keys.Contains(pos) || lines.Keys.Where(x=>x.DistanceSqr(pos,out nil)==0).Count()>0) {
//							Debug.Log ("COUNT:" + lines.Keys.Where(x=>x.DistanceSqr(pos,out nil)==0).Count());
							if (!intersectStation && stations.Keys.Contains (pos)) {
								intersectStation = true;
							}
							if (gridAdd == Vector2.zero) {
								gridAdd = getCardinalOffset (cardinal);
							} else {
								pos -= gridAdd;
								gridAdd = getCardinalOffset (cardinal++) * (1 + (countIterations / 8));
							}
							pos += gridAdd;
							countIterations++;
						}



						if (ultPos != Vector2.zero) {
							Vector2 d = pos - ultPos;
							if (Math.Abs (d.x) == Math.Abs (d.y) || Math.Abs (d.y) == 0 || Math.Abs (d.x) == 0) { //0º,45º,90º
//								Debug.Log ("90,45,0!" );
								Segment2 s = new Segment2 (ultPos, pos);
								float sAngle = Vector2.Angle (s.a, s.b);
								Segment2[] intersectorsSegments = lines.Keys.AsQueryable ().Where (x => Vector2.Angle (x.a, x.b) == sAngle && x.Intersect (s)).ToArray ();
								Vector2[] intersectorStations = stations.Keys.Where (x => s.DistanceSqr (x, out nil) == 0 && x != s.a && x != s.b).ToArray ();
								if (intersectorsSegments.Length > 0 || intersectorStations.Length > 0) {
									
//									Debug.Log ("TEMP!" );
									float lineAngle = sAngle * (180f / (float)Math.PI);
									float detourAngle1 = lineAngle - 45;
									float detourAngle2 = lineAngle + 45;
									CardinalPoint detourCard1 = CardinalPoint.getCardinalPoint (detourAngle1);
									CardinalPoint detourCard2 = CardinalPoint.getCardinalPoint (detourAngle2);
									CardinalPoint lineCard = CardinalPoint.getCardinalPoint (lineAngle);
									Vector2 detourOffset1 = getCardinalOffset (detourCard1);//a-v
									Vector2 detourOffset2 = getCardinalOffset (detourCard2);//w-b
									Vector2 lineParallel = getCardinalOffset (lineCard);

									List<Vector2> pointsToDetour = new List<Vector2> (intersectorStations);
									foreach (Segment2 seg in intersectorsSegments) {
										pointsToDetour.Add (seg.a);
										pointsToDetour.Add (seg.b);
									}
									Vector2 closerA = s.b;
									Vector2 closerB = s.a;
									float minDistA = s.LengthSqr ();
									float minDistB = minDistA;
									foreach (Vector2 v in pointsToDetour) {
										float distA = Vector2.Distance (s.a, v);
										float distB = Vector2.Distance (s.b, v);
										if (distA < minDistA) {
											closerA = v;
											minDistA = distA;
										}										
										if (distB < minDistB) {
											closerB = v;
											minDistB = distB;
										}
									}
									//TEMP!!!!
									lines.Add (new Segment2 (s.a, closerA - lineParallel), color);
									lines.Add (new Segment2 (closerA - lineParallel, closerA - lineParallel + detourOffset1), color);
									lines.Add (new Segment2 (closerA - lineParallel + detourOffset1, closerB + lineParallel + detourOffset2), color);
									lines.Add (new Segment2 (closerB + lineParallel + detourOffset2, closerB + lineParallel), color);									
									lastSeg = new Segment2 (closerB + lineParallel, s.b);
									lines.Add (lastSeg, color);
								} else {
//									Debug.Log ("90,45,0!");
									lines.Add (s, color);
									lastSeg = s;
								}
							} else {
								Segment2 s1, s2;
								s1 = new Segment2 (ultPos, ultPos + new Vector2 (Math.Min (Math.Abs (d.x), Math.Abs (d.y)) * Math.Sign (d.x), Math.Min (Math.Abs (d.x), Math.Abs (d.y)) * Math.Sign (d.y)));
								s2 = new Segment2 (s1.b, pos);
								
								lines.Add (s1, color);
								lines.Add (s2, color);
								lastSeg = s2;
							}
						}

						
								
							CardinalPoint cp = CardinalPoint.E;
							int vizinhosVazios = 0;
							int maxVizinhosVazios = 0;
							CardinalPoint melhorCp = cp;
							for(int v = 0; v<8;v++,cp++){
								Vector2 testPos = pos+getCardinalOffset(cp);
								if(!stations.Keys.Contains(testPos) && lines.Keys.Where(x=>x.DistanceSqr(testPos,out nil)==0).Count()==0){
									vizinhosVazios++;
									if(vizinhosVazios > maxVizinhosVazios){
										maxVizinhosVazios = vizinhosVazios;
										melhorCp = cp;
									}
								}else if(stations.Keys.Contains(testPos)){									
									intersects.Add (pos, testPos);
									name="";
								}
							}
							stations.Add (pos, new Station(name,melhorCp));

//						Debug.Log ("POS:" + pos);
						ultPos = pos;
					}
				}
			}
			float minX = Math.Min(lines.Min (x => Math.Min (x.Key.a.x, x.Key.b.x)), stations.Min(x=> x.Key.x));
			float minY = Math.Min(lines.Min (x => Math.Min (x.Key.a.y, x.Key.b.y)), stations.Min(x=> x.Key.y));
			float maxX = Math.Max(lines.Max (x => Math.Max (x.Key.a.x, x.Key.b.x)), stations.Max(x=> x.Key.x));
			float maxY = Math.Max(lines.Max (x => Math.Max (x.Key.a.y, x.Key.b.y)), stations.Max(x=> x.Key.y));

			SVGTemplate svg = new SVGTemplate ((int)(maxY-minY+4)*30, (int)(maxX-minX+4)*30, 30,minX-2,minY-2);
			foreach (var line in lines) {
				svg.addLineSegment (line.Key.a, line.Key.b, line.Value);
			}
			foreach (var intersectKey in intersects.Keys) {
				List<Vector2> intersections;
				intersects.TryGetValue(intersectKey, out intersections);
				foreach(var intersect in intersections){
					svg.addLineSegment (intersectKey, intersect, Color.gray);
				}
			}
			foreach (var station in stations) {
				CardinalPoint angle = station.Value.preferredAngle;
				if (!string.IsNullOrEmpty (station.Value.name)) {
					Vector2 testingPoint = station.Key + getCardinalOffset (angle);
					int countIterations = 0;
					while (stations.Keys.Contains(testingPoint) || lines.Keys.Where(x=>x.DistanceSqr(testingPoint,out nil)==0).Count()>0) {
						//							Debug.Log ("COUNT:" + lines.Keys.Where(x=>x.DistanceSqr(pos,out nil)==0).Count());
						angle++;
						testingPoint = station.Key + getCardinalOffset (angle);
						countIterations++;
						if (countIterations >= 8) {
							break;
						}
					}
				}
				svg.addStation (station.Key, getCardinalAngle (angle), station.Value.name);
			}
			String folder = "Transport Lines Manager";
			if (File.Exists (folder) && (File.GetAttributes (folder) & FileAttributes.Directory) != FileAttributes.Directory) {
				File.Delete (folder);
			} 
			if (!Directory.Exists (folder)) {
				Directory.CreateDirectory (folder);
			}
			String filename = folder + Path.DirectorySeparatorChar + "0TLM_MAP_" + Singleton<SimulationManager>.instance.m_metaData.m_CityName + "_" + Singleton<SimulationManager>.instance.m_currentGameTime.ToString ("yyyy.MM.dd") + ".html";
			if (File.Exists (filename)) {
				File.Delete (filename);
			}
			var sr = File.CreateText (filename);
			sr.WriteLine (svg.getResult ());
			sr.Close ();
		}

		struct Station {
			public string name;
			public CardinalPoint preferredAngle;

			public Station (string n, CardinalPoint pref){
				name = n;
				preferredAngle = pref;
			}
		}



		private delegate Vector2 CalculateCoords (Vector3 pos);

		public static float GetAngleOfLineBetweenTwoPoints (Vector2 p1, Vector2 p2)
		{

			return (float)(Vector2.Angle (p1, p2) * (180f / Math.PI));
		}

		private static  Vector2 getCardinalOffset (CardinalPoint p)
		{

			switch (p.Value) {
			case CardinalPoint.CardinalInternal.E:
				return new Vector2 (1, 0);
			case CardinalPoint.CardinalInternal.W:
				return new Vector2 (-1, 0);
			case CardinalPoint.CardinalInternal.N:
				return new Vector2 (0, 1);
			case CardinalPoint.CardinalInternal.S:
				return new Vector2 (0, -1);
			case CardinalPoint.CardinalInternal.NE:
				return new Vector2 (1, 1);
			case CardinalPoint.CardinalInternal.NW:
				return new Vector2 (-1, 1);
			case CardinalPoint.CardinalInternal.SE:
				return new Vector2 (1, -1);
			case CardinalPoint.CardinalInternal.SW:
				return new Vector2 (-1, -1);

			}
			return Vector2.zero;
		}

		private static  int getCardinalAngle (CardinalPoint p)
		{
			
			switch (p.Value) {
			case CardinalPoint.CardinalInternal.E:
				return 0;
			case CardinalPoint.CardinalInternal.W:
				return 180;
			case CardinalPoint.CardinalInternal.N:
				return 90;
			case CardinalPoint.CardinalInternal.S:
				return 270;
			case CardinalPoint.CardinalInternal.NE:
				return 45;
			case CardinalPoint.CardinalInternal.NW:
				return 135;
			case CardinalPoint.CardinalInternal.SE:
				return 315;
			case CardinalPoint.CardinalInternal.SW:
				return 225;				
			}
			return 0;
		}






	}

	public struct CardinalPoint
	{
		public static CardinalPoint getCardinalPoint (float angle)
		{
			angle %= 360;
			angle += 360;
			angle %= 360;
			
			if (angle < 157.5f && angle >= 112.5f) {
				return CardinalPoint.NW;
			} else if (angle < 112.5f && angle >= 67.5f) {
				return CardinalPoint.N;
			} else if (angle < 67.5f && angle >= 22.5f) {
				return CardinalPoint.NE;
			} else if (angle < 22.5f || angle >= 337.5f) {
				return CardinalPoint.E;
			} else if (angle < 337.5f && angle >= 292.5f) {
				return CardinalPoint.SE;
			} else if (angle < 292.5f && angle >= 247.5f) {
				return CardinalPoint.S;
			} else if (angle < 247.5f && angle >= 202.5f) {
				return CardinalPoint.SW;
			} else {
				return CardinalPoint.W;
			}
			
		}
		
		private CardinalInternal InternalValue { get; set; }
		
		public CardinalInternal Value { get { return InternalValue; } }
		
		public static readonly  CardinalPoint N = CardinalInternal.N;
		public static readonly  CardinalPoint E = CardinalInternal.E;
		public static readonly  CardinalPoint S = CardinalInternal.S;
		public static readonly  CardinalPoint W = CardinalInternal.W;
		public static readonly  CardinalPoint NE = CardinalInternal.NE;
		public static readonly  CardinalPoint SE = CardinalInternal.SE;
		public static readonly  CardinalPoint SW = CardinalInternal.SW;
		public static readonly  CardinalPoint NW = CardinalInternal.NW;
		
		public static implicit operator CardinalPoint (CardinalInternal otherType)
		{
			return new CardinalPoint
			{
				InternalValue = otherType
			};
		}
		
		public static implicit operator CardinalInternal (CardinalPoint otherType)
		{
			return otherType.InternalValue;
		}
		
		public static CardinalPoint operator ++ (CardinalPoint c)
		{
			switch (c.InternalValue) {
			case CardinalInternal.N:
				return NE;
			case CardinalInternal.NE:
				return E;
			case CardinalInternal.E:
				return SE;
			case CardinalInternal.SE:
				return S;
			case CardinalInternal.S:
				return SW;
			case CardinalInternal.SW:
				return W;
			case CardinalInternal.W:
				return NW;
			case CardinalInternal.NW:
				return N;
			default:
				return N;
			}
		}
		
		public static CardinalPoint operator -- (CardinalPoint c)
		{
			switch (c.InternalValue) {
			case CardinalInternal.N:
				return NW;
			case CardinalInternal.NE:
				return N;
			case CardinalInternal.E:
				return NE;
			case CardinalInternal.SE:
				return E;
			case CardinalInternal.S:
				return SE;
			case CardinalInternal.SW:
				return S;
			case CardinalInternal.W:
				return SW;
			case CardinalInternal.NW:
				return W;
			default:
				return S;
			}
		}
		
		public static CardinalPoint operator & (CardinalPoint c1, CardinalPoint c2)
		{
			return new CardinalPoint{
				InternalValue = c1.InternalValue & c2.InternalValue
			};
		}
		
		public static CardinalPoint operator | (CardinalPoint c1, CardinalPoint c2)
		{
			return new CardinalPoint{
				InternalValue = c1.InternalValue | c2.InternalValue
			};
		}
		
		public enum CardinalInternal
		{
			N = 1,
			E = 2,
			S = 4,
			W = 8,
			NE = 0x10,
			SE = 0x20,
			SW = 0x40,
			NW = 0x80
		}
	}

	public class SVGTemplate
	{
		/// <summary>
		/// The header.<>
		/// 0 = Height
		/// 1 = Width
		/// </summary>
		public readonly string header = "<!DOCTYPE html>" +
			"<html><head> <meta charset=\"UTF-8\"> " +
			"<style>" +
			"* {{" +
			"font-family: 'AvantGarde Md BT', Arial;" +
			"font-weight: bold;" +
			"}}" +
			"</style>" + 
			"</head><body>" +
			"<svg height='{0}' width='{1}'>";
		/// <summary>
		/// The line segment. <>
		/// 0 = X1 
		/// 1 = Y1
		/// 2 = X2 
		/// 3 = Y2
		/// 4 = R
		/// 5 = G 
		/// 6 = B 
		/// </summary>
		private readonly string lineSegment = "<line x1='{0}' y1='{1}' x2='{2}' y2='{3}' style='stroke:rgb({4},{5},{6});stroke-width:30' stroke-linecap='round'/>";
		/// <summary>
		/// The integration.<>
		/// 0 = X 
		/// 1 = Y 
		/// 2 = Ang (º) 
		/// 3 = Offset 
		/// </summary>
		private readonly string integration = "<g transform=\" translate({0},{1}) rotate({2}, {0},{1})\">" +
			"<line x1=\"0\" y1=\"0\" x2=\"{3}\" y2=\"0\" style=\"stroke:rgb(155,155,155);stroke-width:30\" stroke-linecap=\"round\"/>" +
			"<circle cx=\"0\" cy=\"0\" r=\"12\" fill=\"white\" />" +
			"<circle cx=\"{3}\" cy=\"0\" r=\"12\" fill=\"white\" />" +
			"</g>";
		/// <summary>
		/// The station.<>
		/// 0 = X 
		/// 1 = Y 
		/// 2 = Ang (º) 
		/// 3 = Station Name 
		/// </summary>
		private readonly string station = "<g transform=\"rotate({2},{0},{1}) translate({0},{1})\">" +
			"<circle cx=\"0\" cy=\"0\" r=\"12\" fill=\"white\" style=\"stroke:rgb(155,155,155);stroke-width:1\"/>" +
				"<text x=\"15\" y=\"5\" fill=\"black\" style=\"stroke:rgb(255,255,255);stroke-width:1\">{3}</text>" +
			"</g>";
		
		/// <summary>
		/// The station.<>
		/// 0 = X 
		/// 1 = Y 
		/// 2 = Ang (º) 
		/// 3 = Station Name 
		/// </summary>
		private readonly string stationReversed = "<g transform=\"rotate({2},{0},{1}) translate({0},{1})\">" +
			"<circle cx=\"0\" cy=\"0\" r=\"12\" fill=\"white\" style=\"stroke:rgb(155,155,155);stroke-width:1\"/>" +
				"<text x=\"15\" y=\"5\" fill=\"black\" transform=\"rotate(180,15,0)\"text-anchor=\"end \" style=\"stroke:rgb(255,255,255);stroke-width:1\">{3}</text>" +
			"</g>";

		/// <summary>
		/// The metro line symbol.<>
		/// 0 = X 
		/// 1 = Y 
		/// 2 = Line Name 
		/// 3 = R
		/// 4 = G
		/// 5 = B
		/// </summary>
		private readonly string metroLineSymbol = "<g transform=\"translate({0},{1})\">" +
			"  <rect x=\"-30\" y=\"-30\" width=\"60\" height=\"60\" fill=\"rgb({3},{4},{5})\" stroke=\"black\" stroke-width=\"1\" />" +
			"<text x=\"0\" y=\"10\" fill=\"white\"  stroke=\"black\" stroke-width=\"0.5\" style=\"font-size:30px\"   text-anchor=\"middle\">{2}</text>" +
			"</g>";
		/// <summary>
		/// The train line symbol.<>
		/// 0 = X 
		/// 1 = Y 
		/// 2 = Line Name 
		/// 3 = R
		/// 4 = G
		/// 5 = B
		/// </summary>
		private readonly string trainLineSymbol = "<g transform=\"translate({0},{1})\">" +
			"<circle cx=\"0\" cy=\"0\"r=\"30\" fill=\"rgb({3},{4},{5})\" stroke=\"black\" stroke-width=\"1\" />" +
			"<text x=\"0\" y=\"10\" fill=\"white\"  stroke=\"black\" stroke-width=\"0.5\" style=\"font-size:30px\"   text-anchor=\"middle\">{2}</text>" +
			"</g>";
		/// <summary>
		/// The footer.
		/// </summary>
		public readonly string footer = "</svg></body></html>";
		private StringBuilder document;
		private float multiplier;
		private int height;

		private Vector2 offset;

		public SVGTemplate (int width, int height, float multiplier=1, float offsetX=0, float offsetY =0)
		{
			document = new StringBuilder (String.Format (header, width, height));
			this.multiplier = multiplier;
			this.height = height;
			this.offset = new Vector2(offsetX, offsetY);
		}

		public string getResult ()
		{
			document.Append (footer);
			return document.ToString ();
		}

		public void addStation (Vector2 point, float angle, string name)
		{
			switch (CardinalPoint.getCardinalPoint (angle).Value) {
			case CardinalPoint.CardinalInternal.NW:
			case CardinalPoint.CardinalInternal.W:
			case CardinalPoint.CardinalInternal.SW:
				addStationReversed (point, angle, name);
				return;
			}
			point -= offset;
			document.AppendFormat (station, point.x * multiplier,  (point.y * multiplier), angle, (name));
		}

		public void addStationReversed (Vector2 point, float angle, string name)
		{
			switch (CardinalPoint.getCardinalPoint (angle).Value) {
			case CardinalPoint.CardinalInternal.NE:
			case CardinalPoint.CardinalInternal.E:
			case CardinalPoint.CardinalInternal.SE:
				addStation (point, angle, name);
				return;
			}
			point -= offset;
			document.AppendFormat (stationReversed, point.x * multiplier,  (point.y * multiplier), angle, (name));
		}

		public void addLineSegment (Vector2 p1, Vector2 p2, Color32 color)
		{
			p1 -= offset;
			p2 -= offset;
			if (color.r > 240 && color.g > 240 && color.b > 240) {
				color = new Color32(240,240,240,255);
			}
			document.AppendFormat (lineSegment, p1.x * multiplier,  (p1.y * multiplier), p2.x * multiplier,  (p2.y * multiplier), color.r, color.g, color.b);
		}

		public void addMetroLineIndication (Vector2 point, string name, Color32 color)
		{
			point -= offset;
			document.AppendFormat (metroLineSymbol, point.x * multiplier, (point.y * multiplier), name, color.r, color.g, color.b);
		}

		public void addTrainLineSegment (Vector2 point, string name, Color32 color)
		{
			point -= offset;
			document.AppendFormat (trainLineSymbol, point.x * multiplier,  (point.y * multiplier), name, color.r, color.g, color.b);
		}
	}

}

