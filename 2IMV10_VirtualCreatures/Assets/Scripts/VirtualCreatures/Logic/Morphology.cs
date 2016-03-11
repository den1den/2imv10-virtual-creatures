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
        public static String IDCount = "A";
        public NNSpecification brain;
        public IList<EdgeMorph> edges;
        public IList<Node> nodes;
        public Node root;
        public Genotype genotype;
        public String ID;

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

            // Create an ID
            ID = Morphology.IDCount;
            Morphology.IDCount += 1;

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
        
        static public Morphology testSwastika()
        {
            Genotype genotype = null;
            ShapeSpecification body = new Sphere(6);
            Node root = new Node(body);

            //right
            JointSpecification joint10 = new JointSpecification(Face.RIGHT, 0, 0, (float)Math.PI / 2, 0, 0.5f, JointType.HINDGE);
            JointSpecification joint20 = new JointSpecification(Face.FORWARDS, 0, 0, (float)Math.PI / 2, 0, 0.5f, JointType.HINDGE);
            JointSpecification joint30 = new JointSpecification(Face.LEFT, 0, 0, (float)Math.PI / 2, 0, 0.5f, JointType.HINDGE);
            JointSpecification joint40 = new JointSpecification(Face.DOWN, 0, 0, (float)Math.PI / 2, 0, 0.5f, JointType.HINDGE);
            //futher arm
            JointSpecification jointx1 = new JointSpecification(Face.UP, 0, 0, 0, 0, 0.5f, JointType.HINDGE);

            //arm element
            ShapeSpecification fin = Rectangle.createWidthDepthHeight(4, 0.2f, 9);

            //the nodes of the arms
            Node a10 = new Node(fin);
            Node a11 = new Node(fin);
            Node a20 = new Node(fin);
            Node a21 = new Node(fin);
            Node a30 = new Node(fin);
            Node a31 = new Node(fin);
            Node a40 = new Node(fin);
            Node a41 = new Node(fin);

            //arms
            IList<EdgeMorph> edges = new EdgeMorph[]{
                new EdgeMorph(root, a10, joint10, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(a10, a11, jointx1, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(root, a20, joint20, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(a20, a21, jointx1, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(root, a30, joint30, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(a30, a31, jointx1, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(root, a40, joint40, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(a40, a41, jointx1, NNSpecification.createEmptyNetwork()),
            }.ToList();

            return new Morphology(root, NNSpecification.createEmptyNetwork(), edges, genotype);
        }

        static public Morphology testSuperSwastika()
        {
            Genotype genotype = null;
            ShapeSpecification body = new Sphere(4);
            Node root = new Node(body);

            float hover = 0.5f;

            //right
            JointSpecification f = new JointSpecification(Face.FORWARDS, 0, 0, 0, 0, hover, JointType.FIXED);
            JointSpecification rig = new JointSpecification(Face.RIGHT, 0, 0, 0, 0, hover, JointType.FIXED);
            JointSpecification left = new JointSpecification(Face.LEFT, 0, 0, 0, 0, hover, JointType.FIXED);

            JointSpecification up = new JointSpecification(Face.UP, 0, 0, 0, 0, hover, JointType.FIXED);
            JointSpecification down = new JointSpecification(Face.DOWN, 0, 0, 0, 0, hover, JointType.FIXED);

            JointSpecification rev = new JointSpecification(Face.REVERSE, 0, 0, 0, 0, hover, JointType.FIXED);

            //arm element
            ShapeSpecification fin = Rectangle.createWidthDepthHeight(1, 0.2f, 4);

            //the nodes of the arms
            Node f1 = new Node(fin);
            Node f2 = new Node(fin);
            Node rig1 = new Node(fin);
            Node rig2 = new Node(fin);
            Node left1 = new Node(fin);
            Node left2 = new Node(fin);
            Node up1 = new Node(fin);
            Node up2 = new Node(fin);
            Node down1 = new Node(fin);
            Node doen2 = new Node(fin);
            Node rev1 = new Node(fin);
            Node rev2 = new Node(fin);

            //arms
            IList<EdgeMorph> edges = new EdgeMorph[]{
                new EdgeMorph(root, f1, f, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(f1, f2, new JointSpecification(Face.DOWN, 0, 0, 0, 0, hover, JointType.FIXED), NNSpecification.createEmptyNetwork()),
                new EdgeMorph(root, rig1, rig, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(rig1, rig2, new JointSpecification(Face.UP, 0, 0, 0, 0, hover, JointType.FIXED), NNSpecification.createEmptyNetwork()),
                new EdgeMorph(root, left1, left, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(left1, left2, new JointSpecification(Face.DOWN, 0, 0, 0, 0, hover, JointType.FIXED), NNSpecification.createEmptyNetwork()),
                new EdgeMorph(root, up1, up, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(up1, up2, new JointSpecification(Face.LEFT, 0, 0, 0, 0, hover, JointType.FIXED), NNSpecification.createEmptyNetwork()),
                new EdgeMorph(root, down1, down, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(down1, doen2, new JointSpecification(Face.RIGHT, 0, 0, 0, 0, hover, JointType.FIXED), NNSpecification.createEmptyNetwork()),
                new EdgeMorph(root, rev1, rev, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(rev1, rev2, new JointSpecification(Face.UP, 0, 0, 0, 0, hover, JointType.FIXED), NNSpecification.createEmptyNetwork()),
            }.ToList();

            return new Morphology(root, NNSpecification.createEmptyNetwork(), edges, genotype);
        }

        static public Morphology testSnake()
        {
            Genotype genotype = null;
            ShapeSpecification rootBody = new Sphere(6);
            Node root = new Node(rootBody);

            //forwards, all positioned up
            JointSpecification forwards = new JointSpecification(Face.UP, 0, 0, 0, 0, 2.5f, JointType.FIXED);
            JointSpecification different = new JointSpecification(Face.RIGHT, 0, 0, 0, 0, 2.5f, JointType.FIXED);

            //body element
            ShapeSpecification body = Rectangle.createPlane(6, 0.5f);

            //the nodes of the arms
            Node a0 = new Node(body);
            Node a1 = new Node(body);
            Node a2 = new Node(body);
            Node a3 = new Node(body);
            Node a4 = new Node(body);
            Node a5 = new Node(body);
            Node a6 = new Node(body);
            Node a7 = new Node(body);
            Node a8 = new Node(body);

            //arms
            IList<EdgeMorph> edges = new EdgeMorph[]{
                new EdgeMorph(root, a0, forwards, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(a0, a1, forwards, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(a1, a2, forwards, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(a2, a3, forwards, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(a3, a4, forwards, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(a4, a5, different, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(a5, a6, forwards, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(a6, a7, forwards, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(a7, a8, forwards, NNSpecification.createEmptyNetwork()),
            }.ToList();

            return new Morphology(root, NNSpecification.createEmptyNetwork(), edges, genotype);
        }


        internal IList<EdgeMorph> getEdges()
        {
            return new List<EdgeMorph>(this.edges);
        }
    }


}
