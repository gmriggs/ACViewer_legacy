using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Diag;
using ACE.Diag.Entity;

namespace ACViewer
{
    public class Player
    {
        public ACViewer Game { get => ACViewer.Instance; }

        public GameState GameState { get => Game.GameState; }



        public LandblockId GetLandblock()
        {
            if (!Program.UseServer)
                return new LandblockId(Landblock.DefaultLandblock);

            var player = GetPlayer();
            if (player == null) return null;

            return player.Location.landblockId;
        }

        public ACE.Diag.Entity.Player GetPlayer()
        {
            try
            {
                if (Game.Client == null || GameState == null || GameState.WorldObjects == null)
                    return null;

                var player = GameState.WorldObjects.Values.FirstOrDefault(wo => wo is ACE.Diag.Entity.Player);

                if (player == null)
                {
                    Console.WriteLine("No player found");
                    return null;
                }
                return player as ACE.Diag.Entity.Player;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<WorldObject> GetCreatures()
        {
            try
            {
                if (Game.Client == null || GameState == null || GameState.WorldObjects == null)
                    return null;

                var creatures = GameState.WorldObjects.Values.Where(wo => !(wo is ACE.Diag.Entity.Player) && wo is Creature).ToList();

                if (creatures == null || creatures.Count == 0)
                {
                    //Console.WriteLine("No creatures found");
                    return null;
                }
                return creatures;

            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<WorldObject> GetMissiles()
        {
            try
            {
                if (Game.Client == null || GameState == null || GameState.WorldObjects == null)
                    return null;

                var missiles = GameState.WorldObjects.Values.Where(wo => wo.Missile != null && wo.Missile.Value).ToList();

                if (missiles == null || missiles.Count == 0)
                    return null;

                return missiles;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
