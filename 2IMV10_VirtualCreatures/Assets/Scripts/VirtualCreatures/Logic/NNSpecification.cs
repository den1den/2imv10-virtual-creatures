using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VirtualCreatures
{
    /// <summary>
    /// The specification of a Neural Network with input and output.
    /// </summary>
    public class NNSpecification
    {
        public IList<Neuron> neurons;
        public IList<InterfaceNode> networkIO;
        public IList<Sensor> sensors;
        public IList<Actor> actors;

        public IList<InternalConnection> internalConnections;
        public IList<ExternalConnection> externalConnections;

        public NNSpecification(IList<Neuron> neurons, IList<InterfaceNode> networkIO, IList<InternalConnection> internalConnections, IList<ExternalConnection> externalConnections) : this(neurons, networkIO, new List<Sensor>(0), new List<Actor>(0), internalConnections, externalConnections) { }

        public NNSpecification(IList<Neuron> neurons, IList<InterfaceNode> networkIO, IList<Sensor> sensors, IList<Actor> actors, IList<InternalConnection> internalConnections, IList<ExternalConnection> externalConnections)
        {
            IEnumerable<NeuralNode> given = neurons.Select(n => (NeuralNode)n).Union(networkIO.Select(n => (NeuralNode)n)).Union(networkIO.Select(n => (NeuralNode)n)).Union(sensors.Select(n => (NeuralNode)n)).Union(actors.Select(n => (NeuralNode)n));
            IEnumerable<NeuralNode> found = internalConnections.SelectMany(c => new NeuralNode[] { c.source, c.destination }).Union(externalConnections.SelectMany(c => new NeuralNode[] { c.source, c.destination }));
            if(given.Where(nn => nn is Neuron).Except(found).Count() > 0)
            {
                throw new ArgumentException("Not used Neuron given");
            }
            if (found.Except(given).Count() > 0) //check for foreigh neurons
            {
                throw new ArgumentException("Non given NeuralNodes found");
            }
        }

        public NNSpecification createEmptyWriteNetwork(IList<Actor> actors, IList<InterfaceNode> actorInterfaces)
        {
            if(actors.Count() != actorInterfaces.Count())
            {
                throw new ArgumentException();
            }
            IList<Neuron> neurons = new List<Neuron>(0);
            IList<InterfaceNode> networkIO = new List<InterfaceNode>(actorInterfaces);
            IList<Sensor> sensors = new List<Sensor>(0);
            actors = new List<Actor>(actors);
            IList<InternalConnection> internalConnections = new List<InternalConnection>();
            IList<ExternalConnection> externalConnections = new List<ExternalConnection>();
            for (int i = 0; i < actors.Count(); i++)
            {
                ExternalConnection c = new ExternalConnection(actorInterfaces[i], actors[i]);
                externalConnections.Add(c);
            }
            return new NNSpecification(neurons, networkIO, sensors, actors, internalConnections, externalConnections);
        }

        public NNSpecification createEmptyReadWriteNetwork(IList<Sensor> sensors, IList<Actor> actors, IList<InterfaceNode> interfaces)
        {
            if (actors.Count() + sensors.Count() != interfaces.Count())
            {
                throw new ArgumentException();
            }
            IList<Neuron> neurons = new List<Neuron>(0);
            IList<InterfaceNode> networkIO = new List<InterfaceNode>(interfaces);
            sensors = new List<Sensor>(sensors);
            actors = new List<Actor>(actors);
            IList<InternalConnection> internalConnections = new List<InternalConnection>();
            IList<ExternalConnection> externalConnections = new List<ExternalConnection>();
            for (int i = 0; i < sensors.Count(); i++)
            {
                ExternalConnection c = new ExternalConnection(sensors[i], interfaces[i]);
                externalConnections.Add(c);
            }
            int iOffset = sensors.Count();
            for (int i = 0; i < actors.Count(); i++)
            {
                ExternalConnection c = new ExternalConnection(interfaces[iOffset + i], actors[i]);
                externalConnections.Add(c);
            }
            return new NNSpecification(neurons, networkIO, sensors, actors, internalConnections, externalConnections);
        }

        /// <summary>
        /// Check for unused nodes
        /// </summary>
        private void checkDummies()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// These function require at least two values
        /// </summary>
        public static readonly Function[] binaryOperators = {Function.MIN, Function.MAX, Function.DEVISION, Function.PRODUCT, Function.SUM,
            Function.GTE, Function.IF, Function.INTERPOLATE, Function.IFSUM, };

        /// <summary>
        /// Check if this neuron is in this network
        /// </summary>
        /// <param name="n"></param>
        private void check(Neuron n)
        {
            if (!neurons.Contains(n))
            {
                throw new ArgumentException();
            }
        }
    }

    public abstract class NeuralNode { }

    /// <summary>
    /// A sensor of this network
    /// </summary>
    public class Sensor : NeuralNode { }

    /// <summary>
    /// An actor of this network
    /// </summary>
    public class Actor : NeuralNode {
        float weight; //could be usefull?
    }

    /// <summary>
    /// A node that is in the interface of the network
    /// </summary>
    public class InterfaceNode : NeuralNode { }

    /// <summary>
    /// A single neuron in a network.
    /// </summary>
    public class Neuron : NeuralNode
    {
        public Function function;

        public Neuron(Function function)
        {
            this.function = function;
        }
    }

    public class BaseConnection
    {
        public NeuralNode source;
        public NeuralNode destination;

        public BaseConnection(NeuralNode source, NeuralNode destination)
        {
            this.source = source;
            this.destination = destination;
        }
    }

    public class BaseWConnection : BaseConnection
    {
        float weight;
        protected BaseWConnection(NeuralNode source, NeuralNode destination, float weight) : base(source, destination)
        {
            this.weight = weight;
        }
    }

    /// <summary>
    /// A connection that goes to an internal Neuron
    /// </summary>
    public class InternalConnection : BaseWConnection
    {
        public InternalConnection(Neuron source, Neuron destination, float weight) : base(source, destination, weight) { }
        public InternalConnection(Sensor source, Neuron destination, float weight) : base(source, destination, weight) { }
        public InternalConnection(InterfaceNode source, Neuron destination, float weight) : base(source, destination, weight) { }
    }

    /// <summary>
    /// An connection that goes towards a Interface or Actor
    /// </summary>
    public class ExternalConnection : BaseConnection
    {
        public ExternalConnection(Neuron source, InterfaceNode destination) : base(source, destination) { }
        public ExternalConnection(InterfaceNode source, Actor destination) : base(source, destination) { }
        public ExternalConnection(Neuron source, Actor destination) : base(source, destination) { }

        /// <summary>
        /// Only for usage by an Empty network!!!
        /// </summary>
        internal ExternalConnection(Sensor source, InterfaceNode destination) : base(source, destination) { }
    }
    
    /// <summary>
    /// A the different functions that a neuron could have
    /// </summary>
    public enum Function {
        ABS, ATAN, SIN, COS, EXP, LOG,
        DIFFERENTIATE, INTERGRATE, MEMORY, SMOOTH,
        SAW, WAVE,
        SIGMOID, SIGN,
        MIN, MAX, DEVISION, PRODUCT, SUM, GTE,
        IF, INTERPOLATE, IFSUM
    }

    public static NNSpecification test1()
    {
        Neuron n1 = new Neuron(Function.SAW);
        Neuron n2 = new Neuron(Function.SIN);

        //revert old test1 test case
        return null;
    }
}
