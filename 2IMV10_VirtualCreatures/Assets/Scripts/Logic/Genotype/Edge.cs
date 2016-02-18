using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Logic.Genotype
{
    public class Edge
    {
        public Node destination;
        public JointSpecification joint;
        public NNMapping mapping;
        public NNSpecification network;
        public Node source;
        public MultStrategy strategy;
        public Symmetry symmetry;

        public Edge(Node source, Node destination, Symmetry symmetry, JointSpecification joint, NNSpecification network, NNMapping mapping, MultStrategy strategy)
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
        private IDictionary<InConnection, OutConnection> incomming;
        private IList<OutConnection> jointActorConnections;
        private IList<InConnection> jointSensorConnections;
        private IDictionary<OutConnection, InConnection> outgoing;

        public NNMapping(IList<OutConnection> jointActorConnections, IList<InConnection> jointSensorConnections, IDictionary<OutConnection, InConnection> outgoing, IDictionary<InConnection, OutConnection> incomming)
        {
            this.jointActorConnections = jointActorConnections;
            this.jointSensorConnections = jointSensorConnections;
            this.outgoing = outgoing;
            this.incomming = incomming;
        }
    }
}

