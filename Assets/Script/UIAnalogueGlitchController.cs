using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class UIAnalogGlitchController : MonoBehaviour
{
    [Header("Master Switch (0=off, 1=on)")]
    [Range(0f, 1f)] public float master = 1f;

    [Header("Scan Line Jitter")]
    [Range(0f, 1f)] public float scanLineJitter = 0f;

    [Header("Vertical Jump")]
    [Range(0f, 1f)] public float verticalJump = 0f;

    [Header("Horizontal Shake")]
    [Range(0f, 1f)] public float horizontalShake = 0f;

    [Header("Color Drift")]
    [Range(0f, 1f)] public float colorDrift = 0f;

    private Image _img;
    private Material _mat;
    private float _verticalJumpTime;

    void OnEnable()
    {
        _img = GetComponent<Image>();
        // 인스턴스 머티리얼 확보 (공유 머티리얼 오염 방지)
        if (_img.material != null)
            _img.material = new Material(_img.material);
        _mat = _img.material;
    }

    void Update()
    {
        if (_mat == null) return;

        // Master off면 전부 0으로
        float s = master;
        float sj = scanLineJitter * s;
        float vj = verticalJump * s;
        float hs = horizontalShake * s;
        float cd = colorDrift * s;

        // Kino 동작과 유사한 시간 누적
        _verticalJumpTime += Application.isPlaying ? Time.deltaTime * vj * 11.3f : 0f;

        // Kino 스타일 파라미터 구성
        float sl_thresh = Mathf.Clamp01(1.0f - sj * 1.2f);
        float sl_disp = 0.002f + Mathf.Pow(sj, 3) * 0.05f;

        _mat.SetVector("_ScanLineJitter", new Vector2(sl_disp, sl_thresh));
        _mat.SetVector("_VerticalJump", new Vector2(vj, _verticalJumpTime));
        _mat.SetFloat("_HorizontalShake", hs * 0.2f);
        _mat.SetVector("_ColorDrift", new Vector2(cd * 0.04f, Time.time * 606.11f));
    }
}