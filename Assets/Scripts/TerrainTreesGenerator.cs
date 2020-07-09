using System.Collections.Generic;
using System.Linq;

namespace Minecraft
{
    /// <summary>
    /// Generates trees for one terrain chunk.
    /// </summary>
    public sealed class TerrainTreesGenerator
    {
        private readonly TerrainConfig config;
        private readonly TerrainChunk chunk;
        private readonly System.Random rand;

        public TerrainTreesGenerator(TerrainConfig config, TerrainChunk chunk)
        {
            this.config = config;
            this.chunk = chunk;

            rand = new System.Random(chunk.Index.X * 10000 + chunk.Index.Z);
        }

        /// <summary>
        /// This was also inspired by Sam's code: https://github.com/samhogan/Minecraft-Unity3D
        /// However, I've changed the algorithm to fix an issue with trees too close to the
        /// border of the chunk. I calculate the size of the leaves first, then I use this size
        /// to constrain where the trunk can be inside the chunk: I avoid placing it too close
        /// to the border of the chunk.
        /// </summary>
        public void Generate(FastNoise noise)
        {
            var simplex = noise.GetSimplex(chunk.Index.X * 0.8f, chunk.Index.Z * 0.8f);

            if (simplex > 0.0f)
            {
                var numberOfTrees = rand.Next(0, (int)(10.0f * simplex));

                for (var i = 0; i < numberOfTrees; ++i)
                {
                    var treeHeight = rand.Next(3, 14);
                    var leavesWidth = rand.Next(2, 8);
                    var leavesHalfWidth = leavesWidth / 2;

                    var trunkX = rand.Next(chunk.MinBlockIndex.X + leavesHalfWidth, chunk.MaxBlockIndex.X - leavesHalfWidth + 1);
                    var trunkZ = rand.Next(chunk.MinBlockIndex.Z + leavesHalfWidth, chunk.MaxBlockIndex.Z - leavesHalfWidth + 1);

                    var trunkBlocks = CalculateTreeTrunkIndices(treeHeight, trunkX, trunkZ).ToList();

                    if (trunkBlocks.Count() > 0)
                    {
                        foreach (var index in trunkBlocks)
                        {
                            chunk.SetBlockType(index, TerrainBlock.Type.TreeTrunk);
                        }

                        var trunkBaseIndex = trunkBlocks.FirstOrDefault();

                        foreach (var index in CalculateTreeLeavesIndices(trunkBaseIndex, treeHeight, leavesHalfWidth))
                        {
                            chunk.SetBlockType(index, TerrainBlock.Type.TreeLeaves);
                        }
                    }
                }
            }
        }

        private IEnumerable<Index3D> CalculateTreeTrunkIndices(int treeHeight, int trunkX, int trunkZ)
        {
            var y = chunk.FindGroundHeightAt(trunkX, trunkZ) + 1;

            for (var j = 0; j < treeHeight; ++j)
            {
                if (y + j < config.ChunkSizeInBlocks.y)
                {
                    yield return new Index3D(trunkX, y + j, trunkZ);
                }
            }
        }

        private IEnumerable<Index3D> CalculateTreeLeavesIndices(Index3D trunkBaseIndex, int treeHeight, int leavesHalfWidth)
        {
            var iterations = 0;

            for (var y = trunkBaseIndex.Y + treeHeight - 1; y <= trunkBaseIndex.Y + treeHeight - 1 + treeHeight; ++y)
            {
                var halfIterations = iterations / 2;

                for (var x = trunkBaseIndex.X - leavesHalfWidth + halfIterations; x <= trunkBaseIndex.X + leavesHalfWidth - halfIterations; ++x)
                {
                    for (var z = trunkBaseIndex.Z - leavesHalfWidth + halfIterations; z <= trunkBaseIndex.Z + leavesHalfWidth - halfIterations; ++z)
                    {
                        if (rand.NextDouble() < 0.8f)
                        {
                            var blockLocalIndex = new Index3D(x, y, z);

                            if (chunk.IsInThisChunk(blockLocalIndex))
                            {
                                yield return blockLocalIndex;
                            }
                        }
                    }
                }

                ++iterations;
            }
        }
    }
}