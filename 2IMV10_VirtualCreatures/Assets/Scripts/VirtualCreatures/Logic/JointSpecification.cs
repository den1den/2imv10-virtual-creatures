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

        private double _faceHorizontal = 0;
        public double faceHorizontal { get { return _faceHorizontal; } set { if (value < -1 || value > 1) throw new ArgumentOutOfRangeException(); _faceHorizontal=value; } }
        private double _faceVertical = 0;
        public double faceVertical { get { return _faceVertical; } set { if (value < -1 || value > 1) throw new ArgumentOutOfRangeException(); _faceVertical = value; } }

        private double _rotation = 0;
        public double rotation { get { return _rotation; } set { if (value <= -Math.PI/2 || value > Math.PI/2) throw new ArgumentOutOfRangeException(); _rotation = value; } }

        private double _bending = 0;
        public double bending { get { return _bending; } set { if (value <= -Math.PI / 2 || value >= Math.PI / 2) throw new ArgumentOutOfRangeException(); _bending = value; } }

        private double _hover;
        public double hover { get { return _hover; } set { if (value <= 0) throw new ArgumentOutOfRangeException(); _hover = value; } }

        public JointType jointType = JointType.FIXED;

        /// <summary>
        /// Create a default fixed joint
        /// </summary>
        /// <param name="hover">distance from parent</param>
        public JointSpecification(double hover) { this.hover = hover; }

        /// <summary>
        /// Specifies the position of a joint and attached body relative to the base shape.
        /// This only looks at the source shape as a bounded cubus, abstracting from the actual shape itself.
        /// It therefor always has 5 faces. The 1 first face is on the up direction of the base shape. The second to wards the X direction and so on.
        /// Downwards directions are not allowed for simplicity, in this way the risk of self intersection is reduced.
        /// This also defines an extra rotation the shape has over its intial axis, this rotation is independent of the joint's rotation.
        /// The horizontal and vertical offset are relative offsets with respect to the bounding box. With the origin at the center of this box and a lineari reltive domain of (-1, 1)
        /// At the start these positions will be located at the edges of the bounding boxes (I think this is more easily then letting them be relative to the surface of the base shape)
        /// </summary>
        /// <param name="face">See enum Face</param>
        /// <param name="faceH">The horizontal position of the joint in the right, otherwise forward direction with respect to the base shape(-1, 1)</param>
        /// <param name="faceV">The vertical position of the joint in the up direction, otherwise forward direction with respect to the base shape (-1, 1)</param>
        /// <param name="rot">(-Pi/2, Pi/2)</param>
        /// bending
        /// <param name="hover">Additional offset in the direction of the initial joint (For simplicity this can also be in the direction of the face) (minimalAbsHover, maximalAbshover)</param>
        /// 
        public JointSpecification(Face face, double faceH, double faceV, double rot, double bending, double hover, JointType type)
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
                    hindgeJoint.axis = this.getUnityAxisUnitVector();
                    return hindgeJoint;
                case JointType.PISTON:
                    SpringJoint springJoint = parent.AddComponent<SpringJoint>();
                    return springJoint;
                case JointType.ROTATIONAL:
                    break;
            }
            throw new NotImplementedException();
        }

        public Vector3 getNormalUnitVector()
        {
            switch (this.face)
            {
                case Face.RIGHT:
                    return Vector3.right;
                case Face.FORWARDS:
                    return Vector3.forward;
                case Face.LEFT:
                    return Vector3.left;
                case Face.UP:
                    return Vector3.up;
                case Face.DOWN:
                    return Vector3.down;
                case Face.REVERSE:
                    return Vector3.back;
            }
            throw new NotImplementedException();
        }

        public Vector3 getRightUnitVector()
        {
            switch (this.face)
            {
                case Face.RIGHT:
                    return Vector3.back;
                case Face.LEFT:
                    return Vector3.forward;
                case Face.FORWARDS:
                case Face.UP:
                case Face.DOWN:
                    return Vector3.right;
                case Face.REVERSE:
                    return Vector3.back;
            }
            throw new NotImplementedException();
        }

        public Vector3 getUpUnitVector()
        {
            switch (this.face)
            {
                case Face.RIGHT:
                case Face.LEFT:
                case Face.FORWARDS:
                case Face.REVERSE:
                    return Vector3.up;
                case Face.UP:
                    return Vector3.left;
                case Face.DOWN:
                    return Vector3.right;
            }
            throw new NotImplementedException();
        }

        public Vector3 getUnityAxisUnitVector()
        {
            return (float)Math.Cos(this.rotation) * this.getRightUnitVector() + (float)Math.Sin(this.rotation) * this.getUpUnitVector();
        }

        /// <summary>
        /// Get the direction that points towards the second shape
        /// </summary>
        /// <returns>Unitvector</returns>
        public Vector3 getUnityDirection()
        {
            Vector3 normal = this.getNormalUnitVector();
            Vector3 axis = this.getUnityAxisUnitVector();
            Vector3 direcionalVector = Vector3.RotateTowards(normal, axis, (float)_bending, 0);
            return direcionalVector;
        }

        public Quaternion getUnityRotation()
        {
            Quaternion baseRotation;
            switch (this.face)
            {
                case Face.RIGHT:
                    baseRotation = Quaternion.Euler(0, 90, 0);
                    break;
                case Face.FORWARDS:
                    baseRotation = Quaternion.identity;
                    break;
                case Face.LEFT:
                    baseRotation = Quaternion.Euler(0, -90, 0);
                    break;
                case Face.UP:
                    baseRotation = Quaternion.Euler(-90, 0, 0);
                    break;
                case Face.DOWN:
                    baseRotation = Quaternion.Euler(90, 0, 0);
                    break;
                case Face.REVERSE:
                    baseRotation = Quaternion.Euler(0, 180, 0);
                    break;
                default: throw new NotImplementedException();
            }

            float rotationDegrees = (float)(this.rotation / Math.PI * 180);
            Quaternion axialRotation = Quaternion.Euler(0, 0, rotationDegrees);

            float bendingDegrees = (float)(this.bending / Math.PI * 180);
            Quaternion bending = Quaternion.Euler(0, bendingDegrees, 0);

            Quaternion total = baseRotation * axialRotation * bending;
            return total;
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

    public enum Face
    {
        /// <summary>
        /// Continue in the same direction (towards Z axis)
        /// </summary>
        FORWARDS,
        RIGHT,
        LEFT,
        /// <summary>
        /// Reverse in the opposite direction, probably causes a collition (back in Z axis)
        /// </summary>
        REVERSE,
        UP,
        DOWN
    };

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
