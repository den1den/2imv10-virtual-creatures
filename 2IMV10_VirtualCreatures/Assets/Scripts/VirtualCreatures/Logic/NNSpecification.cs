﻿using System;
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

        private IList<Connection> connections;

        /// <summary>
        /// Full neural network specification
        /// </summary>
        /// <param name="sensors"></param>
        /// <param name="neurons"></param>
        /// <param name="actors"></param>
        /// <param name="connections"></param>
        NNSpecification(IList<NeuralSpec> sensors, IList<NeuralSpec> neurons, IList<NeuralSpec> actors, IList<Connection> connections)
        {
            this.sensors = sensors;
            this.neurons = neurons;
            this.actors = actors;
            this.connections = connections;
        }

        NNSpecification(IList<NeuralSpec> neurons, IList<Connection> connections) : this(new List<NeuralSpec>(0), neurons, new List<NeuralSpec>(0), connections) { }

        NNSpecification() : this(new List<NeuralSpec>(0), new List<Connection>(0)){}
        
        public void checkInvariants()
        {
            foreach(Connection c in connections)
            {
                if (c.source.isSensor() && c.destination.isActor()) throw new ApplicationException("no direct connections allowed between sensors and actors");
            }

            if (sensors.Intersect(actors).Count() > 0) throw new ApplicationException("sensors and actors should be disjoint sets");
            if (neurons.Intersect(sensors).Count() > 0) throw new ApplicationException("neurons and sensors should be disjoint sets");
            if (neurons.Intersect(actors).Count() > 0) throw new ApplicationException("neurons and actors should be disjoint sets");

            //redundant or invalid?
            if (getIncommingConnections().Intersect(getOutgoingConnections()).Count() > 0) throw new ApplicationException("connection that did not hit anything in this network");
        }

        internal static NNSpecification createEmptyNetwork()
        {
            return new NNSpecification();
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
            return new NNSpecification(sensors, neurons, actors, connections);
        }
        
        internal NNSpecification copy(IDictionary<NeuralSpec, NeuralSpec> copiedNeurons)
        {
            Func<NeuralSpec, NeuralSpec> getCopied = n =>
            {
                NeuralSpec copied = copiedNeurons[n];
                if (copied == null) throw new ApplicationException();
                return copied;
            };
            IList<NeuralSpec> sensors = this.sensors.Select(getCopied).ToList();
            IList<NeuralSpec> neurons = this.neurons.Select(getCopied).ToList();
            IList<NeuralSpec> actors = this.actors.Select(getCopied).ToList();
            IList<Connection> connections = this.connections.Select(con => new Connection(copiedNeurons[con.source], copiedNeurons[con.destination])).ToList();
            return new NNSpecification(sensors, neurons, actors, connections);
        }

        //getters

        public IEnumerable<NeuralSpec> getAllNeurals()
        {
            return this.sensors
                .Concat(this.neurons)
                .Concat(this.actors);
        }

        /// <summary>
        /// All possible endpoints of an edge
        /// </summary>
        /// <returns></returns>
        public IEnumerable<NeuralSpec> getNeuronsAndActors()
        {
            return this.actors.Concat(this.neurons);
        }

        /// <summary>
        /// All possible starting points of an edge
        /// </summary>
        /// <returns></returns>
        public IEnumerable<NeuralSpec> getNeuronsAndSensors()
        {
            return this.sensors.Concat(this.neurons);
        }

        public IEnumerable<Connection> getInternalConnections()
        {
            return this.connections.Where(c => getNeuronsAndSensors().Contains(c.source) && getNeuronsAndActors().Contains(c.destination));
        }

        public IEnumerable<Connection> getOutgoingConnections()
        {
            return this.connections.Where(c => getNeuronsAndSensors().Contains(c.source) && !getNeuronsAndActors().Contains(c.destination));
        }

        public IEnumerable<Connection> getIncommingConnections()
        {
            return this.connections.Where(c => !getNeuronsAndSensors().Contains(c.source) && getNeuronsAndActors().Contains(c.destination));
        }

        /// <summary>
        /// Get all edges connected to source
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public IEnumerable<Connection> getEdgesBySource(NeuralSpec source)
        {
            return this.connections.Where(c => c.source == source);
        }

        /// <summary>
        /// Get all edges connected to destination
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        public IEnumerable<Connection> getEdgesByDestination(NeuralSpec destination)
        {
            return this.connections.Where(c => c.destination == destination);
        }

        /// <summary>
        /// Check if n is in this network
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool contains(NeuralSpec n)
        {
            return this.getAllNeurals().Contains(n);
        }

        // modifications
        
        public Connection addNewLocalConnection(NeuralSpec source, NeuralSpec destination, float weight)
        {
            if(!getNeuronsAndSensors().Contains(source)) throw new ArgumentException();
            if (!getNeuronsAndActors().Contains(destination)) throw new ArgumentException();
            Connection c = new Connection(source, destination, weight);
            this.connections.Add(c);
            return c;
        }

        public Connection addNewInterConnection(NeuralSpec source, NeuralSpec destination, NNSpecification destinationNetwork, float weight)
        {
            if (!getNeuronsAndSensors().Contains(source)) throw new ArgumentException();
            if (!destinationNetwork.getNeuronsAndActors().Contains(destination)) throw new ArgumentException();
            Connection c = new Connection(source, destination, weight);
            this.connections.Add(c);
            destinationNetwork.connections.Add(c);
            return c;
        }
        
        public void addNewInterConnectionToActors(NeuralSpec source, NNSpecification destinationNetwork)
        {
            foreach(NeuralSpec actor in destinationNetwork.actors)
            {
                addNewInterConnection(source, actor, destinationNetwork, 1);
            }
        }

        /// <summary>
        /// Create a neural network with a sinus wave output at the second neuron
        /// </summary>
        /// <returns>NNSpecification such that r.neurons[1] is a sinusal wave</returns>
        public static NNSpecification testBrain1()
        {
            NeuralSpec nSAW = NeuralSpec.createNeuron(NeuronFunc.SAW);
            NeuralSpec nSIN = NeuralSpec.createNeuron(NeuronFunc.SIN);
            IList<NeuralSpec> neurons = new NeuralSpec[] { nSAW, nSIN }.ToList();

            Connection c = new Connection(nSAW, nSIN);
            IList<Connection> connections = new Connection[] { c }.ToList();

            return new NNSpecification(neurons, connections);
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

        internal NeuralSpec copy()
        {
            return new NeuralSpec(this);
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

        internal Connection(NeuralSpec source, NeuralSpec destination) : this(source, destination, MAX_WEIGHT) { }

        internal Connection(NeuralSpec source, NeuralSpec destination, float weight)
        {
            this.source = source;
            this.destination = destination;
            this.weight = weight;
        }

        private static readonly float MIN_WEIGHT = 0;
        private static readonly float MAX_WEIGHT = 1;
    }
}
