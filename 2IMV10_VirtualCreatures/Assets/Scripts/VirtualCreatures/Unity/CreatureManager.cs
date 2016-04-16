using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace VirtualCreatures
{
    /// <summary>
    /// Unity class to manage a population of creatures
    /// </summary>
    public class CreatureManager : MonoBehaviour
    {
        int simulationNumber = 0;

        float CreatureSpacing = 3;
        float ConstructionHeight = 2.7f; //TODO: This is mere a gues

        EvolutionAlgorithm EA = new EvolutionAlgorithm1();

        Vector3[] positioningGrid = new Vector3[0];
        Vector3[] initialCMs = null;

        CreatureController[] population = new CreatureController[0];
        
        void Start()
        {
            EA.generateNewPopulation();
            constructPopulation();
        }

        private void constructPopulation()
        {
            // Destroy last population
            int i;
            for (i = 0; i < population.Length; i++)
            {
                Destroy(population[i].gameObject);
            }
            population = new CreatureController[EA.population.Length];

            // Check if grid is still valid
            if (positioningGrid.Length != EA.population.Length)
            {
                positioningGrid = positionalGrid(new Vector3(0, ConstructionHeight, 0), EA.population.Length, CreatureSpacing * EA.getCreatureSize());
            }

            // Construct new population
            for (i = 0; i < EA.population.Length; i++)
            {
                Morphology m = EA.population[i].morphology;
                CreatureController creatureController = CreatureController.constructCreature(m, positioningGrid[i], Quaternion.identity);
                creatureController.neuralNetwork = Util.tryWrapNeuralNetwork((NaiveNN)creatureController.neuralNetwork, i, EA.GenerationCount - 1);
                population[i] = creatureController;
            }
        }

        /// <summary>
        /// A timer to keep track of time
        /// </summary>
        private float TimeCount = 0;

        private State state = State.INITIAL;

        // Update is called once per frame
        void Update()
        {
            switch (this.state)
            {
                case State.INITIAL:
                    //keep track of initial movements
                    TimeCount += Time.deltaTime;
                    if (TimeCount >= EA.InitializationTime)
                    {
                        TimeCount = 0;
                        simulationNumber++;
                        if (Util.PAUSE_AFTER_CREATURE_INITIALIZATION)
                        {
                            Debug.Log("Creature initialization of " + this.population.Length + " Creatures completed (" + simulationNumber + ") - pauzing simulation");
                            Debug.Break(); // pause simulation after settling stage of the create
                        }
                        else
                        {
                            Debug.Log("Creature initialization of " + this.population.Length + " Creatures completed (" + simulationNumber + ")");
                        }
                        this.initialCMs = population.Select(cc => cc.getCenterOfMass()).ToArray();
                        state = State.EVALUATING;
                    }
                    break;
                case State.EVALUATING:
                    TimeCount += Time.deltaTime;
                    if (TimeCount >= EA.EvaluationTime)
                    {
                        TimeCount = 0;
                        double[] fitness = evalFitness();
                        EA.generateNewPopulation(population, fitness);
                        constructPopulation();
                        state = State.INITIAL;
                    }
                    break;
            }

        }

        private double[] evalFitness()
        {
            double[] r = new double[this.population.Count()];
            for (int i = 0; i < r.Length; i++)
            {
                Vector3 delta = population[i].getCenterOfMass() - this.initialCMs[i];
                switch (EA.fitness)
                {
                    case Fitness.WALKING:
                        r[i] = delta.x * delta.x + delta.z * delta.z;
                        break;
                    case Fitness.SWIMMING:
                        r[i] = delta.x * delta.x + 1 / 2 * Math.Sign(delta.y) * delta.y * delta.y + delta.z * delta.z;
                        break;
                }
            }
            return r;
        }

        static Vector3[] positionalGrid(Vector3 start, int totalSize, float spacing)
        {
            Vector3[] r = new Vector3[totalSize];
            int rowSize = (int)Math.Ceiling(Math.Sqrt(totalSize));
            float startX = start.x - rowSize / 2 * spacing;
            float startZ = start.z - rowSize / 2 * spacing;
            for (int x = 0; x < rowSize; x++)
            {
                float xC = startX + x * spacing;
                int i = x * rowSize;
                for (int z = 0; z < rowSize && i < totalSize;)
                {
                    float zC = startZ + z * spacing;
                    r[i] = new Vector3(xC, start.y, zC);
                    i = x * rowSize + ++z;
                }
            }
            return r;
        }

        private enum State
        {
            INITIAL,
            EVALUATING
        };
    }
}

