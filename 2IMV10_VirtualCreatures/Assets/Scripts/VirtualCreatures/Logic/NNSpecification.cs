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

        IList<Connection> connections;

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
        
        internal NNSpecification copy(IDictionary<NeuralSpec, NeuralSpec> copiedNeurons, IDictionary<NeuralSpec, IList<Connection>> copiedConnectionSources)
        {
            Func<NeuralSpec, NeuralSpec> getCopied = n => copiedNeurons[n];
            IList<NeuralSpec> sensors = this.sensors.Select(getCopied).ToList();
            IList<NeuralSpec> neurons = this.neurons.Select(getCopied).ToList();
            IList<NeuralSpec> actors = this.actors.Select(getCopied).ToList();

            IList<Connection> connections = this.connections.Select(oldCon =>
            {
                NeuralSpec source = copiedNeurons[oldCon.source];
                NeuralSpec destination = copiedNeurons[oldCon.destination];
                IList<Connection> candidates;
                if (!copiedConnectionSources.TryGetValue(source, out candidates))
                {
                    //create list on the fly
                    candidates = new List<Connection>();
                    copiedConnectionSources[source] = candidates;
                }
                Connection foundCon = candidates.Where(con => con.source == source).SingleOrDefault();
                if(foundCon == null)
                {
                    //create connections on the fly
                    foundCon = new Connection(source, destination);
                    copiedConnectionSources[source].Add(foundCon);
                }
                return foundCon;
            }).ToList();
            return new NNSpecification(sensors, neurons, actors, connections);
        }

        //getters

        internal IEnumerable<NeuralSpec> getNeuronsOnly()
        {
            return this.neurons;
        }

        public IEnumerable<NeuralSpec> getNeuronsAll()
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
        public IEnumerable<NeuralSpec> getNeuronDestinationCandidates() { return getNeuronsAndActors(); }

        internal NeuralSpec addNewNeuron(NeuronFunc neuronFunc)
        {
            NeuralSpec n = NeuralSpec.createNeuron(neuronFunc);
            this.neurons.Add(n);
            return n;
        }
        
        /// <returns>Total number of sensors+neurons+actors in this network</returns>
        internal int getNumberOfNeurons()
        {
            return this.actors.Count + this.neurons.Count + this.sensors.Count;
        }

        /// <summary>
        /// All possible starting points of an edge
        /// </summary>
        /// <returns></returns>
        public IEnumerable<NeuralSpec> getNeuronsAndSensors()
        {
            return this.sensors.Concat(this.neurons);
        }
        public IEnumerable<NeuralSpec> getNeuronSourceCandidates() { return getNeuronsAndSensors(); }

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

        public IEnumerable<Connection> getInterfacingConnections()
        {
            return getIncommingConnections().Concat(getOutgoingConnections());
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
            return this.getNeuronsAll().Contains(n);
        }

        internal void removeInternalConnection(Connection c)
        {
            if (getInterfacingConnections().Contains(c))
            {
                throw new ApplicationException("Edge could not be removed because it is also in other networks");
            }
            if (!connections.Remove(c))
            {
                throw new ApplicationException("Edge could not be removed because it is not longer in this network");
            }
        }
        
        internal void removeExternalConnection(Connection c, NNSpecification destinationNetwork)
        {
            if (!getInterfacingConnections().Contains(c))
            {
                throw new ApplicationException("Edge could not be removed because it is an internal edge");
            }
            if (!destinationNetwork.getInterfacingConnections().Contains(c))
            {
                throw new ApplicationException("Edge could not be removed because it is an internal edge");
            }
            if (!connections.Remove(c))
            {
                throw new ApplicationException("Edge could not be removed because it is not longer in this network");
            }
            if (!destinationNetwork.connections.Remove(c))
            {
                throw new ApplicationException("Edge could not be removed because it is not longer in the destination network");
            }
        }

        /// <summary>
        /// We move a connection from this network to a new network (only move of source XOR destination allowed)
        /// </summary>
        /// <param name="c"></param>
        /// <param name="newSourceNetwork"></param>
        /// <param name="newDestinationNetwork"></param>
        internal void moveExternalConnection(Connection c, NNSpecification newSourceNetwork, NNSpecification newDestinationNetwork)
        {
            if (this == newSourceNetwork || this == newDestinationNetwork || newSourceNetwork == newDestinationNetwork)
            {
                throw new ApplicationException("We should not move this edge to the same network, or else it was not an external edge");
            }
            if(!connections.Contains(c))
                throw new ArgumentException("Cannot move connection to antoher network, the connection is not found in this network");
            if(Util.DEBUG && !(getNeuronSourceCandidates().Contains(c.source) || getNeuronDestinationCandidates().Contains(c.destination)))
                throw new ApplicationException("Edge is in here but not the source or destination???");
            connections.Remove(c);
            if (!newSourceNetwork.connections.Contains(c))
            {
                // Connection was not in newSourceNetwork
                // We move with a new source
                if (Util.DEBUG) {
                    if (!newDestinationNetwork.connections.Contains(c))
                        throw new ApplicationException(); //dest connection was already set
                    if (!newDestinationNetwork.contains(c.destination))
                        throw new ApplicationException(); //connection.dest was already set
                    if (!newSourceNetwork.contains(c.source))
                        throw new ApplicationException(); //connection.source was already set
                }
                newSourceNetwork.connections.Add(c);
            }
            else if(!newDestinationNetwork.connections.Contains(c))
            {
                // Connection was not in newDestinationnetwork
                if (Util.DEBUG)
                {
                    if (!newSourceNetwork.connections.Contains(c))
                        throw new ApplicationException(); //source connection was already set
                    if (!newSourceNetwork.contains(c.source))
                        throw new ApplicationException(); //connection.source was already set
                    if (!newDestinationNetwork.contains(c.destination))
                        throw new ApplicationException(); //connection.dest was already set
                }
                newDestinationNetwork.connections.Add(c);
            }
            else
            {
                //FIXME: throw new ArgumentException();
            }
        }

        public int getConnectedN(NeuralSpec n)
        {
            if (!neurons.Contains(n)) throw new ApplicationException();
            return getEdgesByDestination(n).Count();
        }

        // modifications

        public Connection addNewLocalConnection(NeuralSpec source, NeuralSpec destination, float weight)
        {
            if(!getNeuronsAndSensors().Contains(source))
                throw new ArgumentException();
            if (!getNeuronsAndActors().Contains(destination))
                throw new ArgumentException();
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

        internal int getNumberOfSourceCandidates()
        {
            return this.sensors.Count + this.neurons.Count;
        }
    }

    public class NeuralSpec
    {
        public readonly String id;
        private static int ID = 0;

        private NeuronType type;
        private enum NeuronType { SENSOR, NEURON, ACTOR };

        private NeuronFunc function;
        
        private NeuralSpec(NeuronType type, NeuronFunc function) {
            this.type = type;
            this.function = function;
            this.id = NeuralSpec.ID.ToString() + type.ToString() + function.ToString();
            NeuralSpec.ID++;
        }

        internal static NeuralSpec createSensor()
        {
            return new NeuralSpec(NeuronType.SENSOR, NeuronFunc.SUM);
        }
        internal static NeuralSpec createNeuron(NeuronFunc function) { return new NeuralSpec(NeuronType.NEURON, function); }
        internal static NeuralSpec createActor() { return new NeuralSpec(NeuronType.ACTOR, NeuronFunc.SUM); }

        public bool isSensor() { return this.type == NeuronType.SENSOR; }
        public bool isNeuron() { return this.type == NeuronType.NEURON; }
        public bool isActor() { return this.type == NeuronType.ACTOR; }

        public virtual NeuronFunc getFunction()
        {
            if (this.type != NeuronType.NEURON)
                throw new ApplicationException("This should never be called on a non neuron node?");
            return this.function;
        }

        public NeuralSpec clone()
        {
            return new NeuralSpec(this.type, this.function);
        }

        public int getMinimalConnections() { return getMinimalConnections(this.function); }
        public int getMaximalConnections() { return getMaximalConnections(this.function); }

        public static int getMinimalConnections(NeuronFunc function)
        {
            switch (function)
            {
                case NeuronFunc.ABS:
                case NeuronFunc.ATAN:
                case NeuronFunc.COS:
                case NeuronFunc.SIGN:
                case NeuronFunc.SIGMOID:
                case NeuronFunc.EXP:
                case NeuronFunc.LOG:
                case NeuronFunc.DIFFERENTIATE:
                case NeuronFunc.INTERGRATE:
                case NeuronFunc.MEMORY:
                case NeuronFunc.SMOOTH:
                case NeuronFunc.SUM:
                    return 1;
                case NeuronFunc.SAW:
                case NeuronFunc.WAVE:
                    return 0;
                case NeuronFunc.MIN:
                case NeuronFunc.MAX:
                    return 2;
                case NeuronFunc.PRODUCT:
                case NeuronFunc.DEVISION:
                    return 2;
                case NeuronFunc.GTE:
                case NeuronFunc.IF:
                case NeuronFunc.INTERPOLATE:
                case NeuronFunc.IFSUM:
                    return 3;
                default:
                    throw new ApplicationException("getMinimalConnections is not setup for " + function);
            }
        }

        public static int getMaximalConnections(NeuronFunc function)
        {
            switch (function)
            {
                case NeuronFunc.ABS:
                case NeuronFunc.ATAN:
                case NeuronFunc.COS:
                case NeuronFunc.SIGN:
                case NeuronFunc.SIGMOID:
                case NeuronFunc.EXP:
                case NeuronFunc.LOG:
                case NeuronFunc.DIFFERENTIATE:
                case NeuronFunc.INTERGRATE:
                case NeuronFunc.MEMORY:
                case NeuronFunc.SMOOTH:
                case NeuronFunc.SUM:
                case NeuronFunc.SAW:
                case NeuronFunc.WAVE:
                case NeuronFunc.MIN:
                case NeuronFunc.MAX:
                case NeuronFunc.PRODUCT:
                    return int.MaxValue;
                case NeuronFunc.DEVISION:
                    return 2;
                case NeuronFunc.GTE:
                case NeuronFunc.IF:
                case NeuronFunc.INTERPOLATE:
                case NeuronFunc.IFSUM:
                    return 3;
                default:
                    throw new ApplicationException("getMaximalConnections is not setup for " + function);
            }
        }

        public override string ToString()
        {
            return "NeuralSpec: " + id;
        }
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

        public override string ToString()
        {
            return "Connection: " + source.id + " -> " +destination.id;
        }
    }

    class DotParser
    {
        public static void write(String filename, IEnumerable<string> contents)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filename));
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename))
            {
                foreach(String line in contents) { file.WriteLine(line); }
            }
        }

        public static List<string> parse(IEnumerable<NNSpecification> networks, NNSpecification brain)
        {
            IEnumerable<NNSpecification> nets = Enumerable.Repeat(brain, 1).Concat(networks);

            // Name all the neurals
            int N = 0;
            IDictionary<NNSpecification, string> netnames = networks.ToDictionary(
                net => net,
                net => (N++).ToString()
                );
            netnames[brain] = "Brain";

            IDictionary<NeuralSpec, string> neuronIDs = nets
                .SelectMany(net => net.getNeuronsAll().Select(neural => new object[] {net, neural}))
                .ToDictionary(
                objs => (NeuralSpec)objs[1],
                objs => "x" + netnames[(NNSpecification)objs[0]] + "x" + ((NeuralSpec)objs[1]).id
                );

            N = 0;
            // List all the connections
            List<string> result = new List<string>();
            result.Add("digraph {");
            foreach (NNSpecification network in nets)
            {
                string name = "cluster_" + N.ToString();
                string label = netnames[network];
                result.Add("    subgraph " + name + " {");
                result.Add("        label=\""+ label + "\";");
                int sensor = 0;
                int actor = 0;
                foreach (NeuralSpec n in network.getNeuronsAll())
                {
                    name = neuronIDs[n];
                    if (n.isActor())
                    {
                        label = "Actor" + (++actor);
                    }else if (n.isSensor())
                    {
                        label = "Sensor" + (++sensor);
                    } else
                    {
                        label = n.getFunction().ToString();
                    }
                    result.Add("        " + name + " [label=\"" + label + "\"];");
                }
                result.Add("    }");
                N++;
            }
            foreach (NNSpecification network in networks)
            {
                foreach(Connection connection in network.getInternalConnections().Concat(network.getOutgoingConnections())) //all internal and outgoing connections)
                {
                    string sourceName = neuronIDs[connection.source];
                    string destName = neuronIDs[connection.destination];
                    result.Add("    " + sourceName + " -> " + destName);
                }
            }
            result.Add("}");
            return result;
        }

        public static void write(IEnumerable<NNSpecification> networks, NNSpecification brain)
        {
            write("network.gv", parse(networks, brain));
        }
    }
}
