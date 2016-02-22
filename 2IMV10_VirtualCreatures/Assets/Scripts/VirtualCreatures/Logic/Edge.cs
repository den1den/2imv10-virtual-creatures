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
    }
    /// <summary>
    /// An edge from the Genotype Graph
    /// </summary>
    public class EdgeGen
    {
        public Node destination;
        public JointSpecification joint;
        public NNSpecification network;
        public Node source;
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
        public Vector3 axis;
        public int number;

        public Symmetry(Vector3 axis, int number)
        {
            this.axis = axis;
            this.number = number;
        }
    }

    /// <summary>
    /// Paramters of the Evolutionairy Algorithm on how multiple neural networks are connected when the NNSpecification of a genotype is multiple times in a Morhology.
    /// </summary>
    public class MultStrategy
    {
        /// <summary>
        /// Multiplication not yet used
        /// Should contain the neural network mapping strategy
        /// </summary>
        public MultStrategy() { }
    }
}

