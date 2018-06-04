using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ACViewer.Primitives;

namespace ACViewer.Render
{
    public class Setup
    {
        public BlockRange BlockRange { get => ACViewer.Instance.Landblocks; }
        public Landblock[,] Landblocks { get => BlockRange.Landblocks; }

        public VertexPositionColor[] Vertices;
        public VertexPositionColor[] ModelVertices;
        public VertexPositionColor[] WeenieVertices;
        public VertexPositionColor[] SceneryVertices;

        public int ModelVertexCount;
        public int WeenieVertexCount;
        public int SceneryVertexCount;

        public int[] Indices;
        public int[] ModelLines;
        public int[] WeenieLines;
        public int[] SceneryLines;

        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;

        public VertexBuffer ModelVertexBuffer;
        public IndexBuffer ModelIndexBuffer;

        public VertexBuffer WeenieVertexBuffer;
        public IndexBuffer WeenieIndexBuffer;

        public VertexBuffer SceneryVertexBuffer;
        public IndexBuffer SceneryIndexBuffer;

        public SpherePrimitive Player;
        public List<SpherePrimitive> Monsters;
        public List<SpherePrimitive> Projectiles;

        public GraphicsDevice GraphicsDevice { get => ACViewer.Instance.GraphicsDevice; }

        public Effect Effect;

        public static bool DrawScenery = true;

        public void Init()
        {
            Effect = new Effect(GraphicsDevice, File.ReadAllBytes("Content/effects.mgfxo"));

            BuildLandVertices();
            BuildModelVertices();
            BuildModelLines();

            if (DrawScenery)
            {
                BuildSceneryVertices();
                BuildSceneryLines();
            }

            BuildPlayer();

            SetUpIndices();
            SetUpBuffers();
        }

        public void BuildLandVertices()
        {
            Console.WriteLine("Setting up landblock vertices");

            var terrainWidth = BlockRange.TerrainWidth;
            var terrainHeight = BlockRange.TerrainHeight;

            Vertices = new VertexPositionColor[terrainWidth * terrainHeight];
            for (var x = 0; x < terrainHeight; x++)
            {
                for (var y = 0; y < terrainHeight; y++)
                {
                    Vertices[x + y * terrainWidth] = new VertexPositionColor();
                    Vertices[x + y * terrainWidth].Position = new Vector3(x * Landblock.CellSize, y * Landblock.CellSize, BlockRange.HeightData[x, y]);
                    Vertices[x + y * terrainWidth].Color = Color.White;
                }
            }
        }

