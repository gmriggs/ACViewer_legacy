using System;
using System.Numerics;

namespace ACViewer
{
    /// <summary>
    /// Loads a range of landblocks
    /// </summary>
    public class BlockRange
    {
        /// <summary>
        /// A reference to ACViewer
        /// </summary>
        public ACViewer ACViewer { get => ACViewer.Instance;  }

        /// <summary>
        /// The center landblock
        /// </summary>
        public uint LandblockID;

        /// <summary>
        /// The distance to load around center landblock
        /// </summary>
        public int LoadRadius;

        /// <summary>
        /// The landblock array
        /// </summary>
        public Landblock[,] Landblocks;

        public Vector2 SWBlockIdx;

        public int TerrainWidth = 9;
        public int TerrainHeight = 9;

        public float[,] HeightData;

        /// <summary>
        /// Constructs a landblock range from a center ID
        /// </summary>
        public BlockRange(uint landblockID)
        {
            LandblockID = landblockID;
            LoadRadius = 0;

            LoadCells();
            LoadHeightData();
        }

        /// <summary>
        /// Loads the starting landblock, with anything in radius
        /// </summary>
        public void LoadCells()
        {
            // load dimSize * dimSize landblocks
            var dimSize = LoadRadius * 2 + 1;

            Landblocks = new Landblock[dimSize, dimSize];

            for (var x = 0; x < dimSize; x++)
            {
                for (var y = 0; y < dimSize; y++)
                {
                    var saveFirst = (x == 0 && y == 0);
                    var blockId = GetBlockID(LandblockID, dimSize, x, y, saveFirst);
                    if (blockId == null)
                    {
                        Console.WriteLine("Landblock out-of-bounds, skipping");
                        continue;
                    }
                    Landblocks[x, y] = new Landblock(blockId.Value);
                }
            }
        }

        public uint? GetBlockID(uint center, int diameter, int x, int y, bool saveFirst)
        {
            // get center landblock x / y
            var center_x = center >> 24;
            var center_y = center >> 16 & 0xFF;

            // how many landblocks away in each dimension?
            var xDiff = x - LoadRadius;
            var yDiff = y - LoadRadius;

            var cur_ix = center_x + xDiff;
            var cur_iy = center_y + yDiff;

            // within bounds of map?
            if (cur_ix < 0 || cur_iy < 0 || cur_ix > 255 || cur_iy > 255)
                return null;

            if (saveFirst)
                SWBlockIdx = new Vector2(cur_ix, cur_iy);

            return (uint)(cur_ix << 24 | cur_iy << 16 | 0xFFFF);
        }

        public void LoadHeightData()
        {
            Console.WriteLine("Converting height data");

            // determine the size of the point set
            TerrainWidth = Landblocks.GetLength(0) * 8 + 1;
            TerrainHeight = Landblocks.GetLength(1) * 8 + 1;

            HeightData = new float[TerrainWidth, TerrainHeight];

            var curHeightX = 0;
            var curHeightY = 0;
            var curIdx = 0;

            // process landblock list
            for (var lx = 0; lx < Landblocks.GetLength(0); lx++)
            {
                for (var ly = 0; ly < Landblocks.GetLength(1); ly++)
                {
                    var curLandblock = Landblocks[lx, ly];

                    // process a landblock
                    for (var x = 0; x < 9; x++)
                    {
                        curHeightY = ly == 0 ? 0 : ly * 8 + 1;
                        if (ly > 0) curHeightY--;

                        for (var y = 0; y < 9; y++)
                        {
                            HeightData[curHeightX, curHeightY] = curLandblock == null || curLandblock.CellLandblock.Height.Count <= curIdx ? 0 : ACData.RegionDesc.LandDefs.LandHeightTable[curLandblock.CellLandblock.Height[curIdx++]];
                            curHeightY++;
                        }

                        curHeightX++;
                    }

                    curHeightX = lx == 0 ? 0 : lx * 8 + 1;
                    if (lx > 0) curHeightX--;
                    curIdx = 0;
                }

                curHeightX = (lx + 1) * 8;
            }
        }

        /// <summary>
        /// Determines if a square should
        /// be split NE-SW instead of NW-SE
        /// </summary>
        public bool GetSplitDir(int localX, int localY)
        {
            // get the landblock for these local coords
            var landBlockX = (uint)SWBlockIdx.X + (localX / 8);
            var landBlockY = (uint)SWBlockIdx.Y + (localY / 8);

            // get the tile offset within the landblock
            var tileX = localX % 8;
            var tileY = localY % 8;

            // get the global tile offsets
            var x = landBlockX * 8 + tileX;
            var y = landBlockY * 8 + tileY;

            // Thanks to https://github.com/deregtd/AC2D for this bit
            var dw = x * y * 0x0CCAC033 - x * 0x421BE3BD + y * 0x6C1AC587 - 0x519B8F25;
            return (dw & 0x80000000) != 0;
        }
    }
}
