using UnityEngine;

public class HintScreenController : MonoBehaviour
{
    [Header("UI & Effets")]
    [SerializeField] private GameObject hintCanvas;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip powerOnSound;

    public void ToggleHintScreen(bool isActivated)
    {
        hintCanvas.SetActive(isActivated);

        if (isActivated)
        {
            if (audioSource != null && powerOnSound != null) audioSource.PlayOneShot(powerOnSound);
        }
    }
}