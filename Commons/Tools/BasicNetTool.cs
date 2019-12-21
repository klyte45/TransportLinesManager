using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using Klyte.Commons.Utils;
using System;
using System.Diagnostics;
using UnityEngine;

namespace Klyte.Commons
{

    public abstract class BasicNetTool<T> : TransportTool where T : BasicNetTool<T>
    {

        protected override void Awake()
        {
            this.m_toolController = UnityEngine.Object.FindObjectOfType<ToolController>();
            base.enabled = false;
            instance = (T)this;
        }

        protected override void OnToolGUI(Event e)
        {
            if (UIView.HasModalInput() || UIView.HasInputFocus())
            {
                return;
            }
        }

        protected override void OnEnable()
        {
            InfoManager.InfoMode currentMode = Singleton<InfoManager>.instance.CurrentMode;
            InfoManager.SubInfoMode currentSubMode = Singleton<InfoManager>.instance.CurrentSubMode;
            this.m_prevRenderZones = Singleton<TerrainManager>.instance.RenderZones;
            this.m_prevTool = this.m_toolController.CurrentTool;
            this.m_toolController.CurrentTool = this;
            Singleton<InfoManager>.instance.SetCurrentMode(currentMode, currentSubMode);
            Singleton<TerrainManager>.instance.RenderZones = false;
        }

        protected override void OnDisable()
        {
            Singleton<TerrainManager>.instance.RenderZones = m_prevRenderZones;
        }


        protected override void OnToolUpdate()
        {
            bool isInsideUI = this.m_toolController.IsInsideUI;
            if (this.m_leftClickTime == 0L && Input.GetMouseButton(0) && !isInsideUI)
            {
                this.m_leftClickTime = Stopwatch.GetTimestamp();
                this.OnLeftMouseDown();
            }
            if (this.m_leftClickTime != 0L)
            {
                long num = this.ElapsedMilliseconds(this.m_leftClickTime);
                if (!Input.GetMouseButton(0))
                {
                    this.m_leftClickTime = 0L;
                    if (num < 200L)
                    {
                        this.OnLeftClick();
                    }
                    else
                    {
                        this.OnLeftDragStop();
                    }
                    this.OnLeftMouseUp();
                }
                else if (num >= 200L)
                {
                    this.OnLeftDrag();
                }
            }
            if (this.m_rightClickTime == 0L && Input.GetMouseButton(1) && !isInsideUI)
            {
                this.m_rightClickTime = Stopwatch.GetTimestamp();
                this.OnRightMouseDown();
            }
            if (this.m_rightClickTime != 0L)
            {
                long num2 = this.ElapsedMilliseconds(this.m_rightClickTime);
                if (!Input.GetMouseButton(1))
                {
                    this.m_rightClickTime = 0L;
                    if (num2 < 200L)
                    {
                        this.OnRightClick();
                    }
                    else
                    {
                        this.OnRightDragStop();
                    }
                    this.OnRightMouseUp();
                }
                else if (num2 >= 200L)
                {
                    this.OnRightDrag();
                }
            }
            if (!isInsideUI && Cursor.visible)
            {
                Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                this.m_hoverSegment = 0;
                this.RaycastHoverInstance(mouseRay);
            }
        }

        protected virtual void OnRightDrag() { }
        protected virtual void OnRightMouseUp() { }
        protected virtual void OnRightDragStop() { }
        protected virtual void OnRightClick() { }
        protected virtual void OnRightMouseDown() { }
        protected virtual void OnLeftDrag() { }
        protected virtual void OnLeftMouseUp() { }
        protected virtual void OnLeftDragStop() { }
        protected virtual void OnLeftClick() { }
        protected virtual void OnLeftMouseDown() { }

        protected override void OnToolLateUpdate() { }

        public override void SimulationStep() { }

        public override ToolBase.ToolErrors GetErrors()
        {
            return ToolBase.ToolErrors.None;
        }




