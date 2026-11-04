using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Respawn()
    {
        //clear falling momentum before moving
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = spawnPoint.position;
        transform.position = spawnPoint.position;
    }
}