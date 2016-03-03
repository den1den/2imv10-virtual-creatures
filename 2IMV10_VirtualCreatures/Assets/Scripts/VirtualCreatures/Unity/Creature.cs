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
                GameObject childGO = parentNode.shape.createPrimitive(parentGO);

                // Calculate where the next shape should be by creating the joint
                Joint joint = e.joint.createJoint(childGO, e.source.shape);
                joint.connectedBody = childGO.GetComponent<Rigidbody>();
                allJoints[morphology.edges.IndexOf(e)] = joint;

                //Place the primitive on that position
                childGO.transform.parent = parentGO.transform;
                childGO.transform.position = joint.anchor;
                //primitive.transform.rotation = Calculate rotation from `e`
                
                Creature.recursiveCreateJointsFromMorphology(morphology, e.destination, childGO, allJoints);
            }
        }
    }
}