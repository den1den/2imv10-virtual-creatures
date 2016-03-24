using UnityEngine;
using System.Collections;


namespace VirtualCreatures
{
    public class CreatureFactory : MonoBehaviour
    {
        // Population size
        public const int Population = 100;

        private CreatureScript[] creatures = new CreatureScript[Population];

        private float TimeCount;

        // Use this for initialization
        void Start()
        {
            CreatureScript.construct(Morphology.testGiraffe(), new Vector3(0, 20, 0));
            CreatureScript.construct(Morphology.testCircle(), new Vector3(30, 20, 0));
            CreatureScript.construct(Morphology.testArc(), new Vector3(60, 20, 0));
            CreatureScript.construct(Morphology.testSuperSwastika(), new Vector3(90, 20, 0));
            CreatureScript.construct(Morphology.testHindge(), new Vector3(120, 20, 0));
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
                //return EvolutionAlgorithm.CreateMorphology(creatures);
            }

            // Extra logic
            return null;
        }
    }
}

