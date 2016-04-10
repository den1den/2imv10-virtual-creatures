using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VirtualCreatures
{
    public interface ExplicitNN
    {
        void doTicks(int n, float dt);
        void onDestory();
    }

    internal class NaiveNN : ExplicitNN
    {
        protected float globalTargetVelocity = 180; // deg/sec
        protected bool globalFreeSpin = true;
        protected float globalHindgeForceFactor = 300; // maximal force applied to Unity joints

        /// <summary>
        /// The first ones are the joints then the rest
        /// </summary>
        internal readonly Neural[][] sensorNeurons;
        internal readonly INeuron[][] actorNeurons;
        internal readonly JointType[] jointTypes;
        internal readonly Joint[] joints;

        internal INeuron[] internalNeurons = null;

        /// <summary>
        /// Create a empty NaiveENN network that controlles an couple of joints.
        /// </summary>
        /// <param name="joints"></param>
        internal NaiveNN(Morphology morphology, Joint[] joints)
        {
            // Set the arrays
            this.joints = joints;
            this.sensorNeurons = new Neural[joints.Length][];
            this.actorNeurons = new INeuron[joints.Length][];
            this.jointTypes = new JointType[joints.Length];
            for (int i = 0; i < joints.Length; i++)
            {
                //set the motor of the hindgejoints
                if (joints[i].GetType() == typeof(HingeJoint))
                {
                    HingeJoint hinge = (HingeJoint)joints[i];
                    JointMotor motor = hinge.motor;
                    motor.force = 0;
                    motor.targetVelocity = globalTargetVelocity;
                    motor.freeSpin = globalFreeSpin;
                    hinge.motor = motor; // strange? http://docs.unity3d.com/ScriptReference/HingeJoint-motor.html
                    hinge.useMotor = true;
                }
            }

            // Construct the network
            if (morphology.edges.Count != joints.Length) throw new ArgumentException(); //every edge should correspond to exactly one joint
            //throw all networks together
            IEnumerable<NNSpecification> allNetworks = Enumerable.Repeat(morphology.brain, 1).Union(morphology.edges.Select(e => e.network));

            if (Util.DEBUG)
                foreach (NNSpecification n in allNetworks) { n.checkInvariants(); }

            //create all neurons of the brain
            IDictionary<NeuralSpec, Neural> created = Enumerable
                .Repeat(morphology.brain, 1)
                .SelectMany(nn => nn.neurons) //brain has only normal neurons
                .ToDictionary(n => n, n => this.createNewNeuron(n));

            //create all the sensors and actors for the edges in the morphology
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
                this.jointTypes[i] = joint.jointType;

                //create the actual sensors
                //always create the same amount of sensor implementations as dergees of freedom of a joint
                this.sensorNeurons[i] = new Neural[nDOF];
                for (int j = 0; j < nDOF; j++)
                {
                    Neural sensor = this.createNewSensor();
                    this.sensorNeurons[i][j] = sensor;
                    if (j < nSen)
                    {
                        //this is attached to an specification
                        created[nn.sensors[j]] = sensor;
                    }
                }

                //and create the actual actors
                this.actorNeurons[i] = new INeuron[nDOF];
                for (int j = 0; j < nDOF; j++)
                {
                    INeuron actor = this.createNewActor();

                    this.actorNeurons[i][j] = actor;
                    if (j >= nAct)
                    {
                        //this NeuronActor is not specified to be connected to any joint
                    }
                    else
                    {
                        created[nn.actors[j]] = actor;
                    }
                }

                //also create all normal neurons in this network
                foreach (NeuralSpec n in nn.neurons)
                {
                    created[n] = this.createNewNeuron(n);
                }
            }

            //inv: all neurons are created
            //inv: per node create the implemenetation is stored in a map from Spec->Impl

            //now set all the appropiate weights and connect the neurons
            //so for each network
            foreach (NNSpecification network in allNetworks)
            {
                //and each node in the network
                foreach (NeuralSpec dest in network.getNeuronDestinationCandidates())
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

            // No specific ordering
            this.internalNeurons = created.Where(kvp => kvp.Key.isNeuron()).Select(kvp => kvp.Value).Cast<INeuron>().ToArray();

            // TODO: extra check if the numer of sensors and actors are exactly the DOF's?
        }

        Neural createNewNeuron(NeuralSpec n)
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

        INeuron createNewActor() { return new SUM(); }

        Neural createNewSensor() { return new Neural(); }

        internal float dt = float.NaN;

        public void doTicks(int n, float dt)
        {
            readSensors();

            //do N internal ticks
            this.dt = dt;
            for (int i = 0; i < n; i++)
            {
                tickImpl();
            }

            writeActors();
        }

        internal void readSensors()
        {
            for (int i = 0; i < this.sensorNeurons.Length; i++)
            {
                Joint joint = this.joints[i];
                JointType type = this.jointTypes[i];
                Neural[] sensor = this.sensorNeurons[i];

                switch (type)
                {
                    case JointType.FIXED:
                        break;
                    case JointType.HINDGE:
                        HingeJoint src = (HingeJoint)joint;
                        float valX = src.angle;
                        valX /= 180;
                        sensor[0].value = valX;
                        break;
                    case JointType.PISTON:
                    case JointType.ROTATIONAL:
                    default:
                        throw new NotSupportedException(type + " not supported in " + GetType());
                }
            }

            // Check if there is a sensor skipped
            if (Util.DEBUG)
            {
                for (int i = 0; i < this.sensorNeurons.Length; i++)
                {
                    for (int j = 0; j < this.sensorNeurons[i].Length; j++)
                    {
                        if (this.sensorNeurons[i][j].value == double.NaN)
                        {
                            throw new ApplicationException("Could not read sensor");
                        }
                    }
                }
            }
        }

        internal void tickImpl()
        {
            foreach (INeuron n in this.internalNeurons)
            {
                n.tick();
            }
        }

        internal void writeActors()
        {
            for (int i = 0; i < this.actorNeurons.Length; i++)
            {
                Joint destination = this.joints[i];
                JointType type = this.jointTypes[i];
                INeuron[] actor = this.actorNeurons[i];

                // collect inputs on the actors
                for(int d = 0; d < actor.Length; d++)
                {
                    actor[d].tick();
                }

                switch (type)
                {
                    case JointType.FIXED:
                        break;
                    case JointType.HINDGE:
                        HingeJoint hinge = (HingeJoint)destination;
                        float valX = (float)actor[0].value;

                        JointMotor motor = hinge.motor;
                        motor.force = valX * globalHindgeForceFactor;
                        hinge.motor = motor;
                        break;
                    case JointType.PISTON:
                    case JointType.ROTATIONAL:
                    default:
                        break;
                }
            }
        }

        public void onDestory() { }

        internal class Neural
        {
            internal double value = 0;
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
            NaiveNN parentNetwork;
            internal INeuronSingleTimeFunction(NaiveNN parentNetwork) { this.parentNetwork = parentNetwork; }

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
                value = y(value, parentNetwork.dt);
            }
        }

        internal abstract class INeuronDoubleFunction : INeuron
        {
            public abstract double y(double input1, double input2);

            internal override void tick()
            {
                if(Util.DEBUG)
                {
                    //FIXME: This is only debug
                    if (weights.Length != inputs.Length || weights.Length != 2)
                        throw new ApplicationException();
                }
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

        #region NeuronClasses
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
                if (x == 0)
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
            public SAW(NaiveNN n) : base(n) { }
            double theta = 0;
            public override double y(double x, double dtf)
            {
                theta += x * dtf;
                return 2 * (theta % 1.0) - 1;
            }
        }
        internal class WAVE : INeuronSingleTimeFunction
        {
            public WAVE(NaiveNN n) : base(n) { }
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
                for (int i = 0; i < inputs.Length; i++)
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
        #endregion
    }
    internal class NaiveNNDebugWrapper : ExplicitNN
    {
        NaiveNN wrapped;
        internal NaiveNNDebugWrapper(NaiveNN wrapped, string filename)
        {
            this.wrapped = wrapped;
            this.filename = filename;

            List<string> header = new List<string>();
            header.Add("Time");
            header.Add("TickOperations");
            header.Add("WriteToUnityOperations");
            for(int i = 0; i < wrapped.sensorNeurons.Length; i++)
            {
                NaiveNN.Neural[] sensors = wrapped.sensorNeurons[i];
                for (int j = 0; j < sensors.Length; j++)
                {
                    header.Add("SENSOR" + i + "_DOF" + j);
                }
            }
            for (int i = 0; i < wrapped.internalNeurons.Length; i++)
            {
                header.Add("NEURON" + i);
            }
            for (int i = 0; i < wrapped.actorNeurons.Length; i++)
            {
                NaiveNN.Neural[] actors = wrapped.actorNeurons[i];
                for (int j = 0; j < actors.Length; j++)
                {
                    header.Add("ACTOR" + i + "_DOF" + j);
                }
            }
            this.header = header.ToArray();
        }

        string filename;
        string[] header;
        IList<string[]> log = new List<string[]>();

        int WriteToUnityOperations = 0;
        int TickOperations = 0;
        float time = 0;

        public void doTicks(int n, float dt)
        {
            wrapped.readSensors(); // Copy unity values to the Neurons

            wrapped.dt = dt;
            time += dt;
            for (int i = 0; i < n; i++)
            {
                // Do n internal ticks of the network
                wrapped.tickImpl();
                if (i < n - 1)
                {
                    // Record the values after each tick (except for the last)
                    record();
                }
                TickOperations++;
            }

            wrapped.writeActors();
            record();
            WriteToUnityOperations++;
        }

        string format = "F4";
        IFormatProvider provider = System.Globalization.CultureInfo.CreateSpecificCulture("nl-NL");

        void record()
        {
            string[] line = new string[this.header.Length];
            int index = 0;
            line[index++] = time.ToString(format, provider);
            line[index++] = TickOperations.ToString(format, provider);
            line[index++] = WriteToUnityOperations.ToString(format, provider);
            for (int i = 0; i < wrapped.sensorNeurons.Length; i++)
            {
                NaiveNN.Neural[] sensors = wrapped.sensorNeurons[i];
                for (int j = 0; j < sensors.Length; j++)
                {
                    line[index++] = sensors[j].value.ToString(format, provider);
                }
            }
            for (int i = 0; i < wrapped.internalNeurons.Length; i++)
            {
                line[index++] = wrapped.internalNeurons[i].value.ToString(format, provider);
            }
            for (int i = 0; i < wrapped.actorNeurons.Length; i++)
            {
                NaiveNN.Neural[] actors = wrapped.actorNeurons[i];
                for (int j = 0; j < actors.Length; j++)
                {
                    line[index++] = actors[j].value.ToString(format, provider);
                }
            }
            log.Add(line);
        }

        public void onDestory()
        {
            Debug.Log("Writing networkoutput to " + filename);
            Util.writeCSV(filename, header, log);
            ((ExplicitNN)wrapped).onDestory();
        }
    }
}
