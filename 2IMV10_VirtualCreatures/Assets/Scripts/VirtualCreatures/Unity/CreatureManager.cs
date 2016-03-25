using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace VirtualCreatures
{
    public class CreatureManager : MonoBehaviour
    {
        EvolutionAlgorithm EA;

        Vector3[] positioningGrid;
        CreatureController[] population;

        Vector3[] initialCMs = null;
        
        // Use this for initialization
        void Start()
        {
            EA = new EvolutionAlgorithm1();
            int initalPopulationSize = EA.PopulationSize;
            positioningGrid = positionalGrid(Vector3.zero, initalPopulationSize, 3f * EA.getCreatureSize());
            population = new CreatureController[initalPopulationSize];

            // place initial population
            int i = 0;
            foreach(Morphology m in EA.generateInitialPopulation())
            {
                CreatureController c = CreatureController.constructCreature(m, positioningGrid[i]);
                i++;
            }
        }

        /// <summary>
        /// Keeps track of the time of the simulations
        /// </summary>
        private float TimeCount = 0;

        private State state = State.INITIAL;

        // Update is called once per frame
        void Update()
        {
            switch(this.state)
            {
                case State.INITIAL:
                    //keep track of initial movements
                    TimeCount += Time.deltaTime;
                    if(TimeCount >= EA.InitializationTime){
                        TimeCount = 0;
                        this.initialCMs = population.Select(cc => cc.getCenterOfMass()).ToArray();
                        state = State.EVALUATING;
                    }
                    break;
                case State.EVALUATING:
                    TimeCount += Time.deltaTime;
                    if (TimeCount >= EA.EvalUationTime)
                    {
                        TimeCount = 0;
                        double[] fitness = evalFitness();
                        int i = 0;
                        foreach (Morphology m in EA.generateNewPopulation(population, fitness))
                        {
                            population[i] = CreatureController.constructCreature(m, positioningGrid[i]);
                            i++;
                        }
                        state = State.INITIAL;
                    }
                    break;
            }
            
        }

        private double[] evalFitness()
        {
            double[] r = new double[this.population.Length];
            for (int i = 0; i < population.Length; i ++)
            {
                Vector3 delta = population[i].getCenterOfMass() - this.initialCMs[i];
                switch (EA.fitness)
                {
                    case Fitness.WALKING:
                        r[i] = delta.x * delta.x + delta.z * delta.z;
                        break;
                    case Fitness.SWIMMING:
                        r[i] = delta.x * delta.x + 1/2*Math.Sign(delta.y)*delta.y*delta.y + delta.z * delta.z;
                        break;
                }
            }
            return r;
        }

        static Vector3[] positionalGrid(Vector3 start, int totalSize, float spacing)
        {
            Vector3[] r = new Vector3[totalSize];
            int rowSize = (int)Math.Ceiling(Math.Sqrt(totalSize));
            for (int x = 0; x < rowSize; x++)
            {
                float xC = start.x + x * spacing;
                int i = x * rowSize;
                for (int z = 0; z < rowSize && i < totalSize;)
                {
                    float zC = start.z + z * spacing;
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

