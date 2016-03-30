using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualCreatures {
    /// <summary>
    /// An script to controll a single creature.
    /// This parent (the prefab) is the Phenotype of a creature.
    /// The purpose of this script is to call the neuralnetwork each time.
    /// </summary>
    public class CreatureController : MonoBehaviour {

        /// <summary>
        /// The constructed neural network that controlls this creature
        /// </summary>
        ExplicitNN neuralNetwork = null;

        void setReferences(ExplicitNN neuralNetwork)
        {
            this.neuralNetwork = neuralNetwork;
        }

        // Use this for initialization
        void Start()
        {
            // Let the script run for a couple of ticks to settle down
            neuralNetwork.tickDt = 0;
            neuralNetwork.tick(20);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // Update phenotype in each physics engine step
            float dt = Time.deltaTime;

            int STEPS = 2;

            // Read and write values of Joints once
            neuralNetwork.tickDt = dt / STEPS;
            neuralNetwork.tick(STEPS); // TODO decide wether: When the dt changes to much we should do more ticks? in the network to keep it consistent with Update and Fixedupdate functionalities.
        }

        public Vector3 getCenterOfMass()
        {
            throw new NotImplementedException();
        }

        public static CreatureController constructCreature(Morphology morphology, Vector3 position)
        {
            return CreatureController.construct(morphology, position, Quaternion.identity);
        }

        /// <summary>
        /// Create a creture at some position
        /// </summary>
        /// <param name="morphology"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns>The controlling script of the creature</returns>
        public static CreatureController construct(Morphology morphology, Vector3 position, Quaternion rotation)
        {
            // Instantiate creature prefab to scene
            GameObject superInstance = (GameObject)Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Creature.prefab"), position, rotation);

            // Get `this` from the prefab
            CreatureController controller = superInstance.GetComponent<CreatureController>();

            // Create new joints list 
            Joint[] joints = new Joint[morphology.edges.Count];

            // Recursivaly create and connect all components
            // Start with the root
            GameObject creatureRootNode = createUnscaledGameObject(morphology.root.shape);
            creatureRootNode.transform.parent = superInstance.transform;
            creatureRootNode.transform.localPosition = Vector3.zero; // not redundant

            // then recursivly traverse all connected edges
            recursiveCreateJointsFromMorphology(morphology, morphology.root, creatureRootNode, joints);

            // Create the Neural Network
            ExplicitNN neuralNetwork = NaiveENN.construct(morphology, joints);

            // Store everything in the creature script
            controller.setReferences(neuralNetwork);
            
            //create.transform.position = position;
            return controller; //script
        }

        /// <summary>
        /// Adds all the cildren recursivly to the parent node
        /// </summary>
        /// <param name="morphology">The full morhology (to traverse the edges)</param>
        /// <param name="parentNode">The node ofwhich the children should be added</param>
        /// <param name="parentGO">The created gameobject of the parent</param>
        /// <param name="allJoints">A list of all the joints found thusfar (in the order of traversal)</param>
        static void recursiveCreateJointsFromMorphology(Morphology morphology, Node parentNode, GameObject parentGO, IList<Joint> allJoints)
        {
            // Iterate over each edge that we have for the current node
            foreach (EdgeMorph e in morphology.getOutgoingEdges(parentNode))
            {
                // Create a primitive for the next node
                Node childNode = e.destination;
                GameObject childGO = createUnscaledGameObject(childNode.shape);

                // Calculate where it should be positioned (relative to parent)
                // Calculate the distance between center of child and parent
                Vector3 facePosition = e.getUnityPositionAnchor(); //on parent shape
                Vector3 direction = e.joint.getUnityDirection(); //towards child
                float distFaceToChildCenter = (float)e.joint.hover + childNode.shape.getBound(Face.REVERSE); //on child shape
                Vector3 localPosition = facePosition + distFaceToChildCenter * direction;

                Quaternion localRotation = e.joint.getUnityRotation();

                // Place the child on the correct position
                childGO.transform.parent = parentGO.transform;
                childGO.transform.localPosition = localPosition;
                childGO.transform.localRotation = localRotation;

                // Create the joint at the parent and set the direction of the joint
                Joint joint = createJoint(e.joint, childGO);
                joint.connectedBody = parentGO.GetComponent<Rigidbody>();
                int index = morphology.edges.IndexOf(e);
                allJoints[index] = joint;

                // Position all the children
                CreatureController.recursiveCreateJointsFromMorphology(morphology, childNode, childGO, allJoints);
                
                // Calculate where the joint should be, relative to the childs coordinates system
                joint.anchor = new Vector3(0, 0, -distFaceToChildCenter);
            }
        }

        /// <summary>
        /// Create joint and set the orentiation.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        static Joint createJoint(JointSpecification joint, GameObject parent)
        {
            switch (joint.jointType)
            {
                case JointType.FIXED:
                    FixedJoint fixedjoint = parent.AddComponent<FixedJoint>();
                    return fixedjoint;
                case JointType.HINDGE:
                    //positive angle is in the direction of the normal
                    HingeJoint hindgeJoint = parent.AddComponent<HingeJoint>();
                    hindgeJoint.axis = joint.getUnityAxisUnitVector();
                    return hindgeJoint;
                case JointType.PISTON:
                    SpringJoint springJoint = parent.AddComponent<SpringJoint>();
                    return springJoint;
                case JointType.ROTATIONAL:
                    break;
            }
            throw new NotImplementedException();
        }

        static GameObject createUnscaledGameObject(ShapeSpecification spec)
        {
            GameObject primitive;

            if (spec.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)spec;
                primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                BoxCollider collider = primitive.GetComponent<BoxCollider>();
                collider.size = spec.getSize();
            }
            else if (spec.GetType() == typeof(Sphere))
            {
                Sphere sphere = (Sphere)spec;
                primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereCollider collider = primitive.GetComponent<SphereCollider>();
                collider.radius = sphere.getRadius()/2;
            }
            else throw new NotImplementedException();

            Mesh mesh = primitive.GetComponent<MeshFilter>().mesh;
            mesh.vertices = mesh.vertices.Select(v => new Vector3(v.x * spec.getXSize(), v.y * spec.getYSize(), v.z * spec.getZSize())).ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            primitive.AddComponent<Rigidbody>();

            return primitive;
        }
    }
}