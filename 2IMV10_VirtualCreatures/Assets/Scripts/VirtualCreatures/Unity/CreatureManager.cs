using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace VirtualCreatures
{
    public class CreatureManager : MonoBehaviour
    {
        float CreatureSpacing = 3;

        EvolutionAlgorithm EA;

        Vector3[] positioningGrid = new Vector3[0];
        Vector3[] initialCMs = null;

        CreatureController[] population = new CreatureController[0];
        
        // Use this for initialization
        void Start()
        {
            //Debug.Log("Debugging - Creating test morhologies and pausing...");
            //CreatureController.constructCreature(Morphology.testGiraffe().deepCopy(), new Vector3(0, -50, 0));
            //CreatureController.constructCreature(Morphology.testCircle(), new Vector3(30, -50, 0));
            //CreatureController.constructCreature(Morphology.testArc(), new Vector3(60, -50, 0));
            //CreatureController.constructCreature(Morphology.testSuperSwastika(), new Vector3(90, -50, 0));
            //CreatureController.constructCreature(Morphology.testHindge(), new Vector3(120, -50, 0));
            //Debug.Break();
            
            EA = new EvolutionAlgorithm1();

            EA.generateNewPopulation();

            createPopulation(EA.population);
        }

        private void createPopulation(EvolutionAlgorithm.PopulationMember[] newPopulation)
        {
            // Destroy last population
            int i;
            for (i = 0; i < population.Length; i++)
            {
                Destroy(population[i].gameObject);
            }
            population = new CreatureController[newPopulation.Length];

            // Check if grid is still valid
            if (positioningGrid.Length != newPopulation.Length)
            {
                positioningGrid = positionalGrid(Vector3.zero, newPopulation.Length, CreatureSpacing * EA.getCreatureSize());
            }

            // Construct new population
            for(i = 0; i < newPopulation.Length; i++)
            {
                Morphology m = newPopulation[i].morphology;
                population[i] = CreatureController.constructCreature(m, positioningGrid[i]);
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
                        Debug.Log("Creature initialization of "+ this.population.Length+" Creatures completed - pauzing simulation");
                        Debug.Break(); // pause simulation after settling stage of the create
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
                        EA.generateNewPopulation(population, fitness);
                        createPopulation(EA.population);
                        state = State.INITIAL;
                    }
                    break;
            }
            
        }

        private double[] evalFitness()
        {
            double[] r = new double[this.population.Count()];
            for (int i = 0; i < r.Length; i ++)
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

