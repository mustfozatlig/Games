using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro kullanıyorsan bu satırı aktif bırak

[RequireComponent(typeof(Collider))]
public class NarrationTrigger : MonoBehaviour, IAnlatimDurdurulabilir
{
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
    // Dil artık burada tutulmuyor — tüm sahnede ortak DilYoneticisi.GecerliDil kullanılıyor.

    [TextArea(2, 4)]
    public string altyaziIngilizce = "";
    [TextArea(2, 4)]
    public string altyaziBasitlestirilmisCince = "";
    [TextArea(2, 4)]
    public string altyaziJaponca = "";
    [TextArea(2, 4)]
    public string altyaziKorece = "";
    [TextArea(2, 4)]
    public string altyaziAlmanca = "";
    [TextArea(2, 4)]
    public string altyaziFransizca = "";
    [TextArea(2, 4)]
    public string altyaziIspanyolca = "";
    [TextArea(2, 4)]
    public string altyaziBrezilyaPortekizcesi = "";
    [TextArea(2, 4)]
    public string altyaziRusca = "";
    [TextArea(2, 4)]
    public string altyaziItalyanca = "";
    [TextArea(2, 4)]
    public string altyaziTurkce = "";
    [TextArea(2, 4)]
    public string altyaziArapca = "";

    public TextMeshProUGUI altyaziTextUI;
    [Tooltip("Opsiyonel: metnin içinde bulunduğu arka plan panel objesi (Content Size Fitter'lı). Atanırsa metinle birlikte açılıp kapanır; boş bırakılırsa sadece metin objesi açılıp kapanır.")]
    public GameObject altyaziPanel;
    // altyaziSuresi kaldırıldı, artık ses dosyasının uzunluğu kullanılıyor

    [Header("Kapı Kontrolü (opsiyonel — boş bırakılabilir)")]
    public Kapi acilacakKapi1;
    public Kapi acilacakKapi2;
    public Kapi kapatilipKilitlenecekKapi1;
    public Kapi kapatilipKilitlenecekKapi2;

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
        // Aynı anda tek anlatım olsun: başka bir trigger o sırada anlatıyorsa onu durdur.
        AnlatimYoneticisi.Basla(this);

        if (sesDosyasi != null)
        {
            sesKaynagi.PlayOneShot(sesDosyasi, sesSeviyesi);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: Ses dosyası atanmamış!");
        }

        calindiMi = true;

        if (altyaziGoster)
            AltyaziGoster();

        KapilariKontrolEt();
    }

    /// <summary>Başka bir trigger devreye girdiğinde bu trigger'ın anlatımını (ses+altyazı) keser.</summary>
    public void AnlatimiDurdur()
    {
        if (sesKaynagi != null && sesKaynagi.isPlaying)
            sesKaynagi.Stop();

        CancelInvoke(nameof(AltyaziGizle));
        AltyaziGizle();
    }

    private void KapilariKontrolEt()
    {
        // Her alan opsiyonel — atanmamışsa (null) sessizce atlanır, trigger yine çalışır.
        if (acilacakKapi1 != null) acilacakKapi1.Ac();
        if (acilacakKapi2 != null) acilacakKapi2.Ac();

        if (kapatilipKilitlenecekKapi1 != null)
        {
            kapatilipKilitlenecekKapi1.Kapa();
            kapatilipKilitlenecekKapi1.Kilitle();
        }
        if (kapatilipKilitlenecekKapi2 != null)
        {
            kapatilipKilitlenecekKapi2.Kapa();
            kapatilipKilitlenecekKapi2.Kilitle();
        }
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
        if (altyaziPanel != null) altyaziPanel.SetActive(true);

        // Content Size Fitter bazen aynı karede yeniden hesaplamıyor — zorla tetikle.
        RectTransform panelRect = altyaziPanel != null ? altyaziPanel.GetComponent<RectTransform>() : altyaziTextUI.rectTransform.parent as RectTransform;
        if (panelRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);

        // Altyazı süresi artık ses dosyasının uzunluğuna eşit
        float sure = sesDosyasi != null ? sesDosyasi.length : 4f;

        CancelInvoke(nameof(AltyaziGizle));
        Invoke(nameof(AltyaziGizle), sure);
    }

    private void AltyaziGizle()
    {
        if (altyaziTextUI != null)
            altyaziTextUI.gameObject.SetActive(false);
        if (altyaziPanel != null)
            altyaziPanel.SetActive(false);

        AnlatimYoneticisi.Bitti(this);
    }

    private string SeciliDilMetni()
    {
        switch (DilYoneticisi.GecerliDil)
        {
            case DilYoneticisi.Dil.Ingilizce:
                return altyaziIngilizce;
            case DilYoneticisi.Dil.BasitlestirilmisCince:
                return altyaziBasitlestirilmisCince;
            case DilYoneticisi.Dil.Japonca:
                return altyaziJaponca;
            case DilYoneticisi.Dil.Korece:
                return altyaziKorece;
            case DilYoneticisi.Dil.Almanca:
                return altyaziAlmanca;
            case DilYoneticisi.Dil.Fransizca:
                return altyaziFransizca;
            case DilYoneticisi.Dil.Ispanyolca:
                return altyaziIspanyolca;
            case DilYoneticisi.Dil.BrezilyaPortekizcesi:
                return altyaziBrezilyaPortekizcesi;
            case DilYoneticisi.Dil.Rusca:
                return altyaziRusca;
            case DilYoneticisi.Dil.Italyanca:
                return altyaziItalyanca;
            case DilYoneticisi.Dil.Turkce:
                return altyaziTurkce;
            case DilYoneticisi.Dil.Arapca:
                return altyaziArapca;
            default:
                return altyaziTurkce;
        }
    }
}