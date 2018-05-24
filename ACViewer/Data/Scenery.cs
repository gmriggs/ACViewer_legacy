using System;
using System.Collections.Generic;
using System.Numerics;
using ACE.DatLoader;
using ACE.DatLoader.Entity;
using ACE.DatLoader.FileTypes;

namespace ACViewer
{
    public class Scenery
    {
        public List<ModelInstance> ModelInstances;
        public Landblock Landblock;

        public CellLandblock CellLandblock { get => Landblock.CellLandblock; }
        public RegionDesc RegionDesc { get => ACData.RegionDesc; }

        public Scenery(Landblock landblock)
        {
            Landblock = landblock;
            LoadScenery();
        }

        public void LoadScenery()
        {
            ModelInstances = new List<ModelInstance>();

            var sceneIDs = new Dictionary<string, int>();

            // get landblock world cell coordinates
            var blockX = (CellLandblock.Id >> 24) * 8;
            var blockY = (CellLandblock.Id >> 16 & 0xFF) * 8;

            var i = 0;
            foreach (var terrain in CellLandblock.Terrain)
            {
                var terrainType = terrain >> 2 & 0x1F;      // TerrainTypes table size = 32 (grass, desert, volcano, etc.)
                //Console.WriteLine("TerrainType: " + terrainType);
                /*if (terrainType == 1)
                {
                    i++;
                    continue;
                }*/
                var sceneType = terrain >> 11;              // SceneTypes table size = 89 globally, 32 of which can be indexed for each type of terrain
                //Console.WriteLine("SceneType: " + sceneType);
                var sceneInfo = (int)RegionDesc.TerrainInfo.TerrainTypes[terrainType].SceneTypes[sceneType];
                var scenes = RegionDesc.SceneInfo.SceneTypes[sceneInfo].Scenes;
                if (scenes.Count == 0) continue;

                //Console.WriteLine(string.Format("{0} scenes for cell[{1},{2}] ({3} terrainType, {4} sceneType)", scenes.Count, i / 9, i % 9, terrainType, sceneType));

                var cellX = i / Landblock.VertexDim;
                var cellY = i % Landblock.VertexDim;

                var globalCellX = (uint)(cellX + blockX);
                var globalCellY = (uint)(cellY + blockY);

                var scene_idx = 0;
                var cellMat = globalCellY * (712977289 * globalCellX + 1813693831) - 1109124029 * globalCellX + 2139937281;
                var floorFactor = cellMat * 2.3283064e-10;
                scene_idx = (int)(floorFactor * scenes.Count);
                if (scene_idx >= scenes.Count)
                    scene_idx = 0;

                var sceneID = scenes[scene_idx];

                if (!sceneIDs.ContainsKey(sceneID.ToString("X8")))
                    sceneIDs.Add(sceneID.ToString("X8"), 1);
                else
                    sceneIDs[sceneID.ToString("X8")]++;

                var scene = DatManager.PortalDat.ReadFromDat<Scene>(sceneID);
                //Console.WriteLine("Scene " + sceneID.ToString("X8") + " objects: " + scene.Objects.Count + " (" + RegionDesc.TerrainInfo.TerrainTypes[terrainType].TerrainName + ")");

                //Console.WriteLine(scene.Objects.Count + " objects for this scene");

                var cellXMat = -1109124029 * globalCellX;
                var cellYMat = 1813693831 * globalCellY;
                cellMat = 1360117743 * globalCellX * globalCellY + 1888038839;

                uint sceneObjIdx = 0;
                foreach (var obj in scene.Objects)
                {
                    var noise = (uint)(cellXMat + cellYMat - cellMat * (23399 + sceneObjIdx));
                    if (noise * 2.3283064e-10 < obj.Freq && obj.WeenieObj == 0)
                    {
                        var position = Displace(obj, globalCellX, globalCellY, sceneObjIdx);
                        var lbx = cellX * 24.0f + position.X;
                        var lby = cellY * 24.0f + position.Y;

                        // ensure within landblock range, and not on road
                        if (lbx >= 0 && lby >= 0 && lbx <= Landblock.LandblockSize && lby <= Landblock.LandblockSize &&
                            !OnRoad(obj, lbx, lby))
                        {
                            //Console.WriteLine("Adding " + obj.ObjId.ToString("X8") + " to (" + cellX + "," + cellY + ")");
                            
                            // TODO: ensure walkable slope

                            //Console.WriteLine("Model ID = " + obj.ObjId.ToString("X8"));
                            var model = new ModelInstance();
                            model.StaticMesh = StaticMeshCache.GetMesh(obj.ObjId);
                            if (model.StaticMesh.GfxObjs.Count > 0)
                            {
                                ACViewer.Instance.Render.Setup.SceneryVertexCount += model.StaticMesh.TotalVertices;
                                model.Frame = obj.BaseLoc;
                                model.Position = new Vector3(position.X, position.Y, obj.BaseLoc.Origin.Z);
                                model.ObjectDesc = obj;
                                model.Scale = ScaleObj(model, globalCellX, globalCellY, sceneObjIdx);

                                model.Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, RotateObj(model, globalCellX, globalCellY, sceneObjIdx) * -0.0174533f);
                                model.Cell = new Vector2(cellX, cellY);
                                model.BoundingBox = new BoundingBox(model);
                                //if (obj.Freq < 1.0f)
                                //model.BBox.SetSize(24.0f);
                                //model.BBox.Scale(2.0f);

                                // collision detection
                                if (/*!DetectCollision(Buildings, model) && !DetectCollision(Scenery, model)*/ true)
                                {
                                    ModelInstances.Add(model);
                                    //ACViewer.Instance.TotalScenery++;

                                    // max objects per landblock in client - verify?
                                    if (ModelInstances.Count >= 300) return;
                                }
                            }
                        }
                    }
                    sceneObjIdx++;
                }

                i++;
            }
            //ShowDict(sceneIDs);
        }

