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
            GameObject creatureObject = (GameObject)Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Creature.prefab"));

            creatureObject.transform.position = new Vector3(UnityEngine.Random.Range(0, 10), 10, 0);

            // New creature instance (top of unity hierachy)
            Creature newCreature = (Creature)creatureObject.GetComponent<Creature>();

            Joint[] joints = new Joint[morphology.edges.Count];

            //recursivaly create and connect all components
            //start with the root, with no special transformation
            GameObject creatureRootNode = morphology.root.shape.createPrimitive(creatureObject); //transformation is default (0,0,0)
            //then recursivly traverse all connected edges
            recursiveCreateJointsFromMorphology(morphology, morphology.root, creatureRootNode, joints);

            //
            newCreature.joints = joints.ToList<Joint>();

            //
            Debug.Log(newCreature.joints[0]);
            Debug.Log(newCreature.joints[1]);

            Phenotype phenotype = new Phenotype(morphology, newCreature.getJoints().ToArray<Joint>());

            // Creature Phenotype from morphology
            newCreature.setPhenotype(phenotype);

            return newCreature;
        }


        /// <summary>
        /// See draw.io drawing.
        /// For now forget the joints and rotational aspects
        /// </summary>
        /// <returns></returns>
        public static Creature CreateTest3()
        {
            GameObject creatureContainer = Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Creature.prefab"));

            ShapeSpecification ss1 = new Cube(2f);
            ShapeSpecification ss2 = new Cube(.5f);
            ShapeSpecification ss3 = Rectangle.createWidthDepthHeight(1f, 1f, 3f);
            ShapeSpecification ss4 = new Cube(1f);

            GameObject go1 = ss1.createPrimitive();
            go1.transform.parent = creatureContainer.transform;
            go1.transform.localPosition = new Vector3(0, 40f, 0); //create them high enough to make sure they do not collide with the ground

            GameObject go2 = ss2.createPrimitive();
            go2.transform.parent = go1.transform;
            go2.transform.localPosition = new Vector3(2.25f, 0, 0); //calculated
            go2.transform.localPosition = new Vector3(1.75f, 0, 0); //why 1.75 on first time ?
            go2.transform.localPosition = new Vector3(1.125f, 0, 0); //and it becomes stable at 1.125 ?

            GameObject go3 = ss3.createPrimitive();
            go3.transform.parent = go2.transform;
            go3.transform.localPosition = new Vector3(5f, 0, 0); //calculated
            go3.transform.localPosition = new Vector3(3.5f, 0, 0); //why 3.5?

            GameObject go4 = ss4.createPrimitive();
            go4.transform.parent = go3.transform;
            go4.transform.localPosition = new Vector3(8f, 0, 0); //calculated
            go4.transform.localPosition = new Vector3(2f, 0, 0); //why 2?

            return creatureContainer.GetComponent<Creature>();
        }

        /// <summary>
        /// Adds all the cildren recursivly to the parent node
        /// </summary>
        /// <param name="morphology">invariant</param>
        /// <param name="parentNode">parental node</param>
        /// <param name="parentGO">created parental game object</param>
        /// <param name="allJoints">result</param>
        private static void recursiveCreateJointsFromMorphology(Morphology morphology, Node parentNode, GameObject parentGO, IList<Joint> allJoints)
        {
            // Get the edges of the current node
            IList<EdgeMorph> edges = parentNode.getEdges(morphology.edges);
            // Iterate over each edge that we have for the current node
            foreach (EdgeMorph e in edges)
            {
                // Create a primitive for the next node from the current node
                GameObject childGO = e.destination.shape.createPrimitive(parentGO);

                // Calculate where the next shape should be by creating the joint
                Vector3 facePosition = e.joint.getUnityFaceAnchorPosition(parentNode.shape);
                Vector3 childCenterPosition = facePosition + (e.joint.position.hover + e.destination.shape.getZBound() / 2) * e.joint.getUnityDirection();

                //create the joint
                Joint joint = e.joint.createJoint(childGO);
                allJoints[morphology.edges.IndexOf(e)] = joint;
                joint.anchor = facePosition;
                joint.connectedBody = childGO.GetComponent<Rigidbody>();

                //Place the primitive on that position
                childGO.transform.parent = parentGO.transform;
                childGO.transform.position = childCenterPosition;
                childGO.transform.rotation = e.joint.getUnityRotation();
                
                Creature.recursiveCreateJointsFromMorphology(morphology, e.destination, childGO, allJoints);
            }
        }
    }
}