using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Logic.Genotype
{
    /// <summary>
    /// This defines a single joint between two shapes.
    /// It includes the relative initial position of the attached object and the degrees of freedom
    /// </summary>
    public class JointSpecification
    {
        public float angle;
        public float inclination;
        public JointPosition position;
        public JointType type;
        public float[] limits;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="initInclination"></param>
        /// <param name="initAngle"></param>
        /// <param name="type"></param>
        /// <param name="limits">Symmetrical limits on the axis </param>
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
            else if (type == JointType.HINDGE)
            {
                if (limits[0] > Math.PI / 2) throw new ArgumentOutOfRangeException();
            }
            else if (type == JointType.PISTON) { }
            else if (type == JointType.SADDLE)
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
    }

    public class JointType
    {
        static public readonly JointType FIXED = new JointType(0);
        static public readonly JointType HINDGE = new JointType(1);
        static public readonly JointType PISTON = new JointType(1);
        static public readonly JointType SADDLE = new JointType(2);
        private int dof;
        internal JointType(int degreesOfFreedom) { this.dof = degreesOfFreedom; }

        internal int getDegreesOfFreedom()
        {
            return this.dof;
        }
    }

    public class JointPosition
    {
        public int face;
        public float faceX;
        public float faceY;
        public float hover;
        public float rotation;
        public static float minimalAbsHover = -1;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="offsetHorizontal"></param>
        /// <param name="offsetVertical"></param>
        /// <param name="face">1: Same direction, 2: Right, 3: Downwards, 4: Left, 5: Upwards</param>
        /// <param name="absHover"></param>
        /// <param name="shapeRotation"></param>
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
            if (absHover < minimalAbsHover)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (shapeRotation < -1 || shapeRotation > 1)
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
