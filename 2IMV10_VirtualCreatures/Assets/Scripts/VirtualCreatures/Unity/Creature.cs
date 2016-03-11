using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualCreatures {
    /// <summary>
    /// GameObject
    /// </summary>
    public class Creature : MonoBehaviour {

        private Phenotype phenotype;

        public IList<Joint> joints = new List<Joint>();
        private IList<GameObject> primitives = new List<GameObject>();

        // Use this for initialization
        void Start()
        {
            // Initialize phenotype when the objects are created in the scenes
            //phenotype.initialize();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // Update phenotype in each physics engine step
            //phenotype.update(Time.fixedDeltaTime);
            if (phenotype != null)
            {
                phenotype.update(Time.fixedDeltaTime);
            }
        }

        /// <summary>
        /// Return all the joints of this creature.
        /// </summary>
        /// <returns></returns>
        public IList<Joint> getJoints()
        {
            return joints;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="joint"></param>
        public void AddJoint(Joint joint)
        {
            joints.Add(joint);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="primitive"></param>
        public void AddPrimitive(GameObject primitive)
        {
            primitives.Add(primitive);
        }

        public void setPhenotype(Phenotype phenotype)
        {
            this.phenotype = phenotype;
        }

        /// <summary>
        /// See draw.io drawing.
        /// </summary>
        /// <returns></returns>
        public static Creature CreateTest3()
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

            return Create(m);
        }

        /// <summary>
        /// 
        /// *note to set the position of the creature you have to set Creature.transform.position
        /// </summary>
        /// <param name="morphology"></param>
        /// <returns>Creature</returns>
        public static Creature Create(Morphology morphology)
        {
            // Instantiate empty creature prefab to scene
            GameObject creatureContainer = Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Creature.prefab"));

            // Create new joints list 
            Joint[] joints = new Joint[morphology.edges.Count];

            // Recursivaly create and connect all components
            // Start with the root, at zero
            GameObject creatureRootNode = morphology.root.shape.createUnscaledPrimitive();
            creatureRootNode.transform.parent = creatureContainer.transform;
            creatureRootNode.transform.localPosition = new Vector3(0, 10, 0);

            // then recursivly traverse all connected edges
            recursiveCreateJointsFromMorphology(morphology, morphology.root, creatureRootNode, joints);

            // Get the container creature script to store the joints 
            Creature creatureScript = (Creature)creatureContainer.GetComponent<Creature>();
            creatureScript.joints = joints.ToList<Joint>();

            Phenotype phenotype = new Phenotype(morphology, creatureScript.getJoints().ToArray<Joint>());

            // Creature Phenotype from morphology
            creatureScript.setPhenotype(phenotype);

            return creatureScript;
        }

        /// <summary>
        /// Same as Test3 but with more complex Joints
        /// </summary>
        /// <returns></returns>
        public static Creature CreateTest3_1()
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

            Morphology m = new Morphology(root, NNSpecification.createEmptyNetwork(), edges, null);

            return Create(m);
        }

        /// <summary>
        /// Adds all the cildren recursivly to the parent node
        /// </summary>
        /// <param name="morphology">The full morhology (to traverse the edges)</param>
        /// <param name="parentNode">The node ofwhich the children should be added</param>
        /// <param name="parentGO">The created gameobject of the parent</param>
        /// <param name="allJoints">A list of all the joints found thusfar (in the order of traversal)</param>
        private static void recursiveCreateJointsFromMorphology(Morphology morphology, Node parentNode, GameObject parentGO, IList<Joint> allJoints)
        {
            // Get the connected edges of the current node
            IList<EdgeMorph> edges = parentNode.getEdges(morphology.edges);
            // Iterate over each edge that we have for the current node
            foreach (EdgeMorph e in edges)
            {
                // Create a primitive for the next node below the current node
                Node childNode = e.destination;
                GameObject childGO = childNode.shape.createUnscaledPrimitive();

                childGO.transform.parent = parentGO.transform; // silently applies correction factor for hiearchical scaling
                
                // Create the joint at the parent and set the direction of the joint
                Joint joint = e.joint.createJoint(childGO);
                allJoints[morphology.edges.IndexOf(e)] = joint;

                // Calculate where the distance between center of child and parent
                Vector3 facePosition = e.getUnityPositionAnchor(); //on parent shape
                Vector3 direction = e.joint.getUnityDirection(); //towards child
                float distFaceToChildCenter = e.joint.hover + childNode.shape.getBound(Face.REVERSE); //on child shape
                Vector3 absPosition = facePosition + distFaceToChildCenter * direction;

                Quaternion rotation = e.joint.getUnityRotation();

                // Place the primitive on a specific position
                childGO.transform.localPosition = absPosition;
                childGO.transform.localRotation = rotation;

                // Position all the children
                Creature.recursiveCreateJointsFromMorphology(morphology, childNode, childGO, allJoints);

                // Calculate where the joint should be, relative to the childs coordinates system
                joint.anchor = new Vector3(0, 0, -distFaceToChildCenter);

                // Set the joint
                joint.connectedBody = parentGO.GetComponent<Rigidbody>();
            }
        }

        /// <summary>
        /// This is not defined for vectors but will do in this example
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b">Cannot have any component equal to 0</param>
        /// <returns></returns>
        private static Vector3 componentwiseDevision(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
    }
}