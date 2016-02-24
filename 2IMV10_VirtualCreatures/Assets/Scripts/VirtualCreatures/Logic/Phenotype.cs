﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VirtualCreatures
{
    // We shoud leave this class without any Unity thing related just as abstract as possible.
    class Phenotype
    {
        NaiveENN nerves;
        static double speed = 1.5; //factor for all dt related functions

        public Phenotype(Morphology morphology, Joint[] joints)
        {
            this.nerves = new NaiveENN(morphology.brain, morphology.edges.Select(e => e.network), joints);
        }

        public void update(float dt)
        {
            // Time.deltaTime;
            // Read and write values of Joints once
            // When the dt changes to much we should do more ticks in the network to keep it consistent with Update and Fixedupdate functionalities.
            this.nerves.dt = dt / 2;
            this.nerves.tick();
            this.nerves.tick();
        }

    }

    internal class NaiveENN : ExplicitNN
    {
        internal float dt = float.NaN;
        private readonly List<NeuronSpec> neurons = null;

        public NaiveENN(Joint[] joints) : base(joints) { }

        public NaiveENN create(NNSpecification main, IEnumerable<NNSpecification> subnetworks, Joint[] joints)
        {
            NaiveENN N = new NaiveENN(joints);

            IEnumerable<NNSpecification> allNetworks = subnetworks.Union(Enumerable.Repeat(main, 1));
            //connect the networks
            IDictionary<InterfaceNode, IEnumerable<NNSpecification>> nnmappings = subnetworks.SelectMany(n => n.networkIn).ToDictionary(intnode => intnode, intn => allNetworks.Where(n => n.networkOut.Contains(intn)));

            //per node create the implemenetation and store it in a map from Spec->Impl
            IDictionary<SensorSpec, Neural> created = allNetworks.SelectMany(net => net.getAllNeurals()).ToDictionary(n => n, n => N.create(n));

            //now set all the appropiate weights
            foreach (NNSpecification network in allNetworks)
            {
                foreach (NeuronSpec n in network.getNeuronsAndActors())
                {
                    IList<WeightConnection> wcs = network.getSourceEdges(n).ToList();
                    double[] weights = new double[wcs.Count];
                    int i = 0;
                    foreach (WeightConnection w in wcs)
                    {
                        weights[i] = w.weight;
                        Neural input;
                        if (!w.source.isInterface())
                        {
                            //normal edge
                            SensorSpec source = (SensorSpec)w.source;
                            input = created[source];
                        } else
                        {
                            //edge that is from a different network
                            InterfaceNode hiddenSource = (InterfaceNode)w.source;
                            NNSpecification sourceNetwork = allNetworks.Where(net => net.networkOut == hiddenSource).Single();
                            SimpleConnection sourceConnection = sourceNetwork.getSourceEdges(hiddenSource).Single();
                            if(sourceConnection.source.isNeuron())
                            {
                                //edge from a different neuron
                                NeuronSpec source = (NeuronSpec)sourceConnection.source;
                                input = created[source];
                            }
                            else if (sourceConnection.source.isSensor())
                            {
                                //edge from a different sensor
                                SensorSpec source = (SensorSpec)sourceConnection.source;
                                input = created[source];
                            }
                        }
                        i++;
                    }
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
            IEnumerable<IConnection> startIncomming = edges.Where(e => e.source.isSensor());

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

        internal Neural create(SensorSpec ns)
        {
            if (ns .isActor())
            {
                return new SUM();
            }
            else if (ns.isNeuron())
            {
                NeuronSpec n = (NeuronSpec)ns;
                switch (n.function)
                {
                    case NeuronSpec.NFunc.ABS:
                        return new ABS();
                    case NeuronSpec.NFunc.ATAN:
                        return new ATAN();
                    case NeuronSpec.NFunc.SIN:
                        return new SIN();
                    case NeuronSpec.NFunc.COS:
                        return new COS();
                    case NeuronSpec.NFunc.SIGN:
                        return new SIGN();
                    case NeuronSpec.NFunc.SIGMOID:
                        return new SIGMOID();
                    case NeuronSpec.NFunc.EXP:
                        return new EXP();
                    case NeuronSpec.NFunc.LOG:
                        return new LOG();
                    case NeuronSpec.NFunc.DIFFERENTIATE:
                        return new DIFFERENTIATE(0);
                    case NeuronSpec.NFunc.INTERGRATE:
                        return new INTERGRATE();
                    case NeuronSpec.NFunc.MEMORY:
                        return new MEMORY();
                    case NeuronSpec.NFunc.SMOOTH:
                        return new SMOOTH();
                    case NeuronSpec.NFunc.SAW:
                        return new SAW(this);
                    case NeuronSpec.NFunc.WAVE:
                        return new WAVE(this);
                    case NeuronSpec.NFunc.MIN:
                        return new MIN();
                    case NeuronSpec.NFunc.MAX:
                        return new MAX();
                    case NeuronSpec.NFunc.SUM:
                        return new SUM();
                    case NeuronSpec.NFunc.PRODUCT:
                        return new PRODUCT();
                    case NeuronSpec.NFunc.DEVISION:
                        //double
                        return new DEVISION();
                    case NeuronSpec.NFunc.GTE:
                        //triple
                        return new GTE();
                    case NeuronSpec.NFunc.IF:
                        return new IF();
                    case NeuronSpec.NFunc.INTERPOLATE:
                        return new INTERPOLATE();
                    case NeuronSpec.NFunc.IFSUM:
                        return new IFSUM();
                    default:
                        throw new NotImplementedException("Function not known?");
                }
            }
            else
            {
                return new Neural();
            }
        }

        internal class Neural
        {
            internal double value = double.NaN;
        }

        internal abstract class INeuron : Neural
        {
            internal INeuron[] inputs = null;
            internal double[] weights = null;

            internal abstract void tick();
        }

        internal abstract class SingleFunction : INeuron
        {
            public abstract double y(double x);

            internal override void tick()
            {
                this.value = 0;
                for (int i = 0; i < this.weights.Length; i++)
                {
                    this.value += this.weights[i] * this.inputs[i].value;
                }
                this.value = y(value);
            }
        }
        internal abstract class TimeFunction : SingleFunction
        {
            NaiveENN parent;
            internal TimeFunction(NaiveENN parent)
            {
                this.parent = parent;
            }
            public override double y(double x)
            {
                return y(x, parent.dt);
            }
            public abstract double y(double x, double dtf);
        }
        internal abstract class DoubleFunction : INeuron
        {
            public abstract double y(double x, double y);

            internal override void tick()
            {
                this.value = y(this.weights[0] * this.inputs[0].value, this.weights[1] * this.inputs[1].value);
            }
        }
        internal abstract class TripleFunction : INeuron
        {
            public abstract double y(double x, double y, double z);

            internal override void tick()
            {
                this.value = y(this.weights[0] * this.inputs[0].value, this.weights[1] * this.inputs[1].value, this.weights[2] * this.inputs[2].value);
            }
        }
        internal abstract class MultipleFunction : INeuron
        {
            public abstract double y(INeuron[] x);

            internal override void tick()
            {
                this.value = y(this.inputs);
            }
        }

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
        internal class DIFFERENTIATE : SingleFunction
        {
            double last;
            public DIFFERENTIATE(double initValue) { this.last = initValue; }
            public override double y(double x)
            {
                double x0 = last;
                this.last = x;
                return x - x0;
            }
        }
        internal class INTERGRATE : SingleFunction
        {
            public override double y(double x)
            {
                throw new NotImplementedException();
            }
        }
        internal class MEMORY : SingleFunction
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
            public SAW(NaiveENN n) : base(n) { }
            double theta = 0;
            public override double y(double x, double dtf)
            {
                theta += x * dtf;
                return 2 * (theta % 1.0) - 1;
            }
        }
        internal class WAVE : TimeFunction
        {
            public WAVE(NaiveENN n) : base(n) { }
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
            public override double y(INeuron[] ns)
            {
                double min = ns[0].value;
                int index = 0;
                for (int i = 1; i < ns.Length; i++)
                {
                    if (ns[i].value < min)
                    {
                        min = ns[i].value;
                        index = i;
                    }
                }
                return min * this.weights[index];
            }
        }
        internal class MAX : MultipleFunction
        {
            public override double y(INeuron[] ns)
            {
                double max = ns[0].value;
                int index = 0;
                for (int i = 1; i < ns.Length; i++)
                {
                    if (ns[i].value > max)
                    {
                        max = ns[i].value;
                        index = i;
                    }
                }
                return max * this.weights[index];
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
