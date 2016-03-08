using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VirtualCreatures
{
    /// <summary>
    /// The instantiation of a Genotype
    /// </summary>
    public class Morphology
    {
        public NNSpecification brain;
        public IList<EdgeMorph> edges;
        public IList<Node> nodes;
        public Node root;
        public Genotype genotype;

        public Morphology(Node root, NNSpecification brain, IList<EdgeMorph> edges, Genotype genotype)
        {
            IList<Node> nodes = edges.SelectMany(e => new Node[] { e.source, e.destination }).Distinct().ToList();
            if (edges.Select(e => e.network).Where(net => net == brain).Count() > 0)
            {
                throw new ArgumentException(); //brain can only be added once, and that should not be in one of the edges
            }
            if(brain.actors.Count > 0 || brain.sensors.Count > 0) { throw new ArgumentException("Brain should not have sensors for now"); }
            if (edges.Where(a => edges.Where(b => a.source == b.source && a.destination == b.destination).Count() > 1).Count() > 0)
            {
                throw new ArgumentException(); //check for edges with same source en destination, this should not happen and nodes should be repeated
            }

            this.root = root;
            this.brain = brain;
            this.edges = edges;
            this.nodes = nodes;
        }

        /// <summary>
        /// A first morhology to test the evolution algorithm with.
        /// </summary>
        /// <returns>Morphology of a ball with two fins</returns>
        static public Morphology test1()
        {
            NNSpecification brain = NNSpecification.testBrain1();

            Genotype genotype = null;
            ShapeSpecification body = new Sphere(6);
            Node root = new Node(body);

            //right
            ShapeSpecification fin = Rectangle.createWidthDepthHeight(4, 0.2f, 9);
            Node rfin = new Node(fin);
            JointSpecification rightJoint = new JointSpecification(Face.RIGHT, 0, 0, 0, 0, 0.5f, JointType.HINDGE);
            NNSpecification rightWriteOnlyNNS = NNSpecification.createEmptyWriteNetwork(rightJoint.getDegreesOfFreedom());

            //left
            Node lfin = new Node(fin);
            JointSpecification leftJoint = new JointSpecification(Face.LEFT, 0, 0, 0, 0, 0.5f, JointType.HINDGE);
            NNSpecification leftWriteOnlyNNS = NNSpecification.createEmptyWriteNetwork(leftJoint.getDegreesOfFreedom());

            IList<EdgeMorph> edges = new EdgeMorph[]{
                new EdgeMorph(root, rfin, rightJoint, rightWriteOnlyNNS),
                new EdgeMorph(root, lfin, leftJoint, leftWriteOnlyNNS)
            }.ToList();

            //connect the brain to the other NNSpecifications
            NeuralSpec outgoing = brain.outgoing[0];
            rightWriteOnlyNNS.connectTo(outgoing, rightWriteOnlyNNS.sensors);
            leftWriteOnlyNNS.connectTo(outgoing, leftWriteOnlyNNS.sensors);

            return new Morphology(root, brain, edges, genotype);
        }

        internal IList<EdgeMorph> getEdges()
        {
            return new List<EdgeMorph>(this.edges);
        }
    }


}
