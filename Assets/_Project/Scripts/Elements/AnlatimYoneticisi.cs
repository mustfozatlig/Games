/// <summary>
/// Sahnede aynı anda tek bir anlatım (ses+altyazı) çalışsın diye kullanılır.
/// Ses+altyazı oynatan her trigger (Etkileşimli, Trigger/NarrationTrigger vb.)
/// bu arayüzü uygular. Yeni bir trigger başladığında, o an aktif olan
/// öncekinin AnlatimiDurdur() metodu otomatik çağrılır.
/// </summary>
public interface IAnlatimDurdurulabilir
{
    void AnlatimiDurdur();
}

public static class AnlatimYoneticisi
{
    private static IAnlatimDurdurulabilir _aktifAnlatim;

    /// <summary>Bir trigger anlatıma başladığında çağırır. Önceki aktif anlatımı durdurur.</summary>
    public static void Basla(IAnlatimDurdurulabilir yeni)
    {
        if (_aktifAnlatim != null && _aktifAnlatim != yeni)
        {
            _aktifAnlatim.AnlatimiDurdur();
        }
        _aktifAnlatim = yeni;
    }

    /// <summary>Bir trigger kendi anlatımını doğal olarak bitirdiğinde (ses/altyazı bitti) çağırabilir.</summary>
    public static void Bitti(IAnlatimDurdurulabilir biten)
    {
        if (_aktifAnlatim == biten)
            _aktifAnlatim = null;
    }
}