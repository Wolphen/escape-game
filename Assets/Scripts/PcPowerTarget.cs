using UnityEngine;

/// <summary>Une cible de la sequence du PC. Les interactions de saisie ne la valident pas.</summary>
[DisallowMultipleComponent]
public class PcPowerTarget : MonoBehaviour
{
    [SerializeField] private ComputerTargetPuzzle puzzle;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color idleColor = new Color(0.08f, 0.5f, 0.85f);
    [SerializeField] private Color hitColor = new Color(0.08f, 0.9f, 0.25f);
    public bool IsHit { get; private set; }
    private MaterialPropertyBlock properties;

    public void ReceiveShot(PuzzleBlaster source)
    {
        if (isActiveAndEnabled && puzzle != null)
            puzzle.RegisterHit(source, this);
    }

    public void SetHit(bool hit)
    {
        IsHit = hit;
        if (targetRenderer == null)
            return;
        if (properties == null)
            properties = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(properties);
        Color color = hit ? hitColor : idleColor;
        properties.SetColor("_BaseColor", color);
        properties.SetColor("_Color", color);
        properties.SetColor("_EmissionColor", color * 0.35f);
        targetRenderer.SetPropertyBlock(properties);
    }
}
