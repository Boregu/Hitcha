using UnityEngine;

public class chasecontrol : MonoBehaviour
{
    public fenemymove[] enemyarray;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            foreach (fenemymove enemy in enemyarray)
            {
                enemy.SetChase(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            foreach (fenemymove enemy in enemyarray)
            {
                enemy.SetChase(false);
            }
        }
    }
}
