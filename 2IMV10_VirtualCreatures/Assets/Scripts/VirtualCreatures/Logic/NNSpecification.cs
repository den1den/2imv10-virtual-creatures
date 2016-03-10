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
        public IList<NeuralSpec> sensors;
        public IList<NeuralSpec> neurons;
        public IList<NeuralSpec> actors;

        public IList<Connection> connections;

        public IList<NeuralSpec> outgoing;
        private static readonly float DEFAULT_WEIGHT = 1.0f;

        /// <summary>
        /// Full neural network specification
        /// </summary>
        /// <param name="sensors"></param>
        /// <param name="neurons"></param>
        /// <param name="actors"></param>
        /// <param name="connections"></param>
        /// <param name="outgoing">Neurons or sensors that are interfaced to the outside</param>
        public NNSpecification(IList<NeuralSpec> sensors, IList<NeuralSpec> neurons, IList<NeuralSpec> actors, IList<Connection> connections, IList<NeuralSpec> outgoing)
        {
            if (outgoing.Except(neurons).Except(actors).Count() > 0)
            {
                throw new ArgumentException("Outgoing nodes should only contain nodes that are of this network");
            }
            IEnumerable<NeuralSpec> allConnected = connections.SelectMany(con => new[] { con.source, con.destination });
            if (allConnected.Except(sensors).Except(neurons).Except(actors).Count() > 0)
            {
                throw new ArgumentException("Connections should only contain nodes that are of this network");
            }
            this.sensors = sensors;
            this.neurons = neurons;
            this.actors = actors;
            this.connections = connections;
            this.outgoing = outgoing;
        }

        /// <summary>
        /// Without actors and sensors
        /// </summary>
        /// <param name="neurons"></param>
        /// <param name="connections"></param>
        /// <param name="outgoing"></param>
        public NNSpecification(IList<NeuralSpec> neurons, IList<Connection> connections, IList<NeuralSpec> outgoing) : this(new List<NeuralSpec>(0), neurons, new List<NeuralSpec>(0), connections, outgoing) { }

        public IEnumerable<NeuralSpec> getAllNeurals()
        {
            return this.sensors
                .Concat(this.neurons)
                .Concat(this.actors);
        }

        public IDictionary<NeuralSpec, NeuralSpec> cloneAllNeurals()
        {
            return this.getAllNeurals().ToDictionary(n => n, n => n.clone());
        }

        public IEnumerable<NeuralSpec> getNeuronsAndActors()
        {
            return this.actors.Concat(this.neurons);
        }

        public IEnumerable<Connection> getEdgesBySource(NeuralSpec source)
        {
            return this.connections.Where(c => c.source == source);
        }

        public IEnumerable<Connection> getEdgesByDestination(NeuralSpec destination)
        {
            return this.connections.Where(c => c.destination == destination);
        }

        public bool contains(NeuralSpec n)
        {
            return this.getAllNeurals().Contains(n);
        }

        public void connectTo(NeuralSpec source, NeuralSpec destination, float weight)
        {
            if (!contains(destination)) throw new ArgumentException();
            this.connections.Add(new Connection(source, destination, weight));
        }

        public void connectTo(NeuralSpec source, IList<NeuralSpec> ns, float weight)
        {
            foreach (NeuralSpec destination in ns)
            {
                connectTo(source, destination, weight);
            }
        }

        public void connectTo(NeuralSpec source, NeuralSpec destination) { connectTo(source, destination, DEFAULT_WEIGHT); }

        public void connectTo(NeuralSpec source, IList<NeuralSpec> ns) { connectTo(source, ns, DEFAULT_WEIGHT); }



        internal static NNSpecification createEmptyNetwork()
        {
            return new NNSpecification(new List<NeuralSpec>(0), new List<NeuralSpec>(0), new List<NeuralSpec>(0), new List<Connection>(0), new List<NeuralSpec>(0));
        }

        public static NNSpecification createEmptyReadNetwork(int dof)
        {
            return createEmptyReadWriteNetwork(dof, 0);
        }

        public static NNSpecification createEmptyWriteNetwork(int dof)
        {
            return createEmptyReadWriteNetwork(0, dof);
        }

        public static NNSpecification createEmptyReadWriteNetwork(int nSensors, int nActors)
        {
            IList<NeuralSpec> sensors = Enumerable.Repeat(nSensors, nSensors).Select(n => NeuralSpec.createSensor()).ToList();
            IList<NeuralSpec> neurons = new List<NeuralSpec>(0);
            IList<NeuralSpec> actors = Enumerable.Repeat(nActors, nActors).Select(n => NeuralSpec.createActor()).ToList();
            IList<Connection> connections = new List<Connection>(0);
            IList<NeuralSpec> outgoing = new List<NeuralSpec>(sensors);
            return new NNSpecification(sensors, neurons, actors, connections, outgoing);
        }

        public static NNSpecification testBrain1()
        {
            NeuralSpec nSAW = NeuralSpec.createNeuron(NeuronFunc.SAW);
            NeuralSpec nSIN = NeuralSpec.createNeuron(NeuronFunc.SIN);
            IList<NeuralSpec> neurons = new NeuralSpec[] { nSAW, nSIN }.ToList();

            Connection c = new Connection(nSAW, nSIN);
            IList<Connection> connections = new Connection[] { c }.ToList();

            IList<NeuralSpec> outgoing = new NeuralSpec[] { nSIN }.ToList();

            return new NNSpecification(neurons, connections, outgoing);
        }

        //IDictionary<NodeSpec, NodeSpec> newSensors = this.sensors.ToDictionary(n => n, n => NodeSpec.createSensor());
        public NNSpecification DeepClone()
        {
            return DeepClone(this.cloneAllNeurals());
        }

        public NNSpecification DeepClone(IDictionary<NeuralSpec, NeuralSpec> newNodes)
        {
            IList<Connection> connections = this.connections.Select(c => new Connection(newNodes[c.source], newNodes[c.destination])).ToList();
            IList<NeuralSpec> outgoing = this.outgoing.Select(n => newNodes[n]).ToList();
            return new NNSpecification(
                this.sensors.Select(n => newNodes[n]).ToList(),
                this.neurons.Select(n => newNodes[n]).ToList(),
                this.sensors.Select(n => newNodes[n]).ToList(),
                connections,
                outgoing
            );
        }

        private static IList<NeuralSpec> deepCopy(IList<NeuralSpec> list)
        {
            return list.Select(ns => ns.clone()).ToList();
        }
    }

    public class NeuralSpec
    {
        private NeuronType type;
        private enum NeuronType { SENSOR, NEURON, ACTOR };

        private NeuronFunc function;

        private NeuralSpec(NeuronType type, NeuronFunc function) { this.type = type; this.function = function; }
        private NeuralSpec(NeuralSpec clone) : this(clone.type, clone.function) { }

        internal static NeuralSpec createSensor() { return new NeuralSpec(NeuronType.SENSOR, NeuronFunc.SUM); }
        internal static NeuralSpec createNeuron(NeuronFunc function) { return new NeuralSpec(NeuronType.NEURON, function); }
        internal static NeuralSpec createActor() { return new NeuralSpec(NeuronType.ACTOR, NeuronFunc.SUM); }

        public bool isSensor() { return this.type == NeuronType.SENSOR; }
        public bool isNeuron() { return this.type == NeuronType.NEURON; }
        public bool isActor() { return this.type == NeuronType.ACTOR; }

        public virtual NeuronFunc getFunction()
        {
            if (this.type != NeuronType.NEURON)
                Debug.Log("This should never be called on a non neuron node?");
            return this.function;
        }

        public NeuralSpec clone()
        {
            return new NeuralSpec(this);
        }

        public bool isSingle()
        {
            return SINGLE.Contains(this.function);
        }
        public bool isTimeDep()
        {
            return TIMEDEP.Contains(this.function);
        }
        public bool isMultile()
        {
            return MULTIPLE.Contains(this.function);
        }
        public bool isDouble()
        {
            return TERTIARE.Contains(this.function);
        }
        public bool isTertiare()
        {
            return TERTIARE.Contains(this.function);
        }
        static readonly NeuronFunc[] SINGLE = new NeuronFunc[] { NeuronFunc.ABS, NeuronFunc.ATAN, NeuronFunc.COS, NeuronFunc.SIGN, NeuronFunc.SIGMOID, NeuronFunc.EXP, NeuronFunc.LOG, NeuronFunc.DIFFERENTIATE, NeuronFunc.INTERGRATE, NeuronFunc.MEMORY, NeuronFunc.SMOOTH };
        static readonly NeuronFunc[] TIMEDEP = new NeuronFunc[] { NeuronFunc.SAW, NeuronFunc.WAVE };
        static readonly NeuronFunc[] MULTIPLE = new NeuronFunc[] { NeuronFunc.MIN, NeuronFunc.MAX, NeuronFunc.SUM, NeuronFunc.PRODUCT };
        static readonly NeuronFunc[] DOUBLE = new NeuronFunc[] { NeuronFunc.DEVISION, };
        static readonly NeuronFunc[] TERTIARE = new NeuronFunc[] { NeuronFunc.GTE, NeuronFunc.IF, NeuronFunc.INTERPOLATE, NeuronFunc.IFSUM };
    }

    /// <summary>
    /// A the different functions that a neuron could have
    /// </summary>
    public enum NeuronFunc
    {
        ABS, ATAN, SIN, COS, SIGN, SIGMOID, EXP, LOG,
        DIFFERENTIATE, INTERGRATE, MEMORY, SMOOTH,
        SAW, WAVE,
        MIN, MAX, SUM, PRODUCT,
        DEVISION,
        GTE, IF, INTERPOLATE, IFSUM
    }

    /// <summary>
    /// Connection between two Neurons
    /// </summary>
    public class Connection
    {
        private float _weight;
        public float weight { get { return this._weight; } set { if (value < MIN_WEIGHT || value > MAX_WEIGHT) throw new ArgumentOutOfRangeException(); this._weight = value; } }

        private NeuralSpec _source;
        public NeuralSpec source
        {
            get { return this._source; }
            set
            {
                if (value.isActor())
                    throw new ArgumentOutOfRangeException();
                this._source = value;
            }
        }

        private NeuralSpec _destination;
        public NeuralSpec destination
        {
            get { return this._destination; }
            set
            {
                if (value.isSensor())
                    throw new ArgumentOutOfRangeException();
                this._destination = value;
            }
        }

        public Connection(NeuralSpec source, NeuralSpec destination) : this(source, destination, 1.0f) { }

        public Connection(NeuralSpec source, NeuralSpec destination, float weight)
        {
            this.source = source;
            this.destination = destination;
            this.weight = weight;
        }

        private static readonly float MIN_WEIGHT = 0;
        private static readonly float MAX_WEIGHT = 1;
    }
}
