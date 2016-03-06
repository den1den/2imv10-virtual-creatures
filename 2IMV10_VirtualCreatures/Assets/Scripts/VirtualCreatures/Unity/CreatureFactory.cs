using UnityEngine;
using System.Collections;


namespace VirtualCreatures
{
    public class CreatureFactory : MonoBehaviour
    {

        public const int N = 100;

        Creature[] creature = new Creature[N];

        // Use this for initialization
        void Start()
        {
            Creature.CreateTest3();
            /* 
            Morphology initialMorphology;

            for (int i = 0; i < N; i++)
                creature[i] = Creature.Create(initialMorphology);
            */
        }

        bool pauzed = false;

        // Update is called once per frame
        void Update()
        {
            if (!pauzed)
            {
                pauzed = true;
                Debug.Break();
            }
        }
    }
}

