using System.Collections.Generic;
using System.Numerics;
using ACE.DatLoader;
using ACE.DatLoader.FileTypes;

namespace ACViewer.Data
{
    /// <summary>
    /// Loads indoor environment cells
    /// for dungeons and building interiors
    /// </summary>
    public class Environment
    {
        public EnvCell EnvCell;

        public List<ACE.DatLoader.FileTypes.Environment> Environments = new List<ACE.DatLoader.FileTypes.Environment>();

        public List<ACE.DatLoader.Entity.Polygon> Polygons = new List<ACE.DatLoader.Entity.Polygon>();

        public List<int> CellOffsets = new List<int>();
        public List<int> PolyOffsets = new List<int>();

        public List<int> PortalPolys = new List<int>();
        public int TotalVertices;

        public BoundingBox BBox;

        public Environment(EnvCell envCell)
        {
            EnvCell = envCell;
            LoadEnv(envCell.EnvironmentId);
        }

        public void LoadEnv(uint envID)
        {
            var env = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.Environment>(envID);

            var cellOffset = 0;
            var polyOffset = 0;
            var gPolyIdx = 0;
            foreach (var cell in env.Cells.Values)
            {
                ushort polyIdx = 0;
                foreach (var poly in cell.Polygons.Values)
                {
                    CellOffsets.Add(cellOffset);
                    PolyOffsets.Add(polyOffset);
                    poly.LoadVertices(cell.VertexArray);
                    Polygons.Add(poly);

                    if (cell.Portals.Contains(polyIdx))
                        PortalPolys.Add(gPolyIdx);

                    polyIdx++;
                    gPolyIdx++;
                }
                cellOffset += cell.VertexArray.Vertices.Count;
                polyOffset += cell.Polygons.Count;
                TotalVertices += cell.VertexArray.Vertices.Count;
            }
            Environments.Add(env);

            var origin = EnvCell.Position.Origin;
            var orientation = EnvCell.Position.Orientation;
            var translate = Matrix4x4.CreateTranslation(new Vector3(origin.X, origin.Y, origin.Z));
            var rotate = Matrix4x4.CreateFromQuaternion(new Quaternion(orientation.X, orientation.Y, orientation.Z, orientation.W));

            BBox = new BoundingBox(Polygons, rotate * translate);
        }
    }
}
