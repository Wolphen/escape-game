using UnityEngine;
using UnityEngine.Events;

public class LaserReceiver : MonoBehaviour
{
    [Header("Indicator")]
    [SerializeField] private Renderer indicator;
    [SerializeField] private Color offColor = Color.red;
    [SerializeField] private Color onColor = Color.green;

    [Header("Puzzle")]
    [Tooltip("The laser power (1 to 10) needed to validate the receiver")]
    [SerializeField, Range(1, 10)] private int requiredPower = 7;

    public UnityEvent onActivated;

    private bool isHit;
    private int beamPower;
    private bool activated;

    private void Awake()
    {
        if (indicator != null)
        {
            if (indicator.sharedMaterial != null)
                indicator.material.EnableKeyword("_EMISSION");

            SetIndicatorColor(offColor);
        }
    }

    // Called by the LaserEmitter while the laser hits the receiver or when it stops hitting it
    public void SetHit(bool hit, int power)
    {
        isHit = hit;
        beamPower = power;
    }

    private void Update()
    {
        if (activated) return;

        // Validated as soon as the laser hits the receiver with the right power
        if (isHit && beamPower == requiredPower)
        {
            Debug.Log("Receiver hit");
            activated = true;
            SetIndicatorColor(onColor);
            onActivated.Invoke();
        }
    }

    private void SetIndicatorColor(Color color)
    {
        if (indicator == null) return;

        if (indicator is LineRenderer laser)
        {
            laser.startColor = color;
            laser.endColor = color;
        }

        // Also colors the material when it has an emission (ex: Lit shader), so the color is visible either way
        if (indicator.sharedMaterial != null && indicator.sharedMaterial.HasProperty("_EmissionColor"))
            indicator.material.SetColor("_EmissionColor", color);
    }
}