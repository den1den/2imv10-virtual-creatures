using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VirtualCreatures
{
    public abstract class Util {
        Util() { throw new NotImplementedException(); }

        /// <summary>
        /// This is not defined for vectors but will do in this example
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b">Cannot have any component equal to 0</param>
        /// <returns></returns>
        public static Vector3 componentwiseDevision(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public static readonly bool DEBUG;
        public static readonly bool PAUSE_AFTER_CREATURE_INITIALIZATION;
        public static readonly int INITIAL_POPULATION_SIZE;
        public static readonly float INITIAL_EVALUATION_TIME;
        public static readonly float FITNESS_EVALUATION_TIME;
        public static readonly bool WRITE_NETWORK_GRAPHS;
        public static readonly bool WRITE_NETWORK_FLOATS;

        static Util()
        {
            //DEBUG = System.Diagnostics.Debugger.IsAttached;
            DEBUG = false;
            WRITE_NETWORK_GRAPHS = true;
            WRITE_NETWORK_FLOATS = true;
            if (DEBUG)
            {
                Debug.Log("Util.DEBUG variable is True");
                PAUSE_AFTER_CREATURE_INITIALIZATION = false;

                INITIAL_POPULATION_SIZE = 10;
                INITIAL_EVALUATION_TIME = 0.1f;
                FITNESS_EVALUATION_TIME = 1.0f;
            }
            else
            {
                PAUSE_AFTER_CREATURE_INITIALIZATION = false;

                INITIAL_POPULATION_SIZE = 1;
                FITNESS_EVALUATION_TIME = 10000f;
                INITIAL_EVALUATION_TIME = 1f;
            }
        }
    }
}
