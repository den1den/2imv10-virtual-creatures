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

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

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