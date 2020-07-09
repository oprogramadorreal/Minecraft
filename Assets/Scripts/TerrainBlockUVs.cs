using System.Collections.Generic;
using UnityEngine;

namespace Minecraft
{
    public sealed class TerrainBlockUVs
    {
        public Vector2[] TopUVs { get; }
        public Vector2[] SideUVs { get; }
        public Vector2[] BottomUVs { get; }

        private static readonly Dictionary<TileType, Vector2[]> tileTypeToUVs = new Dictionary<TileType, Vector2[]>()
        {
            { TileType.Dirt, GetTileUVCoords(0, 0) },
            { TileType.Grass, GetTileUVCoords(1, 0) },
            { TileType.GrassAndDirt, GetTileUVCoords(0, 1) },
            { TileType.Stone, GetTileUVCoords(3, 0) },
            { TileType.TreeTrunk, GetTileUVCoords(2, 0) },
            { TileType.TreeTrunkTip, GetTileUVCoords(2, 1) },
            { TileType.TreeLeaves, GetTileUVCoords(1, 1) }
        };

        private static readonly Dictionary<TerrainBlock.Type, TerrainBlockUVs> blockTypeToUVs = new Dictionary<TerrainBlock.Type, TerrainBlockUVs>()
        {
            { TerrainBlock.Type.Dirt, new TerrainBlockUVs(TileType.Dirt) },
            { TerrainBlock.Type.Grass, new TerrainBlockUVs(TileType.Grass, TileType.GrassAndDirt, TileType.Dirt) },
            { TerrainBlock.Type.Stone, new TerrainBlockUVs(TileType.Stone) },
            { TerrainBlock.Type.TreeTrunk, new TerrainBlockUVs(TileType.TreeTrunkTip, TileType.TreeTrunk, TileType.TreeTrunkTip) },
            { TerrainBlock.Type.TreeLeaves, new TerrainBlockUVs(TileType.TreeLeaves) }
        };

        private TerrainBlockUVs(TileType tile)
            : this(tile, tile, tile)
        { }

        private TerrainBlockUVs(TileType topTile, TileType sideTile, TileType bottomTile)
        {
            TopUVs = tileTypeToUVs[topTile];
            SideUVs = tileTypeToUVs[sideTile];
            BottomUVs = tileTypeToUVs[bottomTile];
        }

        public static TerrainBlockUVs Create(TerrainBlock.Type blockType)
        {
            return blockTypeToUVs[blockType];
        }

        private static Vector2[] GetTileUVCoords(int uOffset, int vOffset)
        {
            const float textureSize = 128;
            const float tileSize = 32;
            const float offsetFix = 0.001f;

            var u = uOffset * tileSize;
            var v = vOffset * tileSize;

            return new Vector2[]
            {
                new Vector2(u / textureSize + offsetFix, v / textureSize + offsetFix),
                new Vector2(u / textureSize + offsetFix, (v + tileSize) / textureSize - offsetFix),
                new Vector2((u + tileSize) / textureSize - offsetFix, (v + tileSize) / textureSize - offsetFix),
                new Vector2((u + tileSize) / textureSize - offsetFix, v / textureSize + offsetFix),
            };
        }

        private enum TileType
        {
            Dirt = 0,
            Grass,
            GrassAndDirt,
            Stone,
            TreeTrunk,
            TreeTrunkTip,
            TreeLeaves
        }
    }
}