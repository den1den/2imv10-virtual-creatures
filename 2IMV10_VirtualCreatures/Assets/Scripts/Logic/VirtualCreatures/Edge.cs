using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Logic.VirtualCreatures
{
    /// <summary>
    /// An edge from the Morhology Graph
    /// </summary>
    public class EdgeMorph
    {
        public Node destination;
        public JointSpecification joint;
        public NNMapping mapping;
        public NNSpecification network;
        public Node source;

        public EdgeMorph(Node source, Node destination, JointSpecification joint, NNSpecification network, NNMapping mapping)
        {
            this.source = source;
            this.destination = destination;
            this.joint = joint;
            this.network = network;
            this.mapping = mapping;
        }
    }
    /// <summary>
    /// An edge from the Genotype Graph
    /// </summary>
    public class EdgeGen
    {
        public Node destination;
        public JointSpecification joint;
        public NNMappingComplete mapping;
        public NNSpecification network;
        public Node source;
        public MultStrategy strategy;
        public Symmetry symmetry;

        public EdgeGen(Node source, Node destination, Symmetry symmetry, JointSpecification joint, NNSpecification network, NNMappingComplete mapping, MultStrategy strategy)
        {
            this.source = source;
            this.destination = destination;
            this.symmetry = symmetry;
            this.joint = joint;
            this.network = network;
            this.mapping = mapping;
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
        /// </summary>
        public MultStrategy() { }
    }

    /// <summary>
    /// Defines how a NNSpecification is connected to a Joint.
    /// </summary>
    public class NNMapping
    {
        public IList<OutConnection> jointActorConnections;
        public IList<InConnection> jointSensorConnections;
        public NNMapping(IList<OutConnection> jointActorConnections, IList<InConnection> jointSensorConnections)
        {
            this.jointActorConnections = jointActorConnections;
            this.jointSensorConnections = jointSensorConnections;
        }
        public NNMapping(IList<InConnection> jointSensorConnections) : this(new List<OutConnection>(), jointSensorConnections) { }
        public NNMapping(IList<OutConnection> jointActorConnections) : this(jointActorConnections, new List<InConnection>()) { }
    }
    /// <summary>
    /// Defines how a NNSpecification is connected to a Joint and to other NNSpecifications
    /// </summary>
    public class NNMappingComplete : NNMapping
    {
        private IDictionary<InConnection, IList<OutConnection>> incomming;
        private IDictionary<OutConnection, IList<InConnection>> outgoing;

        public NNMappingComplete(IList<OutConnection> jointActorConnections, IList<InConnection> jointSensorConnections, IDictionary<OutConnection, IList<InConnection>> outgoing, IDictionary<InConnection, IList<OutConnection>> incomming) : base(jointActorConnections, jointSensorConnections)
        {
            this.outgoing = outgoing;
            this.incomming = incomming;
        }
    }
}

