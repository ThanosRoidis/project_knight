using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shared
{
    public class Directions
    {
        public enum Direction
        {
            None, Up, RightUp, Right, RightDown, Down, LeftDown, Left, LeftUp
        }

        public static readonly Direction[] baseDirections = new Direction[4] {
            Direction.Up,
            Direction.Down,
            Direction.Left,
            Direction.Right};

        private static Dictionary<Direction, Vector2> dir2vector = new Dictionary<Direction, Vector2>() {
            {Direction.Up,        new Vector2(0, 1) },
            {Direction.RightUp,   new Vector2(1, 1).normalized },
            {Direction.Right,     new Vector2(1, 0) },
            {Direction.RightDown, new Vector2(1, -1).normalized },
            {Direction.Down,      new Vector2(0, -1) },
            {Direction.LeftDown,  new Vector2(-1, -1).normalized },
            {Direction.Left,      new Vector2(-1, 0) },
            {Direction.LeftUp,    new Vector2(-1, 1).normalized },
            {Direction.None,      new Vector2(0, 0)},
        };

        private static Dictionary<Direction, float> dir2Euler = new Dictionary<Direction, float>() {
            {Direction.Right,     0},
            {Direction.RightUp,   45 },
            {Direction.Up,        90 },
            {Direction.LeftUp,    135},
            {Direction.Left,      180},
            {Direction.LeftDown,  225},
            {Direction.Down,      270},
            {Direction.RightDown, 315 },
            {Direction.None,      Mathf.Infinity},
        };

        private static Dictionary<int, Direction> euler2Dir = new Dictionary<int, Direction>() {
            {0, Direction.Right},
            {45, Direction.RightUp},
            {90, Direction.Up},
            {135, Direction.LeftUp},
            {180, Direction.Left},
            {225, Direction.LeftDown},
            {270, Direction.Down},
            {315, Direction.RightDown}
        };

        private static Dictionary<Direction, Direction> oppositeDir = new Dictionary<Direction, Direction>() {
            {Direction.Up,        Direction.Down },
            {Direction.RightUp,   Direction.LeftDown },
            {Direction.Right,     Direction.Left },
            {Direction.RightDown, Direction.LeftUp},
            {Direction.Down,      Direction.Up },
            {Direction.LeftDown,  Direction.RightUp },
            {Direction.Left,      Direction.Right },
            {Direction.LeftUp,    Direction.RightDown },
            {Direction.None,      Direction.None },
        };


        public static Direction EulerToDirection(float degrees)
        {
            // TODO test 
            while (degrees < 0) {
                degrees += 360f;
            }
            degrees = degrees % 360f;
            degrees /= 45f;
            degrees = Mathf.Round(degrees);
            degrees *= 45;
            degrees = degrees % 360f;
            return euler2Dir[(int)degrees];
        }


        public static float DirectionToEuler(Direction dir)
        {
            return dir2Euler[dir];
        }


        /// <summary>
        /// Input vector is not necessary to be normalized (magnitude can be different than 1) <para/>
        /// If vector ==(0,0) then returns null
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static float? VectorToEuler(Vector2 vector, int anchorAxisSign = 1)
        {
            Vector2 anchorAxis = anchorAxisSign == 1 ? Vector2.right : -Vector2.right;
            if (vector.x == 0 && vector.y == 0)
                return null;
            if (vector.y > 0) {
                return Vector2.Angle(anchorAxis, vector);
            } else {
                return 360 - Vector2.Angle(anchorAxis, vector);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="anchorAxis"> anchorAxisSign = 1 => axis (0,x), anchorAxisSign == -1 => axis(0, -x)</param>
        /// <returns></returns>
        public static Direction VectorToDirection(Vector2 vector, int anchorAxisSign = 1)
        {
            float? euler = VectorToEuler(vector, anchorAxisSign);

            if (euler == null) {
                return Direction.None;
            } else {
                return EulerToDirection(euler.Value);
            }
        }


        public static Vector2 DirectionToVector(Direction direction)
        {
            return dir2vector[direction];
        }


        public static Vector2 DiscretizeVector(Vector2 vector)
        {
            return dir2vector[VectorToDirection(vector)];
        }

        public static bool DirectionsAgree(Direction directionA, Direction directionB)
        {
            bool directionsAgree = false;
            if ((directionA == Direction.None || directionB == Direction.None) || (directionA == directionB))
                directionsAgree = true;
            return directionsAgree;
        }


        public static Direction ToFourDirectional(Direction dir8)
        {
            Direction dir4 = dir8;
            if (dir8 == Direction.LeftDown || dir8 == Direction.LeftUp) {
                dir4 = Direction.Left;
            }
            if (dir8 == Direction.RightDown || dir8 == Direction.RightUp) {
                dir4 = Direction.Right;
            }
            return dir4;
        }

        public static bool IsFourDirectional(Direction dir)
        {
            return ToFourDirectional(dir) == dir;
        }

        public static Direction OppositeDirection(Direction dir)
        {
            return oppositeDir[dir];
        }


        public static bool AreAdjacent(Direction dir1, Direction dir2)
        {
            if (dir1 == Direction.None || dir2 == Direction.None)
                return false;

            // On the sides of the array you can't do integer math
            if (dir1 == Direction.Up) {
                return (dir2 == Direction.LeftUp || dir2 == Direction.Up || dir2 == Direction.RightUp);
            }
            if (dir1 == Direction.LeftUp) {
                return (dir2 == Direction.Left || dir2 == Direction.LeftUp || dir2 == Direction.Up);
            }

            return Mathf.Abs(dir1 - dir2) <= 1;

        }


        public static Vector2 Rotate(Vector2 v, float degrees)
        {
            float radians = Mathf.Deg2Rad * degrees;
            return new Vector2(
                v.x * Mathf.Cos(radians) - v.y * Mathf.Sin(radians),
                v.x * Mathf.Sin(radians) + v.y * Mathf.Cos(radians)
            );
        }

        public static Vector3Int[] GetNeighbors(Vector3Int pos)
        {
            Vector3Int[] neighbors ={
                    pos + Vector3Int.right,
                    pos + Vector3Int.left,
                    pos + Vector3Int.up,
                    pos + Vector3Int.down,
                    pos + Vector3Int.down + Vector3Int.left,
                    pos + Vector3Int.down + Vector3Int.right,
                    pos + Vector3Int.up   + Vector3Int.left,
                    pos + Vector3Int.up   + Vector3Int.right
        };
            return neighbors;
        }

    }
}