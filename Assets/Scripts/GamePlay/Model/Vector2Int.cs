using System;

namespace GamePlay
{
    public sealed class Vector2Int : IEquatable<Vector2Int>
    {
        public int X { get; }
        public int Y { get; }

        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vector2Int(Vector2Int vector)
        {
            X = vector.X;
            Y = vector.Y;
        }

        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        public bool Equals(Vector2Int other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Vector2Int);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }

        public static bool operator ==(Vector2Int left, Vector2Int right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(Vector2Int left, Vector2Int right)
        {
            return !(left == right);
        }

        public static Vector2Int operator +(Vector2Int left, Vector2Int right)
        {
            return new Vector2Int(left.X + right.X, left.Y + right.Y);
        }

        public static Vector2Int operator *(int scalar, Vector2Int vector)
        {
            return new Vector2Int(scalar * vector.X, scalar * vector.Y);
        }

        public static Vector2Int operator -(Vector2Int left, Vector2Int right)
        {
            return new Vector2Int(left.X - right.X, left.Y - right.Y);
        }

        public static Vector2Int operator -(Vector2Int vector)
        {
            return new Vector2Int(-vector.X, -vector.Y);
        }
    }
}
