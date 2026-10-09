using System.Collections;
using UnityEngine;

/// <summary>
/// Sliding door to the next room. Call Open() from the Socket Interactor's "Select Entered" event.
/// When the door opens, the objects listed in "Activate On Open" are activated
/// (ex: the teleportation area of the next room, which stays disabled until then).
/// </summary>
public class RoomDoor : MonoBehaviour
{
    [Header("Door")]
    [Tooltip("The moving part of the door (this object if left empty)")]
    [SerializeField] private Transform door;
    [Tooltip("Movement when the door opens, in the door's parent space (ex: 0,0,2 or 2,0,0 or 0,3,0)")]
    [SerializeField] private Vector3 openLocalOffset = new Vector3(0f, 0f, 2f);
    [SerializeField] private float openDuration = 2f;

    [Header("When opened")]
    [Tooltip("Objects activated when the door opens (they are deactivated at the start)")]
    [SerializeField] private GameObject[] activateOnOpen;

    [Header("Feedback (optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openClip;

    private Vector3 closedLocalPosition;
    private bool opened;

    private void Awake()
    {
        if (door == null) door = transform;
        closedLocalPosition = door.localPosition;

        foreach (GameObject go in activateOnOpen)
            if (go != null) go.SetActive(false);
    }

    // To call from a UnityEvent
    public void Open()
    {
        if (opened) return;
        opened = true;

        foreach (GameObject go in activateOnOpen)
            if (go != null) go.SetActive(true);

        if (audioSource != null && openClip != null)
            audioSource.PlayOneShot(openClip);

        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        Vector3 target = closedLocalPosition + openLocalOffset;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, openDuration);
            door.localPosition = Vector3.Lerp(closedLocalPosition, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        door.localPosition = target;
    }
}