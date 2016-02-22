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
        public IList<InterfaceNode> networkIn;
        public IList<InterfaceNode> networkOut;
        public IList<Sensor> sensors; //can be made integers
        public IList<Actor> actors; //can be made integers

        public IList<InternalConnection> internalConnections;
        public IList<ExternalConnection> externalConnections;

        public NNSpecification(IList<Neuron> neurons, IList<InterfaceNode> networkIn, IList<InterfaceNode> networkOut, IList<InternalConnection> internalConnections, IList<ExternalConnection> externalConnections) : this(neurons, networkIn, networkOut, new List<Sensor>(0), new List<Actor>(0), internalConnections, externalConnections) { }

        public NNSpecification(IList<Neuron> neurons, IList<InterfaceNode> networkIn, IList<InterfaceNode> networkOut, IList<Sensor> sensors, IList<Actor> actors, IList<InternalConnection> internalConnections, IList<ExternalConnection> externalConnections)
        {
            IEnumerable<NeuralNode> given = neurons.Select(n => (NeuralNode)n).Union(networkIn.Select(n => (NeuralNode)n)).Union(networkOut.Select(n => (NeuralNode)n)).Union(sensors.Select(n => (NeuralNode)n)).Union(actors.Select(n => (NeuralNode)n));
            IEnumerable<NeuralNode> found = internalConnections.SelectMany(c => new NeuralNode[] { c.source, c.destination }).Union(externalConnections.SelectMany(c => new NeuralNode[] { c.source, c.destination }));
            if(given.Where(nn => nn is Neuron).Except(found).Count() > 0)
            {
                throw new ArgumentException("Not used Neuron given");
            }
            if (found.Except(given).Count() > 0) //check for foreigh neurons
            {
                throw new ArgumentException("Non given NeuralNodes found");
            }
            if(internalConnections
                .Select(c => c.source)
                .Where(source => source is InterfaceNode && !(networkIn.Contains((InterfaceNode)source)))
                .Count() > 0)
            {
                throw new ArgumentException();
            }
            if (externalConnections
                .Select(c => c.destination)
                .Where(destination => destination is InterfaceNode && !(networkOut.Contains((InterfaceNode)destination)))
                .Count() > 0)
            {
                throw new ArgumentException();
            }
        }

        public static NNSpecification createEmptyWriteNetwork(JointType jt, IList<InterfaceNode> networkIn)
        {
            return createEmptyWriteNetwork(jt.createActors(), networkIn);
        }

        public static NNSpecification createEmptyWriteNetwork(IList<Actor> actors, IList<InterfaceNode> networkIn)
        {
            if(actors.Count() != networkIn.Count())
            {
                throw new ArgumentException();
            }
            IList<Neuron> neurons = new List<Neuron>(0);
            IList<InterfaceNode> networkOut = new List<InterfaceNode>(0);
            networkIn = new List<InterfaceNode>(networkIn);
            IList<Sensor> sensors = new List<Sensor>(0);
            actors = new List<Actor>(actors);
            IList<InternalConnection> internalConnections = new List<InternalConnection>();
            IList<ExternalConnection> externalConnections = new List<ExternalConnection>();
            for (int i = 0; i < actors.Count(); i++)
            {
                ExternalConnection c = new ExternalConnection(networkIn[i], actors[i]);
                externalConnections.Add(c);
            }
            return new NNSpecification(neurons, networkIn, networkOut, sensors, actors, internalConnections, externalConnections);
        }

        public static NNSpecification createEmptyReadWriteNetwork(JointType jt, IList<InterfaceNode> networkOut, IList<InterfaceNode> networkIn)
        {
            return createEmptyReadWriteNetwork(jt.createSensors(), networkOut, jt.createActors(), networkIn);
        }

        public static NNSpecification createEmptyReadWriteNetwork(IList<Sensor> sensors, IList<InterfaceNode> networkOut, IList<Actor> actors, IList<InterfaceNode> networkIn)
        {
            if (sensors.Count() != networkOut.Count()){ throw new ArgumentException(); }
            if (actors.Count() != networkIn.Count()) { throw new ArgumentException(); }

            IList<Neuron> neurons = new List<Neuron>(0);

            sensors = new List<Sensor>(sensors);
            networkOut = new List<InterfaceNode>(networkOut);
            IList<ExternalConnection> externalConnections = new List<ExternalConnection>();
            for (int i = 0; i < sensors.Count(); i++)
            {
                ExternalConnection c = new ExternalConnection(sensors[i], networkOut[i]); //this specific constructuror is only used here
                externalConnections.Add(c);
            }

            actors = new List<Actor>(actors);
            networkIn = new List<InterfaceNode>(networkIn);
            IList<InternalConnection> internalConnections = new List<InternalConnection>();
            
            for (int i = 0; i < actors.Count(); i++)
            {
                ExternalConnection c = new ExternalConnection(networkIn[i], actors[i]);
                externalConnections.Add(c);
            }
            return new NNSpecification(neurons, networkIn, networkOut, sensors, actors, internalConnections, externalConnections);
        }

        /// <summary>
        /// These function require at least two values
        /// </summary>
        public static readonly Function[] binaryOperators = {Function.MIN, Function.MAX, Function.DEVISION, Function.PRODUCT, Function.SUM,
            Function.GTE, Function.IF, Function.INTERPOLATE, Function.IFSUM, };

        public static bool checkBinaryOperators()
        {
            throw new NotImplementedException();
        }

        public static NNSpecification testBrain1()
        {
            IList<Neuron> neurons = new Neuron[]
            {
            new Neuron(Function.SAW),
            new Neuron(Function.SIN)
            }.ToList();
            IList<InterfaceNode> networkIn = new List<InterfaceNode>();
            IList<InterfaceNode> networkOut = new InterfaceNode[] { new InterfaceNode() }.ToList();
            IList<InternalConnection> internalConnections = new InternalConnection[]
            {
            new InternalConnection(neurons[0], neurons[1], 1.0f)
            }.ToList();
            IList<ExternalConnection> externalConnections = new ExternalConnection[]
            {
            new ExternalConnection(neurons[1], networkOut[0])
            }.ToList();
            return new NNSpecification(neurons, networkIn, networkOut, internalConnections, externalConnections);
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
}
