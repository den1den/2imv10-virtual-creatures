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

        NNSpecification() : this(new List<NeuralSpec>(0), new List<Connection>(0)) { }

        public void checkInvariants()
        {
            foreach (Connection c in connections)
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
            IList<NeuralSpec> newSensors = this.sensors.Select(oldS => copiedNeurons[oldS]).ToList();
            IList<NeuralSpec> newNeurons = this.neurons.Select(oldN => copiedNeurons[oldN]).ToList();
            IList<NeuralSpec> newActors = this.actors.Select(oldA => copiedNeurons[oldA]).ToList();
            IList<Connection> newConnections = new List<Connection>(this.connections.Count);
            for(int i = 0; i < this.connections.Count; i++)
            {
                Connection oldCon = this.connections[i];
                float oldWeight = oldCon.weight;
                NeuralSpec newSource = copiedNeurons[oldCon.source];
                NeuralSpec newDestination = copiedNeurons[oldCon.destination];
                Connection newCon;

                // This connection could have already been created in antoher network
                IList<Connection> candidates;
                if (copiedConnectionSources.TryGetValue(newSource, out candidates))
                {
                    newCon = candidates.Where(con => con.source == newSource && con.destination == newDestination).SingleOrDefault();
                    if (newCon == null)
                    {
                        // This connection was not already created
                        newCon = new Connection(newSource, newDestination, oldWeight);
                        candidates.Add(newCon);
                    }
                    else if (newCon.weight != oldWeight) throw new ApplicationException();
                }
                else
                {
                    // No connection with newSource has been created
                    newCon = new Connection(newSource, newDestination, oldWeight);
                    candidates = new List<Connection>();
                    candidates.Add(newCon);
                    copiedConnectionSources[newSource] = candidates;
                }

                //Update the connection
                newConnections.Add(newCon);
            }
            return new NNSpecification(newSensors, newNeurons, newActors, newConnections);
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
            if (Util.DEBUG)
                debugCheckConnection(c, this);
            if (!connections.Remove(c))
            {
                throw new ApplicationException("Edge could not be removed because it is not longer in this network");
            }
        }

        internal void removeExternalConnection(Connection c, NNSpecification destinationNetwork)
        {
            if(Util.DEBUG)
            {
                debugCheckConnection(c, this, destinationNetwork);
            }
            if (!connections.Remove(c))
            {
                throw new ApplicationException("Edge could not be removed because it was not longer in this network");
            }
            if (!destinationNetwork.connections.Remove(c))
            {
                throw new ApplicationException("Edge could not be removed because it was not longer in the destination network");
            }
        }

        /// <summary>
        /// Move a source of an external connection
        /// </summary>
        /// <param name="connection">A connection from oldSourceNetwork to this</param>
        /// <param name="newSourceNetwork"></param>
        /// <param name="newSource"></param>
        /// <param name="oldSourceNetwork"></param>
        internal void moveExternalConnectionItsSource(Connection connection, NNSpecification newSourceNetwork, NeuralSpec newSource, NNSpecification oldSourceNetwork)
        {
            if (Util.DEBUG)
            {
                // Check if connection is a connection from oldSourceNetwork to this
                debugCheckConnection(connection, oldSourceNetwork, this);
                // Check if the new source is valid
                if (!newSourceNetwork.contains(newSource))
                    throw new ApplicationException("newSource is not in newSourceNetwork");
                if (newSourceNetwork == this)
                    throw new ApplicationException("We should not move this edge to the same network, or else it was not an external edge");
                if (newSourceNetwork.connections.Contains(connection))
                    throw new ApplicationException("newSourceNetwork already contained this connection!?");
                // Check if it can be moved
                if (connection.source == newSource || connection.destination == newSource)
                    throw new ApplicationException("Connection's newSource is not valid!?");
            }
            if (!oldSourceNetwork.connections.Remove(connection))
            {
                throw new ApplicationException("Removing connection while connection was not in oldSourceNetwork");
            }
            connection.source = newSource;
            newSourceNetwork.connections.Add(connection);
        }

        /// <summary>
        /// Move a destination of an external connection
        /// </summary>
        /// <param name="connection">A connection from this to oldDestinationNetwork</param>
        /// <param name="newDestinationNetwork"></param>
        /// <param name="newDestination"></param>
        /// <param name="oldDestinationNetwork"></param>
        internal void moveExternalConnectionItsDestination(Connection connection, NNSpecification newDestinationNetwork, NeuralSpec newDestination, NNSpecification oldDestinationNetwork)
        {
            if (Util.DEBUG)
            {
                // Check if connection is a connection from this to oldDestinationNetwork
                debugCheckConnection(connection, this, oldDestinationNetwork);
                // Check if the new destination is valid
                if (!newDestinationNetwork.contains(newDestination))
                    throw new ApplicationException("newDestination is not in newDestinationNetwork");
                if (newDestinationNetwork == this)
                    throw new ApplicationException("We should not move this edge to the same network, or else it was not an external edge");
                if (newDestinationNetwork.connections.Contains(connection))
                    throw new ApplicationException("newDestinationNetwork already contained this connection!?");
                // Check if it can be moved
                if (connection.source == newDestination || connection.destination == newDestination)
                    throw new ApplicationException("Connection's newDestination is not valid!?");
            }
            if (!oldDestinationNetwork.connections.Remove(connection))
            {
                throw new ApplicationException("Removing connection while connection was not in oldDestinationNetwork");
            }
            connection.destination = newDestination;
            newDestinationNetwork.connections.Add(connection);
        }

        public int getConnectedN(NeuralSpec n)
        {
            if (!neurons.Contains(n)) throw new ApplicationException();
            return getEdgesByDestination(n).Count();
        }

        // modifications

        public Connection addNewLocalConnection(NeuralSpec source, NeuralSpec destination, float weight)
        {
            if (!getNeuronsAndSensors().Contains(source))
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
            foreach (NeuralSpec actor in destinationNetwork.actors)
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

        public static void debugCheckConnection(Connection connection, NNSpecification source, NNSpecification destination)
        {
            if (!source.connections.Contains(connection))
                throw new ApplicationException("Expected connection is source network");
            if (!source.getNeuronSourceCandidates().Contains(connection.source))
                throw new ApplicationException("Expected connection.source is source network");
            if (!destination.connections.Contains(connection))
                throw new ApplicationException("Expected connection is destination network");
            if (!destination.getNeuronDestinationCandidates().Contains(connection.destination))
                throw new ApplicationException("Expected connection.destination is destination network");
        }

        public static void debugCheckConnection(Connection connection, NNSpecification destinationAndSource)
        {
            if (!destinationAndSource.connections.Contains(connection))
                throw new ApplicationException("Expected connection is not in the network");
            if (!destinationAndSource.getNeuronSourceCandidates().Contains(connection.source))
                throw new ApplicationException("Expected connection.source is not in the network");
            if (!destinationAndSource.getNeuronDestinationCandidates().Contains(connection.destination))
                throw new ApplicationException("Expected connection.destination is not in the network");
        }
    }

    public class NeuralSpec
    {
        public readonly String id;
        private static int ID = 0;

        public NeuronType type;
        public enum NeuronType { SENSOR, NEURON, ACTOR };

        public NeuronFunc function;

        private NeuralSpec(NeuronType type, NeuronFunc function)
        {
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
            return "Connection: " + source.id + " -> " + destination.id;
        }
    }

    class DotParser
    {
        public static List<string> parse(Morphology m)
        {
            IEnumerable<NNSpecification> networks = Enumerable.Repeat(m.brain, 1).Concat(m.edges.Select(e => e.network));

            // Name all the neurals
            int N = 0;
            IDictionary<NNSpecification, string> netnames = networks.ToDictionary(
                net => net,
                net => (N++).ToString()
                );
            netnames[m.brain] = "Brain";

            IDictionary<NeuralSpec, string> neuronIDs = networks
                .SelectMany(net => net.getNeuronsAll().Select(neural => new object[] { net, neural }))
                .ToDictionary(
                objs => (NeuralSpec)objs[1],
                objs => "x" + netnames[(NNSpecification)objs[0]] + "x" + ((NeuralSpec)objs[1]).id
                );

            N = 0;
            // List all the connections
            List<string> result = new List<string>();
            result.Add("digraph {");
            foreach (NNSpecification network in networks)
            {
                string name = "cluster_" + N.ToString();
                string label = netnames[network];
                result.Add("    subgraph " + name + " {");
                result.Add("        label=\"" + label + "\";");
                int sensor = 0;
                int actor = 0;
                foreach (NeuralSpec n in network.getNeuronsAll())
                {
                    name = neuronIDs[n];
                    if (n.isActor())
                    {
                        label = "Actor" + (++actor);
                    }
                    else if (n.isSensor())
                    {
                        label = "Sensor" + (++sensor);
                    }
                    else
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
                foreach (Connection connection in network.getInternalConnections().Concat(network.getOutgoingConnections())) //all internal and outgoing connections)
                {
                    string sourceName = neuronIDs[connection.source];
                    string destName = neuronIDs[connection.destination];
                    result.Add("    " + sourceName + " -> " + destName);
                }
            }
            result.Add("}");
            return result;
        }
    }
}
