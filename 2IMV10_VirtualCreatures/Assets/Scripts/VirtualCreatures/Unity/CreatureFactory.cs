using UnityEngine;
using System.Collections;

public class CreatureFactory : MonoBehaviour
{
    public int numberOfCreatures = 300;


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }







    /*public static Creature createNewCreature(Morphology morphology)
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

    }*/


}
