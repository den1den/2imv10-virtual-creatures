using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Logic.VirtualCreatures
{
    /// <summary>
    /// The specification of a Neural Network with input and output.
    /// </summary>
    public class NNSpecification
    {
        public IList<InConnection> incommingConnections;
        public IList<OutConnection> outgoingConnections;
        public IList<Connection> internalConnections;
        public IList<Neuron> neurons;

        public NNSpecification(IList<Connection> internalConnections, IList<InConnection> incommingConnections, IList<OutConnection> outgoingConnections)
        {
            //check for nodes that have no use in incomming edges
            if(this.incommingConnections.Select(c => c.destination)
                .Except(this.internalConnections.Select(c => c.source))
                .Except(this.outgoingConnections.Select(c => c.source))
                .Count() > 0)
            {
                throw new ArgumentException();
            }

            IList<Neuron> neurons = internalConnections.SelectMany(c => new Neuron[] { c.source, c.destination })
                .Union(outgoingConnections.Select(c => c.source)).ToList();

            this.incommingConnections = incommingConnections;
            this.outgoingConnections = outgoingConnections;
            this.internalConnections = internalConnections;
            this.neurons = neurons;
            if (!this.isValid())
            {
                throw new ArgumentException("neuron specification within this network is not valid");
            }
        }
        /// <summary>
        /// Create an empty network that simply maps to the input and output of a joint.
        /// </summary>
        /// <param name="joint"></param>
        /// <returns></returns>
        public static NNSpecification createReadWriteNetwork(JointSpecification joint)
        {
            Neuron copy = Neuron.createCopyNeuron();
            IList<Connection> interCon = new List<Connection>();
            IList<OutConnection> outCon = new OutConnection[] { new OutConnection(copy) }.ToList();
            IList<InConnection> inCon = new InConnection[] { new InConnection(copy, 1.0f) }.ToList();
            return new NNSpecification(interCon, inCon, outCon);
        }
        /// <summary>
        /// Create an empty network that simply maps to the output of a joint.
        /// </summary>
        /// <param name="rightJoint"></param>
        /// <returns></returns>
        internal static NNSpecification createWriteOnlyNetwork(JointSpecification rightJoint)
        {
            Neuron copy = Neuron.createCopyNeuron();
            IList<Connection> interCon = new List<Connection>();
            IList<OutConnection> outCon = new List<OutConnection>();
            IList<InConnection> inCon = new InConnection[] { new InConnection(copy, 1.0f) }.ToList();
            return new NNSpecification(interCon, inCon, outCon);
        }

        /// <summary>
        /// A network that genrates a periodical sinus wave at its outputs.
        /// </summary>
        /// <returns></returns>
        internal static NNSpecification test1()
        {
            Neuron n1 = new Neuron(Function.SAW);
            Neuron n2 = new Neuron(Function.SIN);

            IList<Connection> interCon = new Connection[]
            {
                    new Connection(n1, n2, 0.5f)
            }.ToList();
            IList<OutConnection> outCon = new OutConnection[]
            {
                new OutConnection(n2)
            }.ToList();
            IList<InConnection> inCon = new InConnection[]
            {
                new InConnection(n1, 1f)
            }.ToList();

            return new NNSpecification(interCon, inCon, outCon);
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

        /// <summary>
        /// Get the incomming edges of a neuron in this network
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public int getIncomming(Neuron n)
        {
            check(n);
            return this.internalConnections.Where(con => n == con.destination).Count()
                + this.incommingConnections.Where(con => n == con.destination).Count();
        }

        /// <summary>
        /// Get the outgoing edges of a neuron in this network
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public int getOutgoing(Neuron n)
        {
            check(n);
            return this.internalConnections.Where(con => n == con.source).Count()
                + this.outgoingConnections.Where(con => n == con.source).Count();
        }

        /// <summary>
        /// Check the binary restrictions of the neurons in this network
        /// </summary>
        /// <returns>True iff all constraints are met.</returns>
        public Boolean isValid()
        {
            foreach (Neuron n in neurons) {
                if (binaryOperators.Contains(n.function)){
                    if (this.getIncomming(n) < 2)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    /// <summary>
    /// A single neuron in a network.
    /// </summary>
    public class Neuron
    {
        public Function function;

        public Neuron(Function function)
        {
            this.function = function;
        }

        /// <summary>
        /// A neuron that simply copies its input to its output. This should model the behaviour of an empty network.
        /// </summary>
        /// <returns></returns>
        internal static Neuron createCopyNeuron()
        {
            return new Neuron(Function.SUM);
        }
    }
    /// <summary>
    /// An internal connection between two neurons.
    /// </summary>
    public class Connection : InConnection
    {
        public Neuron source;

        public Connection(Neuron source, Neuron destination, float weight) : base(destination, weight)
        {
            this.source = source;
        }
    }
    /// <summary>
    /// A connection from the outside towards on of the neurons in this network.
    /// </summary>
    public class InConnection
    {
        public Neuron destination;
        public float weight;

        public InConnection(Neuron destination, float weight)
        {
            this.destination = destination;
            this.weight = weight;
        }
    }
    /// <summary>
    /// A connection from one of these neurons to outside the network. This could be connected to another neuron or to some force on a joint.
    /// </summary>
    public class OutConnection
    {
        public Neuron source;
        public OutConnection(Neuron source)
        {
            this.source = source;
        }
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

