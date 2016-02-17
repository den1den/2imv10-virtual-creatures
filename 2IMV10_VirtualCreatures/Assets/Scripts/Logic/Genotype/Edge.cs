using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public Vector axis;
        public int number;

        public Symmetry(Vector axis, int number)
        {
            this.axis = axis;
            this.number = number;
        }
    }

    public class MultStrategy
    {
        //TODO
    }

    public class NNMapping
    {
        //TODO
    }
}
