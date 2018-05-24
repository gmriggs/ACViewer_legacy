using System;
using System.Collections.Generic;
using System.Linq;
using ACE.DatLoader;
using ACE.DatLoader.FileTypes;

namespace ACViewer.Data
{
    public class Dungeon
    {
        public List<Environment> EnvCells;

        public void LoadDungeon()
        {
            //uint dungeonID = 0x01D9;      // A Red Rat Lair
            //uint dungeonID = 0x18A;       // Hotel Swank
            uint dungeonID = 0x0103;        // Black Spawn Den

            uint landblockID = dungeonID << 16 | 0xFFFF;
            uint blockInfoID = dungeonID << 16 | 0xFFFE;

            var files = DatManager.CellDat.AllFiles.Where(f => f.Key >> 16 == dungeonID).ToList();

            Console.WriteLine("Reading landblock");
            var landblock = DatManager.CellDat.ReadFromDat<CellLandblock>(landblockID);

            Console.WriteLine("Reading landblock info");
            var blockinfo = DatManager.CellDat.ReadFromDat<LandblockInfo>(blockInfoID);
            var numCells = blockinfo.NumCells;

            BuildEnv(dungeonID, numCells);
        }

        public void BuildEnv(uint dungeonID, uint numCells)
        {
            // interior cellIDs always start at 0x100
            // always sequential from 0x100 + numCells
            Console.WriteLine("Reading cells");
            EnvCells = new List<Environment>();
            uint firstCellID = 0x100;
            for (uint i = 0; i < numCells; i++)
            {
                uint cellID = firstCellID + i;
                uint blockCell = dungeonID << 16 | cellID;
                var cell = DatManager.CellDat.ReadFromDat<EnvCell>(blockCell);
                EnvCells.Add(new Environment(cell));
            }
        }
    }
}
