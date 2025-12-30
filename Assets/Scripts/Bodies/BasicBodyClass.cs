using NUnit.Framework;
using PerspectiveRenderer.Body.Types;
using PerspectiveRenderer.Config;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

namespace PerspectiveRenderer.Body.BasicBody
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [System.Serializable]
    public class BasicBodyClass : MonoBehaviour
    {
        private static readonly Vector3 X_AXIS_ROTATION = Vector3.right;
        private static readonly Vector3 Y_AXIS_ROTATION = Vector3.up;

        private static readonly IReadOnlyDictionary<RevolutionAxis, Vector3> REVOLUTION_AXES = new Dictionary<RevolutionAxis, Vector3>()
        {
            {RevolutionAxis.X_AXIS, X_AXIS_ROTATION },
            {RevolutionAxis.Y_AXIS, Y_AXIS_ROTATION }
        };

        [SerializeField]
        private SerializedInfo bodyInfo;

        private Mesh bodyMesh;
        private MeshFilter meshFilter;

        public ref readonly SerializedInfo BodyInfo => ref bodyInfo;


        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            bodyMesh = new Mesh();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            ReBuild();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void ReBuild()
        {
            bodyMesh.Clear();

            if (bodyInfo.points.Count < 3)
            {
                Debug.LogError("At least three points are needed");
                return;
            }

            Vector3[] vertices = new Vector3[0];
            int[] triangles = new int[0];

            switch (bodyInfo.bodyType)
            {
                case BodyType.EXTRUSION:
                    BuildExtrusion(out vertices, out triangles);
                    break;
                case BodyType.REVOLUTION:
                    BuildRevolution(Vector3.zero, bodyInfo.revolutionRadianLength, out vertices, out triangles);
                    break;
                default:
                    Debug.LogError("Unknown body type");
                    return;
            }

            bodyMesh.vertices = vertices;
            bodyMesh.triangles = triangles;

            bodyMesh.RecalculateNormals();
            bodyMesh.RecalculateBounds();
            bodyMesh.RecalculateTangents();

            meshFilter.mesh = bodyMesh;
        }

        private void BuildExtrusion(out Vector3[] vertices, out int[] triangles)
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            Vector3[] temp_vertices;
            int[] temp_triangles;

            int accumOffset = 0;

            /* Upper face */
            temp_vertices = bodyInfo.points.ToArray();
            temp_triangles = FanTriangulation(temp_vertices.Length, 0);
            verts.AddRange(temp_vertices);
            tris.AddRange(temp_triangles);

            accumOffset += temp_vertices.Length;

            /* Lower face */
            List<Vector3> lowerFace = new List<Vector3>(bodyInfo.points);
            lowerFace.Reverse();
            for (int i = 0; i < lowerFace.Count; i++)
            {
                lowerFace[i] += bodyInfo.extrusionVector;
            }
            temp_vertices = lowerFace.ToArray();
            temp_triangles = FanTriangulation(temp_vertices.Length, accumOffset);
            verts.AddRange(temp_vertices);
            tris.AddRange(temp_triangles);

            accumOffset += temp_vertices.Length;

            /* Side faces */
            int pointCount = bodyInfo.points.Count;
            for (int i = 0; i < pointCount; i++)
            {
                int nextIndex = (i + 1) % pointCount;
                Vector3 v1 = bodyInfo.points[i];
                Vector3 v0 = bodyInfo.points[nextIndex];
                Vector3 v3 = bodyInfo.points[nextIndex] + bodyInfo.extrusionVector;
                Vector3 v2 = bodyInfo.points[i] + bodyInfo.extrusionVector;
                temp_vertices = new Vector3[] { v0, v1, v2, v3 };
                temp_triangles = FanTriangulation(temp_vertices.Length, accumOffset);
                verts.AddRange(temp_vertices);
                tris.AddRange(temp_triangles);
                accumOffset += temp_vertices.Length;
            }


            vertices = verts.ToArray();
            triangles = tris.ToArray();
        }

        private void BuildRevolution(Vector3 center, float arcRadians, out Vector3[] vertices, out int[] triangles)
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            Vector3[] temp_vertices;
            int[] temp_triangles;

            int accumOffset = 0;

            if ((arcRadians > -FixedConfig.TWO_PI) && (arcRadians < FixedConfig.TWO_PI))
            {
                /* Arc start face */
                temp_vertices = bodyInfo.points.ToArray();
                temp_triangles = FanTriangulation(temp_vertices.Length, 0);
                verts.AddRange(temp_vertices);
                tris.AddRange(temp_triangles);

                accumOffset += temp_vertices.Length;

                /* Arc end face */
                Vector3 rotation_vector = bodyInfo.revolutionRadianLength * Mathf.Rad2Deg * REVOLUTION_AXES[bodyInfo.revolutionAxis];
                Quaternion rot_end = Quaternion.Euler(rotation_vector);

                List<Vector3> lowerFace = new(bodyInfo.points);
                lowerFace.Reverse();
                for (int i = 0; i < lowerFace.Count; i++)
                {
                    lowerFace[i] = bodyInfo.revolutionCenter + (rot_end * (lowerFace[i]-bodyInfo.revolutionCenter));
                }
                temp_vertices = lowerFace.ToArray();
                temp_triangles = FanTriangulation(temp_vertices.Length, accumOffset);
                verts.AddRange(temp_vertices);
                tris.AddRange(temp_triangles);

                accumOffset += temp_vertices.Length;
            }

            /* Side faces */
            float rot_total = Mathf.Clamp(bodyInfo.revolutionRadianLength, -FixedConfig.TWO_PI, FixedConfig.TWO_PI);
            float arc_step = rot_total / FixedConfig.REVOLUTION_SEGMENT_COUNT;
            for (int rot = 0; rot < FixedConfig.REVOLUTION_SEGMENT_COUNT; rot++)
            {
                int nextRotIndex = rot + 1;
                float radians_start = arc_step * rot;
                float radians_end = arc_step * nextRotIndex;
                Vector3 rotation_vector = Mathf.Rad2Deg * REVOLUTION_AXES[bodyInfo.revolutionAxis];
                Quaternion rot_start = Quaternion.Euler(radians_start * rotation_vector);
                Quaternion rot_end = Quaternion.Euler(radians_end * rotation_vector);

                for (int i = 0; i < bodyInfo.points.Count; i++)
                {
                    int nextIndex = (i + 1) % bodyInfo.points.Count;
                    
                    Vector3 v1 = bodyInfo.revolutionCenter + (rot_start * (bodyInfo.points[i] - bodyInfo.revolutionCenter));
                    Vector3 v0 = bodyInfo.revolutionCenter + (rot_start * (bodyInfo.points[nextIndex] - bodyInfo.revolutionCenter));
                    Vector3 v3 = bodyInfo.revolutionCenter + (rot_end * (bodyInfo.points[nextIndex] - bodyInfo.revolutionCenter));
                    Vector3 v2 = bodyInfo.revolutionCenter + (rot_end * (bodyInfo.points[i] - bodyInfo.revolutionCenter));

                    temp_vertices = new Vector3[] { v0, v1, v2, v3 };
                    temp_triangles = FanTriangulation(temp_vertices.Length, accumOffset);

                    verts.AddRange(temp_vertices);
                    tris.AddRange(temp_triangles);
                    accumOffset += temp_vertices.Length;
                }
            }

            if(bodyInfo.revolutionAxis == RevolutionAxis.Y_AXIS)
            {
                tris.Reverse();
            }

            vertices = verts.ToArray();
            triangles = tris.ToArray();
        }

        private int[] FanTriangulation(int verticesCount, int offset)
        {
            int[] triangles = new int[(verticesCount - 2) * 3];

            for (int i = 0; i < verticesCount - 2; i++)
            {
                triangles[i * 3] = offset;
                triangles[i * 3 + 1] = offset + i + 1;
                triangles[i * 3 + 2] = offset + i + 2;
            }

            return triangles;
        }
    }
}
