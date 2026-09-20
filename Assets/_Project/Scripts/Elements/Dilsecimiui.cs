using UnityEngine;

/// <summary>
/// Bir UI objesine (örn. boş bir "DilSecimi" GameObject veya Canvas) eklenir.
/// Buton OnClick() event'lerinden bu metotları çağır — statik DilYoneticisi
/// metotları doğrudan Inspector'a sürüklenemediği için bu köprü gerekiyor.
///
/// Kurulum:
/// 1) Sahnede boş bir obje oluştur (örn. "DilYoneticisiUI"), bu script'i ekle.
/// 2) İngilizce/Türkçe/Almanca butonlarının OnClick() listesine bu objeyi sürükle,
///    fonksiyon olarak IngilizceSec() / TurkceSec() / AlmancaSec() seç.
/// </summary>
public class DilSecimiUI : MonoBehaviour
{
    public void IngilizceSec() => DilYoneticisi.DilAyarla(DilYoneticisi.Dil.Ingilizce);
    public void BasitlestirilmisCinceSec() => DilYoneticisi.DilAyarla(DilYoneticisi.Dil.BasitlestirilmisCince);
    public void JaponcaSec() => DilYoneticisi.DilAyarla(DilYoneticisi.Dil.Japonca);
    public void KoreceSec() => DilYoneticisi.DilAyarla(DilYoneticisi.Dil.Korece);
    public void AlmancaSec() => DilYoneticisi.DilAyarla(DilYoneticisi.Dil.Almanca);
    public void FransizcaSec() => DilYoneticisi.DilAyarla(DilYoneticisi.Dil.Fransizca);
    public void IspanyolcaSec() => DilYoneticisi.DilAyarla(DilYoneticisi.Dil.Ispanyolca);
    public void BrezilyaPortekizcesiSec() => DilYoneticisi.DilAyarla(DilYoneticisi.Dil.BrezilyaPortekizcesi);
    public void RuscaSec() => DilYoneticisi.DilAyarla(DilYoneticisi.Dil.Rusca);
    public void ItalyancaSec() => DilYoneticisi.DilAyarla(DilYoneticisi.Dil.Italyanca);
    public void TurkceSec() => DilYoneticisi.DilAyarla(DilYoneticisi.Dil.Turkce);
    public void ArapcaSec() => DilYoneticisi.DilAyarla(DilYoneticisi.Dil.Arapca);

    // Şu an için test: Inspector'dan enum seçip anında dili değiştirmek istersen
    // bu alanı doldurup sağ tık > "Şimdi Uygula" yerine, Play modundayken
    // context menu ile tetikleyebilirsin (ContextMenu attribute sayesinde).
    [Header("Test (Play modunda Inspector'dan sağ tık > Uygula)")]
    public DilYoneticisi.Dil testDili = DilYoneticisi.Dil.Turkce;

    [ContextMenu("Test Dilini Uygula")]
    private void TestDiliniUygula()
    {
        DilYoneticisi.DilAyarla(testDili);
    }
}