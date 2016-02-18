using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Logic.VirtualCreatures
{
    /// <summary>
    /// This defines a shape, excluding position and rotation. 
    /// The shape also has a right handed coordinate system with X, Y and Z.
    /// </summary>
    public interface ShapeSpecification
    {
        UnityEngine.Object createUnityObject();
    }

    public class Rectangle : ShapeSpecification
    {
        internal float width, depth, height;

        public Rectangle(float width, float depth, float height)
        {
            this.width = width;
            this.depth = depth;
            this.height = height;
        }

        public UnityEngine.Object createUnityObject()
        {
            throw new NotImplementedException();
        }
    }

    public class Cube : Rectangle
    {
        public Cube(float scale) : base(scale, scale, scale) { }
    }

    public class LongRectangle : Rectangle
    {
        public LongRectangle(float size, float factor) : base(size, size, factor * size) { }
    }

    public class PlaneRectangle : Rectangle
    {
        public static float width = 0.2f;
        public PlaneRectangle(float size, float factor) : base(size, width, size * factor) { }
    }

    public class Sphere : ShapeSpecification
    {
        internal float r;
        public Sphere(float r)
        {
            this.r = r;
        }
        public UnityEngine.Object createUnityObject()
        {
            throw new NotImplementedException();
        }
    }
}
