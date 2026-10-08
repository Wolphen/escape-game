# Vérification de Main 1 — 8 octobre 2026

Ce bilan correspond au contrôle général effectué avant la modification de l'épreuve de tir et la correction du rendu du tiroir. Pour les réglages actuels, voir [Épreuve du pistolet et PC](Epreuve-pistolet-et-PC.md) et [Tiroir et note](Montage-tiroir-et-note.md).

Contrôle de `Assets/Scene/Main 1.unity` dans Unity **6000.3.24f1**. La scène enregistrée contient 568 objets. Les essais ont été exécutés dans Unity en Play automatisé, sans casque.

**Bilan : 156 contrôles réussis** (134 contrôles de scène et de progression, 20 contrôles supplémentaires des entrées, du keypad et des grilles, puis 2 contrôles du contact réel avec le plafond). Aucun échec fonctionnel non résolu ; le placement de Checkpoint1 reste à revoir ci-dessous.

## Corrections enregistrées

- **Main 1 est maintenant la première scène activée dans la liste de build.** La scène de démonstration XR est conservée après elle.
- **Sept références manquantes du décor ont été nettoyées dans Main 1** : quatre références d'Avatar sur des Animator de murs, et trois références audio inexistantes sur le ventilateur et les deux portes. Cela supprime les références invalides ; cela n'ajoute pas de nouveaux sons à ces objets. Les prefabs sources n'ont pas été modifiés.

## Fonctionnement vérifié

| Élément | Résultat |
| --- | --- |
| Scripts et références | Les 12 scripts de gameplay ont un `.meta` valide ; aucun script manquant ni référence cassée dans la scène après correction |
| Téléportation | 72 surfaces configurées sans partage de collider entre plusieurs zones ; une demande de téléportation déplace effectivement le XR Origin |
| Gamerule | Aucun composant de saisie ; progression jusqu'à l'étape PC puis à la fin |
| Tiroir et note | Ouverture, fermeture, inversion en mouvement, lecture bloquée tiroir fermé, note accessible au rayon, fenêtre indépendante et bouton Fermer |
| Papier | Retour au départ, checkpoints successifs, priorité de la destruction sur un retour en cours, protection du papier livré |
| Wall_02 | Son collider hérite de la règle du parent `Trial 1/ObstacleRetour` ; un contact physique provoque bien le retour du papier |
| Plafond | Un contact avec la surface réelle du MeshCollider remet le papier au départ et efface ses checkpoints |
| Grilles | Barrières solides sur Ignore Raycast ; collision avec le papier activée et saisie par les mains XR à travers la grille |
| Checkpoints optionnels | L'imprimante demande seulement l'index 1 : le checkpoint 2 et les suivants ne sont pas nécessaires à l'impression |
| Imprimante | Le socket accepte le papier ; l'impression se termine une seule fois, puis le clic révèle l'indice |
| Keypad et coffre | Les 11 boutons XR sont reliés ; un mauvais code remet le papier, les checkpoints et l'imprimante à zéro ; 1420 ouvre le coffre |
| Épreuve du PC | Cibles verrouillées avant le coffre, ordre 1–2–3, erreur qui réinitialise la séquence, tirs répétés ignorés, obstacle qui bloque le tir |
| Fin | La réussite active l'écran « Fin », sa lumière et la dernière étape de Gamerule |
| Fenêtres et sons | EventSystem XR unique, Canvas interactifs configurés, sources de sons renseignées ; les clics Fermer jouent même après masquage des panneaux |

Les entrées de manette sont contrôlées par leurs références et leurs événements XR. Cela ne remplace pas un essai réel des gestes dans le casque.

## Point de placement à revoir

**Checkpoint1 valide le papier automatiquement dès le départ.**

Son emplacement est `Trial 1/PaperResetZone/Checkpoint1`. Le centre de sa zone est à environ **9,8 cm** de `PaperStart`, et sa zone de détection mesure environ **1 × 0,26 × 1 m**. Le papier apparaît donc déjà à l'intérieur. Après un retour au départ, il peut reprendre immédiatement l'index 1.

Ce placement n'a pas été déplacé pendant le contrôle. Pour que le premier checkpoint représente une vraie étape, placer sa zone après un déplacement du papier, en gardant un support au départ et en vérifiant que le papier initial n'entre plus dans cette zone. Comme les suivants sont optionnels, ce premier checkpoint détermine à lui seul l'autorisation d'imprimer.

La saisie à distance du papier est actuellement autorisée : **Limit Grab Distance** est désactivé. Le contrôle respecte ce réglage, qui permet la saisie à travers les grilles.

## Essai conseillé dans le casque

1. Se téléporter jusqu'au parcours et prendre le papier à travers la grille.
2. Essayer de traverser la grille avec le papier, puis toucher Wall_02 et le plafond.
3. Déposer le papier dans l'imprimante, lire l'indice et fermer la fenêtre avec la gâchette.
4. Entrer un mauvais code, refaire la livraison, puis entrer 1420.
5. Prendre le pistolet avec le Grip, tirer sur 1–2–3 avec la gâchette et vérifier « Fin ».
6. Vérifier aussi la lisibilité de la note du tiroir, le confort des positions et le volume des sons.

Les sauvegardes avant correction et les rapports détaillés sont conservés dans `Library/FullSceneAudit-2026-10-08`. Les essais ne sauvegardent pas leurs déplacements temporaires d'objets.

Les premiers journaux incluent une tentative de contact avec l'enveloppe du plafond qui n'atteignait pas sa surface réelle. Ce test a été corrigé en utilisant le point d'impact sur le MeshCollider, puis rejoué avec succès. Le bilan consolidé est dans `final-summary.txt`.
