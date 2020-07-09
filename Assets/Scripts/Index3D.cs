using System;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft
{
    /// <summary>
    /// Can be used to index a block or a chunk on the terrain.
    /// </summary>
    public struct Index3D : IEquatable<Index3D>
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }

        public static Index3D InvalidIndex
        {
            get
            {
                return new Index3D(int.MinValue, int.MinValue, int.MinValue);
            }
        }

        public Index3D(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(Index3D other)
        {
            return X == other.X
                && Y == other.Y
                && Z == other.Z;
        }

        public Index3D StepUp() { return Step(Vector3Int.up); }
        public Index3D StepDown() { return Step(Vector3Int.down); }
        public Index3D StepForward() { return Step(ToVector3Int(Vector3.forward)); }
        public Index3D StepBack() { return Step(ToVector3Int(Vector3.back)); }
        public Index3D StepRight() { return Step(Vector3Int.right); }
        public Index3D StepLeft() { return Step(Vector3Int.left); }

        public Index3D Step(Vector3 step)
        {
            return Step(ToVector3Int(step));
        }

        public Index3D Step(Vector3Int step)
        {
            return new Index3D(X + step.x, Y + step.y, Z + step.z);
        }

        private static Vector3Int ToVector3Int(Vector3 v)
        {
            return new Vector3Int((int)v.x, (int)v.y, (int)v.z);
        }

        public Vector3 AsVector3()
        {
            return new Vector3(X, Y, Z);
        }

        public IEnumerable<Index3D> GetIndicesAround(Vector3Int radius)
        {
            for (var x = X - radius.x; x <= X + radius.x; ++x)
            {
                for (var y = Y - radius.y; y <= Y + radius.y; ++y)
                {
                    for (var z = Z - radius.z; z <= Z + radius.z; ++z)
                    {
                        yield return new Index3D(x, y, z);
                    }
                }
            }
        }
    }
}