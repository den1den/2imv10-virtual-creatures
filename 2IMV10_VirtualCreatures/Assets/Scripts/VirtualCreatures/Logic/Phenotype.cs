using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VirtualCreatures
{
    // We shoud leave this class without any Unity thing related just as abstract as possible.
    public class Phenotype
    {
        NaiveENN nerves;

        public Phenotype(Morphology morphology, Joint[] joints)
        {
            this.nerves = NaiveENN.create(morphology, joints);
        }

        /// <summary>
        /// This function invokes 2 updates in the neural network and then writes the output forces to the joints directly
        /// </summary>
        /// <param name="dt"></param>
        public void update(float dt)
        {
            // Read and write values of Joints once
            // ? When the dt changes to much we should do more ticks in the network to keep it consistent with Update and Fixedupdate functionalities.
            this.nerves.tickDt = dt / 2;
            this.nerves.tick(2);
        }

    }

    internal class NaiveENN : ExplicitNN
    {
        internal float tickDt = float.NaN;
        /// <summary>
        /// The first ones are the joints then the rest
        /// </summary>
        private Neural[][] sensorNeurons;
        private double[][] sOffset, sFactor;

        private Neural[][] actorNeurons;
        private double[][] aOffset, aFactor;

        private Neural[] internalNeurons = null;

        /// <summary>
        /// Create a NaiveENN network that controlles an couple of joints.
        /// </summary>
        /// <param name="joints"></param>
        public NaiveENN(Joint[] joints) : base(joints)
        {
            this.sensorNeurons = new Neural[joints.Length][];
            this.sOffset = new double[joints.Length][];
            this.sFactor = new double[joints.Length][];
            this.actorNeurons = new Neural[joints.Length][];
            this.aOffset = new double[joints.Length][];
            this.aFactor = new double[joints.Length][];
        }

        public static NaiveENN create(Morphology morphology, Joint[] joints)
        {
            if (morphology.edges.Count != joints.Length) throw new ArgumentException(); //every edge should correspond to exactly one joint

            //create reference for the creation of neurons
            NaiveENN N = new NaiveENN(joints);

            //create all neurons of the brain
            IDictionary<NeuralSpec, Neural> created = Enumerable
                .Repeat(morphology.brain, 1)
                .SelectMany(nn => nn.neurons) //brain has only normal neurons
                .ToDictionary(n => n, n => N.createNeuron(n));

            //create all the sensors and actors for the edges
            for (int i = 0; i < morphology.edges.Count; i++)
            {
                //pick the corresponsing joint and network
                JointSpecification joint = morphology.edges[i].joint;
                int nDOF = joint.getDegreesOfFreedom();
                NNSpecification nn = morphology.edges[i].network;
                int nSen = nn.sensors.Count();
                int nAct = nn.actors.Count();

                //check that that nothing is overspecified
                if (nDOF < nSen) throw new ArgumentException();
                if (nDOF < nAct) throw new ArgumentException();

                N.sOffset[i] = Enumerable.Repeat(0.0, nSen).ToArray();
                N.sFactor[i] = Enumerable.Repeat(1.0, nSen).ToArray();

                N.aOffset[i] = Enumerable.Repeat(0.0, nAct).ToArray();
                N.aFactor[i] = Enumerable.Repeat(1.0, nAct).ToArray();

                //create the actual sensors
                N.sensorNeurons[i] = nn.sensors.Select(sen => N.createSensor(sen)).ToArray();
                //and add them
                for (int j = 0; j < N.sensorNeurons[i].Length; j++)
                {
                    Neural createdSensor = N.sensorNeurons[i][j];
                    created[nn.sensors[j]] = createdSensor;
                }

                //and create the actual actors
                N.actorNeurons[i] = nn.actors.Select(act => N.createActor(act)).ToArray();
                //and add them also
                for (int j = 0; j < N.actorNeurons[i].Length; j++)
                {
                    Neural createdActor = N.actorNeurons[i][j];
                    created[nn.actors[j]] = createdActor;
                }

                //also create all normal neurons in this network
                foreach (NeuralSpec n in nn.neurons)
                {
                    created[n] = N.createNeuron(n);
                }
            }
            //inv: per node create the implemenetation is stored in a map from Spec->Impl

            //throw all networks together
            IEnumerable<NNSpecification> allNetworks = Enumerable.Repeat(morphology.brain, 1).Union(morphology.edges.Select(e => e.network));

            //now set all the appropiate weights and connect the neurons
            //so for each network
            foreach (NNSpecification network in allNetworks)
            {
                //and each node in the network
                foreach (NeuralSpec dest in network.getNeuronsAndActors())
                {
                    //find the implemented INeuron
                    INeuron destImpl = (INeuron)created[dest];
                    //find all attached edges
                    IList<Connection> connectedEdges = network.getEdgesByDestination(dest).ToList();
                    double[] destWeights = new double[connectedEdges.Count];
                    Neural[] destInputs = new Neural[connectedEdges.Count];

                    int i = 0;
                    foreach (Connection con in connectedEdges)
                    {
                        NeuralSpec source = con.source;
                        Neural sourceImpl = created[source];
                        destWeights[i] = con.weight;
                        destInputs[i] = sourceImpl;
                        i++;
                    }
                    destImpl.weights = destWeights;
                    destImpl.inputs = destInputs;
                }
            }
            N.internalNeurons = created.Where(kvp => kvp.Key.isNeuron()).Select(kvp => kvp.Value).ToArray();

            return N;
        }

        private Neural createNeuron(NeuralSpec n)
        {
            if (!n.isNeuron()) throw new ArgumentException();
            switch (n.getFunction())
            {
                case NeuronFunc.ABS:
                    return new ABS();
                case NeuronFunc.ATAN:
                    return new ATAN();
                case NeuronFunc.SIN:
                    return new SIN();
                case NeuronFunc.COS:
                    return new COS();
                case NeuronFunc.SIGN:
                    return new SIGN();
                case NeuronFunc.SIGMOID:
                    return new SIGMOID();
                case NeuronFunc.EXP:
                    return new EXP();
                case NeuronFunc.LOG:
                    return new LOG();
                case NeuronFunc.DIFFERENTIATE:
                    return new DIFFERENTIATE(0);
                case NeuronFunc.INTERGRATE:
                    return new INTERGRATE();
                case NeuronFunc.MEMORY:
                    return new MEMORY();
                case NeuronFunc.SMOOTH:
                    return new SMOOTH();
                case NeuronFunc.SAW:
                    return new SAW(this);
                case NeuronFunc.WAVE:
                    return new WAVE(this);
                case NeuronFunc.MIN:
                    return new MIN();
                case NeuronFunc.MAX:
                    return new MAX();
                case NeuronFunc.SUM:
                    return new SUM();
                case NeuronFunc.PRODUCT:
                    return new PRODUCT();
                case NeuronFunc.DEVISION:
                    //double
                    return new DEVISION();
                case NeuronFunc.GTE:
                    //triple
                    return new GTE();
                case NeuronFunc.IF:
                    return new IF();
                case NeuronFunc.INTERPOLATE:
                    return new INTERPOLATE();
                case NeuronFunc.IFSUM:
                    return new IFSUM();
                default:
                    throw new NotImplementedException("Function not known?");
            }
        }

        private Neural createActor(NeuralSpec n) { if (n.isActor()) return new SUM(); else throw new ArgumentException(); }

        private Neural createSensor(NeuralSpec n) { if (n.isSensor()) return new Neural(); else throw new ArgumentException(); }


        internal override void tick(int N)
        {
            int i = 0;
            //read sensors
            for (int j = 0; j < this.sensorNeurons.Length; j++)
            {
                Joint joint = this.joints[j];
                Neural[] sNeurons = this.sensorNeurons[j];
                if (sNeurons.Length == 3)
                {
                    //3 degrees of freedom
                    //sNeurons[0].value = joint.X * sFactor[j][0] + sOffset[j][0];
                    //sNeurons[1].value = joint.Y * sFactor[j][1] + sOffset[j][1];
                    //sNeurons[2].value = joint.Z * sFactor[j][2] + sOffset[j][2];
                }
                else if (sNeurons.Length == 2)
                {
                    //2 degrees of freedom
                    //sNeurons[0].value = joint.X * sFactor[j][0] + sOffset[j][0];
                    //sNeurons[1].value = joint.Y * sFactor[j][1] + sOffset[j][1];
                }
                else if (sNeurons.Length == 1)
                {
                    //1 degrees of freedom
                    //hindge

                    //sNeurons[0].value = joint.X * sFactor[j][0] + sOffset[j][0];
                }
                else
                {
                    //0 degrees of freedom
                }
            }

            i = 0;
            //do N internal ticks
            do
            {
                foreach (INeuron n in this.internalNeurons)
                {
                    n.tick();
                }
            } while (++i < N);

            i = 0;
            //write to actors
            for (int j = 0; j < this.sensorNeurons.Length; j++)
            {
                Joint joint = this.joints[j];
                Neural[] aNeurons = this.actorNeurons[j];
                if (aNeurons.Length == 3)
                {
                    //3 degrees of freedom
                    //joint.X = aOffset[j][0] + aFactor[j][0] * aNeurons[0].value;
                    //joint.Y = aOffset[j][1] + aFactor[j][1] * aNeurons[1].value;
                    //joint.Z = aOffset[j][2] + aFactor[j][2] * aNeurons[2].value;
                }
                else if (aNeurons.Length == 2)
                {
                    //2 degrees of freedom
                    //joint.X = aOffset[j][0] + aFactor[j][0] * aNeurons[0].value;
                    //joint.Y = aOffset[j][1] + aFactor[j][1] * aNeurons[1].value;
                }
                else if (aNeurons.Length == 1)
                {
                    //1 degrees of freedom
                    HingeJoint h = (HingeJoint)joint;
                    JointMotor m = h.motor;
                    float force = (float)(aOffset[j][0] + aFactor[j][0] * aNeurons[0].value);
                    Console.WriteLine(String.Format("Appling force {0} to joint {1}", force, h));
                    m.force = force;
                }
                else
                {
                    //0 degrees of freedom
                }
            }
        }

        internal class Neural
        {
            internal double value = double.NaN;
        }

        internal abstract class INeuron : Neural
        {
            internal Neural[] inputs = null;
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
                return y(x, parent.tickDt);
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
            public abstract double y(Neural[] x);

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
                throw new NotImplementedException("I do not know how?");
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
            public override double y(Neural[] ns)
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
            public override double y(Neural[] ns)
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
        internal Joint[] joints;
        internal ExplicitNN(Joint[] joints)
        {
            this.joints = joints;
        }

        internal abstract void tick(int n);
    }
}
