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
    class Phenotype<ResultClass>
    {
        Morphology morphology;  //for tracking only
        ExplicitNN theNetwork;

        float fitness = float.NaN;

        public Phenotype(Morphology morphology, ResultClass unity, ExplicitNN theNetwork)
        {
            this.morphology = morphology;
            this.unity = unity;
            this.theNetwork = theNetwork;
        }

        public static Phenotype<MonoBehaviour> createNew(UnityFactory factory, Morphology morphology)
        {
            if(factory == null) //create default factory
            {
                factory = new UnityFactory();
            }

            //invoke the factory
            MonoBehaviour unity = factory.constructNew(morphology);

            //get the other results of the factory
            IDictionary<EdgeMorph, UnityEngine.Joint> jointsMapping = factory.getJointsMapping();

            //create the neural network
            IList<UnityEngine.Joint> joints = morphology.edges.Select(e => jointsMapping[e]).ToList();
            ExplicitNN theNetwork = ExplicitNN.createNew(morphology, joints);

            return new Phenotype<MonoBehaviour>(morphology, unity, theNetwork);
        }
    }

    /// <summary>
    /// A factory for unity objects
    /// </summary>
    public class UnityFactory : IUnityFactory<UnityEngine.Object, MonoBehaviour>
    {
        BoxCollider boxcolider; //example
        public UnityFactory()
        {
            // default values for the factory
            this.boxcolider = new BoxCollider();
        }

        /// <summary>
        /// This generates body parts
        /// </summary>
        /// <param name="node">the source node</param>
        /// <returns>The shape which can have a joint connected to it</returns>
        protected override UnityEngine.Object processNode(Node node)
        {
            //TODO
            throw new NotImplementedException();
        }

        /// <summary>
        /// This connects and translates body parts
        /// </summary>
        /// <param name="src">The source node</param>
        /// <param name="source">The source body part</param>
        /// <param name="edge">The edge with all the joint specification info</param>
        /// <param name="dst">The destination node</param>
        /// <param name="destination">The (already generated) destination body part (at the origin)</param>
        /// <returns>The joint that is added</returns>
        protected override UnityEngine.Joint processJoint(Node src, UnityEngine.Object source, EdgeMorph edge, Node dst, UnityEngine.Object destination)
        {
            //TODO
            throw new NotImplementedException();
            //this will be a large function probably for all the case distinctions.
        }

        /// <summary>
        /// Generate the resulting class
        /// </summary>
        /// <returns></returns>
        protected override MonoBehaviour constructResult()
        {
            //TODO
            throw new NotImplementedException();
        }
    }

    public abstract class IUnityFactory<InstanceClass, ResultClass>
        where InstanceClass : UnityEngine.Object
        where ResultClass : UnityEngine.MonoBehaviour
    {
        public IUnityFactory() { }
        
        protected LinkedList<InstanceClass> instances;
        protected IDictionary<EdgeMorph, UnityEngine.Joint> jointsMapping;

        public IDictionary<EdgeMorph, UnityEngine.Joint> getJointsMapping()
        {
            return this.jointsMapping;
        }

        /// <summary>
        /// Traverses the tree
        /// </summary>
        /// <param name="source">Root of the tree</param>
        /// <param name="sourceGameObject">Generated shape of the root</param>
        /// <param name="edgelist">All the edges in the graph</param>
        protected void processJointRecursion(Node source, InstanceClass sourceGameObject, IList<EdgeMorph> edgelist)
        {
            IEnumerator<EdgeMorph> itChildren = edgelist.Where(edge => edge.source == source).GetEnumerator();
            while (itChildren.MoveNext())
            {
                EdgeMorph child = itChildren.Current;

                Node destination = child.destination;
                InstanceClass destinationGameObject = processNode(destination);
                this.instances.AddLast(destinationGameObject);

                Joint j = processJoint(source, sourceGameObject, child, destination, destinationGameObject);
                jointsMapping.Add(child, j);

                processJointRecursion(destination, destinationGameObject, edgelist);
            }
        }
        
        /// <summary>
        /// Construction by a morphology.
        /// </summary>
        /// <param name="morphology">A predfined static morhology that defines this creature.</param>
        /// <returns></returns>
        public ResultClass constructNew(Morphology morphology)
        {
            this.instances = new LinkedList<InstanceClass>();
            this.jointsMapping = new Dictionary<EdgeMorph, UnityEngine.Joint>();

            Node root = morphology.root;
            InstanceClass rootObj = processNode(root);
            this.instances.AddLast(rootObj);

            processJointRecursion(root, rootObj, morphology.edges);

            return constructResult();
        }

        protected abstract  ResultClass constructResult();

        protected abstract InstanceClass processNode(Node node);

        protected abstract UnityEngine.Joint processJoint(Node src, InstanceClass source, EdgeMorph edge, Node dst, InstanceClass destination);
    }
}
