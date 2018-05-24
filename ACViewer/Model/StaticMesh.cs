using System;
using System.Collections.Generic;
using ACE.DatLoader;
using ACE.DatLoader.FileTypes;

namespace ACViewer
{
    /// <summary>
    /// A static mesh contains all data that is common
    /// to each instance with this model id
    /// </summary>
    public class StaticMesh
    {
        /// <summary>
        /// The unique model identifier
        /// </summary>
        public uint ModelId;

        /// <summary>
        /// A multi-part model
        /// </summary>
        public SetupModel SetupModel;

        /// <summary>
        /// The individual model parts
        /// </summary>
        public List<GfxObj> GfxObjs;

        /// <summary>
        /// The list of polygons for this model
        /// </summary>
        public List<ModelPolygon> Polygons;

        /// <summary>
        /// The bounding box for the static mesh
        /// </summary>
        public BoundingBox BoundingBox;

        /// <summary>
        /// The total # of unique vertices from each GfxObj
        /// </summary>
        public int TotalVertices;

        /// <summary>
        /// The vertex offsets for each polygon in the model 
        /// </summary>
        public List<int> PolyOffsets = new List<int>();

        /// <summary>
        /// The vertex counts for each polygon
        /// </summary>
        public List<int> VertexCounts = new List<int>();

        /// <summary>
        /// The lowest z-coordinate of all the vertices
        /// </summary>
        public float LowestZ = float.MaxValue;

        /// <summary>
        /// Constructs a new static mesh from a model id
        /// </summary>
        public StaticMesh(uint modelId)
        {
            ModelId = modelId;

            var modelType = modelId >> 24;

            if (modelType == 0x01)
            {
                LoadModelPart(modelId);
            }
            else if (modelType == 0x02)
            {
                SetupModel = DatManager.PortalDat.ReadFromDat<SetupModel>(modelId);
                foreach (var part in SetupModel.Parts)
                    LoadModelPart(part);
            }
            BoundingBox = new BoundingBox(this);
        }

        /// <summary>
        /// Loads an individual piece of a model
        /// </summary>
        public void LoadModelPart(uint modelId)
        {
            if (GfxObjs == null) GfxObjs = new List<GfxObj>();
            if (Polygons == null) Polygons = new List<ModelPolygon>();

            var gfxObj = DatManager.PortalDat.ReadFromDat<GfxObj>(modelId);
            GfxObjs.Add(gfxObj);

            // vertices scope is for each GfxObj
            TotalVertices += gfxObj.VertexArray.Vertices.Count;

            var vertexOffset = 0;
            foreach (var poly in gfxObj.Polygons.Values)
            {
                VertexCounts.Add(poly.VertexIds.Count);
                vertexOffset += poly.VertexIds.Count;

                PolyOffsets.Add(vertexOffset);

                Polygons.Add(new ModelPolygon(poly, gfxObj.VertexArray));
            }

            CalcLowestZ(gfxObj);
        }

        /// <summary>
        /// Calculates the lowest z-coordinate from all vertices
        /// For placing the model on the ground
        /// </summary>
        public void CalcLowestZ(GfxObj gfxObj)
        {
            foreach (var v in gfxObj.VertexArray.Vertices.Values)
                LowestZ = Math.Min(v.Z, LowestZ);
        }
    }
}
