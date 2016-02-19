using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Logic.VirtualCreatures
{
    /// <summary>
    /// A simple layer of abstraction for the Unity implementation
    /// </summary>
    /// <typeparam name="ResultClass">The class that is needed by the algorithm to render this creature</typeparam>
    public class Phenotype<ResultClass> : IPhenotype
    {
        ResultClass unity;
        
        public Phenotype(Morphology morphology, ResultClass unity, ExplicitNN theNetwork) : base(morphology, theNetwork)
        {
            this.unity = unity;
        }
    }

    /// <summary>
    /// A creature that is executable. This class basically consists of a Unity related object and a NeuralNetwork that is connected to it.
    /// </summary>
    public abstract class IPhenotype
    {
        Morphology morphology;  //for tracking only
        ExplicitNN theNetwork;

        internal IPhenotype(Morphology morphology, ExplicitNN theNetwork)
        {
            this.morphology = morphology;
            this.theNetwork = theNetwork;
        }

        /// <summary>
        /// First attempt at the Unity intergration using a List of objects as ResultClass
        /// </summary>
        /// <param name="factory">A object that is capable of creating unity objects</param>
        /// <param name="morphology">The definition of the creature</param>
        /// <returns></returns>
        public static Phenotype<MonoBehaviour> createNew(MyUnityFactory factory, Morphology morphology)
        {
            if (factory == null)
            {
                //create default factory if no explicit factory is given
                factory = new MyUnityFactory();
            }

            //invoke the factory
            MonoBehaviour unity = factory.constructNew(morphology);

            //get the other results of the factory
            IDictionary<EdgeMorph, UnityEngine.Joint> jointsMapping = factory.getJointsMapping();

            //create the neural network using the joints
            IList<UnityEngine.Joint> joints = morphology.edges.Select(e => jointsMapping[e]).ToList();
            ExplicitNN theNetwork = ExplicitNN.createNew(morphology, joints);

            return new Phenotype<MonoBehaviour>(morphology, unity, theNetwork);
        }
    }

    /// <summary>
    /// A factory for unity objects
    /// See http://answers.unity3d.com/questions/572852/how-do-you-create-an-empty-gameobject-in-code-and.html
    /// This could be done in a sperate file???
    /// </summary>
    public class MyUnityFactory : IUnityFactory<
        UnityEngine.GameObject, //I think this is the right class???
        UnityEngine.MonoBehaviour> //I think this is the right class???
    {
        BoxCollider boxcolider; //example
        GameObject spherePreFab = null; //example
        public MyUnityFactory()
        {
            //TODO
            this.boxcolider = new BoxCollider(); //example
        }

        /// <summary>
        /// This generates body parts
        /// </summary>
        /// <param name="node">the source node</param>
        /// <returns>The shape which can have a joint connected to it</returns>
        protected override UnityEngine.GameObject processNode(Node node)
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
        protected override UnityEngine.Joint processJoint(Node src, UnityEngine.GameObject source, EdgeMorph edge, Node dst, UnityEngine.GameObject destination)
        {
            //TODO
            throw new NotImplementedException();
            //this will be a large function probably because of all the case distinctions.
        }

        /// <summary>
        /// Generate the resulting class (using the intermediate objects).
        /// </summary>
        /// <returns></returns>
        protected override MonoBehaviour constructResult()
        {
            //TODO
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// A wrapper for the Unity factory
    /// </summary>
    /// <typeparam name="InstanceClass">An intermediate result of the nodes (a shape instantiation for exmaple)</typeparam>
    /// <typeparam name="ResultClass">The class that can be used to evaluate the creatures fitness</typeparam>
    public abstract class IUnityFactory<InstanceClass, ResultClass>
        where InstanceClass : UnityEngine.Object
        where ResultClass : UnityEngine.Behaviour
    {
        public IUnityFactory() { }
        
        protected LinkedList<InstanceClass> instances;
        protected IDictionary<EdgeMorph, UnityEngine.Joint> jointsMapping;

        /// <summary>
        /// This might be used in the future to only make it calculatable, exclude lightning and meshes to get faster results.
        /// </summary>
        public Boolean onlyCalc = false;

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
        /// Construction of the ResultClass by a morphology.
        /// </summary>
        /// <param name="morphology">A predfined static morhology that defines this creature.</param>
        /// <returns>An instance of ResultClass that can be used to simulate the fitness of this creature.</returns>
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
