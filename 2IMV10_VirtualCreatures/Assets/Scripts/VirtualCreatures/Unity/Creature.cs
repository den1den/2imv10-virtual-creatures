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

        private Morphology morphology;
        private Phenotype phenotype;
        private Joint[] joints;

        public Creature(Morphology morphology)
        {
            phenotype = new Phenotype(morphology, joints);
        }

        // Use this for initialization
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            phenotype.update();
        }

        

        /// <summary>
        /// Return all the joints of this creature.
        /// </summary>
        /// <returns></returns>
        public IList<Joint> getJoints()
        {
            Joint[] joints = FindObjectsOfType(typeof(Joint)) as Joint[];
            return joints.ToList();
        }
    }
}