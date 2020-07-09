using System.Collections.Generic;
using UnityEngine;

namespace Minecraft
{
    public sealed class TerrainModifier : MonoBehaviour
    {
        private TerrainConfig config;
        private ObjectsPool rigidbodiesPool;
        private TerrainChunkGenerator chunkGenerator;
        private Transform chunksParent;

        private void Start()
        {
            config = GetComponent<TerrainConfig>();
            rigidbodiesPool = GetComponent<ObjectsPool>();
        }

        public void Setup(TerrainChunkGenerator chunkGenerator, Transform chunksParent)
        {
            this.chunkGenerator = chunkGenerator;
            this.chunksParent = chunksParent;
        }

        public void MakeSimulatedBlock(Ray viewRay, bool addForce)
        {
            var pointOnTerrainMesh = config.RaycastTerrainMesh(viewRay, 0.01f);

            if (pointOnTerrainMesh != null)
            {
                TerrainChunk chunk;
                var block = GetBlockAt(pointOnTerrainMesh.Point, out chunk);

                if (block.BlockType != TerrainBlock.Type.None)
                {
                    var removedBlockType = SetBlockTypeAt(chunk, block.LocalIndex, TerrainBlock.Type.None);
                    SetTypeOfSameBlockInAdjacentChunks(chunk, block.LocalIndex, pointOnTerrainMesh.Point, TerrainBlock.Type.None);

                    var rigidbody = CreateBlockRigidbody(removedBlockType, block.Center);

                    if (addForce)
                    {
                        var force = CalculateForce(block.Center);
                        rigidbody.AddForce(force, ForceMode.Impulse);
                        //rigidbody.AddRelativeTorque(CalculateTorque(), ForceMode.Force);
                    }
                }
            }
        }

        private Rigidbody CreateBlockRigidbody(TerrainBlock.Type blockType, Vector3 position)
        {
            var rigidbodyObject = rigidbodiesPool.Instantiate(position);
            rigidbodyObject.GetComponent<MeshFilter>().mesh = BuildBlockMesh(blockType);
            return rigidbodyObject.GetComponent<Rigidbody>();
        }

        private Vector3 CalculateForce(Vector3 blockPosition)
        {
            if (IsBlockEmpty(blockPosition + Vector3.down * config.BlockSize))
            {
                return Vector3.zero;
            }

            var forceMagnitude = Random.Range(10.0f, 20.0f);

            if (IsBlockEmpty(blockPosition + Vector3.up * config.BlockSize))
            {
                return Vector3.up * forceMagnitude;
            }

            var force = Vector3.zero;

            if (IsBlockEmpty(blockPosition + Vector3.left * config.BlockSize))
            {
                force += Vector3.left;
            }
            else if (IsBlockEmpty(blockPosition + Vector3.right * config.BlockSize))
            {
                force += Vector3.right;
            }

            if (IsBlockEmpty(blockPosition + Vector3.back * config.BlockSize))
            {
                force += Vector3.back;
            }
            else if (IsBlockEmpty(blockPosition + Vector3.forward * config.BlockSize))
            {
                force += Vector3.forward;
            }

            if (force.sqrMagnitude >= 0.01)
            {
                force.Normalize();
            }

            return force * forceMagnitude;
        }

        private bool IsBlockEmpty(Vector3 pointInWorld)
        {
            return GetBlockAt(pointInWorld)?.IsEmpty() ?? false;
        }

        //private static Vector3 CalculateTorque()
        //{
        //    return new Vector3(
        //        Random.Range(-50.0f, 50.0f),
        //        Random.Range(-50.0f, 50.0f),
        //        Random.Range(-50.0f, 50.0f)
        //    );
        //}

        private Mesh BuildBlockMesh(TerrainBlock.Type blockType)
        {
            return new TerrainChunkMeshBuilder(config).BuildBlockMesh(blockType);
        }

        public TerrainBlock AddBlock(Ray ray, TerrainBlock.Type blockType)
        {
            TerrainBlock block = null;
            var pointOnTerrainMesh = config.RaycastTerrainMesh(ray, -0.01f)?.Point;

            if (pointOnTerrainMesh.HasValue)
            {
                block = GetBlockAt(pointOnTerrainMesh.Value, out TerrainChunk chunk);

                SetBlockTypeAt(chunk, block.LocalIndex, blockType);
                SetTypeOfSameBlockInAdjacentChunks(chunk, block.LocalIndex, pointOnTerrainMesh.Value, blockType);
            }

            return block;
        }

