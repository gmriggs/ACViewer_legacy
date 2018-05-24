using System;
using System.Collections.Generic;
using System.Numerics;

using ACE.DatLoader;
using ACE.DatLoader.FileTypes;

namespace ACViewer
{
    /// <summary>
    /// Loads everything contained within a landblock
    /// </summary>
    public class Landblock
    {
        /// <summary>
        /// The landblock ID, ie. 0x7D64FFFF
        /// </summary>
        public uint ID;

        /// <summary>
        /// The landblock X-coordinate, ie. 0x7D
        /// </summary>
        public uint X { get => ID >> 24; }

        /// <summary>
        /// The landblock Y-coordinate, ie. 0x64
        /// </summary>
        public uint Y { get => ID >> 16 & 0xFF; }

        /// <summary>
        /// A landblock has this many cells squared
        /// </summary>
        public static readonly int CellDim = 8;

        /// <summary>
        /// A landblock is this unit size squared
        /// </summary>
        public static readonly int LandblockSize = 192;

        /// <summary>
        /// A landblock cell is this unit size squared
        /// </summary>
        public static readonly int CellSize = LandblockSize / CellDim;

        /// <summary>
        /// A landblock has this many vertices squared
        /// </summary>
        public static readonly int VertexDim = CellDim + 1;

        /// <summary>
        /// The land mesh for an outdoor landblock
        /// </summary>
        public Mesh Mesh;

        public List<ModelInstance> LandObjects;
        public List<ModelInstance> Buildings;
        public List<ModelInstance> WeenieObjects;

        /// <summary>
        /// A reference to an 0x1 record from cell.dat
        /// </summary>
        public CellLandblock CellLandblock;

        /// <summary>
        /// A reference to an 0x2 record from cell.dat
        /// contains scenery info
        /// </summary>
        public LandblockInfo LandblockInfo;

        /// <summary>
        /// The height of each cell vertex for outdoor cells
        /// from using the RegionDesc lookup table
        /// </summary>
        public float[,] VertexHeights;

        public Scenery Scenery;

        //public static uint DefaultLandblock = 0x7D64FFFF;     // yaraq
        //public static uint DefaultLandblock = 0x7C63FFFF;     // yaraq - 1
        //public static uint DefaultLandblock = 0x7D68FFFF;   // north yaraq
        //public static uint DefaultLandblock = 0x8064FFFF;
        //public static uint DefaultLandblock = 0x7E64FFFF;   // like a rock
        //public static uint DefaultLandblock = 0x7D63FFFF;
        public static uint DefaultLandblock = 0x7C65FFFF;   // orchard w/ missing trees
        //public static uint DefaultLandblock = 0x7B63FFFF;   // yaraq beach
        //public static uint DefaultLandblock = 0xE63EFFFF;   // nanto scenery
        //public static uint DefaultLandblock = 0xA9B3FFFF;   // holtburg
        //public static uint DefaultLandblock = 0xBC9FFFFF;   // cragstone
        //public static uint DefaultLandblock = 0x8090FFFF;   // zaikhal
        //public static uint DefaultLandblock = 0xC95BFFFF;   // sawato
        //public static uint DefaultLandblock = 0xDA55FFFF;   // shoushi
        //public static uint DefaultLandblock = 0x9722FFFF;   // qalabar
        //public static uint DefaultLandblock = 0xC6A9FFFF;   // arwic
        //public static uint DefaultLandblock = 0x1134FFFF;   // ayan baqur
        //public static uint DefaultLandblock = 0x2581FFFF;   // fort tethana
        //public static uint DefaultLandblock = 0xA1A4FFFF;   // glenden wood
        //public static uint DefaultLandblock = 0xF75CFFFF;   // tou-tou
        //public static uint DefaultLandblock = 0x9058FFFF;   // al-arqas
        //public static uint DefaultLandblock = 0xE74EFFFF;   // hebian-to
        //public static uint DefaultLandblock = 0xCD41FFFF;   // baishi
        //public static uint DefaultLandblock = 0xE532FFFF;   // mayoi
        //public static uint DefaultLandblock = 0xBA17FFFF;   // kara
        //public static uint DefaultLandblock = 0x84B0FFFF;   // charnhold

        public Landblock(uint x, uint y)
        {
            Init(x << 24 | y << 16 | 0xFFFF);
        }

        /// <summary>
        /// Constructs a new Landblock from a landblock ID
        /// </summary>
        public Landblock(uint id)
        {
            Init(id);
        }

        /// <summary>
        /// Loads a landblock from an ID
        /// </summary>
        public void Init(uint id)
        {
            ID = id;
            CellLandblock = DatManager.CellDat.ReadFromDat<CellLandblock>(id);

            LoadMesh();
            LoadModels();
        }

        /// <summary>
        /// Loads the mesh for an outdoor landblock
        /// </summary>
        public void LoadMesh()
        {
            VertexHeights = GetVertexHeights(CellLandblock);

            Mesh = new Mesh();
            Mesh.LoadVertices(VertexHeights);
            Mesh.BuildTriangles(this);
        }

