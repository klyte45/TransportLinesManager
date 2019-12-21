using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.IO;
using ColossalFramework.Math;
using ColossalFramework.Packaging;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using Klyte.TransportLinesManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace Klyte.Commons.Utils
{
    public class KlyteUtils
    {

        #region Math Utils
        public static float calcBezierLenght(Bezier3 bz, float precision = 0.5f)
        {
            return calcBezierLenght(bz.a, bz.b, bz.c, bz.d, precision);
        }
        public static float calcBezierLenght(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float precision)
        {

            Vector3 aa = (-a + 3 * (b - c) + d);
            Vector3 bb = 3 * (a + c) - 6 * b;
            Vector3 cc = 3 * (b - a);

            int len = (int)(1.0f / precision);
            float[] arcLengths = new float[len + 1];
            arcLengths[0] = 0;

            Vector3 ov = a;
            Vector3 v;
            float clen = 0.0f;
            for (int i = 1; i <= len; i++)
            {
                float t = (i * precision);
                v = ((aa * t + (bb)) * t + cc) * t + a;
                clen += (ov - v).magnitude;
                arcLengths[i] = clen;
                ov = v;
            }
            return clen;

        }

        public static Vector2 RadianToVector2(float radian)
        {
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        }

        public static Vector2 DegreeToVector2(float degree)
        {
            return RadianToVector2(degree * Mathf.Deg2Rad);
        }
        #endregion

        #region UI utils
        public static T createElement<T>(Transform parent, string name = null) where T : MonoBehaviour
        {
            createElement<T>(out T uiItem, parent, name);
            return uiItem;
        }
        public static void createElement<T>(out T uiItem, Transform parent, string name = null) where T : MonoBehaviour
        {
            GameObject container = new GameObject();
            container.transform.parent = parent;
            uiItem = (T)container.AddComponent(typeof(T));
            if (name != null)
            {
                container.name = name;
            }
        }
        public static GameObject createElement(Type type, Transform parent)
        {
            GameObject container = new GameObject();
            container.transform.parent = parent;
            container.AddComponent(type);
            return container;
        }
        public static void createUIElement<T>(out T uiItem, Transform parent, String name = null, Vector4 area = default(Vector4)) where T : UIComponent
        {
            GameObject container = new GameObject();
            container.transform.parent = parent;
            uiItem = container.AddComponent<T>();
            if (name != null)
            {
                uiItem.name = name;
            }
            if (area != default(Vector4))
            {
                uiItem.autoSize = false;
                uiItem.area = area;
            }
        }
        public static void uiTextFieldDefaults(UITextField uiItem)
        {
            uiItem.selectionSprite = "EmptySprite";
            uiItem.useOutline = true;
            uiItem.hoveredBgSprite = "TextFieldPanelHovered";
            uiItem.focusedBgSprite = "TextFieldPanel";
            uiItem.builtinKeyNavigation = true;
            uiItem.submitOnFocusLost = true;
        }
        public static Color contrastColor(Color color)
        {
            int d = 0;

            // Counting the perceptive luminance - human eye favors green color... 
            double a = (0.299 * color.r + 0.587 * color.g + 0.114 * color.b);

            if (a > 0.5)
                d = 0; // bright colors - black font
            else
                d = 1; // dark colors - white font

            return new Color(d, d, d, 1);
        }
        public static UIDragHandle createDragHandle(UIComponent parent, UIComponent target)
        {
            return createDragHandle(parent, target, -1);
        }
        public static UIDragHandle createDragHandle(UIComponent parent, UIComponent target, float height)
        {
            createUIElement(out UIDragHandle dh, parent.transform);
            dh.target = target;
            dh.relativePosition = new Vector3(0, 0);
            dh.width = parent.width;
            dh.height = height < 0 ? parent.height : height;
            dh.name = "DragHandle";
            dh.Start();
            return dh;
        }
        public static void initButton(UIButton button, bool isCheck, string baseSprite, bool allLower = false)
        {
            string sprite = baseSprite;//"ButtonMenu";
            string spriteHov = baseSprite + "Hovered";
            button.normalBgSprite = sprite;
            button.disabledBgSprite = sprite + "Disabled";
            button.hoveredBgSprite = spriteHov;
            button.focusedBgSprite = spriteHov;
            button.pressedBgSprite = isCheck ? sprite + "Pressed" : spriteHov;

            if (allLower)
            {
                button.normalBgSprite = button.normalBgSprite.ToLower();
                button.disabledBgSprite = button.disabledBgSprite.ToLower();
                button.hoveredBgSprite = button.hoveredBgSprite.ToLower();
                button.focusedBgSprite = button.focusedBgSprite.ToLower();
                button.pressedBgSprite = button.pressedBgSprite.ToLower();
            }

            button.textColor = new Color32(255, 255, 255, 255);
        }
        public static void initButtonSameSprite(UIButton button, string baseSprite)
        {
            string sprite = baseSprite;//"ButtonMenu";
            button.normalBgSprite = sprite;
            button.disabledBgSprite = sprite;
            button.hoveredBgSprite = sprite;
            button.focusedBgSprite = sprite;
            button.pressedBgSprite = sprite;
            button.textColor = new Color32(255, 255, 255, 255);
        }
        public static void initButtonFg(UIButton button, bool isCheck, string baseSprite)
        {
            string sprite = baseSprite;//"ButtonMenu";
            string spriteHov = baseSprite + "Hovered";
            button.normalFgSprite = sprite;
            button.disabledFgSprite = sprite;
            button.hoveredFgSprite = spriteHov;
            button.focusedFgSprite = spriteHov;
            button.pressedFgSprite = isCheck ? sprite + "Pressed" : spriteHov;
            button.textColor = new Color32(255, 255, 255, 255);
        }
        public static void copySpritesEvents(UIButton source, UIButton target)
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


        public UISlider GenerateSliderField(UIHelperExtension uiHelper, OnValueChanged action, out UILabel label, out UIPanel container)
        {
            UISlider budgetMultiplier = (UISlider)uiHelper.AddSlider("", 0f, 5, 0.05f, 1, action);
            label = budgetMultiplier.transform.parent.GetComponentInChildren<UILabel>();
            label.autoSize = true;
            label.wordWrap = false;
            label.text = string.Format(" x{0:0.00}", 0);
            container = budgetMultiplier.GetComponentInParent<UIPanel>();
            container.width = 300;
            container.autoLayoutDirection = LayoutDirection.Horizontal;
            container.autoLayoutPadding = new RectOffset(5, 5, 3, 3);
            container.wrapLayout = true;
            return budgetMultiplier;
        }
        public static void clearAllVisibilityEvents(UIComponent u)
        {
            u.eventVisibilityChanged += null;
            for (int i = 0; i < u.components.Count; i++)
            {
                clearAllVisibilityEvents(u.components[i]);
            }
        }
        public static PropertyChangedEventHandler<Vector2> LimitWidth(UIComponent x)
        {
            return LimitWidth(x, x.minimumSize.x);
        }
        public static PropertyChangedEventHandler<Vector2> LimitWidth(UIComponent x, float maxWidth, bool alsoMinSize = false)
        {
            x.autoSize = true;
            void callback(UIComponent y, Vector2 z)
            {
                x.transform.localScale = new Vector3(Math.Min(1, maxWidth / x.width), x.transform.localScale.y, x.transform.localScale.z);
            }
            x.eventSizeChanged += callback;
            if (alsoMinSize)
            {
                x.minimumSize = new Vector2(maxWidth, x.minimumSize.y);
            }
            return callback;
        }
        public static UIHelperExtension CreateScrollPanel(UIComponent parent, out UIScrollablePanel scrollablePanel, out UIScrollbar scrollbar, float width, float height, Vector3 relativePosition)
        {
            createUIElement(out scrollablePanel, parent?.transform);
            scrollablePanel.width = width;
            scrollablePanel.height = height;
            scrollablePanel.autoLayoutDirection = LayoutDirection.Vertical;
            scrollablePanel.autoLayoutStart = LayoutStart.TopLeft;
            scrollablePanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            scrollablePanel.autoLayout = true;
            scrollablePanel.clipChildren = true;
            scrollablePanel.relativePosition = relativePosition;

            createUIElement(out UIPanel trackballPanel, parent?.transform);
            trackballPanel.width = 10f;
            trackballPanel.height = scrollablePanel.height;
            trackballPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            trackballPanel.autoLayoutStart = LayoutStart.TopLeft;
            trackballPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            trackballPanel.autoLayout = true;
            trackballPanel.relativePosition = new Vector3(relativePosition.x + width + 5, relativePosition.y);


            createUIElement(out scrollbar, trackballPanel.transform);
            scrollbar.width = 10f;
            scrollbar.height = scrollbar.parent.height;
            scrollbar.orientation = UIOrientation.Vertical;
            scrollbar.pivot = UIPivotPoint.BottomLeft;
            scrollbar.AlignTo(trackballPanel, UIAlignAnchor.TopRight);
            scrollbar.minValue = 0f;
            scrollbar.value = 0f;
            scrollbar.incrementAmount = 25f;

            createUIElement(out UISlicedSprite scrollBg, scrollbar.transform);
            scrollBg.relativePosition = Vector2.zero;
            scrollBg.autoSize = true;
            scrollBg.size = scrollBg.parent.size;
            scrollBg.fillDirection = UIFillDirection.Vertical;
            scrollBg.spriteName = "ScrollbarTrack";
            scrollbar.trackObject = scrollBg;

            createUIElement(out UISlicedSprite scrollFg, scrollBg.transform);
            scrollFg.relativePosition = Vector2.zero;
            scrollFg.fillDirection = UIFillDirection.Vertical;
            scrollFg.autoSize = true;
            scrollFg.width = scrollFg.parent.width - 4f;
            scrollFg.spriteName = "ScrollbarThumb";
            scrollbar.thumbObject = scrollFg;
            scrollablePanel.verticalScrollbar = scrollbar;
            scrollablePanel.eventMouseWheel += delegate (UIComponent component, UIMouseEventParameter param)
            {
                ((UIScrollablePanel)component).scrollPosition += new Vector2(0f, Mathf.Sign(param.wheelDelta) * -1f * ((UIScrollablePanel)component).verticalScrollbar.incrementAmount);
            };

            return new UIHelperExtension(scrollablePanel);
        }

        public static UIHelperExtension CreateHorizontalScrollPanel(UIComponent parent, out UIScrollablePanel scrollablePanel, out UIScrollbar scrollbar, float width, float height, Vector3 relativePosition)
        {
            createUIElement(out scrollablePanel, parent?.transform);
            scrollablePanel.width = width;
            scrollablePanel.height = height;
            scrollablePanel.autoLayoutDirection = LayoutDirection.Horizontal;
            scrollablePanel.wrapLayout = false;
            scrollablePanel.autoLayoutStart = LayoutStart.TopLeft;
            scrollablePanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            scrollablePanel.autoLayout = true;
            scrollablePanel.clipChildren = true;
            scrollablePanel.relativePosition = relativePosition;

            createUIElement(out UIPanel trackballPanel, parent?.transform);
            trackballPanel.height = 10f;
            trackballPanel.width = scrollablePanel.width;
            trackballPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            trackballPanel.autoLayoutStart = LayoutStart.TopLeft;
            trackballPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            trackballPanel.autoLayout = true;
            trackballPanel.relativePosition = new Vector3(relativePosition.x, relativePosition.y + height + 5);


            createUIElement(out scrollbar, trackballPanel.transform);
            scrollbar.height = 10f;
            scrollbar.width = scrollbar.parent.width;
            scrollbar.orientation = UIOrientation.Horizontal;
            scrollbar.pivot = UIPivotPoint.BottomLeft;
            scrollbar.AlignTo(trackballPanel, UIAlignAnchor.TopLeft);
            scrollbar.minValue = 0f;
            scrollbar.value = 0f;
            scrollbar.incrementAmount = 25f;

            createUIElement(out UISlicedSprite scrollBg, scrollbar.transform);
            scrollBg.relativePosition = Vector2.zero;
            scrollBg.autoSize = true;
            scrollBg.size = scrollBg.parent.size;
            scrollBg.fillDirection = UIFillDirection.Horizontal;
            scrollBg.spriteName = "ScrollbarTrack";
            scrollbar.trackObject = scrollBg;

            createUIElement(out UISlicedSprite scrollFg, scrollBg.transform);
            scrollFg.relativePosition = Vector2.zero;
            scrollFg.fillDirection = UIFillDirection.Horizontal;
            scrollFg.autoSize = true;
            scrollFg.width = scrollFg.parent.width - 4f;
            scrollFg.spriteName = "ScrollbarThumb";
            scrollbar.thumbObject = scrollFg;
            scrollablePanel.horizontalScrollbar = scrollbar;
            scrollablePanel.eventMouseWheel += delegate (UIComponent component, UIMouseEventParameter param)
            {
                ((UIScrollablePanel)component).scrollPosition += new Vector2(Mathf.Sign(param.wheelDelta) * -1f * ((UIScrollablePanel)component).horizontalScrollbar.incrementAmount, 0);
            };

            return new UIHelperExtension(scrollablePanel);
        }
        #endregion

        #region Numbering Utils
        public static string ToRomanNumeral(ushort value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException("Please use a positive integer greater than zero.");

            StringBuilder sb = new StringBuilder();
            if (value >= 4000)
            {
                RomanizeCore(sb, (ushort)(value / 1000));
                sb.Append("·");
                value %= 1000;
            }
            RomanizeCore(sb, value);

            return sb.ToString();
        }
        private static ushort RomanizeCore(StringBuilder sb, ushort remain)
        {
            while (remain > 0)
            {
                if (remain >= 1000)
                {
                    sb.Append("Ⅿ");
                    remain -= 1000;
                }
                else if (remain >= 900)
                {
                    sb.Append("ⅭⅯ");
                    remain -= 900;
                }
                else if (remain >= 500)
                {
                    sb.Append("Ⅾ");
                    remain -= 500;
                }
                else if (remain >= 400)
                {
                    sb.Append("ⅭⅮ");
                    remain -= 400;
                }
                else if (remain >= 100)
                {
                    sb.Append("Ⅽ");
                    remain -= 100;
                }
                else if (remain >= 90)
                {
                    sb.Append("ⅩⅭ");
                    remain -= 90;
                }
                else if (remain >= 50)
                {
                    sb.Append("Ⅼ");
                    remain -= 50;
                }
                else if (remain >= 40)
                {
                    sb.Append("ⅩⅬ");
                    remain -= 40;
                }
                else if (remain >= 13)
                {
                    sb.Append("Ⅹ");
                    remain -= 10;
                }
                else
                {
                    switch (remain)
                    {
                        case 12:
                            sb.Append("Ⅻ");
                            break;
                        case 11:
                            sb.Append("Ⅺ");
                            break;
                        case 10:
                            sb.Append("Ⅹ");
                            break;
                        case 9:
                            sb.Append("Ⅸ");
                            break;
                        case 8:
                            sb.Append("Ⅷ");
                            break;
                        case 7:
                            sb.Append("Ⅶ");
                            break;
                        case 6:
                            sb.Append("Ⅵ");
                            break;
                        case 5:
                            sb.Append("Ⅴ");
                            break;
                        case 4:
                            sb.Append("Ⅳ");
                            break;
                        case 3:
                            sb.Append("Ⅲ");
                            break;
                        case 2:
                            sb.Append("Ⅱ");
                            break;
                        case 1:
                            sb.Append("Ⅰ");
                            break;
                    }
                    remain = 0;
                }
            }

            return remain;
        }
        public static string getStringFromNumber(string[] array, int number)
        {
            int arraySize = array.Length;
            string saida = "";
            while (number > 0)
            {
                int idx = (number - 1) % arraySize;
                saida = "" + array[idx] + saida;
                if (number % arraySize == 0)
                {
                    number /= arraySize;
                    number--;
                }
                else
                {
                    number /= arraySize;
                }

            }
            return saida;
        }
        #endregion

        #region Building Utils
        public static IEnumerator setBuildingName(ushort buildingID, string name, OnEndProcessingBuildingName function)
        {
            InstanceID buildingIdSelect = default(InstanceID);
            buildingIdSelect.Building = buildingID;
            yield return Singleton<SimulationManager>.instance.AddAction<bool>(Singleton<BuildingManager>.instance.SetBuildingName(buildingID, name));
            function();
        }

        public delegate void OnEndProcessingBuildingName();
        public static ushort FindBuilding(Vector3 pos, float maxDistance, ItemClass.Service service, ItemClass.SubService subService, TransferManager.TransferReason[] allowedTypes, Building.Flags flagsRequired, Building.Flags flagsForbidden)
        {
            BuildingManager bm = Singleton<BuildingManager>.instance;
            //if (allowedTypes == null || allowedTypes.Length == 0)
            //{
            //    return bm.FindBuilding(pos, maxDistance, service, subService, flagsRequired, flagsForbidden);
            //}


            int num = Mathf.Max((int)((pos.x - maxDistance) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((pos.z - maxDistance) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((pos.x + maxDistance) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((pos.z + maxDistance) / 64f + 135f), 269);
            ushort result = 0;
            float currentDistance = maxDistance * maxDistance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort buildingId = bm.m_buildingGrid[i * 270 + j];
                    int num7 = 0;
                    while (buildingId != 0)
                    {
                        BuildingInfo info = bm.m_buildings.m_buffer[buildingId].Info;
                        if (!CheckInfoCompatibility(pos, service, subService, allowedTypes, flagsRequired, flagsForbidden, bm, ref result, ref currentDistance, buildingId, info) && info.m_subBuildings?.Length > 0)
                        {
                            foreach (var subBuilding in info.m_subBuildings)
                            {
                                if (subBuilding != null && CheckInfoCompatibility(pos, service, subService, allowedTypes, flagsRequired, flagsForbidden, bm, ref result, ref currentDistance, buildingId, subBuilding.m_buildingInfo))
                                {
                                    break;
                                }
                            }
                        }
                        buildingId = bm.m_buildings.m_buffer[(int)buildingId].m_nextGridBuilding;
                        if (++num7 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return result;
        }

        private static bool CheckInfoCompatibility(Vector3 pos, ItemClass.Service service, ItemClass.SubService subService, TransferManager.TransferReason[] allowedTypes, Building.Flags flagsRequired, Building.Flags flagsForbidden, BuildingManager bm, ref ushort result, ref float lastNearest, ushort buildingId, BuildingInfo info)
        {
            //doErrorLog($"CheckInfoCompatibility 0  {pos}, {service}, {subService}, {allowedTypes},  {flagsRequired},  {flagsForbidden}, {bm},  {result},  {lastNearest},  {buildingId}, {info}");
            if (info != null && (info.m_class.m_service == service || service == ItemClass.Service.None) && (info.m_class.m_subService == subService || subService == ItemClass.SubService.None))
            {
                //doErrorLog("CheckInfoCompatibility 1");
                Building.Flags flags = bm.m_buildings.m_buffer[(int)buildingId].m_flags;
                //doErrorLog("CheckInfoCompatibility 2");
                if ((flags & (flagsRequired | flagsForbidden)) == flagsRequired)
                {
                    //doErrorLog("CheckInfoCompatibility 3");
                    if (allowedTypes == null
                        || allowedTypes.Length == 0
                        || !(info.GetAI() is DepotAI depotAI)
                        || (depotAI.m_transportInfo != null && allowedTypes.Contains(depotAI.m_transportInfo.m_vehicleReason))
                        || (depotAI.m_secondaryTransportInfo != null && allowedTypes.Contains(depotAI.m_secondaryTransportInfo.m_vehicleReason)))
                    {
                        //doErrorLog("CheckInfoCompatibility 4");
                        float dist = Vector3.SqrMagnitude(pos - bm.m_buildings.m_buffer[(int)buildingId].m_position);
                        //doErrorLog("CheckInfoCompatibility 5");
                        if (dist < lastNearest)
                        {
                            result = buildingId;
                            lastNearest = dist;
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        public static ushort GetBuildingDistrict(uint bId)
        {
            return GetBuildingDistrict(Singleton<BuildingManager>.instance.m_buildings.m_buffer[bId]);
        }
        public static ushort GetBuildingDistrict(Building b)
        {
            return DistrictManager.instance.GetDistrict(b.m_position);
        }
        public static ushort GetPark(Vector3 location)
        {
            return Singleton<DistrictManager>.instance.GetPark(location);
        }



        public static StopPointDescriptor[] GetAllSpawnPoints(BuildingAI buidlingAI)
        {
            if (!(buidlingAI is DepotAI depotAI))
            {
                return null;
            }
            List<StopPointDescriptor> stops = new List<StopPointDescriptor>();
            if (depotAI.m_spawnPoints != null && depotAI.m_spawnPoints.Length != 0)
            {
                for (int i = 0; i < depotAI.m_spawnPoints.Length; i++)
                {
                    AddSpawnPoint(depotAI.m_transportInfo, stops, depotAI.m_spawnPoints[i].m_position, depotAI.m_spawnPoints[i].m_target, depotAI.m_canInvertTarget);
                }
            }
            else
            {
                AddSpawnPoint(depotAI.m_transportInfo, stops, depotAI.m_spawnPosition, depotAI.m_spawnTarget, depotAI.m_canInvertTarget);
            }
            if (depotAI.m_secondaryTransportInfo != null)
            {
                if (depotAI.m_spawnPoints2 != null && depotAI.m_spawnPoints2.Length != 0)
                {
                    for (int i = 0; i < depotAI.m_spawnPoints2.Length; i++)
                    {
                        AddSpawnPoint(depotAI.m_secondaryTransportInfo, stops, depotAI.m_spawnPoints2[i].m_position, depotAI.m_spawnPoints2[i].m_target, depotAI.m_canInvertTarget);
                    }
                }
                else
                {
                    AddSpawnPoint(depotAI.m_secondaryTransportInfo, stops, depotAI.m_spawnPosition2, depotAI.m_spawnTarget2, depotAI.m_canInvertTarget);
                }
            }
            foreach (var subBuilding in depotAI.m_info.m_subBuildings)
            {
                var subPlats = GetAllSpawnPoints(subBuilding.m_buildingInfo.m_buildingAI);
                if (subPlats != null)
                {
                    stops.AddRange(subPlats.Select(x =>
                    {
                        x.relativePosition -= subBuilding.m_position;
                        return x;
                    }));
                }
            }
            stops.Sort((x, y) =>
            {
                if (x.relativePosition.x != y.relativePosition.x) return x.relativePosition.x.CompareTo(y.relativePosition.x);
                if (x.relativePosition.z != y.relativePosition.z) return x.relativePosition.z.CompareTo(y.relativePosition.z);
                return x.relativePosition.y.CompareTo(y.relativePosition.y);
            });
            return stops.ToArray();
        }

        private static void AddSpawnPoint(TransportInfo info, List<StopPointDescriptor> stops, Vector3 position, Vector3 target, bool canInvert)
        {
            stops.Add(new StopPointDescriptor
            {
                relativePosition = target,
                vehicleType = info.m_vehicleType
            });
            if (canInvert)
            {
                stops.Add(new StopPointDescriptor
                {
                    relativePosition = position * 2 - target,
                    vehicleType = info.m_vehicleType
                });
            }
        }

        public class StopPointDescriptor
        {
            public Vector3 relativePosition;
            public VehicleInfo.VehicleType vehicleType;
        }
        #endregion

        #region Road Search Utils
        public static ushort FindNearNamedRoad(Vector3 position)
        {
            return FindNearNamedRoad(position, ItemClass.Service.Road, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle,
                    VehicleInfo.VehicleType.Car, false, false, 256f,
                    out PathUnit.Position pathPosA, out PathUnit.Position pathPosB, out float distanceSqrA, out float distanceSqrB);
        }
        public static ushort FindNearNamedRoad(Vector3 position, ItemClass.Service service, NetInfo.LaneType laneType, VehicleInfo.VehicleType vehicleType,
            bool allowUnderground, bool requireConnect, float maxDistance,
            out PathUnit.Position pathPosA, out PathUnit.Position pathPosB, out float distanceSqrA, out float distanceSqrB)
        {
            return FindNearNamedRoad(position, service, service, laneType,
                vehicleType, VehicleInfo.VehicleType.None, allowUnderground,
                requireConnect, maxDistance,
                out pathPosA, out pathPosB, out distanceSqrA, out distanceSqrB);
        }
        public static ushort FindNearNamedRoad(Vector3 position, ItemClass.Service service, ItemClass.Service service2, NetInfo.LaneType laneType,
            VehicleInfo.VehicleType vehicleType, VehicleInfo.VehicleType stopType, bool allowUnderground,
            bool requireConnect, float maxDistance,
            out PathUnit.Position pathPosA, out PathUnit.Position pathPosB, out float distanceSqrA, out float distanceSqrB)
        {


            Bounds bounds = new Bounds(position, new Vector3(maxDistance * 2f, maxDistance * 2f, maxDistance * 2f));
            int num = Mathf.Max((int)((bounds.min.x - 64f) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((bounds.min.z - 64f) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((bounds.max.x + 64f) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((bounds.max.z + 64f) / 64f + 135f), 269);
            NetManager instance = Singleton<NetManager>.instance;
            pathPosA.m_segment = 0;
            pathPosA.m_lane = 0;
            pathPosA.m_offset = 0;
            distanceSqrA = 1E+10f;
            pathPosB.m_segment = 0;
            pathPosB.m_lane = 0;
            pathPosB.m_offset = 0;
            distanceSqrB = 1E+10f;
            float num5 = maxDistance * maxDistance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num6 = instance.m_segmentGrid[i * 270 + j];
                    int num7 = 0;
                    while (num6 != 0)
                    {
                        NetInfo info = instance.m_segments.m_buffer[(int)num6].Info;
                        if (info != null && (info.m_class.m_service == service || info.m_class.m_service == service2) && (instance.m_segments.m_buffer[(int)num6].m_flags & (NetSegment.Flags.Collapsed | NetSegment.Flags.Flooded)) == NetSegment.Flags.None && (allowUnderground || !info.m_netAI.IsUnderground()))
                        {
                            ushort startNode = instance.m_segments.m_buffer[(int)num6].m_startNode;
                            ushort endNode = instance.m_segments.m_buffer[(int)num6].m_endNode;
                            Vector3 position2 = instance.m_nodes.m_buffer[(int)startNode].m_position;
                            Vector3 position3 = instance.m_nodes.m_buffer[(int)endNode].m_position;
                            float num8 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position2.x, bounds.min.z - 64f - position2.z), Mathf.Max(position2.x - bounds.max.x - 64f, position2.z - bounds.max.z - 64f));
                            float num9 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position3.x, bounds.min.z - 64f - position3.z), Mathf.Max(position3.x - bounds.max.x - 64f, position3.z - bounds.max.z - 64f));
                            if ((num8 < 0f || num9 < 0f) && instance.m_segments.m_buffer[(int)num6].m_bounds.Intersects(bounds) && instance.m_segments.m_buffer[(int)num6].GetClosestLanePosition(position, laneType, vehicleType, stopType, requireConnect, out Vector3 b, out int num10, out float num11, out Vector3 b2, out int num12, out float num13))
                            {
                                float num14 = Vector3.SqrMagnitude(position - b);
                                if (num14 < num5)
                                {
                                    num5 = num14;
                                    pathPosA.m_segment = num6;
                                    pathPosA.m_lane = (byte)num10;
                                    pathPosA.m_offset = (byte)Mathf.Clamp(Mathf.RoundToInt(num11 * 255f), 0, 255);
                                    distanceSqrA = num14;
                                    num14 = Vector3.SqrMagnitude(position - b2);
                                    if (num12 == -1 || num14 >= maxDistance * maxDistance)
                                    {
                                        pathPosB.m_segment = 0;
                                        pathPosB.m_lane = 0;
                                        pathPosB.m_offset = 0;
                                        distanceSqrB = 1E+10f;
                                    }
                                    else
                                    {
                                        pathPosB.m_segment = num6;
                                        pathPosB.m_lane = (byte)num12;
                                        pathPosB.m_offset = (byte)Mathf.Clamp(Mathf.RoundToInt(num13 * 255f), 0, 255);
                                        distanceSqrB = num14;
                                    }
                                }
                            }
                        }
                        num6 = instance.m_segments.m_buffer[(int)num6].m_nextGridSegment;
                        if (++num7 >= 36864)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return pathPosA.m_segment;
        }
        #endregion

        #region Stop Search Utils
        public static List<ushort> FindNearStops(Vector3 position)
        {
            return FindNearStops(position, ItemClass.Service.PublicTransport, true, 24f, out List<float> distanceSqrA, out List<Vector3> stopPositions);
        }
        public static List<ushort> FindNearStops(Vector3 position, ItemClass.Service service, bool allowUnderground, float maxDistance, out List<float> distanceSqrA, out List<Vector3> stopPositions, List<Quad2> boundaries = null)
        {
            return FindNearStops(position, service, service, VehicleInfo.VehicleType.None, allowUnderground, maxDistance, out distanceSqrA, out stopPositions, boundaries);
        }
        public static List<ushort> FindNearStops(Vector3 position, ItemClass.Service service, ItemClass.Service service2, VehicleInfo.VehicleType stopType, bool allowUnderground, float maxDistance,
             out List<float> distanceSqrA, out List<Vector3> stopPositions, List<Quad2> boundaries = null)
        {


            Bounds bounds = new Bounds(position, new Vector3(maxDistance * 2f, maxDistance * 2f, maxDistance * 2f));
            int num = Mathf.Max((int)((bounds.min.x - 64f) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((bounds.min.z - 64f) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((bounds.max.x + 64f) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((bounds.max.z + 64f) / 64f + 135f), 269);
            NetManager instance = Singleton<NetManager>.instance;
            List<Tuple<ushort, float, Vector3>> result = new List<Tuple<ushort, float, Vector3>>();

            float maxDistSqr = maxDistance * maxDistance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    var idx = i * 270 + j;
                    ushort nodeId = 0;
                    int num7 = 0;
                    try
                    {
                        nodeId = instance.m_nodeGrid[idx];
                        num7 = 0;
                        while (nodeId != 0)
                        {
                            NetInfo info = instance.m_nodes.m_buffer[(int)nodeId].Info;
                            if (info != null
                                && (info.m_class.m_service == service || info.m_class.m_service == service2)
                                && (instance.m_nodes.m_buffer[(int)nodeId].m_flags & (NetNode.Flags.Collapsed)) == NetNode.Flags.None
                                && (instance.m_nodes.m_buffer[(int)nodeId].m_flags & (NetNode.Flags.Created)) != NetNode.Flags.None
                                && instance.m_nodes.m_buffer[(int)nodeId].m_transportLine > 0
                                && (allowUnderground || !info.m_netAI.IsUnderground())
                                && (stopType == VehicleInfo.VehicleType.None || stopType == TransportManager.instance.m_lines.m_buffer[instance.m_nodes.m_buffer[nodeId].m_transportLine].Info.m_vehicleType))
                            {
                                var node = instance.m_nodes.m_buffer[(int)nodeId];
                                Vector3 nodePos = node.m_position;
                                if (boundaries != null && boundaries.Count != 0 && !boundaries.Any(x => x.Intersect(VectorUtils.XZ(nodePos))))
                                {
                                    goto GOTO_NEXT;
                                }
                                float delta = Mathf.Max(Mathf.Max(bounds.min.x - 64f - nodePos.x, bounds.min.z - 64f - nodePos.z), Mathf.Max(nodePos.x - bounds.max.x - 64f, nodePos.z - bounds.max.z - 64f));
                                if (delta < 0f && instance.m_nodes.m_buffer[(int)nodeId].m_bounds.Intersects(bounds))
                                {
                                    float num14 = Vector3.SqrMagnitude(position - nodePos);
                                    if (num14 < maxDistSqr)
                                    {
                                        result.Add(Tuple.New(nodeId, num14, nodePos));
                                    }
                                }
                            }
                            GOTO_NEXT:
                            nodeId = instance.m_nodes.m_buffer[(int)nodeId].m_nextGridNode;
                            if (++num7 >= 36864)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        doErrorLog($"ERROR ON TRYING FindNearStops: (It = {num7}; Init = {idx}; Curr = {nodeId})==>  {e.Message}\n{e.StackTrace}");
                    }
                }
            }
            result = result.OrderBy(x => x.First).ToList();
            distanceSqrA = result.Select(x => x.Second).ToList();
            stopPositions = result.Select(x => x.Third).ToList();
            return result.Select(x => x.First).ToList();
        }


        private const float DEFAULT_STOP_OFFSET = 0.5019608f;

        public class StopPointDescriptorLanes
        {
            public Bezier3 platformLine;
            public float width;
            public VehicleInfo.VehicleType vehicleType;
        }

        public static StopPointDescriptorLanes[] MapStopPoints(BuildingInfo buildingInfo)
        {
            var result = new List<StopPointDescriptorLanes>();
            foreach (var path in buildingInfo.m_paths)
            {
                Vector3 position = path.m_nodes[0];
                Vector3 position2 = path.m_nodes[1];
                Vector3 directionPath = Quaternion.AngleAxis(90, Vector3.up) * (position2 - position).normalized;

                doLog($"[{buildingInfo}] pos + dir = ({position} {position2} + {directionPath})");
                foreach (var lane in path.m_netInfo.m_lanes)
                {
                    if (lane.m_stopType == VehicleInfo.VehicleType.None) continue;
                    var lanePos = position + lane.m_position * directionPath;
                    var lanePos2 = position2 + lane.m_position * directionPath;
                    NetSegment.CalculateMiddlePoints(lanePos, Vector3.zero, lanePos2, Vector3.zero, true, true, out Vector3 b3, out Vector3 c);
                    var refBezier = new Bezier3(lanePos, b3, c, lanePos2);
                    doLog($"[{buildingInfo}]refBezier = {refBezier} ({lanePos} {b3} {c} {lanePos2})");


                    var positionR = refBezier.Position(DEFAULT_STOP_OFFSET);
                    var direction = refBezier.Tangent(DEFAULT_STOP_OFFSET);
                    doLog($"[{buildingInfo}]1positionR = {positionR}; direction = {direction}");

                    Vector3 normalized = Vector3.Cross(Vector3.up, direction).normalized;
                    positionR += normalized * (MathUtils.SmootherStep(0.5f, 0f, Mathf.Abs(DEFAULT_STOP_OFFSET - 0.5f)) * lane.m_stopOffset);
                    doLog($"[{buildingInfo}]2positionR = {positionR}; direction = {direction}; {normalized}");
                    result.Add(new StopPointDescriptorLanes
                    {
                        platformLine = refBezier,
                        width = lane.m_width,
                        vehicleType = lane.m_stopType
                    });

                }
            }
            foreach (var subBuilding in buildingInfo.m_subBuildings)
            {
                var subPlats = MapStopPoints(subBuilding.m_buildingInfo);
                if (subPlats != null)
                {
                    result.AddRange(subPlats.Select(x =>
                    {
                        x.platformLine.a -= subBuilding.m_position;
                        x.platformLine.b -= subBuilding.m_position;
                        x.platformLine.c -= subBuilding.m_position;
                        x.platformLine.d -= subBuilding.m_position;
                        return x;
                    }));
                }
            }
            result.Sort((x, y) =>
            {
                var priorityX = VehicleToPriority(x.vehicleType);
                var priorityY = VehicleToPriority(y.vehicleType);
                if (priorityX != priorityY) return priorityX.CompareTo(priorityY);
                var centerX = x.platformLine.GetBounds().center;
                var centerY = y.platformLine.GetBounds().center;
                if (centerX.z != centerY.z) return centerX.z.CompareTo(centerY.z);
                if (centerX.x != centerY.x) return -centerX.x.CompareTo(centerY.x);
                return centerX.y.CompareTo(centerY.y);
            });
            return result.ToArray();
        }
        public static int VehicleToPriority(VehicleInfo.VehicleType tt)
        {
            switch (tt)
            {
                case VehicleInfo.VehicleType.Car:
                    return 99;
                case VehicleInfo.VehicleType.Metro:
                case VehicleInfo.VehicleType.Train:
                case VehicleInfo.VehicleType.Monorail:
                    return 20;
                case VehicleInfo.VehicleType.Ship:
                    return 10;
                case VehicleInfo.VehicleType.Plane:
                    return 5;
                case VehicleInfo.VehicleType.Tram:
                    return 88;
                case VehicleInfo.VehicleType.Helicopter:
                    return 7;
                case VehicleInfo.VehicleType.Ferry:
                    return 15;

                case VehicleInfo.VehicleType.CableCar:
                    return 30;
                case VehicleInfo.VehicleType.Blimp:
                    return 12;
                case VehicleInfo.VehicleType.Balloon:
                    return 11;
                default: return 9999;
            }
        }
        #endregion

        #region Reflection
        #region Deprecated
        public static T GetPrivateField<T>(object o, string fieldName)
        {
            if (fieldName != null)
            {
                var field = o.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (field != null)
                {
                    return (T)field.GetValue(o);
                }
            }
            return default(T);

        }

        public static T RunPrivateMethod<T>(object o, string methodName, params object[] paramList)
        {
            if ((methodName ?? "") != string.Empty)
            {
                var method = o.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (method != null)
                {
                    return (T)method.Invoke(o, paramList);
                }
            }
            return default(T);

        }

        public static T RunPrivateStaticMethod<T>(Type t, string methodName, params object[] paramList)
        {
            if ((methodName ?? "") != string.Empty)
            {
                var method = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (method != null)
                {
                    return (T)method.Invoke(null, paramList);
                }
            }
            return default(T);

        }

        public static object[] RunPrivateStaticAction(Type t, string methodName, params object[] paramList)
        {
            if ((methodName ?? "") != string.Empty)
            {
                var method = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, paramList);
                    return paramList;
                }
            }
            return null;

        }

        public static object GetPrivateStaticField(string fieldName, Type type)
        {
            if (fieldName != null)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (field != null)
                {
                    return field.GetValue(null);
                }
            }
            return null;

        }

        public static void SetPrivateStaticField(string fieldName, Type type, object newValue)
        {
            if (fieldName != null)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (field != null)
                {
                    field.SetValue(null, newValue);
                }
            }
        }

        public static object GetPrivateStaticProperty(string fieldName, Type type)
        {
            if (fieldName != null)
            {
                PropertyInfo field = type.GetProperty(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (field != null)
                {
                    return field.GetValue(null, null);
                }
            }
            return null;

        }
        public static object ExecuteReflectionMethod(object o, string methodName, params object[] args)
        {
            var method = o.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            try
            {
                return method?.Invoke(o, args);
            }
            catch (Exception e)
            {
                KlyteUtils.doErrorLog("ERROR REFLECTING METHOD: {0} ({1}) => {2}\r\n{3}\r\n{4}", o, methodName, args, e.Message, e.StackTrace);
                return null;
            }
        }
        public static object ExecuteReflectionMethod(Type t, string methodName, params object[] args)
        {
            var method = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            try
            {
                return method?.Invoke(null, args);
            }
            catch (Exception e)
            {
                KlyteUtils.doErrorLog("ERROR REFLECTING METHOD: {0} ({1}) => {2}\r\n{3}\r\n{4}", null, methodName, args, e.Message, e.StackTrace);
                return null;
            }
        }
        #endregion
        #region Extract Properties
        public static void GetPropertyDelegates<CL, PT>(string propertyName, out Action<CL, PT> setter, out Func<CL, PT> getter)
        {
            setter = (Action<CL, PT>)Delegate.CreateDelegate(typeof(Action<CL, PT>), null, typeof(CL).GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetSetMethod());
            getter = (Func<CL, PT>)Delegate.CreateDelegate(typeof(Func<CL, PT>), null, typeof(CL).GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod());
        }
        public static void GetStaticPropertyDelegates<CL, PT>(string propertyName, out Action<PT> setter, out Func<PT> getter)
        {
            setter = (Action<PT>)Delegate.CreateDelegate(typeof(Action<PT>), null, typeof(CL).GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetSetMethod());
            getter = (Func<PT>)Delegate.CreateDelegate(typeof(Func<PT>), null, typeof(CL).GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetGetMethod());
        }

        #endregion
        #region Field Access

        /// <summary>
        /// Gets a strong typed delegate to a generated method that allows you to get the field value, that is represented
        /// by the given <paramref name="fieldInfo"/>. The delegate is instance independend, means that you pass the source 
        /// of the field as a parameter to the method and get back the value of it's field.
        /// </summary>
        /// <typeparam name="TSource">The reflecting type. This can be an interface that is implemented by the field's declaring type
        /// or an derrived type of the field's declaring type.</typeparam>
        /// <typeparam name="TValue">The type of the field value.</typeparam>
        /// <param name="fieldInfo">Provides the metadata of the field.</param>
        /// <returns>A strong typed delegeate that can be cached to get the field's value with high performance.</returns>
        public Func<TSource, TValue> GetGetFieldDelegate<TSource, TValue>(FieldInfo fieldInfo)
        {
            if (fieldInfo == null) throw new ArgumentNullException("fieldInfo");

            Type fieldDeclaringType = fieldInfo.DeclaringType;

            ParameterExpression sourceParameter =
             Expression.Parameter(typeof(TSource), "source");


            Expression sourceExpression = this.GetCastOrConvertExpression(
                sourceParameter, fieldDeclaringType);

            MemberExpression fieldExpression = Expression.Field(sourceExpression, fieldInfo);

            Expression resultExpression = this.GetCastOrConvertExpression(
            fieldExpression, typeof(TValue));

            LambdaExpression lambda =
                Expression.Lambda(typeof(Func<TSource, TValue>), resultExpression, sourceParameter);

            Func<TSource, TValue> compiled = (Func<TSource, TValue>)lambda.Compile();
            return compiled;
        }

        /// <summary>
        /// Gets a strong typed delegate to a generated method that allows you to get the field value, that is represented
        /// by the given <paramref name="fieldName"/>. The delegate is instance independend, means that you pass the source 
        /// of the field as a parameter to the method and get back the value of it's field.
        /// </summary>
        /// <typeparam name="TSource">The reflecting type. This can be an interface that is implemented by the field's declaring type
        /// or an derrived type of the field's declaring type.</typeparam>
        /// <typeparam name="TValue">The type of the field value.</typeparam>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <param name="fieldDeclaringType">The type that declares the field.</param>
        /// <returns>A strong typed delegeate that can be cached to get the field's value with high performance.</returns>
        public Func<TSource, TValue> GetGetFieldDelegate<TSource, TValue>(string fieldName, Type fieldDeclaringType)
        {
            if (fieldName == null) throw new ArgumentNullException("fieldName");
            if (fieldDeclaringType == null) throw new ArgumentNullException("fieldDeclaringType");

            FieldInfo fieldInfo = fieldDeclaringType.GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

            return this.GetGetFieldDelegate<TSource, TValue>(fieldInfo);
        }

        /// <summary>
        /// Gets a strong typed delegate to a generated method that allows you to set the field value, that is represented
        /// by the given <paramref name="fieldInfo"/>. The delegate is instance independend, means that you pass the source 
        /// of the field as a parameter to the generated method and get back the value of it's field.
        /// </summary>
        /// <typeparam name="TSource">The reflecting type. This can be an interface that is implemented by the field's declaring type
        /// or an derrived type of the field's declaring type.</typeparam>
        /// <typeparam name="TValue">The type of the field value.</typeparam>
        /// <param name="fieldInfo">Provides the metadata of the field.</param>
        /// <returns>A strong typed delegeate that can be cached to set the field's value with high performance.</returns>
        public Action<TSource, TValue> GetSetFieldDelegate<TSource, TValue>(FieldInfo fieldInfo)
        {
            if (fieldInfo == null) throw new ArgumentNullException("fieldInfo");

            Type fieldDeclaringType = fieldInfo.DeclaringType;
            //Type fieldType = fieldInfo.FieldType;
            //String fieldName = fieldInfo.Name;

            // Define the parameters of the lambda expression: (source,value) =>
            ParameterExpression sourceParameter = Expression.Parameter(typeof(TSource), "source");
            ParameterExpression valueParameter = Expression.Parameter(typeof(TValue), "value");



            // Add cast or convert expression if necessary. (e.g. when fieldDeclaringType is not assignable from typeof(TSource)
            Expression sourceExpression = this.GetCastOrConvertExpression(sourceParameter, fieldDeclaringType);

            // Get the field access expression.
            Expression fieldExpression = Expression.Field(sourceExpression, fieldInfo);

            // Add cast or convert expression if necessary.
            Expression valueExpression = this.GetCastOrConvertExpression(valueParameter, fieldExpression.Type);

            // Get the generic method that assigns the field value.
            var genericSetFieldMethodInfo = setFieldMethod.MakeGenericMethod(fieldExpression.Type);

            // get the set field expression 
            // e.g. source.SetField(ref (arg as MyClass).integerProperty, Convert(value)
            MethodCallExpression setFieldMethodCallExpression = Expression.Call(
                null, genericSetFieldMethodInfo, fieldExpression, valueExpression);

            // Create the final lambda expression
            // e.g. (source,value) => SetField(ref (arg as MyClass).integerProperty, Convert(value))
            LambdaExpression lambda = Expression.Lambda(typeof(Action<TSource, TValue>),
                                                        setFieldMethodCallExpression, sourceParameter, valueParameter);

            Action<TSource, TValue> result = (Action<TSource, TValue>)lambda.Compile();
            return result;
        }

        /// <summary>
        /// Gets a strong typed delegate to a generated method that allows you to set the field value, that is represented
        /// by the given <paramref name="fieldName"/>. The delegate is instance independend, means that you pass the source 
        /// of the field as a parameter to the generated method and get back the value of it's field.
        /// </summary>
        /// <typeparam name="TSource">The reflecting type. This can be an interface that is implemented by the field's declaring type
        /// or an derrived type of the field's declaring type.</typeparam>
        /// <typeparam name="TValue">The type of the field value.</typeparam>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="fieldType">The type of the field.</param>
        /// <param name="fieldDeclaringType">The type that declares the field.</param>
        /// <returns>A strong typed delegeate that can be cached to set the field's value with high performance.</returns>
        public Action<TSource, TValue> GetSetFieldDelegate<TSource, TValue>(string fieldName, Type fieldType, Type fieldDeclaringType)
        {
            if (fieldName == null) throw new ArgumentNullException("fieldName");
            if (fieldType == null) throw new ArgumentNullException("fieldType");
            if (fieldDeclaringType == null) throw new ArgumentNullException("fieldDeclaringType");

            // Define the parameters of the lambda expression: (source,value) =>
            ParameterExpression sourceParameter = Expression.Parameter(typeof(TSource), "source");
            ParameterExpression valueParameter = Expression.Parameter(typeof(TValue), "value");


            // Add cast or convert expression if necessary. (e.g. when fieldDeclaringType is not assignable from typeof(TSource)
            Expression sourceExpression = this.GetCastOrConvertExpression(sourceParameter, fieldDeclaringType);
            Expression valueExpression = this.GetCastOrConvertExpression(valueParameter, fieldType);

            // Get the field access expression.
            MemberExpression fieldExpression = Expression.Field(sourceExpression, fieldName);

            // Get the generic method that assigns the field value.
            var genericSetFieldMethodInfo = setFieldMethod.MakeGenericMethod(fieldType);

            // get the set field expression 
            // e.g. source.SetField(ref (arg as MyClass).integerProperty, Convert(value)
            MethodCallExpression setFieldMethodCallExpression = Expression.Call(
                null, genericSetFieldMethodInfo, fieldExpression, valueExpression);

            // Create the final lambda expression
            // e.g. (source,value) => SetField(ref (arg as MyClass).integerProperty, Convert(value))
            LambdaExpression lambda = Expression.Lambda(typeof(Action<TSource, TValue>),
                                                        setFieldMethodCallExpression, sourceParameter, valueParameter);

            Action<TSource, TValue> result = (Action<TSource, TValue>)lambda.Compile();
            return result;
        }

        /// <summary>
        /// Gets an expression that can be assigned to the given target type. 
        /// Creates a new expression when a cast or conversion is required, 
        /// or returns the given <paramref name="expression"/> if no cast or conversion is required.
        /// </summary>
        /// <param name="expression">The expression which resulting value should be passed to a 
        /// parameter with a different type.</param>
        /// <param name="targetType">The target parameter type.</param>
        /// <returns>The <paramref name="expression"/> if no cast or conversion is required, 
        /// otherwise a new expression instance that wraps the the given <paramref name="expression"/> 
        /// inside the required cast or conversion.</returns>
        private Expression GetCastOrConvertExpression(Expression expression, Type targetType)
        {
            Expression result;
            Type expressionType = expression.Type;

            // Check if a cast or conversion is required.
            if (targetType.IsAssignableFrom(expressionType))
            {
                result = expression;
            }
            else
            {
                // Check if we can use the as operator for casting or if we must use the convert method
                if (targetType.IsValueType && !IsNullableType(targetType))
                {
                    result = Expression.Convert(expression, targetType);
                }
                else
                {
                    result = Expression.TypeAs(expression, targetType);
                }
            }

            return result;
        }


        #region Called by reflection - Don't delete.

        /// <summary>
        /// Stores the method info for the method that performs the assignment of the field value.
        /// Note: There is no assign expression in .NET 3.0/3.5. With .NET 4.0 this method becomes obsolete.
        /// </summary>
        private static readonly MethodInfo setFieldMethod =
            typeof(KlyteUtils).GetMethod("SetField",
            BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// A strong type method that assigns the given value to the field that is represented by the given field reference.
        /// Note: .NET 4.0 provides an assignment expression. This method is just required for .NET 3.0/3.5.
        /// </summary>
        /// <typeparam name="TValue">The type of the field.</typeparam>
        /// <param name="field">A reference to the field.</param>
        /// <param name="newValue">The new value that should be assigned to the field.</param>
        private static void SetField<TValue>(ref TValue field, TValue newValue)
        {
            field = newValue;
        }

        #endregion Called by reflection - Don't delete.

        #endregion Field Access

        /// <summary>
        /// Determines whether the given type is a nullable type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>true if the given type is a nullable type, otherwise false.</returns>
        public static bool IsNullableType(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            bool result = false;
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                result = true;
            }

            return result;
        }

        public static bool HasField(object o, string fieldName)
        {
            var fields = o.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var f in fields)
            {
                if (f.Name == fieldName)
                {
                    return true;
                }
            }
            return false;
        }
        public static List<Type> GetSubtypesRecursive(Type typeTarg, Type refType)
        {
            //doLog($"typeTarg = {typeTarg} | IsGenType={typeTarg.IsGenericType} ");
            var classes = from t in AppDomain.CurrentDomain.GetAssemblies().Where(x => refType == null || x == refType.Assembly)?.SelectMany(x =>
            {
                try { return x?.GetTypes(); } catch { return new Type[0]; }
            })
                          let y = t.BaseType
                          where t.IsClass && y != null && y.IsGenericType && y.GetGenericTypeDefinition() == typeTarg
                          select t;
            List<Type> result = new List<Type>();
            //doLog($"classes:\r\n\t {string.Join("\r\n\t", classes.Select(x => x.ToString()).ToArray())} ");
            foreach (Type t in classes)
            {
                if (!t.IsSealed)
                {
                    result.AddRange(GetSubtypesRecursive(t, t));
                }
                if (!t.IsAbstract)
                {
                    result.Add(t);
                }
            }
            return result;
        }

        public static Type GetImplementationForGenericType(Type typeOr, params Type[] typeArgs)
        {
            var typeTarg = typeOr.MakeGenericType(typeArgs);

            var instances = from t in Assembly.GetAssembly(typeOr).GetTypes()
                            where t.IsClass && !t.IsAbstract && typeTarg.IsAssignableFrom(t) && !t.IsGenericType
                            select t;
            if (instances.Count() != 1)
            {
                throw new Exception($"Defininções inválidas para [{ String.Join(", ", typeArgs.Select(x => x.ToString()).ToArray()) }] no tipo genérico {typeOr}");
            }

            Type targetType = instances.First();
            return targetType;
        }
        #endregion

        #region XML Utils

        public static T DefaultXmlDeserialize<T>(string s)
        {
            XmlSerializer xmlser = new XmlSerializer(typeof(T));
            try
            {
                using (TextReader tr = new StringReader(s))
                {
                    using (XmlReader reader = XmlReader.Create(tr))
                    {
                        if (xmlser.CanDeserialize(reader))
                        {
                            var val = (T)xmlser.Deserialize(reader);
                            return val;
                        }
                        else
                        {
                            doErrorLog($"CAN'T DESERIALIZE {typeof(T)}!\nText : {s}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                doErrorLog($"CAN'T DESERIALIZE {typeof(T)}!\nText : {s}\n{e.Message}\n{e.StackTrace}");
            }
            return default(T);
        }

        public static string DefaultXmlSerialize<T>(T targetObj, bool indent = true)
        {
            XmlSerializer xmlser = new XmlSerializer(typeof(T));
            XmlWriterSettings settings = new XmlWriterSettings { Indent = indent };
            using (StringWriter textWriter = new StringWriter())
            {
                using (XmlWriter xw = XmlWriter.Create(textWriter, settings))
                {
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("", "");
                    xmlser.Serialize(xw, targetObj, ns);
                    return textWriter.ToString();
                }
            }
        }

        public class ListWrapper<T>
        {
            [XmlElement("item")]
            public List<T> listVal = new List<T>();
        }

        #endregion

        public static void doLocaleDump()
        {
            string localeDump = "LOCALE DUMP:\r\n";
            try
            {
                var locale = GetPrivateField<Dictionary<Locale.Key, string>>(GetPrivateField<Locale>(LocaleManager.instance, "m_Locale"), "m_LocalizedStrings");
                foreach (Locale.Key k in locale.Keys)
                {
                    localeDump += string.Format("{0}  =>  {1}\n", k.ToString(), locale[k]);
                }
            }
            catch (Exception e)
            {

                KlyteUtils.doErrorLog("LOCALE DUMP FAIL: {0}", e.ToString());
            }
            Debug.LogWarning(localeDump);
        }
        public static bool findSimetry(int[] array, out int middle)
        {
            middle = -1;
            int size = array.Length;
            if (size == 0)
                return false;
            for (int j = -1; j < size / 2; j++)
            {
                int offsetL = (j + size) % size;
                int offsetH = (j + 2) % size;
                if (array[offsetL] == array[offsetH])
                {
                    middle = j + 1;
                    break;
                }
            }
            //			KlyteUtils.doLog("middle="+middle);
            if (middle >= 0)
            {
                for (int k = 1; k <= size / 2; k++)
                {
                    int offsetL = (-k + middle + size) % size;
                    int offsetH = (k + middle) % size;
                    if (array[offsetL] != array[offsetH])
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        #region Default (de)serialization
        public static string SerializeColor(Color32 value, string separator)
        {
            return string.Join(separator, new string[] { value.r.ToString(), value.g.ToString(), value.b.ToString() });
        }

        public static Color DeserializeColor(string value, string separator)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var list = value.Split(separator.ToCharArray()).ToList();
                if (list.Count == 3 && byte.TryParse(list[0], out byte r) && byte.TryParse(list[1], out byte g) && byte.TryParse(list[2], out byte b))
                {
                    return new Color32(r, g, b, 255);
                }
                else
                {
                    doLog($"val = {value}; list = {String.Join(",", list.ToArray())} (Size {list.Count})");
                }
            }
            return Color.clear;
        }
        #endregion

        #region Utility Numbering Arrays
        protected static string[] latinoMaiusculo = {
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z"
        };
        protected static string[] latinoMinusculo = {
            "a",
            "b",
            "c",
            "d",
            "e",
            "f",
            "g",
            "h",
            "i",
            "j",
            "k",
            "l",
            "m",
            "n",
            "o",
            "p",
            "q",
            "r",
            "s",
            "t",
            "u",
            "v",
            "w",
            "x",
            "y",
            "z"
        };
        protected static string[] gregoMaiusculo = {
            "Α",
            "Β",
            "Γ",
            "Δ",
            "Ε",
            "Ζ",
            "Η",
            "Θ",
            "Ι",
            "Κ",
            "Λ",
            "Μ",
            "Ν",
            "Ξ",
            "Ο",
            "Π",
            "Ρ",
            "Σ",
            "Τ",
            "Υ",
            "Φ",
            "Χ",
            "Ψ",
            "Ω"
        };
        protected static string[] gregoMinusculo = {
            "α",
            "β",
            "γ",
            "δ",
            "ε",
            "ζ",
            "η",
            "θ",
            "ι",
            "κ",
            "λ",
            "μ",
            "ν",
            "ξ",
            "ο",
            "π",
            "ρ",
            "σ",
            "τ",
            "υ",
            "φ",
            "χ",
            "ψ",
            "ω"
        };
        protected static string[] cirilicoMaiusculo = {
            "А",
            "Б",
            "В",
            "Г",
            "Д",
            "Е",
            "Ё",
            "Ж",
            "З",
            "И",
            "Й",
            "К",
            "Л",
            "М",
            "Н",
            "О",
            "П",
            "Р",
            "С",
            "Т",
            "У",
            "Ф",
            "Х",
            "Ц",
            "Ч",
            "Ш",
            "Щ",
            "Ъ",
            "Ы",
            "Ь",
            "Э",
            "Ю",
            "Я"
        };
        protected static string[] cirilicoMinusculo = {
            "а",
            "б",
            "в",
            "г",
            "д",
            "е",
            "ё",
            "ж",
            "з",
            "и",
            "й",
            "к",
            "л",
            "м",
            "н",
            "о",
            "п",
            "р",
            "с",
            "т",
            "у",
            "ф",
            "х",
            "ц",
            "ч",
            "ш",
            "щ",
            "ъ",
            "ы",
            "ь",
            "э",
            "ю",
            "я"
        };
        protected static string[] numeros = {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"
        };
        #endregion

        #region Vehicle Utils
        public static VehicleInfo GetRandomModel(List<string> assetList, out string selectedModel)
        {
            selectedModel = null;
            if (assetList.Count == 0) return null;
            Randomizer r = new Randomizer(new System.Random().Next());

            selectedModel = assetList[r.Int32(0, assetList.Count - 1)];

            var saida = PrefabCollection<VehicleInfo>.FindLoaded(selectedModel);
            if (saida == null)
            {
                KlyteUtils.doLog("MODEL DOESN'T EXIST!");
                return null;
            }
            return saida;
        }
        public static int getCapacity(VehicleInfo info, bool noLoop = false)
        {
            if (info == null) return -1;
            int capacity = GetPrivateField<int>(info.GetAI(), "m_passengerCapacity");
            try
            {
                if (!noLoop)
                {
                    foreach (var trailer in info.m_trailers)
                    {
                        capacity += getCapacity(trailer.m_info, true);
                    }
                }
            }
            catch (Exception e)
            {
                KlyteUtils.doLog("ERRO AO OBTER CAPACIDADE: [{0}] {1}", info, e.Message);
            }
            return capacity;
        }

        public static bool IsTrailer(PrefabInfo prefab)
        {
            string @unchecked = Locale.GetUnchecked("VEHICLE_TITLE", prefab.name);
            return @unchecked.StartsWith("VEHICLE_TITLE") || @unchecked.StartsWith("Trailer");
        }
        private static UIColorField s_colorFieldTemplate;
        public static UIColorField CreateColorField(UIComponent parent)
        {
            if (s_colorFieldTemplate == null)
            {
                UIComponent uIComponent = UITemplateManager.Get("LineTemplate");
                if (uIComponent == null)
                {
                    return null;
                }
                s_colorFieldTemplate = uIComponent.Find<UIColorField>("LineColor");
                if (s_colorFieldTemplate == null)
                {
                    return null;
                }
            }
            var go = GameObject.Instantiate(s_colorFieldTemplate.gameObject, parent.transform);
            UIColorField component = go.GetComponent<UIColorField>();
            component.pickerPosition = UIColorField.ColorPickerPosition.RightAbove;
            component.transform.SetParent(parent.transform);
            component.eventColorPickerOpen += DefaultColorPickerHandler;
            return component;
        }
        private static void DefaultColorPickerHandler(UIColorField dropdown, UIColorPicker popup, ref bool overridden)
        {
            var panel = popup.GetComponent<UIPanel>();
            overridden = true;
            panel.height = 250;
            createUIElement<UITextField>(out UITextField textField, panel.transform, "ColorText", new Vector4(15, 225, 200, 20));
            uiTextFieldDefaults(textField);
            textField.normalBgSprite = "TextFieldPanel";
            textField.maxLength = 6;
            textField.eventKeyUp += (x, y) =>
            {
                if (popup && textField.text.Length == 6)
                {
                    try
                    {
                        Color32 targetColor = ColorExtensions.FromRGB(((UITextField)x).text);
                        dropdown.selectedColor = targetColor;
                        popup.color = targetColor;
                        ((UITextField)x).textColor = Color.white;
                        ((UITextField)x).text = targetColor.ToRGB();
                    }
                    catch
                    {
                        ((UITextField)x).textColor = Color.red;
                    }
                }
            };
            popup.eventColorUpdated += (x) =>
            {
                textField.text = ((Color32)x).ToRGB();
            };
            textField.text = ((Color32)popup.color).ToRGB();
        }


        #endregion

        #region File & Prefab Utils
        public static readonly string BASE_FOLDER_PATH = DataLocation.localApplicationData + Path.DirectorySeparatorChar + "Klyte45Mods" + Path.DirectorySeparatorChar;

        public static FileInfo EnsureFolderCreation(string folderName)
        {
            if (File.Exists(folderName) && (File.GetAttributes(folderName) & FileAttributes.Directory) != FileAttributes.Directory)
            {
                File.Delete(folderName);
            }
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            return new FileInfo(folderName);
        }
        public static bool IsFileCreated(string fileName)
        {
            return File.Exists(fileName);
        }

        public static void ScanPrefabsFolders(string filenameToSearch, Action<FileStream> action)
        {
            List<string> list = new List<string>();
            ForEachLoadedPrefab<BuildingInfo>((loaded) =>
            {
                Package.Asset asset = PackageManager.FindAssetByName(loaded.name);
                if (!(asset == null) && !(asset.package == null))
                {
                    string packagePath = asset.package.packagePath;
                    if (packagePath != null)
                    {
                        string filePath = Path.Combine(Path.GetDirectoryName(packagePath), filenameToSearch);
                        if (!list.Contains(filePath))
                        {
                            list.Add(filePath);
                            if (File.Exists(filePath))
                            {
                                using (var stream = File.OpenRead(filePath))
                                {
                                    action(stream);
                                }
                            }
                        }
                    }
                }
            });
        }

        public static void ForEachLoadedPrefab<PI>(Action<PI> action) where PI : PrefabInfo
        {
            List<string> list = new List<string>();
            for (uint i = 0; i < PrefabCollection<PI>.LoadedCount(); i++)
            {
                PI loaded = PrefabCollection<PI>.GetLoaded(i);
                if (!(loaded == null))
                {
                    action(loaded);
                }
            }
        }
        #endregion

        #region Sorting

        public static int NaturalCompare(string left, string right)
        {
            return (int)typeof(PublicTransportDetailPanel).GetMethod("NaturalCompare", Redirector.allFlags).Invoke(null, new object[] { left, right });
        }

        public static void Quicksort(IList<UIComponent> elements, Comparison<UIComponent> comp, bool invert)
        {
            Quicksort(elements, 0, elements.Count - 1, comp, invert);
        }

        protected static void Quicksort(IList<UIComponent> elements, int left, int right, Comparison<UIComponent> comp, bool invert)
        {
            int i = left;
            int num = right;
            UIComponent y = elements[(left + right) / 2];
            int multiplier = invert ? -1 : 1;
            while (i <= num)
            {
                while (comp(elements[i], y) * multiplier < 0)
                {
                    i++;
                }
                while (comp(elements[num], y) * multiplier > 0)
                {
                    num--;
                }
                if (i <= num)
                {
                    UIComponent value = elements[i];
                    elements[i] = elements[num];
                    elements[i].forceZOrder = i;
                    elements[num] = value;
                    elements[num].forceZOrder = num;
                    i++;
                    num--;
                }
            }
            if (left < num)
            {
                Quicksort(elements, left, num, comp, invert);
            }
            if (i < right)
            {
                Quicksort(elements, i, right, comp, invert);
            }
        }
        #endregion

        #region Log Utils

        internal static void doLog(string format, params object[] args)
        {
            try
            {
                if (TransportLinesManagerMod.debugMode)
                {
                    Debug.LogWarningFormat("TLMv" + TransportLinesManagerMod.version + " " + format, args);
                }

            }
            catch
            {
                Debug.LogErrorFormat("KltUtils: Erro ao fazer log: {0} (args = {1})", format, args == null ? "[]" : string.Join(",", args.Select(x => x != null ? x.ToString() : "--NULL--").ToArray()));
            }
        }
        internal static void doErrorLog(string format, params object[] args)
        {
            try
            {
                Console.WriteLine("TLMv" + TransportLinesManagerMod.version + " " + format, args);
            }
            catch
            {
                Debug.LogErrorFormat("KltUtils: Erro ao fazer log: {0} (args = {1})", format, args == null ? "[]" : string.Join(",", args.Select(x => x != null ? x.ToString() : "--NULL--").ToArray()));
            }
        }
        #endregion

        #region Map Position

        public static Vector2 gridPositionGameDefault(Vector3 pos)
        {
            int x = Mathf.Max((int)((pos.x) / 64f + 135f), 0);
            int z = Mathf.Max((int)((-pos.z) / 64f + 135f), 0);
            return new Vector2(x, z);
        }


        public static Vector2 gridPosition81Tiles(Vector3 pos, float invResolution = 24f)
        {
            int x = Mathf.Max((int)((pos.x) / invResolution + 648), 0);
            int z = Mathf.Max((int)((-pos.z) / invResolution + 648), 0);
            return new Vector2(x, z);
        }

        public static Vector2 getMapTile(Vector3 pos)
        {
            float x = (pos.x + 8640f) / 1920f;
            float z = (pos.z + 8640f) / 1920f;
            return new Vector2(x, z);
        }

        public static Vector3 CalculatePositionRelative(Vector3 absolutePos, float angle, Vector3 reference)
        {
            Vector3 relativePos = new Vector3
            {
                y = absolutePos.y - reference.y
            };

            var cos = Mathf.Cos(angle);
            var sin = Mathf.Sin(angle);

            //           position.x = original.x +              cos * offset.x +  sin   * offset.z;
            //position.z            =              original.z + sin * offset.x + (-cos) * offset.z;

            //                   cos * position.x = cos * original.x +                                  cos * cos * offset.x  + sin *   cos  * offset.z;
            //sin * position.z                    =                    sin * original.z +               sin * sin * offset.x  + sin * (-cos) * offset.z;
            //==========================================================================================================================================
            //sin * position.z + cos * position.x = cos * original.x + sin * original.z + (cos * cos + sin * sin) * offset.x;

            relativePos.x = -(cos * reference.x + sin * reference.z - sin * absolutePos.z - cos * absolutePos.x);
            relativePos.z = (-absolutePos.x + reference.x + cos * relativePos.x) / -sin;

            return relativePos;
        }

        public static Vector3 OffsetWithRotation(Vector3 offset, float angle)
        {
            Vector3 position = new Vector3
            {
                y = offset.y
            };

            var cos = Mathf.Cos(angle);
            var sin = Mathf.Sin(angle);

            position.x = cos * offset.x + sin * offset.z;
            position.z = sin * offset.x + (-cos) * offset.z;

            return position;
        }

        #endregion

        #region Districts Utils
        public static Dictionary<string, int> getValidDistricts()
        {
            Dictionary<string, int> districts = new Dictionary<string, int>
            {
                [$"<{Singleton<SimulationManager>.instance.m_metaData.m_CityName}>"] = 0
            };
            for (int i = 1; i <= 0x7F; i++)
            {
                if ((Singleton<DistrictManager>.instance.m_districts.m_buffer[i].m_flags & District.Flags.Created) != District.Flags.None)
                {
                    String districtName = Singleton<DistrictManager>.instance.GetDistrictName(i);
                    if (districts.ContainsKey(districtName))
                    {
                        districtName += $" (ID:{i})";
                    }
                    districts[districtName] = i;
                }
            }

            return districts;
        }
        #endregion

        #region Segment Utils
        public static void UpdateSegmentNamesView()
        {
            if (Singleton<NetManager>.exists)
            {
                var nm = Singleton<NetManager>.instance;

                HashSet<ushort> segments = new HashSet<ushort>();

                for (ushort i = 1; i < nm.m_segments.m_size; i++)
                {
                    if (nm.m_segments.m_buffer[i].m_flags != 0)
                    {
                        nm.UpdateSegmentRenderer(i, false);
                        segments.Add(i);
                    }
                }
                nm.m_updateNameVisibility = segments;
            }
        }

        private static List<ushort> calculatePathNet(ushort segmentID, bool startSegment, bool requireSameDirection, bool requireSameSizeAndType)
        {
            List<ushort> result = new List<ushort>();
            ushort edgeNode;
            if (startSegment)
            {
                edgeNode = NetManager.instance.m_segments.m_buffer[segmentID].m_startNode;
            }
            else
            {
                edgeNode = NetManager.instance.m_segments.m_buffer[segmentID].m_endNode;
            }
            calculatePathNet(segmentID, segmentID, edgeNode, startSegment, ref result, requireSameDirection, requireSameSizeAndType);
            return result;
        }


        private static void calculatePathNet(ushort firstSegmentID, ushort segmentID, ushort nodeCurr, bool insertOnEnd, ref List<ushort> segmentOrder, bool requireSameDirection, bool requireSameSizeAndType)
        {
            var strict = requireSameDirection || requireSameSizeAndType;
            ushort otherEdgeNode = 0;
            List<ushort> possibilities = new List<ushort>();
            int crossingDirectionsCounter = 0;
            for (int i = 0; i < 8; i++)
            {
                ushort segment = NetManager.instance.m_nodes.m_buffer[nodeCurr].GetSegment(i);

                if (segment != 0 && firstSegmentID != segment && segment != segmentID && !segmentOrder.Contains(segment) && IsSameName(segmentID, segment, false))
                {
                    crossingDirectionsCounter++;
                    if (strict && !IsSameName(segmentID, segment, requireSameDirection, requireSameSizeAndType))
                    {
                        continue;
                    }
                    possibilities.Add(segment);
                }
            }
            if (strict && crossingDirectionsCounter > 1)
            {
                return;
            }
            if (possibilities.Count > 0)
            {
                ushort segment = possibilities.Min();
                otherEdgeNode = NetManager.instance.m_segments.m_buffer[segment].GetOtherNode(nodeCurr);
                if (insertOnEnd || segmentOrder.Count == 0)
                {
                    segmentOrder.Add(segment);
                }
                else
                {
                    segmentOrder.Insert(0, segment);
                }
                calculatePathNet(firstSegmentID, segment, otherEdgeNode, insertOnEnd, ref segmentOrder, requireSameDirection, requireSameSizeAndType);
            }
        }

        public static List<ushort> GetCrossingPath(ushort relSegId)
        {
            ushort nodeCurr = NetManager.instance.m_segments.m_buffer[relSegId].m_startNode;
            List<ushort> possibilities = new List<ushort>();
            for (int i = 0; i < 8; i++)
            {
                ushort segment = NetManager.instance.m_nodes.m_buffer[nodeCurr].GetSegment(i);

                if (segment != 0 && relSegId != segment && !IsSameName(relSegId, segment, false))
                {
                    foreach (ushort prevSeg in possibilities)
                    {
                        if (IsSameName(prevSeg, segment, false))
                        {
                            goto LoopEnd;
                        }
                    }
                    possibilities.Add(segment);
                }
                LoopEnd: continue;
            }
            nodeCurr = NetManager.instance.m_segments.m_buffer[relSegId].GetOtherNode(nodeCurr);
            for (int i = 0; i < 8; i++)
            {
                ushort segment = NetManager.instance.m_nodes.m_buffer[nodeCurr].GetSegment(i);

                if (segment != 0 && relSegId != segment && !IsSameName(relSegId, segment, false))
                {
                    foreach (ushort prevSeg in possibilities)
                    {
                        if (IsSameName(prevSeg, segment, false))
                        {
                            goto LoopEnd2;
                        }
                    }
                    possibilities.Add(segment);
                }
                LoopEnd2: continue;
            }
            return possibilities;
        }

        public static List<ushort> getSegmentOrderRoad(ushort segmentID, bool requireSameDirection, bool requireSameSizeAndType, bool localAdjust, out bool startRef, out bool endRef)
        {
            var flags = NetManager.instance.m_segments.m_buffer[segmentID].m_flags;
            if (segmentID != 0 && flags != NetSegment.Flags.None)
            {
                List<ushort> path = calculatePathNet(segmentID, false, requireSameDirection, requireSameSizeAndType);
                path.Add(segmentID);

                ushort startNode0 = NetManager.instance.m_segments.m_buffer[path[0]].m_startNode;
                ushort endNode0 = NetManager.instance.m_segments.m_buffer[path[0]].m_endNode;
                ushort startNodeRef = NetManager.instance.m_segments.m_buffer[segmentID].m_startNode;
                ushort endNodeRef = NetManager.instance.m_segments.m_buffer[segmentID].m_endNode;

                bool circular = (path.Count > 2 && (startNode0 == endNodeRef || startNode0 == startNodeRef || endNode0 == endNodeRef || endNode0 == startNodeRef)) ||
                    (path.Count == 2 && (startNode0 == endNodeRef || startNode0 == startNodeRef) && (endNode0 == endNodeRef || endNode0 == startNodeRef));

                if (circular)
                {
                    doLog("Circular!");
                    var refer = path.Min();
                    var referIdx = path.IndexOf(refer);
                    if (referIdx != 0)
                    {
                        path = path.GetRange(referIdx, path.Count - referIdx).Union(path.Take(referIdx)).ToList();
                    }
                }
                else
                {
                    path.AddRange(calculatePathNet(segmentID, true, requireSameDirection, requireSameSizeAndType));
                }
                //doLog($"[s={strict}]path = [{string.Join(",", path.Select(x => x.ToString()).ToArray())}]");
                GetEdgeNodes(ref path, out startRef, out endRef, localAdjust);
                return path;
            }
            startRef = false;
            endRef = false;
            return null;
        }

        public static void GetClosestPositionAndDirectionAndPoint(NetSegment s, Vector3 point, out Vector3 pos, out Vector3 dir, out float length)
        {
            Bezier3 curve = default(Bezier3);
            curve.a = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)s.m_startNode].m_position;
            curve.d = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)s.m_endNode].m_position;
            bool smoothStart = (Singleton<NetManager>.instance.m_nodes.m_buffer[(int)s.m_startNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            bool smoothEnd = (Singleton<NetManager>.instance.m_nodes.m_buffer[(int)s.m_endNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            NetSegment.CalculateMiddlePoints(curve.a, s.m_startDirection, curve.d, s.m_endDirection, smoothStart, smoothEnd, out curve.b, out curve.c);
            float closestDistance = 1E+11f;
            float pointPerc = 0f;
            Vector3 targetA = curve.a;
            for (int i = 1; i <= 16; i++)
            {
                Vector3 vector = curve.Position((float)i / 16f);
                float dist = Segment3.DistanceSqr(targetA, vector, point, out float distSign);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    pointPerc = ((float)i - 1f + distSign) / 16f;
                }
                targetA = vector;
            }
            float precision = 0.03125f;
            for (int j = 0; j < 4; j++)
            {
                Vector3 a2 = curve.Position(Mathf.Max(0f, pointPerc - precision));
                Vector3 vector2 = curve.Position(pointPerc);
                Vector3 b = curve.Position(Mathf.Min(1f, pointPerc + precision));
                float num6 = Segment3.DistanceSqr(a2, vector2, point, out float num7);
                float num8 = Segment3.DistanceSqr(vector2, b, point, out float num9);
                if (num6 < num8)
                {
                    pointPerc = Mathf.Max(0f, pointPerc - precision * (1f - num7));
                }
                else
                {
                    pointPerc = Mathf.Min(1f, pointPerc + precision * num9);
                }
                precision *= 0.5f;
            }
            pos = curve.Position(pointPerc);
            dir = VectorUtils.NormalizeXZ(curve.Tangent(pointPerc));
            length = pointPerc;
        }
        public static bool IsSameName(ushort segment1, ushort segment2, bool strict = false)
        {
            return IsSameName(segment1, segment2, strict, strict);
        }
        public static bool IsSameName(ushort segment1, ushort segment2, bool requireSameDirection, bool requireSameSizeAndHighway)
        {
            return IsSameName(segment1, segment2, requireSameDirection, requireSameSizeAndHighway, requireSameSizeAndHighway);
        }
        public static bool IsSameName(ushort segment1, ushort segment2, bool requireSameDirection, bool requireSameSize, bool requireSameHigwayFlag, bool requireSameDistrict = false, bool requireSameAI = false)
        {
            NetManager nm = NetManager.instance;
            NetInfo info = nm.m_segments.m_buffer[(int)segment1].Info;
            NetInfo info2 = nm.m_segments.m_buffer[(int)segment2].Info;
            if (info.m_class.m_service != info2.m_class.m_service)
            {
                return false;
            }
            if (info.m_class.m_subService != info2.m_class.m_subService)
            {
                return false;
            }
            if (requireSameDirection)
            {
                var seg1 = nm.m_segments.m_buffer[(int)segment1];
                var seg2 = nm.m_segments.m_buffer[(int)segment2];
                if ((seg1.m_endNode == seg2.m_endNode || seg1.m_startNode == seg2.m_startNode) && (nm.m_segments.m_buffer[(int)segment1].m_flags & NetSegment.Flags.Invert) == (nm.m_segments.m_buffer[(int)segment2].m_flags & NetSegment.Flags.Invert))
                {
                    return false;
                }
                if ((seg1.m_endNode == seg2.m_startNode || seg1.m_startNode == seg2.m_endNode) && (nm.m_segments.m_buffer[(int)segment1].m_flags & NetSegment.Flags.Invert) != (nm.m_segments.m_buffer[(int)segment2].m_flags & NetSegment.Flags.Invert))
                {
                    return false;
                }
            }

            if (requireSameSize)
            {
                if (!((info.m_forwardVehicleLaneCount == info2.m_backwardVehicleLaneCount && info2.m_forwardVehicleLaneCount == info.m_backwardVehicleLaneCount)
                    || (info2.m_forwardVehicleLaneCount == info.m_forwardVehicleLaneCount && info2.m_backwardVehicleLaneCount == info.m_backwardVehicleLaneCount)))
                {
                    return false;
                }
            }
            if (requireSameAI)
            {
                if (info.GetAI().GetType() != info2.GetAI().GetType())
                {
                    return false;
                }
            }
            else if (requireSameHigwayFlag)
            {
                if ((info.GetAI() as RoadBaseAI)?.m_highwayRules != (info2.GetAI() as RoadBaseAI)?.m_highwayRules)
                {
                    return false;
                }
            }
            if (requireSameDistrict)
            {
                var pos1 = nm.m_segments.m_buffer[(int)segment1].m_middlePosition;
                var pos2 = nm.m_segments.m_buffer[(int)segment2].m_middlePosition;
                if (DistrictManager.instance.GetDistrict(pos1) != DistrictManager.instance.GetDistrict(pos2))
                {
                    return false;
                }
            }
            bool customName1 = (nm.m_segments.m_buffer[(int)segment1].m_flags & NetSegment.Flags.CustomName) != NetSegment.Flags.None;
            bool customName2 = (nm.m_segments.m_buffer[(int)segment2].m_flags & NetSegment.Flags.CustomName) != NetSegment.Flags.None;
            if (customName1 != customName2)
            {
                return false;
            }
            if (customName1)
            {
                InstanceID id = default(InstanceID);
                id.NetSegment = segment1;
                string name = Singleton<InstanceManager>.instance.GetName(id);
                id.NetSegment = segment2;
                string name2 = Singleton<InstanceManager>.instance.GetName(id);
                return name == name2;
            }
            ushort nameSeed = nm.m_segments.m_buffer[(int)segment1].m_nameSeed;
            ushort nameSeed2 = nm.m_segments.m_buffer[(int)segment2].m_nameSeed;
            return nameSeed == nameSeed2;
        }



        private static List<ushort> GetEdgeNodes(ref List<ushort> accessSegments, out bool nodeStartS, out bool nodeStartE, bool localAdjust)
        {
            if (accessSegments.Count > 1)
            {
                var seg0 = NetManager.instance.m_segments.m_buffer[accessSegments[0]];
                var seg1 = NetManager.instance.m_segments.m_buffer[accessSegments[1]];
                var segN2 = NetManager.instance.m_segments.m_buffer[accessSegments[accessSegments.Count - 2]];
                var segN1 = NetManager.instance.m_segments.m_buffer[accessSegments[accessSegments.Count - 1]];
                nodeStartS = seg1.m_startNode == seg0.m_endNode || seg1.m_endNode == seg0.m_endNode;
                nodeStartE = segN2.m_startNode == segN1.m_endNode || segN2.m_endNode == segN1.m_endNode;
                if (localAdjust)
                {
                    if (nodeStartS == ((NetManager.instance.m_segments.m_buffer[accessSegments[0]].m_flags & NetSegment.Flags.Invert) != 0))
                    {
                        accessSegments.Reverse();
                        var temp = nodeStartS;
                        nodeStartS = nodeStartE;
                        nodeStartE = temp;
                    }
                }
                else
                {
                    var nodeF = nodeStartS ? seg0.m_startNode : seg0.m_endNode;
                    var nodeL = nodeStartE ? segN1.m_startNode : segN1.m_endNode;
                    if (VectorUtils.XZ(NetManager.instance.m_nodes.m_buffer[nodeF].m_position).GetAngleToPoint(VectorUtils.XZ(NetManager.instance.m_nodes.m_buffer[nodeL].m_position)) > 180)
                    {
                        accessSegments.Reverse();
                        var temp = nodeStartS;
                        nodeStartS = nodeStartE;
                        nodeStartE = temp;
                    }
                }
            }
            else
            {
                nodeStartS = (NetManager.instance.m_segments.m_buffer[accessSegments[0]].m_flags & NetSegment.Flags.Invert) == 0;
                nodeStartE = (NetManager.instance.m_segments.m_buffer[accessSegments[0]].m_flags & NetSegment.Flags.Invert) != 0;
            }

            return accessSegments;
        }

        public static IEnumerable<Tuple<ushort, float>> GetSegmentRoadEdges(ushort segmentId, bool requireSameDirection, bool requireSameSizeAndType, bool localAdjust, out ComparableRoad startRef, out ComparableRoad endRef)
        {
            List<ushort> accessSegments = getSegmentOrderRoad(segmentId, requireSameDirection, requireSameSizeAndType, localAdjust, out bool nodeStartS, out bool nodeStartE);
            if (accessSegments == null)
            {
                startRef = default(ComparableRoad);
                endRef = default(ComparableRoad);
                return null;
            }
            //doLog($"[{segmentId}-> sd {requireSameDirection} rsst {requireSameSizeAndType} la {localAdjust}] segs = [{string.Join(",", accessSegments.Select(x => x.ToString()).ToArray())}]; start={nodeStartS}; end={nodeStartE}");
            startRef = new ComparableRoad(accessSegments[0], nodeStartS);
            endRef = new ComparableRoad(accessSegments[accessSegments.Count - 1], nodeStartE);
            return accessSegments.Select(x => Tuple.New(x, NetManager.instance.m_segments.m_buffer[x].m_averageLength));
        }

        public struct ComparableRoad
        {
            public ComparableRoad(ushort segmentId, bool startNode)
            {
                var segment = NetManager.instance.m_segments.m_buffer[segmentId];
                if (startNode)
                {
                    nodeReference = segment.m_startNode;
                }
                else
                {
                    nodeReference = segment.m_endNode;
                }

                bool entering = startNode != ((NetManager.instance.m_segments.m_buffer[segmentId].m_flags & NetSegment.Flags.Invert) != 0);

                var node = NetManager.instance.m_nodes.m_buffer[nodeReference];
                isPassing = false;
                segmentReference = 0;
                for (int i = 0; i < 7; i++)
                {
                    var segment1 = node.GetSegment(i);
                    if (segment1 > 0 && segment1 != segmentId)
                    {
                        for (int j = i + 1; j < 8; j++)
                        {
                            var segment2 = node.GetSegment(j);
                            if (segment2 > 0 && segment2 != segmentId)
                            {
                                isPassing = IsSameName(segment1, segment2, true);
                                if (isPassing)
                                {
                                    segmentReference = segment1;
                                    break;
                                }
                            }
                        }
                        if (isPassing) break;
                    }
                }
                if (!isPassing)
                {

                    for (int i = 0; i < 7; i++)
                    {
                        var segment1 = node.GetSegment(i);
                        if (segment1 > 0 && segment1 != segmentId)
                        {
                            var segment1Obj = NetManager.instance.m_segments.m_buffer[segment1];
                            bool isSegment1StartNode = segment1Obj.m_startNode == nodeReference;
                            bool isSegment1Entering = isSegment1StartNode != ((NetManager.instance.m_segments.m_buffer[segment1].m_flags & NetSegment.Flags.Invert) != 0);

                            if (!(segment1Obj.Info.GetAI() is RoadBaseAI seg1Ai)) continue;
                            bool isSegment1TwoWay = segment1Obj.Info.m_hasBackwardVehicleLanes && segment1Obj.Info.m_hasForwardVehicleLanes;

                            if (!isSegment1TwoWay && isSegment1Entering == entering)
                            {
                                doLog($"IGNORED: {segment1} (Tw=>{isSegment1TwoWay},entering=>{entering},s1entering=>{isSegment1Entering})");
                                continue;
                            }

                            if (segmentReference == 0)
                            {
                                segmentReference = segment1;
                            }
                            else
                            {
                                NetSegment segmentRefObj = NetManager.instance.m_segments.m_buffer[segmentReference];
                                if (!(segmentRefObj.Info.GetAI() is RoadBaseAI roadAi))
                                {
                                    continue;
                                }
                                if (!roadAi.m_highwayRules && seg1Ai.m_highwayRules)
                                {
                                    segmentReference = segment1;
                                    continue;
                                }
                                if (roadAi.m_highwayRules && !seg1Ai.m_highwayRules)
                                {
                                    continue;
                                }
                                int laneCount1 = (segment1Obj.Info.m_forwardVehicleLaneCount + segment1Obj.Info.m_backwardVehicleLaneCount);
                                int laneCountRef = (segmentRefObj.Info.m_forwardVehicleLaneCount + segmentRefObj.Info.m_backwardVehicleLaneCount);
                                if (laneCount1 > laneCountRef)
                                {
                                    segmentReference = segment1;
                                    continue;
                                }
                                if (laneCount1 < laneCountRef)
                                {
                                    continue;
                                }
                                if (segment1Obj.Info.m_halfWidth > segmentRefObj.Info.m_halfWidth)
                                {
                                    segmentReference = segment1;
                                    continue;
                                }
                            }
                        }
                    }
                }
                if (segmentReference > 0)
                {
                    var segmentRefObj = NetManager.instance.m_segments.m_buffer[segmentReference];
                    NetInfo infoRef = segmentRefObj.Info;
                    isHighway = infoRef.GetAI() is RoadBaseAI aiRef && aiRef.m_highwayRules;
                    width = infoRef.m_halfWidth * 2;
                    lanes = infoRef.m_backwardVehicleLaneCount + infoRef.m_forwardVehicleLaneCount;
                }
                else
                {
                    isHighway = false;
                    width = 0;
                    lanes = 0;
                }
            }

            public readonly ushort nodeReference;
            public readonly ushort segmentReference;
            public readonly bool isHighway;
            public readonly bool isPassing;
            public readonly float width;
            public readonly int lanes;

            public int compareTo(ComparableRoad otherRoad)
            {
                int result = 0;
                if (otherRoad.isHighway != isHighway)
                {
                    result = isHighway ? 1 : -1;
                }
                if (otherRoad.isPassing != isPassing)
                {
                    result = isPassing ? 1 : -1;
                }
                if (otherRoad.width != width)
                {
                    result = otherRoad.width < width ? 1 : -1;
                }
                if (otherRoad.lanes != lanes)
                {
                    result = otherRoad.lanes < lanes ? 1 : -1;
                }
                doLog($"cmp: {this} & {otherRoad} => {result}");
                return result;
            }

            public override string ToString()
            {
                return $"[n{nodeReference} s{segmentReference} h:{isHighway} p:{isPassing} w{width} l{lanes}]";
            }
        }

        public static byte GetCardinalDirection(ComparableRoad startRef, ComparableRoad endRef)
        {
            var nodeS = NetManager.instance.m_nodes.m_buffer[startRef.nodeReference];
            var nodeE = NetManager.instance.m_nodes.m_buffer[endRef.nodeReference];

            var cardinalDirection = CardinalPoint.getCardinalPoint(VectorUtils.XZ(nodeS.m_position).GetAngleToPoint(VectorUtils.XZ(nodeE.m_position))).getCardinalIndex8();
            return cardinalDirection;
        }


        #endregion

        #region Streets Addressing
        private static ushort[] m_closestSegsFind = new ushort[16];
        public static bool GetAddressStreetAndNumber(Vector3 sidewalk, Vector3 midPosBuilding, out int number, out string streetName)
        {
            GetNearestSegment(sidewalk, out Vector3 targetPosition, out float targetLength, out ushort targetSegmentId);
            if (targetSegmentId == 0)
            {
                number = 0;
                streetName = string.Empty;
                return false;
            }

            number = GetNumberAt(targetLength, targetSegmentId, out bool startAsEnd);
            streetName = NetManager.instance.GetSegmentName(targetSegmentId)?.ToString();
            float angleTg = VectorUtils.XZ(targetPosition).GetAngleToPoint(VectorUtils.XZ(midPosBuilding));
            if (angleTg == 90 || angleTg == 270)
            {
                angleTg += Math.Sign(targetPosition.z - midPosBuilding.z);
            }

            NetSegment targSeg = NetManager.instance.m_segments.m_buffer[targetSegmentId];
            Vector3 startSeg = NetManager.instance.m_nodes.m_buffer[targSeg.m_startNode].m_position;
            Vector3 endSeg = NetManager.instance.m_nodes.m_buffer[targSeg.m_endNode].m_position;

            float angleSeg = Vector2.zero.GetAngleToPoint(VectorUtils.XZ(endSeg - startSeg));
            if (angleSeg == 180)
            {
                angleSeg += Math.Sign(targetPosition.z - midPosBuilding.z);
            }


            //doLog($"angleTg = {angleTg};angleSeg = {angleSeg}");
            if ((angleTg + 90) % 360 < 180 ^ (angleSeg < 180 ^ startAsEnd))
            {
                number &= ~1;
            }
            else
            {
                number |= 1;
            }
            return true;
        }
        public static void GetNearestSegment(Vector3 sidewalk, out Vector3 targetPosition, out float targetLength, out ushort targetSegmentId)
        {
            NetManager.instance.GetClosestSegments(sidewalk, m_closestSegsFind, out int found);
            targetPosition = default(Vector3);
            Vector3 targetDirection = default(Vector3);
            targetLength = 0;
            targetSegmentId = 0;
            if (found == 0)
            {
                return;
            }
            else if (found > 1)
            {
                float minSqrDist = float.MaxValue;
                for (int i = 0; i < found; i++)
                {
                    ushort segId = m_closestSegsFind[i];
                    var seg = NetManager.instance.m_segments.m_buffer[segId];
                    if (!(seg.Info.GetAI() is RoadBaseAI)) continue;
                    GetClosestPositionAndDirectionAndPoint(seg, sidewalk, out Vector3 position, out Vector3 direction, out float length);
                    float sqrDist = Vector3.SqrMagnitude(sidewalk - position);
                    if (i == 0 || sqrDist < minSqrDist)
                    {
                        minSqrDist = sqrDist;
                        targetPosition = position;
                        targetDirection = direction;
                        targetLength = length;
                        targetSegmentId = segId;
                    }
                }
            }
            else
            {
                targetSegmentId = m_closestSegsFind[0];
                GetClosestPositionAndDirectionAndPoint(NetManager.instance.m_segments.m_buffer[targetSegmentId], sidewalk, out targetPosition, out targetDirection, out targetLength);
            }
        }


        public static int GetNumberAt(ushort targetSegmentId, bool startNode)
        {
            List<ushort> roadSegments = getSegmentOrderRoad(targetSegmentId, false, false, false, out bool startRef, out bool endRef);
            int targetSegmentIdIdx = roadSegments.IndexOf(targetSegmentId);
            var targSeg = NetManager.instance.m_segments.m_buffer[targetSegmentId];
            ushort targetNode = startNode ? targSeg.m_startNode : targSeg.m_endNode;
            NetSegment preSeg = default(NetSegment);
            if (targetSegmentIdIdx > 0)
            {
                preSeg = NetManager.instance.m_segments.m_buffer[roadSegments[targetSegmentIdIdx - 1]];
            }
            if (preSeg.m_endNode != targetNode && preSeg.m_startNode != targetNode)
            {
                targetSegmentIdIdx++;
            }
            float distanceFromStart = 0;
            for (int i = 0; i < targetSegmentIdIdx; i++)
            {
                distanceFromStart += NetManager.instance.m_segments.m_buffer[roadSegments[i]].m_averageLength;
            }
            return (int)Math.Round(distanceFromStart);
        }

        private static int GetNumberAt(float targetLength, ushort targetSegmentId, out bool startAsEnd)
        {
            //doLog($"targets = S:{targetSegmentId} P:{targetPosition} D:{targetDirection.magnitude} L:{targetLength}");

            List<ushort> roadSegments = getSegmentOrderRoad(targetSegmentId, false, false, false, out bool startRef, out bool endRef);
            //doLog("roadSegments = [{0}] ", string.Join(",", roadSegments.Select(x => x.ToString()).ToArray()));
            int targetSegmentIdIdx = roadSegments.IndexOf(targetSegmentId);
            //doLog($"targSeg = {targetSegmentIdIdx} ({targetSegmentId})");
            var targSeg = NetManager.instance.m_segments.m_buffer[targetSegmentId];
            NetSegment preSeg = default(NetSegment);
            NetSegment posSeg = default(NetSegment);
            //doLog("PreSeg");
            if (targetSegmentIdIdx > 0)
            {
                preSeg = NetManager.instance.m_segments.m_buffer[roadSegments[targetSegmentIdIdx - 1]];
            }
            //doLog("PosSeg");
            if (targetSegmentIdIdx < roadSegments.Count - 1)
            {
                posSeg = NetManager.instance.m_segments.m_buffer[roadSegments[targetSegmentIdIdx + 1]];
            }
            //doLog("startAsEnd");
            startAsEnd = (targetSegmentIdIdx > 0 && (preSeg.m_endNode == targSeg.m_endNode || preSeg.m_startNode == targSeg.m_endNode)) || (targetSegmentIdIdx < roadSegments.Count && (posSeg.m_endNode == targSeg.m_startNode || posSeg.m_startNode == targSeg.m_startNode));

            //doLog($"startAsEnd = {startAsEnd}");

            if (startAsEnd)
            {
                targetLength = 1 - targetLength;
            }
            //doLog("streetName"); 

            //doLog("distanceFromStart");
            float distanceFromStart = 0;
            for (int i = 0; i < targetSegmentIdIdx; i++)
            {
                distanceFromStart += NetManager.instance.m_segments.m_buffer[roadSegments[i]].m_averageLength;
            }
            distanceFromStart += targetLength * targSeg.m_averageLength;
            return (int)Math.Round(distanceFromStart);

            //doLog($"number = {number} B");
        }
        #endregion
    }



    public class UIRadialChartExtended : UIRadialChart
    {
        public void AddSlice(Color32 innerColor, Color32 outterColor)
        {
            SliceSettings slice = new UIRadialChart.SliceSettings
            {
                outterColor = outterColor,
                innerColor = innerColor
            };
            this.m_Slices.Add(slice);
            this.Invalidate();
        }
        public void SetValues(float offset, int[] percentages)
        {
            if (percentages.Length != this.sliceCount)
            {
                CODebugBase<InternalLogChannel>.Error(InternalLogChannel.UI, string.Concat(new object[]
                {
            "Percentage count should be ",
            sliceCount,
            " but is ",
            percentages.Length
                }), base.gameObject);
                return;
            }
            float num = offset;
            for (int i = 0; i < this.sliceCount; i++)
            {
                SliceSettings sliceSettings = this.m_Slices[i];
                sliceSettings.Setter(null);
                sliceSettings.startValue = Mathf.Max(num % 1, 0f);
                num += percentages[i] * 0.01f;
                sliceSettings.endValue = Mathf.Min(num % 1, 1f);
            }
            this.Invalidate();
        }
        public void SetValuesStarts(int[] starts)
        {
            if (starts.Length != sliceCount)
            {
                CODebugBase<InternalLogChannel>.Error(InternalLogChannel.UI, string.Concat(new object[]
                {
            "Starts count should be ",
            sliceCount,
            " but is ",
            starts.Length
                }), base.gameObject);
                return;
            }
            float num = 0;
            for (int i = 0; i < sliceCount; i++)
            {
                SliceSettings sliceSettings = this.m_Slices[i];
                sliceSettings.Setter(null);
                sliceSettings.startValue = num;
                if (i == sliceCount - 1)
                {
                    num = 1f;
                }
                else
                {
                    num = (starts[i + 1]) * 0.01f;
                }
                sliceSettings.endValue = num;
            }
            this.Invalidate();
        }
    }

    public class Range<T> where T : IComparable<T>
    {
        /// <summary>
        /// Minimum value of the range
        /// </summary>
        public T Minimum { get; set; }

        /// <summary>
        /// Maximum value of the range
        /// </summary>
        public T Maximum { get; set; }

        public Range(T min, T max)
        {
            if (min.CompareTo(max) >= 0)
            {
                var temp = min;
                min = max;
                max = temp;
            }
            Minimum = min;
            Maximum = max;
        }

        /// <summary>
        /// Presents the Range in readable format
        /// </summary>
        /// <returns>String representation of the Range</returns>
        public override string ToString() => $"[{Minimum} - {Maximum}]";

        /// <summary>
        /// Determines if the range is valid
        /// </summary>
        /// <returns>True if range is valid, else false</returns>
        public Boolean IsValid() => Minimum.CompareTo(Maximum) <= 0;

        /// <summary>
        /// Determines if the provided value is inside the range
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns>True if the value is inside Range, else false</returns>
        public Boolean ContainsValue(T value) => (Minimum.CompareTo(value) <= 0) && (value.CompareTo(Maximum) <= 0);


        /// <summary>
        /// Determines if the provided value is inside the range
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns>True if the value is inside Range, else false</returns>
        public Boolean IsBetweenLimits(T value) => (Minimum.CompareTo(value) < 0) && (value.CompareTo(Maximum) < 0);

        /// <summary>
        /// Determines if this Range is inside the bounds of another range
        /// </summary>
        /// <param name="Range">The parent range to test on</param>
        /// <returns>True if range is inclusive, else false</returns>
        public Boolean IsInsideRange(Range<T> Range) => this.IsValid() && Range.IsValid() && Range.ContainsValue(this.Minimum) && Range.ContainsValue(this.Maximum);



        /// <summary>
        /// Determines if another range is inside the bounds of this range
        /// </summary>
        /// <param name="Range">The child range to test</param>
        /// <returns>True if range is inside, else false</returns>
        public Boolean ContainsRange(Range<T> Range) => this.IsValid() && Range.IsValid() && this.ContainsValue(Range.Minimum) && this.ContainsValue(Range.Maximum);

        /// <summary>
        /// Determines if another range intersect this range
        /// </summary>
        /// <param name="Range">The child range to test</param>
        /// <returns>True if range is inside, else false</returns>
        public Boolean IntersectRange(Range<T> Range) => this.IsValid() && Range.IsValid() && (this.ContainsValue(Range.Minimum) || this.ContainsValue(Range.Maximum) || Range.ContainsValue(this.Maximum) || Range.ContainsValue(this.Maximum));

        public Boolean IsBorderSequence(Range<T> Range) => this.IsValid() && Range.IsValid() && (this.Maximum.Equals(Range.Minimum) || this.Minimum.Equals(Range.Maximum));
    }

    public static class Vector3Extensions
    {
        public static float GetAngleXZ(this Vector3 dir)
        {
            return Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        }
        public static float SqrDistance(this Vector3 a, Vector3 b)
        {
            Vector3 vector = new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
            return (vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }
    }

    public static class Vector2Extensions
    {
        public static float GetAngleToPoint(this Vector2 from, Vector2 to)
        {
            float ca = to.x - from.x;
            float co = -to.y + from.y;
            //KlyteUtils.doLog($"ca = {ca},co = {co};");
            if (co == 0)
            {
                if (ca < 0)
                {
                    return 270;
                }
                else
                {
                    return 90;
                }
            }
            if (co < 0)
            {
                return (360 - ((Mathf.Atan(ca / co) * Mathf.Rad2Deg + 360) % 360) % 360);
            }
            else
            {
                return 360 - ((Mathf.Atan(ca / co) * Mathf.Rad2Deg + 180 + 360) % 360);
            }
        }
    }

    public static class Int32Extensions
    {
        public static int ParseOrDefault(string val, int defaultVal)
        {
            try
            {
                return int.Parse(val);
            }
            catch
            {
                return defaultVal;
            }
        }
    }
    public static class ColorExtensions
    {
        public static string ToRGBA(this Color32 color)
        {
            return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");
        }
        public static string ToRGB(this Color32 color)
        {
            return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        }
        public static Color32 FromRGBA(string rgba)
        {
            var value = Convert.ToInt64(rgba, 16);
            return new Color32((byte)((value & 0xFF000000) >> 24), (byte)((value & 0xFF0000) >> 16), (byte)((value & 0xFF00) >> 8), (byte)((value & 0xFF)));
        }
        public static Color32 FromRGB(string rgb)
        {
            var value = Convert.ToInt32(rgb, 16);
            return new Color32((byte)((value & 0xFF0000) >> 16), (byte)((value & 0xFF00) >> 8), (byte)((value & 0xFF)), 0xFF);
        }
    }

    public static class NetSegmentExtensions
    {
        public static Bezier3 GetBezier(this NetSegment segment)
        {
            Bezier3 bezier = default(Bezier3);
            bezier.a = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)segment.m_startNode].m_position;
            bezier.d = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)segment.m_endNode].m_position;
            bool smoothStart = (Singleton<NetManager>.instance.m_nodes.m_buffer[(int)segment.m_startNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            bool smoothEnd = (Singleton<NetManager>.instance.m_nodes.m_buffer[(int)segment.m_endNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            NetSegment.CalculateMiddlePoints(bezier.a, segment.m_startDirection, bezier.d, segment.m_endDirection, smoothStart, smoothEnd, out bezier.b, out bezier.c);
            return bezier;
        }
    }

    public class Tuple<T1, T2, T3, T4>
    {
        public T1 First { get; private set; }
        public T2 Second { get; private set; }
        public T3 Third { get; private set; }
        public T4 Fourth { get; private set; }
        internal Tuple(T1 first, T2 second, T3 third, T4 fourth)
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
        }
    }

    public class Tuple<T1, T2, T3>
    {
        public T1 First { get; private set; }
        public T2 Second { get; private set; }
        public T3 Third { get; private set; }
        internal Tuple(T1 first, T2 second, T3 third)
        {
            First = first;
            Second = second;
            Third = third;
        }
    }

    public class Tuple<T1, T2>
    {
        public T1 First { get; private set; }
        public T2 Second { get; private set; }
        internal Tuple(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }
    }

    public static class Tuple
    {
        public static Tuple<T1, T2, T3, T4> New<T1, T2, T3, T4>(T1 first, T2 second, T3 third, T4 fourth)
        {
            var tuple = new Tuple<T1, T2, T3, T4>(first, second, third, fourth);
            return tuple;
        }
        public static Tuple<T1, T2, T3> New<T1, T2, T3>(T1 first, T2 second, T3 third)
        {
            var tuple = new Tuple<T1, T2, T3>(first, second, third);
            return tuple;
        }
        public static Tuple<T1, T2> New<T1, T2>(T1 first, T2 second)
        {
            var tuple = new Tuple<T1, T2>(first, second);
            return tuple;
        }
    }
}
