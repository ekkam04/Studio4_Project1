using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ekkam
{
    public class TutorialController : MonoBehaviour
    {
        UIManager uiManager;
        DialogueController dialogueController;
        
        public GameObject blackScreen;
        public GameObject objectiveLight1;
        public GameObject objectiveLight2;
        public GameObject coverShowcase;
        public List<AreaTrigger> startingAreaTriggers;
        public List<EnemySpawner> startingSkeletonSpawners;
        
        public bool gunPickedUp;

        private void Start()
        {
            uiManager = FindObjectOfType<UIManager>();
            dialogueController = FindObjectOfType<DialogueController>();
            
            uiManager.endTurnButtonGO.SetActive(false);
            uiManager.shootButtonGO.SetActive(false);
            blackScreen.SetActive(true);
            objectiveLight1.SetActive(false);
            objectiveLight2.SetActive(false);
            
            StartCoroutine(StartTutorial());
            
            PlayerPickUpItems.onItemPickedUp += OnItemPickedUp;
        }
        
        private void OnItemPickedUp(string itemKey)
        {
            switch (itemKey)
            {
                case "gun":
                    gunPickedUp = true;
                    break;
            }
        }

        IEnumerator StartTutorial()
        {
            yield return new WaitForSeconds(1f);
            string playerName = NetworkManager.instance.AgentData.name;
            
            var dialogues = new Dialogue[]
            {
                new Dialogue(playerName, "So where is this cemetery?"),
                new Dialogue("Jeff", "Let me ask siri real quick... It should be somewhere near your position... Oh, she ways you should take the next left."),
                new Dialogue(playerName, "You're the boss."),
                new Dialogue("", "*Loud crashing sounds*"),
                new Dialogue("Jeff", "Are you alive? respond? Helloooooooo?"),
                new Dialogue(playerName, "Damn, I'm up I'm up..."),
                new Dialogue("Jeff", "Great, you should gear up. I am picking up a surge of eldritch energy in your vicinity."),
                new Dialogue(playerName, "Affirmative, I'm on it."),
            };
            StartCoroutine(dialogueController.ShowDialogues(dialogues));
            uiManager.moveButtonGO.SetActive(false);
            
            yield return new WaitUntil(() => !dialogueController.isShowingDialogues);
            var timer = 0f;
            while (timer < 2f)
            {
                timer += Time.deltaTime;
                blackScreen.GetComponent<RawImage>().color = Color.Lerp(Color.black, Color.clear, timer);
                yield return null;
            }
            blackScreen.SetActive(false);
            
            yield return new WaitForSeconds(0.5f); // --------------------------------------------------------------------------------
            Player player = FindObjectOfType<Player>();
            
            objectiveLight1.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            dialogues = new Dialogue[]
            {
                new Dialogue("Jeff", "Oh look! there's a chest right there.")
            };
            StartCoroutine(dialogueController.ShowDialogues(dialogues));
            yield return new WaitUntil(() => !dialogueController.isShowingDialogues);
            
            dialogues = new Dialogue[]
            {
                new Dialogue("Jeff", "Let's move there. Try clicking on your character and select the move action.")
            };
            StartCoroutine(dialogueController.ShowDialogues(dialogues, false));
            uiManager.moveButtonGO.SetActive(true);
            yield return new WaitUntil(() => player.selectingTarget == Player.SelectingTarget.Move);
            dialogueController.continueDialogueManual = true;
            
            yield return new WaitForSeconds(0.1f); // --------------------------------------------------------------------------------
            
            dialogues = new Dialogue[]
            {
                new Dialogue("Jeff", "Great! Now click on any of the highlighted tiles to move there.")
            };
            StartCoroutine(dialogueController.ShowDialogues(dialogues, false));
            yield return new WaitUntil(() => player.isTakingAction);
            dialogueController.continueDialogueManual = true;
            yield return new WaitUntil(() => !player.isTakingAction);
            player.movementPoints = 4;
            
            yield return new WaitForSeconds(0.1f); // --------------------------------------------------------------------------------
            
            // if (!gunPickedUp)
            // {
            //     dialogues = new Dialogue[]
            //     {
            //         new Dialogue("Jeff", "There's a gun near that chest. You should pick it up.")
            //     };
            //     StartCoroutine(dialogueController.ShowDialogues(dialogues, false));
            //     while (!gunPickedUp)
            //     {
            //         player.movementPoints = 4;
            //         yield return null;
            //     }
            //     dialogueController.continueDialogueManual = true;
            // }
            // else
            // {
            //     dialogues = new Dialogue[]
            //     {
            //         new Dialogue("Jeff", "Great! Good thing you picked up that gun.")
            //     };
            //     StartCoroutine(dialogueController.ShowDialogues(dialogues));
            //     yield return new WaitUntil(() => !dialogueController.isShowingDialogues);
            // }
            //
            // player.movementPoints = 4;
            // objectiveLight1.SetActive(false);
            // uiManager.moveButtonGO.SetActive(false);
            
            yield return new WaitForSeconds(0.1f); // --------------------------------------------------------------------------------
            
            // dialogues = new Dialogue[]
            // {
            //     new Dialogue("Jeff", "Press I or click on the chest icon to open your inventory")
            // };
            // StartCoroutine(dialogueController.ShowDialogues(dialogues, false));
            // yield return new WaitUntil(() => !uiManager.openInventoryButton.gameObject.activeSelf);
            // dialogueController.continueDialogueManual = true;
            
            yield return new WaitForSeconds(0.1f); // --------------------------------------------------------------------------------
            
            // dialogues = new Dialogue[]
            // {
            //     new Dialogue("Jeff", "Great! Now drag the gun to your right hand slot and close the inventory.")
            // };
            // StartCoroutine(dialogueController.ShowDialogues(dialogues, false));
            // yield return new WaitUntil(() => uiManager.openInventoryButton.gameObject.activeSelf);
            // dialogueController.continueDialogueManual = true;
            
            yield return new WaitForSeconds(0.1f); // --------------------------------------------------------------------------------
            
            objectiveLight2.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            dialogues = new Dialogue[]
            {
                new Dialogue("Jeff", "Let's proceed... Now let's see how cover works.")
            };
            StartCoroutine(dialogueController.ShowDialogues(dialogues));
            yield return new WaitUntil(() => !dialogueController.isShowingDialogues);
            
            yield return new WaitForSeconds(0.1f); // --------------------------------------------------------------------------------
            
            coverShowcase.SetActive(true);
            dialogues = new Dialogue[]
            {
                new Dialogue("Jeff", "Cover is important in combat. It reduces the chance of getting hit."),
                new Dialogue("Jeff", "Half cover gives half the evasion boost of full cover but it is easier to shoot from."),
                new Dialogue("Jeff", "Now let's proceed."),
            };
            StartCoroutine(dialogueController.ShowDialogues(dialogues));
            yield return new WaitUntil(() => !dialogueController.isShowingDialogues);
            coverShowcase.SetActive(false);
            player.movementPoints = 5;
            
            yield return new WaitForSeconds(0.1f); // --------------------------------------------------------------------------------
            
            uiManager.moveButtonGO.SetActive(true);
            
            bool anyTriggered = false;
            while (!anyTriggered)
            {
                player.movementPoints = 5;
                foreach (var trigger in startingAreaTriggers)
                {
                    if (trigger.isTriggered)
                    {
                        anyTriggered = true;
                        break;
                    }
                }
                yield return null;
            }
            
            yield return new WaitForSeconds(0.1f); // --------------------------------------------------------------------------------

            uiManager.shootButtonGO.SetActive(true);
            uiManager.endTurnButtonGO.SetActive(true);
            foreach (var spawner in startingSkeletonSpawners)
            {
                spawner.Spawn(spawner.transform.position);
            }
            dialogues = new Dialogue[]
            {
                new Dialogue("Jeff", "Watch out! Skeletons are coming!"),
                new Dialogue("Jeff", "Use the shoot action to attack them."),
            };
            StartCoroutine(dialogueController.ShowDialogues(dialogues));
            yield return new WaitUntil(() => !dialogueController.isShowingDialogues);
            
            yield return null;
        }
    }
}