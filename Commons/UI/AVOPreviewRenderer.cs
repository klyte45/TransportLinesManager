using ColossalFramework;
using System;
using UnityEngine;

namespace Klyte.Commons.UI
{
    public class AVOPreviewRenderer : MonoBehaviour
    {
        private readonly Camera m_camera;

        private float m_rotation = 120f;

        private float m_zoom = 3f;

        public Vector2 Size
        {
            get {
                return new Vector2((float)this.m_camera.targetTexture.width, (float)this.m_camera.targetTexture.height);
            }
            set {
                if (this.Size != value)
                {
                    this.m_camera.targetTexture = new RenderTexture((int)value.x, (int)value.y, 24, RenderTextureFormat.ARGB32);
                    this.m_camera.pixelRect = new Rect(0f, 0f, value.x, value.y);
                }
            }
        }

        public RenderTexture Texture
        {
            get {
                return this.m_camera.targetTexture;
            }
        }

        public float CameraRotation
        {
            get {
                return this.m_rotation;
            }
            set {
                this.m_rotation = value % 360f;
            }
        }

        public float Zoom
        {
            get {
                return this.m_zoom;
            }
            set {
                this.m_zoom = Mathf.Clamp(value, 0.5f, 5f);
            }
        }

        public AVOPreviewRenderer()
        {
            m_camera = new GameObject("Camera").AddComponent<Camera>();
            m_camera.transform.SetParent(base.transform);
            m_camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            m_camera.fieldOfView = 30f;
            m_camera.nearClipPlane = 1f;
            m_camera.farClipPlane = 1000f;
            m_camera.allowHDR = true;
            m_camera.enabled = false;
            m_camera.targetTexture = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
            m_camera.pixelRect = new Rect(0f, 0f, 512f, 512f);
            m_camera.clearFlags = CameraClearFlags.Color;
            m_camera.name = "TLMCamera";
        }

        public void RenderVehicle(VehicleInfo info)
        {
            this.RenderVehicle(info, info.m_color0, false);
        }

        public void RenderVehicle(VehicleInfo info, Color color, bool useColor = true)
        {
            InfoManager instance = Singleton<InfoManager>.instance;
            InfoManager.InfoMode currentMode = instance.CurrentMode;
            InfoManager.SubInfoMode currentSubMode = instance.CurrentSubMode;
            instance.SetCurrentMode(InfoManager.InfoMode.None, InfoManager.SubInfoMode.Default);
            instance.UpdateInfoMode();
            Light sunLightSource = DayNightProperties.instance.sunLightSource;
            float intensity = sunLightSource.intensity;
            Color color2 = sunLightSource.color;
            Vector3 eulerAngles = sunLightSource.transform.eulerAngles;
            sunLightSource.intensity = 2f;
            sunLightSource.color = Color.white;
            sunLightSource.transform.eulerAngles = new Vector3(50f, 180f, 70f);
            Light mainLight = Singleton<RenderManager>.instance.MainLight;
            Singleton<RenderManager>.instance.MainLight = sunLightSource;
            if (mainLight == DayNightProperties.instance.moonLightSource)
            {
                DayNightProperties.instance.sunLightSource.enabled = true;
                DayNightProperties.instance.moonLightSource.enabled = false;
            }
            Vector3 one = Vector3.one;
            float magnitude = info.m_mesh.bounds.extents.magnitude;
            float num = magnitude + 16f;
            float num2 = magnitude * this.m_zoom;
            this.m_camera.transform.position = Vector3.forward * num2;
            this.m_camera.transform.rotation = Quaternion.AngleAxis(180f, Vector3.up);
            this.m_camera.nearClipPlane = Mathf.Max(num2 - num * 1.5f, 0.01f);
            this.m_camera.farClipPlane = num2 + num * 1.5f;
            Quaternion quaternion = Quaternion.Euler(20f, 0f, 0f) * Quaternion.Euler(0f, this.m_rotation, 0f);
            Vector3 pos = quaternion * -info.m_mesh.bounds.center;
            VehicleManager instance2 = Singleton<VehicleManager>.instance;
            Matrix4x4 matrix = Matrix4x4.TRS(pos, quaternion, Vector3.one);
            Matrix4x4 value = info.m_vehicleAI.CalculateTyreMatrix(Vehicle.Flags.Created, ref pos, ref quaternion, ref one, ref matrix);
            MaterialPropertyBlock materialBlock = instance2.m_materialBlock;
            materialBlock.Clear();
            materialBlock.SetMatrix(instance2.ID_TyreMatrix, value);
            materialBlock.SetVector(instance2.ID_TyrePosition, Vector3.zero);
            materialBlock.SetVector(instance2.ID_LightState, Vector3.zero);
            if (useColor)
            {
                materialBlock.SetColor(instance2.ID_Color, color);
            }
            instance2.m_drawCallData.m_defaultCalls += 1;
            info.m_material.SetVectorArray(instance2.ID_TyreLocation, info.m_generatedInfo.m_tyres);
            Graphics.DrawMesh(info.m_mesh, matrix, info.m_material, 0, this.m_camera, 0, materialBlock, true, true);
            this.m_camera.RenderWithShader(info.m_material.shader, "");
            sunLightSource.intensity = intensity;
            sunLightSource.color = color2;
            sunLightSource.transform.eulerAngles = eulerAngles;
            Singleton<RenderManager>.instance.MainLight = mainLight;
            if (mainLight == DayNightProperties.instance.moonLightSource)
            {
                DayNightProperties.instance.sunLightSource.enabled = false;
                DayNightProperties.instance.moonLightSource.enabled = true;
            }
            instance.SetCurrentMode(currentMode, currentSubMode);
            instance.UpdateInfoMode();
        }
    }
}
