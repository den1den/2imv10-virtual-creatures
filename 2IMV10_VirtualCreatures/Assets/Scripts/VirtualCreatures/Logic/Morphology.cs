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
        public static Char IDCount = 'A';
        public NNSpecification brain;
        public IList<EdgeMorph> edges;
        public IList<Node> nodes;
        public Node root;
        public Genotype genotype;
        public Char ID;

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
            Morphology.IDCount = (Char)(Convert.ToUInt16(Morphology.IDCount) + 1);

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
            JointSpecification rightJoint = new JointSpecification(Face.RIGHT, 0, 0, 0, 0, 5.5f, JointType.HINDGE);
            NNSpecification rightWriteOnlyNNS = NNSpecification.createEmptyWriteNetwork(rightJoint.getDegreesOfFreedom());

            //left
            Node lfin = new Node(fin);
            JointSpecification leftJoint = new JointSpecification(Face.LEFT, 0, 0, 0, 0, 5.5f, JointType.HINDGE);
            NNSpecification leftWriteOnlyNNS = NNSpecification.createEmptyWriteNetwork(leftJoint.getDegreesOfFreedom());

            IList<EdgeMorph> edges = new EdgeMorph[]{
                new EdgeMorph(root, rfin, rightJoint, rightWriteOnlyNNS),
                new EdgeMorph(root, lfin, leftJoint, leftWriteOnlyNNS)
            }.ToList();

            //connect the brain to the other NNSpecifications
            NeuralSpec outgoing = brain.outgoing[0];
            rightWriteOnlyNNS.connectTo(outgoing, rightWriteOnlyNNS.actors);
            leftWriteOnlyNNS.connectTo(outgoing, leftWriteOnlyNNS.actors);

            return new Morphology(root, brain, edges, genotype);
        }

        /// <summary>
        /// A first morhology to test the evolution algorithm with.
        /// </summary>
        /// <returns>Morphology of a ball with two fins</returns>
        static public Morphology testHindge()
        {
            NNSpecification brain = NNSpecification.testBrain1();

            Genotype genotype = null;
            ShapeSpecification body = Rectangle.createCube(10);
            Node root = new Node(body);

            //right
            ShapeSpecification fin = Rectangle.createWidthDepthHeight(10, 0.2f, 50);
            Node rfin = new Node(fin);
            JointSpecification rightJoint = new JointSpecification(Face.RIGHT, 0, 0, 0, 0, 1, JointType.FIXED);
            NNSpecification rightWriteOnlyNNS = NNSpecification.createEmptyNetwork();

            //left
            Node lfin = new Node(fin);
            JointSpecification leftJoint = new JointSpecification(Face.LEFT, 0, 0, 0, 0, 1, JointType.FIXED);
            NNSpecification leftWriteOnlyNNS = NNSpecification.createEmptyNetwork();

            IList<EdgeMorph> edges = new EdgeMorph[]{
                new EdgeMorph(root, rfin, rightJoint, rightWriteOnlyNNS),
                new EdgeMorph(root, lfin, leftJoint, leftWriteOnlyNNS)
            }.ToList();

            //connect the brain to the other NNSpecifications
            //NeuralSpec outgoing = brain.outgoing[0];
            //rightWriteOnlyNNS.connectTo(outgoing, rightWriteOnlyNNS.actors);
            //leftWriteOnlyNNS.connectTo(outgoing, leftWriteOnlyNNS.actors);

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

        static public Morphology testRotation()
        {
            Genotype genotype = null;
            ShapeSpecification rootBody = Rectangle.createCube(4);
            Node root = new Node(rootBody);

            float angle = (float)(Math.PI / 4);
            float h = (float)Math.Sqrt(2);
            float k = h / 2;

            //body element
            ShapeSpecification smaller_body = Rectangle.createPlane(k*2, 0.1f);
            ShapeSpecification body = Rectangle.createPlane(h*2, 0.1f);
            ShapeSpecification bigger_body = Rectangle.createPlane((h + k)*2, 0.15f);

            smaller_body = Rectangle.createCube(k*2);
            body = Rectangle.createCube(h*2);
            bigger_body = Rectangle.createPilar((h+k)*2, 0.01f);

            //right
            JointSpecification rl = new JointSpecification(Face.RIGHT, 0, 0, 0, -angle, k, JointType.FIXED);
            JointSpecification r = new JointSpecification(Face.RIGHT, 0, 0, 0, 0, 2 - h, JointType.FIXED);
            JointSpecification rr = new JointSpecification(Face.RIGHT, 0, 0, 0, angle, h+k, JointType.FIXED);
            Node r0 = new Node(bigger_body);
            Node r1 = new Node(body);
            Node r2 = new Node(smaller_body);

            //recursion on the one facing forwards: r0
            JointSpecification fl = new JointSpecification(Face.FORWARDS, 0, 0, 0, -angle, 2.5f, JointType.FIXED);
            JointSpecification f = new JointSpecification(Face.FORWARDS, 0, 0, 0, 0, 2.5f, JointType.FIXED);
            JointSpecification fr = new JointSpecification(Face.FORWARDS, 0, 0, 0, angle, 2.5f, JointType.FIXED);
            Node r00 = new Node(bigger_body);
            Node r01 = new Node(body);
            Node r02 = new Node(smaller_body);

            //left
            JointSpecification ll = new JointSpecification(Face.LEFT, 0, 0, 0, -angle, k, JointType.FIXED);
            JointSpecification l = new JointSpecification(Face.LEFT, 0, 0, 0, 0, 2 - h, JointType.FIXED);
            JointSpecification lr = new JointSpecification(Face.LEFT, 0, 0, 0, angle, h + k, JointType.FIXED);
            Node l0 = new Node(bigger_body);
            Node l1 = new Node(body);
            Node l2 = new Node(smaller_body);

            //recursion on the one facing forwards: r0
            Node l00 = new Node(bigger_body);
            Node l01 = new Node(body);
            Node l02 = new Node(smaller_body);

            //arms
            IList<EdgeMorph> edges = new EdgeMorph[]{
                new EdgeMorph(root, r0, rl, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(root, r1, r, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(root, r2, rr, NNSpecification.createEmptyNetwork()),

                new EdgeMorph(r0, r00, fl, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(r0, r01, f, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(r0, r02, fr, NNSpecification.createEmptyNetwork()),

                new EdgeMorph(root, l0, ll, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(root, l1, l, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(root, l2, lr, NNSpecification.createEmptyNetwork()),

                new EdgeMorph(l2, l00, fl, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(l2, l01, f, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(l2, l02, fr, NNSpecification.createEmptyNetwork())
            }.ToList();

            //FIXME: +45 -45 != 0 in phenotype

            return new Morphology(root, NNSpecification.createEmptyNetwork(), edges, genotype);
        }

        static public Morphology testArc()
        {
            Genotype genotype = null;

            int N = 10;
            double angle = (Math.PI / 2) / N;
            float x = 2;
            
            ShapeSpecification body = Rectangle.createPilar(x/2, 0.1f);
            JointSpecification joint = new JointSpecification(Face.FORWARDS, 0, 0, 0, angle, x/2, JointType.FIXED);
            IList<EdgeMorph> edges = new List<EdgeMorph>();

            Node a = new Node(body);
            Node root = a;
            for (int i = 0; i < N; i++)
            {
                Node b = new Node(body);
                edges.Add(new EdgeMorph(a, b, joint, NNSpecification.createEmptyNetwork()));
                a = b;
            }

            return new Morphology(root, NNSpecification.createEmptyNetwork(), edges, genotype);
        }

        static public Morphology testCircle()
        {
            Genotype genotype = null;
            ShapeSpecification rootBody = new Sphere(6);
            Node root = new Node(rootBody);

            float angleRadium10th = (float)(2 * Math.PI / 8) - 0.1f;

            //forwards, all positioned up
            JointSpecification forwards = new JointSpecification( Face.FORWARDS, 0, 0, 0, angleRadium10th, 2.5f, JointType.FIXED);

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
                new EdgeMorph(a4, a5, forwards, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(a5, a6, forwards, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(a6, a7, forwards, NNSpecification.createEmptyNetwork()),
                new EdgeMorph(a7, a8, forwards, NNSpecification.createEmptyNetwork()),
            }.ToList();

            return new Morphology(root, NNSpecification.createEmptyNetwork(), edges, genotype);
        }

        static public Morphology testCreature3()
        {
            ShapeSpecification b0 = Rectangle.createCube(2);
            Node root = new Node(b0);

            ShapeSpecification b1 = Rectangle.createCube(0.5f);
            Node n1 = new Node(b1);

            ShapeSpecification b2 = Rectangle.createCube(3);
            Node n2 = new Node(b2);

            ShapeSpecification b3 = Rectangle.createCube(1);
            Node n3 = new Node(b3);

            float absHover = 1f;
            JointSpecification toTheRight = JointSpecification.createSimple(Face.RIGHT, absHover);
            JointSpecification forwards = JointSpecification.createSimple(Face.UP, absHover);
            NNSpecification emptyNN = NNSpecification.createEmptyNetwork();

            IList<EdgeMorph> edges = new EdgeMorph[]{
                new EdgeMorph(root, n1, toTheRight, emptyNN),
                new EdgeMorph(n1, n2, forwards, emptyNN),
                new EdgeMorph(n2, n3, forwards, emptyNN)
            }.ToList();

            Morphology m = new Morphology(root, NNSpecification.createEmptyNetwork(), edges, null);
            return m;
        }

        static public Morphology testCreature3_1()
        {
            ShapeSpecification b0 = Rectangle.createCube(2);
            Node root = new Node(b0);

            ShapeSpecification b1 = Rectangle.createCube(0.5f);
            Node n1 = new Node(b1);

            ShapeSpecification b2 = Rectangle.createCube(3);
            Node n2 = new Node(b2);

            ShapeSpecification b3 = Rectangle.createCube(1);
            Node n3 = new Node(b3);

            float absHover = 1f;
            JointSpecification toTheRight = JointSpecification.createSimple(Face.RIGHT, absHover);
            JointSpecification forwards = JointSpecification.createSimple(Face.UP, absHover);
            forwards.jointType = JointType.HINDGE;
            NNSpecification emptyNN = NNSpecification.createEmptyNetwork();

            IList<EdgeMorph> edges = new EdgeMorph[]{
                new EdgeMorph(root, n1, toTheRight, emptyNN),
                new EdgeMorph(n1, n2, forwards, emptyNN),
                new EdgeMorph(n2, n3, forwards, emptyNN)
            }.ToList();
            
            return new Morphology(root, NNSpecification.createEmptyNetwork(), edges, null);
        }


        internal IList<EdgeMorph> getEdges()
        {
            return new List<EdgeMorph>(this.edges);
        }
    }


}