        /// <summary>
        /// Lodas all of the model types for landblock
        /// ( LandObject / Buildings / DB weenies / Scenery )
        /// </summary>
        public void LoadModels()
        {
            LandblockInfo = DatManager.CellDat.ReadFromDat<LandblockInfo>(ID - 1);

            LoadObjects(LandblockInfo);
            LoadBuildings(LandblockInfo);
            //LoadWeenies();
            Scenery = new Scenery(this);
        }

        /// <summary>
        /// Loads all of the landblock object models
        /// </summary>
        /// <param name="landblockInfo">The 0x2 record contain the list of LandObjects</param>
        public void LoadObjects(LandblockInfo landblockInfo)
        {
            LandObjects = new List<ModelInstance>();

            foreach (var obj in landblockInfo.Objects)
            {
                var landObject = new ModelInstance();
                //landObject.Model = GetModel(obj.Id, ModelType.LandObject);
                landObject.StaticMesh = StaticMeshCache.GetMesh(obj.Id);
                landObject.Frame = obj.Frame;

                LandObjects.Add(landObject);
                ACViewer.Instance.Render.Setup.ModelVertexCount += landObject.StaticMesh.TotalVertices;
            }
        }

        /// <summary>
        /// Loads all of the buildings contained in landblock
        /// </summary>
        /// <param name="landblockInfo">The 0x2 record contain the building list</param>
        public void LoadBuildings(LandblockInfo landblockInfo)
        {
            Buildings = new List<ModelInstance>();

            foreach (var obj in landblockInfo.Buildings)
            {
                var building = new ModelInstance();
                //building.Model = GetModel(obj.ModelId, ModelType.Building);
                building.StaticMesh = StaticMeshCache.GetMesh(obj.ModelId);
                building.Frame = obj.Frame;
                building.Position = obj.Frame.Origin;
                building.BoundingBox = new BoundingBox(building);

                Buildings.Add(building);
                ACViewer.Instance.Render.Setup.ModelVertexCount += building.StaticMesh.TotalVertices;
            }
        }

        public void LoadWeenies()
        {
            /*WeenieObjects = new List<ModelInstance>();

            var objects = DatabaseManager.World.GetWeenieInstancesByLandblock(GetShortID());
            foreach (var obj in objects)
            {
                var weenieClassId = obj.WeenieClassId;
                var weenieInfo = obj.AceObjectPropertiesPositions.Values.LastOrDefault();
                var weenieObject = new ModelInstance();
                weenieObject.Model = GetModel(obj.SetupDID.Value, ModelType.LandObject);
                weenieObject.Frame = new ACE.DatLoader.Entity.Frame(weenieInfo);
                WeenieObjects.Add(weenieObject);
                ACViewer.Instance.WeenieVertexCount += weenieObject.Model.TotalVertices;
            }*/
        }
        
        /// <summary>
        /// Reads the heights for each vertex in the landblock cells
        /// </summary>
        /// <param name="cellLandblock">A landblock from the cell database</param>
        /// <returns>The vertex heights for the landblock cells</returns>
        public float[,] GetVertexHeights(CellLandblock cellLandblock)
        {
            // The vertex heights in the cell database are stored in bytes,
            // which map to offsets in the land height table from the region file in the portal database.

            var heights = new float[VertexDim, VertexDim];

            for (int x = 0; x < VertexDim; x++)
            {
                for (int y = 0; y < VertexDim; y++)
                {
                    heights[x, y] = ACData.RegionDesc.LandDefs.LandHeightTable[cellLandblock.Height[x * VertexDim + y]];
                }
            }
            return heights;
        }

        /// <summary>
        /// Returns the cell that contains a pair of 2D coordinates
        /// within a landblock
        /// </summary>
        /// <param name="x">The landblock x-coord in the range 0 - 192</param>
        /// <param name="y">The landblock y-coord in the range 0 - 192</param>
        /// <returns>The cell offsets that contain these coordinates</returns>
        public static Microsoft.Xna.Framework.Vector2 GetCell(Microsoft.Xna.Framework.Vector2 point)
        {
            var cellX = (float)Math.Floor(point.X / CellSize);
            var cellY = (float)Math.Floor(point.Y / CellSize);

            if (cellX < 0) cellX = 0;
            if (cellY < 0) cellY = 0;
            if (cellX > CellDim - 1) cellX = CellDim - 1;
            if (cellY > CellDim - 1) cellY = CellDim - 1;

            return new Microsoft.Xna.Framework.Vector2(cellX, cellY);
        }

        /// <summary>
        /// Returns the x,y coordinates for a cell idx
        /// </summary>
        public static Vector2 GetCell(int idx)
        {
            // counts upwards in Y first?
            var cellX = idx / VertexDim;
            var cellY = idx % VertexDim;

            return new Vector2(cellX, cellY);
        }
    }
}
