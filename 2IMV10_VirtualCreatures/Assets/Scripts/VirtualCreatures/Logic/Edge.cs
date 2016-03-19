using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VirtualCreatures
{
    /// <summary>C:\Users\s157641\Desktop\Visual Computing\Repo\2IMV10_VirtualCreatures\Assets\Scripts\VirtualCreatures\Logic\ExplicitNN.cs
    /// An edge from the Morhology Graph
    /// </summary>
    public class EdgeMorph
    {
        public Node destination;
        public JointSpecification joint;
        public NNSpecification network;
        public Node source;

        public EdgeMorph(Node source, Node destination, JointSpecification joint, NNSpecification network)
        {
            this.source = source;
            this.destination = destination;
            this.joint = joint;
            this.network = network;
        }

        /// <summary>
        /// Return the positional vector of the point on the face where the next shape is attached
        /// </summary>
        /// <param name="parentShape"></param>
        /// <returns></returns>
        public Vector3 getUnityPositionAnchor()
        {
            Vector3 posAnchorUnscaled = (float)joint.faceHorizontal * joint.getRightUnitVector() + (float)joint.faceVertical * joint.getUpUnitVector();
            Vector3 absAnchor = Vector3.Scale(posAnchorUnscaled, source.shape.getBounds());
            return absAnchor;
        }
    }
    /// <summary>
    /// An edge from the Genotype Graph
    /// </summary>
    public class EdgeGen
    {
        public Node source;
        public Node destination;
        public JointSpecification joint;
        public NNSpecification network;
        public MultStrategy strategy;
        public Symmetry symmetry;

        public EdgeGen(Node source, Node destination, Symmetry symmetry, JointSpecification joint, NNSpecification network, MultStrategy strategy)
        {
            this.source = source;
            this.destination = destination;
            this.symmetry = symmetry;
            this.joint = joint;
            this.network = network;
            this.strategy = strategy;
        }
    }
    /// <summary>
    /// An notation of symmetry constraints on some EdgeGen, includes the cardinatlity/multiplicity of an EdgeGen. Is not included in the Morhology.
    /// </summary>
    public class Symmetry
    {
        public static int MAX_SYMMETRY = 4;

        Face _axis = 0;
        public Face axis { get { return _axis; } set { if (value == Face.FORWARDS || value == Face.RIGHT || value == Face.UP) _axis = value; else throw new ArgumentOutOfRangeException(); } }

        int _number;
        public int number { get { return _number; } set { if (value >= 1 || value <= MAX_SYMMETRY) _number = value; else throw new ArgumentOutOfRangeException(); } }

        public Symmetry(Face axis, int number)
        {
            this.axis = axis;
            this.number = number;
        }
    }

    /// <summary>
    /// Paramters of the Evolutionairy Algorithm on how multiple neural networks are connected when the NNSpecification of a genotype is multiple times in a Morhology.
    /// </summary>
    public enum MultStrategy
    {
        COPY_ACTORS_SENSORS,
        COPY_NETWORK
    }
}

