using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>Ouvre le coffre en faisant tourner sa porte autour d'une charnière.</summary>
[DisallowMultipleComponent]
public class SafeDoor : MonoBehaviour
{
    [Tooltip("Objet placé sur la charnière, parent du visuel et des colliders de la porte.")]
    [SerializeField] private Transform doorPivot;
    [Tooltip("Rotation ajoutée à la rotation locale initiale. Inverser le signe pour changer le sens.")]
    [SerializeField] private Vector3 openEulerAngles = new Vector3(0f, 90f, 0f);
    [Min(0f)][SerializeField] private float openDuration = 1f;
    [SerializeField] private Gamerule gameRule;
    [SerializeField] private UnityEvent onOpened = new UnityEvent();

    private Quaternion closedRotation;
    private Quaternion targetRotation;
    private Coroutine openingRoutine;
    private float elapsed;
    private float duration;
    private bool isOpening;
    private bool isOpen;

    public bool IsOpen => isOpen;
    public bool IsOpening => isOpening;

    private void Awake()
    {
        if (doorPivot == null)
            doorPivot = transform;
        closedRotation = doorPivot.localRotation;
    }

    private void OnEnable()
    {
        if (isOpening && openingRoutine == null)
            openingRoutine = StartCoroutine(AnimateOpening());
    }

    private void OnDisable()
    {
        if (openingRoutine != null)
        {
            StopCoroutine(openingRoutine);
            openingRoutine = null;
        }
    }

    /// <summary>À relier à On Access Granted du keypad. Les appels suivants sont ignorés.</summary>
    public void Open()
    {
        if (!isActiveAndEnabled || isOpening || isOpen || doorPivot == null)
            return;

        isOpening = true;
        elapsed = 0f;
        duration = Mathf.Max(0f, openDuration);
        targetRotation = closedRotation * Quaternion.Euler(openEulerAngles);
        if (duration <= 0f)
        {
            FinishOpening();
            return;
        }

        openingRoutine = StartCoroutine(AnimateOpening());
    }

    private IEnumerator AnimateOpening()
    {
        while (elapsed < duration)
        {
            if (doorPivot == null)
            {
                isOpening = false;
                openingRoutine = null;
                yield break;
            }

            elapsed += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            doorPivot.localRotation = Quaternion.Slerp(closedRotation, targetRotation, progress);
            yield return null;
        }

        FinishOpening();
    }

    private void FinishOpening()
    {
        openingRoutine = null;
        isOpening = false;
        isOpen = true;
        if (doorPivot != null)
            doorPivot.localRotation = targetRotation;
        if (gameRule != null)
            gameRule.MarkSafeOpened();
        onOpened.Invoke();
    }
}
