using System.Collections.Generic;
using UnityEngine;

namespace Minecraft
{
    /// <summary>
    /// Knows how to generate a terrain chunk.
    /// </summary>
    public sealed class TerrainChunkGenerator
    {
        private readonly TerrainBlocksGenerator blocksGenerator;
        private readonly TerrainChunksPool chunksPool;
        private readonly TerrainConfig config;

        public TerrainChunkGenerator(TerrainBlocksGenerator blocksGenerator, TerrainChunksPool chunksPool, TerrainConfig config)
        {
            this.blocksGenerator = blocksGenerator;
            this.chunksPool = chunksPool;
            this.config = config;
        }

        public void Generate(Index3D chunkIndex, Transform chunkParent)
        {
            var chunk = InstantiateAndSetup(chunkIndex, chunkParent);
            blocksGenerator.GenerateBlocksFor(chunk, config.BlockSize);
            BuildMeshFor(chunk);
        }

        public TerrainChunk GetOrGenerateEmpty(Index3D chunkIndex, Transform chunkParent)
        {
            var chunk = chunksPool.GetChunk(chunkIndex);

            if (chunk == null)
            {
                chunk = InstantiateAndSetup(chunkIndex, chunkParent);
                blocksGenerator.GenerateEmptyBlocksFor(chunk, config.BlockSize);
            }

            return chunk;
        }

        private TerrainChunk InstantiateAndSetup(Index3D chunkIndex, Transform chunkParent)
        {
            var chunk = chunksPool.Instantiate(chunkIndex, chunkParent);
            chunk.Setup(chunkIndex, config);
            return chunk;
        }

        public void BuildMeshFor(TerrainChunk chunk)
        {
            var meshBuilder = new TerrainChunkMeshBuilder(config);
            var mesh = meshBuilder.BuildMesh(chunk);

            var chunkObject = chunk.gameObject;
            chunkObject.GetComponent<MeshFilter>().mesh = mesh;
            chunkObject.GetComponent<MeshCollider>().sharedMesh = mesh;

            chunkObject.transform.position = chunk.FirstVisibleBlockGlobalIndex.AsVector3() * config.BlockSize;

            if (chunk.Index.Y == 0) // generates water only for chunks at the ground level
            {
                var waterMesh = new WaterMeshBuilder(config).BuildMesh(chunk);

                if (waterMesh != null)
                {
                    var waterObject = chunkObject.transform.GetChild(0);
                    waterObject.transform.localPosition = new Vector3(0, config.WaterLevelInBlocks * config.BlockSize - config.HalfBlockSize * 1.3f, 0);
                    waterObject.GetComponent<MeshFilter>().mesh = waterMesh;
                }
            }
        }

        private sealed class WaterMeshBuilder
        {
            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<int> triangles = new List<int>();
            private readonly List<Vector2> uvs = new List<Vector2>();

            private readonly TerrainConfig config;

            public WaterMeshBuilder(TerrainConfig config)
            {
                this.config = config;
            }

            public Mesh BuildMesh(TerrainChunk chunk)
            {
                chunk.ForEachGroundBlock(block =>
                {
                    if (block.LocalIndex.Y < config.WaterLevelInBlocks)
                    {
                        var x = block.LocalIndex.X * config.BlockSize - config.HalfBlockSize;
                        var z = block.LocalIndex.Z * config.BlockSize - config.HalfBlockSize;

                        triangles.Add(vertices.Count);
                        triangles.Add(vertices.Count + 1);
                        triangles.Add(vertices.Count + 2);
                        triangles.Add(vertices.Count);
                        triangles.Add(vertices.Count + 2);
                        triangles.Add(vertices.Count + 3);

                        vertices.Add(new Vector3(x, 0, z));
                        vertices.Add(new Vector3(x, 0, z + config.BlockSize));
                        vertices.Add(new Vector3(x + config.BlockSize, 0, z + config.BlockSize));
                        vertices.Add(new Vector3(x + config.BlockSize, 0, z));

                        uvs.AddRange(new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) });
                    }
                });

                return BuildMesh();
            }

            private Mesh BuildMesh()
            {
                Mesh mesh = null;

                if (vertices.Count > 0)
                {
                    mesh = new Mesh();

                    mesh.vertices = vertices.ToArray();
                    mesh.triangles = triangles.ToArray();
                    mesh.uv = uvs.ToArray();

                    mesh.RecalculateNormals();
                }

                return mesh;
            }
        }
    }
}