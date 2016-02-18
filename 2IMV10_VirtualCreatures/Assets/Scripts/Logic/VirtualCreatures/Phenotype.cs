using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Logic.VirtualCreatures
{
    class Phenotype
    {
        Morphology morphology; //for tracking only
        SomeUnityObject someUnityOutput;
        ExplicitNN theNetwork;

        public Phenotype(Morphology morphology, SomeUnityObject someUnityOutput, ExplicitNN theNetwork)
        {
            this.morphology = morphology;
            this.someUnityOutput = someUnityOutput;
            this.theNetwork = theNetwork;
        }

        public static Phenotype createNew(Morphology morphology)
        {
            SomeUnityObject someUnityOutput = SomeUnityObject.createNew(morphology);
            IList<Joint> joints = someUnityOutput.getJoints();
            
            ExplicitNN theNetwork = ExplicitNN.createNew(morphology, joints);

            return new Phenotype(morphology, someUnityOutput, theNetwork);
        }
    }

    public class SomeUnityObject
    {
        internal static SomeUnityObject createNew(Morphology morphology)
        {
            throw new NotImplementedException();
        }

        internal IList<Joint> getJoints()
        {
            throw new NotImplementedException();
        }
    }
}
