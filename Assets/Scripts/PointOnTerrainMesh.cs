using UnityEngine;

namespace Minecraft
{
    public sealed class PointOnTerrainMesh
    {
        public PointOnTerrainMesh(Vector3 point, Vector3 normal, Ray sourceRay)
        {
            Point = point;
            Normal = normal;
            SourceRay = sourceRay;
        }

        public Vector3 Point { get; private set; }

        public Vector3 Normal { get; private set; }

        public Ray SourceRay { get; private set; }
    }
}