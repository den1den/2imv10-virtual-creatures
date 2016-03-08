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
    public class JointSpecification : ICloneable
    {
        public Face face = Face.UP;

        private float _faceHorizontal = 0;
        public float faceHorizontal { get { return _faceHorizontal; } set { if (value < -1 || value > 1) throw new ArgumentOutOfRangeException(); _faceHorizontal=value; } }
        private float _faceVertical = 0;
        public float faceVertical { get { return _faceVertical; } set { if (value < -1 || value > 1) throw new ArgumentOutOfRangeException(); _faceVertical = value; } }

        private float _rotation = 0;
        public float rotation { get { return _rotation; } set { if (value <= -Math.PI/2 || value > Math.PI/2) throw new ArgumentOutOfRangeException(); _rotation = value; } }

        private float _bending = 0;
        public float bending { get { return _bending; } set { if (value <= -Math.PI / 2 || value >= Math.PI / 2) throw new ArgumentOutOfRangeException(); _bending = value; } }

        private float _hover;
        public float hover { get { return _hover; } set { if (value <= 0) throw new ArgumentOutOfRangeException(); _hover = value; } }

        public JointType jointType = JointType.FIXED;

        /// <summary>
        /// Create a default fixed joint
        /// </summary>
        /// <param name="hover">distance from parent</param>
        public JointSpecification(float hover) { this.hover = hover; }

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
        public JointSpecification(Face face, float faceH, float faceV, float rot, float bending, float hover, JointType type)
        {
            this.face = face;
            this.faceHorizontal = faceH;
            this.faceVertical = faceV;
            this.rotation = rotation;
            this.bending = bending;
            this.hover = hover;
            this.jointType = type;
        }

        /// <summary>
        /// Create joint and set the axis and orentiation. Anchor should not be set as this is dependent on scaling.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public Joint createJoint(GameObject parent)
        {
            switch (this.jointType)
            {
                case JointType.FIXED:
                    FixedJoint fixedjoint = parent.AddComponent<FixedJoint>();
                    return fixedjoint;
                case JointType.HINDGE:
                    //positive angle is in the direction of the normal
                    HingeJoint hindgeJoint = parent.AddComponent<HingeJoint>();
                    hindgeJoint.axis = getUnityFirstRotationAxis();
                    return hindgeJoint;
                case JointType.PISTON:
                    SpringJoint springJoint = parent.AddComponent<SpringJoint>();
                    return springJoint;
                case JointType.ROTATIONAL:
                    break;
            }
            throw new NotImplementedException();
        }

        public Vector3 getUnityFirstRotationAxis()
        {
            return new Vector3((float)(-Math.Sin(this._rotation)), 0, (float)(Math.Cos(this._rotation)));
        }

        /// <summary>
        /// Get the direction that points towards the second shape
        /// </summary>
        /// <returns>Unitvector</returns>
        public Vector3 getUnityDirection()
        {
            //first get the directional vector of the topFace and then translate the rotation
            Vector3 normalTopFace = Vector3.up;
            Vector3 rProjectedTopFace = this.getUnityFirstRotationAxis();
            Vector3 rTopFace = Vector3.RotateTowards(normalTopFace, rProjectedTopFace, this._bending, 0);
            //then translate the rotation
            return this.getUnityRotation() * rTopFace;
        }

        public Quaternion getUnityRotation()
        {
            switch (this.face)
            {
                case Face.UP: // Same Direction
                    return Quaternion.identity;
                case Face.RIGHT: // Right
                    return Quaternion.Euler(90, 90, 0);
                case Face.FORWARDS: // Away
                    return Quaternion.Euler(90, 0, 0);
                case Face.LEFT: // Left
                    return Quaternion.Euler(90, 270, 0);
                case Face.BACKWARDS: // Towards
                    return Quaternion.Euler(90, 180, 0);
                case Face.DOWN: // Backwards
                    return Quaternion.Euler(180, 0, 0);
            }
            throw new NotImplementedException();
        }

        public static JointSpecification createSimple(Face face, float absHover)
        {
            JointSpecification js = new JointSpecification(absHover);
            js.face = face;
            return js;
        }

        public object Clone()
        {
            JointSpecification clone = new JointSpecification(this.face, this._faceHorizontal, this._faceVertical, this._rotation, this._bending, this._hover, this.jointType);
            return clone;
        }
        
        internal int getDegreesOfFreedom()
        {
            return getDegreesOfFreedom(this.jointType);
        }

        public static int getDegreesOfFreedom(JointType type)
        {
            switch (type)
            {
                case JointType.FIXED:
                    return 0;
                case JointType.HINDGE:
                case JointType.PISTON:
                    return 1;
                case JointType.ROTATIONAL:
                    return 2;
                default: throw new NotImplementedException();
            }
        }
    }

    public enum Face { UP, RIGHT, FORWARDS, LEFT, BACKWARDS, DOWN };

    public enum JointType
    {
        /// <summary>
        /// A fixed joint, it cannot move.
        /// DOF = 0
        /// </summary>
        FIXED,
        /// <summary>
        /// A hinge joint, like in a door. The axis is defined by the rotation
        /// DOF = 1
        /// </summary>
        HINDGE,
        /// <summary>
        /// A piston joint, can only contract and extend in the initial direction.
        /// DOF = 1
        /// </summary>
        PISTON,
        /// <summary>
        /// A free joint with 2 degrees of freedom. This joint cannot rotate the attached shape over the directional axis but can freely move in 2 degrees of freedom.
        /// The first degree of freedom is in the JointSpecification.initInclination direction, towards the normal of the JointPosition.face.
        /// The second degree of freedom is in the JointSpecification.initAngle direction, perpendicular to the radial vector.
        /// This is not a saddle joint as the saddle joint does not move idependantly as the inclination angle will change when the other degree of freedom is increased. Here both degrees of freedom are independantly.
        /// It is more like a ball and socket joint without rotation, thus a special king of Condyloid in spherical coordinates.
        /// DOF = 2
        /// </summary>
        ROTATIONAL
    };

}
