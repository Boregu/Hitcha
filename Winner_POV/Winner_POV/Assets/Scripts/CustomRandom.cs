using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRandom : MonoBehaviour
{
    public static int WeightedRandom(int[] spawnWeights)
    {
        int totalWeight = 0;

        foreach (int i in spawnWeights)
        {
            totalWeight += i;
        }

        int randomNumber = Random.Range(0, totalWeight);
        totalWeight = 0;
        int loops = 0;

        foreach(int i in spawnWeights)
        {
            totalWeight += i;

            if (randomNumber  < totalWeight)
            {
                return loops;
            }
            else
            {
                loops++;
            }
        }
        return -1;
    }
}
