using System;
using UnityEngine;

namespace Minecraft
{
    /// <summary>
    /// A chunk of the terrain. Made of blocks (see TerrainBlock).
    /// The mesh for this chunk must be in its parent GameObject (see TerrainChunkGenerator).
    /// </summary>
    public sealed class TerrainChunk : MonoBehaviour
    {
        public Index3D Index { get; private set; }

        private Index3D firstBlockGlobalIndex;
        private TerrainBlock[,,] blocks;
        private TerrainConfig config;

        public void Setup(Index3D chunkIndex, TerrainConfig terrainConfig)
        {
            Index = chunkIndex;
            config = terrainConfig;

            // We include two extra "invisible blocks" for each dimension (one at the beginning and other at the end).
            blocks = new TerrainBlock[
                config.ChunkSizeInBlocks.x + 2,
                config.ChunkSizeInBlocks.y + 2,
                config.ChunkSizeInBlocks.z + 2
            ];

            firstBlockGlobalIndex = new Index3D(
                Index.X * config.ChunkSizeInBlocks.x - 1,
                Index.Y * config.ChunkSizeInBlocks.y - 1,
                Index.Z * config.ChunkSizeInBlocks.z - 1
            );
        }

        public Index3D GetBlockLocalIndexAt(Vector3 pointInWorld)
        {
            var pointInChunk = pointInWorld - GetMinVertex();

            return new Index3D(
                Mathf.FloorToInt(pointInChunk.x / config.BlockSize) - 1,
                Mathf.FloorToInt(pointInChunk.y / config.BlockSize) - 1,
                Mathf.FloorToInt(pointInChunk.z / config.BlockSize) - 1
            );
        }

        private Vector3 GetMinVertex()
        {
            return new Vector3(
                firstBlockGlobalIndex.X * config.BlockSize - config.HalfBlockSize,
                firstBlockGlobalIndex.Y * config.BlockSize - config.HalfBlockSize,
                firstBlockGlobalIndex.Z * config.BlockSize - config.HalfBlockSize
            );
        }

        public Index3D FirstVisibleBlockGlobalIndex
        {
            get
            {
                return new Index3D(firstBlockGlobalIndex.X + 1, firstBlockGlobalIndex.Y + 1, firstBlockGlobalIndex.Z + 1);
            }
        }

        public void SetBlock(Index3D blockLocalIndex, TerrainBlock block)
        {
            blocks[blockLocalIndex.X, blockLocalIndex.Y, blockLocalIndex.Z] = block;
        }

        public TerrainBlock GetBlock(Index3D blockLocalIndex)
        {
            return blocks[blockLocalIndex.X, blockLocalIndex.Y, blockLocalIndex.Z];
        }

        public bool IsBlockEmpty(Index3D blockLocalIndex)
        {
            return GetBlock(blockLocalIndex).IsEmpty();
        }

        public int FindGroundHeightAt(int xLocalIndex, int zLocalIndex)
        {
            var yLocalIndex = MaxBlockIndex.Y;

            while (yLocalIndex >= MinBlockIndex.Y && IsEmptyOrLeaves(blocks[xLocalIndex, yLocalIndex, zLocalIndex].BlockType))
            {
                --yLocalIndex;
            }

            return yLocalIndex;
        }

        private static bool IsEmptyOrLeaves(TerrainBlock.Type blockType)
        {
            return blockType == TerrainBlock.Type.None
                || blockType == TerrainBlock.Type.TreeLeaves;
        }

        public void SetBlockType(Index3D blockLocalIndex, TerrainBlock.Type newBlockType)
        {
            var newBlock = new TerrainBlock(newBlockType, blockLocalIndex, ToGlobalIndex(blockLocalIndex), config.BlockSize);
            blocks[blockLocalIndex.X, blockLocalIndex.Y, blockLocalIndex.Z] = newBlock;
        }

        public void ForEachGroundBlock(Action<TerrainBlock> action)
        {
            for (var x = MinBlockIndex.X; x <= MaxBlockIndex.X; ++x)
            {
                for (var z = MinBlockIndex.Z; z <= MaxBlockIndex.Z; ++z)
                {
                    var y = FindGroundHeightAt(x, z);
                    action(GetBlock(new Index3D(x, y, z)));
                }
            }
        }

        /// <summary>
        /// Does not include "invisible blocks". Used for creating the chunk's mesh. (See TerrainChunkGenerator.)
        /// </summary>
        public void ForEachNonEmptyVisibleBlock(Action<TerrainBlock> action)
        {
            for (var x = MinBlockIndex.X; x <= MaxBlockIndex.X; ++x)
            {
                for (var y = MinBlockIndex.Y; y <= MaxBlockIndex.Y; ++y)
                {
                    for (var z = MinBlockIndex.Z; z <= MaxBlockIndex.Z; ++z)
                    {
                        var b = blocks[x, y, z];

                        if (!b.IsEmpty())
                        {
                            action(b);
                        }
                    }
                }
            }
        }

        public bool IsInThisChunk(Index3D blockLocalIndex)
        {
            return blockLocalIndex.X >= MinBlockIndex.X && blockLocalIndex.X <= MaxBlockIndex.X
                && blockLocalIndex.Y >= MinBlockIndex.Y && blockLocalIndex.Y <= MaxBlockIndex.Y
                && blockLocalIndex.Z >= MinBlockIndex.Z && blockLocalIndex.Z <= MaxBlockIndex.Z;
        }

        public Index3D MinBlockIndex
        {
            get
            {
                return new Index3D(1, 1, 1);
            }
        }

        public Index3D MaxBlockIndex
        {
            get
            {
                return new Index3D(
                    blocks.GetLength(0) - 2,
                    blocks.GetLength(1) - 2,
                    blocks.GetLength(2) - 2
                );
            }
        }

        /// <summary>
        /// Includes "invisible blocks". Used for setting initial blocks. (See TerrainBlockGenerator.)
        /// </summary>
        public void ForEachBlockIndex(Action<Index3D, Index3D> action)
        {
            for (var x = 0; x < blocks.GetLength(0); ++x)
            {
                for (var y = 0; y < blocks.GetLength(1); ++y)
                {
                    for (var z = 0; z < blocks.GetLength(2); ++z)
                    {
                        var localIndex = new Index3D(x, y, z);
                        var globalIndex = ToGlobalIndex(localIndex);

                        action(localIndex, globalIndex);
                    }
                }
            }
        }

        private Index3D ToGlobalIndex(Index3D blockLocalIndex)
        {
            return new Index3D(
                blockLocalIndex.X + firstBlockGlobalIndex.X + 1,
                blockLocalIndex.Y + firstBlockGlobalIndex.Y + 1,
                blockLocalIndex.Z + firstBlockGlobalIndex.Z + 1
            );
        }
    }
}