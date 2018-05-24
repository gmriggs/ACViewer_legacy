using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ACE.DatLoader.Entity;
using ACViewer.Data;

namespace ACViewer.Render
{
    public class SetupInterior
    {
        public VertexPositionColor[] Vertices;
        public int[] Indices;

        public List<Environment> EnvCells;

        public void BuildVertices()
        {
            var vertices = new List<VertexPositionColor>();

            var i = 0;
            foreach (var envCell in EnvCells)
            {
                var transform = BuildTransform(envCell.EnvCell.Position, false);

                foreach (var environment in envCell.Environments)
                {
                    foreach (var cell in environment.Cells.Values)
                    {
                        foreach (var v in cell.VertexArray.Vertices.Values)
                        {
                            var vertex = new Vector3(v.X, v.Y, v.Z);
                            vertex = Vector3.Transform(vertex, transform);
                            vertices.Add(new VertexPositionColor(vertex, Color.White));
                        }
                    }
                }
                i++;
            }
            Vertices = vertices.ToArray();
        }

        public void BuildLineIndices()
        {
            var lines = new List<int>();

            var cellOffset = 0;
            foreach (var envCell in EnvCells)
            {
                var gPolyIdx = 0;
                var j = 0;
                for (var polyIdx = 0; polyIdx < envCell.Polygons.Count; polyIdx++)
                {
                    var poly = envCell.Polygons[polyIdx];
                    var polyOffset = envCell.CellOffsets[j];
                    var polyOffset2 = envCell.PolyOffsets[j++];

                    if (envCell.PortalPolys.Contains(polyOffset + gPolyIdx))
                        continue;

                    var numVerts = poly.Vertices.Count;
                    var vertexIndices = poly.VertexIds;
                    for (var i = 0; i < numVerts; i++)
                    {
                        lines.Add(vertexIndices[i] + cellOffset + polyOffset);
                        if (i < numVerts - 1)
                            lines.Add(vertexIndices[i + 1] + cellOffset + polyOffset);
                        else
                            lines.Add(vertexIndices[0] + cellOffset + polyOffset);
                    }
                    gPolyIdx++;
                }
                cellOffset += envCell.TotalVertices;
            }
            Indices = lines.ToArray();
        }

        public Matrix BuildTransform(Frame frame, bool transpose = true)
        {
            var translate = Matrix.CreateTranslation(new Vector3(frame.Origin.X, frame.Origin.Y, frame.Origin.Z));
            var rotate = Matrix.CreateFromQuaternion(new Quaternion(frame.Orientation.X, frame.Orientation.Y, frame.Orientation.Z, frame.Orientation.W));
            if (transpose)
                rotate = Matrix.Transpose(rotate);
            var transform = rotate * translate;
            return transform;
        }
    }
}
