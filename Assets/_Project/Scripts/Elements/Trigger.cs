using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro kullanıyorsan bu satırı aktif bırak

[RequireComponent(typeof(Collider))]
public class NarrationTrigger : MonoBehaviour
{
    public enum AltyaziDili
    {
        Ingilizce,
        Turkce,
        Almanca
    }

    [Header("Ses Ayarları")]
    public AudioClip sesDosyasi;
    public Transform sesinCikacagiYer;

    [Header("Ayarlar")]
    [Range(0f, 1f)]
    public float sesSeviyesi = 1f;
    public bool sadeceBirKereCalsin = true;
    public bool sadeceOyuncuTetiklesin = true;
    public string oyuncuTagi = "Player";

    [Header("3D Ses Ayarları (opsiyonel)")]
    public bool yonluSes = false;
    [Range(0f, 50f)]
    public float maksimumMesafe = 20f;

    [Header("Altyazı Ayarları")]
    public bool altyaziGoster = true;
    public AltyaziDili seciliDil = AltyaziDili.Turkce;

    [TextArea(2, 4)]
    public string altyaziIngilizce = "";
    [TextArea(2, 4)]
    public string altyaziTurkce = "";
    [TextArea(2, 4)]
    public string altyaziAlmanca = "";

    public TextMeshProUGUI altyaziTextUI;
    // altyaziSuresi kaldırıldı, artık ses dosyasının uzunluğu kullanılıyor

    private bool calindiMi = false;
    private AudioSource sesKaynagi;

    private void Awake()
    {
        if (sesinCikacagiYer == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // örnek: player'ın "AgizNoktasi" isimli child'ını bul
                Transform hedef = player.transform.Find("Anlatıcı");
                sesinCikacagiYer = hedef != null ? hedef : player.transform;
            }
            else
            {
                sesinCikacagiYer = transform;
            }
        }


        if (sesinCikacagiYer == null)
            sesinCikacagiYer = transform;

        sesKaynagi = sesinCikacagiYer.gameObject.AddComponent<AudioSource>();
        sesKaynagi.playOnAwake = false;
        sesKaynagi.spatialBlend = yonluSes ? 1f : 0f;
        sesKaynagi.maxDistance = maksimumMesafe;
        sesKaynagi.volume = sesSeviyesi;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (sadeceOyuncuTetiklesin && !other.CompareTag(oyuncuTagi))
            return;

        if (sadeceBirKereCalsin && calindiMi)
            return;

        SesiCal();
    }

    public void SesiCal()
    {
        if (sesDosyasi == null)
        {
            Debug.LogWarning($"{gameObject.name}: Ses dosyası atanmamış!");
            return;
        }

        sesKaynagi.PlayOneShot(sesDosyasi, sesSeviyesi);
        calindiMi = true;

        if (altyaziGoster)
            AltyaziGoster();
    }

    private void AltyaziGoster()
    {
        if (altyaziTextUI == null)
        {
            Debug.LogWarning($"{gameObject.name}: Altyazı Text UI atanmamış!");
            return;
        }

        string metin = SeciliDilMetni();

        if (string.IsNullOrEmpty(metin))
            return;

        altyaziTextUI.text = metin;
        altyaziTextUI.gameObject.SetActive(true);

        // Altyazı süresi artık ses dosyasının uzunluğuna eşit
        float sure = sesDosyasi != null ? sesDosyasi.length : 4f;

        CancelInvoke(nameof(AltyaziGizle));
        Invoke(nameof(AltyaziGizle), sure);
    }

    private void AltyaziGizle()
    {
        if (altyaziTextUI != null)
            altyaziTextUI.gameObject.SetActive(false);
    }

    private string SeciliDilMetni()
    {
        switch (seciliDil)
        {
            case AltyaziDili.Ingilizce:
                return altyaziIngilizce;
            case AltyaziDili.Turkce:
                return altyaziTurkce;
            case AltyaziDili.Almanca:
                return altyaziAlmanca;
            default:
                return altyaziTurkce;
        }
    }

    public void DilDegistir(AltyaziDili yeniDil)
    {
        seciliDil = yeniDil;
    }
}