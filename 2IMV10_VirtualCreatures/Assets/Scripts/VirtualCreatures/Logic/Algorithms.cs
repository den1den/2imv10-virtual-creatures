﻿using System;
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
        public float InitializationTime = Util.INITIAL_EVALUATION_TIME;
        public float EvaluationTime = Util.FITNESS_EVALUATION_TIME;
        public int GenerationCount;
        
        public PopulationMember[] population;

        /// <summary>
        /// Generates a new population. this sets the this.population variable.
        /// </summary>
        public abstract void generateNewPopulation(IList<CreatureController> oldPopulation, double[] fitness);
        public abstract void generateNewPopulation();

        internal abstract float getCreatureSize();

        public Fitness fitness = Fitness.WALKING;

        /// <summary>
        /// Stores all the generated population in a graph.
        /// Beware this graph is not a tree, because it can contain common parents
        /// </summary>
        public class PopulationMember
        {
            private static int idcounter = 0;

            public readonly Morphology morphology;
            public readonly IEnumerable<PopulationMember> parents;
            public double fitness = double.NaN;
            public double avgFitness = double.NaN;
            public readonly int id;

            internal PopulationMember(Morphology morphology, IEnumerable<PopulationMember> parents)
            {
                this.morphology = morphology;
                this.parents = parents;
                this.id = idcounter;
                idcounter++;
            }

            internal PopulationMember(Morphology morphology, params PopulationMember[] parents) : this(morphology, parents.AsEnumerable()) { }

            internal PopulationMember(Morphology morphology) : this(morphology, new List<PopulationMember>()) { }
        }

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
                0.2,  // chance of adding DIFFERENTIATE (1+ dimentsional, complex, history) //NOTE: intergration is left out
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

        Morphology mutate(Morphology morphology, double coherenceToOriginal, int sequenceNumber, int populationNumber)
        {
            morphology = morphology.deepCopy();
            Util.tryPrintMutation("0-start", morphology, sequenceNumber, populationNumber);

            // first modify each edge internally
            foreach (EdgeMorph e in morphology.edges)
            {
                // Add an extra neuron to the network
                if (newNeuron.happens())
                {
                    NeuronFunc newFunction = newNeuron.getNext(e.network);
                    e.network.addNewNeuron(newFunction); // iff the network is empty, the most simple neuron is returned
                }

                // change existing internal connections of the network
                mutateInternalConnections(e.network, coherenceToOriginal);

                // add internal connections to the network
                possiblyAddInternalConnections(e.network);

                // change parameters of the joints
                mutateJoint(e.joint, coherenceToOriginal);
            }

            Util.tryPrintMutation("1-internal", morphology, sequenceNumber, populationNumber);

            // change existing connections between networks
            IDictionary<NNSpecification, IEnumerable<NNSpecification>> neighbours = morphology.getNeighboringNetworksMap();

            foreach (KeyValuePair<Connection, NNSpecification[]> kvp in morphology.getInterEdgeMap())
            {
                Connection c = kvp.Key;
                NeuralSpec source = c.source;
                NeuralSpec destination = c.destination;
                NNSpecification sourceNetwork = kvp.Value[0];
                NNSpecification destinationNetwork = kvp.Value[1];

                c.weight = weights.possiblyChangeVal(c.weight, coherenceToOriginal);

                switch (reconnectExternal.happens(coherenceToOriginal))
                {
                    case 0: // reconnect source (sourceNetwork remains the same)
                        NeuralSpec newSource = findNewSource(destinationNetwork, destination, sourceNetwork);
                        if (newSource == null) break;

                        c.source = newSource;
                        break;
                    case 1: // reconnect source (sourceNetwork is in adjacent network)
                        NNSpecification newSourceNetwork = EvolutionAlgorithm.getElementExcept(neighbours[sourceNetwork], destinationNetwork);
                        if (newSourceNetwork == null) break;

                        newSource = findNewSource(destinationNetwork, destination, newSourceNetwork);
                        if (newSource == null) break;

                        // Change connection
                        destinationNetwork.moveExternalConnectionItsSource(c, newSourceNetwork, newSource, sourceNetwork);
                        kvp.Value[0] = newSourceNetwork; // update the interedge map source network to keep inv
                        break;
                    case 2: // reconnect destination (destinationNetwork remains the same)
                        NeuralSpec newDestination = findNewDestination(sourceNetwork, source, destinationNetwork);
                        if (newDestination == null) break;

                        c.destination = newDestination;
                        break;
                    case 3: // reconnect destination (destinationNetwork is in adjacent network)
                        NNSpecification newDestinationNetwork = EvolutionAlgorithm.getElementExcept(neighbours[destinationNetwork], sourceNetwork);
                        if (newDestinationNetwork == null) break;

                        newDestination = findNewDestination(sourceNetwork, source, newDestinationNetwork);
                        if (newDestination == null) break;

                        // Change connection
                        sourceNetwork.moveExternalConnectionItsDestination(c, newDestinationNetwork, newDestination, destinationNetwork);
                        kvp.Value[1] = newDestinationNetwork; // update the interedge map source network to keep inv
                        break;
                    case 4: // remove edge
                        sourceNetwork.removeExternalConnection(c, destinationNetwork);
                        break;
                    default: // no reconnection
                        break;
                }
            }

            Util.tryPrintMutation("2-betweennetworks", morphology, sequenceNumber, populationNumber);

            // Add new inter network neuron connections
            int numberOfNewExternalConnections = addExteralConnection.newVal(coherenceToOriginal);
            for (int i = 0; i < numberOfNewExternalConnections; i++)
            {
                // get source from some network
                // TODO: could be made porportional to the neural nodes? instead of first choosing an (uniformly) random newSourceNetwork
                NNSpecification newSourceNetwork = EvolutionAlgorithm.getElement(morphology.getAllNetworks());
                if (newSourceNetwork == null) continue;

                NeuralSpec newSource = EvolutionAlgorithm.getElement(newSourceNetwork.getNeuronSourceCandidates());
                if (newSource == null) continue; //Not possible for this network

                // get destination from neighbouring network
                NNSpecification newDestinationNetwork = EvolutionAlgorithm.getElementExcept(neighbours[newSourceNetwork], newSourceNetwork);
                if (newDestinationNetwork == null) continue;

                NeuralSpec newDest = findNewDestination(newSourceNetwork, newSource, newDestinationNetwork);
                if (newDest == null) continue;

                newSourceNetwork.addNewInterConnection(newSource, newDest, newDestinationNetwork, weights.newVal());
            }

            Util.tryPrintMutation("3-add-betweennetworks", morphology, sequenceNumber, populationNumber);

            // Finalize by checking all cardinality constraints on the Neurons
            // first modify each edge internally
            foreach (EdgeMorph e in morphology.edges)
            {
                NNSpecification network = e.network;
                foreach (NeuralSpec neuron in network.neurons)
                {
                    int connected = network.getConnectedN(neuron);
                    if (connected < neuron.getMinimalConnections())
                    {
                        do // Add extra edge
                        {
                            NeuralSpec destination = neuron;
                            NeuralSpec newSource = findNewSource(network, destination, network);
                            if (newSource == null)
                            {
                                //FIXME: No suitable action for a network with to few nodes to cohere to a nodes cardinality constraints. Maby exclude or remove neurons then?
                                break;
                            }
                            network.addNewLocalConnection(newSource, destination, weights.newVal());
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

            Util.tryPrintMutation("4-fix", morphology, sequenceNumber, populationNumber);

            return morphology;
        }

        void mutateJoint(JointSpecification joint, double coherenceToOriginal)
        {
            joint.faceHorizontal = joint_faceHorizontal.possiblyChangeVal(joint.faceHorizontal, coherenceToOriginal);
            joint.faceVertical = joint_faceVertical.possiblyChangeVal(joint.faceVertical, coherenceToOriginal);
            joint.rotation = joint_rotation.possiblyChangeVal(joint.rotation, coherenceToOriginal);
            joint.bending = joint_bending.possiblyChangeVal(joint.bending, coherenceToOriginal);
            joint.hover = joint_hover.possiblyChangeVal(joint.hover, coherenceToOriginal);
            joint.face = joint_face.possiblyChangeVal(joint.face, coherenceToOriginal);
            joint.jointType = joint_type.possiblyChangeVal(joint.jointType, coherenceToOriginal);
        }

        void possiblyAddInternalConnections(NNSpecification network)
        {
            if (network.getNumberOfSourceCandidates() <= 0)
            {
                //Could not add anything here...
                return;
            }
            int numberOfNewInternalConnections = this.addInternalConnection.newVal();
            for (int i = 0; i < numberOfNewInternalConnections; i++)
            {
                // pick source at random
                NeuralSpec newSource = EvolutionAlgorithm.getElement(network.getNeuronSourceCandidates());

                // find a possible destination
                NeuralSpec newDest = findNewDestination(network, newSource, network);

                if (newDest != null) network.addNewLocalConnection(newSource, newDest, weights.newVal());
            }
        }

        void mutateInternalConnections(NNSpecification network, double coherenceToOriginal)
        {
            var internalConns = network.getInternalConnections().ToList();
            foreach (Connection c in internalConns)
            {
                NeuralSpec originalSource = c.source;
                NeuralSpec originalDestination = c.destination;
                c.weight = weights.possiblyChangeVal(c.weight, coherenceToOriginal);
                switch (reconnectInternally.happens(coherenceToOriginal))
                {
                    case 0: // change source
                        NeuralSpec newSource = findNewSource(network, originalDestination, network);
                        if (newSource != null) c.source = newSource;
                        break;
                    case 1: // change destination
                        NeuralSpec newDest = findNewDestination(network, originalSource, network);
                        if (newDest != null) c.destination = newDest;
                        break;
                    case 2: // remove edge
                        network.removeInternalConnection(c);
                        break;
                    default: // no reconnection
                        break;
                }
            }
        }

        /// <summary>
        /// Finds a new random destination given a source
        /// </summary>
        /// <param name="sourceNetwork"></param>
        /// <param name="source"></param>
        /// <param name="newDestinationNetwork"></param>
        /// <returns>null or a NeuralSpec</returns>
        private static NeuralSpec findNewDestination(NNSpecification sourceNetwork, NeuralSpec source, NNSpecification newDestinationNetwork)
        {
            IEnumerable<NeuralSpec> candidates;
            if (source.isSensor())
            {
                // we cannot select actors
                candidates = newDestinationNetwork.getNeuronsOnly();
            }
            else
            {
                candidates = newDestinationNetwork.getNeuronDestinationCandidates();
            }
            // must not already be connected (also excludes same edge)
            candidates = candidates.Except(sourceNetwork.getEdgesBySource(source).Select(edge => edge.destination));
            if (sourceNetwork == newDestinationNetwork)
            {
                // also exclude self loops
                candidates = candidates.Except(Enumerable.Repeat(source, 1));
            }
            return EvolutionAlgorithm.getElement(candidates);
        }

        private static NeuralSpec findNewSource(NNSpecification destinationNetwork, NeuralSpec destination, NNSpecification newSourceNetwork)
        {
            IEnumerable<NeuralSpec> candidates;
            if (destination.isActor())
            {
                // we cannot select sensors
                candidates = newSourceNetwork.getNeuronsOnly();
            }
            else
            {
                candidates = newSourceNetwork.getNeuronSourceCandidates();
            }
            // must not already be connected (also excludes same edge)
            candidates = candidates.Except(destinationNetwork.getEdgesByDestination(destination).Select(edge => edge.source));
            if (newSourceNetwork == destinationNetwork)
            {
                // also exclude self loops
                candidates = candidates.Except(Enumerable.Repeat(destination, 1));
            }
            return EvolutionAlgorithm.getElement(candidates);
        }

        void mutate(Node node)
        {
            // Out of scope
            return;
        }

        /// <summary>
        /// Generate the spec for the first population
        /// </summary>
        public override void generateNewPopulation()
        {
            this.population = new PopulationMember[Util.INITIAL_POPULATION_SIZE];
            GenerationCount = 1;

            PopulationMember the_first = new PopulationMember(BASE); // no parents
            this.population[0] = the_first;

            double coherenceToOriginal = 0;
            for (int i = 1; i < this.population.Length; i++)
            {
                PopulationMember offspring = new PopulationMember(
                    mutate(BASE, coherenceToOriginal, i, GenerationCount) // no parents, although BASE actually is it's parent
                );
                this.population[i] = offspring;
            }
        }

        public override void generateNewPopulation(IList<CreatureController> prevPopulation, double[] fitness)
        {
            double max = fitness.Max();

            IList<PopulationMember> newPopulation = new List<PopulationMember>();

            for (int oldIndex = 0; oldIndex < prevPopulation.Count; oldIndex++)
            {
                // Store fitness of the previous generation
                PopulationMember member = this.population[oldIndex];
                member.fitness = fitness[oldIndex];
                double parentalFitness = member.parents.Select(p => p.avgFitness).Sum();
                parentalFitness = parentalFitness / member.parents.Count();
                member.avgFitness =
                    0.75 * parentalFitness
                    + 0.25 * member.fitness;
                if (double.IsNaN(member.avgFitness))
                {
                    member.avgFitness = member.fitness;
                }
                // FIXME: terminate this spieces if it has a dislocated joint (or broken body part)
            }
            Util.tryPrintEveryPopulation(this.population, GenerationCount);

            IList<PopulationMember> oldPopulation = this.population.OrderByDescending(p => p.avgFitness).ToList();
            int i = 0;
            double children = Util.INITIAL_POPULATION_SIZE / 10;
            while(newPopulation.Count < Util.INITIAL_POPULATION_SIZE && i < oldPopulation.Count)
            {
                PopulationMember member = oldPopulation[i++];
                double coherenceToOriginal = 0.97 * member.fitness / max;
                for(int c = 0; c < children; c++)
                {
                    Morphology newSpec = mutate(member.morphology, coherenceToOriginal, newPopulation.Count, GenerationCount);
                    PopulationMember newMember = new PopulationMember(newSpec, member);
                    newPopulation.Add(newMember);
                }
                
                children *= 0.8;
            }

            // Save the new population
            this.population = newPopulation.ToArray();
            GenerationCount++;
        }

        internal override float getCreatureSize()
        {
            return 6f + 0.1f + 3f + 0.1f + 6f;
        }

        public static readonly Genotype BASE_GEN = new Genotype();
        public static readonly Morphology BASE = getBaseMutation();
        private static Morphology getBaseMutation()
        {
            //Root body
            ShapeSpecification body = Rectangle.createCube(3);
            Node root = new Node(body, "Base_Root");

            //Left
            ShapeSpecification fin = Rectangle.createPilar(6, 0.1f);
            Node left = new Node(fin, "Base_Left");
            JointSpecification leftCon = new JointSpecification(Face.LEFT, 0, 0, 0, 0, 0.1f, JointType.HINDGE);
            NNSpecification leftNetwork = NNSpecification.createEmptyWriteNetwork(leftCon.getDegreesOfFreedom());

            //Right
            Node right = new Node(fin, "Base_Right");
            JointSpecification rightCon = new JointSpecification(Face.RIGHT, 0, 0, 0, 0, 0.1f, JointType.HINDGE);
            NNSpecification rightNetwork = NNSpecification.createEmptyWriteNetwork(rightCon.getDegreesOfFreedom());

            //Brain (sinus wave output on second node)
            NNSpecification brain = NNSpecification.testBrain1();
            //Directly manages left and right output
            brain.addNewInterConnection(brain.neurons[1], leftNetwork.actors[0], leftNetwork, 1);
            brain.addNewInterConnection(brain.neurons[1], rightNetwork.actors[0], rightNetwork, 1);

            IList<EdgeMorph> edges = new EdgeMorph[]{
                new EdgeMorph(root, left, leftCon, leftNetwork),
                new EdgeMorph(root, right, rightCon, rightNetwork),
            }.ToList();

            if (Util.DEBUG)
            {
                Connection[] bi = brain.getOutgoingConnections().ToArray();
                Connection[] bo = brain.getIncommingConnections().ToArray();
                Connection[] li = leftNetwork.getOutgoingConnections().ToArray();
                Connection[] lo = leftNetwork.getIncommingConnections().ToArray();
                Connection[] ri = rightNetwork.getOutgoingConnections().ToArray();
                Connection[] ro = rightNetwork.getIncommingConnections().ToArray();
            }

            Morphology m = new Morphology(root, brain, edges, BASE_GEN);

            return m;
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
                new NeuronFunc[] { NeuronFunc.DIFFERENTIATE }, // Note: NeuronFunc.INTERGRATE is left out because NaiveNN cannot handle it
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
        static IList<IDictionary<NeuronFunc, int>> REVERSE_GROUPING = GROUPINGS.Select(GROUP => {
            IDictionary<NeuronFunc, int> ReverseGroup = new Dictionary<NeuronFunc, int>();
            for (int i = 0; i < GROUP.Length; i++)
            {
                GROUP[i].Select(f => ReverseGroup[f] = i);
            }
            return ReverseGroup;
        }).ToList();

        NeuronFunc[][] set;
        IDictionary<NeuronFunc, int> reverseSet;
        MultipleDescision group_element_descision;

        public NeuronChooser(NeuronFunc[][] set, IDictionary<NeuronFunc, int> reverseSet, double p, params double[] porportions) : base(p)
        {
            this.set = set;
            this.reverseSet = reverseSet;
            if (porportions.Length != set.Length) throw new ArgumentException();
            group_element_descision = new MultipleDescision(true, porportions);
        }

        /// <summary>
        /// Construct using the static GROUPING groupings.
        /// </summary>
        /// <param name="groupingIndex">the index of the GROUPING</param>
        /// <param name="p">chance on this event</param>
        /// <param name="porportions">The poprtions of chance off each possibel result, in the same ordering. (an (unscaled) chancedensityfunction)</param>
        public NeuronChooser(int groupingIndex, double p, params double[] porportions) : this(GROUPINGS[groupingIndex], REVERSE_GROUPING[groupingIndex], p, porportions) { }

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
            if (networkSize == 0)
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

        public int getGroup(NeuronFunc function)
        {
            return reverseSet[function];
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

        public bool happens(double coherenceToFalse)
        {
            double probability = this.probability * (1 - coherenceToFalse);
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

        internal double possiblyChangeVal(double old, double coherenceToOriginal)
        {
            if (happens(coherenceToOriginal))
            {
                return coherenceToOriginal * old
                    + (1 - coherenceToOriginal) * newVal(old);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weight"></param>
        /// <param name="coherence">Extra coherence</param>
        /// <returns></returns>
        internal float possiblyChangeVal(float old, double coherenceToOriginal)
        {
            return (float)(
                (coherenceToOriginal) * old
                + (1 - coherenceToOriginal) * baseMutation.possiblyChangeVal(old));
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
        
        /// <param name="coherenceToOriginal">The (extra) change to be 0. If it is 1 this always returns 0</param>
        /// <returns></returns>
        public int newVal(double coherenceToOriginal)
        {
            if (!happens(coherenceToOriginal)) return 0;
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

        internal E possiblyChangeVal(E oldVal, double coherence)
        {
            if (!happens(coherence)) return oldVal;
            return EvolutionAlgorithm.getElementExcept(vals, oldVal);
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
                for (int i = 0; i < ps.Length; i++)
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

        public int happens(double coherenceToLastValue)
        {
            double r = (1) * coherenceToLastValue
                + (1 - coherenceToLastValue) * EvolutionAlgorithm.random.NextDouble();
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
