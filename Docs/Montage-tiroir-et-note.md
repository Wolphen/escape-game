# Tiroir du bureau et note — Main 1

Le montage est déjà installé dans **Assets/Scene/Main 1.unity**, sur le **tiroir du haut** du bureau du PC. Les deux scripts sont `Assets/Scripts/DrawerSlide.cs` et `Assets/Scripts/ReadableNote.cs`.

## Essayer dans Unity

1. Ouvrir **Main 1** et attendre la fin de la compilation.
2. Lancer Play, viser la façade du tiroir du haut puis presser et relâcher le **Grip**, sur le côté de la manette.
3. Le tiroir glisse vers l'extérieur. Viser la note à l'intérieur et presser puis relâcher le **Grip** pour lire l'indice.
4. Avec une main libre, viser **Fermer** dans la fenêtre et presser puis relâcher la **gâchette avant de tir**.
5. Cliquer de nouveau sur la façade avec le Grip pour refermer le tiroir. Sa fermeture masque aussi la fenêtre de la note si elle est ouverte.

La note est fixe dans le tiroir et se déplace avec lui. La feuille à transporter reste l'objet `Paper` du parcours ; seule cette dernière est acceptée par les checkpoints et l'imprimante.

## Où sont les objets ?

```text
(Prb)Desk2
└─ desk2
   └─ desk2_drawers
      └─ desk2_drawer1             ← tiroir du haut
         ├─ DrawerColliders       ← fond et quatre parois
         │  └─ Front              ← collider de la façade cliquable
         └─ DrawerNote            ← ReadableNote et interaction XR
            ├─ PaperVisual        ← apparence de la feuille
            └─ NoteLabel          ← petit texte visible sur la note

DrawerNoteCanvas                  ← fenêtre indépendante près du bureau
└─ NotePanel
   ├─ NoteText
   └─ CloseButton
      └─ Label
```

## Régler l'ouverture

Sélectionner **desk2_drawer1**, puis son composant **Drawer Slide** :

| Champ | Réglage installé | Rôle |
| --- | --- | --- |
| Drawer | `desk2_drawer1` | Partie qui se déplace |
| Open Offset | `(0.35, 0, 0)` | Déplacement dans les axes du parent du tiroir |
| Move Duration | `0.45` | Durée d'une ouverture complète, en secondes |

Sur ce modèle, **X positif** sort le tiroir. Pour diminuer l'ouverture, essayer `0.25` dans X. Une valeur négative inverse le sens. La distance dans le monde dépend aussi de l'échelle des parents.

Un clic pendant le mouvement inverse la direction. Le script conserve la position fermée initiale, y compris après plusieurs ouvertures ou une désactivation temporaire. Une durée de `0` produit un mouvement instantané.

Le composant **XR Simple Interactable** du tiroir référence uniquement le collider **DrawerColliders/Front**. `DrawerSlide` branche automatiquement son événement Select Entered : **ne pas ajouter d'appel manuel à Toggle()** pour éviter deux changements à chaque clic. L'appel à `DrawerClickSound.Play()` ajouté pour le son peut rester dans cet événement.

Le Rigidbody du tiroir est **Is Kinematic activé**, **Use Gravity désactivé**. Le BoxCollider plein d'origine est désactivé dans cette instance. Il est remplacé par le fond et les quatre parois de DrawerColliders : réactiver le gros collider boucherait l'intérieur et pourrait empêcher le rayon d'atteindre la note.

**Static doit être décoché sur `desk2_drawer1` et ses enfants mobiles.** Le prefab d'origine les marquait statiques : Unity pouvait conserver le dessin du tiroir à sa place, alors que la note et les colliders bougeaient. Ce réglage a été corrigé dans Main 1 le 8 octobre. Dans l'Inspector, la case **Static** se trouve en haut, à droite du nom de l'objet. Si Unity propose d'appliquer le changement aux enfants du tiroir, choisir **Yes, change children**.

