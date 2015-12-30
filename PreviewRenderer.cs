using ColossalFramework;
using System;
using UnityEngine;

namespace Klyte.TransportLinesManager
{
    public class PreviewRenderer : MonoBehaviour
    {
        private Camera m_camera;

        private Mesh m_mesh;

        private Bounds m_bounds;

        private float m_rotation = 120f;

        private float m_zoom = 3f;

        public Vector2 size
        {
            get
            {
                return new Vector2((float)this.m_camera.targetTexture.width, (float)this.m_camera.targetTexture.height);
            }
            set
            {
                if (this.size != value)
                {
                    this.m_camera.targetTexture = new RenderTexture((int)value.x, (int)value.y, 24, RenderTextureFormat.ARGB32);
                    this.m_camera.pixelRect = new Rect(0f, 0f, value.x, value.y);
                }
            }
        }

        public Mesh mesh
        {
            get
            {
                return this.m_mesh;
            }
            set
            {
                if (this.m_mesh != value)
                {
                    this.m_mesh = value;
                    if (value != null)
                    {
                        this.m_bounds = new Bounds(Vector3.zero, Vector3.zero);
                        Vector3[] vertices = this.mesh.vertices;
                        for (int i = 0; i < vertices.Length; i++)
                        {
                            this.m_bounds.Encapsulate(vertices[i]);
                        }
                    }
                }
            }
        }

        public Material material
        {
            get;
            set;
        }

        public RenderTexture texture
        {
            get
            {
                return this.m_camera.targetTexture;
            }
        }

        public float cameraRotation
        {
            get
            {
                return this.m_rotation;
            }
            set
            {
                this.m_rotation = value % 360f;
            }
        }

        public float zoom
        {
            get
            {
                return this.m_zoom;
            }
            set
            {
                this.m_zoom = Mathf.Clamp(value, 0.5f, 5f);
            }
        }

        public PreviewRenderer()
        {
            this.m_camera = new GameObject("Camera").AddComponent<Camera>();
            this.m_camera.transform.SetParent(base.transform);
            this.m_camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            this.m_camera.fieldOfView = 30f;
            this.m_camera.nearClipPlane = 1f;
            this.m_camera.farClipPlane = 1000f;
            this.m_camera.hdr = true;
            this.m_camera.enabled = false;
            this.m_camera.targetTexture = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
            this.m_camera.pixelRect = new Rect(0f, 0f, 512f, 512f);
        }

        public void Render()
        {
            if (this.m_mesh == null)
            {
                return;
            }
            float magnitude = this.m_bounds.extents.magnitude;
            float num = magnitude + 16f;
            float num2 = magnitude * this.m_zoom;
            this.m_camera.transform.position = -Vector3.forward * num2;
            this.m_camera.transform.rotation = Quaternion.identity;
            this.m_camera.nearClipPlane = Mathf.Max(num2 - num * 1.5f, 0.01f);
            this.m_camera.farClipPlane = num2 + num * 1.5f;
            Quaternion quaternion = Quaternion.Euler(-20f, 0f, 0f) * Quaternion.Euler(0f, this.m_rotation, 0f);
            Vector3 pos = quaternion * -this.m_bounds.center;
            Matrix4x4 matrix = Matrix4x4.TRS(pos, quaternion, Vector3.one);
            InfoManager instance = Singleton<InfoManager>.instance;
            InfoManager.InfoMode currentMode = instance.CurrentMode;
            InfoManager.SubInfoMode currentSubMode = instance.CurrentSubMode;
            instance.SetCurrentMode(InfoManager.InfoMode.None, InfoManager.SubInfoMode.Default);
            Graphics.DrawMesh(this.m_mesh, matrix, this.material, 0, this.m_camera, 0, null, true, true);
            this.m_camera.RenderWithShader(this.material.shader, "");
            instance.SetCurrentMode(currentMode, currentSubMode);
        }

        public void Render(Color color)
        {
            if (this.material == null)
            {
                return;
            }
            Color color2 = this.material.color;
            this.material.color = color;
            this.Render();
            this.material.color = color2;
        }
    }
}
