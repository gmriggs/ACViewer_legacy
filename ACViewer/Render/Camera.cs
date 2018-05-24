using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ACViewer
{
    public class Camera
    {
        public ACViewer Game;

        public Matrix ViewMatrix;
        public Matrix ProjectionMatrix;

        public Vector3 Position;
        public Vector3 Dir;
        public Vector3 Up;

        public MouseState PrevMouseState;
        public int PrevScrollWheelValue;

        public float Speed = 2.0f;
        public float SpeedMod = 1.5f;

        public int DrawDistance = 100000;

        public float FieldOfView = 90.0f;

        public Camera(ACViewer game)
        {
            Game = game;
            Init();
        }

        public void Init()
        {
            Console.WriteLine("Setting up camera");

            var vertices = ACViewer.Instance.Render.Setup.Vertices;

            var dist = 50.0f;
            Position = new Vector3(vertices[0].Position.X - dist, vertices[0].Position.Y - dist, vertices[0].Position.Z + dist);
            Dir = new Vector3(1.0f, 1.0f, 0);
            Dir.Normalize();
            Up = Vector3.UnitZ;

            CreateLookAt();
            CreateProjection();

            SetMouse();
        }

        public Matrix CreateLookAt()
        {
            return ViewMatrix = Matrix.CreateLookAt(Position, Position + Dir, Up);
        }

        public Matrix CreateProjection()
        {
            return ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                 FieldOfView * 0.0174533f / 2,       // degrees to radians
                 (float)Game.GraphicsDevice.Viewport.Width /
                 (float)Game.GraphicsDevice.Viewport.Height,
                 0.0001f,
                 DrawDistance);
        }

        public void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();

            if (!Game.IsActive)
            {
                PrevMouseState = mouseState;
                return;
            }
            if (keyboardState.IsKeyDown(Keys.W))
                Position += Dir * Speed;
            if (keyboardState.IsKeyDown(Keys.S))
                Position -= Dir * Speed;
            if (keyboardState.IsKeyDown(Keys.A))
                Position += Vector3.Cross(Up, Dir) * Speed;
            if (keyboardState.IsKeyDown(Keys.D))
                Position -= Vector3.Cross(Up, Dir) * Speed;
            if (keyboardState.IsKeyDown(Keys.Space))
                Position += Up * Speed;

            // camera speed control
            if (mouseState.ScrollWheelValue != PrevScrollWheelValue)
            {
                var diff = mouseState.ScrollWheelValue - PrevScrollWheelValue;
                if (diff >= 0)
                    Speed *= SpeedMod;
                else
                    Speed /= SpeedMod;

                PrevScrollWheelValue = mouseState.ScrollWheelValue;
            }

            // yaw / x-rotation
            Dir = Vector3.Transform(Dir, Matrix.CreateFromAxisAngle(Up,
                -MathHelper.PiOver4 / 160 * (mouseState.X - PrevMouseState.X)));

            // pitch / y-rotation
            Dir = Vector3.Transform(Dir, Matrix.CreateFromAxisAngle(Vector3.Cross(Up, Dir),
                MathHelper.PiOver4 / 160 * (mouseState.Y - PrevMouseState.Y)));

            Dir.Normalize();

            SetMouse();

            CreateLookAt();
        }

        public void SetMouse()
        {
            // set mouse position to center of window
            Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
            PrevMouseState = Mouse.GetState();
        }
    }
}
