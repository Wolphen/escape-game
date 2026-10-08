using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>Une note fixe a lire au clic, distincte du papier a transporter.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(XRSimpleInteractable))]
public class ReadableNote : MonoBehaviour, IXRSelectFilter
{
    [Tooltip("Facultatif. Si renseigne, la note ne se lit que lorsque ce tiroir est ouvert.")]
    [SerializeField] private DrawerSlide requiredDrawer;
    [Tooltip("Panneau separe de la note : ni cet objet ni l'un de ses parents.")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private TMP_Text popupText;
    [TextArea(4, 10)] [SerializeField] private string clueText =
        "L’imprimante garde la suite du secret, mais son bac est vide.\n\n" +
        "Récupère la feuille dans le parcours derrière les grilles, puis dépose-la dans l’imprimante. Elle te livrera le prochain indice.\n\n" +
        "Pour fermer, vise « Fermer » et appuie sur la gâchette de tir.";

    public bool canProcess => isActiveAndEnabled;
    public bool CanRead => isActiveAndEnabled &&
        (requiredDrawer == null || (requiredDrawer.isActiveAndEnabled && requiredDrawer.IsOpen && !requiredDrawer.IsMoving));

    private XRSimpleInteractable interaction;
    private bool HasValidPopup => popupRoot != null && popupRoot != gameObject &&
        !transform.IsChildOf(popupRoot.transform);

    private void Awake()
    {
        interaction = GetComponent<XRSimpleInteractable>();
    }

    private void OnEnable()
    {
        interaction.selectEntered.AddListener(OnSelected);
        interaction.selectFilters.Add(this);
        ClosePopup();
    }

    private void OnDisable()
    {
        interaction.selectEntered.RemoveListener(OnSelected);
        interaction.selectFilters.Remove(this);
        ClosePopup();
    }

    private void Update()
    {
        if (!CanRead && HasValidPopup && popupRoot.activeSelf)
            ClosePopup();
    }

    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable) => CanRead;

    private void OnSelected(SelectEnterEventArgs args) => Read();

    public void Read()
    {
        if (!Application.isPlaying || !CanRead)
            return;
        if (!HasValidPopup || popupText == null)
        {
            Debug.LogWarning("ReadableNote : renseignez Popup Root avec un panneau distinct, et Popup Text avec son texte TMP.", this);
            return;
        }
        popupText.text = clueText;
        popupRoot.SetActive(true);
    }

    public void ClosePopup()
    {
        if (HasValidPopup)
            popupRoot.SetActive(false);
    }
}
