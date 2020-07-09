using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Minecraft
{
    /// <summary>
    /// Class to help building a mesh for a terrain chunk.
    /// Generates a single mesh for the entire chunk.
    /// </summary>
    public sealed class TerrainChunkMeshBuilder
    {
        private readonly IList<Vector3> vertices = new List<Vector3>();
        private readonly IList<int> triangles = new List<int>();
        private readonly List<Vector2> uvs = new List<Vector2>();

        private readonly TerrainConfig config;

        public TerrainChunkMeshBuilder(TerrainConfig config)
        {
            this.config = config;
        }

        public Mesh BuildBlockMesh(TerrainBlock.Type blockType)
        {
            var blockMinVertex = new Vector3(
                -config.HalfBlockSize,
                -config.HalfBlockSize,
                -config.HalfBlockSize
            );

            var blockUVs = TerrainBlockUVs.Create(blockType);

            AddBlockFace_up(blockMinVertex, blockUVs);
            AddBlockFace_down(blockMinVertex, blockUVs);
            AddBlockFace_forward(blockMinVertex, blockUVs);
            AddBlockFace_right(blockMinVertex, blockUVs);
            AddBlockFace_back(blockMinVertex, blockUVs);
            AddBlockFace_left(blockMinVertex, blockUVs);

            return BuildMesh();
        }

        public Mesh BuildMesh(TerrainChunk chunk)
        {
            chunk.ForEachNonEmptyVisibleBlock(block =>
            {
                var blockLocalIndex = block.LocalIndex;

                var blockMinVertex = new Vector3(
                    blockLocalIndex.X * config.BlockSize - config.HalfBlockSize,
                    blockLocalIndex.Y * config.BlockSize - config.HalfBlockSize,
                    blockLocalIndex.Z * config.BlockSize - config.HalfBlockSize
                );

                var blockUVs = TerrainBlockUVs.Create(block.BlockType);

                if (chunk.IsBlockEmpty(blockLocalIndex.StepUp()))
                {
                    AddBlockFace_up(blockMinVertex, blockUVs);
                }

                if (chunk.IsBlockEmpty(blockLocalIndex.StepDown()))
                {
                    AddBlockFace_down(blockMinVertex, blockUVs);
                }

                if (chunk.IsBlockEmpty(blockLocalIndex.StepForward()))
                {
                    AddBlockFace_forward(blockMinVertex, blockUVs);
                }

                if (chunk.IsBlockEmpty(blockLocalIndex.StepRight()))
                {
                    AddBlockFace_right(blockMinVertex, blockUVs);
                }

                if (chunk.IsBlockEmpty(blockLocalIndex.StepBack()))
                {
                    AddBlockFace_back(blockMinVertex, blockUVs);
                }

                if (chunk.IsBlockEmpty(blockLocalIndex.StepLeft()))
                {
                    AddBlockFace_left(blockMinVertex, blockUVs);
                }
            });

            return BuildMesh();
        }

        private Mesh BuildMesh()
        {
            var mesh = new Mesh();

            //mesh.indexFormat = IndexFormat.UInt32; // (this can allow larger chunks)
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.RecalculateNormals();

            return mesh;
        }

        private void AddBlockFace_up(Vector3 blockMinVertex, TerrainBlockUVs blockUVs)
        {
            AddFacesForNextFourVertices();

            vertices.Add(blockMinVertex + new Vector3(0, 1, 0) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(0, 1, 1) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(1, 1, 1) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(1, 1, 0) * config.BlockSize);

            uvs.AddRange(blockUVs.TopUVs);
        }

        private void AddBlockFace_down(Vector3 blockMinVertex, TerrainBlockUVs blockUVs)
        {
            AddFacesForNextFourVertices();

            vertices.Add(blockMinVertex + new Vector3(0, 0, 0) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(1, 0, 0) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(1, 0, 1) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(0, 0, 1) * config.BlockSize);

            uvs.AddRange(blockUVs.BottomUVs);
        }

        private void AddBlockFace_forward(Vector3 blockMinVertex, TerrainBlockUVs blockUVs)
        {
            AddFacesForNextFourVertices();

            vertices.Add(blockMinVertex + new Vector3(1, 0, 1) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(1, 1, 1) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(0, 1, 1) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(0, 0, 1) * config.BlockSize);

            uvs.AddRange(blockUVs.SideUVs);
        }

        private void AddBlockFace_right(Vector3 blockMinVertex, TerrainBlockUVs blockUVs)
        {
            AddFacesForNextFourVertices();

            vertices.Add(blockMinVertex + new Vector3(1, 0, 0) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(1, 1, 0) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(1, 1, 1) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(1, 0, 1) * config.BlockSize);

            uvs.AddRange(blockUVs.SideUVs);
        }

        private void AddBlockFace_back(Vector3 blockMinVertex, TerrainBlockUVs blockUVs)
        {
            AddFacesForNextFourVertices();

            vertices.Add(blockMinVertex + new Vector3(0, 0, 0) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(0, 1, 0) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(1, 1, 0) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(1, 0, 0) * config.BlockSize);

            uvs.AddRange(blockUVs.SideUVs);
        }

        private void AddBlockFace_left(Vector3 blockMinVertex, TerrainBlockUVs blockUVs)
        {
            AddFacesForNextFourVertices();

            vertices.Add(blockMinVertex + new Vector3(0, 0, 1) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(0, 1, 1) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(0, 1, 0) * config.BlockSize);
            vertices.Add(blockMinVertex + new Vector3(0, 0, 0) * config.BlockSize);

            uvs.AddRange(blockUVs.SideUVs);
        }

        private void AddFacesForNextFourVertices()
        {
            triangles.Add(vertices.Count);
            triangles.Add(vertices.Count + 1);
            triangles.Add(vertices.Count + 2);
            triangles.Add(vertices.Count);
            triangles.Add(vertices.Count + 2);
            triangles.Add(vertices.Count + 3);
        }
    }
}
