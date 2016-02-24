using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VirtualCreatures
{
    // We shoud leave this class without any Unity thing related just as abstract as possible.
    class Phenotype
    {
        ExplicitNN nerves;
        static double speed = 1.5; //factor for all dt related functions

        public Phenotype(Morphology morphology, Joint[] joints)
        {
            this.nerves = new NaiveENN(morphology.brain, morphology.edges.Select(e => e.network), joints);
        }

        public void update()
        {
            // Time.deltaTime;
            // Read and write values of Joints once
            this.nerves.tick();
            this.nerves.tick();
        }

    }

    internal class NaiveENN : ExplicitNN
    {
        private readonly List<NeuronSpec> neurons;

        public NaiveENN(NNSpecification main, IEnumerable<NNSpecification> subnetworks, Joint[] joints) : base(joints)
        {
            IEnumerable<NNSpecification> allNetworks = subnetworks.Union(Enumerable.Repeat(main, 1));
            //connect the networks
            IDictionary<InterfaceNode, IEnumerable<NNSpecification>> nnmappings = subnetworks.SelectMany(n => n.networkIn).ToDictionary(intnode => intnode, intn => allNetworks.Where(n => n.networkOut.Contains(intn)));

            //per node collect the weights and create the implemenetation
            IDictionary<SensorSpec, INeuron> created = allNetworks.SelectMany(net => net.getAllNeurals()).ToDictionary(n => n, n => INeuron.create(n));


            foreach (NNSpecification network in allNetworks)
            {
                foreach (NeuronSpec n in network.getNeuronsAndActors())
                {
                    float[] weights = network.getSourceEdges(n).Select(e => e.weight).ToArray();

                }
            }



            //start with actors
            foreach (NNSpecification n in subnetworks)
            {

            }


            //IEnumerable<IConnection> allEdges = subnetworks.SelectMany(n => n.externalConnections.Cast<IConnection>()).Union(subnetworks.SelectMany(n => n.internalConnections.Cast<IConnection>()));
            IDictionary<INeuralNodeSpec, SingleFunction> created = new Dictionary<INeuralNodeSpec, SingleFunction>();

            IList<ActorSpec> actorSpecs = subnetworks.SelectMany(n => n.actors).ToList();
            SingleFunction[] actors = new SingleFunction[actorSpecs.Count];
            for (int i = 0; i < joints.Length; i++)
            {
                ActorSpec a = actorSpecs[i];
                Joint j = joints[i];
                IList<SimpleConnection> theseEdges = subnetworks.SelectMany()

            }


            //So we first remove all the interfaces to simplify the network to a single network without topology
            //first we get the source interfaces
            IEnumerable<InterfaceNode> srcs = main.networkOut.Union(subnetworks.SelectMany(n => n.networkOut));

            //Then we traverse all nodes and for each node we add all the corresponsing edges from a bid edge list.




            //first get all the sensors and starting edges

            IList<SensorSpec> sensors = subnetworks.SelectMany(n => n.sensors).ToList();
            if (sensors.Count != joints.Length) { throw new ArgumentException("sensors and joints do not match"); }
            IEnumerable<IConnection> startIncomming = edges.Where(e => e.source is SensorSpec);

            //IDictionary<VirtualCreatures.Neuron, Neuron> createdN = main.neurons.Union(subnetworks.SelectMany(sn => sn.neurons)).ToDictionary(
            //    (neuronkey => neuronkey),
            //    (neuronspec =>)
            //);

            throw new NotImplementedException();
        }

        internal override void tick()
        {
            //read actors
            //execute the order
            //write the forces
            throw new NotImplementedException();
        }

        internal abstract class INeuron
        {
            float value = float.NaN;
            IList<INeuron> inputs = null;
            internal abstract void tick();

            internal static INeuron create(SensorSpec n)
            {
                if (n is ActorSpec)
                {
                    return new Neuron();
                }
                else if (n is NeuronSpec)
                {

                }
                else
                {

                }
            }
        }

        internal abstract class SingleFunction : INeuron
        {
            public abstract double y(double x);

            internal override void tick()
            {

            }
        }
        internal abstract class SingleFunction { public abstract double y(double x); }
        internal abstract class TimeFunction { public abstract double y(double x, double dtf); }
        internal abstract class DoubleFunction { public abstract double y(double x, double y); }
        internal abstract class TripleFunction { public abstract double y(double x, double y, double z); }
        internal abstract class MultipleFunction { public abstract double y(double[] x); }

        internal class ABS : SingleFunction
        {
            public override double y(double x)
            {
                return Math.Abs(x);
            }
        }
        internal class ATAN : SingleFunction
        {
            static double C = 1.0f / Math.Atan(7);
            static double CX = 7;
            public override double y(double x)
            {
                return Math.Atan(CX * x) * C;
            }
        }
        internal class COS : SingleFunction
        {
            static double CX = 2 * Math.PI;
            public override double y(double x)
            {
                return Math.Cos(CX * x);
            }
        }
        internal class SIN : SingleFunction
        {
            static double CX = 2 * Math.PI;
            public override double y(double x)
            {
                return Math.Sin(CX * x);
            }
        }
        internal class EXP : SingleFunction
        {
            static double C = 1.0 / Math.Exp(2);
            static double CX = 2;
            public override double y(double x)
            {
                return Math.Exp(CX * x) * C;
            }
        }
        internal class LOG : SingleFunction
        {
            static double Base = 2;
            static double C = 0.2;
            public override double y(double x)
            {
                return Math.Max(-1, C * Math.Log(x, Base));
            }
        }
        internal class DIFF : SingleFunction
        {
            double last;
            public DIFF(double initValue) { this.last = initValue; }
            public override double y(double x)
            {
                double x0 = last;
                this.last = x;
                return x - x0;
            }
        }
        internal class INTEGRATE : SingleFunction
        {
            public override double y(double x)
            {
                throw new NotImplementedException();
            }
        }
        internal class MEM : SingleFunction
        {
            static int size = 30;
            double[] vals = new double[size];
            int ptr = 0;
            public override double y(double x)
            {
                vals[ptr] = x;
                ptr = ptr + 1 % vals.Length;
                return vals[ptr];
            }
        }
        internal class SMOOTH : SingleFunction
        {
            static readonly double[] WEIGHTS = new double[] { 1.0, 0.875, 0.75, 0.625, 0.5, 0.375, 0.25, 0.125 };
            double[] vals = new double[WEIGHTS.Length];
            int ptr = 0;
            public override double y(double x)
            {
                vals[ptr] = x;
                ptr = ptr + 1 % vals.Length;
                double r = x * WEIGHTS[0];
                for (int w = 1; w < WEIGHTS.Length; w++) { r += WEIGHTS[w] * vals[(ptr + w) % vals.Length]; }
                return r;
            }
        }
        internal class SAW : TimeFunction
        {
            double theta = 0;
            public override double y(double x, double dtf)
            {
                theta += x * dtf;
                return 2 * (theta % 1.0) - 1;
            }
        }
        internal class WAVE : TimeFunction
        {
            static double CX = Math.PI * 2;
            double theta = 0;
            public override double y(double x, double dtf)
            {
                theta += x * dtf;
                return Math.Cos(CX * theta);
            }
        }
        internal class SIGMOID : SingleFunction
        {
            static double CX = -5;
            double bias = 0;
            public override double y(double x)
            {
                return 1.0 / (1 + Math.Exp(CX * (x - this.bias)));
            }
        }
        internal class SIGN : SingleFunction
        {
            public override double y(double x)
            {
                if (x > 0)
                {
                    return 1;
                }
                return -1;
            }
        }


        internal class MIN : MultipleFunction
        {
            public override double y(double[] x)
            {
                double min = x[0];
                for (int i = 1; i < x.Length; i++)
                {
                    if (x[i] < min)
                    {
                        min = x[i];
                    }
                }
                return min;
            }
        }
        internal class MAX : MultipleFunction
        {
            public override double y(double[] x)
            {
                double max = x[0];
                for (int i = 1; i < x.Length; i++)
                {
                    if (x[i] > max)
                    {
                        max = x[i];
                    }
                }
                return max;
            }
        }
        internal class DEVISION : DoubleFunction
        {
            static double limit = 0.01;
            public override double y(double x, double y)
            {
                if (x < limit && x > -limit)
                {
                    if (x < 0) { x = -limit; } else { x = limit; }
                }
                return y / x;
            }
        }
        internal class PRODUCT : DoubleFunction
        {
            public override double y(double x, double y)
            {
                return x * y;
            }
        }
        internal class SUM : DoubleFunction
        {
            public override double y(double x, double y)
            {
                return x + y;
            }
        }
        internal class GTE : TripleFunction
        {
            public override double y(double x, double y, double z)
            {
                if (x > y)
                {
                    return z;
                }
                else return -z;
            }
        }
        internal class IF : TripleFunction
        {
            public override double y(double x, double y, double z)
            {
                if (x > 0)
                {
                    return y;
                }
                else return -z;
            }
        }
        internal class INTERPOLATE : TripleFunction
        {
            public override double y(double x, double y, double z)
            {
                double w = (z + 1) / 2;
                return x * w + y * (1 - w);
            }
        }
        internal class IFSUM : TripleFunction
        {
            public override double y(double x, double y, double z)
            {
                if (x + y > 0)
                {
                    return 1;
                }
                else return -1;
            }
        }
    }

    internal abstract class ExplicitNN
    {
        Joint[] joints;
        internal ExplicitNN(Joint[] joints)
        {
            this.joints = joints;
        }

        internal abstract void tick();
    }
}
