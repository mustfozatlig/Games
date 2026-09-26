using UnityEngine;

public class Player : MonoBehaviour
{
    public void RestartPlayer()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Rigidbody varken sadece transform'u taşımak yetmez: hız da sıfırlanmalı
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = Vector3.zero;
        }
        transform.position = Vector3.zero;
    }
}