        public float ScaleObj(ModelInstance model, uint x, uint y, uint k)
        {
            var scale = 1.0f;

            var minScale = model.ObjectDesc.MinScale;
            var maxScale = model.ObjectDesc.MaxScale;

            if (minScale == maxScale)
                scale = maxScale;
            else
                scale = (float)(Math.Pow(maxScale / minScale,
                    (1813693831 * y - (k + 32593) * (1360117743 * y * x + 1888038839) - 1109124029 * x) * 2.3283064e-10) * minScale);

            return scale;
        }

        public float RotateObj(ModelInstance model, uint x, uint y, uint k)
        {
            if (model.ObjectDesc.MaxRotation <= 0.0f)
                return 0.0f;

            return (float)((1813693831 * y - (k + 63127) * (1360117743 * y * x + 1888038839) - 1109124029 * x) * 2.3283064e-10 * model.ObjectDesc.MaxRotation);
        }

        public bool DetectCollision(List<ModelInstance> models, ModelInstance model)
        {
            foreach (var m in models)
            {
                if (model.BoundingBox.Intersect(m.BoundingBox))
                    return true;
            }
            return false;
        }

        public bool OnRoad(ObjectDesc obj, float x, float y)
        {
            var cellX = (int)Math.Floor(x / Landblock.CellSize);
            var cellY = (int)Math.Floor(y / Landblock.CellSize);
            var terrain = CellLandblock.Terrain[cellX * Landblock.CellDim + cellY];     // ensure within bounds?
            return (terrain & 0x3) != 0;    // TODO: more complicated check for within road range
        }

        /// <summary>
        /// Displaces a scenery object into a pseudo-randomized location
        /// </summary>
        /// <param name="obj">The object description</param>
        /// <param name="ix">The global cell X-offset</param>
        /// <param name="iy">The global cell Y-offset</param>
        /// <param name="iq">The scene index of the object</param>
        /// <returns>The new location of the object</returns>
        public static Vector2 Displace(ObjectDesc obj, uint ix, uint iy, uint iq)
        {
            float x;
            float y;

            var loc = obj.BaseLoc.Origin;

            if (obj.DisplaceX <= 0)
                x = loc.X;
            else
                x = (float)((1813693831 * iy - (iq + 45773) * (1360117743 * iy * ix + 1888038839) - 1109124029 * ix)
                    * 2.3283064e-10 * obj.DisplaceX + loc.X);

            if (obj.DisplaceY <= 0)
                y = loc.Y;
            else
                y = (float)((1813693831 * iy - (iq + 72719) * (1360117743 * iy * ix + 1888038839) - 1109124029 * ix)
                    * 2.3283064e-10 * obj.DisplaceY + loc.Y);

            var quadrant = (1813693831 * iy - ix * (1870387557 * iy + 1109124029) - 402451965) * 2.3283064e-10;

            if (quadrant >= 0.75) return new Vector2(y, -x);
            if (quadrant >= 0.5) return new Vector2(-x, -y);
            if (quadrant >= 0.25) return new Vector2(-y, x);

            return new Vector2(x, y);
        }
    }
}