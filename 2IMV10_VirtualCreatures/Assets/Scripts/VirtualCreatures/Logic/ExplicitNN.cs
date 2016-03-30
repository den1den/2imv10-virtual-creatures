using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VirtualCreatures
{
    internal class NaiveENN : ExplicitNN
    {
        /// <summary>
        /// The first ones are the joints then the rest
        /// </summary>
        Neural[][] sensorNeurons;
        Neural[][] actorNeurons;
        JointType[] jointTypes;

        Neural[] internalNeurons = null;

        /// <summary>
        /// Create a NaiveENN network that controlles an couple of joints.
        /// </summary>
        /// <param name="joints"></param>
        NaiveENN(Joint[] joints) : base(joints)
        {
            this.sensorNeurons = new Neural[joints.Length][];
            this.actorNeurons = new Neural[joints.Length][];
            this.jointTypes = new JointType[joints.Length];
        }

        public static NaiveENN construct(Morphology morphology, Joint[] joints)
        {
            if (morphology.edges.Count != joints.Length) throw new ArgumentException(); //every edge should correspond to exactly one joint
            //throw all networks together
            IEnumerable<NNSpecification> allNetworks = Enumerable.Repeat(morphology.brain, 1).Union(morphology.edges.Select(e => e.network));
            foreach(NNSpecification n in allNetworks) { n.checkInvariants();}

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
                //check that that nothing is underspecified
                if (nDOF < nSen) throw new ArgumentException();
                if (nDOF < nAct) throw new ArgumentException();

                //set the action that should be performed on write to a neuron
                N.jointTypes[i] = joint.jointType;

                //create the actual sensors
                //always create the same amount of sensor implementations as dergees of freedom of a joint
                N.sensorNeurons[i] = new Neural[nDOF];
                for(int j = 0; j < nDOF; j++)
                {
                    Neural sensor = N.createSensor();
                    N.sensorNeurons[i][j] = sensor;
                    if(j < nSen)
                    {
                        //this is attached to an specification
                        created[nn.sensors[j]] = sensor;
                    }
                }

                //and create the actual actors
                N.actorNeurons[i] = new Neural[nDOF];
                for (int j = 0; j < nDOF; j++)
                {
                    Neural actor = N.createActor();
                    N.actorNeurons[i][j] = actor;
                    if (j < nAct)
                    {
                        //this is attached to an specification
                        created[nn.actors[j]] = actor;
                    }
                }

                //also create all normal neurons in this network
                foreach (NeuralSpec n in nn.neurons)
                {
                    created[n] = N.createNeuron(n);
                }
            }

            //inv: all neurons are created
            //inv: per node create the implemenetation is stored in a map from Spec->Impl

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

            // DEBUG
            // extra check if the numer of sensors and actors are exactly the DOF's?

            return N;
        }

        Neural createNeuron(NeuralSpec n)
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

        Neural createActor() { return new SUM(); }

        Neural createSensor() { return new Neural(); }

        LinkedList<float> ticks = new LinkedList<float>();

        internal override void tickimpl(int N)
        {
            //read sensors
            for (int i = 0; i < this.sensorNeurons.Length; i++)
            {
                Joint source = this.joints[i];
                JointType type = this.jointTypes[i];
                Neural[] destination = this.sensorNeurons[i];
                
                switch (type)
                {
                    case JointType.FIXED:
                        break;
                    case JointType.HINDGE:
                        HingeJoint src = (HingeJoint)source;
                        float valX = src.angle;
                        valX /= 180;
                        // Actually read a value
                        //Debug.Log("Phenotype:tick:read " + valX);
                        destination[0].value = valX;
                        break;
                    case JointType.PISTON:
                    case JointType.ROTATIONAL:
                    default:
                        throw new NotSupportedException(type + " not supported in " + GetType());
                }
            }
            
            //do N internal ticks
            for(int i = 0; i < N; i++)
            {
                foreach (INeuron n in this.internalNeurons)
                {
                    n.tick();
                }
            }

            bool b = true;

            //write to actors
            for (int i = 0; i < this.sensorNeurons.Length; i++)
            {
                Joint destination = this.joints[i];
                JointType type = this.jointTypes[i];
                Neural[] source = this.sensorNeurons[i];

                switch (type)
                {
                    case JointType.FIXED:
                        break;
                    case JointType.HINDGE:
                        HingeJoint dest = (HingeJoint)destination;
                        JointMotor motor = dest.motor;
                        float valX = (float)source[0].value;

                        if (b)
                        {
                            ticks.AddLast(valX);
                            if(false && ticks.Count % 200 == 0)
                            {
                                //Write output values to file
                                string fileName = "floats.dat";
                                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(System.IO.File.Open(fileName, System.IO.FileMode.Create)))
                                {
                                    foreach (float val in ticks)
                                    {
                                        writer.WriteLine(val);
                                    }
                                    writer.Close();
                                }
                                Debug.Log("Written to file: floats.dat");
                            }
                            b = false;
                        }

                        motor.force = valX;
                        break;
                    case JointType.PISTON:
                    case JointType.ROTATIONAL:
                    default:
                        break;
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

        internal abstract class INeuronSingleFunction : INeuron
        {
            public abstract double y(double input);

            internal override void tick()
            {
                value = 0;
                for (int i = 0; i < this.weights.Length; i++)
                {
                    value += weights[i] * inputs[i].value;
                }
                value = y(value);
            }
        }

        internal abstract class INeuronSingleTimeFunction : INeuron
        {
            NaiveENN super;
            internal INeuronSingleTimeFunction(NaiveENN parent) { this.super = parent; }

            public abstract double y(double x, double dtf);

            internal override void tick()
            {
                if (this.weights.Length == 0)
                {
                    value = 1;
                }
                else
                {
                    value = 0;
                    for (int i = 0; i < this.weights.Length; i++)
                    {
                        value += weights[i] * inputs[i].value;
                    }
                }
                value = y(value, super.tickDt);
            }
        }

        internal abstract class INeuronDoubleFunction : INeuron
        {
            public abstract double y(double input1, double input2);

            internal override void tick()
            {
                value = y(weights[0] * inputs[0].value, weights[1] * inputs[1].value);
            }
        }

        internal abstract class INeuronTripleFunction : INeuron
        {
            public abstract double y(double input1, double input2, double input3);

            internal override void tick()
            {
                value = y(weights[0] * inputs[0].value, weights[1] * inputs[1].value, weights[2] * inputs[2].value);
            }
        }

        internal abstract class INeuronMultiFunction : INeuron
        {
            public abstract double y(Neural[] inputs);

            internal override void tick()
            {
                value = y(inputs);
            }
        }

        internal class ABS : INeuronSingleFunction
        {
            public override double y(double x)
            {
                return Math.Abs(x);
            }
        }
        internal class ATAN : INeuronSingleFunction
        {
            static double CX = 7;
            static double C = 1.0f / Math.Atan(CX);
            public override double y(double x)
            {
                return Math.Atan(CX * x) * C;
            }
        }
        internal class COS : INeuronSingleFunction
        {
            static double CX = Math.PI;
            public override double y(double x)
            {
                return Math.Cos(CX * x);
            }
        }
        internal class SIN : INeuronSingleFunction
        {
            static double CX = Math.PI;
            public override double y(double x)
            {
                return Math.Sin(CX * x);
            }
        }
        /// <summary>
        /// Expontential function: [-1,1] -> [0.018, 1]
        /// </summary>
        internal class EXP : INeuronSingleFunction
        {
            static double C = 1.0 / Math.Exp(2);
            static double CX = 2;
            public override double y(double x)
            {
                return Math.Exp(CX * x) * C;
            }
        }
        internal class LOG : INeuronSingleFunction
        {
            static double Base = Math.E;
            static double C = 0.2;
            public override double y(double x)
            {
                if(x == 0)
                {
                    return -1;
                }
                return Math.Max(-1, C * Math.Log(x, Base));
            }
        }
        internal class DIFFERENTIATE : INeuronSingleFunction
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
        internal class INTERGRATE : INeuronSingleFunction
        {
            public override double y(double x)
            {
                throw new NotImplementedException("I do not know how?");
            }
        }
        internal class MEMORY : INeuronSingleFunction
        {
            static int SIZE = 30;
            double[] vals = new double[SIZE];
            int ptr = 0;
            public override double y(double x)
            {
                vals[ptr] = x;
                ptr = ptr + 1 % vals.Length;
                return vals[ptr];
            }
        }
        internal class SMOOTH : INeuronSingleFunction
        {
            static readonly double[] WEIGHTS = new double[] { 1.0, 0.875, 0.75, 0.625, 0.5, 0.375, 0.25, 0.125 };
            double[] history = new double[WEIGHTS.Length];
            int ptr = 0;
            public override double y(double x)
            {
                //set the new value in the history
                history[ptr] = x;
                ptr = ptr + 1 % history.Length;
                double r = x * WEIGHTS[0];
                for (int w = 1; w < WEIGHTS.Length; w++) { r += WEIGHTS[w] * history[(ptr + w) % history.Length]; }
                return r;
            }
        }
        internal class SAW : INeuronSingleTimeFunction
        {
            public SAW(NaiveENN n) : base(n) { }
            double theta = 0;
            public override double y(double x, double dtf)
            {
                theta += x * dtf;
                return 2 * (theta % 1.0) - 1;
            }
        }
        internal class WAVE : INeuronSingleTimeFunction
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
        internal class SIGMOID : INeuronSingleFunction
        {
            static double CX = -5;
            public override double y(double x)
            {
                return 1.0 / (1 + Math.Exp(CX * (x)));
            }
        }
        internal class SIGN : INeuronSingleFunction
        {
            public override double y(double x)
            {
                if (x >= 0)
                {
                    return 1;
                }
                return -1;
            }
        }

        internal class MIN : INeuronMultiFunction
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
        internal class MAX : INeuronMultiFunction
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
        internal class DEVISION : INeuronDoubleFunction
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
        internal class PRODUCT : INeuronMultiFunction
        {
            public override double y(Neural[] inputs)
            {
                double val = 1;
                for(int i = 0; i < inputs.Length; i++)
                {
                    val *= inputs[i].value;
                }
                return val;
            }
        }
        internal class SUM : INeuronSingleFunction
        {
            public override double y(double input)
            {
                return input;
            }
        }
        internal class GTE : INeuronTripleFunction
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
        internal class IF : INeuronTripleFunction
        {
            public override double y(double x, double y, double z)
            {
                if (x >= 0)
                {
                    return y;
                }
                else return -z;
            }
        }
        internal class INTERPOLATE : INeuronTripleFunction
        {
            public override double y(double x, double y, double z)
            {
                double w = (z + 1) / 2;
                return x * w + y * (1 - w);
            }
        }
        internal class IFSUM : INeuronTripleFunction
        {
            public override double y(double x, double y, double z)
            {
                if (x + y > z)
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

        internal float tickDt = float.NaN;
        internal void tick(int n)
        {
            tickimpl(n);
            totalTicks += n;
        }

        internal abstract void tickimpl(int n);

        int totalTicks = 0;
    }
}
