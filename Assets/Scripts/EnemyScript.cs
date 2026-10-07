using UnityEngine;

public class HazardZone : MonoBehaviour
{
    public GameManager gameManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Life Lost.");

            gameManager.LoseLife();

            
        }
    }
}