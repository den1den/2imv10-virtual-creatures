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
    public interface ShapeSpecification
    {
        GameObject createPrimitive(GameObject parentPrimitive, Joint parentJoint);
        float getXBound();
        float getYBound();
        float getZBound();
    }

    public class Rectangle : ShapeSpecification
    {
        /// <summary>
        /// The size of the rectangle from the center to the edge in the positive (and negatve) right direction (X)
        /// </summary>
        internal float width;
        /// <summary>
        /// The size of the rectangle from the center to the edge in the positive (and negatve) forwards direction (Y)
        /// </summary>
        internal float depth;
        /// <summary>
        /// The size of the rectangle from the center to the edge in the positive (and negatve) up direction (Z)
        /// </summary>
        internal float height;

        public Rectangle(float width, float depth, float height)
        {
            if (width < 0 || depth < 0 || height < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            this.width = width;
            this.depth = depth;
            this.height = height;
        }

        public GameObject createPrimitive(GameObject parentPrimitive, Joint parentJoint)
        {
            // Create a primitive with mesh renderer and collider attached.
            GameObject rectangle = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // Attach a Rigid body to the mesh
            rectangle.AddComponent<Rigidbody>();
            
            // Transform mesh to the scale of this shape specification
            rectangle.transform.localScale = new Vector3(width, depth, height);

            if (parentJoint != null)
                rectangle.transform.position = parentPrimitive.transform.position + parentJoint.anchor ;
            else
                rectangle.transform.position = parentPrimitive.transform.position;

            return rectangle;
        }

        public float getXBound() { return this.width; }
        public float getYBound() { return this.depth; }
        public float getZBound() { return this.height; }
    }

    /// <summary>
    /// A rectangle with equal sides
    /// </summary>
    public class Cube : Rectangle
    {
        public Cube(float scale) : base(scale, scale, scale) { }
    }

    public class LongRectangle : Rectangle
    {
        /// <summary>
        /// A beam with has a scaled extension in the up direction (for arms)
        /// </summary>
        /// <param name="size">the width of the beam</param>
        /// <param name="factor">the factor of the height of the beam, must be greater then 1</param>
        public LongRectangle(float size, float factor) : base(size, size, size * factor)
        {
            if (factor <= 1)
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class PlaneRectangle : Rectangle
    {
        /// <summary>
        /// The with of all the planes.
        /// </summary>
        public static float width = 0.2f;
        /// <summary>
        /// A plane like rectangle standing upwards.
        /// </summary>
        /// <param name="size">The size of the plane</param>
        /// <param name="factor">A factor to make the plane less square, same as in LongRectangle</param>
        public PlaneRectangle(float size, float factor) : base(size, width, size * factor)
        {
            if (factor <= 1)
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class Sphere : ShapeSpecification
    {
        internal float r;
        /// <summary>
        /// Create a sphere with a certain radius
        /// </summary>
        /// <param name="r"></param>
        public Sphere(float r)
        {
            this.r = r;
        }

        public GameObject createPrimitive(GameObject parentPrimitive, Joint parentJoint)
        {
            // Create a primitive with mesh renderer and collider attached.
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            // Attach a Rigid body to the mesh
            sphere.AddComponent<Rigidbody>();

            // Transform mesh to the scale of this shape specification
            sphere.transform.localScale = new Vector3(r, r, r);

            // 
            if(parentJoint != null)
                sphere.transform.position = parentPrimitive.transform.position + parentJoint.anchor;
            else
                sphere.transform.position = parentPrimitive.transform.position;

            return sphere;
        }

        public float getXBound() { return this.r; }
        public float getYBound() { return this.r; }
        public float getZBound() { return this.r; }
    }
}
