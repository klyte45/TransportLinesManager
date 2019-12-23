using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using System.Diagnostics;
using UnityEngine;

namespace Klyte.Commons
{

    public abstract class BasicBuildingTool<T> : BuildingTool where T : BasicBuildingTool<T>
    {

        protected override void Awake()
        {
            m_toolController = UnityEngine.Object.FindObjectOfType<ToolController>();
            base.enabled = false;
            instance = (T) this;
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
            m_prevRenderZones = Singleton<TerrainManager>.instance.RenderZones;
            m_toolController.CurrentTool = this;
            Singleton<InfoManager>.instance.SetCurrentMode(currentMode, currentSubMode);
            Singleton<TerrainManager>.instance.RenderZones = false;
        }

        protected override void OnDisable() => Singleton<TerrainManager>.instance.RenderZones = m_prevRenderZones;


        protected override void OnToolUpdate()
        {
            var isInsideUI = m_toolController.IsInsideUI;
            if (m_leftClickTime == 0L && Input.GetMouseButton(0) && !isInsideUI)
            {
                m_leftClickTime = Stopwatch.GetTimestamp();
                OnLeftMouseDown();
            }
            if (m_leftClickTime != 0L)
            {
                var num = ElapsedMilliseconds(m_leftClickTime);
                if (!Input.GetMouseButton(0))
                {
                    m_leftClickTime = 0L;
                    if (num < 200L)
                    {
                        OnLeftClick();
                    }
                    else
                    {
                        OnLeftDragStop();
                    }
                    OnLeftMouseUp();
                }
                else if (num >= 200L)
                {
                    OnLeftDrag();
                }
            }
            if (m_rightClickTime == 0L && Input.GetMouseButton(1) && !isInsideUI)
            {
                m_rightClickTime = Stopwatch.GetTimestamp();
                OnRightMouseDown();
            }
            if (m_rightClickTime != 0L)
            {
                var num2 = ElapsedMilliseconds(m_rightClickTime);
                if (!Input.GetMouseButton(1))
                {
                    m_rightClickTime = 0L;
                    if (num2 < 200L)
                    {
                        OnRightClick();
                    }
                    else
                    {
                        OnRightDragStop();
                    }
                    OnRightMouseUp();
                }
                else if (num2 >= 200L)
                {
                    OnRightDrag();
                }
            }
            if (!isInsideUI && Cursor.visible)
            {
                Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                m_hoverBuilding = 0;
                RaycastHoverInstance(mouseRay);
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

        public override ToolBase.ToolErrors GetErrors() => ToolBase.ToolErrors.None;



        private void RaycastHoverInstance(Ray mouseRay)
        {
            var input = new ToolBase.RaycastInput(mouseRay, Camera.main.farClipPlane);
            Vector3 origin = input.m_ray.origin;
            Vector3 normalized = input.m_ray.direction.normalized;
            Vector3 vector = input.m_ray.origin + (normalized * input.m_length);
            var ray = new Segment3(origin, vector);

            BuildingManager.instance.RayCast(ray, ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default, Building.Flags.None, out _, out m_hoverBuilding);
            var i = 0;
            while (BuildingBuffer[m_hoverBuilding].m_parentBuilding != 0 && i < 10)
            {
                m_hoverBuilding = BuildingBuffer[m_hoverBuilding].m_parentBuilding;
                i++;
            }

        }
        public void RenderOverlay(RenderManager.CameraInfo cameraInfo, Color toolColor, ushort buildingId)
        {
            if (buildingId == 0)
            {
                return;
            }
            BuildingBuffer[buildingId].GetTotalPosition(out Vector3 pos, out Quaternion rot, out Vector3 size);
            var quad = new Quad3(
               (rot * new Vector3(-size.x, 0, size.z) / 2) + pos,
               (rot * new Vector3(-size.x, 0, -size.z) / 2) + pos,
               (rot * new Vector3(size.x, 0, -size.z) / 2) + pos,
               (rot * new Vector3(size.x, 0, size.z) / 2) + pos

            );
            Singleton<RenderManager>.instance.OverlayEffect.DrawQuad(cameraInfo, toolColor, quad, -1f, 1280f, false, true);

        }

        private long ElapsedMilliseconds(long startTime)
        {
            var timestamp = Stopwatch.GetTimestamp();
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

        protected static Building[] BuildingBuffer => Singleton<BuildingManager>.instance.m_buildings.m_buffer;


        public static T instance;


        protected static Color m_hoverColor = new Color32(47, byte.MaxValue, 47, byte.MaxValue);

        protected static Color m_removeColor = new Color32(byte.MaxValue, 47, 47, 191);
        protected static Color m_despawnColor = new Color32(byte.MaxValue, 160, 47, 191);

        public static Shader shaderBlend = Shader.Find("Custom/Props/Decal/Blend");

        public static Shader shaderSolid = Shader.Find("Custom/Props/Decal/Solid");

        protected ushort m_hoverBuilding;

        private bool m_prevRenderZones;

        private long m_rightClickTime;

        private long m_leftClickTime;



    }

}
