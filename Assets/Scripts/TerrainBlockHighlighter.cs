using System.Collections.Generic;
using UnityEngine;

namespace Minecraft
{
    /// <summary>
    /// Must be attached to a camera.
    /// </summary>
    public sealed class TerrainBlockHighlighter : MonoBehaviour
    {
        [SerializeField]
        private TerrainManager terrainManager;

        [SerializeField]
        private Material material;

        private BlockFace? blockFace;

        private void Update()
        {
            blockFace = null;

            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                return;
            }

            var pointOnTerrainMesh = terrainManager.RaycastTerrainMesh(GetViewRay(), 0.01f);

            if (pointOnTerrainMesh != null)
            {
                var block = terrainManager.GetBlockAt(pointOnTerrainMesh.Point);

                if (block.BlockType != TerrainBlock.Type.None)
                {
                    var blockBounds = block.GetBounds();

                    const float faceOffset = 1.001f;
                    var blockFaceCenter = blockBounds.center + Vector3.Scale(pointOnTerrainMesh.Normal, blockBounds.extents * faceOffset);

                    blockFace = new BlockFace
                    {
                        Center = blockFaceCenter,
                        Normal = pointOnTerrainMesh.Normal,
                        BlockExtents = blockBounds.extents
                    };
                }
            }
        }

        private Ray GetViewRay()
        {
            return new Ray(transform.position, transform.forward);
        }

        private void OnPostRender()
        {
            if (blockFace != null)
            {
                var faceVertices = GetFaceVertices(blockFace.Value);
                DrawFace(faceVertices);
            }
        }

        private static IEnumerable<Vector3> GetFaceVertices(BlockFace face)
        {
            Vector3 right, up;
            GetFaceAxis(face, out right, out up);

            var center = face.Center;

            return new Vector3[]
            {
                center - right - up,
                center - right + up,
                center + right + up,
                center + right - up,
            };
        }

        private static void GetFaceAxis(BlockFace face, out Vector3 right, out Vector3 up)
        {
            var normal = face.Normal;
            right = Vector3.Cross(normal, Vector3.up);

            if (right.sqrMagnitude < 0.01f)
            {
                right = Vector3.Cross(normal, Vector3.forward);
            }

            up = Vector3.Cross(right, normal);

            right = Vector3.Scale(right, face.BlockExtents);
            up = Vector3.Scale(up, face.BlockExtents);
        }

        private void DrawFace(IEnumerable<Vector3> faceVertices)
        {
            material.SetPass(0);

            GL.Begin(GL.QUADS);
            GL.Color(new Color(1.0f, 1.0f, 1.0f, 0.2f));

            foreach (var v in faceVertices)
            {
                GL.Vertex(v);
            }

            GL.End();
        }

        private struct BlockFace
        {
            public Vector3 Center { get; set; }
            public Vector3 Normal { get; set; }
            public Vector3 BlockExtents { get; set; }
        }
    }
}