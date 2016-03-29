using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VirtualCreatures
{
    public enum Fitness
    {
        WALKING, SWIMMING
    }

    public abstract class EvolutionAlgorithm
    {
        public int PopulationSize = 100;
        public float EvalUationTime = 10;
        public float InitializationTime = 1;

        internal abstract float getCreatureSize();

        abstract public IEnumerable<Morphology> generateInitialPopulation();

        /// <summary>
        /// Generates a new population and returns the resulting evolutions
        /// </summary>
        /// <param name="population">The previous population</param>
        /// <param name="initalPositions">The positions of the initial population</param>
        /// <returns>result[x] is evolution from population[x] AND result.SelectMany(xs => xs).Distinct().Count() == PopulationSize</returns>
        abstract public IEnumerable<IEnumerable<Morphology>> generateNewPopulation(CreatureController[] population, double[] fitness);

        public Fitness fitness = Fitness.WALKING;

        public static T getElement<T>(IEnumerable<T> enumerable)
        {
            int lastIndex = enumerable.Count() - 1;
            if (lastIndex == -1) return default(T);
            int index = EvolutionAlgorithm.random.Next(0, lastIndex);
            return enumerable.ElementAt(index);
        }

        public static T getElementExcept<T>(IEnumerable<T> enumerable, params T[] except)
        {
            return getElement(enumerable.Except(except));
        }

        readonly static public System.Random random = new System.Random(12041993);
    }

    public class EvolutionAlgorithm1 : EvolutionAlgorithm
    {
        // Neural Network Evolution
        FloatMutation weights;
        NeuronChooser newNeuron;
        MultipleDescision reconnectInternally;
        IntegerDescision addInternalConnection;
        MultipleDescision reconnectExternal;
        IntegerDescision addExteralConnection;

        // Edge and Joint Evolution
        DoubleMutation joint_faceHorizontal, joint_faceVertical, joint_rotation, joint_bending, joint_hover;
        NominalMutation<Face> joint_face;
        NominalMutation<JointType> joint_type;

        // not yet: Node Evolution

        public EvolutionAlgorithm1()
        {
            weights = new FloatMutation(
                0.25, // change of mutation
                0.75, // coherence with previous weight
                0.05f,// minimal weight > 0
                1     // maximal weight
            );
            newNeuron = new NeuronChooser( // TODO: These are all a gut feeling and should be tuned
                2,    // pick the second set of neurons (see NeuronChooser.GROUPINGS)
                0.95, // chance of adding a neuron on a iteration
                6,    // chance of adding SUM (1+ dimensional, default)
                2,    // chance of adding ABS, SIGN, SIGMOID (1+ dimensional, simple)
                0.5,  // chance of adding MEMROY, SMOOTH (1+ dimensional, simple, history)
                0.5,  // chance of adding ATAN, COS, EXP, LOG (1+ dimensional, complex)
                0.5,  // chance of adding SAW, WAVE (1+ dimensional, complex, time dependent)
                0.2,  // chance of adding DIFFERENTIATE, INTERGRATE (1+ dimentsional, complex, history)
                0.5,  // chance of adding MIN, MAX (2+ dimensional, simple)
                0.2,  // chance of adding DEVISION (2. dimenstional, complex)
                0.5,  // chance of adding PRODUCT (2+ dimensional, complex)
                0.5,  // chance of adding GTE, IF, IFSUM (3. dimensional, simple)
                0.5   // chance of adding INTERPOLATE (3. dimensional, complex)
            );
            reconnectInternally = new MultipleDescision(
                0.1,  // reconnect source
                0.1,  // reconnect destination
                0.05  // remove edge
            );
            addInternalConnection = new IntegerDescision(
                0.9,  // P(x==0) = 0.9 
                3     // a gaussian distribution for p(x>0) with p(x==3) < 0.05
            );
            reconnectExternal = new MultipleDescision(
                0.05,  // reconnect source (newSource in same network)
                0.05,  // reconnect source (newSource in adjacent network)
                0.05,  // reconnect destination (newDestination in same network)
                0.05,  // reconnect destination (newDestination in adjacent network)
                0.10   // remove edge
            );
            addExteralConnection = new IntegerDescision(
                0.9,  // P(x==0) = 0.9 
                1     // a gaussian distribution for p(x>0) with p(x==1) < 0.05
            );

            joint_faceHorizontal = new DoubleMutation(0, 1, -1, 1);
            joint_faceVertical = new DoubleMutation(0, 1, -1, 1);
            joint_rotation = new DoubleMutation(0, 1, (-Math.PI / 2), (-Math.PI / 2));
            joint_bending = new DoubleMutation(0, 1, (-Math.PI / 2), (-Math.PI / 2));
            joint_hover = new DoubleMutation(0, 1, 0.5f, 1f);
            joint_face = new NominalMutation<Face>(0);
            joint_type = new NominalMutation<JointType>(0);
        }

        Morphology mutate(Morphology parent)
        {
            Morphology result = parent.deepCopy();

            // first modify each edge internally
            foreach (EdgeMorph e in result.edges)
            {
                // Add an extra neuron to the network
                if (newNeuron.happens())
                {
                    e.network.addNewNeuron(newNeuron.getNext(e.network)); // iff the network is empty, the most simple neuron is returned
                }

                // change existing internal connections of the network
                mutateInternalConnections(e.network);

                // add internal connections to the network
                possiblyAddInternalConnections(e.network);

                // change parameters of the joints
                mutateJoint(e.joint);
            }

            // change existing connections between networks
            IDictionary<NNSpecification, IEnumerable<NNSpecification>> neighbours = parent.getNeighboringNetworksMap();
            foreach (KeyValuePair<Connection, NNSpecification[]> kvp in parent.getInterEdgeMap())
            {
                Connection c = kvp.Key;
                NeuralSpec sourceNeuron = c.source;
                NeuralSpec destNeuron = c.destination;
                NNSpecification sourceNetwork = kvp.Value[0];
                NNSpecification destNetwork = kvp.Value[1];

                c.weight = weights.possiblyChangeVal(c.weight);

                switch (reconnectExternal.happens())
                {
                    case 0: // reconnect source (newSource in same network)
                        NNSpecification newSourceNetwork = sourceNetwork;
                        NeuralSpec newSource = findNewSource(c.destination.isActor(), newSourceNetwork, destNeuron, destNetwork);
                        c.source = newSource;
                        break;
                    case 1: // reconnect source (newSource in adjacent network)
                        newSourceNetwork = EvolutionAlgorithm.getElement(neighbours[sourceNetwork]);
                        newSource = findNewSource(c.destination.isActor(), newSourceNetwork, destNeuron, destNetwork);
                        c.source = newSource;
                        kvp.Value[0] = newSourceNetwork;
                        break;
                    case 2: // reconnect destination (newDestination in same network)
                        NNSpecification newDestinationNetwork = destNetwork;
                        NeuralSpec newDestination = findNewDestination(c.source.isSensor(), newDestinationNetwork, sourceNeuron, sourceNetwork);
                        c.destination = newDestination;
                        break;
                    case 3: // reconnect destination (newDestination in adjacent network)
                        newDestinationNetwork = EvolutionAlgorithm.getElement(neighbours[destNetwork]);
                        newDestination = findNewDestination(c.source.isSensor(), newDestinationNetwork, sourceNeuron, sourceNetwork);
                        c.destination = newDestination;
                        kvp.Value[1] = newDestinationNetwork;
                        break;
                    case 4: // remove edge
                        sourceNetwork.removeExternalConnectionPartially(c);
                        destNetwork.removeExternalConnectionPartially(c);
                        break;
                    default: // no reconnection
                        break;
                }
            }

            // Add new connections
            int numberOfNewExternalConnections = addExteralConnection.newVal();
            for (int i = 0; i < numberOfNewExternalConnections; i++)
            {
                // get source from some network
                // TODO: could be made porportional to the neural nodes? instead of first choosing an random newSourceNetwork
                NNSpecification newSourceNetwork = EvolutionAlgorithm.getElement(parent.getAllNetworks());
                NeuralSpec newSource = EvolutionAlgorithm.getElement(newSourceNetwork.getNeuronSourceCandidates());

                // get destination from neighbouring network
                NNSpecification newDestNetwork = EvolutionAlgorithm.getElement(neighbours[newSourceNetwork]);
                NeuralSpec newDest = findNewDestination(newSource.isSensor(), newDestNetwork, newSource, newSourceNetwork);

                newSourceNetwork.addNewInterConnection(newSource, newDest, newDestNetwork, weights.newVal());
            }

            DotParser.write("stats/pre.gv", DotParser.parse(result.edges.Select(e => e.network), result.brain));

            // TODO: nextTODO: if this feasable?
            // Finalize by checking all cardinality constraints on the Neurons
            // first modify each edge internally
            foreach (EdgeMorph e in result.edges)
            {
                NNSpecification network = e.network;
                foreach(NeuralSpec neuron in network.neurons)
                {
                    int connected = network.getConnectedN(neuron);
                    if(connected < neuron.getMinimalConnections())
                    {
                        do // Add extra edge
                        {
                            NeuralSpec newSource = findNewSource(neuron.isActor(), network, neuron);
                            if(newSource == null)
                            {
                                throw new NotImplementedException("Not implemented yet; When the cardinality check gives to few neurons."); //TODO: Make sure to exclude certain neurons when the network is to small.
                            }
                            network.addNewLocalConnection(newSource, neuron, weights.newVal());
                            connected++;
                        } while (connected < neuron.getMinimalConnections());
                    }
                    else if (connected > neuron.getMaximalConnections())
                    {
                        do // Remove and edge
                        {
                            Connection subject = EvolutionAlgorithm.getElement(network.getEdgesByDestination(neuron));
                            network.removeInternalConnection(subject);
                            connected--;
                        } while (connected > neuron.getMaximalConnections());
                    }
                }
            }

            DotParser.write("stats/post.gv", DotParser.parse(result.edges.Select(e => e.network), result.brain));

            return result;
        }

        void mutateJoint(JointSpecification joint)
        {
            joint.faceHorizontal = joint_faceHorizontal.possiblyChangeVal(joint.faceHorizontal);
            joint.faceVertical = joint_faceVertical.possiblyChangeVal(joint.faceVertical);
            joint.rotation = joint_rotation.possiblyChangeVal(joint.rotation);
            joint.bending = joint_bending.possiblyChangeVal(joint.bending);
            joint.hover = joint_hover.possiblyChangeVal(joint.hover);
            joint.face = joint_face.possiblyChangeVal(joint.face);
            joint.jointType = joint_type.possiblyChangeVal(joint.jointType);
        }

        void possiblyAddInternalConnections(NNSpecification network)
        {
            int numberOfNewInternalConnections = this.addInternalConnection.newVal();
            for (int i = 0; i < numberOfNewInternalConnections; i++)
            {
                // pick source at random
                NeuralSpec newSource = EvolutionAlgorithm.getElement(network.getNeuronSourceCandidates());

                // find a possible destination
                NeuralSpec newDest = EvolutionAlgorithm.getElement(
                    network.getNeuronDestinationCandidates() //some sensor or neuron
                    .Except(network.getEdgesBySource(newSource).Select(edge => edge.destination)) //not already connected to source
                    .Except(Enumerable.Repeat(newSource, 1)) //nor the source itself
                );
                network.addNewLocalConnection(newSource, newDest, weights.newVal());
            }
        }

        void mutateInternalConnections(NNSpecification network)
        {
            var internalConns = network.getInternalConnections().ToList();
            foreach (Connection c in internalConns)
            {
                NeuralSpec originalSource = c.source;
                NeuralSpec originalDestination = c.destination;
                c.weight = weights.possiblyChangeVal(c.weight);
                switch (reconnectInternally.happens())
                {
                    case 0: // change source
                        NeuralSpec newSource = findNewSource(c.destination.isActor(), network, originalDestination);
                        c.source = newSource;
                        break;
                    case 1: // change destination
                        NeuralSpec newDest = findNewDestination(c.source.isSensor(), network, originalSource);
                        c.destination = newDest;
                        break;
                    case 2: // remove edge
                        network.removeInternalConnection(c);
                        break;
                    default: // no reconnection
                        break;
                }
            }
        }

        private static NeuralSpec findNewDestination(bool isSensor, NNSpecification inNetwork, NeuralSpec originalSource, NNSpecification sourceNetwork = null)
        {
            IEnumerable<NeuralSpec> candidates;
            if(isSensor)
            {
                candidates = inNetwork.getNeuronsOnly();
            }
            else
            {
                candidates = inNetwork.getNeuronDestinationCandidates();
            }
            if (sourceNetwork == null)
            {
                candidates = candidates
                .Except(inNetwork.getEdgesBySource(originalSource).Select(edge => edge.destination)) //must not already be connected (also excludes same edge)
                .Except(Enumerable.Repeat(originalSource, 1)); //must not be a direct loop
            }
            else
            {
                candidates = inNetwork.getNeuronDestinationCandidates()
                .Except(sourceNetwork.getEdgesBySource(originalSource).Select(edge => edge.destination)); //must not already be connected (also excludes same edge)
            }
            return EvolutionAlgorithm.getElement(candidates);
        }

        private static NeuralSpec findNewSource(bool isActor, NNSpecification inNetwork, NeuralSpec originalDestination, NNSpecification destinationNetwork = null)
        {
            IEnumerable<NeuralSpec> candidates;
            if (isActor)
            {
                candidates = inNetwork.getNeuronsOnly();
            }
            else
            {
                candidates = inNetwork.getNeuronSourceCandidates();
            }
            if (destinationNetwork == null)
            {
                candidates = candidates
                    .Except(inNetwork.getEdgesByDestination(originalDestination).Select(edge => edge.source)) //must not already be connected (also excludes same edge)
                    .Except(Enumerable.Repeat(originalDestination, 1)); //must not be a direct loop
            }
            else
            {
                candidates = candidates
                    .Except(destinationNetwork.getEdgesByDestination(originalDestination).Select(edge => edge.source)); //must not already be connected (also excludes same edge)
            }
            return EvolutionAlgorithm.getElement(candidates);
        }

        void mutate(Node node)
        {
            //not yet
            return;
        }

        internal override float getCreatureSize()
        {
            return 6f + 0.1f + 3f + 0.1f + 6f;
        }

        public override IEnumerable<Morphology> generateInitialPopulation()
        {
            return Enumerable.Range(1, this.PopulationSize).Select(i => mutate(BASE));
        }

        public override IEnumerable<IEnumerable<Morphology>> generateNewPopulation(CreatureController[] population, double[] fitness)
        {
            throw new NotImplementedException();
        }

        public static readonly Genotype BASE_GEN = new Genotype();
        public static readonly Morphology BASE = getBaseMutation();
        private static Morphology getBaseMutation()
        {
            ShapeSpecification body = Rectangle.createCube(3);
            ShapeSpecification fin = Rectangle.createPilar(6, 0.1f);

            Node root, left, right;
            root = new Node(body);
            left = new Node(fin);
            right = new Node(fin);

            JointSpecification leftCon = new JointSpecification(Face.LEFT, 0, 0, 0, 0, 0.1f, JointType.HINDGE);
            JointSpecification rightCon = new JointSpecification(Face.RIGHT, 0, 0, 0, 0, 0.1f, JointType.HINDGE);

            IList<EdgeMorph> edges = new EdgeMorph[]{
                new EdgeMorph(root, left, leftCon, generateNetwork(leftCon)),
                new EdgeMorph(root, right, rightCon, generateNetwork(rightCon)),
            }.ToList();

            return new Morphology(root, NNSpecification.createEmptyNetwork(), edges, BASE_GEN);
        }

        static NNSpecification generateNetwork(JointSpecification js)
        {
            return NNSpecification.createEmptyReadWriteNetwork(js.getDegreesOfFreedom(), js.getDegreesOfFreedom());
        }
    }

    internal class NeuronChooser : Descision
    {
        /// <summary>
        /// GROUPINGS[group id][indexedparameter] = functions of that index
        /// </summary>
        static readonly NeuronFunc[][][] GROUPINGS = new NeuronFunc[][][]
        {
            // 0, Single set
            new NeuronFunc[][] {
                (NeuronFunc[]) Enum.GetValues(typeof(NeuronFunc))
            },
            // 1, First intuition
            new NeuronFunc[][]
            {
                // singe
                new NeuronFunc[] { NeuronFunc.ABS, NeuronFunc.ATAN, NeuronFunc.COS, NeuronFunc.SIGN, NeuronFunc.SIGMOID, NeuronFunc.EXP, NeuronFunc.LOG, NeuronFunc.DIFFERENTIATE, NeuronFunc.INTERGRATE, NeuronFunc.MEMORY, NeuronFunc.SMOOTH },
                // time dependent
                new NeuronFunc[] { NeuronFunc.SAW, NeuronFunc.WAVE },
                // multiple
                new NeuronFunc[] { NeuronFunc.MIN, NeuronFunc.MAX, NeuronFunc.SUM, NeuronFunc.PRODUCT },
                // double
                new NeuronFunc[] { NeuronFunc.DEVISION },
                // tertaire
                new NeuronFunc[] { NeuronFunc.GTE, NeuronFunc.IF, NeuronFunc.INTERPOLATE, NeuronFunc.IFSUM }
            },
            // 2, Behavioural
            new NeuronFunc[][]
            {
                // 1+ dimensional, simplelest
                new NeuronFunc[] { NeuronFunc.SUM },
                // 1+ dimensional, simple
                new NeuronFunc[] { NeuronFunc.ABS, NeuronFunc.SIGN, NeuronFunc.SIGMOID },
                // 1+ dimensional, simple, history
                new NeuronFunc[] { NeuronFunc.MEMORY, NeuronFunc.SMOOTH },
                // 1+ dimensional, complex
                new NeuronFunc[] { NeuronFunc.ATAN, NeuronFunc.COS, NeuronFunc.EXP, NeuronFunc.LOG },
                // 1+ dimensional, complex, time dependent
                new NeuronFunc[] { NeuronFunc.SAW, NeuronFunc.WAVE },
                // 1+ dimentsional, complex, history
                new NeuronFunc[] { NeuronFunc.DIFFERENTIATE, NeuronFunc.INTERGRATE },
                // 2+ dimensional, simple
                new NeuronFunc[] { NeuronFunc.MIN, NeuronFunc.MAX },
                // 2. dimenstional, complex
                new NeuronFunc[] { NeuronFunc.DEVISION },
                // 2+ dimensional, complex
                new NeuronFunc[] { NeuronFunc.PRODUCT },
                // 3. dimensional, simple
                new NeuronFunc[] { NeuronFunc.GTE, NeuronFunc.IF, NeuronFunc.IFSUM },
                // 3. dimensional, complex
                new NeuronFunc[] { NeuronFunc.INTERPOLATE }
            },
        };
        /// <summary>
        /// REVERSE_GOURPING[group i][function] = index
        /// </summary>
        static IList<IDictionary<NeuronFunc, int>> REVERSE_GOURPING = GROUPINGS.Select(GROUP => {
            IDictionary<NeuronFunc, int> ReverseGroup = new Dictionary<NeuronFunc, int>();
            for (int i = 0; i < GROUP.Length; i++)
            {
                GROUP[i].Select(f => ReverseGroup[f] = i);
            }
            return ReverseGroup;
        }).ToList();

        NeuronFunc[][] set;
        MultipleDescision group_element_descision;

        public NeuronChooser(int indexSet, double p, params double[] porportions) : base(p)
        {
            set = GROUPINGS[indexSet];
            if (porportions.Length != set.Length) throw new ArgumentException();
            group_element_descision = new MultipleDescision(true, porportions);
        }

        public NeuronChooser(NeuronFunc[][] set, double p, params double[] porportions) : base(p)
        {
            this.set = set;
            if (porportions.Length != set.Length) throw new ArgumentException();
            group_element_descision = new MultipleDescision(true, porportions);
        }

        /// <summary>
        /// Get a plausible function for the next neuron to some network
        /// </summary>
        /// <param name="network"></param>
        /// <returns></returns>
        public NeuronFunc getNext(NNSpecification network)
        {
            //TODO network topology is ignored, we iterate until a right one is found
            //TODO Might even be a good choice, but the current network topology is not used when generating new neuron
            int networkSize = network.getNumberOfNeurons();
            if(networkSize == 0)
            {
                //just return a Neuron from the first set
                return EvolutionAlgorithm.getElement(set[0]);
            }
            int minimalConnectionsNeeded;
            NeuronFunc result;
            do
            {
                int chosenGroup = group_element_descision.happens();
                NeuronFunc[] group = set[chosenGroup];
                result = EvolutionAlgorithm.getElement(group);
                minimalConnectionsNeeded = NeuralSpec.getMinimalConnections(result);
            } while (minimalConnectionsNeeded > networkSize);
            return result;
        }
    }

    public class Descision
    {
        public double probability = 0.2;
        internal Descision(double p) { this.probability = p; }

        public bool happens()
        {
            // probability = 0 => false;
            // probability = 1 => true;
            return EvolutionAlgorithm.random.NextDouble() < probability;
        }
    }

    public class DoubleMutation : Descision
    {
        public double min, max;
        public double coherence;

        /// <summary>
        /// Coherence of 0 means no coherence and gives a completely uniform distribution between min and max.
        /// Coherence of 1 means maximal coherence and gives a normal distribution with mean = old value, and sigma is very small
        /// </summary>
        /// <param name="p">Change on a possibleMuitation</param>
        /// <param name="coherence">Between 0 and 1 to define similarity with old value.</param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        internal DoubleMutation(double p, double coherence, double min, double max) : base(p) { this.coherence = coherence; this.min = min; this.max = max; }
        internal DoubleMutation(double min, double max) : this(0, double.MaxValue, min, max) { }

        /// <summary>
        /// Uniform distribution
        /// </summary>
        /// <returns></returns>
        public double newVal()
        {
            return min + (max - min) * EvolutionAlgorithm.random.NextDouble();
        }

        public double newVal(double oldVal)
        {
            if (coherence >= 1) return oldVal;
            return coherence * normalDistScaled(oldVal, min, max) + (1 - coherence) * newVal();
        }

        public double possiblyChangeVal(double old)
        {
            if (happens())
            {
                return newVal(old);
            }
            return old;
        }

        public static double SIGMA = 1.0 / 3.5; // sigma of the normal distr s.t. P( x < -1 || x > 1 ) <= 0.01
        public static double normalDist(double max)
        {
            double u1 = EvolutionAlgorithm.random.NextDouble();
            double u2 = EvolutionAlgorithm.random.NextDouble();

            double rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                Math.Sin(2.0 * Math.PI * u2);

            // FloatMutation.sigma is the value such that p(x > 1) < 0.005
            rand_std_normal *= SIGMA;

            if (rand_std_normal > 1) return max;
            else if (rand_std_normal < -1) return -max;
            else return rand_std_normal * max;
        }

        public static double normalDistScaled(double mean, double min, double max)
        {
            double diffMin = mean - min;
            double diffMax = max - mean;

            // get a normal distributed var s.t. the maximal value is not larger then diffMin and diffMax in both directions
            double mean0normalDist = normalDist(Math.Min(diffMin, diffMax));

            return mean + mean0normalDist;
        }
    }

    public class FloatMutation
    {
        DoubleMutation baseMutation;
        FloatMutation(DoubleMutation baseMutation) { this.baseMutation = baseMutation; }
        internal FloatMutation(double p, double coherence, float min, float max) : this(new DoubleMutation(p, coherence, min, max)) { }
        internal FloatMutation(float min, float max) : this(new DoubleMutation(min, max)) { }

        internal float possiblyChangeVal(float old)
        {
            return (float)baseMutation.possiblyChangeVal(old);
        }

        internal float newVal()
        {
            return (float)baseMutation.newVal();
        }
    }

    public class IntegerMutation
    {
        DoubleMutation baseMutation;
        IntegerMutation(DoubleMutation baseMutation) { this.baseMutation = baseMutation; }
        internal IntegerMutation(double p, double coherence, int min, int max) : this(new DoubleMutation(p, coherence, min, max)) { }

        public int newVal()
        {
            return (int)(Math.Round(baseMutation.newVal()));
        }

        public int newVal(int oldVal)
        {
            return (int)(Math.Round(baseMutation.newVal(oldVal)));
        }

        public int possiblyChangeVal(int oldVal)
        {
            return (int)(Math.Round(baseMutation.possiblyChangeVal(oldVal)));
        }
    }

    public class IntegerDescision : Descision
    {
        int max;
        public IntegerDescision(double pZero, int max) : base(pZero) { this.max = max; }

        public int newVal()
        {
            if (!happens()) return 0;
            double normalVar = DoubleMutation.normalDist(this.max - 1);
            int normalInt = (int)(Math.Round(normalVar));
            return Math.Abs(normalInt) + 1;
        }
    }

    public class NominalMutation<E> : Descision
    {
        E[] vals;

        internal NominalMutation(double p, E[] vals) : base(p) { this.vals = vals; }

        internal NominalMutation(double p) : base(p)
        {
            var type = typeof(E);
            if (!type.IsEnum) throw new ApplicationException();
            this.vals = (E[])Enum.GetValues(typeof(E));
        }

        public E possiblyChangeVal(E oldVal)
        {
            if (!happens()) return oldVal;
            return EvolutionAlgorithm.getElementExcept(vals, oldVal);
        }

        internal E getNewVal()
        {
            return EvolutionAlgorithm.getElement(vals);
        }

        internal E getNewValExcept(IEnumerable<E> except)
        {
            return EvolutionAlgorithm.getElement(vals.Except(except));
        }
    }

    public class MultipleDescision
    {
        double[] ps;
        internal MultipleDescision(params double[] ps)
        {
            this.ps = ps;
        }

        internal MultipleDescision(bool normalized, params double[] ps) : this(ps)
        {
            if (normalized)
            {
                double total = this.ps.Max();
                for(int i = 0; i < ps.Length; i++)
                {
                    ps[i] /= total;
                }
            }
        }

        public int happens()
        {
            double r = EvolutionAlgorithm.random.NextDouble();
            int i;
            for (i = 0; i < ps.Length; i++)
            {
                if (r < ps[i])
                {
                    return i;
                }
                else
                {
                    r -= ps[i];
                }
            }
            return ps.Length;
        }
    }

}
