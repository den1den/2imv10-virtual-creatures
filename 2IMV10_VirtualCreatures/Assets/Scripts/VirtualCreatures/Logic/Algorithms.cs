using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualCreatures
{
    public class Main
    {
        public static void main(String[] args)
        {
            while (true)
            {
                Morphology testMorhology = Morphology.test1();

                //MyUnityFactory factory = new MyUnityFactory(); //this might need a scene to construct right???

                SelectionAlgorithm sa = null;
                MutationAlgorithm ma = null;
                Fitness fitness = Fitness.WALKING;
                List<IPhenotype> population = new List<IPhenotype>(new IPhenotype[] { /*IPhenotype.createNew(factory, testMorhology) */});

                EvolutionAlgorithm algorithm = new EvolutionAlgorithm(sa, ma, fitness, population);
            }
        }
    }

    public enum Fitness
    {
        WALKING, SWIMMING
    }

    public class EvolutionAlgorithm
    {
        protected Fitness fitness;
        protected MutationAlgorithm mutAlg;
        protected SelectionAlgorithm selectAlg;
        protected ICollection<IPhenotype> population;

        public Boolean cancelled = false;

        public EvolutionAlgorithm(SelectionAlgorithm selectAlg, MutationAlgorithm mutAlg, Fitness fitness, ICollection<IPhenotype> population)
        {
            this.selectAlg = selectAlg;
            this.mutAlg = mutAlg;
            this.fitness = fitness;
            this.population = population;
        }

        Executor executor = new Executor();
        public void run()
        {
            while (!cancelled) {
                foreach (IPhenotype phenotype in population)
                {
                    executor.submit(createTask(phenotype));
                }
                selectAlg.select(population);
                mutAlg.mutate(population);
            }
        }

        private int maxTicks = 100;
        private double dt = 0.1;
        protected Task<float> createTask(IPhenotype phenotype)
        {
            //run unity without a screen
            throw new NotImplementedException();
        }

        public void setPopulation(ICollection<IPhenotype> population)
        {
            this.population = population;
        }
    }

    public abstract class SelectionAlgorithm
    {
        public abstract void select(ICollection<IPhenotype> population);
    }

    public abstract class MutationAlgorithm
    {
        /// <summary>
        /// Mutate and crossover. Is called ONCE per generation.
        /// </summary>
        /// <param name="population"></param>
        public abstract void mutate(ICollection<IPhenotype> population);
    }



    public class Task<T> { } //TODO: replace by some libary

    internal class Executor
    { //TODO: replace by some libary
        public void submit<T>(Task<T> t) { }
    }
}
