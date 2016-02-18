using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Logic.VirtualCreatures
{
    public class ExplicitNN
    {
        public static ExplicitNN createNew(Morphology morphology, IList<Joint> joints)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This invokes the network. It reads the input values from the UnityJoints and sets all the forces accordingly.
        /// </summary>
        public void tick()
        {

        }
    }
}