        public TerrainBlock.Type? RemoveBlock(Ray ray)
        {
            TerrainBlock.Type? removedBlockType = null;

            var pointOnTerrainMesh = config.RaycastTerrainMesh(ray, 0.01f)?.Point;

            if (pointOnTerrainMesh.HasValue)
            {
                var block = GetBlockAt(pointOnTerrainMesh.Value, out TerrainChunk chunk);

                removedBlockType = SetBlockTypeAt(chunk, block.LocalIndex, TerrainBlock.Type.None);
                SetTypeOfSameBlockInAdjacentChunks(chunk, block.LocalIndex, pointOnTerrainMesh.Value, TerrainBlock.Type.None);
            }

            return removedBlockType;
        }

        public TerrainBlock GetBlockAt(Vector3 pointInWorld)
        {
            return GetBlockAt(pointInWorld, out _);
        }

        private TerrainBlock GetBlockAt(Vector3 pointInWorld, out TerrainChunk chunk)
        {
            var chunkIndex = config.GetChunkIndexAt(pointInWorld);
            chunk = chunkGenerator.GetOrGenerateEmpty(chunkIndex, chunksParent);
            var blockLocalIndex = chunk.GetBlockLocalIndexAt(pointInWorld);

            return chunk.GetBlock(blockLocalIndex);
        }

        private void SetTypeOfSameBlockInAdjacentChunks(TerrainChunk chunk, Index3D blockLocalIndex, Vector3 pointOnTerrainMesh, TerrainBlock.Type blockNewType)
        {
            foreach (var adjacentChunk in GetAdjacentChunksThatShareBlock(chunk, blockLocalIndex))
            {
                SetBlockTypeAt(adjacentChunk, adjacentChunk.GetBlockLocalIndexAt(pointOnTerrainMesh), blockNewType);
            }
        }

        /// <summary>
        /// This is highly dependent on TerrainChunk internal details. (See "GetDirectionsToLook" method.)
        /// A TerrainChunk stores some blocks of adjacent chunks in order to simplify mesh generation.
        /// Therefore, when we change a block type, we have to update the block type in adjacent chunks as well.
        /// This method gathers the adjacent chunks that need to be updated.
        /// </summary>
        private IEnumerable<TerrainChunk> GetAdjacentChunksThatShareBlock(TerrainChunk chunk, Index3D blockLocalIndex)
        {
            foreach (var d in GetDirectionsToLook(chunk, blockLocalIndex))
            {
                yield return chunkGenerator.GetOrGenerateEmpty(chunk.Index.Step(d), chunksParent);
            }
        }

        private IEnumerable<Vector3> GetDirectionsToLook(TerrainChunk chunk, Index3D blockLocalIndex)
        {
            var directionsToUpdate = new List<Vector3>();

            if (blockLocalIndex.X <= chunk.MinBlockIndex.X)
            {
                directionsToUpdate.Add(Vector3.left);
            }
            else if (blockLocalIndex.X >= chunk.MaxBlockIndex.X)
            {
                directionsToUpdate.Add(Vector3.right);
            }

            if (blockLocalIndex.Y <= chunk.MinBlockIndex.Y)
            {
                directionsToUpdate.Add(Vector3.down);
            }
            else if (blockLocalIndex.Y >= chunk.MaxBlockIndex.Y)
            {
                directionsToUpdate.Add(Vector3.up);
            }

            if (blockLocalIndex.Z <= chunk.MinBlockIndex.Z)
            {
                directionsToUpdate.Add(Vector3.back);
            }
            else if (blockLocalIndex.Z >= chunk.MaxBlockIndex.Z)
            {
                directionsToUpdate.Add(Vector3.forward);
            }

            var numberOfDirections = directionsToUpdate.Count;

            for (var i = 0; i < Mathf.CeilToInt(numberOfDirections / 2.0f); ++i)
            {
                for (var j = i + 1; j < numberOfDirections; ++j)
                {
                    directionsToUpdate.Add(directionsToUpdate[i] + directionsToUpdate[j]);
                }
            }

            return directionsToUpdate;
        }

        private TerrainBlock.Type SetBlockTypeAt(TerrainChunk chunk, Index3D blockLocalIndex, TerrainBlock.Type blockNewType)
        {
            var blockType = chunk.GetBlock(blockLocalIndex).BlockType;
            chunk.SetBlockType(blockLocalIndex, blockNewType);
            chunkGenerator.BuildMeshFor(chunk);
            return blockType;
        }
    }
}