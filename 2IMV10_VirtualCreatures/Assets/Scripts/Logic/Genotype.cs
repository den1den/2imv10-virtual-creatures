using UnityEngine;
using System.Collections;

public class Genotype : MonoBehaviour {

    public float length = 5.0f;


	public Genotype()
    {
    }


    public void doSomething()
    {
        Genotype myGenotype = new Genotype();

        myGenotype.length = 2.0f;
    }
}
