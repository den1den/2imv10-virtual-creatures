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
        /// 
        /// </summary>
        /// <param name="morphology"></param>
        /// <returns></returns>
        public static Creature Create(Morphology morphology)
        {
            // Instantiate empty creature prefab to scene
            GameObject creatureContainer = Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Creature.prefab"));

            Joint[] joints = new Joint[morphology.edges.Count];

            //recursivaly create and connect all components
            //start with the root, with no special transformation
            GameObject creatureRootNode = morphology.root.shape.createPrimitive();
            creatureRootNode.transform.parent = creatureContainer.transform;
            creatureRootNode.transform.localPosition = new Vector3(0, 50, 0);
            
            Debug.Log("Created a root " + creatureRootNode.ToString() + " with position: " + creatureRootNode.transform.position.ToString());

            // then recursivly traverse all connected edges
            recursiveCreateJointsFromMorphology(morphology, morphology.root, creatureRootNode, joints);

            //
            Creature creatureScript = (Creature)creatureContainer.GetComponent<Creature>();
            creatureScript.joints = joints.ToList<Joint>();

            //
            Debug.Log(creatureScript.joints[0]);
            Debug.Log(creatureScript.joints[1]);

            Phenotype phenotype = new Phenotype(morphology, creatureScript.getJoints().ToArray<Joint>());

            // Creature Phenotype from morphology
            creatureScript.setPhenotype(phenotype);
            
            creatureContainer.transform.position = new Vector3(UnityEngine.Random.Range(0, 50.0F), 3, 0);

            return creatureScript;
        }


        /// <summary>
        /// See draw.io drawing.
        /// For now forget the joints and rotational aspects
        /// </summary>
        /// <returns></returns>
        public static Creature CreateTest3()
        {
            ShapeSpecification b0 = new Cube(2);
            Node root = new Node(b0);

            ShapeSpecification b1 = new Cube(0.5f);
            Node n1 = new Node(b1);

            ShapeSpecification b2 = new Cube(3);
            Node n2 = new Node(b2);

            ShapeSpecification b3 = new Cube(1);
            Node n3 = new Node(b3);

            float absHover = 1f;
            JointSpecification j = JointSpecification.createSimple(1, absHover);
            NNSpecification emptyNN = NNSpecification.createEmptyNetwork();

            IList<EdgeMorph> edges = new EdgeMorph[]{
                new EdgeMorph(root, n1, j, emptyNN),
                new EdgeMorph(n1, n2, j, emptyNN),
                new EdgeMorph(n2, n3, j, emptyNN)
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
            Vector3 positionFactor = new Vector3(1.0f / parentGO.transform.lossyScale.x, 1.0f / parentGO.transform.lossyScale.y, 1.0f / parentGO.transform.lossyScale.z);

            // Get the connected edges of the current node
            IList<EdgeMorph> edges = parentNode.getEdges(morphology.edges);
            // Iterate over each edge that we have for the current node
            foreach (EdgeMorph e in edges)
            {
                // Create a primitive for the next node below the current node
                Node childNode = e.destination;
                GameObject childGO = childNode.shape.createPrimitive();

                childGO.transform.parent = parentGO.transform; // silently applies correction factor for hiearchical scaling

#if false //backup of code
                //Create the joint and set the direction of the joint
                //Joint joint = e.joint.createJoint(parentGO);
                //allJoints[morphology.edges.IndexOf(e)] = joint;
                //joint.connectedBody = childGO.GetComponent<Rigidbody>();

                // Calculate where the distance between center of child and parent
                Vector3 facePosition = e.joint.getUnityFaceAnchorPosition(parentNode.shape);
                Vector3 direction = e.joint.getUnityDirection();
                float absDist_Face_ChildCenter = e.joint.position.hover + childNode.shape.getBound(0); // attached in Z direction
                Vector3 absPosition = facePosition + absDist_Face_ChildCenter * direction;

                Quaternion rotation = e.joint.getUnityRotation();

                // Calculate where the joint should be, relative to the child
                //joint.anchor = direction * 0.5f;
#endif

                // Place the primitive on a specific position
                float testPositionX = parentNode.shape.getXBound() + e.joint.position.hover + childNode.shape.getXBound();
                Vector3 testPosition = new Vector3(testPositionX, 0, 0); //first everything to the right (so no direction)
                childGO.transform.localPosition = Vector3.Scale(testPosition, positionFactor);
                childGO.transform.localRotation = Quaternion.identity;

                Debug.Log("Created a child " + childGO.ToString() + " with position: " + childGO.transform.position.ToString());

                //position all the children
                Creature.recursiveCreateJointsFromMorphology(morphology, childNode, childGO, allJoints);
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