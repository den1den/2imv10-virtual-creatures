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
        public Joint[] joints;



        private Morphology morphology; //for tracking only
        //Creature creature;
        private ExplicitNN theNetwork;

        private float lastTime;


        public Phenotype(Morphology morphology, Joint[] joints)
        {
            this.joints = joints;

            this.morphology = morphology;
            //this.creature = creature;
            
            theNetwork = ExplicitNN.createNew(morphology, joints);
        }

        public void update()
        {
            // Time.deltaTime;
            // Read and write values of Joints once
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
