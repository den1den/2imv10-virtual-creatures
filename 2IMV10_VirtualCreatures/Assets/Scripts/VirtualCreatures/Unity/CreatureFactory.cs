using UnityEngine;
using System.Collections;


namespace VirtualCreatures
{
    public class CreatureFactory : MonoBehaviour
    {
        // Population size
        public const int Population = 100;

        private Creature[] creatures = new Creature[Population];

        private float TimeCount;

        // Use this for initialization
        void Start()
        {
            Creature.Create(Morphology.testSnake());
            Debug.Break();
        }

        bool pauzed = false;

        // Update is called once per frame
        void Update()
        {
            // Wait 10 seconds to update population
            WaitToUpdate(10);
        }

        /// <summary>
        /// Wait a certain time to update population
        /// </summary>
        /// <param name="TimeInSeconds"></param>
        private Morphology[] WaitToUpdate(uint TimeInSeconds)
        {
            TimeCount += Time.deltaTime;

            if(TimeCount >= TimeInSeconds)
            {
                TimeCount = 0;
                Debug.Log("Call EvolutionaryAlgorithm");
                //return EvolutionAlgorithm.CreateMorphology(creatures);
            }

            // Extra logic
            return null;
        }
    }
}

