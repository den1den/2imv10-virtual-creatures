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
        UnityEngine.Object someUnityOutput;
        ExplicitNN theNetwork;

        public Phenotype(Morphology morphology, UnityEngine.Object someUnityOutput, ExplicitNN theNetwork)
        {
            this.morphology = morphology;
            this.someUnityOutput = someUnityOutput;
            this.theNetwork = theNetwork;
        }

        public static Phenotype createNew(Morphology morphology)
        {
            //
            UnityEngine.Object someUnityOutput = null;
            throw new NotImplementedException();

            //
            ExplicitNN theNetwork = ExplicitNN.createNew(morphology);

            return new Phenotype(morphology, someUnityOutput, theNetwork);
        }
    }
}
