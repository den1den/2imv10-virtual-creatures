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
        public IList<NeuronSpec> neurons;
        public IList<InterfaceNode> networkIn;
        public IList<InterfaceNode> networkOut;
        public IList<SensorSpec> sensors; //can be made integers
        public IList<ActorSpec> actors; //can be made integers

        public IList<WeightConnection> internalConnections;
        public IList<SimpleConnection> externalConnections;

        public NNSpecification(IList<NeuronSpec> neurons, IList<InterfaceNode> networkIn, IList<InterfaceNode> networkOut, IList<WeightConnection> internalConnections, IList<SimpleConnection> externalConnections) : this(neurons, networkIn, networkOut, new List<SensorSpec>(0), new List<ActorSpec>(0), internalConnections, externalConnections) { }

        public NNSpecification(IList<NeuronSpec> neurons, IList<InterfaceNode> networkIn, IList<InterfaceNode> networkOut, IList<SensorSpec> sensors, IList<ActorSpec> actors, IList<WeightConnection> internalConnections, IList<SimpleConnection> externalConnections)
        {
            IEnumerable<InterfaceNode> given = neurons.Select(n => (InterfaceNode)n).Union(networkIn.Select(n => (InterfaceNode)n)).Union(networkOut.Select(n => (InterfaceNode)n)).Union(sensors.Select(n => (InterfaceNode)n)).Union(actors.Select(n => (InterfaceNode)n));
            IEnumerable<InterfaceNode> found = internalConnections.SelectMany(c => new InterfaceNode[] { c.source, c.destination }).Union(externalConnections.SelectMany(c => new InterfaceNode[] { c.source, c.destination }));
            if(given.Where(nn => nn.isNeuron()).Except(found).Count() > 0)
            {
                throw new ArgumentException("Not used Neuron given");
            }
            if (found.Except(given).Count() > 0) //check for foreigh neurons
            {
                throw new ArgumentException("Non given NeuralNodes found");
            }
            if(internalConnections
                .Select(c => c.source)
                .Where(source => source.isInterface() && !(networkIn.Contains((InterfaceNode)source)))
                .Count() > 0)
            {
                throw new ArgumentException(); //foreigh?
            }
            if (externalConnections
                .Select(c => c.destination)
                .Where(destination => destination.isInterface() && !(networkOut.Contains((InterfaceNode)destination)))
                .Count() > 0)
            {
                throw new ArgumentException(); //foreigh?
            }
            foreach(InterfaceNode outInterface in networkOut)
            {
                //each outinterface should be connected to exactly one neuron
                if(externalConnections.Select(c => c.destination).Where(dest => dest == outInterface).Count() != 1) { throw new ArgumentException(); }
            }
            this.neurons = neurons;
            this.networkIn = networkIn;
            this.networkOut = networkOut;
            this.sensors = sensors;
            this.actors = actors;
            this.internalConnections = internalConnections;
            this.externalConnections = externalConnections;
        }

        public IEnumerable<SensorSpec> getAllNeurals()
        {
            return this.sensors.Union(
                this.neurons.Cast<SensorSpec>()).Union(
                this.actors.Cast<SensorSpec>());
        }

        public IEnumerable<NeuronSpec> getNeuronsAndActors()
        {
            return this.neurons.Union(this.actors.Cast<NeuronSpec>());
        }

        public IEnumerable<WeightConnection> getSourceEdges(NeuronSpec neuronOrActor)
        {
            return this.internalConnections.Where(c => c.destination == neuronOrActor);
        }

        internal static NNSpecification createEmptyNetwork()
        {
            return new NNSpecification(new List<NeuronSpec>(0), new List<InterfaceNode>(0), new List<InterfaceNode>(0), new List<WeightConnection>(0), new List<SimpleConnection>(0));
        }

        public IEnumerable<SimpleConnection> getSourceEdges(InterfaceNode i)
        {
            return this.externalConnections.Where(c => c.destination == i);
        }

        public static NNSpecification createEmptyWriteNetwork(JointType jt, IList<InterfaceNode> networkIn)
        {
            return createEmptyWriteNetwork(jt.createActors(), networkIn);
        }

        public static NNSpecification createEmptyWriteNetwork(IList<ActorSpec> actors, IList<InterfaceNode> networkIn)
        {
            if(actors.Count() != networkIn.Count())
            {
                throw new ArgumentException();
            }
            IList<NeuronSpec> neurons = new List<NeuronSpec>(0);
            IList<InterfaceNode> networkOut = new List<InterfaceNode>(0);
            networkIn = new List<InterfaceNode>(networkIn);
            IList<SensorSpec> sensors = new List<SensorSpec>(0);
            actors = new List<ActorSpec>(actors);
            IList<WeightConnection> internalConnections = new List<WeightConnection>();
            IList<SimpleConnection> externalConnections = new List<SimpleConnection>();
            for (int i = 0; i < actors.Count(); i++)
            {
                WeightConnection c = new WeightConnection(networkIn[i], actors[i], 1);
                internalConnections.Add(c);
            }
            return new NNSpecification(neurons, networkIn, networkOut, sensors, actors, internalConnections, externalConnections);
        }

        public static NNSpecification createEmptyReadWriteNetwork(JointType jt, IList<InterfaceNode> networkOut, IList<InterfaceNode> networkIn)
        {
            return createEmptyReadWriteNetwork(jt.createSensors(), networkOut, jt.createActors(), networkIn);
        }

        public static NNSpecification createEmptyReadWriteNetwork(IList<SensorSpec> sensors, IList<InterfaceNode> networkOut, IList<ActorSpec> actors, IList<InterfaceNode> networkIn)
        {
            if (sensors.Count() != networkOut.Count()){ throw new ArgumentException(); }
            if (actors.Count() != networkIn.Count()) { throw new ArgumentException(); }

            IList<NeuronSpec> neurons = new List<NeuronSpec>(0);

            sensors = new List<SensorSpec>(sensors);
            networkOut = new List<InterfaceNode>(networkOut);
            IList<SimpleConnection> externalConnections = new List<SimpleConnection>();
            for (int i = 0; i < sensors.Count(); i++)
            {
                SimpleConnection c = SimpleConnection.createSimpleEmptyConnection(sensors[i], networkOut[i]); //this specific constructuror is only used here
                externalConnections.Add(c);
            }

            actors = new List<ActorSpec>(actors);
            networkIn = new List<InterfaceNode>(networkIn);
            IList<WeightConnection> internalConnections = new List<WeightConnection>();
            
            for (int i = 0; i < actors.Count(); i++)
            {
                WeightConnection c = new WeightConnection(networkIn[i], actors[i], 1);
                internalConnections.Add(c);
            }
            return new NNSpecification(neurons, networkIn, networkOut, sensors, actors, internalConnections, externalConnections);
        }

        public static NNSpecification testBrain1()
        {
            IList<NeuronSpec> neurons = new NeuronSpec[]
            {
            new NeuronSpec(NeuronSpec.NFunc.SAW),
            new NeuronSpec(NeuronSpec.NFunc.SIN)
            }.ToList();
            IList<InterfaceNode> networkIn = new List<InterfaceNode>();
            IList<InterfaceNode> networkOut = new InterfaceNode[] { new InterfaceNode() }.ToList();
            IList<WeightConnection> internalConnections = new WeightConnection[]
            {
            new WeightConnection(neurons[0], neurons[1], 1.0f)
            }.ToList();
            IList<SimpleConnection> externalConnections = new SimpleConnection[]
            {
            new SimpleConnection(neurons[1], networkOut[0])
            }.ToList();
            return new NNSpecification(neurons, networkIn, networkOut, internalConnections, externalConnections);
        }
    }

    

    /// <summary>
    /// A node that is in the interface of the network
    /// </summary>
    public class InterfaceNode
    {
        internal InterfaceNode() : this(NeuronType.INTERFACE) { }
        protected InterfaceNode(NeuronType type) { this.type = type; }
        protected enum NeuronType{ INTERFACE, SENSOR, NEURON, ACTOR }
        private readonly NeuronType type;
        public bool isInterface() { return this.type == NeuronType.INTERFACE; }
        public bool isSensor() { return this.type == NeuronType.SENSOR; }
        public bool isNeuron() { return this.type == NeuronType.NEURON; }
        public bool isActor() { return this.type == NeuronType.ACTOR; }
    }

    /// <summary>
    /// A sensor of this network
    /// </summary>
    public class SensorSpec : InterfaceNode
    {
        internal SensorSpec() : this(NeuronType.SENSOR) { }
        protected SensorSpec(NeuronType type) : base(type) { }
    }

    /// <summary>
    /// An actor of this network
    /// </summary>
    public class ActorSpec : NeuronSpec
    {
        internal ActorSpec() : base(NeuronType.ACTOR, NFunc.SUM) { }
    }

    /// <summary>
    /// A single neuron in a network.
    /// </summary>
    public class NeuronSpec : SensorSpec
    {
        public NFunc function;
        internal NeuronSpec(NFunc function) : this(NeuronType.NEURON, function) { }
        protected NeuronSpec(NeuronType type, NFunc function) : base(type) { this.function = function; }

        /// <summary>
        /// A the different functions that a neuron could have
        /// </summary>
        public enum NFunc
        {
            ABS, ATAN, SIN, COS, SIGN, SIGMOID, EXP, LOG,
            DIFFERENTIATE, INTERGRATE, MEMORY, SMOOTH,
            SAW, WAVE,
            MIN, MAX, SUM, PRODUCT,
            DEVISION,  
            GTE, IF, INTERPOLATE, IFSUM
        }

        static NFunc[] SINGLE = new NFunc[] { NFunc.ABS, NFunc.ATAN, NFunc.COS, NFunc.SIGN, NFunc.SIGMOID, NFunc.EXP, NFunc.LOG, NFunc.DIFFERENTIATE, NFunc.INTERGRATE, NFunc.MEMORY, NFunc.SMOOTH };
        bool isSingle()
        {
            return SINGLE.Contains(this.function);
        }

        static NFunc[] TIMEDEP = new NFunc[] { NFunc.SAW, NFunc.WAVE };
        bool isTimeDep()
        {
            return TIMEDEP.Contains(this.function);
        }

        static NFunc[] MULTIPLE = new NFunc[] { NFunc.MIN, NFunc.MAX, NFunc.SUM, NFunc.PRODUCT };
        bool isMultile()
        {
            return MULTIPLE.Contains(this.function);
        }

        static NFunc[] DOUBLE = new NFunc[] { NFunc.DEVISION,  };
        bool isDouble()
        {
            return TERTIARE.Contains(this.function);
        }

        static NFunc[] TERTIARE = new NFunc[] { NFunc.GTE, NFunc.IF, NFunc.INTERPOLATE, NFunc.IFSUM };
        bool isTertiare()
        {
            return TERTIARE.Contains(this.function);
        }
    }

    public class IConnection
    {
        internal InterfaceNode source;
        internal InterfaceNode destination;

        protected IConnection(InterfaceNode source, InterfaceNode destination)
        {
            this.source = source;
            this.destination = destination;
        }
    }

    public class IWeightConnection : IConnection
    {
        public float weight;
        protected IWeightConnection(InterfaceNode source, NeuronSpec destination, float weight) : base(source, destination)
        {
            this.weight = weight;
        }
    }

    /// <summary>
    /// A connection that goes to an internal Neuron
    /// </summary>
    public class WeightConnection : IWeightConnection
    {
        public WeightConnection(InterfaceNode source, ActorSpec destination, float weight) : base(source, destination, weight) { }
        public WeightConnection(NeuronSpec source, ActorSpec destination, float weight) : base(source, destination, weight) { }
        public WeightConnection(SensorSpec source, NeuronSpec destination, float weight) : base(source, destination, weight) { }
    }

    /// <summary>
    /// An connection that goes towards a Interface or Actor
    /// </summary>
    public class SimpleConnection : IConnection
    {
        public SimpleConnection(NeuronSpec source, InterfaceNode destination) : base(source, destination) { }
        
        /// <summary>
        /// Only for usage by an Empty network!!!
        /// </summary>
        internal static SimpleConnection createSimpleEmptyConnection(SensorSpec source, InterfaceNode destination) { return new SimpleConnection(source, destination); }
        private SimpleConnection(SensorSpec source, InterfaceNode destination) : base(source, destination) { }
    }
    
    
}
