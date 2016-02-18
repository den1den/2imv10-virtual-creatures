using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Logic.VirtualCreatures
{
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

    public class MultStrategy
    {
        /// <summary>
        /// Multiplication not yet used
        /// </summary>
        public MultStrategy() { }
    }

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

