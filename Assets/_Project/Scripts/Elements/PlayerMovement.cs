using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OyuncuHareketi : MonoBehaviour
{
    [Header("Kamera Ayarları")]
    [Tooltip("Main Camera DEĞİL! Player'ın altındaki boş 'KameraPivotu' objesini buraya sürükle.")]
    public Transform oyuncuKamerasi;
    public float fareHassasiyeti = 2f;
    public bool imleciKilitle = true;
    private float _xRotasyonu = 0f;

    [Header("Hareket Ayarları")]
    public float yurumeHizi = 6f;
    public float kosmaHizi = 10f;
    public float havaKontrolCarpani = 0.5f; // Havadayken yön değiştirme gücü
    private Vector3 _hareketGirdisi;
    private bool _kosuyorMu;

    [Header("Zıplama & Fizik")]
    public float ziplamaHizi = 5f;
    public float dusmeHiziniArttir = 2.5f;
    public LayerMask ziplamaYuzeyi;
    public Transform zeminKontrolNoktasi;
    public float zeminKontrolYaricapi = 0.3f;
    private bool _zemindeMi;

    [Header("Etkileşim Ayarları (Raycast)")]
    public float etkilesimMesafesi = 5f;
    public LayerMask etkilesimYuzeyi;

    private Rigidbody _rb;
    private Camera _anaKamera;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
    }

    private void Start()
    {
        // Pivot'un altındaki gerçek Camera bileşenini otomatik bul
        if (oyuncuKamerasi != null)
        {
            _anaKamera = oyuncuKamerasi.GetComponentInChildren<Camera>();
        }
        if (_anaKamera == null)
        {
            _anaKamera = Camera.main;
        }

        if (imleciKilitle)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        // Escape ile cursor'ı serbest bırak
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Oyun alanına tıklayınca cursor'ı tekrar kilitle
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return; // Bu frame'de tıklama sadece kilitlesin, aynı anda etkileşim tetiklemesin
        }
        // Oyun içinde Esc ile imleci serbest bırak/kilitle (test için kullanışlı)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool kilitliMi = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = kilitliMi ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = kilitliMi;
        }

        // 1. Girdileri Topla
        float yatay = Input.GetAxisRaw("Horizontal");
        float dikey = Input.GetAxisRaw("Vertical");
        _hareketGirdisi = (transform.forward * dikey + transform.right * yatay).normalized;

        _kosuyorMu = Input.GetKey(KeyCode.LeftShift);

        // 2. Fare Bakışı (FPS Kamera) — sadece imleç kilitliyken çalışsın
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            FareBakisi();
        }

        // 3. Zemin kontrolü (her frame güncel tutulur)
        _zemindeMi = KarakterZemineDegiyorMu();

        // 4. Zıplama
        if (Input.GetKeyDown(KeyCode.Space) && _zemindeMi)
        {
            Ziplama();
        }

        // 5. Ekran Ortasından Etkileşim Kontrolü
        EtkilesimKontrolu();
    }

    private void FixedUpdate()
    {
        float aktifHiz = _kosuyorMu ? kosmaHizi : yurumeHizi;
        Vector3 hedefHiz = _hareketGirdisi * aktifHiz;

        Vector3 mevcutHiz = _rb.linearVelocity;

        if (_zemindeMi)
        {
            mevcutHiz.x = hedefHiz.x;
            mevcutHiz.z = hedefHiz.z;
        }
        else
        {
            // Havadayken tam kontrol yerine kısmi kontrol (daha gerçekçi his)
            mevcutHiz.x = Mathf.Lerp(mevcutHiz.x, hedefHiz.x, havaKontrolCarpani);
            mevcutHiz.z = Mathf.Lerp(mevcutHiz.z, hedefHiz.z, havaKontrolCarpani);
        }

        if (mevcutHiz.y < 0)
        {
            mevcutHiz.y -= dusmeHiziniArttir * Time.fixedDeltaTime;
        }

        _rb.linearVelocity = mevcutHiz;
    }

    private void FareBakisi()
    {
        float fareX = Input.GetAxis("Mouse X") * fareHassasiyeti;
        float fareY = Input.GetAxis("Mouse Y") * fareHassasiyeti;

        _xRotasyonu -= fareY;
        _xRotasyonu = Mathf.Clamp(_xRotasyonu, -85f, 85f);

        if (oyuncuKamerasi != null)
        {
            oyuncuKamerasi.localRotation = Quaternion.Euler(_xRotasyonu, 0f, 0f);
        }

        transform.Rotate(Vector3.up * fareX);
    }

    private void EtkilesimKontrolu()
    {
        if (_anaKamera == null) return;

        Ray isin = _anaKamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit carpanObje;

        Debug.DrawRay(isin.origin, isin.direction * etkilesimMesafesi, Color.red);

        if (Physics.Raycast(isin, out carpanObje, etkilesimMesafesi, etkilesimYuzeyi))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Etkileşime geçilen nesne: " + carpanObje.transform.name);

                Kapi kapi = carpanObje.collider.GetComponentInParent<Kapi>();
                if (kapi != null)
                {
                    // Kapı durumuna göre aç/kapa: açıksa kapat, kapalıysa aç
                    if (kapi.durum == Kapi.KapiDurumu.Acik)
                        kapi.Kapa();
                    else
                        kapi.Ac();
                }
            }
        }
    }

    private bool KarakterZemineDegiyorMu()
    {
        Vector3 kontrolPozisyonu = zeminKontrolNoktasi != null ? zeminKontrolNoktasi.position : transform.position;
        return Physics.CheckSphere(kontrolPozisyonu, zeminKontrolYaricapi, ziplamaYuzeyi);
    }

    private void Ziplama()
    {
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, ziplamaHizi, _rb.linearVelocity.z);
    }

    private void OnDrawGizmosSelected()
    {
        if (zeminKontrolNoktasi != null)
        {
            Gizmos.color = _zemindeMi ? Color.green : Color.red;
            Gizmos.DrawWireSphere(zeminKontrolNoktasi.position, zeminKontrolYaricapi);
        }
    }
}