using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Bağımsız kapı bileşeni. Kendi açık/kapalı/kilitli durumunu, dönme animasyonunu
/// ve kendi seslerini yönetir. Dışarıdan sadece Ac() / Kapa() / Kilitle() çağrılır.
/// Hangi objenin bu kapıyı ne zaman tetikleyeceğiyle hiç ilgilenmez — o iş Tetikleyici.cs'de.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Kapi : MonoBehaviour
{
    public enum KapiDurumu
    {
        Acik,
        Kapali,
        Kilitli   // Kilitlendikten sonra Ac()/Kapa() hiçbir şey yapmaz
    }

    [Header("Durum")]
    public KapiDurumu durum = KapiDurumu.Kapali;

    [Header("Hareket Ayarları")]
    public float aciliAcisi = 90f;
    public float hareketHizi = 3f;

    [Header("Sesler")]
    [Range(0f, 1f)]
    public float sesSeviyesi = 1f;
    public AudioClip acilmaSesi;
    public AudioClip kapanmaSesi;
    public AudioClip kilitliSesi; // Kilitliyken Ac()/Kapa() denenirse çalar

    [Header("Ses Çıkış Noktası (boş bırakılırsa kapının kendisi kullanılır)")]
    public Transform sesCikisNoktasi;

    [Header("İlk Kapanışta Otomatik Yeniden Açılma")]
    [Tooltip("İşaretlenirse: kapı ilk kez kapandığında bir süre sonra kendisi tekrar açılır (örn. anlatıcı sesi bitince).")]
    public bool ilkKapanistaOtomatikAcil = false;
    [Tooltip("Otomatik açılmadan önce beklenecek süre (saniye). anlaticiSesi atanmışsa onun uzunluğu kullanılır, bu alan yok sayılır.")]
    public float anlaticiSuresi = 5f;
    [Tooltip("Opsiyonel: süre yerine bu klibin uzunluğu kullanılsın (örn. o an çalan anlatıcı sesiyle aynı klip).")]
    public AudioClip anlaticiSesi;

    [Header("İkinci (Kalıcı) Kapanışta İndirilecek Obje")]
    public Transform hareketEdecekObje;
    public float inisMiktari = 1f;
    public float inisHizi = 1.5f;
    public AudioClip inisSesi;
    public Transform inisSesCikisNoktasi;

    [Header("Trigger Zincirleme (opsiyonel)")]
    [Tooltip("Kapı ilk kez kapandığında tetiklenir (örn. anlatıcı trigger'ını çalıştırmak için).")]
    public UnityEvent ilkKapanistaTetiklenecek;
    [Tooltip("Kapı ikinci kez (kalıcı olarak) kapandığında tetiklenir.")]
    public UnityEvent kaliciKapanistaTetiklenecek;

    [Header("Oda Kontrolü (opsiyonel)")]
    [Tooltip("Atanırsa: ilk/kalıcı kapanış davranışı (tetiklenecekler, otomatik açılma, obje indirme) SADECE oyuncu bu odanın içindeyken çalışır. Boş bırakılırsa kontrol yapılmaz, her zaman çalışır.")]
    public OdaTakip odaTakip;

    private int _kapanmaSayisi = 0;
    private Coroutine _aktifOtomatikAcilma;
    private Coroutine _aktifObjeHareketi;
    private AudioSource _inisAudioSource;
    private Vector3 _objeHedefPozisyonu;

    private AudioSource _audioSource;
    private Quaternion _acikRotasyon;
    private Quaternion _kapaliRotasyon;
    private Coroutine _aktifDonme;

    private void Awake()
    {
        Transform sesYeri = sesCikisNoktasi != null ? sesCikisNoktasi : transform;
        _audioSource = sesYeri.GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = sesYeri.gameObject.AddComponent<AudioSource>();

        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 1f;
        _audioSource.rolloffMode = AudioRolloffMode.Linear;
        _audioSource.minDistance = 1f;
        _audioSource.maxDistance = 15f;

        if (inisSesCikisNoktasi != null)
        {
            _inisAudioSource = inisSesCikisNoktasi.GetComponent<AudioSource>();
            if (_inisAudioSource == null) _inisAudioSource = inisSesCikisNoktasi.gameObject.AddComponent<AudioSource>();
            _inisAudioSource.playOnAwake = false;
            _inisAudioSource.spatialBlend = 1f;
            _inisAudioSource.rolloffMode = AudioRolloffMode.Linear;
            _inisAudioSource.minDistance = 1f;
            _inisAudioSource.maxDistance = 15f;
        }
    }

    private void Start()
    {
        // Başlangıç durumuna göre "açık" ve "kapalı" rotasyonlarını hesapla
        if (durum == KapiDurumu.Kapali || durum == KapiDurumu.Kilitli)
        {
            _kapaliRotasyon = transform.localRotation;
            _acikRotasyon = _kapaliRotasyon * Quaternion.Euler(0f, aciliAcisi, 0f);
        }
        else
        {
            _acikRotasyon = transform.localRotation;
            _kapaliRotasyon = _acikRotasyon * Quaternion.Euler(0f, -aciliAcisi, 0f);
        }

        if (hareketEdecekObje != null)
        {
            _objeHedefPozisyonu = hareketEdecekObje.localPosition + Vector3.down * inisMiktari;
        }
    }

    public void Ac()
    {
        if (durum == KapiDurumu.Kilitli)
        {
            CalKilitliSesi();
            return;
        }
        if (durum == KapiDurumu.Acik) return;

        durum = KapiDurumu.Acik;
        CalSes(acilmaSesi);
        DonmeyiBaslat(_acikRotasyon);
    }

    public void Kapa()
    {
        if (durum == KapiDurumu.Kilitli)
        {
            CalKilitliSesi();
            return;
        }
        if (durum == KapiDurumu.Kapali) return;

        durum = KapiDurumu.Kapali;
        CalSes(kapanmaSesi);
        DonmeyiBaslat(_kapaliRotasyon);

        // Oyuncu odanın içinde değilse (ve oda kontrolü atanmışsa) özel davranışı hiç çalıştırma —
        // kapı sadece normal şekilde kapanır, sayaç ilerlemez, hiçbir event tetiklenmez.
        bool oyuncuOdadaMi = odaTakip == null || odaTakip.OyuncuIceride;
        if (!oyuncuOdadaMi) return;

        _kapanmaSayisi++;

        if (_kapanmaSayisi == 1)
        {
            ilkKapanistaTetiklenecek?.Invoke();

            if (ilkKapanistaOtomatikAcil)
            {
                if (_aktifOtomatikAcilma != null) StopCoroutine(_aktifOtomatikAcilma);
                _aktifOtomatikAcilma = StartCoroutine(BeklemeSonrasiAc());
            }
        }
        else
        {
            // İkinci (kalıcı) kapanış
            if (_aktifOtomatikAcilma != null) StopCoroutine(_aktifOtomatikAcilma);

            durum = KapiDurumu.Kilitli;
            kaliciKapanistaTetiklenecek?.Invoke();

            if (hareketEdecekObje != null)
            {
                CalInisSesi();
                if (_aktifObjeHareketi != null) StopCoroutine(_aktifObjeHareketi);
                _aktifObjeHareketi = StartCoroutine(ObjeyiIndir());
            }
        }
    }

    private IEnumerator BeklemeSonrasiAc()
    {
        float sure = anlaticiSesi != null ? anlaticiSesi.length : anlaticiSuresi;
        yield return new WaitForSeconds(sure);

        if (durum == KapiDurumu.Kilitli) yield break; // Bu arada kalıcı kapanmış olabilir
        Ac();
    }

    private void CalInisSesi()
    {
        if (inisSesi != null && _inisAudioSource != null)
            _inisAudioSource.PlayOneShot(inisSesi, sesSeviyesi);
    }

    private IEnumerator ObjeyiIndir()
    {
        while (Vector3.Distance(hareketEdecekObje.localPosition, _objeHedefPozisyonu) > 0.01f)
        {
            hareketEdecekObje.localPosition = Vector3.Lerp(hareketEdecekObje.localPosition, _objeHedefPozisyonu, Time.deltaTime * inisHizi);
            yield return null;
        }
        hareketEdecekObje.localPosition = _objeHedefPozisyonu;
    }

    /// <summary>Kapıyı mevcut konumunda kalıcı olarak kilitler; artık Ac()/Kapa() işe yaramaz.</summary>
    public void Kilitle()
    {
        durum = KapiDurumu.Kilitli;
    }

    private void CalSes(AudioClip klip)
    {
        if (klip != null && _audioSource != null)
            _audioSource.PlayOneShot(klip, sesSeviyesi);
    }

    private void CalKilitliSesi()
    {
        CalSes(kilitliSesi);
    }

    private void DonmeyiBaslat(Quaternion hedef)
    {
        if (_aktifDonme != null) StopCoroutine(_aktifDonme);
        _aktifDonme = StartCoroutine(KapiyiDondur(hedef));
    }

    private IEnumerator KapiyiDondur(Quaternion hedefRotasyon)
    {
        while (Quaternion.Angle(transform.localRotation, hedefRotasyon) > 0.5f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, hedefRotasyon, Time.deltaTime * hareketHizi);
            yield return null;
        }
        transform.localRotation = hedefRotasyon;
    }
}