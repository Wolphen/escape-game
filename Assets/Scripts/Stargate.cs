using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Stargate : MonoBehaviour
{
    [Header("Sockets du Puzzle")]
    [SerializeField] private XRSocketInteractor[] sockets = new XRSocketInteractor[4];

    [Header("Porte des Étoiles")]
    [SerializeField] private GameObject stargateVFX;

    [Header("Gestion du passage / TP")]
    [SerializeField] private Collider solidFloorCollider;
    [SerializeField] private GameObject teleportAreaUnderground;

    private int fragmentsPlacedCount = 0;

    private void OnEnable()
    {
        foreach (var socket in sockets)
        {
            if (socket != null)
            {
                socket.selectEntered.AddListener(OnFragmentInserted);
                socket.selectExited.AddListener(OnFragmentRemoved);
            }
        }
    }

    private void OnDisable()
    {
        foreach (var socket in sockets)
        {
            if (socket != null)
            {
                socket.selectEntered.RemoveListener(OnFragmentInserted);
                socket.selectExited.RemoveListener(OnFragmentRemoved);
            }
        }
    }

    private void OnFragmentInserted(SelectEnterEventArgs args)
    {
        fragmentsPlacedCount++;
        CheckPuzzleState();
    }

    private void OnFragmentRemoved(SelectExitEventArgs args)
    {
        fragmentsPlacedCount--;
        if (fragmentsPlacedCount < 0) fragmentsPlacedCount = 0;
        CheckPuzzleState();
    }

    private void CheckPuzzleState()
    {
        if (fragmentsPlacedCount >= sockets.Length)
        {
            ActivateStargate();
        }
        else
        {
            DeactivateStargate();
        }
    }

    private void ActivateStargate()
    {
        if (stargateVFX != null) stargateVFX.SetActive(true);

        if (solidFloorCollider != null) solidFloorCollider.enabled = false;

        if (teleportAreaUnderground != null) teleportAreaUnderground.SetActive(true);
    }

    private void DeactivateStargate()
    {
        if (stargateVFX != null) stargateVFX.SetActive(false);

        if (solidFloorCollider != null) solidFloorCollider.enabled = true;
        if (teleportAreaUnderground != null) teleportAreaUnderground.SetActive(false);
    }
}