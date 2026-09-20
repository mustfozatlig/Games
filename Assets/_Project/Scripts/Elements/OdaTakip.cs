using UnityEngine;

/// <summary>
/// Bir oda/alanın collider'ına eklenir (Is Trigger işaretli). Oyuncu bu alanın
/// içinde mi değil mi, OyuncuIceride ile her an okunabilir. Kapi.cs gibi
/// scriptler, "bu davranış sadece oyuncu odadayken olsun" diye buna bakar.
/// </summary>
[RequireComponent(typeof(Collider))]
public class OdaTakip : MonoBehaviour
{
    public string oyuncuTagi = "Player";

    public bool OyuncuIceride { get; private set; } = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(oyuncuTagi))
            OyuncuIceride = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(oyuncuTagi))
            OyuncuIceride = false;
    }
}