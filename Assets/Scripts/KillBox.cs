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

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySound(AudioManager.Instance.knockoutSound);

            Destroy(target);
        }
        else if (target.CompareTag("Player"))
        {
            GameManager.Instance.LoseLife();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySound(AudioManager.Instance.loseLifeSound);

            if (GameManager.Instance.CurrentStatus == GameManager.GameStatus.Playing)
            {
                PlayerRespawn respawn = target.GetComponent<PlayerRespawn>();
                if (respawn != null)
                    respawn.Respawn();
            }
        }
    }
}