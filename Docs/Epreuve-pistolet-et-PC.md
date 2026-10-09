# Épreuve du pistolet et écran « Fin » — Main 1

Le montage est installé dans **Assets/Scene/Main 1.unity**. Il réutilise le pistolet à eau déjà posé près du coffre et les sons présents dans le projet.

## Jouer

1. Terminer le parcours du papier, lire l'indice de l'imprimante et ouvrir le coffre avec le keypad.
2. Prendre `SM_Wep_Watergun_01` avec le **Grip**, sur le côté de la manette.
3. Viser les cibles numérotées près du PC et presser la **gâchette avant** pour tirer. Relâcher puis presser pour chaque tir.
4. Lire l'**ordre affiché sur le panneau**, par exemple **2 → 1 → 3**. Chaque cible validée devient verte.
5. Terminer **trois manches**. L'ordre change à chaque nouvelle manche ; chaque manche utilise les trois cibles une fois.
6. Le premier bon tir lance un chrono de **10 secondes** pour terminer la manche. On peut prendre le temps de lire le nouvel ordre avant de commencer.
7. Réussir la troisième manche allume le moniteur, affiche **Fin** et termine le jeu.

Avant l'ouverture du coffre, les cibles sont verrouillées. Un mauvais ordre ou un délai dépassé remet les cibles au bleu : recommencer **la manche en cours**, dans le même ordre. Les manches déjà réussies restent acquises. Un second tir sur une cible déjà validée dans la manche est ignoré ; le chrono continue. Une erreur de tir ne remet pas le papier au départ.

Le jet bleu indique la trajectoire du tir. Les colliders solides des couches comprises dans **Hit Mask** arrêtent le tir ; les triggers et les propres colliders du pistolet sont ignorés. La portée installée est de 12 mètres, avec au moins 0,25 seconde entre deux tirs. Le pistolet doit être tenu pour tirer.

## Objets et réglages

| Objet à rechercher dans Hierarchy | Composant ou enfant | Rôle |
| --- | --- | --- |
| `PCPowerChallenge` | `Computer Target Puzzle` | Gère les manches, leur ordre, le chrono, le coffre requis et l'écran |
| `Target1`, `Target2`, `Target3` | `Pc Power Target` | Détecte les tirs et colore le disque |
| `SM_Wep_Watergun_01` | `Puzzle Blaster` | Portée, cadence, masque de collision et son du tir |
| `SM_Wep_Watergun_01/Muzzle` | Transform | Point de départ du tir, suivant son axe bleu Z |
| `(Prb)PC/PC/PC_monitor/ScreenOff` | Surface noire | Apparence du moniteur éteint |
| `(Prb)PC/PC/PC_monitor/PoweredScreen` | Surface éclairée et `FinText` | S'affiche quand les trois cibles sont validées |
| `(Prb)PC/PC/PC_monitor/ScreenGlow` | Light | Petite lumière verte lors de l'allumage |
| `Gamerule` | **Require Computer Puzzle** | Activé dans Main 1 : le coffre débloque l'étape PC |

Les références sont déjà renseignées. `PuzzleBlaster` écoute automatiquement **Activated** du XR Grab Interactable : ne pas ajouter d'appel manuel à `Fire()` dans cet événement.

Pour déplacer l'épreuve, déplacer **PCPowerChallenge en entier**, hors Play. Garder les cibles devant leur panneau et dégagées des murs et du mobilier. La liste **Targets** doit correspondre aux numéros visibles : première entrée = cible 1, deuxième = cible 2, troisième = cible 3. Le script choisit l'ordre à chaque manche.

Dans **Computer Target Puzzle**, régler **Rounds To Win** pour changer le nombre de manches (3 actuellement), et **Seconds Per Round** pour ajuster le temps autorisé (10 actuellement). Une valeur de 15 ou 20 secondes rend le jeu plus facile. Le chrono ne commence qu'au premier bon tir.

Le texte des consignes est produit par `ComputerTargetPuzzle.cs`. Le texte final « Fin » est également fixé dans ce script. `Gamerule` passe maintenant par **PowerComputer** après l'ouverture du coffre, puis par **Completed** lorsque le PC s'allume.

Le texte **Fin** a été avancé de **1 cm** vers le joueur le 8 octobre. Son Canvas est désormais sous `PoweredScreen/FinTextOffset/FinText` : régler **Position X** de **FinTextOffset** pour changer cette distance (**0,01** actuellement). Ce repère conserve le décalage lorsque le Canvas s'active. Sur ce modèle, X positif correspond à l'avant du moniteur.

## Sons ajoutés

| Action | Source utilisée |
| --- | --- |
| Tir | `SFX_Liquid_Splash.wav` |
| Checkpoint du papier | `Button Pop.wav` |
| Reset du papier | `sfx_keypadDeniedd.wav` |
| Début d'impression, clic du tiroir et bouton Fermer | `sfx_keypadClick.wav` |
| Impression terminée et ouverture du coffre | `sfx_keypadGranted.wav` |
| Cible validée / mauvais ordre / PC allumé | Clic / accès refusé / accès accordé du keypad |

Les sons existants du keypad restent en place. Les nouveaux sons sont brefs et leur volume est réduit. Pour ajuster un volume, sélectionner l'enfant correspondant (**ShotSound**, **CheckpointSound**, **ResetSound**, **PrintStartSound**, **PrintReadySound**, **SafeOpenSound**, **DrawerClickSound**, **CloseClickSound**, **TargetFeedback**), puis régler **Audio Source → Volume**.

Les sons des objets utilisent une position 3D ; le reset du papier et la fermeture des fenêtres sont en 2D pour rester audibles même si leur objet est déplacé ou masqué. Les deux **CloseClickSound** se trouvent sous **Gamerule**, afin de rester actifs après la fermeture des panneaux. Aucun son ne démarre automatiquement au chargement de la scène.

## Vérification effectuée

Le montage initial et les sons ont été contrôlés le 5 octobre. La version à trois manches a été vérifiée en Play avec le rendu graphique actif le 8 octobre : ordre variable, chrono, reprise après erreur, conservation des manches réussies, neuf tirs par événement XR et allumage final. Le décalage de « Fin » a été mesuré après rechargement et activation du Canvas, puis contrôlé sur une capture. Les captures et résultats de cette modification sont conservés dans `Library/GameplayUpdate-2026-10-08`.

La manipulation réelle des manettes et le confort du délai de 10 secondes restent à vérifier dans le casque.
