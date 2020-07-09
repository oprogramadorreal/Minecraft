using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Minecraft
{
    /// <summary>
    /// This is the main script of the game.
    /// It controls all terrain generation related stuff.
    /// </summary>
    public sealed class TerrainManager : MonoBehaviour
    {
        [SerializeField]
        private Transform player;

        private GameObject terrainObject;

        private TerrainConfig config;
        private TerrainChunksPool chunksPool;
        private TerrainChunkGenerator chunkGenerator;
        private TerrainModifier modifier;

        private Index3D playerCurrentChunk = Index3D.InvalidIndex;
        private readonly HashSet<Index3D> currentChunks = new HashSet<Index3D>();

        private bool updatingTerrain = false;

        private void Start()
        {
            terrainObject = new GameObject("Terrain")
            {
                isStatic = true
            };

            config = GetComponent<TerrainConfig>();
            var blocksGenerator = GetComponent<TerrainBlocksGenerator>();
            chunksPool = GetComponent<TerrainChunksPool>();

            chunkGenerator = new TerrainChunkGenerator(blocksGenerator, chunksPool, config);

            modifier = GetComponent<TerrainModifier>();
            modifier.Setup(chunkGenerator, terrainObject.transform);

            UpdateTerrain()
                .ContinueWith(_ => Debug.Log("First load finished!"));
        }

        private async void Update()
        {
            await UpdateTerrain();
        }

        private async Task UpdateTerrain()
        {
            if (!updatingTerrain)
            {
                var newPlayerChunk = config.GetChunkIndexAt(player.position);

                if (!newPlayerChunk.Equals(playerCurrentChunk))
                {
                    var newCurrentChunks = CalculateChunksAround(newPlayerChunk);

                    var chunksToDestroy = currentChunks.Except(newCurrentChunks);
                    var chunksToCreate = newCurrentChunks.Except(currentChunks);

                    chunksPool.Deactivate(chunksToDestroy);

                    updatingTerrain = true;

                    await GenerateChunks(chunksToCreate)
                        .ContinueWith(_ =>
                        {
                            currentChunks.Clear();
                            currentChunks.UnionWith(newCurrentChunks);
                            playerCurrentChunk = newPlayerChunk;
                            updatingTerrain = false;
                        });
                }
            }
        }

        private static IEnumerable<Index3D> CalculateChunksAround(Index3D chunkIndex)
        {
            const int maxHeight = 0;
            const int activeChunksRadius = 10;

            return chunkIndex
                .GetIndicesAround(new Vector3Int(activeChunksRadius, 0, activeChunksRadius))
                .Select(i => new Index3D(i.X, System.Math.Min(i.Y, maxHeight), i.Z));
        }

        private async Task GenerateChunks(IEnumerable<Index3D> chunksToCreate)
        {
            foreach (var chunkIndex in chunksToCreate)
            {
                chunkGenerator.Generate(chunkIndex, terrainObject.transform);
                await Task.Yield();
            }
        }

        public TerrainBlock.Type? RemoveBlock(Ray ray)
        {
            return modifier.RemoveBlock(ray);
        }

        public TerrainBlock AddBlock(Ray ray, TerrainBlock.Type blockType)
        {
            return modifier.AddBlock(ray, blockType);
        }

        public PointOnTerrainMesh RaycastTerrainMesh(Ray ray, float offset)
        {
            return config.RaycastTerrainMesh(ray, offset);
        }

        public TerrainBlock GetBlockAt(Vector3 pointInWorld)
        {
            return modifier.GetBlockAt(pointInWorld);
        }

        public void MakeSimulatedBlock(Ray viewRay, bool addForce)
        {
            modifier.MakeSimulatedBlock(viewRay, addForce);
        }
    }
}