using System;

/// <summary>
/// Oyun genelinde tek bir "seçili dil" kaynağı. Her trigger kendi enum'unu tutmak
/// yerine buradaki GecerliDil'i okur. Arayüzden DilAyarla() çağrıldığında
/// DilDegisti event'i tetiklenir; o anda ekranda duran altyazı varsa anında
/// güncellenmesini istersen buna abone olabilirsin.
/// </summary>
public static class DilYoneticisi
{
    public enum Dil
    {
        Ingilizce,
        BasitlestirilmisCince,
        Japonca,
        Korece,
        Almanca,
        Fransizca,
        Ispanyolca,
        BrezilyaPortekizcesi,
        Rusca,
        Italyanca,
        Turkce,
        Arapca
    }

    public static Dil GecerliDil = Dil.Turkce;

    public static event Action<Dil> DilDegisti;

    public static void DilAyarla(Dil yeniDil)
    {
        if (GecerliDil == yeniDil) return;
        GecerliDil = yeniDil;
        DilDegisti?.Invoke(GecerliDil);
    }

    // Arayüzdeki butonlara doğrudan bağlayabilmek için parametresiz kısayollar
    public static void DilAyarlaIngilizce() => DilAyarla(Dil.Ingilizce);
    public static void DilAyarlaBasitlestirilmisCince() => DilAyarla(Dil.BasitlestirilmisCince);
    public static void DilAyarlaJaponca() => DilAyarla(Dil.Japonca);
    public static void DilAyarlaKorece() => DilAyarla(Dil.Korece);
    public static void DilAyarlaAlmanca() => DilAyarla(Dil.Almanca);
    public static void DilAyarlaFransizca() => DilAyarla(Dil.Fransizca);
    public static void DilAyarlaIspanyolca() => DilAyarla(Dil.Ispanyolca);
    public static void DilAyarlaBrezilyaPortekizcesi() => DilAyarla(Dil.BrezilyaPortekizcesi);
    public static void DilAyarlaRusca() => DilAyarla(Dil.Rusca);
    public static void DilAyarlaItalyanca() => DilAyarla(Dil.Italyanca);
    public static void DilAyarlaTurkce() => DilAyarla(Dil.Turkce);
    public static void DilAyarlaArapca() => DilAyarla(Dil.Arapca);
}