        public void BuildModelVertices()
        {
            Console.WriteLine("Setting up model vertices");

            ModelVertices = new VertexPositionColor[ModelVertexCount];

            var vertexIdx = 0;
            var scale = 1.0f;
            var numModelInstances = 0;
            var numModelInstanceVertices = 0;

            for (var x = 0; x < Landblocks.GetLength(0); x++)
            {
                for (var y = 0; y < Landblocks.GetLength(1); y++)
                {
                    var landblock = Landblocks[x, y];

                    var modelInstances = landblock.LandObjects.Concat(landblock.Buildings);

                    foreach (var modelInstance in modelInstances)
                    {
                        numModelInstances++;

                        foreach (var gfxObj in modelInstance.StaticMesh.GfxObjs)
                        {
                            foreach (var v in gfxObj.VertexArray.Vertices.Values)
                            {
                                var vertex = new VertexPositionColor();
                                vertex.Position = new Vector3(v.X * scale, v.Y * scale, v.Z * scale);
                                vertex.Color = Color.White;

                                // build the translation matrix
                                var translateb = Matrix.CreateTranslation(new Vector3(x * Landblock.LandblockSize, y * Landblock.LandblockSize, 0));  // translate to landblock
                                var translate = Matrix.CreateTranslation(new Vector3(modelInstance.Frame.Origin.X, modelInstance.Frame.Origin.Y, modelInstance.Frame.Origin.Z));    // translate within landblock
                                var rotate = Matrix.CreateFromQuaternion(new Quaternion(modelInstance.Frame.Orientation.X, modelInstance.Frame.Orientation.Y, modelInstance.Frame.Orientation.Z, modelInstance.Frame.Orientation.W));
                                vertex.Position = Vector3.Transform(vertex.Position, rotate * translateb * translate);

                                ModelVertices[vertexIdx++] = vertex;
                                numModelInstanceVertices++;
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Total models: " + StaticMeshCache.Meshes.Count);
            Console.WriteLine("Total model instances: " + numModelInstances);
            Console.WriteLine("Total model instance vertices: " + numModelInstanceVertices);
        }

        public void BuildModelLines()
        {
            Console.WriteLine("Setting up model lines");

            var lines = new List<int>();

            var offset = 0;

            for (var x = 0; x < Landblocks.GetLength(0); x++)
            {
                for (var y = 0; y < Landblocks.GetLength(1); y++)
                {
                    var landblock = Landblocks[x, y];

                    var modelInstances = landblock.LandObjects.Concat(landblock.Buildings);

                    foreach (var model in modelInstances)
                    {
                        foreach (var gfxObj in model.StaticMesh.GfxObjs)
                        {
                            foreach (var poly in gfxObj.Polygons.Values)
                            {
                                var vertexIndices = poly.VertexIds;
                                for (var i = 0; i < vertexIndices.Count; i++)
                                {
                                    lines.Add(vertexIndices[i] + offset);
                                    if (i < vertexIndices.Count - 1)
                                        lines.Add(vertexIndices[i + 1] + offset);
                                    else
                                        lines.Add(vertexIndices[0] + offset);
                                }
                            }
                            offset += gfxObj.VertexArray.Vertices.Count;
                        }
                    }
                }
            }
            ModelLines = lines.ToArray();
        }

        public void BuildSceneryVertices()
        {
            Console.WriteLine("Setting up scenery vertices");

            SceneryVertices = new VertexPositionColor[SceneryVertexCount];

            var vertexIdx = 0;
            var scalar = 1.0f;
            var numModelInstances = 0;
            var numModelInstanceVertices = 0;
            for (var x = 0; x < Landblocks.GetLength(0); x++)
            {
                for (var y = 0; y < Landblocks.GetLength(1); y++)
                {
                    var landblock = Landblocks[x, y];

                    var modelInstances = landblock.Scenery.ModelInstances;

                    foreach (var modelInstance in modelInstances)
                    {
                        numModelInstances++;
                        foreach (var gfxObj in modelInstance.StaticMesh.GfxObjs)
                        {
                            foreach (var v in gfxObj.VertexArray.Vertices.Values)
                            {
                                var vertex = new VertexPositionColor();
                                vertex.Position = new Vector3(v.X * scalar, v.Y * scalar, v.Z * scalar);
                                vertex.Color = Color.White;

                                // build the translation matrix
                                var scale = Matrix.CreateScale(modelInstance.Scale);
                                var rotate = Matrix.CreateFromQuaternion(new Quaternion(modelInstance.Frame.Orientation.X, modelInstance.Frame.Orientation.Y, modelInstance.Frame.Orientation.Z, modelInstance.Frame.Orientation.W));
                                var translatez = BuildTranslateZ(landblock, modelInstance);
                                var translateb = Matrix.CreateTranslation(new Vector3(x * Landblock.LandblockSize, y * Landblock.LandblockSize, 0));  // translate to landblock
                                var translatec = Matrix.CreateTranslation(new Vector3(modelInstance.Cell.X * Landblock.CellSize, modelInstance.Cell.Y * Landblock.CellSize, 0));    // translate to cell
                                var translate = Matrix.CreateTranslation(new Vector3(modelInstance.Position.X, modelInstance.Position.Y, modelInstance.Position.Z));    // translate within cell
                                var rotateb = Matrix.CreateFromQuaternion(new Quaternion(modelInstance.Rotation.X, modelInstance.Rotation.Y, modelInstance.Rotation.Z, modelInstance.Rotation.W));

                                vertex.Position = Vector3.Transform(vertex.Position, scale * rotate * rotateb * translatez * translateb * translatec * translate);

                                SceneryVertices[vertexIdx++] = vertex;
                                numModelInstanceVertices++;
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Total scenery models: " + numModelInstances);
            Console.WriteLine("Total scenery vertices: " + numModelInstanceVertices);
        }

        public void BuildSceneryLines()
        {
            // doit2
            Console.WriteLine("Setting up scenery lines");

            var lines = new List<int>();

            var offset = 0;
            for (var x = 0; x < Landblocks.GetLength(0); x++)
            {
                for (var y = 0; y < Landblocks.GetLength(1); y++)
                {
                    var landblock = Landblocks[x, y];

                    var modelInstances = landblock.Scenery.ModelInstances;

                    foreach (var model in modelInstances)
                    {
                        foreach (var gfxObj in model.StaticMesh.GfxObjs)
                        {
                            foreach (var poly in gfxObj.Polygons.Values)
                            {
                                var vertexIndices = poly.VertexIds;
                                for (var i = 0; i < vertexIndices.Count; i++)
                                {
                                    lines.Add(vertexIndices[i] + offset);
                                    if (i < vertexIndices.Count - 1)
                                        lines.Add(vertexIndices[i + 1] + offset);
                                    else
                                        lines.Add(vertexIndices[0] + offset);
                                }
                            }
                            offset += gfxObj.VertexArray.Vertices.Count;

                        }
                    }
                }
            }
            SceneryLines = lines.ToArray();
        }

        public Matrix BuildTranslateZ(Landblock landblock, ModelInstance modelInstance)
        {
            // get the landblock x/y position
            var x = modelInstance.Cell.X * Landblock.CellSize + modelInstance.Frame.Origin.X;
            var y = modelInstance.Cell.Y * Landblock.CellSize + modelInstance.Frame.Origin.Y;

            var z = GetZ(landblock, new Vector2(x, y));

            return Matrix.CreateTranslation(new Vector3(0, 0, z));
        }

        public float GetZ(Landblock landblock, Vector2 point)
        {
            // find the triangle that contains this x,y
            var triangle = landblock.Mesh.GetTriangle(point);

            // calculate the z coordinate at x,y
            // for the plane defined by this triangle
            var z = triangle.GetZ(landblock.Mesh.Vertices, point);

            // TODO: verify colinear coordinates
            return z;
        }

        public ACE.Diag.Entity.Player WorldObjectPlayer;

        public bool BuildPlayer()
        {
            var player = ACViewer.Instance.Player.GetPlayer();
            if (player == null) return false;

            var radius = player.Radius;
            var pos = player.Location.Pos.ToXna();
            pos.Z += radius;

            // check for landblock change
            if (WorldObjectPlayer == null)
                WorldObjectPlayer = player;

            var updatedLandblock = false;
            if (!WorldObjectPlayer.Location.landblockId.Equals(player.Location.landblockId))
            {
                //Console.WriteLine("Landblock change - " + player.Location.landblockId.Raw.ToString("X8"));
                var landblock = player.Location.landblockId.Raw | 0xFFFF;
                ACViewer.Instance.LoadLandblock(landblock);
                updatedLandblock = true;
            }

            WorldObjectPlayer = player;

            Player = new SpherePrimitive(GraphicsDevice, radius * 2.0f, 10, pos);
            return updatedLandblock;
        }

        public void BuildCreatures()
        {
            var creatures = ACViewer.Instance.Player.GetCreatures();

            Monsters = new List<SpherePrimitive>();

            if (creatures == null)
                return;

            foreach (var creature in creatures)
            {
                var radius = creature.Radius;
                var pos = creature.Location.Pos.ToXna();
                pos.Z += radius;

                var monster = new SpherePrimitive(GraphicsDevice, radius * 2.0f, 10, pos);
                Monsters.Add(monster);
            }
        }

        public void BuildProjectiles()
        {
            var projs = ACViewer.Instance.Player.GetMissiles();

            Projectiles = new List<SpherePrimitive>();

            if (projs == null)
                return;

            foreach (var proj in projs)
            {
                var radius = proj.Radius;
                var pos = proj.Location.Pos.ToXna();
                pos.Z += radius;

                var p = new SpherePrimitive(GraphicsDevice, radius * 2.0f, 10, pos);
                Projectiles.Add(p);
            }
        }

        public void SetUpIndices()
        {
            Console.WriteLine("Setting up indices");
            int firstSplit = 0;
            int secondSplit = 0;
            var terrainWidth = BlockRange.TerrainWidth;
            var terrainHeight = BlockRange.TerrainHeight;

            Indices = new int[(terrainWidth - 1) * (terrainHeight - 1) * 6];
            int counter = 0;
            for (int y = 0; y < terrainHeight - 1; y++)
            {
                for (int x = 0; x < terrainWidth - 1; x++)
                {
                    int lowerLeft = x + y * terrainWidth;
                    int lowerRight = (x + 1) + y * terrainWidth;
                    int topLeft = x + (y + 1) * terrainWidth;
                    int topRight = (x + 1) + (y + 1) * terrainWidth;


                    // Determine which direction to split quad into triangles
                    var splitAlg = true;
                    if (BlockRange.GetSplitDir(x, y) && splitAlg)
                    {
                        Indices[counter++] = topRight;
                        Indices[counter++] = lowerRight;
                        Indices[counter++] = lowerLeft;

                        Indices[counter++] = topRight;
                        Indices[counter++] = lowerLeft;
                        Indices[counter++] = topLeft;

                        firstSplit++;
                    }
                    else
                    {
                        Indices[counter++] = topLeft;
                        Indices[counter++] = lowerRight;
                        Indices[counter++] = lowerLeft;

                        Indices[counter++] = topLeft;
                        Indices[counter++] = topRight;
                        Indices[counter++] = lowerRight;

                        secondSplit++;
                    }
                }
            }

            //Console.WriteLine("First split: " + firstSplit);
            //Console.WriteLine("Second split: " + secondSplit);
        }

        public void SetUpBuffers()
        {
            Console.WriteLine("Setting up buffers");

            VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), Vertices.Length, BufferUsage.WriteOnly);
            VertexBuffer.SetData<VertexPositionColor>(Vertices);

            IndexBuffer = new IndexBuffer(GraphicsDevice, typeof(int), Indices.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData(Indices);

            if (ModelVertices.Length > 0)
            {
                ModelVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), ModelVertices.Length, BufferUsage.WriteOnly);
                ModelVertexBuffer.SetData<VertexPositionColor>(ModelVertices);

                ModelIndexBuffer = new IndexBuffer(GraphicsDevice, typeof(int), ModelLines.Length, BufferUsage.WriteOnly);
                ModelIndexBuffer.SetData(ModelLines);
            }

            if (WeenieVertices != null)
            {
                WeenieVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), WeenieVertices.Length, BufferUsage.WriteOnly);
                WeenieVertexBuffer.SetData<VertexPositionColor>(WeenieVertices);

                WeenieIndexBuffer = new IndexBuffer(GraphicsDevice, typeof(int), WeenieLines.Length, BufferUsage.WriteOnly);
                WeenieIndexBuffer.SetData(WeenieLines);
            }

            if (SceneryVertices != null && SceneryVertices.Length > 0)
            {
                SceneryVertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), SceneryVertices.Length, BufferUsage.WriteOnly);
                SceneryVertexBuffer.SetData<VertexPositionColor>(SceneryVertices);

                SceneryIndexBuffer = new IndexBuffer(GraphicsDevice, typeof(int), SceneryLines.Length, BufferUsage.WriteOnly);
                SceneryIndexBuffer.SetData(SceneryLines);
            }
        }
    }
}
