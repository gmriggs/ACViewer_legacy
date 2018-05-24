namespace ACViewer
{
    public static class Vector3Extensions
    {
        public static System.Numerics.Vector3 ToNumerics(this Microsoft.Xna.Framework.Vector3 v)
        {
            return new System.Numerics.Vector3(v.X, v.Y, v.Z);
        }

        public static Microsoft.Xna.Framework.Vector3 ToXna(this System.Numerics.Vector3 v)
        {
            return new Microsoft.Xna.Framework.Vector3(v.X, v.Y, v.Z);
        }
    }
}
