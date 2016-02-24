﻿using UnityEngine;
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

        private IList<Joint> joints = new List<Joint>();
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
            phenotype.update(Time.fixedDeltaTime);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phenotype"></param>
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

            // New creature instance
            Creature newCreature = (Creature)creatureObject.GetComponent<Creature>();

            // This function is just a deeper step to abstract more the createJointsFromMorphology function
            recursiveCreateJointsFromMorphology(morphology, morphology.root, null, newCreature);

            // Creature Phenotype from morphology
            newCreature.setPhenotype(new Phenotype(morphology, newCreature.getJoints().ToArray<Joint>()));
            
            //GameObject primitiveObject = Creature.createPrimitive();
            //primitiveObject.transform.parent = creatureObject.transform;
            newCreature.transform.position = new Vector3(Random.Range(0, 10), 10, 0);

            return newCreature;
        }

     
        /// <summary>
        /// 
        /// </summary>
        /// <param name="morphology"></param>
        /// <param name="node"></param>
        /// <param name="lastJoint"></param>
        /// <param name="parent"></param>
        private static void recursiveCreateJointsFromMorphology(Morphology morphology, Node node, Joint lastJoint, Creature parent)
        {
            // Create a primitive from the current node
            GameObject primitive = node.shape.createPrimitive();

            // Connect the last joint created to the current destination primitive
            if(lastJoint != null)
                lastJoint.connectedBody = primitive.GetComponent<Rigidbody>();

            // Get the edges of the current node
            IList<EdgeMorph> edges = node.edges(morphology.edges);

            // Iterate over each edge that we have for the current node
            foreach (EdgeMorph e in edges)
            {
                // Create a joint and add it to the parent joint list
                Joint joint = e.joint.createJoint(primitive);
                parent.AddJoint(joint);

                Creature.recursiveCreateJointsFromMorphology(morphology, e.destination, joint, parent);
            }

            // Add primitive to the creature
            parent.AddPrimitive(primitive);

        }
    }
}