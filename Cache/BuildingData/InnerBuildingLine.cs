using ColossalFramework.Math;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using System;
using UnityEngine;

namespace Klyte.TransportLinesManager.Cache
{
    public class InnerBuildingLine : IIdentifiable
    {
        public long? Id { get => SrcStop; set { } }
        public TransportInfo Info { get; set; }
        public ushort SrcBuildingId { get; set; }
        public ushort DstBuildingId { get; set; }
        public ushort SrcStop { get; set; }
        public ushort DstStop { get; set; }
        public bool BrokenFromSrc { get; set; }
        public bool BrokenFromDst { get; set; }

        private bool m_needsToBeCalculated;
        private uint m_lastCheckTick;

        private Mesh[] m_lineMeshes;
        private RenderGroup.MeshData[] m_lineMeshData;

        public int CountStops()
        {
            int num = 0;
            ushort stops = SrcStop;
            ushort num2 = stops;
            int num3 = 0;
            while (num2 != 0)
            {
                num++;
                num2 = TransportLine.GetNextStop(num2);
                if (num2 == stops)
                {
                    break;
                }
                if (++num3 >= 32768)
                {
                    LogUtils.DoErrorLog("Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return num;
        }
        public ushort GetStop(int index)
        {
            if (index == -1)
            {
                return GetLastStop();
            }
            ushort stops = SrcStop;
            ushort num = stops;
            int num2 = 0;
            while (num != 0)
            {
                if (index-- == 0)
                {
                    return num;
                }
                num = TransportLine.GetNextStop(num);
                if (num == stops)
                {
                    break;
                }
                if (++num2 >= 32768)
                {
                    LogUtils.DoErrorLog("Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            return 0;
        }
        public ushort GetLastStop()
        {
            NetManager instance = NetManager.instance;
            ushort num = SrcStop;
            int num2 = 0;
            for (; ; )
            {
                bool flag = false;
                int i = 0;
                while (i < 8)
                {
                    ushort segment = instance.m_nodes.m_buffer[num].GetSegment(i);
                    if (segment != 0 && instance.m_segments.m_buffer[segment].m_startNode == num)
                    {
                        num = instance.m_segments.m_buffer[segment].m_endNode;
                        if (num == SrcStop)
                        {
                            return num;
                        }
                        flag = true;
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }
                if (++num2 >= 32768)
                {
                    LogUtils.DoErrorLog("Invalid list detected!\n" + Environment.StackTrace);
                    return num;
                }
                if (!flag)
                {
                    return num;
                }
            }
        }

        private OutsideConnectionLineInfo m_cachedLineInfoRef;
        public OutsideConnectionLineInfo LineDataObject 
            => !TLMBuildingDataContainer.Instance.SafeGet(SrcBuildingId).TlmManagedRegionalLines 
            ? null 
            : m_cachedLineInfoRef ?? (m_cachedLineInfoRef = TLMBuildingDataContainer.Instance.SafeGet(SrcBuildingId).PlatformMappings[NetManager.instance.m_nodes.m_buffer[SrcStop].m_lane].TargetOutsideConnections[DstBuildingId]);

        public void RenderLine(RenderManager.CameraInfo cameraInfo)
        {
            if (Info.m_transportType != TransportInfo.TransportType.Train) return;// TEMPORARY
            if (m_needsToBeCalculated && SimulationManager.instance.m_currentTickIndex - m_lastCheckTick > 50)
            {
                UpdateMeshData();
            }
            if (m_lineMeshData != null)
            {
                UpdateMesh();
            }
            RenderLine_internal(cameraInfo);
        }
        public bool UpdateMeshData()
        {
            NetManager instance2 = NetManager.instance;
            PathManager instance3 = PathManager.instance;
            TerrainManager instance4 = TerrainManager.instance;
            TransportLine.TempUpdateMeshData[] array = Info.m_requireSurfaceLine ? (new TransportLine.TempUpdateMeshData[81]) : (new TransportLine.TempUpdateMeshData[1]);
            bool flag = true;
            int num = 0;
            int num2 = 0;
            float num3 = 0f;
            ushort stops = SrcStop;
            ushort num4 = stops;
            int num5 = 0;
            m_needsToBeCalculated = false;
            while (num4 != 0)
            {
                ushort num6 = 0;
                for (int i = 0; i < 8; i++)
                {
                    ushort segment = instance2.m_nodes.m_buffer[num4].GetSegment(i);
                    if (segment != 0 && instance2.m_segments.m_buffer[segment].m_startNode == num4)
                    {
                        uint path = instance2.m_segments.m_buffer[segment].m_path;
                        if (path != 0U)
                        {
                            byte pathFindFlags = instance3.m_pathUnits.m_buffer[path].m_pathFindFlags;
                            if ((pathFindFlags & 4) != 0)//not calculated
                            {
                                Vector3 zero = Vector3.zero;
                                if (!TransportLine.CalculatePathSegmentCount(path, 0, NetInfo.LaneType.All, VehicleInfo.VehicleType.All, VehicleInfo.VehicleCategory.All, ref array, ref num2, ref num3, ref zero))
                                {
                                    TransportLineAI.StartPathFind(segment, ref instance2.m_segments.m_buffer[segment], Info.m_netService, Info.m_secondaryNetService, Info.m_vehicleType, Info.vehicleCategory, false);
                                    flag = false;
                                    m_needsToBeCalculated = true;
                                }
                            }
                            else if ((pathFindFlags & 8) == 0) //invalid
                            {
                                if (num4 == stops)
                                {
                                    BrokenFromSrc = true;
                                }
                                else
                                {
                                    BrokenFromDst = true;
                                }
                                flag = false;
                            }
                        }
                        else
                        {
                            TransportLineAI.StartPathFind(segment, ref instance2.m_segments.m_buffer[segment], Info.m_netService, Info.m_secondaryNetService, Info.m_vehicleType, Info.vehicleCategory, false);
                            flag = false;
                            m_needsToBeCalculated = true;
                        }
                        num6 = instance2.m_segments.m_buffer[segment].m_endNode;
                        break;
                    }
                }
                if (Info.m_requireSurfaceLine)
                {
                    TransportLine.TempUpdateMeshData[] array2 = array;
                    int patchIndex = instance4.GetPatchIndex(instance2.m_nodes.m_buffer[num4].m_position);
                    array2[patchIndex].m_pathSegmentCount = array2[patchIndex].m_pathSegmentCount + 1;
                }
                else
                {
                    TransportLine.TempUpdateMeshData[] array3 = array;
                    int num7 = 0;
                    array3[num7].m_pathSegmentCount = array3[num7].m_pathSegmentCount + 1;
                }
                num++;
                num4 = num6;
                if (num4 == stops)
                {
                    break;
                }
                //if (!flag)
                //{
                //    break;
                //}
                if (++num5 >= 32768)
                {
                    LogUtils.DoErrorLog("Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            //if (!flag)
            //{
            //    return flag;
            //}
            int num8 = 0;
            for (int j = 0; j < array.Length; j++)
            {
                int pathSegmentCount = array[j].m_pathSegmentCount;
                if (pathSegmentCount != 0)
                {
                    RenderGroup.MeshData meshData = new RenderGroup.MeshData
                    {
                        m_vertices = new Vector3[pathSegmentCount * 8],
                        m_normals = new Vector3[pathSegmentCount * 8],
                        m_tangents = new Vector4[pathSegmentCount * 8],
                        m_uvs = new Vector2[pathSegmentCount * 8],
                        m_uvs2 = new Vector2[pathSegmentCount * 8],
                        m_colors = new Color32[pathSegmentCount * 8],
                        m_triangles = new int[pathSegmentCount * 30]
                    };
                    array[j].m_meshData = meshData;
                    num8++;
                }
            }
            TransportManager.LineSegment[] array4 = new TransportManager.LineSegment[num];
            Bezier3[] array5 = new Bezier3[num2];
            int num9 = 0;
            int num10 = 0;
            float lengthScale = Mathf.Ceil(num3 / 64f) / num3;
            float num11 = 0f;
            num4 = stops;
            Vector3 vector = new Vector3(100000f, 100000f, 100000f);
            Vector3 vector2 = new Vector3(-100000f, -100000f, -100000f);
            num5 = 0;
            while (num4 != 0)
            {
                ushort num12 = 0;
                for (int k = 0; k < 8; k++)
                {
                    ushort segment2 = instance2.m_nodes.m_buffer[num4].GetSegment(k);
                    if (segment2 != 0 && instance2.m_segments.m_buffer[segment2].m_startNode == num4)
                    {
                        uint path2 = instance2.m_segments.m_buffer[segment2].m_path;
                        if (path2 != 0U && (instance3.m_pathUnits.m_buffer[(int)((UIntPtr)path2)].m_pathFindFlags & 4) != 0)
                        {
                            array4[num9].m_curveStart = num10;
                            TransportLine.FillPathSegments(path2, 0, NetInfo.LaneType.All, VehicleInfo.VehicleType.All, VehicleInfo.VehicleCategory.All, ref array, array5, null, ref num10, ref num11, lengthScale, out Vector3 vector3, out Vector3 vector4, Info.m_requireSurfaceLine, true);
                            vector = Vector3.Min(vector, vector3);
                            vector2 = Vector3.Max(vector2, vector4);
                            array4[num9].m_bounds.SetMinMax(vector3, vector4);
                            array4[num9].m_curveEnd = num10;
                        }
                        num12 = instance2.m_segments.m_buffer[segment2].m_endNode;
                        break;
                    }
                }
                if (Info.m_requireSurfaceLine)
                {
                    int patchIndex2 = instance4.GetPatchIndex(instance2.m_nodes.m_buffer[num4].m_position);
                    TransportLine.FillPathNode(instance2.m_nodes.m_buffer[num4].m_position, array[patchIndex2].m_meshData, array[patchIndex2].m_pathSegmentIndex, 4f, 20f, true);
                    TransportLine.TempUpdateMeshData[] array6 = array;
                    int num13 = patchIndex2;
                    array6[num13].m_pathSegmentIndex = array6[num13].m_pathSegmentIndex + 1;
                }
                else
                {
                    TransportLine.FillPathNode(instance2.m_nodes.m_buffer[num4].m_position, array[0].m_meshData, array[0].m_pathSegmentIndex, 4f, 5f, false);
                    TransportLine.TempUpdateMeshData[] array7 = array;
                    int num14 = 0;
                    array7[num14].m_pathSegmentIndex = array7[num14].m_pathSegmentIndex + 1;
                }
                num9++;
                num4 = num12;
                if (num4 == stops)
                {
                    break;
                }
                if (++num5 >= 32768)
                {
                    LogUtils.DoErrorLog("Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            RenderGroup.MeshData[] array8 = new RenderGroup.MeshData[num8];
            int num15 = 0;
            for (int l = 0; l < array.Length; l++)
            {
                if (array[l].m_meshData != null)
                {
                    array[l].m_meshData.UpdateBounds();
                    if (Info.m_requireSurfaceLine)
                    {
                        Vector3 min = array[l].m_meshData.m_bounds.min;
                        Vector3 max = array[l].m_meshData.m_bounds.max;
                        max.y += 1024f;
                        array[l].m_meshData.m_bounds.SetMinMax(min, max);
                    }
                    array8[num15++] = array[l].m_meshData;
                }
            }
            m_lineMeshData = array8;
            m_lastCheckTick = SimulationManager.instance.m_currentTickIndex;
            //m_lineSegments[lineID] = array4;
            //m_lineCurves[lineID] = array5;
            //tl.m_bounds.SetMinMax(vector, vector2);

            return flag;
        }

        private void RenderLine_internal(RenderManager.CameraInfo cameraInfo)
        {
            Material material = Info.m_lineMaterial2;
            TerrainManager instance2 = TerrainManager.instance;
            if (m_lineMeshes != null)
            {
                int num = m_lineMeshes.Length;
                for (int i = 0; i < num; i++)
                {
                    Mesh mesh = m_lineMeshes[i];
                    if (mesh != null && cameraInfo.Intersect(mesh.bounds))
                    {
                        material.color = LineDataObject?.LineColor ?? TLMController.COLOR_ORDER[SrcStop % TLMController.COLOR_ORDER.Length];
                        material.SetFloat(TransportManager.instance.ID_StartOffset, -1000f);
                        if (Info.m_requireSurfaceLine)
                        {
                            instance2.SetWaterMaterialProperties(mesh.bounds.center, material);
                        }
                        if (material.SetPass(0))
                        {
                            TransportManager.instance.m_drawCallData.m_overlayCalls++;
                            Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
                        }
                    }
                }
            }
        }

        private void UpdateMesh()
        {
            RenderGroup.MeshData[] array;
            array = m_lineMeshData;
            m_lineMeshData = null;
            if (array != null)
            {
                int num = 0;
                if (m_lineMeshes != null)
                {
                    num = m_lineMeshes.Length;
                }
                if (num != array.Length)
                {
                    Mesh[] array3 = new Mesh[array.Length];
                    int num2 = Mathf.Min(num, array3.Length);
                    for (int i = 0; i < num2; i++)
                    {
                        array3[i] = m_lineMeshes[i];
                    }
                    for (int j = num2; j < array3.Length; j++)
                    {
                        array3[j] = new Mesh();
                    }
                    for (int k = num2; k < num; k++)
                    {
                        UnityEngine.Object.Destroy(m_lineMeshes[k]);
                    }
                    m_lineMeshes = array3;
                }
                for (int l = 0; l < array.Length; l++)
                {
                    m_lineMeshes[l].Clear();
                    m_lineMeshes[l].vertices = array[l].m_vertices;
                    m_lineMeshes[l].normals = array[l].m_normals;
                    m_lineMeshes[l].tangents = array[l].m_tangents;
                    m_lineMeshes[l].uv = array[l].m_uvs;
                    m_lineMeshes[l].uv2 = array[l].m_uvs2;
                    m_lineMeshes[l].colors32 = array[l].m_colors;
                    m_lineMeshes[l].triangles = array[l].m_triangles;
                    m_lineMeshes[l].bounds = array[l].m_bounds;
                }
            }
        }

    }
}