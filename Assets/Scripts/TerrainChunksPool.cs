using System.Collections.Generic;
using UnityEngine;

namespace Minecraft
{
    public sealed class TerrainChunksPool : MonoBehaviour
    {
        [SerializeField]
        private GameObject chunkPrefab;

        private readonly Dictionary<Index3D, TerrainChunk> currentChunks = new Dictionary<Index3D, TerrainChunk>();

        private readonly Queue<TerrainChunk> deactivatedChunks = new Queue<TerrainChunk>();

        public TerrainChunk Instantiate(Index3D chunkIndex, Transform parent)
        {
            TerrainChunk newChunk;

            if (deactivatedChunks.Count == 0)
            {
                var chunkObject = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, parent);
                newChunk = chunkObject.GetComponent<TerrainChunk>();
            }
            else
            {
                newChunk = deactivatedChunks.Dequeue();
                newChunk.gameObject.SetActive(true);
            }

            currentChunks[chunkIndex] = newChunk;

            return newChunk;
        }

        public void Deactivate(IEnumerable<Index3D> chunksToDestroy)
        {
            foreach (var chunkIndex in chunksToDestroy)
            {
                var chunk = GetChunk(chunkIndex);

                if (chunk != null)
                {
                    chunk.gameObject.SetActive(false);
                    deactivatedChunks.Enqueue(chunk);
                }
            }
        }

        public TerrainChunk GetChunk(Index3D chunkIndex)
        {
            return currentChunks.TryGetValue(chunkIndex, out TerrainChunk chunk) ? chunk : null;
        }
    }
}