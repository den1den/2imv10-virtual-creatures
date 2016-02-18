using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Logic.VirtualCreatures
{
    /// <summary>
    /// A creature that is executable. This class basically consists of a UnityObject and a NeuralNetwork that is connected to it.
    /// </summary>
    class Phenotype
    {
        Morphology morphology; //for tracking only
        SomeUnityObject someUnityOutput;
        ExplicitNN theNetwork;

        float fitness;

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

    /// <summary>
    /// The Unity Classes, this still has to be defined
    /// </summary>
    public class SomeUnityObject : MonoBehaviour
    {
        /// <summary>
        /// Construction by a morphology.
        /// </summary>
        /// <param name="morphology">A predfined static morhology that defines this creature.</param>
        /// <returns></returns>
        internal static SomeUnityObject createNew(Morphology morphology)
        {
            processNode(morphology.root);
            processJointRec(morphology.root, morphology.edges);
            throw new NotImplementedException();
        }

        static void processJointRec(Node src, IList<EdgeMorph> edgelist)
        {
            IEnumerator<EdgeMorph> it = edgelist.GetEnumerator();
            while (it.MoveNext())
            {
                EdgeMorph e = it.Current;
                if(e.source == src)
                {
                    it.Dispose();
                    processJoint(src, e);
                    processJointRec(e.destination, edgelist);
                }
            }
        }

        static void processNode(Node node)
        {

        }

        static void processJoint(Node node, EdgeMorph edge)
        {

        }
        /// <summary>
        /// Get the Unity Joints. This is used by the neural network to controll the creature.
        /// </summary>
        /// <returns>All joints such that joints.get(i) is the joint of the edges.get(i) from the used morhology for generation of this instance.</returns>
        internal IList<Joint> getJoints()
        {
            throw new NotImplementedException();
        }
    }
}
