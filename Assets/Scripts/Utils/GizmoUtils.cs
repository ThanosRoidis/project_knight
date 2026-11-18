using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shared
{
    public static class GizmoUtils
    {

        /// <summary>
        /// Draw a Gizmo empty square at the specified location 
        /// </summary>
        /// <param name="x"></param> X position (Upper left corner)
        /// <param name="y"></param> Y position (Upper left corner)
        /// <param name="w"></param> Width of the square
        /// <param name="h"></param> Height of the square
        public static void DrawSquare(float x, float y, float w, float h)
        {
            // left
            //Gizmos.DrawLine(new Vector2(x, y), new Vector2(x, y - h));
            //// Right
            //Gizmos.DrawLine(new Vector2(x + w, y), new Vector2(x + w, y - h));
            //// Top
            //Gizmos.DrawLine(new Vector2(x, y), new Vector2(x + w, y));
            //// Bottom
            //Gizmos.DrawLine(new Vector2(x, y - h), new Vector2(x + w, y - h));

            List<Vector2> squarePoints = new List<Vector2> { new Vector2(x, y), 
                                                             new Vector2(x + w, y), 
                                                             new Vector2(x + w, y - h), 
                                                             new Vector2(x, y - h), 
                                                             new Vector2(x, y) };

            DrawLine(squarePoints);
        }

        /// <summary>
        /// Draw a Gizmo grid with the specified number of cells
        /// </summary>
        /// <param name="x"></param> The grid's start potition on the X axis (Bottom Left corner)
        /// <param name="y"></param> The grid's start potition on the Y axis (Bottom left corner)
        /// <param name="w"></param> The number of cells on the X axis
        /// <param name="h"></param> The number of cells on the Y axis
        /// <param name="cellWidth"></param> The width of a cell
        /// <param name="cellHeight"></param> The height of a cell
        public static void DrawGrid(float x, float y, int w, int h, float cellWidth, float cellHeight)
        {
            //Draw horizontal lines
            for (int j = 0; j <= h; j++)
                Gizmos.DrawLine(new Vector2(x, y + j * cellHeight), new Vector2(x + w * cellWidth, y + j * cellHeight));

            //Draw vertical lines
            for (int i = 0; i <= w; i++)
                Gizmos.DrawLine(new Vector2(x + i * cellWidth, y), new Vector2(x + i * cellWidth, y + h * cellHeight));
        }


        public static void DrawLine(List<Vector2> positions)
        {
            for (int i = 0; i < positions.Count - 1; i++) {
                Gizmos.DrawLine(positions[i], positions[i + 1]);
            }
        }


    }
}
