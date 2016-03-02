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
            Morphology initialMorphology = Morphology.test1();

            Creature.Create(initialMorphology);

            /* 
            Morphology initialMorphology;

            for (int i = 0; i < N; i++)
                creature[i] = Creature.Create(initialMorphology);
            */
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

