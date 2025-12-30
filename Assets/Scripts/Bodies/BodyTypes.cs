using System.Collections.Generic;
using UnityEngine;

namespace PerspectiveRenderer.Body.Types
{
    public enum BodyType
    {
        EXTRUSION,
        REVOLUTION
    }

    public enum RevolutionAxis
    {
        X_AXIS,
        Y_AXIS
    }

    [System.Serializable]
    public struct SerializedInfo
    {
        public string bodyName;
        public BodyType bodyType;
        public RevolutionAxis revolutionAxis;
        public List<Vector3> points;
        public float revolutionRadianLength;
        public Vector3 extrusionVector;
        public Vector3 revolutionCenter;
    }
}
