using UnityEngine;

namespace Minecraft
{
    public sealed class TerrainBlocksGenerator : MonoBehaviour
    {
        private readonly FastNoise noise = new FastNoise();

        private TerrainConfig config;

        private void Awake()
        {
            config = GetComponentInParent<TerrainConfig>();
        }

        public void GenerateBlocksFor(TerrainChunk chunk, float blocksSize)
        {
            chunk.ForEachBlockIndex(
                (localIndex, globalIndex) =>
                {
                    var blockType = CalculateBlockType(globalIndex);
                    var block = new TerrainBlock(blockType, localIndex, globalIndex, blocksSize);
                    chunk.SetBlock(localIndex, block);
                }
            );

            new TerrainTreesGenerator(config, chunk).Generate(noise);
        }

        public void GenerateEmptyBlocksFor(TerrainChunk chunk, float blocksSize)
        {
            chunk.ForEachBlockIndex(
                (localIndex, globalIndex) =>
                {
                    var block = new TerrainBlock(TerrainBlock.Type.None, localIndex, globalIndex, blocksSize);
                    chunk.SetBlock(localIndex, block);
                }
            );
        }

        /// <summary>
        /// This is based on Sam's Hogan code: https://github.com/samhogan/Minecraft-Unity3D
        /// </summary>
        private TerrainBlock.Type CalculateBlockType(Index3D blockGlobalIndex)
        {
            var pos = blockGlobalIndex.AsVector3() * config.BlockSize;

            var simplex1 = noise.GetSimplex(pos.x * 0.8f, pos.z * 0.8f) * 10;
            var simplex2 = noise.GetSimplex(pos.x * 3.0f, pos.z * 3.0f) * 10 * (noise.GetSimplex(pos.x * 0.3f, pos.z * 0.3f) + 0.5f);

            var heightMap = simplex1 + simplex2;

            var chunkHeight = config.ChunkSize.y;

            // add the 2d perlinNoise to the middle of the terrain chunk
            float baseLandHeight = chunkHeight * 0.5f + heightMap;

            // 3d perlin noise for caves and overhangs and such
            var caveNoise1 = noise.GetPerlinFractal(pos.x * 5.0f, pos.y * 10.0f, pos.z * 5.0f);
            var caveMask = noise.GetSimplex(pos.x * 0.3f, pos.z * 0.3f) + 0.3f;

            // stone layer heightmap
            var simplexStone1 = noise.GetSimplex(pos.x * 1.0f, pos.z * 1.0f) * 10;
            var simplexStone2 = (noise.GetSimplex(pos.x * 5.0f, pos.z * 5.0f) + 0.5f) * 20 * (noise.GetSimplex(pos.x * 0.3f, pos.z * 0.3f) + 0.5f);

            var stoneHeightMap = simplexStone1 + simplexStone2;
            var baseStoneHeight = chunkHeight * 0.25f + stoneHeightMap;

            var blockType = TerrainBlock.Type.None;

            // under the surface, dirt block
            if (pos.y <= baseLandHeight)
            {
                blockType = TerrainBlock.Type.Dirt;

                // just on the surface, use a grass type
                if (pos.y > baseLandHeight - 1 && pos.y > config.WaterLevelInBlocks * config.BlockSize - 2)
                {
                    blockType = TerrainBlock.Type.Grass;
                }

                if (pos.y <= baseStoneHeight)
                {
                    blockType = TerrainBlock.Type.Stone;
                }
            }

            if (caveNoise1 > Mathf.Max(caveMask, 0.2f))
            {
                blockType = TerrainBlock.Type.None;
            }

            return blockType;
        }
    }
}