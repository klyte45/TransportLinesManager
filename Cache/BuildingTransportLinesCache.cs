using ColossalFramework.Math;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Klyte.TransportLinesManager.Cache
{
    public class BuildingTransportLinesCache
    {

        private SimpleNonSequentialList<List<InnerBuildingLine>> OutsideConnectionsLinesBuilding = new SimpleNonSequentialList<List<InnerBuildingLine>>();
        private SimpleNonSequentialList<Mesh[][]> m_lineMeshes = new SimpleNonSequentialList<Mesh[][]>();
        private SimpleNonSequentialList<RenderGroup.MeshData[][]> m_lineMeshData = new SimpleNonSequentialList<RenderGroup.MeshData[][]>();

        public void RenderBuildingLines(RenderManager.CameraInfo cameraInfo, ushort buildingId)
        {
            var targetBuildingId = Building.FindParentBuilding(buildingId);
            if (targetBuildingId == 0)
            {
                targetBuildingId = buildingId;
            }

            ref Building b = ref BuildingManager.instance.m_buildings.m_buffer[targetBuildingId];
            var info = b.Info;
            if (info.m_buildingAI is TransportStationAI tsai)
            {
                if (!OutsideConnectionsLinesBuilding.ContainsKey(targetBuildingId))
                {
                    DoMapping(targetBuildingId, ref b, tsai);
                }

                for (int i = 0; i < OutsideConnectionsLinesBuilding[targetBuildingId].Count; i++)
                {
                    if (OutsideConnectionsLinesBuilding[targetBuildingId][i].BrokenFromSrc && OutsideConnectionsLinesBuilding[targetBuildingId][i].BrokenFromDst)
                    {
                        continue;
                    }

                    if (m_lineMeshData[targetBuildingId][i] != null)
                    {
                        UpdateMesh(targetBuildingId, i);
                    }
                    else
                    {
                        RenderLine(cameraInfo, targetBuildingId, (ushort)i);
                    }
                }
            }
        }

        public List<InnerBuildingLine> SafeGet(ushort buildingId)
        {
            ref Building b = ref BuildingManager.instance.m_buildings.m_buffer[buildingId];
            var info = b.Info;
            if (b.m_parentBuilding != 0)
            {
                return SafeGet(Building.FindParentBuilding(buildingId));
            }
            if (OutsideConnectionsLinesBuilding.ContainsKey(buildingId))
            {
                return OutsideConnectionsLinesBuilding[buildingId];
            }
            if (info.m_buildingAI is TransportStationAI tsai)
            {
                DoMapping(buildingId, ref b, tsai);
            }
            else
            {
                OutsideConnectionsLinesBuilding[buildingId] = null;
            }
            return OutsideConnectionsLinesBuilding[buildingId];
        }
        private void DoMapping(ushort buildingId, ref Building b, TransportStationAI tsai)
        {
            OutsideConnectionsLinesBuilding[buildingId] = new List<InnerBuildingLine>();
            var useSecInfo = tsai.UseSecondaryTransportInfoForConnection();
            var targetInfo = useSecInfo ? tsai.m_secondaryTransportInfo : tsai.m_transportInfo;
            MapBuildingLines(buildingId, buildingId, targetInfo);
            var nextSubBuildingId = b.m_subBuilding;
            do
            {
                MapBuildingLines(buildingId, nextSubBuildingId, targetInfo);
                nextSubBuildingId = BuildingManager.instance.m_buildings.m_buffer[nextSubBuildingId].m_subBuilding;
            } while (nextSubBuildingId != 0);

            m_lineMeshData[buildingId] = new RenderGroup.MeshData[OutsideConnectionsLinesBuilding[buildingId].Count][];
            m_lineMeshes[buildingId] = new Mesh[OutsideConnectionsLinesBuilding[buildingId].Count][];
            for (ushort i = 0; i < OutsideConnectionsLinesBuilding[buildingId].Count; i++)
            {
                UpdateMeshData(buildingId, i);
            }
        }

        private void MapBuildingLines(ushort buildingIdKey, ushort buildingId, TransportInfo targetInfo)
        {
            var nextNodeId = BuildingManager.instance.m_buildings.m_buffer[buildingId].m_netNode;
            do
            {
                var currentNode = nextNodeId;
                ref NetNode node = ref NetManager.instance.m_nodes.m_buffer[currentNode];

                if (!(node.Info.m_netAI is TransportLineAI))
                {
                    break;
                }
                InnerBuildingLine transportLine =
                    TLMStationUtils.GetStationBuilding(nextNodeId, (ushort)OutsideConnectionsLinesBuilding[buildingIdKey].Count, buildingId) != buildingIdKey
                        ? new InnerBuildingLine
                        {
                            Info = targetInfo,
                            DstStop = nextNodeId,
                            SrcStop = NetManager.instance.m_segments.m_buffer[node.m_segment0].GetOtherNode(nextNodeId)
                        }
                        : new InnerBuildingLine
                        {
                            Info = targetInfo,
                            SrcStop = nextNodeId,
                            DstStop = NetManager.instance.m_segments.m_buffer[node.m_segment0].GetOtherNode(nextNodeId)
                        };
                OutsideConnectionsLinesBuilding[buildingIdKey].Add(transportLine);
                nextNodeId = node.m_nextBuildingNode;
            } while (nextNodeId != 0);
        }

        public bool UpdateMeshData(ushort buildingId, ushort lineID)
        {
            InnerBuildingLine tl = OutsideConnectionsLinesBuilding[buildingId][lineID];
            NetManager instance2 = NetManager.instance;
            PathManager instance3 = PathManager.instance;
            TerrainManager instance4 = TerrainManager.instance;
            TransportInfo info = tl.Info;
            TransportLine.TempUpdateMeshData[] array = info.m_requireSurfaceLine ? (new TransportLine.TempUpdateMeshData[81]) : (new TransportLine.TempUpdateMeshData[1]);
            bool flag = true;
            int num = 0;
            int num2 = 0;
            float num3 = 0f;
            ushort stops = tl.SrcStop;
            ushort num4 = stops;
            int num5 = 0;
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
                            if ((pathFindFlags & 4) != 0)
                            {
                                Vector3 zero = Vector3.zero;
                                if (!TransportLine.CalculatePathSegmentCount(path, 0, NetInfo.LaneType.All, VehicleInfo.VehicleType.All, ref array, ref num2, ref num3, ref zero))
                                {
                                    TransportLineAI.StartPathFind(segment, ref instance2.m_segments.m_buffer[segment], info.m_netService, info.m_secondaryNetService, info.m_vehicleType, false);
                                    flag = false;
                                }
                            }
                            else if ((pathFindFlags & 8) == 0)
                            {
                                if (num4 == stops)
                                {
                                    tl.BrokenFromSrc = true;
                                }
                                else
                                {
                                    tl.BrokenFromDst = true;
                                }
                                flag = false;
                            }
                        }
                        num6 = instance2.m_segments.m_buffer[segment].m_endNode;
                        break;
                    }
                }
                if (info.m_requireSurfaceLine)
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
            if (!flag)
            {
                return flag;
            }
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
                            TransportLine.FillPathSegments(path2, 0, NetInfo.LaneType.All, VehicleInfo.VehicleType.All, ref array, array5, null, ref num10, ref num11, lengthScale, out Vector3 vector3, out Vector3 vector4, info.m_requireSurfaceLine, true);
                            vector = Vector3.Min(vector, vector3);
                            vector2 = Vector3.Max(vector2, vector4);
                            array4[num9].m_bounds.SetMinMax(vector3, vector4);
                            array4[num9].m_curveEnd = num10;
                        }
                        num12 = instance2.m_segments.m_buffer[segment2].m_endNode;
                        break;
                    }
                }
                if (info.m_requireSurfaceLine)
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
                    if (info.m_requireSurfaceLine)
                    {
                        Vector3 min = array[l].m_meshData.m_bounds.min;
                        Vector3 max = array[l].m_meshData.m_bounds.max;
                        max.y += 1024f;
                        array[l].m_meshData.m_bounds.SetMinMax(min, max);
                    }
                    array8[num15++] = array[l].m_meshData;
                }
            }
            while (!Monitor.TryEnter(m_lineMeshData, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            try
            {
                m_lineMeshData[buildingId][lineID] = array8;
                //m_lineSegments[lineID] = array4;
                //m_lineCurves[lineID] = array5;
                //tl.m_bounds.SetMinMax(vector, vector2);
            }
            finally
            {
                Monitor.Exit(m_lineMeshData);
            }
            return flag;
        }

        private void RenderLine(RenderManager.CameraInfo cameraInfo, ushort buildingId, ushort lineID)
        {
            InnerBuildingLine tl = OutsideConnectionsLinesBuilding[buildingId][lineID];
            TransportInfo info = tl.Info;
            Material material = info.m_lineMaterial2;
            TerrainManager instance2 = TerrainManager.instance;
            Mesh[] array = m_lineMeshes[buildingId][lineID];
            if (array != null)
            {
                int num = array.Length;
                for (int i = 0; i < num; i++)
                {
                    Mesh mesh = array[i];
                    if (mesh != null && cameraInfo.Intersect(mesh.bounds))
                    {
                        material.color = COLOR_ORDER[lineID % COLOR_ORDER.Length];
                        material.SetFloat(TransportManager.instance.ID_StartOffset, -1000f);
                        if (info.m_requireSurfaceLine)
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

        private void UpdateMesh(ushort buildingId, int lineID)
        {
            while (!Monitor.TryEnter(m_lineMeshData, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            RenderGroup.MeshData[] array;
            try
            {
                array = m_lineMeshData[buildingId][lineID];
                m_lineMeshData[buildingId][lineID] = null;
            }
            finally
            {
                Monitor.Exit(m_lineMeshData);
            }
            if (array != null)
            {
                Mesh[] array2 = m_lineMeshes[buildingId][lineID];
                int num = 0;
                if (array2 != null)
                {
                    num = array2.Length;
                }
                if (num != array.Length)
                {
                    Mesh[] array3 = new Mesh[array.Length];
                    int num2 = Mathf.Min(num, array3.Length);
                    for (int i = 0; i < num2; i++)
                    {
                        array3[i] = array2[i];
                    }
                    for (int j = num2; j < array3.Length; j++)
                    {
                        array3[j] = new Mesh();
                    }
                    for (int k = num2; k < num; k++)
                    {
                        UnityEngine.Object.Destroy(array2[k]);
                    }
                    array2 = array3;
                    m_lineMeshes[buildingId][lineID] = array2;
                }
                for (int l = 0; l < array.Length; l++)
                {
                    array2[l].Clear();
                    array2[l].vertices = array[l].m_vertices;
                    array2[l].normals = array[l].m_normals;
                    array2[l].tangents = array[l].m_tangents;
                    array2[l].uv = array[l].m_uvs;
                    array2[l].uv2 = array[l].m_uvs2;
                    array2[l].colors32 = array[l].m_colors;
                    array2[l].triangles = array[l].m_triangles;
                    array2[l].bounds = array[l].m_bounds;
                }
            }
        }

        private void RenderSpawnPoint(RenderManager.CameraInfo cameraInfo, ref RenderManager.Instance renderInstance, int i, Vector3 point)
            => RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo,
               COLOR_ORDER[i % COLOR_ORDER.Length],
               renderInstance.m_dataMatrix1.MultiplyPoint(point),
               3,
               -1, 1280f, false, true);

        internal static readonly Color[] COLOR_ORDER = new Color[]
             {
                Color.red,
                Color.Lerp(Color.red, Color.yellow,0.5f),
                Color.yellow,
                Color.green,
                Color.cyan,
                Color.blue,
                Color.Lerp(Color.blue, Color.magenta,0.5f),
                Color.magenta,
                Color.white,
                Color.black,
                Color.Lerp( Color.red,                                    Color.black,0.5f),
                Color.Lerp( Color.Lerp(Color.red, Color.yellow,0.5f),     Color.black,0.5f),
                Color.Lerp( Color.yellow,                                 Color.black,0.5f),
                Color.Lerp( Color.green,                                  Color.black,0.5f),
                Color.Lerp( Color.cyan,                                   Color.black,0.5f),
                Color.Lerp( Color.blue,                                   Color.black,0.5f),
                Color.Lerp( Color.Lerp(Color.blue, Color.magenta,0.5f),   Color.black,0.5f),
                Color.Lerp( Color.magenta,                                Color.black,0.5f),
                Color.Lerp( Color.white,                                  Color.black,0.25f),
                Color.Lerp( Color.white,                                  Color.black,0.75f)
             };
    }
}