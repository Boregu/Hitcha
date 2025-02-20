using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class XpSystem : MonoBehaviour
{
    public int level = 1;
    private int levelUpPoints = 0;

    [Header("XP Threshold = Startvärdet för Antal XP som Krävs för att Levla Upp")]
    [SerializeField] public float xpThreshold = 100;
    [SerializeField] private float xpThresholdScaling = 1.2f;

    public float xp;

    public UnityEvent onXpChange;
    public UnityEvent onLevelUp;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            GiveXP(100, 2.5f);
        }
    }
    public void GiveXP(float giveAmmount)
    {
        xp += giveAmmount;

        while (xp >= xpThreshold)
        {
            level += 1;
            levelUpPoints += 1;

            xp -= xpThreshold;
            xpThreshold *= xpThresholdScaling;

            onLevelUp.Invoke();

            print("Level Up! New Level is " + level + "!");
        }

        onXpChange.Invoke();
    }

    public void GiveXP(float totalgiveAmmount, float applyDurationInSeconds)
    {
        StartCoroutine(ApplyXPOverTime(totalgiveAmmount, applyDurationInSeconds));
    }

    private IEnumerator ApplyXPOverTime(float totalgiveAmmount, float applyDurationInSeconds)
    {
        float timesToLoop = applyDurationInSeconds * 10;
        float xpSpill = timesToLoop - Mathf.Floor(timesToLoop);
        timesToLoop = Mathf.Floor(timesToLoop);

        for (int i = 0; i < timesToLoop; i++)
        {
            xp += totalgiveAmmount / timesToLoop;

            while (xp >= xpThreshold)
            {
                level += 1;
                levelUpPoints += 1;

                xp -= xpThreshold;
                xpThreshold *= xpThresholdScaling;

                onLevelUp.Invoke();

                print("Level Up! New Level is " + level + "!");
            }

            onXpChange.Invoke();
            yield return new WaitForSeconds(0.1f);
        }
        GiveXP(xpSpill * (totalgiveAmmount / timesToLoop));
    }
}
