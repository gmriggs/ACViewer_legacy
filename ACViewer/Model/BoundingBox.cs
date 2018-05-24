using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ACViewer
{
    /// <summary>
    /// A bounding box performs collision detection
    /// </summary>
    public class BoundingBox
    {
        public StaticMesh Model;
        public ModelInstance ModelInstance;

        public Vector3 Min;
        public Vector3 Max;

        public Vector3 Center;
        public Vector3 Size;

        public BoundingBox(StaticMesh model)
        {
            Model = model;
            BuildBox(model);
        }

        public BoundingBox(ModelInstance modelInstance)
        {
            ModelInstance = modelInstance;
            BuildBox(modelInstance);
        }


        public BoundingBox(List<ACE.DatLoader.Entity.Polygon> polys, System.Numerics.Matrix4x4 transform)
        {
            Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var poly in polys)
            {
                foreach (var vertex in poly.Vertices)
                {
                    var v = System.Numerics.Vector3.Transform(new System.Numerics.Vector3(vertex.X, vertex.Y, vertex.Z), transform);

                    if (v.X < Min.X) Min.X = v.X;
                    if (v.Y < Min.Y) Min.Y = v.Y;
                    if (v.Z < Min.Z) Min.Z = v.Z;

                    if (v.X > Max.X) Max.X = v.X;
                    if (v.Y > Max.Y) Max.Y = v.Y;
                    if (v.Z > Max.Z) Max.Z = v.Z;
                }
            }
        }

        public void BuildBox(StaticMesh model)
        {
            Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var gfxObj in model.GfxObjs)
            {
                foreach (var vertex in gfxObj.VertexArray.Vertices.Values)
                {
                    if (vertex.X < Min.X) Min.X = vertex.X;
                    if (vertex.Y < Min.Y) Min.Y = vertex.Y;
                    if (vertex.Z < Min.Z) Min.Z = vertex.Z;

                    if (vertex.X > Max.X) Max.X = vertex.X;
                    if (vertex.Y > Max.Y) Max.Y = vertex.Y;
                    if (vertex.Z > Max.Z) Max.Z = vertex.Z;
                }
            }

            GetSize();
        }

        public void BuildBox(ModelInstance modelInstance)
        {
            Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // build the transformation matrix
            var scale = Matrix.CreateScale(modelInstance.Scale);
            var rotate = Matrix.CreateFromQuaternion(new Quaternion(modelInstance.Frame.Orientation.X, modelInstance.Frame.Orientation.Y, modelInstance.Frame.Orientation.Z, modelInstance.Frame.Orientation.W));
            var cellTranslate = Matrix.CreateTranslation(new Vector3(modelInstance.Cell.X * Landblock.CellSize, modelInstance.Cell.Y * Landblock.CellSize, 0));
            var cellTranslateInner = Matrix.CreateTranslation(new Vector3(modelInstance.Position.X, modelInstance.Position.Y, modelInstance.Position.Z));

            var transform = scale * rotate * cellTranslate * cellTranslateInner;

            foreach (var gfxObj in modelInstance.StaticMesh.GfxObjs)
            {
                foreach (var v in gfxObj.VertexArray.Vertices.Values)
                {
                    var vertex = Vector3.Transform(new Vector3(v.X, v.Y, v.Z), transform);

                    if (vertex.X < Min.X) Min.X = vertex.X;
                    if (vertex.Y < Min.Y) Min.Y = vertex.Y;
                    if (vertex.Z < Min.Z) Min.Z = vertex.Z;

                    if (vertex.X > Max.X) Max.X = vertex.X;
                    if (vertex.Y > Max.Y) Max.Y = vertex.Y;
                    if (vertex.Z > Max.Z) Max.Z = vertex.Z;
                }
            }

            GetSize();
        }

        public void GetSize()
        {
            Size = new Vector3(Max.X - Min.X, Max.Y - Min.Y, Max.Z - Min.Z);

            Center = new Vector3(Min.X + Size.X / 2, Min.Y + Size.Y / 2, Min.Z + Size.Z / 2);
        }

        public void Scale(float factor)
        {
            Min = new Vector3(Center.X - Size.X / 2 * factor, Center.Y - Size.Y / 2 * factor, Center.Z - Size.Z / 2 * factor);
            Max = new Vector3(Center.X + Size.X / 2 * factor, Center.Y + Size.Y / 2 * factor, Center.Z + Size.Z / 2 * factor);

            GetSize();
        }

        public void SetSize(float size)
        {
            Min = new Vector3(Center.X - size / 2, Center.Y - size / 2, Center.Z - size / 2);
            Max = new Vector3(Center.X + size / 2, Center.Y + size / 2, Center.Z + size / 2);

            GetSize();
        }

        /// <summary>
        /// Returns TRUE if point is inside box
        /// </summary>
        public bool Contains(Vector3 point)
        {
            return (point.X >= Min.X && point.X <= Max.X) &&
                   (point.Y >= Min.Y && point.Y <= Max.Y) &&
                   (point.Z >= Min.Z && point.Z <= Max.Z);
        }

        /// <summary>
        /// Returns TRUE if bounding boxes are touching
        /// </summary>
        public bool Intersect(BoundingBox b)
        {
            return (Min.X <= b.Max.X && Max.X >= b.Min.X) &&
                   (Min.Y <= b.Max.Y && Max.Y >= b.Min.Y) /*&&
                   (Min.Z <= b.Max.Z && Max.Z >= b.Min.Z)*/;
        }

        /// <summary>
        /// Returns 8 corner points of the bounding box
        /// </summary>
        public List<Vector3> GetCornerPoints()
        {
            // lower corner points
            var lowerNW = new Vector3(Min.X, Min.Y, Max.Z);
            var lowerNE = new Vector3(Max.X, Min.Y, Max.Z);
            var lowerSW = new Vector3(Min.X, Min.Y, Min.Z);
            var lowerSE = new Vector3(Max.X, Min.Y, Min.Z);

            // upper corner points
            var upperNW = new Vector3(Min.X, Max.Y, Max.Z);
            var upperNE = new Vector3(Max.X, Max.Y, Max.Z);
            var upperSW = new Vector3(Min.X, Max.Y, Min.Z);
            var upperSE = new Vector3(Max.X, Max.Y, Min.Z);

            return new List<Vector3>() { lowerNW, lowerNE, lowerSW, lowerSE, upperNW, upperNE, upperSW, upperSE };
        }
    }
}
