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
        protected abstract GameObject createPrimitiveImpl();
        public GameObject createPrimitive()
        {
            GameObject primitive = createPrimitiveImpl();
            primitive.name = this.getGameObjectName();
            return primitive;
        }
        public abstract void setMesh(Mesh mesh);
        public GameObject createNaive()
        {
            GameObject g = new GameObject("naiveGameObject");
            MeshFilter filter = g.AddComponent<MeshFilter>();
            Mesh mesh = filter.mesh;
            mesh.Clear();
            this.setMesh(mesh);
            BoxCollider bc = g.AddComponent<BoxCollider>();
            bc.size = this.getSize();
            MeshRenderer renderer = g.AddComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Diffuse"));
            Rigidbody rb = g.AddComponent<Rigidbody>();
            return g;
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
        public float getBound(int face)
        {
            switch (face)
            {
                case 1: // Same Direction
                case 6: // Backwards
                default:
                    return this.getYBound();
                case 2: // Right
                case 4: // Left
                    return this.getXBound();
                case 3: // Away
                case 5: // Towards
                    return this.getZBound();
            }
        }
        public Vector3 getBounds() { return new Vector3(this.getXBound(), this.getYBound(), this.getZBound()); }
        public Vector3 getSize() { return 2 * this.getBounds(); }
        public float getXSize() { return 2 * this.getXBound(); }
        public float getYSize() { return 2 * this.getYBound(); }
        public float getZSize() { return 2 * this.getZBound(); }
    }

    public class Rectangle : ShapeSpecification
    {
        /// <summary>
        /// The total size of the rectangle from the left to the right
        /// </summary>
        internal float width;
        /// <summary>
        /// The total size of the rectangle from the front to the back
        /// </summary>
        internal float depth;
        /// <summary>
        /// The total size of the rectangle from the bottom to the top
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

        public static Rectangle createWidthDepthHeight(float width, float depth, float height)
        {
            return new Rectangle(width, depth, height);
        }

        public override void setMesh(Mesh mesh)
        {
            float length = this.depth;
            float width = this.width;
            float height = this.height;

            #region Vertices
            Vector3 p0 = new Vector3(-length * .5f, -width * .5f, height * .5f);
            Vector3 p1 = new Vector3(length * .5f, -width * .5f, height * .5f);
            Vector3 p2 = new Vector3(length * .5f, -width * .5f, -height * .5f);
            Vector3 p3 = new Vector3(-length * .5f, -width * .5f, -height * .5f);
            Vector3 p4 = new Vector3(-length * .5f, width * .5f, height * .5f);
            Vector3 p5 = new Vector3(length * .5f, width * .5f, height * .5f);
            Vector3 p6 = new Vector3(length * .5f, width * .5f, -height * .5f);
            Vector3 p7 = new Vector3(-length * .5f, width * .5f, -height * .5f);

            Vector3[] vertices = new Vector3[]
            {
	            // Bottom
	            p0, p1, p2, p3,
	            // Left
	            p7, p4, p0, p3,
	            // Front
	            p4, p5, p1, p0,
	            // Back
	            p6, p7, p3, p2,
	            // Right
	            p5, p6, p2, p1,
	            // Top
	            p7, p6, p5, p4
            };
            #endregion

            #region Normales
            Vector3 up = Vector3.up;
            Vector3 down = Vector3.down;
            Vector3 front = Vector3.forward;
            Vector3 back = Vector3.back;
            Vector3 left = Vector3.left;
            Vector3 right = Vector3.right;

            Vector3[] normales = new Vector3[]
            {
	            // Bottom
	            down, down, down, down,
	            // Left
	            left, left, left, left,
	            // Front
	            front, front, front, front,
	            // Back
	            back, back, back, back,
	            // Right
	            right, right, right, right,
	            // Top
	            up, up, up, up
            };
            #endregion

            #region UVs
            Vector2 _00 = new Vector2(0f, 0f);
            Vector2 _10 = new Vector2(1f, 0f);
            Vector2 _01 = new Vector2(0f, 1f);
            Vector2 _11 = new Vector2(1f, 1f);

            Vector2[] uvs = new Vector2[]
            {
	            // Bottom
	            _11, _01, _00, _10,
	            // Left
	            _11, _01, _00, _10,
	            // Front
	            _11, _01, _00, _10,
	            // Back
	            _11, _01, _00, _10,
	            // Right
	            _11, _01, _00, _10,
	            // Top
	            _11, _01, _00, _10,
            };
            #endregion

            #region Triangles
            int[] triangles = new int[]
            {
	            // Bottom
	            3, 1, 0,
                3, 2, 1,		
	            // Left
	            3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
                3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
	            // Front
	            3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
                3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
	            // Back
	            3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
                3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
	            // Right
	            3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
                3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
	            // Top
	            3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
                3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,
            };
            #endregion

            mesh.vertices = vertices;
            mesh.normals = normales;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            mesh.Optimize();
        }

        protected override GameObject createPrimitiveImpl()
        {
            // Create a primitive with mesh renderer and collider attached.
            GameObject rectangle = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // Attach a Rigid body to the mesh
            rectangle.AddComponent<Rigidbody>();

            // Transform mesh to the scale of this shape specification
            rectangle.transform.localScale = new Vector3(width, height, depth);

            return rectangle;
        }

        public override float getXBound() { return this.width / 2; }
        public override float getYBound() { return this.height / 2; }
        public override float getZBound() { return this.depth / 2; }
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
        /// <param name="length">the width of the beam</param>
        /// <param name="widthRatio">the factor of the height of the beam, must be greater then 1</param>
        public LongRectangle(float length, float widthRatio) : base(length * widthRatio, length * widthRatio, length)
        {
            if (widthRatio > 1)
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
        public static float DEPTH = 0.2f;
        /// <summary>
        /// A plane like rectangle standing upwards.
        /// </summary>
        /// <param name="length">The size of the plane</param>
        /// <param name="widthRatio">A factor less or equal to 1 to make the plane less square, same as in LongRectangle</param>
        public PlaneRectangle(float length, float widthRatio) : base(length * widthRatio, DEPTH, length)
        {
            if (widthRatio > 1)
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class Sphere : ShapeSpecification
    {
        internal float diameter;
        /// <summary>
        /// Create a sphere with a certain diameter
        /// </summary>
        /// <param name="r"></param>
        public Sphere(float diameter)
        {
            this.diameter = diameter;
        }

        public override void setMesh(Mesh mesh) { meshUVSphere(mesh); }

        private void meshUVSphere(Mesh mesh)
        {
            float radius = this.diameter/2;
            // Longitude |||
            int nbLong = 16;
            // Latitude ---
            int nbLat = 12;

            #region Vertices
            Vector3[] vertices = new Vector3[(nbLong + 1) * nbLat + 2];
            float _pi = Mathf.PI;
            float _2pi = _pi * 2f;

            vertices[0] = Vector3.up * radius;
            for (int lat = 0; lat < nbLat; lat++)
            {
                float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
                float sin1 = Mathf.Sin(a1);
                float cos1 = Mathf.Cos(a1);

                for (int lon = 0; lon <= nbLong; lon++)
                {
                    float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
                    float sin2 = Mathf.Sin(a2);
                    float cos2 = Mathf.Cos(a2);

                    vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
                }
            }
            vertices[vertices.Length - 1] = Vector3.up * -radius;
            #endregion

            #region Normales		
            Vector3[] normales = new Vector3[vertices.Length];
            for (int n = 0; n < vertices.Length; n++)
                normales[n] = vertices[n].normalized;
            #endregion

            #region UVs
            Vector2[] uvs = new Vector2[vertices.Length];
            uvs[0] = Vector2.up;
            uvs[uvs.Length - 1] = Vector2.zero;
            for (int lat = 0; lat < nbLat; lat++)
                for (int lon = 0; lon <= nbLong; lon++)
                    uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));
            #endregion

            #region Triangles
            int nbFaces = vertices.Length;
            int nbTriangles = nbFaces * 2;
            int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[nbIndexes];

            //Top Cap
            int i = 0;
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = lon + 2;
                triangles[i++] = lon + 1;
                triangles[i++] = 0;
            }

            //Middle
            for (int lat = 0; lat < nbLat - 1; lat++)
            {
                for (int lon = 0; lon < nbLong; lon++)
                {
                    int current = lon + lat * (nbLong + 1) + 1;
                    int next = current + nbLong + 1;

                    triangles[i++] = current;
                    triangles[i++] = current + 1;
                    triangles[i++] = next + 1;

                    triangles[i++] = current;
                    triangles[i++] = next + 1;
                    triangles[i++] = next;
                }
            }

            //Bottom Cap
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = vertices.Length - 1;
                triangles[i++] = vertices.Length - (lon + 2) - 1;
                triangles[i++] = vertices.Length - (lon + 1) - 1;
            }
            #endregion

            mesh.vertices = vertices;
            mesh.normals = normales;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            mesh.Optimize();
        }

        protected override GameObject createPrimitiveImpl()
        {
            // Create a primitive with mesh renderer and collider attached.
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            // Attach a Rigid body to the mesh
            sphere.AddComponent<Rigidbody>();

            // Transform mesh to the scale of this shape specification
            float r = diameter / 2;
            sphere.transform.localScale = new Vector3(r, r, r);

            return sphere;
        }

        public override float getXBound() { return this.diameter / 2; }
        public override float getYBound() { return this.diameter / 2; }
        public override float getZBound() { return this.diameter / 2; }
    }
}
