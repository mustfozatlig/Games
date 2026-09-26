using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Altyazı yardımcıları: uzun metni ekrana sığan parçalara böler ve yazı kutusunu
/// (dolayısıyla arka plan panelini) metnin boyutuna göre ayarlar.
/// Trigger ve Etkileşimli ortak kullanır.
/// </summary>
public static class AltyaziYardimcisi
{
    /// <summary>
    /// Metni kelime kelime ekleyerek, her biri en fazla maksSatir satır tutan parçalara böler.
    /// </summary>
    public static List<string> Parcala(TextMeshProUGUI tmp, string metin, float maksGenislik, int maksSatir)
    {
        var parcalar = new List<string>();
        if (string.IsNullOrEmpty(metin)) return parcalar;

        maksSatir = Mathf.Max(1, maksSatir);
        tmp.textWrappingMode = TextWrappingModes.Normal;

        // Tek satırın yüksekliğini ölç; izin verilen yükseklik = maksSatir satır (+ yarım satır tolerans)
        float satirYuksekligi = tmp.GetPreferredValues("Ag", maksGenislik, 10000f).y;
        float sinir = satirYuksekligi * (maksSatir + 0.5f);

        string[] kelimeler = metin.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        string mevcut = "";

        foreach (string kelime in kelimeler)
        {
            string aday = mevcut.Length == 0 ? kelime : mevcut + " " + kelime;
            float yukseklik = tmp.GetPreferredValues(aday, maksGenislik, 10000f).y;

            if (mevcut.Length > 0 && yukseklik > sinir)
            {
                parcalar.Add(mevcut);   // Bu parça doldu, yeni parçaya geç
                mevcut = kelime;
            }
            else
            {
                mevcut = aday;
            }
        }

        if (mevcut.Length > 0) parcalar.Add(mevcut);
        return parcalar;
    }

    /// <summary>
    /// Parçayı yazar, yazı kutusunu metnin gerçek boyutuna getirir ve paneli yeniden hesaplatır.
    /// Panel (Layout Group + Content Size Fitter) kutunun boyutuna göre büyüyüp küçülür.
    /// </summary>
    public static void Goster(TextMeshProUGUI tmp, GameObject panel, string parca, float maksGenislik)
    {
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.text = parca;

        float tekSatirGenislik = tmp.GetPreferredValues(parca).x;
        float genislik = Mathf.Min(tekSatirGenislik, maksGenislik);
        float yukseklik = tmp.GetPreferredValues(parca, genislik, 10000f).y;
        tmp.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, genislik);
        tmp.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, yukseklik);

        // Content Size Fitter bazen aynı karede yeniden hesaplamıyor — zorla tetikle.
        RectTransform panelRect = panel != null ? panel.GetComponent<RectTransform>() : tmp.rectTransform.parent as RectTransform;
        if (panelRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
    }
}
