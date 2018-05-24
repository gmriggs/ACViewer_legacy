using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ACViewer.Render
{
    public class Render
    {
        public ACViewer ACViewer { get => ACViewer.Instance; }
        public GraphicsDevice GraphicsDevice { get => ACViewer.GraphicsDevice; }
        public Effect Effect { get => Setup.Effect; }

        public Setup Setup;
        public Camera Camera { get => ACViewer.Camera; set => ACViewer.Camera = value; }

        public Render()
        {
            Setup = new Setup();
        }

        public void Init()
        {
            Setup.Init();

            if (Camera == null)
                Camera = new Camera(ACViewer.Instance);
        }

        public void Draw()
        {
            GraphicsDevice.Clear(new Color(48, 48, 48));

            var rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            rs.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = rs;

            Effect.CurrentTechnique = Effect.Techniques["ColoredNoShading"];
            Effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            Effect.Parameters["xView"].SetValue(Camera.ViewMatrix);
            Effect.Parameters["xProjection"].SetValue(Camera.ProjectionMatrix);

            DrawLand();
            DrawModels();
            //DrawWeenies();
            DrawScenery();

            DrawPlayer();
            DrawMonsters();
        }


        public void DrawLand()
        {
            GraphicsDevice.SetVertexBuffer(Setup.VertexBuffer);
            GraphicsDevice.Indices = Setup.IndexBuffer;

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                //GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indices, 0, Indices.Length / 3, VertexPositionColor.VertexDeclaration);
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Setup.Indices.Length / 3);
            }
        }

        public void DrawModels()
        {
            if (Setup.ModelVertices.Length == 0) return;

            GraphicsDevice.SetVertexBuffer(Setup.ModelVertexBuffer);
            GraphicsDevice.Indices = Setup.ModelIndexBuffer;

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                //GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indices, 0, Indices.Length / 3, VertexPositionColor.VertexDeclaration);
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, Setup.ModelLines.Length);
            }
        }

        public void DrawWeenies()
        {
            if (Setup.WeenieVertices == null || Setup.WeenieVertices.Length == 0) return;

            GraphicsDevice.SetVertexBuffer(Setup.WeenieVertexBuffer);
            GraphicsDevice.Indices = Setup.WeenieIndexBuffer;

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                //GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indices, 0, Indices.Length / 3, VertexPositionColor.VertexDeclaration);
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, Setup.WeenieLines.Length);
            }
        }

        public void DrawScenery()
        {
            if (Setup.SceneryVertices == null || Setup.SceneryVertices.Length == 0) return;

            GraphicsDevice.SetVertexBuffer(Setup.SceneryVertexBuffer);
            GraphicsDevice.Indices = Setup.SceneryIndexBuffer;

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                //GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indices, 0, Indices.Length / 3, VertexPositionColor.VertexDeclaration);
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, Setup.SceneryLines.Length);
            }
        }

        public void DrawPlayer()
        {
            if (Setup.Player == null) return;

            Setup.Player.Draw(Matrix.Identity, Camera.ViewMatrix, Camera.ProjectionMatrix, Color.Yellow);
        }

        public void DrawMonsters()
        {
            if (Setup.Monsters == null) return;

            foreach (var monster in Setup.Monsters)
            {
                monster.Draw(Matrix.Identity, Camera.ViewMatrix, Camera.ProjectionMatrix, Color.Red);
            }
        }
    }
}
