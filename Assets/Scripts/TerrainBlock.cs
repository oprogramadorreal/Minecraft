using UnityEngine;

namespace Minecraft
{
    public sealed class TerrainBlock
    {
        public TerrainBlock(Type type, Index3D localIndex, Index3D globalIndex, float size)
        {
            BlockType = type;
            LocalIndex = localIndex;
            GlobalIndex = globalIndex;
            Size = size;
        }

        /// <summary>
        /// The index of the block in local (or object) space (relative index inside its chunk).
        /// </summary>
        public Index3D LocalIndex { get; private set; }

        public Index3D GlobalIndex { get; private set; }

        public Type BlockType { get; private set; }

        public float Size { get; private set; }

        public Vector3 SizeAsVector3
        {
            get
            {
                return new Vector3(Size, Size, Size);
            }
        }

        public Vector3 Center
        {
            get
            {
                return Vector3.Scale(GlobalIndex.AsVector3(), SizeAsVector3);
            }
        }

        public bool IsEmpty()
        {
            return BlockType == Type.None;
        }

        public Bounds GetBounds()
        {
            return new Bounds(Center, SizeAsVector3);
        }

        public enum Type
        {
            None = 0, // aka "Air" block
            Dirt,
            Grass,
            Stone,
            TreeTrunk,
            TreeLeaves
        }
    }
}