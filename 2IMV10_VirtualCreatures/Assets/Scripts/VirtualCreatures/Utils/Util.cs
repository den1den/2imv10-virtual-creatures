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
    }
}
