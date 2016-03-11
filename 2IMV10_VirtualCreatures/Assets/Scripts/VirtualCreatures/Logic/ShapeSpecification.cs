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
    /// Every shape has a root
    /// </summary>
    public abstract class ShapeSpecification
    {
        protected virtual String getGameObjectName()
        {
            return this.GetType().Name + " [" + this.getSize().ToString() + "]";
        }
        internal abstract GameObject _primitive();
        public GameObject createUnscaledPrimitive()
        {
            GameObject primitive = this._primitive();
            this._setColider(primitive.GetComponent<Collider>());
            Mesh mesh = primitive.GetComponent<MeshFilter>().mesh;
            mesh.vertices = mesh.vertices.Select(v => new Vector3(v.x * this.getXBound(), v.y * this.getYBound(), v.z * this.getZBound())).ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            primitive.AddComponent<Rigidbody>();
            return primitive;
        }

        internal abstract void _setColider(Collider collider);

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
        public Vector3 getBounds() { return new Vector3(this.getXBound(), this.getYBound(), this.getZBound()); }
        public Vector3 getSize() { return 2 * this.getBounds(); }
        public float getXSize() { return 2 * this.getXBound(); }
        public float getYSize() { return 2 * this.getYBound(); }
        public float getZSize() { return 2 * this.getZBound(); }
    }

    public class Rectangle : ShapeSpecification
    {
        internal float factorRight;
        internal float factorForwards;
        internal float factorUp;

        /// <summary>
        /// Create a rectable with a certain size.
        /// (everything is measured from edge to edge)
        /// </summary>
        /// <param name="width">>0 (in left right direction)</param>
        /// <param name="thickness">>0 (in up/down direction)</param>
        /// <param name="height">>0 (in forwards direction)</param>
        public Rectangle(float width, float thickness, float height)
        {
            if (width < 0 || thickness < 0 || height < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            this.factorRight = width / 2;
            this.factorUp = thickness / 2;
            this.factorForwards = height / 2;
        }

        public static Rectangle createWidthDepthHeight(float width, float depth, float height)
        {
            return new Rectangle(width, depth, height);
        }
        public static Rectangle createCube(float size) { return new Rectangle(size, size, size); }
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
            float width = height;
            return new Rectangle(width, width, height);
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
            return new Rectangle(width, DEPTH, height);
        }

        public override float getXBound() { return this.factorRight; }
        public override float getYBound() { return this.factorUp; }
        public override float getZBound() { return this.factorForwards; }

        internal override GameObject _primitive()
        {
            return GameObject.CreatePrimitive(PrimitiveType.Cube);
        }

        internal override void _setColider(Collider collider)
        {
            BoxCollider bc = (BoxCollider)collider;
            bc.size = this.getBounds();
        }
    }

    public class Sphere : ShapeSpecification
    {
        internal float r;
        /// <summary>
        /// Create a sphere with a certain diameter
        /// </summary>
        /// <param name="diameter"></param>
        public Sphere(float diameter)
        {
            if (diameter < 0) throw new ArgumentOutOfRangeException();
            this.r = diameter / 2;
        }
        public override float getXBound() { return this.r; }
        public override float getYBound() { return this.r; }
        public override float getZBound() { return this.r; }
        internal override GameObject _primitive()
        {
            return GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }

        internal override void _setColider(Collider collider)
        {
            SphereCollider sp = (SphereCollider)collider;
            sp.radius = this.r;
        }
    }
}
