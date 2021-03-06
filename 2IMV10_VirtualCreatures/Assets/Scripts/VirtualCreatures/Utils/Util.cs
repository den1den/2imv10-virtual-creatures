﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VirtualCreatures
{
    public abstract class Util {
        Util() { throw new NotImplementedException(); }

        /// <summary>
        /// This is not defined for vectors but will do in this example
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b">Cannot have any component equal to 0</param>
        /// <returns></returns>
        public static Vector3 componentwiseDevision(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public static readonly bool DEBUG;
        public static readonly bool PAUSE_AFTER_CREATURE_INITIALIZATION;
        public static readonly int INITIAL_POPULATION_SIZE;
        public static readonly float INITIAL_EVALUATION_TIME;
        public static readonly float FITNESS_EVALUATION_TIME;
        /// <summary>
        /// Print the full mutation steps of the ith creature for each generation
        /// </summary>
        public static readonly int PRINT_MUTATION_OF;
        public static readonly bool PRINT_ONLY_LAST_MUTATION;
        /// <summary>
        /// Print the full network values of the ith creature for each generation
        /// </summary>
        public static readonly int WRITE_NETWORK_FLOATS_OF;
        private static readonly bool PRINT_EVERY_POPULATION;

        static Util()
        {
            //DEBUG = System.Diagnostics.Debugger.IsAttached;
            DEBUG = false;

            Debug.Log("Util.DEBUG variable is " + DEBUG);

            PRINT_ONLY_LAST_MUTATION = false;

            if (DEBUG)
            {
                PAUSE_AFTER_CREATURE_INITIALIZATION = false;
                PRINT_MUTATION_OF = 1;
                WRITE_NETWORK_FLOATS_OF = 0;
                PRINT_EVERY_POPULATION = true;

                INITIAL_POPULATION_SIZE = 500;
                INITIAL_EVALUATION_TIME = 2.0f;
                FITNESS_EVALUATION_TIME = 10.0f;
            }
            else
            {
                PAUSE_AFTER_CREATURE_INITIALIZATION = false;
                PRINT_ONLY_LAST_MUTATION = true;
                PRINT_MUTATION_OF = 0;
                PRINT_EVERY_POPULATION = true;
                WRITE_NETWORK_FLOATS_OF = -1;

                INITIAL_POPULATION_SIZE = 300;
                INITIAL_EVALUATION_TIME = 2.0f;
                FITNESS_EVALUATION_TIME = 10.0f;
            }
        }

        public static string STARTTIME = DateTime.Now.ToString("MMddHHmmss");
        public static string format = "F4";
        public static IFormatProvider provider = System.Globalization.CultureInfo.CreateSpecificCulture("nl-NL");

        static string[] fitnessHistoryHeader = new string[] { "Generation", "id", "Predessors", "Fitnesses" };
        static IList<string[]> fitnesshistory = new List<string[]>();
        public static void tryPrintEveryPopulation(IList<EvolutionAlgorithm.PopulationMember> newPop, int populationNumber)
        {
            if (!Util.PRINT_EVERY_POPULATION)
            {
                return;
            }
            //store
            for(int i = 0; i < newPop.Count; i++)
            {
                fitnesshistory.Add(new string[] {
                    populationNumber.ToString(),
                    newPop[i].id.ToString(),
                    String.Join(", ", newPop[i].parents.Select(p => p.id.ToString()).ToArray()),
                    newPop[i].fitness.ToString(Util.format, Util.provider)
                });
            }
            if(populationNumber % 20 == 0)
            {
                //write
                string filename = "stats/populations" + Util.STARTTIME + "/FITNESSES_" + populationNumber + ".csv";
                writeCSV(filename, fitnessHistoryHeader, fitnesshistory);
                fitnesshistory.Clear();
            }
        }

        internal static void tryPrintMutation(string stage, Morphology morphology, int sequenceNumber, int populationNumber)
        {
            if(sequenceNumber != Util.PRINT_MUTATION_OF)
            {
                return;
            }
            if(PRINT_ONLY_LAST_MUTATION && !stage.Equals("4-fix"))
            {
                return;
            }
            string filename = "stats/populations" + Util.STARTTIME + "/POP_" + populationNumber + "_" + sequenceNumber + "_MUTATION-"+ stage + ".gv";
            write(filename, DotParser.parse(morphology));
        }

        internal static ExplicitNN tryWrapNeuralNetwork(NaiveNN network, int sequenceNumber, int populationNumber)
        {
            if(sequenceNumber != Util.WRITE_NETWORK_FLOATS_OF)
            {
                return network;
            }
            string filename = "stats/populations" + Util.STARTTIME + "/POP_" + populationNumber + "_" + sequenceNumber + "_NNV.csv";
            return new NaiveNNDebugWrapper(network, filename);
        }

        public static void write(string filename, IEnumerable<string> contents)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filename));
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename))
            {
                foreach (string line in contents) { file.WriteLine(line); }
            }
        }

        public static void writeCSV(string filename, string[] header, IEnumerable<string[]> contents)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filename));
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename))
            {
                file.WriteLine(string.Join(";", header));
                foreach (string[] line in contents) { file.WriteLine(string.Join(";", line)); }
            }
        }
    }
}
