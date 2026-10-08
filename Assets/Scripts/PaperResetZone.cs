using UnityEngine;

[DisallowMultipleComponent]
public class PaperResetZone : MonoBehaviour
{
    public enum ResetMode { LastCheckpoint, DestroyAndRestart }

    [SerializeField] private ResetMode mode = ResetMode.LastCheckpoint;
    [Tooltip("Le papier de cette enigme. Les autres objets sont ignores.")]
    [SerializeField] private PaperPuzzleItem targetPaper;

    private void Reset()
    {
        // Un mur garde son collider solide. Sur un objet vide, creer une zone trigger.
        if (GetComponent<Collider>() == null)
            gameObject.AddComponent<BoxCollider>().isTrigger = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckPaper(collision.collider);
    }

    private void OnCollisionStay(Collision collision)
    {
        CheckPaper(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckPaper(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // Detecte aussi un papier qui se trouve deja dans la zone a son activation.
        CheckPaper(other);
    }

    private void CheckPaper(Collider other)
    {
        if (targetPaper == null || other.attachedRigidbody == null ||
            other.attachedRigidbody.GetComponent<PaperPuzzleItem>() != targetPaper)
            return;

        ResetPaper(targetPaper);
    }

    // Le papier appelle aussi cette methode quand il touche un collider enfant de la zone.
    public void ResetPaper(PaperPuzzleItem paper)
    {
        if (!isActiveAndEnabled || targetPaper == null || paper != targetPaper)
            return;

        if (mode == ResetMode.DestroyAndRestart)
            targetPaper.DestroyAndRespawn();
        else
            targetPaper.ResetToCheckpoint();
    }
}
