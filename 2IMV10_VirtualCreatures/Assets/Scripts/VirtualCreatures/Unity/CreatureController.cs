using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualCreatures {
    /// <summary>
    /// An script to controll a single creature.
    /// This parent (the prefab) is the Phenotype of a creature.
    /// The purpose of this script is to control a single creature.
    /// </summary>
    public class CreatureController : MonoBehaviour {

        /// <summary>
        /// The neural network that is called to control this creature
        /// </summary>
        public ExplicitNN neuralNetwork = null;

        // Use this for initialization
        void Start()
        {
            // Let the script run for a couple of ticks to settle down
            float dt = 1f / 60; // assuming around 60 FPS
            neuralNetwork.doTicks(20, dt);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            int STEPS = 2;
            // TODO decide wether: When the dt changes to much we should do more ticks? in the network to keep it consistent with Update and Fixedupdate functionalities.
            float dt = Time.deltaTime / STEPS;

            // Do ticks of the neural network for each physics step   
            neuralNetwork.doTicks(STEPS, dt);
        }

        void onDestroy()
        {
            neuralNetwork.onDestory();
        }

        public Vector3 getCenterOfMass()
        {
            //FIXME: this might only be the center of mass of the root, instead of the center of mass of all components
            return getRoot().GetComponent<Rigidbody>().worldCenterOfMass;
        }

        public GameObject getRoot()
        {
            return gameObject.transform.GetChild(0).gameObject;
        }

        /// <summary>
        /// Create a creture at some position
        /// </summary>
        /// <param name="morphology"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns>The controlling script of the creature</returns>
        public static CreatureController constructCreature(Morphology morphology, Vector3 position, Quaternion rotation)
        {
            // Instantiate creature prefab to scene
            GameObject phenotype = (GameObject)Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Creature.prefab"), position, rotation);

            // Get the CreatureController script from the prefab
            CreatureController creatureController = phenotype.GetComponent<CreatureController>();
            
            // Recursivaly create and connect all components
            // Start with the root
            GameObject creatureRootNode = createUnscaledGameObject(morphology.root.shape);
            creatureRootNode.transform.parent = phenotype.transform;
            creatureRootNode.transform.localPosition = Vector3.zero;
            // Then recursivly traverse all connected edges
            Joint[] joints = new Joint[morphology.edges.Count];
            recursiveCreateJointsFromMorphology(morphology, morphology.root, creatureRootNode, joints); // recursive call

            // Create the Neural Network
            ExplicitNN neuralNetwork = new NaiveNN(morphology, joints);
            // Link the neural network to the script
            creatureController.neuralNetwork = neuralNetwork;
            
            return creatureController;
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
            IList<EdgeMorph> outgoing = morphology.getOutgoingEdges(parentNode);
            foreach (EdgeMorph e in outgoing)
            {
                // Create a GameObject for the next node
                Node childNode = e.destination;
                GameObject childGO = createUnscaledGameObject(childNode.shape);
                
                // Center of parent to the position on the surface
                Vector3 facePosition = e.getUnityPositionAnchor();
                // Directional unitvector from position on the surface towards center of the child
                Vector3 direction = e.joint.getUnityDirection();
                // Calculate the distance between center of child and the position on the parent's surface
                float distFaceToChildCenter = (float)e.joint.hover + childNode.shape.getBound(Face.REVERSE);
                // Positional vector from parent to child
                Vector3 localPosition = facePosition + distFaceToChildCenter * direction;

                // Calculate the rotation needed to go from the parent to the childs coordinate system
                Quaternion localRotation = e.joint.getUnityRotation();

                // Place the child on the correct position
                childGO.transform.parent = parentGO.transform;
                childGO.transform.localPosition = localPosition;
                childGO.transform.localRotation = localRotation;

                // Create the joint at the parent and set the direction of the joint
                Joint joint = createJoint(e.joint, parentGO, childGO, facePosition, localRotation);

                int index = morphology.edges.IndexOf(e);
                allJoints[index] = joint;

                // Position all the children
                CreatureController.recursiveCreateJointsFromMorphology(morphology, childNode, childGO, allJoints);
            }
        }

        /// <summary>
        /// Create joint and set the orentiation.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        static Joint createJoint(JointSpecification spec, GameObject parent, GameObject child, Vector3 anchor, Quaternion rotation)
        {
            // First specifiy the joint
            Joint joint;
            switch (spec.jointType)
            {
                case JointType.FIXED:
                    FixedJoint fixedjoint = parent.AddComponent<FixedJoint>();
                    joint = fixedjoint;
                    break;
                case JointType.HINDGE:
                    HingeJoint hindgeJoint = parent.AddComponent<HingeJoint>();
                    hindgeJoint.axis = rotation * Vector3.right;
                    joint = hindgeJoint;
                    break;
                case JointType.PISTON:
                    SpringJoint springJoint = parent.AddComponent<SpringJoint>();
                    joint = springJoint;
                    break;
                case JointType.ROTATIONAL:
                    joint = null;
                    break;
                default:
                    throw new NotImplementedException();
            }
            joint.anchor = anchor;

            // Then connect and let it be autoconfigured
            joint.connectedBody = child.GetComponent<Rigidbody>();
            joint.autoConfigureConnectedAnchor = true; // Triggers the autoconfiguration
            joint.enableCollision = true;

            return joint;
        }

        static GameObject createUnscaledGameObject(ShapeSpecification spec)
        {
            GameObject primitive;

            if (spec.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)spec;
                primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                BoxCollider collider = primitive.GetComponent<BoxCollider>();
                collider.size = rect.getSize();
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