La hauteur de **DrawerNote** et du collider **DrawerColliders/Bottom** a aussi été ajustée sur la surface réelle du modèle. La note reste visible une fois le tiroir ouvert, au-dessus de cette surface. Pour remplacer le modèle du tiroir, vérifier cette hauteur : le bas de ses dimensions globales ne correspond pas forcément au fond visible.

## Modifier l'indice

Sélectionner **DrawerNote**, puis le composant **Readable Note**. Modifier **Clue Text** pour changer ce texte :

> L’imprimante garde la suite du secret, mais son bac est vide.
>
> Récupère la feuille dans le parcours derrière les grilles, puis dépose-la dans l’imprimante. Elle te livrera le prochain indice.
>
> Pour fermer, vise « Fermer » et appuie sur la gâchette de tir.

Les références sont déjà renseignées :

| Champ | Objet |
| --- | --- |
| Required Drawer | Le `DrawerSlide` de `desk2_drawer1` |
| Popup Root | `DrawerNoteCanvas/NotePanel` |
| Popup Text | `DrawerNoteCanvas/NotePanel/NoteText` |

La note attend que le tiroir soit complètement ouvert et à l'arrêt avant d'autoriser la lecture. `ReadableNote` branche automatiquement Select Entered sur son propre XR Simple Interactable : **aucun appel manuel Read() à ajouter**. Sa liste Colliders référence le BoxCollider de DrawerNote.

Le bouton Fermer est relié à **ReadableNote.ClosePopup()**. Le Canvas utilise World Space, la caméra XR et un Tracked Device Graphic Raycaster. Il réutilise l'EventSystem XR déjà présent dans Main 1.

Pour ajuster la fenêtre, déplacer ou tourner **DrawerNoteCanvas** hors Play. Pour modifier sa taille, régler son RectTransform ou son échelle uniforme. Le Canvas reste actif ; le script affiche et cache uniquement NotePanel. Garder ce Canvas séparé du tiroir et de la note, afin de pouvoir refermer le panneau sans désactiver le mécanisme.

Cette fenêtre appartient à la note du tiroir. L'indice du code 1420 reste géré séparément par PrinterClue, après livraison de la feuille.

## Réutiliser les scripts ailleurs

Pour un autre tiroir, décocher **Static** sur sa partie mobile et ses enfants, puis ajouter **DrawerSlide** sur cette partie. Unity ajoute le XR Simple Interactable requis ; lui assigner le collider de la façade. Régler Open Offset suivant l'axe du nouveau meuble.

Pour une autre note, ajouter **ReadableNote** sur un objet avec un collider solide et renseigner sa propre fenêtre et son texte. Renseigner Required Drawer si sa lecture dépend d'un tiroir ouvert ; laisser ce champ vide pour une note toujours accessible. Le texte peut aussi être affiché par un bouton UI relié à Read(), et fermé par ClosePopup().

## Vérification effectuée

Compilation et essais automatisés en Play réussis dans Unity 6000.3.24f1 le 5 octobre 2026 : ouverture par événement XR, lecture bloquée tiroir fermé, fenêtre indépendante, bouton Fermer, fermeture du panneau pendant la fermeture du tiroir, inversion en mouvement, désactivation/réactivation et dix cycles sans dérive de position. Un rayon physique lancé au-dessus de la note atteint bien son collider quand le tiroir est ouvert. Le texte tient dans le panneau.

Les événements ont été déclenchés par le programme de contrôle. Vérifier dans le casque la position de la fenêtre, la visibilité de la feuille et les gestes de sélection. Une sauvegarde de Main 1 avant installation et les résultats des essais sont conservés dans `Library/DrawerNoteSetup`.

La correction du 8 octobre est contrôlée avec le rendu graphique actif : exclusion du tiroir du regroupement statique, déplacement de sa géométrie visible avec la note, captures fermé/ouvert et retour à la position initiale. Résultats et captures dans `Library/GameplayUpdate-2026-10-08`.
