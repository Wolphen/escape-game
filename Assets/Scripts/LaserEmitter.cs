using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserEmitter : MonoBehaviour
{
    [Header("Laser")]
    [SerializeField] private int maxBounces = 6;
    [SerializeField] private float maxDistance = 40f;
    [SerializeField] private LayerMask reflectionLayers;

    [Header("State")]
    [SerializeField] private bool startOn = false;
    [Tooltip("Components kept disabled until the laser is turned on (ex: the XR Knob, the mirror's XR Grab Interactable, ...)")]
    [SerializeField] private Behaviour[] enableWhenOn;

    [Header("Power")]
    [SerializeField, Range(1, 10)] private int powerLevel = 1;
    [SerializeField] private float minWidth = 0.2f;
    [SerializeField] private float maxWidth = 2f;
    [SerializeField] private TMP_Text powerDisplay;

    public int PowerLevel => powerLevel;

    private LineRenderer laser;
    private readonly List<Vector3> laserPoints = new List<Vector3>();
    private LaserReceiver currentReceiver;
    private bool isOn;

    private void Awake()
    {
        laser = GetComponent<LineRenderer>();
        SetOn(startOn);
    }

    private void Start()
    {
        ApplyPower();
    }

    
    public void TurnOn()
    {
        SetOn(true);
    }

    public void TurnOff()
    {
        SetOn(false);
    }

    public void Toggle()
    {
        SetOn(!isOn);
    }

    private void SetOn(bool on)
    {
        isOn = on;
        laser.enabled = on;

        foreach (Behaviour b in enableWhenOn)
            if (b != null) b.enabled = on;

        if (!on) ReleaseReceiver();
    }

 
    public void SetPowerFromKnob(float knobValue)
    {
        powerLevel = Mathf.Clamp(Mathf.RoundToInt(knobValue * 9f) + 1, 1, 10);
        ApplyPower();
    }

    private void ApplyPower()
    {
        if (laser == null) laser = GetComponent<LineRenderer>();

        float t = (powerLevel - 1) / 9f;
        laser.widthMultiplier = Mathf.Lerp(minWidth, maxWidth, t);

        if (powerDisplay != null)
            powerDisplay.text = $"PUISSANCE : {powerLevel:00} / 10";
    }

    private void Update()
    {
        if (!isOn) return;

        laserPoints.Clear();

        // Starting point of the laser
        Vector3 laserOrigin = transform.position;
        Vector3 laserDirection = transform.forward;

        laserPoints.Add(laserOrigin);

        LaserReceiver hitReceiver = null;

        for (int i = 0; i <= maxBounces; i++)
        {
            if (Physics.Raycast(laserOrigin, laserDirection, out RaycastHit hit, maxDistance, reflectionLayers))
            {

                laserPoints.Add(hit.point);

                if (hit.collider.CompareTag("Mirror"))
                {
                    // Bounces perpendicularly to the mirror's surface
                    laserDirection = Vector3.Reflect(laserDirection, hit.normal);
                    // Offset the origin so the laser doesn't hit the same mirror again
                    laserOrigin = hit.point + laserDirection * 0.001f;
                    continue;
                }
                else
                {
                    // Not a mirror, the laser stops here 
                    hitReceiver = hit.collider.GetComponentInParent<LaserReceiver>(); // If it hits the receiver, get the component
                    break;
                }
            }

            // Didn't hit anything, extend the laser to its maximum distance
            laserPoints.Add(laserOrigin + laserDirection * maxDistance);
            break;
        }

        laser.positionCount = laserPoints.Count;
        laser.SetPositions(laserPoints.ToArray());

        UpdateReceiver(hitReceiver);
    }

    private void UpdateReceiver(LaserReceiver hitReceiver)
    {
        if (hitReceiver != currentReceiver)
        {
            if (currentReceiver != null) currentReceiver.SetHit(false, powerLevel);
            currentReceiver = hitReceiver;
        }

        if (currentReceiver != null) currentReceiver.SetHit(true, powerLevel);
    }

    private void ReleaseReceiver()
    {
        if (currentReceiver != null) currentReceiver.SetHit(false, powerLevel);
        currentReceiver = null;
    }

    private void OnDisable()
    {
        ReleaseReceiver();
    }
}