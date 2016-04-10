using System;
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
        /// <summary>
        /// Print the full network values of the ith creature for each generation
        /// </summary>
        public static readonly int WRITE_NETWORK_FLOATS_OF;
        private static readonly bool PRINT_EVERY_POPULATION;

        static Util()
        {
            //DEBUG = System.Diagnostics.Debugger.IsAttached;
            DEBUG = true;

            Debug.Log("Util.DEBUG variable is " + DEBUG);

            PRINT_MUTATION_OF = 1;
            WRITE_NETWORK_FLOATS_OF = 0;
            if (DEBUG)
            {
                PAUSE_AFTER_CREATURE_INITIALIZATION = false;
                PRINT_EVERY_POPULATION = true;

                INITIAL_POPULATION_SIZE = 1000;
                INITIAL_EVALUATION_TIME = 2.0f;
                FITNESS_EVALUATION_TIME = 10.0f;
            }
            else
            {
                PAUSE_AFTER_CREATURE_INITIALIZATION = false;
                PRINT_EVERY_POPULATION = false;

                INITIAL_POPULATION_SIZE = 1000;
                INITIAL_EVALUATION_TIME = 1.0f;
                FITNESS_EVALUATION_TIME = 100.0f;
            }
        }

        public static string STARTTIME = DateTime.Now.ToString("MMddHHmmss");

        public static void tryPrintEveryPopulation(Morphology morphology, int sequenceNumber, int populationNumber)
        {
            if (!Util.PRINT_EVERY_POPULATION)
            {
                return;
            }
            string filename = "stats/populations" + Util.STARTTIME + "/POP_" + populationNumber + "_" + sequenceNumber + ".gv";
            write(filename, DotParser.parse(morphology));
        }

        internal static void tryPrintMutation(string stage, Morphology morphology, int sequenceNumber, int populationNumber)
        {
            if(sequenceNumber != Util.PRINT_MUTATION_OF)
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
