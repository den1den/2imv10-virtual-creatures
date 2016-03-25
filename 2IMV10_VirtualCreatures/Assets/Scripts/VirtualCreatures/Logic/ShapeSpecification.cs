using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VirtualCreatures
{
    /// <summary>
    /// This defines a shape, excluding position and rotation. 
    /// The shape also has a right handed coordinate system with X, Y and Z.
    /// </summary>
    public abstract class ShapeSpecification
    {
        protected virtual String getGameObjectName()
        {
            return this.GetType().Name + " [" + this.getSize().ToString() + "]";
        }

        /// <summary>
        /// Distance from center to the bounding plane in the X direction (left right in f.o.r.)
        /// </summary>
        /// <returns>float > 0</returns>
        public abstract float getXBound();
        /// <summary>
        /// Distance from center to the bounding plane in the Y direction (up down in f.o.r.)
        /// </summary>
        /// <returns>float > 0</returns>
        public abstract float getYBound();
        /// <summary>
        /// Distance from center to the bounding plane in the Z direction (forwards backwards in f.o.r.)
        /// </summary>
        /// <returns>float > 0</returns>
        public abstract float getZBound();
        /// <summary>
        /// Distance from center to the bounding plane in the certain direction
        /// </summary>
        /// <returns>float > 0</returns>
        public float getBound(Face face)
        {
            switch (face)
            {
                case Face.FORWARDS:
                case Face.REVERSE:
                    return this.getZBound();
                case Face.RIGHT:
                case Face.LEFT:
                    return this.getXBound();
                case Face.UP:
                case Face.DOWN:
                    return this.getYBound();
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Size of the shape from the center
        /// </summary>
        /// <returns></returns>
        public Vector3 getBounds() { return new Vector3(this.getXBound(), this.getYBound(), this.getZBound()); }

        /// <summary>
        /// Total size in each direction
        /// </summary>
        /// <returns>2 * getBounds</returns>
        public Vector3 getSize() { return 2 * this.getBounds(); }
        
        public float getXSize() { return 2 * this.getXBound(); }
        public float getYSize() { return 2 * this.getYBound(); }
        public float getZSize() { return 2 * this.getZBound(); }

        public abstract ShapeSpecification deepCopy();
    }

    public class Rectangle : ShapeSpecification
    {
        internal float factorRight;
        internal float factorForwards;
        internal float factorUp;

        /// <summary>
        /// Create a rectable with a certain size.
        /// (everything is measured from center to edge)
        /// </summary>
        /// <param name="factorRight">>0 (in left right direction)</param>
        /// <param name="factorUp">>0 (in up/down direction)</param>
        /// <param name="factorForwards">>0 (in forwards direction)</param>
        Rectangle(float factorRight, float factorUp, float factorForwards)
        {
            this.factorRight = factorRight;
            this.factorUp = factorUp;
            this.factorForwards = factorForwards;
        }

        public static Rectangle createWidthDepthHeight(float width, float depth, float height)
        {
            if (width < 0 || depth < 0 || height < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            return new Rectangle(width / 2, depth / 2, height / 2);
        }
        public static Rectangle createCube(float size) { return createWidthDepthHeight(size, size, size); }
        /// <summary>
        /// A beam with has a scaled extension in the up direction (for arms)
        /// </summary>
        /// <param name="height">the width of the beam</param>
        /// <param name="widthRatio">the factor of the height of the beam, must be greater then 1</param>
        public static Rectangle createPilar(float height, float widthRatio)
        {
            if (widthRatio > 1)
            {
                throw new ArgumentOutOfRangeException();
            }
            float width = height * widthRatio;
            return createWidthDepthHeight(width, width, height);
        }
        /// <summary>
        /// The with of all the planes.
        /// </summary>
        public static float DEPTH = 0.2f;
        /// <summary>
        /// A plane like rectangle standing upwards.
        /// </summary>
        /// <param name="length">The size of the plane</param>
        /// <param name="widthRatio">A factor less or equal to 1 to make the plane less square, same as in LongRectangle</param>
        public static Rectangle createPlane(float height, float widthRatio)
        {
            if (widthRatio > 1)
            {
                throw new ArgumentOutOfRangeException();
            }
            float width = height * widthRatio;
            return createWidthDepthHeight(width, DEPTH, height);
        }

        public override float getXBound() { return this.factorRight; }
        public override float getYBound() { return this.factorUp; }
        public override float getZBound() { return this.factorForwards; }

        public override ShapeSpecification deepCopy()
        {
            return new Rectangle(this.factorRight, this.factorUp, this.factorForwards);
        }
    }

    public class Sphere : ShapeSpecification
    {
        internal float r;
        /// <summary>
        /// Create a sphere with a certain radius
        /// </summary>
        /// <param name="radius"></param>
        Sphere(float radius)
        {
            this.r = radius;
        }

        public static Sphere create(float diameter)
        {
            if (diameter < 0) throw new ArgumentOutOfRangeException();
            return new Sphere(diameter / 2);
        }

        public override float getXBound() { return this.r; }
        public override float getYBound() { return this.r; }
        public override float getZBound() { return this.r; }
        public float getRadius() { return this.r; }

        public override ShapeSpecification deepCopy()
        {
            return Sphere.create(this.r);
        }
    }
}
