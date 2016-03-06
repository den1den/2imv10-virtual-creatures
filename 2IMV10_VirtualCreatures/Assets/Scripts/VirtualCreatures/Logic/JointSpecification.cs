using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VirtualCreatures
{
    /// <summary>
    /// This defines a single joint between two shapes.
    /// It includes the relative initial position of the attached object.
    /// </summary>
    public class JointSpecification
    {
        public float angle; //(-Pi, Pi)
        public float inclination; //(0, Pi/2)
        public JointPosition position;
        public JointType type;
        public float[] limits;

        /// <param name="position">The position of this joint</param>
        /// <param name="initInclination">The initial angle with the normal of this position.face (0, Pi/2)</param>
        /// <param name="initAngle">The angle with respect to the normal vector between the up vector of the source node and the up vector of the attached object, before rotation. In the case of abiguity the X axis is used. (-Pi, Pi) exclusing -Pi</param>
        /// <param name="type">The type of this joint</param>
        /// <param name="limits">Symmetrical limits on each degree of freedom. domain depends on the joint but is in radians (0, max Pi/2)</param>
        public JointSpecification(JointPosition position, float initInclination, float initAngle, JointType type, float[] limits)
        {
            if (initInclination < 0 || initInclination > Math.PI/2)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (initAngle <= -Math.PI || initAngle > Math.PI)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (limits.Length != type.getDegreesOfFreedom())
            {
                throw new ArgumentOutOfRangeException();
            }
            foreach(float limit in limits)
            {
                if(limit < 0) throw new ArgumentOutOfRangeException();
            }
            if (type == JointType.FIXED) { }
            else if (type == JointType.HINGE)
            {
                if (limits[0] > Math.PI / 2) throw new ArgumentOutOfRangeException();
            }
            else if (type == JointType.PISTON) { }
            else if (type == JointType.ROTATIONAL)
            {
                if (limits[0] > Math.PI / 2) throw new ArgumentOutOfRangeException();
                if (limits[1] > Math.PI / 2) throw new ArgumentOutOfRangeException();
            }
            else
            {
                throw new ArgumentOutOfRangeException("type");
            }
            this.position = position;
            this.inclination = initInclination;
            this.angle = initAngle;
            this.type = type;
            this.limits = limits;
        }

        internal double[] getActorFactors()
        {
            return Enumerable.Repeat(1.0, this.type.dof).ToArray();
        }

        internal double[] getActorOffsets()
        {
            return Enumerable.Repeat(0.0, this.type.dof).ToArray();
        }

        internal double[] getSensorFactors()
        {
            return Enumerable.Repeat(1.0, this.type.dof).ToArray();
        }

        internal double[] getSensorOffsets()
        {
            return Enumerable.Repeat(0.0, this.type.dof).ToArray();
        }

        /// <summary>
        /// Create joint and set the axis and orentiation. Anchor should not be set as this is dependent on scaling.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public Joint createJoint(GameObject parent)
        {
            Joint joint = null;

            //define the joint positioning and rotational direction
            if (type.Equals(JointType.FIXED))
            {
                joint = (Joint)parent.AddComponent<FixedJoint>();
            }
            else if (type.Equals(JointType.HINGE))
            {
                HingeJoint h = parent.AddComponent<HingeJoint>(); joint = h;
                Vector3 axis = this.getUnityRProjected(this.angle - (float)(Math.PI / 2)); //positive angle is in the direction of the normal
                h.axis = axis;
            }
            else if (type.Equals(JointType.PISTON))
            {
                joint = (Joint)parent.AddComponent<SpringJoint>();
            }
            else if (type.Equals(JointType.ROTATIONAL))
            {
                joint = (Joint)parent.AddComponent<HingeJoint>();
            }
            else throw new NotImplementedException();

            return joint;
        }

        /// <summary>
        /// Return the positional vector of the point on the face where the next shape is attached
        /// </summary>
        /// <param name="parentShape"></param>
        /// <returns></returns>
        public Vector3 getUnityFaceAnchorPosition(ShapeSpecification parentShape)
        {
            float x, y, z;
            switch (position.face)
            {
                case 1: // Same Direction
                    x = this.position.faceX;
                    y = this.position.faceY;
                    z = 1;
                    break;
                case 2: // Right
                    x = 1;
                    y = this.position.faceX;
                    z = this.position.faceY;
                    break;
                case 3: // Away
                    x = -this.position.faceX;
                    y = 1;
                    z = this.position.faceY;
                    break;
                case 4: // Left
                    x = -1;
                    y = this.position.faceY;
                    z = -this.position.faceX;
                    break;
                case 5: // Towards
                    x = this.position.faceX;
                    y = -1;
                    z = this.position.faceY;
                    break;
                case 6: // Backwards
                default:
                    x = this.position.faceX;
                    y = -this.position.faceY;
                    z = -1;
                    throw new NotImplementedException(); //for now no joints that go backwards
            }
            return new Vector3(x * parentShape.getXBound(), y * parentShape.getYBound(), z * parentShape.getZBound());
        }

        /// <summary>
        /// Get the direction that points towards the second shape
        /// </summary>
        /// <returns>Unitvector</returns>
        public Vector3 getUnityDirection()
        {
            Vector3 r = Vector3.RotateTowards(this.getUnityNormalVector(), this.getUnityRProjected(this.angle), this.inclination, 0);
            return r;
        }



        /// <summary>
        /// Unitvector perpendicular to the face
        /// </summary>
        /// <returns></returns>
        Vector3 getUnityNormalVector()
        {
            switch (position.face)
            {
                case 1: // Same Direction
                default:
                    return new Vector3(0, 0, 1);
                case 2: // Right
                    return new Vector3(1, 0, 0);
                case 3: // Away
                    return new Vector3(0, 1, 0);
                case 4: // Left
                    return new Vector3(-1, 0, 0);
                case 5: // Towards
                    return new Vector3(0, -1, 0);
                case 6: // Backwards
                    return new Vector3(0, 0, -1);
            }
        }
        
        /// <summary>
        /// Unitvector in the upward direction
        /// </summary>
        /// <returns></returns>
        Vector3 getUnityUpVector()
        {
            switch (position.face)
            {
                case 1: // Same Direction   
                default:
                    return new Vector3(0, 1, 0);
                case 2: // Right
                case 3: // Away
                case 4: // Left
                case 5: // Towards
                    return new Vector3(0, 0, 1);
                case 6: // Backwards
                    return new Vector3(0, -1, 0);
            }
        }


        /// <summary>
        /// Unitvector with the direction angle relative to up direction. The r vector projected on the face
        /// </summary>
        /// <returns></returns>
        Vector3 getUnityRProjected(float angle)
        {
            Vector3 angle0Direction;
            switch (position.face)
            {
                case 1: // Same Direction
                case 6: // Backwards
                default:
                    angle0Direction = new Vector3(1, 0, 0);
                    break;
                case 2: // Right
                    angle0Direction = new Vector3(0, 1, 0);
                    break;
                case 3: // Away
                    angle0Direction = new Vector3(-1, 0, 0);
                    break;
                case 4: // Left
                    angle0Direction = new Vector3(0, -1, 0);
                    break;
                case 5: // Towards
                    angle0Direction = new Vector3(1, 0, 0);
                    break;
            }
            return (float)Math.Cos(angle) * this.getUnityUpVector() + (float)Math.Sin(angle) * angle0Direction;
        }

        public Quaternion getUnityRotation()
        {
            Quaternion r;
            switch (position.face)
            {
                case 1: // Same Direction
                default:
                    r = Quaternion.identity;
                    break;
                case 2: // Right
                    r = Quaternion.Euler(0, 0, -90);
                    break;
                case 3: // Away
                    r = Quaternion.Euler(0, -90, -90);
                    break;
                case 4: // Left
                    r = Quaternion.Euler(0, -180, -90);
                    break;
                case 5: // Towards
                    r = Quaternion.Euler(0, -270, -90);
                    break;
                case 6: // Backwards
                    r = Quaternion.Euler(0, 180, 180);
                    break;
            }
            return r;
        }
    }

    public class JointType
    {
        /// <summary>
        /// A fixed joint, it cannot move.
        /// </summary>
        static public readonly JointType FIXED = new JointType(0);
        /// <summary>
        /// A hinge joint, like in a door. The turning direction is defined via the JointSpecification.initAngle
        /// </summary>
        static public readonly JointType HINGE = new JointType(1);
        /// <summary>
        /// A piston joint, can only contract and extend in the initial direction.
        /// </summary>
        static public readonly JointType PISTON = new JointType(1);
        /// <summary>
        /// A free joint with 2 degrees of freedom. This joint cannot rotate the attached shape over the directional axis but can freely move in 2 degrees of freedom.
        /// The first degree of freedom is in the JointSpecification.initInclination direction, towards the normal of the JointPosition.face.
        /// The second degree of freedom is in the JointSpecification.initAngle direction, perpendicular to the radial vector.
        /// This is not a saddle joint as the saddle joint does not move idependantly as the inclination angle will change when the other degree of freedom is increased. Here both degrees of freedom are independantly.
        /// It is more like a ball and socket joint without rotation, thus a special king of Condyloid in spherical coordinates.
        /// </summary>
        static public readonly JointType ROTATIONAL = new JointType(2);
        internal int dof;
        internal JointType(int degreesOfFreedom) { this.dof = degreesOfFreedom; }

        internal int getDegreesOfFreedom()
        {
            return this.dof;
        }

        public IList<SensorSpec> createSensors()
        {
            List<SensorSpec> r = new List<SensorSpec>(this.dof);
            for(int i = 0; i < this.dof; i++)
            {
                r.Add(new SensorSpec());
            }
            return r;
        }

        public IList<ActorSpec> createActors()
        {
            List<ActorSpec> r = new List<ActorSpec>(this.dof);
            for (int i = 0; i < this.dof; i++)
            {
                r.Add(new ActorSpec());
            }
            return r;
        }
    }

    /// <summary>
    /// Specifies the position of a joint and the attached body relative to the original body (base shape).
    /// </summary>
    public class JointPosition
    {
        public int face;
        public float faceX;
        public float faceY;
        public float hover;
        public float rotation;
        public static float minimalAbsHover = -1;
        public static float maximalAbshover = 20;
        /// <summary>
        /// Specifies the position of a joint and attached body relative to the base shape.
        /// This only looks at the source shape as a bounded cubus, abstracting from the actual shape itself.
        /// It therefor always has 5 faces. The 1 first face is on the up direction of the base shape. The second to wards the X direction and so on.
        /// Downwards directions are not allowed for simplicity, in this way the risk of self intersection is reduced.
        /// This also defines an extra rotation the shape has over its intial axis, this rotation is independent of the joint's rotation.
        /// The horizontal and vertical offset are relative offsets with respect to the bounding box. With the origin at the center of this box and a lineari reltive domain of (-1, 1)
        /// At the start these positions will be located at the edges of the bounding boxes (I think this is more easily then letting them be relative to the surface of the base shape)
        /// </summary>
        /// <param name="offsetHorizontal">The horizontal position of the joint in the right, otherwise forward direction with respect to the base shape(-1, 1)</param>
        /// <param name="offsetVertical">The vertical position of the joint in the up direction, otherwise forward direction with respect to the base shape (-1, 1)</param>
        /// <param name="face">1: Same direction, 2: Right, 3: Downwards, 4: Left, 5: Upwards</param>
        /// <param name="absHover">Additional offset in the direction of the initial joint (For simplicity this can also be in the direction of the face) (minimalAbsHover, maximalAbshover)</param>
        /// <param name="shapeRotation">Additional independent rotation of the attached shape (-Pi, Pi) exclusing -Pi</param>
        public JointPosition(float offsetHorizontal, float offsetVertical, int face, float absHover, float shapeRotation)
        {
            if (face < 1 || face > 6)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (offsetHorizontal < -1 || offsetHorizontal > 1 || offsetVertical < -1 || offsetVertical > 1)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (absHover < minimalAbsHover || absHover > maximalAbshover)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (shapeRotation <= -Math.PI || shapeRotation > Math.PI)
            {
                throw new ArgumentOutOfRangeException();
            }

            this.faceX = offsetHorizontal;
            this.faceY = offsetVertical;
            this.face = face;
            this.hover = absHover;
            this.rotation = shapeRotation;
        }
    }
}
