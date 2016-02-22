using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VirtualCreatures
{
    // We shoud leave this class without any Unity thing related just as abstract as possible.
    class Phenotype
    {
        ExplicitNN nerves;

        public Phenotype(Morphology morphology, Joint[] joints)
        {
            this.nerves = new NaiveENN(morphology, joints);
        }

        public void update()
        {
            // Time.deltaTime;
            // Read and write values of Joints once
            this.nerves.tick();
            this.nerves.tick();
        }

    }

    internal class NaiveENN : ExplicitNN
    {
        public NaiveENN(Morphology morphology, Joint[] joints) : base(joints)
        {

        }

        internal override void tick()
        {
            throw new NotImplementedException();
        }
    }

    internal abstract class ExplicitNN
    {
        Joint[] joints;
        internal ExplicitNN(Joint[] joints)
        {
            this.joints = joints;
        }

        internal abstract void tick();
    }
}
