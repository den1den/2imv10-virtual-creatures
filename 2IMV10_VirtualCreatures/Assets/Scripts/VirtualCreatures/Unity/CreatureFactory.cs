using UnityEngine;
using System.Collections;

namespace VirtualCreatures
{
    public class PhenotypeFactory : MonoBehaviour
    {

        public static Creature createNewCreature(Morphology morphology)
        {
            processNode(morphology.root);
            processJointRec(morphology.root, morphology.edges);

            throw new NotImplementedException();
        }

        public static void processJointRec(Node src, IList<EdgeMorph> edgelist)
        {
            IEnumerator<EdgeMorph> it = edgelist.GetEnumerator();
            while (it.MoveNext())
            {
                EdgeMorph e = it.Current;
                if (e.source == src)
                {
                    it.Dispose();
                    processNode(e.destination);
                    processJoint(src, e, e.destination);
                    processJointRec(e.destination, edgelist);
                }
            }
        }

        private static void processNode(Node node)
        {

        }

        private static processJoint(Node a, EdgeMorph edge, Node b)
        {

        }
    }
}
