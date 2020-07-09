using UnityEngine;

namespace Minecraft
{
    public sealed class TerrainConfig : MonoBehaviour
    {
        [SerializeField]
        private Vector3Int chunkSizeInBlocks = new Vector3Int(8, 64, 8);

        [SerializeField]
        private int waterLevelInBlocks = 27;

        [SerializeField]
        private float blockSize = 1.0f;

        public Vector3Int ChunkSizeInBlocks
        {
            get
            {
                return chunkSizeInBlocks;
            }
        }

        public int WaterLevelInBlocks
        {
            get
            {
                return waterLevelInBlocks;
            }
        }

        public float BlockSize
        {
            get
            {
                return blockSize;
            }
        }

        public float HalfBlockSize
        {
            get
            {
                return BlockSize / 2.0f;
            }
        }

        public Vector3 ChunkSize
        {
            get
            {
                return new Vector3(
                    ChunkSizeInBlocks.x * BlockSize,
                    ChunkSizeInBlocks.y * BlockSize,
                    ChunkSizeInBlocks.z * BlockSize
                );
            }
        }

        public Index3D GetChunkIndexAt(Vector3 pointInWorld)
        {
            var chunkSize = ChunkSize;

            return new Index3D(
                Mathf.FloorToInt(pointInWorld.x / chunkSize.x),
                Mathf.FloorToInt(pointInWorld.y / chunkSize.y),
                Mathf.FloorToInt(pointInWorld.z / chunkSize.z)
            );
        }

        public PointOnTerrainMesh RaycastTerrainMesh(Ray ray, float offset)
        {
            const float maxDistance = 3.0f;

            PointOnTerrainMesh pointOnTerrainMesh = null;

            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, maxDistance))
            {
                var point = hitInfo.point + ray.direction * (BlockSize * offset);
                pointOnTerrainMesh = new PointOnTerrainMesh(point, hitInfo.normal, ray);
            }

            return pointOnTerrainMesh;
        }
    }
}