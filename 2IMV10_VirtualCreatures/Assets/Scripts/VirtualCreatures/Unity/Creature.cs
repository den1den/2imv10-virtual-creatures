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
        private IList<Joint> joints;
        private IList<> shapes;

        public Creature(Morphology morphology)
        {
            phenotype = new Phenotype(morphology, joints.ToArray<Joint>());

            // Load the joints from the morphology to this Creature list of joints

            joints = Creature.createJointsFromMorphology(morphology);
        }

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
            return joints.ToList();
        }

        /// <summary>
        /// Load all joints from the morphology
        /// </summary>
        /// <param name="morphology"></param>
        /// <returns>Joints objects</returns>
        public static IList<Joint> createJointsFromMorphology(Morphology morphology)
        {
            IList<Joint> jointsList = new List<Joint>();

            // This function is just a deeper step to abstract more the createJointsFromMorphology function
            recursiveCreateJointsFromMorphology(morphology, morphology.root, jointsList);

            return jointsList;
        }

        /// <summary>
        /// Recursive function to create joints from morphology
        /// </summary>
        /// <param name="morphology"></param>
        /// <param name="node"></param>
        /// <param name="jointsList"></param>
        private static void recursiveCreateJointsFromMorphology(Morphology morphology, Node node, IList<Joint> jointsList)
        {
            IList<EdgeMorph> edges = node.edges(morphology.edges);

            // Iterate over each edge that we have on the next node of the recursion
            foreach (EdgeMorph e in edges)
            {
                jointsList.Add(e.joint.createJoint());
                Creature.recursiveCreateJointsFromMorphology(morphology, e.destination, jointsList);
            }
        }
    }
}