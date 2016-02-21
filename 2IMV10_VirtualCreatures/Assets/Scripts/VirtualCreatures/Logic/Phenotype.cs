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

        Morphology morphology; //for tracking only
        //Creature creature;
        ExplicitNN theNetwork;

        public Phenotype(Morphology morphology /*, Creature creature*/, ExplicitNN theNetwork)
        {
            this.morphology = morphology;
            //this.creature = creature;
            this.theNetwork = theNetwork;
        }


        /*public static Phenotype createNew(Morphology morphology)
        {
            Creature creature = CreatureFactory.createNewCreature(morphology);

            IList<Joint> joints = PhenotypeObject.getJoints();
            
            ExplicitNN theNetwork = ExplicitNN.createNew(morphology, joints);

            return new Phenotype(morphology, someUnityOutput, theNetwork);
        }*/


    }
}
