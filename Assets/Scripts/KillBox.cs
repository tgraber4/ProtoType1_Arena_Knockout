using UnityEngine;

public class KillBox : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        GameObject target = other.transform.root.gameObject;

        if (target.CompareTag("Enemy"))
        {
            //untag so extra colliders cant score twice
            target.tag = "Untagged";

            GameManager.Instance.EnemyDefeated();
            Destroy(target);
        }
        else if (target.CompareTag("Player"))
        {
            GameManager.Instance.LoseLife();

            if (GameManager.Instance.CurrentStatus == GameManager.GameStatus.Playing)
            {
                PlayerRespawn respawn = target.GetComponent<PlayerRespawn>();
                if (respawn != null)
                    respawn.Respawn();
            }
        }
    }
}