        private ItemClass.Layer GetItemLayers()
        {
            ItemClass.Layer layer = ItemClass.Layer.Default;
            if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.Water)
            {
                layer |= ItemClass.Layer.WaterPipes;
            }
            if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.Underground || Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.Traffic || Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.Transport)
            {
                layer |= ItemClass.Layer.MetroTunnels;
            }
            return layer;
        }

        private void RaycastHoverInstance(Ray mouseRay)
        {
            ToolBase.RaycastInput input = new ToolBase.RaycastInput(mouseRay, Camera.main.farClipPlane);
            input.m_netService.m_itemLayers = this.GetItemLayers();
            input.m_ignoreTerrain = true;
            input.m_ignoreSegmentFlags = NetSegment.Flags.None;
            this.m_hoverSegment = 0;
            Vector3 origin = input.m_ray.origin;
            Vector3 normalized = input.m_ray.direction.normalized;
            Vector3 vector = input.m_ray.origin + normalized * input.m_length;
            ToolBase.RaycastOutput output;
            output.m_hitPos = vector;
            output.m_overlayButtonIndex = 0;
            output.m_netNode = 0;
            output.m_netSegment = 0;
            output.m_building = 0;
            output.m_propInstance = 0;
            output.m_treeInstance = 0u;
            output.m_vehicle = 0;
            output.m_parkedVehicle = 0;
            output.m_citizenInstance = 0;
            output.m_transportLine = 0;
            output.m_transportStopIndex = 0;
            output.m_transportSegmentIndex = 0;
            output.m_district = 0;
            output.m_park = 0;
            output.m_disaster = 0;
            output.m_currentEditObject = false;
            Segment3 ray = new Segment3(origin, vector);

            RayCastSegmentAndNode(input.m_buildObject as NetInfo, ray, input.m_netSnap, input.m_segmentNameOnly, input.m_netService.m_service, input.m_netService2.m_service, input.m_netService.m_subService, input.m_netService2.m_subService, input.m_netService.m_itemLayers, input.m_netService2.m_itemLayers, input.m_ignoreNodeFlags, input.m_ignoreSegmentFlags, out Vector3 vector2, out output.m_netNode, out output.m_netSegment);

            this.m_hoverSegment = output.m_netSegment;


            if (m_hoverSegment == 0)
            {

            }
        }

        private Vector3 RaycastMouseLocation(Ray mouseRay)
        {
            ToolBase.RayCast(new ToolBase.RaycastInput(mouseRay, Camera.main.farClipPlane)
            {
                m_ignoreTerrain = false
            }, out RaycastOutput raycastOutput);
            return raycastOutput.m_hitPos;
        }


        private long ElapsedMilliseconds(long startTime)
        {
            long timestamp = Stopwatch.GetTimestamp();
            long num;
            if (timestamp > startTime)
            {
                num = timestamp - startTime;
            }
            else
            {
                num = startTime - timestamp;
            }
            return num / (Stopwatch.Frequency / 1000L);
        }

        protected static NetSegment[] segmentBuffer => Singleton<NetManager>.instance.m_segments.m_buffer;
        protected static NetNode[] nodeBuffer => Singleton<NetManager>.instance.m_nodes.m_buffer;

        public void RenderOverlay(RenderManager.CameraInfo cameraInfo, Color toolColor, Color despawnColor, ushort netSegment)
        {
            if (netSegment == 0)
            {
                return;
            }
            NetInfo info = segmentBuffer[(int)netSegment].Info;
            ushort startNode = segmentBuffer[(int)netSegment].m_startNode;
            ushort endNode = segmentBuffer[(int)netSegment].m_endNode;
            bool smoothStart = (nodeBuffer[(int)startNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            bool smoothEnd = (nodeBuffer[(int)endNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            Bezier3 bezier;
            bezier.a = nodeBuffer[(int)startNode].m_position;
            bezier.d = nodeBuffer[(int)endNode].m_position;
            NetSegment.CalculateMiddlePoints(bezier.a, segmentBuffer[(int)netSegment].m_startDirection, bezier.d, segmentBuffer[(int)netSegment].m_endDirection, smoothStart, smoothEnd, out bezier.b, out bezier.c);
            Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(cameraInfo, toolColor, bezier, info.m_halfWidth * 4f / 3f, 100000f, -100000f, -1f, 1280f, false, true);
            Segment3 segment;
            segment.a = nodeBuffer[(int)startNode].m_position;
            Segment3 segment2;
            segment2.a = nodeBuffer[(int)endNode].m_position;
            segment.b = this.GetControlPoint(netSegment);
            segment2.b = segment.b;
            toolColor.a /= 2f;
            Singleton<RenderManager>.instance.OverlayEffect.DrawSegment(cameraInfo, toolColor, segment, segment2, 0f, 10f, -1f, 1280f, false, true);
            Singleton<RenderManager>.instance.OverlayEffect.DrawCircle(cameraInfo, toolColor, segment.b, info.m_halfWidth / 2f, -1f, 1280f, false, true);
        }

        private Vector3 GetControlPoint(ushort segment)
        {
            Vector3 position = nodeBuffer[(int)segmentBuffer[(int)segment].m_startNode].m_position;
            Vector3 startDirection = segmentBuffer[(int)segment].m_startDirection;
            Vector3 position2 = nodeBuffer[(int)segmentBuffer[(int)segment].m_endNode].m_position;
            Vector3 endDirection = segmentBuffer[(int)segment].m_endDirection;
            if (!NetSegment.IsStraight(position, startDirection, position2, endDirection, out float num))
            {
                float num2 = startDirection.x * endDirection.x + startDirection.z * endDirection.z;
                if (num2 >= -0.999f && Line2.Intersect(VectorUtils.XZ(position), VectorUtils.XZ(position + startDirection), VectorUtils.XZ(position2), VectorUtils.XZ(position2 + endDirection), out float d, out float num3))
                {
                    return position + startDirection * d;
                }
                KlyteUtils.doErrorLog("Warning! Invalid segment directions!");
            }
            return (position + position2) / 2f;
        }


        public static T instance;


        protected static Color m_hoverColor = new Color32(47, byte.MaxValue, 47, byte.MaxValue);

        protected static Color m_removeColor = new Color32(byte.MaxValue, 47, 47, 191);
        protected static Color m_despawnColor = new Color32(byte.MaxValue, 160, 47, 191);

        public static Shader shaderBlend = Shader.Find("Custom/Props/Decal/Blend");

        public static Shader shaderSolid = Shader.Find("Custom/Props/Decal/Solid");

        protected ushort m_hoverSegment;

        private bool m_prevRenderZones;

        private ToolBase m_prevTool;

        private long m_rightClickTime;

        private long m_leftClickTime;


        private bool RayCastSegmentAndNode(NetInfo connectedType, Segment3 ray, float snapElevation, bool nameOnly, ItemClass.Service service, ItemClass.Service service2, ItemClass.SubService subService, ItemClass.SubService subService2, ItemClass.Layer itemLayers, ItemClass.Layer itemLayers2, NetNode.Flags ignoreNodeFlags, NetSegment.Flags ignoreSegmentFlags, out Vector3 hit, out ushort nodeIndex, out ushort segmentIndex)
        {
            Bounds bounds = new Bounds(new Vector3(0f, 512f, 0f), new Vector3(17280f, 1152f, 17280f));
            if (ray.Clip(bounds))
            {
                Vector3 vector = ray.b - ray.a;
                int num = (int)(ray.a.x / 64f + 135f);
                int num2 = (int)(ray.a.z / 64f + 135f);
                int num3 = (int)(ray.b.x / 64f + 135f);
                int num4 = (int)(ray.b.z / 64f + 135f);
                float num5 = Mathf.Abs(vector.x);
                float num6 = Mathf.Abs(vector.z);
                int num7;
                int num8;
                if (num5 >= num6)
                {
                    num7 = ((vector.x <= 0f) ? -1 : 1);
                    num8 = 0;
                    if (num5 > 0.001f)
                    {
                        vector *= 64f / num5;
                    }
                }
                else
                {
                    num7 = 0;
                    num8 = ((vector.z <= 0f) ? -1 : 1);
                    if (num6 > 0.001f)
                    {
                        vector *= 64f / num6;
                    }
                }
                float num9 = 2f;
                float num10 = 16f;
                float num11 = 2f;
                float num12 = 16f;
                ushort num13 = 0;
                ushort num14 = 0;
                ushort num15 = 0;
                Vector3 vector2 = ray.a;
                Vector3 vector3 = ray.a;
                int num16 = num;
                int num17 = num2;
                do
                {
                    Vector3 vector4 = vector3 + vector;
                    int num18;
                    int num19;
                    int num20;
                    int num21;
                    if (num7 != 0)
                    {
                        if ((num16 == num && num7 > 0) || (num16 == num3 && num7 < 0))
                        {
                            num18 = Mathf.Max((int)((vector4.x - 64f) / 64f + 135f), 0);
                        }
                        else
                        {
                            num18 = Mathf.Max(num16, 0);
                        }
                        if ((num16 == num && num7 < 0) || (num16 == num3 && num7 > 0))
                        {
                            num19 = Mathf.Min((int)((vector4.x + 64f) / 64f + 135f), 269);
                        }
                        else
                        {
                            num19 = Mathf.Min(num16, 269);
                        }
                        num20 = Mathf.Max((int)((Mathf.Min(vector2.z, vector4.z) - 64f) / 64f + 135f), 0);
                        num21 = Mathf.Min((int)((Mathf.Max(vector2.z, vector4.z) + 64f) / 64f + 135f), 269);
                    }
                    else
                    {
                        if ((num17 == num2 && num8 > 0) || (num17 == num4 && num8 < 0))
                        {
                            num20 = Mathf.Max((int)((vector4.z - 64f) / 64f + 135f), 0);
                        }
                        else
                        {
                            num20 = Mathf.Max(num17, 0);
                        }
                        if ((num17 == num2 && num8 < 0) || (num17 == num4 && num8 > 0))
                        {
                            num21 = Mathf.Min((int)((vector4.z + 64f) / 64f + 135f), 269);
                        }
                        else
                        {
                            num21 = Mathf.Min(num17, 269);
                        }
                        num18 = Mathf.Max((int)((Mathf.Min(vector2.x, vector4.x) - 64f) / 64f + 135f), 0);
                        num19 = Mathf.Min((int)((Mathf.Max(vector2.x, vector4.x) + 64f) / 64f + 135f), 269);
                    }
                    for (int i = num20; i <= num21; i++)
                    {
                        for (int j = num18; j <= num19; j++)
                        {
                            ushort num22 = Singleton<NetManager>.instance.m_nodeGrid[i * 270 + j];
                            int num23 = 0;
                            while (num22 != 0)
                            {
                                NetNode.Flags flags = nodeBuffer[(int)num22].m_flags;
                                NetInfo info = nodeBuffer[(int)num22].Info;
                                ItemClass connectionClass = info.GetConnectionClass();
                                if ((((service == ItemClass.Service.None || connectionClass.m_service == service) && (subService == ItemClass.SubService.None || connectionClass.m_subService == subService) && (itemLayers == ItemClass.Layer.None || (connectionClass.m_layer & itemLayers) != ItemClass.Layer.None)) || (info.m_intersectClass != null && (service == ItemClass.Service.None || info.m_intersectClass.m_service == service) && (subService == ItemClass.SubService.None || info.m_intersectClass.m_subService == subService) && (itemLayers == ItemClass.Layer.None || (info.m_intersectClass.m_layer & itemLayers) != ItemClass.Layer.None)) || (info.m_netAI.CanIntersect(connectedType) && connectionClass.m_service == service2 && (subService2 == ItemClass.SubService.None || connectionClass.m_subService == subService2) && (itemLayers2 == ItemClass.Layer.None || (connectionClass.m_layer & itemLayers2) != ItemClass.Layer.None))) && (flags & ignoreNodeFlags) == NetNode.Flags.None && (connectedType == null || (info.m_netAI.CanConnect(connectedType) && connectedType.m_netAI.CanConnect(info))))
                                {
                                    bool flag = false;
                                    if ((flags & (NetNode.Flags.Middle | NetNode.Flags.Untouchable)) == (NetNode.Flags.Middle | NetNode.Flags.Untouchable) && nodeBuffer[(int)num22].CountSegments(NetSegment.Flags.Untouchable, 0) >= 2)
                                    {
                                        flag = true;
                                    }
                                    if (!flag && nodeBuffer[(int)num22].RayCast(ray, snapElevation, out float num24, out float num25) && (num25 < num12 || (num25 == num12 && num24 < num11)))
                                    {
                                        num11 = num24;
                                        num12 = num25;
                                        num14 = num22;
                                    }
                                }
                                num22 = nodeBuffer[(int)num22].m_nextGridNode;
                                if (++num23 > 32768)
                                {
                                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                    break;
                                }
                            }
                        }
                    }
                    for (int k = num20; k <= num21; k++)
                    {
                        for (int l = num18; l <= num19; l++)
                        {
                            ushort num26 = Singleton<NetManager>.instance.m_segmentGrid[k * 270 + l];
                            int num27 = 0;
                            while (num26 != 0)
                            {
                                NetSegment.Flags flags2 = segmentBuffer[(int)num26].m_flags;
                                NetInfo info2 = segmentBuffer[(int)num26].Info;
                                ItemClass connectionClass2 = info2.GetConnectionClass();
                                if (((service == ItemClass.Service.None || connectionClass2.m_service == service) && (subService == ItemClass.SubService.None || connectionClass2.m_subService == subService) && (itemLayers == ItemClass.Layer.None || (connectionClass2.m_layer & itemLayers) != ItemClass.Layer.None || nameOnly)) || (info2.m_intersectClass != null && (service == ItemClass.Service.None || info2.m_intersectClass.m_service == service) && (subService == ItemClass.SubService.None || info2.m_intersectClass.m_subService == subService) && (itemLayers == ItemClass.Layer.None || (info2.m_intersectClass.m_layer & itemLayers) != ItemClass.Layer.None || nameOnly)) || (info2.m_netAI.CanIntersect(connectedType) && connectionClass2.m_service == service2 && (subService2 == ItemClass.SubService.None || connectionClass2.m_subService == subService2) && (itemLayers2 == ItemClass.Layer.None || (connectionClass2.m_layer & itemLayers2) != ItemClass.Layer.None || nameOnly)))
                                {
                                    bool flag2 = (flags2 & ignoreSegmentFlags) != NetSegment.Flags.None && !nameOnly;
                                    if ((flags2 & ignoreSegmentFlags) == NetSegment.Flags.None && (connectedType == null || (info2.m_netAI.CanConnect(connectedType) && connectedType.m_netAI.CanConnect(info2))) && segmentBuffer[(int)num26].RayCast(num26, ray, snapElevation, nameOnly, out float num28, out float num29) && (num29 < num10 || (num29 == num10 && num28 < num9)))
                                    {
                                        ushort startNode = segmentBuffer[(int)num26].m_startNode;
                                        ushort endNode = segmentBuffer[(int)num26].m_endNode;
                                        Vector3 position = nodeBuffer[(int)startNode].m_position;
                                        Vector3 position2 = nodeBuffer[(int)endNode].m_position;
                                        float num30 = nodeBuffer[(int)startNode].Info.GetMinNodeDistance();
                                        float num31 = nodeBuffer[(int)endNode].Info.GetMinNodeDistance();
                                        num10 = num29;
                                        num9 = num28;
                                        Vector3 a = ray.a + (ray.b - ray.a) * num28;
                                        NetNode.Flags flags3 = nodeBuffer[(int)startNode].m_flags;
                                        if ((flags3 & NetNode.Flags.End) != NetNode.Flags.None)
                                        {
                                            flags3 &= ~NetNode.Flags.Moveable;
                                        }
                                        NetNode.Flags flags4 = nodeBuffer[(int)endNode].m_flags;
                                        if ((flags4 & NetNode.Flags.End) != NetNode.Flags.None)
                                        {
                                            flags4 &= ~NetNode.Flags.Moveable;
                                        }
                                        if (flag2)
                                        {
                                            num30 = 1000f;
                                            num31 = 1000f;
                                        }
                                        bool flag3 = (flags3 & (NetNode.Flags.Moveable | ignoreNodeFlags)) == NetNode.Flags.None;
                                        bool flag4 = (flags4 & (NetNode.Flags.Moveable | ignoreNodeFlags)) == NetNode.Flags.None;
                                        float num32 = VectorUtils.LengthSqrXZ(a - position) / (num30 * num30);
                                        float num33 = VectorUtils.LengthSqrXZ(a - position2) / (num31 * num31);
                                        if (flag3 && num32 < 1f && (!flag4 || num32 < num33) && !nameOnly)
                                        {
                                            num13 = startNode;
                                            if (!flag2)
                                            {
                                                num15 = num26;
                                            }
                                        }
                                        else if (flag4 && num33 < 1f && !nameOnly)
                                        {
                                            num13 = endNode;
                                            if (!flag2)
                                            {
                                                num15 = num26;
                                            }
                                        }
                                        else if (!flag2)
                                        {
                                            num13 = 0;
                                            num15 = num26;
                                        }
                                    }
                                }
                                num26 = segmentBuffer[(int)num26].m_nextGridSegment;
                                if (++num27 > 36864)
                                {
                                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                    break;
                                }
                            }
                        }
                    }
                    vector2 = vector3;
                    vector3 = vector4;
                    num16 += num7;
                    num17 += num8;
                }
                while ((num16 <= num3 || num7 <= 0) && (num16 >= num3 || num7 >= 0) && (num17 <= num4 || num8 <= 0) && (num17 >= num4 || num8 >= 0));
                if (num12 < num10 || (num12 == num10 && num11 < num9))
                {
                    num9 = num11;
                    num13 = num14;
                }
                if (num9 != 2f)
                {
                    hit = ray.Position(num9);
                    nodeIndex = num13;
                    segmentIndex = num15;
                    return true;
                }
            }
            hit = Vector3.zero;
            nodeIndex = 0;
            segmentIndex = 0;
            return false;
        }

    }

}
