using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Logic.Genotype
{
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

        private void checkDummies()
        {
            throw new NotImplementedException();
        }

        public static readonly Function[] binaryOperators = {Function.MIN, Function.MAX, Function.DEVISION, Function.PRODUCT, Function.SUM,
            Function.GTE, Function.IF, Function.INTERPOLATE, Function.IFSUM, };

        private void check(Neuron n)
        {
            if (!neurons.Contains(n))
            {
                throw new ArgumentException();
            }
        }

        public int getIncomming(Neuron n)
        {
            check(n);
            return this.internalConnections.Where(con => n == con.destination).Count()
                + this.incommingConnections.Where(con => n == con.destination).Count();
        }

        public int getOutgoing(Neuron n)
        {
            check(n);
            return this.internalConnections.Where(con => n == con.source).Count()
                + this.outgoingConnections.Where(con => n == con.source).Count();
        }

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

    public class Neuron
    {
        public Function function;

        public Neuron(Function function)
        {
            this.function = function;
        }
    }

    public class Connection : InConnection
    {
        public Neuron source;

        public Connection(Neuron source, Neuron destination, float weight) : base(destination, weight)
        {
            this.source = source;
        }
    }

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

    public class OutConnection
    {
        public Neuron source;
        public OutConnection(Neuron source)
        {
            this.source = source;
        }
    }

    public enum Function {
        ABS, ATAN, SIN, COS, EXP, LOG,
        DIFFERENTIATE, INTERGRATE, MEMORY, SMOOTH,
        SAW, WAVE,
        SIGMOID, SIGN,
        MIN, MAX, DEVISION, PRODUCT, SUM, GTE,
        IF, INTERPOLATE, IFSUM
    }
}

