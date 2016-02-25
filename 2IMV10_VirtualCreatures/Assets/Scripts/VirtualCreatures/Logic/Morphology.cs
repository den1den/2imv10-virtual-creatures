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
            if (!edges.Select(e => e.network).Contains(brain))
            {
                throw new ArgumentException(); //brain can only be added once
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
            Node root = new Node(new Sphere(1f/4));

            //right
            Node fin = new Node(new PlaneRectangle(2, 0.5f));

            float[] limits = new float[] { (float)(Math.PI / 2 * 0.8) };
            JointPosition right = new JointPosition(0, 0, 2, 0.1f, 0);
            JointSpecification rightJoint = new JointSpecification(right, 0, 0, JointType.HINGE, limits);

            NNSpecification rightWriteOnlyNNS = NNSpecification.createEmptyWriteNetwork(rightJoint.type, brain.networkOut);

            //left
            Node fin2 = new Node(new PlaneRectangle(2, 0.5f));

            JointPosition left = new JointPosition(0, 0, 4, 0.1f, 0);
            JointSpecification leftJoint = new JointSpecification(left, 0, 0, JointType.HINGE, limits);

            NNSpecification leftReadWriteNNS = NNSpecification.createEmptyWriteNetwork(leftJoint.type, brain.networkOut);

            IList<EdgeMorph> edges = new EdgeMorph[]{
                new EdgeMorph(root, fin, rightJoint, rightWriteOnlyNNS),
                new EdgeMorph(root, fin2, leftJoint, leftReadWriteNNS)
            }.ToList();

            return new Morphology(root, brain, edges, genotype);
        }

        internal IList<EdgeMorph> getEdges()
        {
            return new List<EdgeMorph>(this.edges);
        }
    }


}
