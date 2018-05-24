using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ACViewer
{
    /// <summary>
    /// A 3D mesh of vertices and triangles
    /// for landblocks and models
    /// </summary>
    public class Mesh
    {
        /// <summary>
        /// The list of vertices comprising the mesh
        /// </summary>
        public List<Vector3> Vertices;

        /// <summary>
        /// A list of indices into the vertex array
        /// </summary>
        public List<Triangle> Triangles;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Mesh() { }

        /// <summary>
        /// Loads the vertices for a landblock mesh
        /// </summary>
        /// <param name="height">The height of each vertex in the landblock cells</param>
        public void LoadVertices(float[,] height)
        {
            var xSize = height.GetLength(0);
            var ySize = height.GetLength(1);

            Vertices = new List<Vector3>(xSize * ySize);

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    Vertices.Add(new Vector3(x * Landblock.CellSize, y * Landblock.CellSize, height[x, y]));
                }
            }
        }

        /// <summary>
        /// Generates the triangles from the mesh vertices
        /// </summary>
        /// <param name="id">The landblock to generate triangles for</param>
        public void BuildTriangles(Landblock landblock)
        {
            var cellDim = Landblock.CellDim;
            var vertexDim = Landblock.VertexDim;

            Triangles = new List<Triangle>();

            for (int x = 0; x < cellDim; x++)
            {
                for (int y = 0; y < cellDim; y++)
                {
                    int lowerLeft = x + y * vertexDim;
                    int lowerRight = (x + 1) + y * vertexDim;
                    int topLeft = x + (y + 1) * vertexDim;
                    int topRight = (x + 1) + (y + 1) * vertexDim;

                    // determine where to draw the split line
                    if (GetSplitDir(landblock, x, y))
                    {
                        // clockwise winding order
                        Triangles.Add(new Triangle(topLeft, lowerRight, lowerLeft));
                        Triangles.Add(new Triangle(topLeft, topRight, lowerRight));
                    }
                    else
                    {
                        Triangles.Add(new Triangle(topRight, lowerRight, lowerLeft));
                        Triangles.Add(new Triangle(topRight, lowerLeft, topLeft));
                    }
                }
            }
        }

        /// <summary>
        /// Determines the split line direction
        /// for a cell triangulation
        /// </summary>
        /// <param name="id">A reference to the landblock ID</param>
        /// <param name="cellX">The horizontal cell position within the landblock</param>
        /// <param name="cellY">The vertical cell position within the landblock</param>
        /// <returns>TRUE if NW-SE split, FALSE if NE-SW split</returns>
        public bool GetSplitDir(Landblock landblock, int cellX, int cellY)
        {
            // get the global tile offsets
            var x = (landblock.X * 8) + cellX;
            var y = (landblock.Y * 8) + cellY;

            // Thanks to https://github.com/deregtd/AC2D for this bit
            var dw = x * y * 0x0CCAC033 - x * 0x421BE3BD + y * 0x6C1AC587 - 0x519B8F25;
            return (dw & 0x80000000) == 0;
        }

        /// <summary>
        /// Returns the shared line between 2 triangles
        /// </summary>
        public Line2 GetSplitter(List<Triangle> triangles)
        {
            if (triangles[0].Indices[1] == triangles[1].Indices[2])
                return new Line2(Vertices[triangles[0].Indices[0]], Vertices[triangles[0].Indices[1]]);
            else
                return new Line2(Vertices[triangles[0].Indices[0]], Vertices[triangles[0].Indices[2]]);
        }

        /// <summary>
        /// Returns the triangle containing a pair of x,y coordinates
        /// </summary>
        public Triangle GetTriangle(Vector2 point)
        {
            // find the cell which contains these coordinates
            Vector2 cellOffset = Landblock.GetCell(point);

            // get the triangles for this cell
            var cellTriangles = GetCellTriangles(cellOffset);

            // TODO: handle all collinear cases
            if (cellTriangles[0].Contains(point, Vertices))
                return cellTriangles[0];
            else
                return cellTriangles[1];
        }

        /// <summary>
        /// Returns the 2 triangles for a cell
        /// </summary>
        public List<Triangle> GetCellTriangles(Vector2 cellOffset)
        {
            var offset = (int)((cellOffset.Y * Landblock.CellDim + cellOffset.X) * 2);
            // TODO: ensure within bounds
            return new List<Triangle>() { Triangles[offset], Triangles[offset + 1] };
        }
    }
}
