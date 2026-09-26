using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro kullanıyorsan bu satırı aktif bırak
using System.Collections;

[RequireComponent(typeof(Collider))]
public class Etkileşimli : MonoBehaviour, IAnlatimDurdurulabilir
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

    [Header("Yükselen Obje Ayarları")]
    public bool objeYukselsin = false;
    public Transform yukselecekObje;              // Yukarı kaldırılacak obje (platform, sütun, vs.)
    public float yukselmeHizi = 1f;                // Birim/saniye
    public float yukselmeMesafesi = 3f;            // Ne kadar yukarı çıkacak (yerel Y ekseninde)
    public AudioClip yukselmeSesi;                 // Yükselirken çalacak animasyon/motor sesi
    [Range(0f, 1f)]
    public float yukselmeSesSeviyesi = 1f;
    public bool yukselmeSesiDongude = true;        // Ses, yükselme süresince loop çalsın mı

    [Header("Kapı Ayarları")]
    public bool kapiAcilsin = false;
    public Transform kapi;                         // Açılacak kapı objesi (pivot doğru yerde olmalı)
    public float kapiAcilmaAcisi = 90f;             // Kaç derece açılacak (yerel Y ekseni etrafında)
    public float kapiAcilmaHizi = 60f;              // Derece/saniye
    public AudioClip kapiAcilmaSesi;                // Kapı açılırken çalacak ses
    [Range(0f, 1f)]
    public float kapiAcilmaSesSeviyesi = 1f;
    public Transform kapiSesininCikacagiYer;        // Boş bırakılırsa kapı objesinin kendisinden çalar

    private bool calindiMi = false;
    private AudioSource sesKaynagi;
    private AudioSource yukselmeSesKaynagi;
    private AudioSource kapiSesKaynagi;
    private bool hareketBasladiMi = false;

    private void Awake()
    {
        if (sesinCikacagiYer == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
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

        // Yükselen obje için ayrı bir ses kaynağı (objenin üzerinde, onunla birlikte hareket eder)
        if (objeYukselsin && yukselecekObje != null)
        {
            yukselmeSesKaynagi = yukselecekObje.gameObject.AddComponent<AudioSource>();
            yukselmeSesKaynagi.playOnAwake = false;
            yukselmeSesKaynagi.spatialBlend = yonluSes ? 1f : 0f;
            yukselmeSesKaynagi.maxDistance = maksimumMesafe;
            yukselmeSesKaynagi.loop = yukselmeSesiDongude;
            yukselmeSesKaynagi.volume = yukselmeSesSeviyesi;
        }

        // Kapı açılma sesi için ayrı bir ses kaynağı (sesin çıkacağı yer seçilebilir)
        if (kapiAcilsin && kapi != null)
        {
            Transform kapiSesYeri = kapiSesininCikacagiYer != null ? kapiSesininCikacagiYer : kapi;
            kapiSesKaynagi = kapiSesYeri.gameObject.AddComponent<AudioSource>();
            kapiSesKaynagi.playOnAwake = false;
            kapiSesKaynagi.spatialBlend = yonluSes ? 1f : 0f;
            kapiSesKaynagi.maxDistance = maksimumMesafe;
            kapiSesKaynagi.volume = kapiAcilmaSesSeviyesi;
        }
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

        if (sesDosyasi == null)
        {
            Debug.LogWarning($"{gameObject.name}: Ses dosyası atanmamış!");
        }
        else
        {
            sesKaynagi.PlayOneShot(sesDosyasi, sesSeviyesi);
        }

        calindiMi = true;

        if (altyaziGoster)
            AltyaziGoster();

        // Yükselen obje ve kapı açma işlemini aynı anda başlat
        if (objeYukselsin && yukselecekObje != null && !hareketBasladiMi)
        {
            hareketBasladiMi = true;
            StartCoroutine(ObjeyiYukselt());
        }

        if (kapiAcilsin && kapi != null)
        {
            StartCoroutine(KapiyiAc());
        }
    }

    /// <summary>Başka bir trigger devreye girdiğinde bu trigger'ın anlatımını (ses+altyazı) keser.</summary>
    public void AnlatimiDurdur()
    {
        if (sesKaynagi != null && sesKaynagi.isPlaying)
            sesKaynagi.Stop();

        CancelInvoke(nameof(AltyaziGizle));
        AltyaziGizle();
    }

    private IEnumerator ObjeyiYukselt()
    {
        Vector3 baslangicPozisyonu = yukselecekObje.localPosition;
        Vector3 hedefPozisyon = baslangicPozisyonu + Vector3.up * yukselmeMesafesi;

        if (yukselmeSesi != null && yukselmeSesKaynagi != null)
        {
            yukselmeSesKaynagi.clip = yukselmeSesi;
            yukselmeSesKaynagi.Play();
        }

        while (Vector3.Distance(yukselecekObje.localPosition, hedefPozisyon) > 0.01f)
        {
            yukselecekObje.localPosition = Vector3.MoveTowards(
                yukselecekObje.localPosition,
                hedefPozisyon,
                yukselmeHizi * Time.deltaTime
            );
            yield return null;
        }

        yukselecekObje.localPosition = hedefPozisyon;

        if (yukselmeSesKaynagi != null && yukselmeSesKaynagi.isPlaying)
            yukselmeSesKaynagi.Stop();
    }

    private IEnumerator KapiyiAc()
    {
        if (kapiAcilmaSesi != null && kapiSesKaynagi != null)
        {
            kapiSesKaynagi.PlayOneShot(kapiAcilmaSesi, kapiAcilmaSesSeviyesi);
        }

        Quaternion baslangicRotasyonu = kapi.localRotation;
        Quaternion hedefRotasyon = baslangicRotasyonu * Quaternion.Euler(0f, kapiAcilmaAcisi, 0f);

        while (Quaternion.Angle(kapi.localRotation, hedefRotasyon) > 0.5f)
        {
            kapi.localRotation = Quaternion.RotateTowards(
                kapi.localRotation,
                hedefRotasyon,
                kapiAcilmaHizi * Time.deltaTime
            );
            yield return null;
        }

        kapi.localRotation = hedefRotasyon;
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