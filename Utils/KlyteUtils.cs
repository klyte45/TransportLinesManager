using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using Klyte.Extensions;
using Klyte.TransportLinesManager.Extensors.TransportTypeExt;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Klyte.TransportLinesManager.Utils
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
        #endregion

        #region UI utils
        public static void createElement<T>(out T uiItem, Transform parent) where T : MonoBehaviour
        {
            GameObject container = new GameObject();
            container.transform.parent = parent;
            uiItem = (T)container.AddComponent(typeof(T));
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
            if (allowedTypes == null || allowedTypes.Length == 0)
            {
                return bm.FindBuilding(pos, maxDistance, service, subService, flagsRequired, flagsForbidden);
            }
            int num = Mathf.Max((int)((pos.x - maxDistance) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((pos.z - maxDistance) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((pos.x + maxDistance) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((pos.z + maxDistance) / 64f + 135f), 269);
            ushort result = 0;
            float num5 = maxDistance * maxDistance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num6 = bm.m_buildingGrid[i * 270 + j];
                    int num7 = 0;
                    while (num6 != 0)
                    {
                        BuildingInfo info = bm.m_buildings.m_buffer[(int)num6].Info;
                        if ((info.m_class.m_service == service || service == ItemClass.Service.None) && (info.m_class.m_subService == subService || subService == ItemClass.SubService.None))
                        {
                            Building.Flags flags = bm.m_buildings.m_buffer[(int)num6].m_flags;
                            if ((flags & (flagsRequired | flagsForbidden)) == flagsRequired)
                            {
                                if (info.GetAI() is DepotAI depotAI && allowedTypes.Contains(depotAI.m_transportInfo.m_vehicleReason))
                                {
                                    float num8 = Vector3.SqrMagnitude(pos - bm.m_buildings.m_buffer[(int)num6].m_position);
                                    if (num8 < num5)
                                    {
                                        result = num6;
                                        num5 = num8;
                                    }
                                }
                            }
                        }
                        num6 = bm.m_buildings.m_buffer[(int)num6].m_nextGridBuilding;
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

        public static ushort GetBuildingDistrict(uint bId)
        {
            return GetBuildingDistrict(Singleton<BuildingManager>.instance.m_buildings.m_buffer[bId]);
        }
        public static ushort GetBuildingDistrict(Building b)
        {
            return GetDistrict(b.m_position);
        }
        public static ushort GetDistrict(Vector3 location)
        {
            return Singleton<DistrictManager>.instance.GetDistrict(location);
        }
        #endregion

        #region Road Name Utils
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
            //			TLMUtils.doLog("middle="+middle);
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
        public static T GetPrivateField<T>(object o, string fieldName)
        {
            var fields = o.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo field = null;

            foreach (var f in fields)
            {
                if (f.Name == fieldName)
                {
                    field = f;
                    break;
                }
            }
            if (field != null)
            {
                return (T)field.GetValue(o);
            }
            else
            {
                return default(T);
            }
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

                TLMUtils.doErrorLog("LOCALE DUMP FAIL: {0}", e.ToString());
            }
            Debug.LogWarning(localeDump);
        }

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
        public static VehicleInfo GetRandomModel(List<string> assetList)
        {
            if (assetList.Count == 0) return null;
            Randomizer r = new Randomizer(new System.Random().Next());

            string model = assetList[r.Int32(0, assetList.Count - 1)];

            var saida = PrefabCollection<VehicleInfo>.FindLoaded(model);
            if (saida == null)
            {
                TLMUtils.doLog("MODEL DOESN'T EXIST!");
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
                TLMUtils.doLog("ERRO AO OBTER CAPACIDADE: [{0}] {1}", info, e.Message);
            }
            return capacity;
        }

        protected static bool IsTrailer(PrefabInfo prefab)
        {
            string @unchecked = Locale.GetUnchecked("VEHICLE_TITLE", prefab.name);
            return @unchecked.StartsWith("VEHICLE_TITLE") || @unchecked.StartsWith("Trailer");
        }
        private static UIColorField s_colorFieldTemplate;
        internal static UIColorField CreatColorField(UIComponent parent)
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
            UIColorField component = UnityEngine.Object.Instantiate<GameObject>(s_colorFieldTemplate.gameObject).GetComponent<UIColorField>();
            parent.AttachUIComponent(component.gameObject);
            component.size = new Vector2(40f, 26f);
            component.pickerPosition = UIColorField.ColorPickerPosition.LeftAbove;
            return component;
        }
        #endregion
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
        public override string ToString() { return String.Format("[{0} - {1}]", Minimum, Maximum); }

        /// <summary>
        /// Determines if the range is valid
        /// </summary>
        /// <returns>True if range is valid, else false</returns>
        public Boolean IsValid() { return Minimum.CompareTo(Maximum) <= 0; }

        /// <summary>
        /// Determines if the provided value is inside the range
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns>True if the value is inside Range, else false</returns>
        public Boolean ContainsValue(T value)
        {
            return (Minimum.CompareTo(value) <= 0) && (value.CompareTo(Maximum) <= 0);
        }


        /// <summary>
        /// Determines if the provided value is inside the range
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns>True if the value is inside Range, else false</returns>
        public Boolean IsBetweenLimits(T value)
        {
            return (Minimum.CompareTo(value) < 0) && (value.CompareTo(Maximum) < 0);
        }

        /// <summary>
        /// Determines if this Range is inside the bounds of another range
        /// </summary>
        /// <param name="Range">The parent range to test on</param>
        /// <returns>True if range is inclusive, else false</returns>
        public Boolean IsInsideRange(Range<T> Range)
        {
            return this.IsValid() && Range.IsValid() && Range.ContainsValue(this.Minimum) && Range.ContainsValue(this.Maximum);
        }



        /// <summary>
        /// Determines if another range is inside the bounds of this range
        /// </summary>
        /// <param name="Range">The child range to test</param>
        /// <returns>True if range is inside, else false</returns>
        public Boolean ContainsRange(Range<T> Range)
        {
            return this.IsValid() && Range.IsValid() && this.ContainsValue(Range.Minimum) && this.ContainsValue(Range.Maximum);
        }

        /// <summary>
        /// Determines if another range intersect this range
        /// </summary>
        /// <param name="Range">The child range to test</param>
        /// <returns>True if range is inside, else false</returns>
        public Boolean IntersectRange(Range<T> Range)
        {
            return this.IsValid() && Range.IsValid() && (this.ContainsValue(Range.Minimum) || this.ContainsValue(Range.Maximum) || Range.ContainsValue(this.Maximum) || Range.ContainsValue(this.Maximum));
        }

        public Boolean IsBorderSequence(Range<T> Range)
        {
            return this.IsValid() && Range.IsValid() && (this.Maximum.Equals(Range.Minimum) || this.Minimum.Equals(Range.Maximum));
        }
    }

    public static class Vector2Extensions
    {
        public static float GetAngleToPoint(this Vector2 from, Vector2 to)
        {
            float ca = to.x - from.x;
            float co = to.y - from.y;
            if (co == 0)
            {
                return ca > 0 ? 0 : 180;
            }
            else if (ca < 0)
            {
                return Mathf.Atan(co / ca) * Mathf.Rad2Deg + 180;
            }
            else
            {
                return Mathf.Atan(co / ca) * Mathf.Rad2Deg;
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
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                return defaultVal;
            }
        }
    }
}
