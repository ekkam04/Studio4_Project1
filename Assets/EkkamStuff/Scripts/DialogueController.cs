using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ekkam
{
    public class DialogueController : MonoBehaviour
    {
        public GameObject dialoguePanel;
        public TMP_Text speakerText;
        public TMP_Text dialogueText;
        public Button continueButton;
        public bool continueDialogue;
        public bool continueDialogueManual;
        
        public bool isShowingDialogues;
        
        private void Start()
        {
            continueButton.onClick.AddListener(OnContinueButton);
        }
        
        public IEnumerator ShowDialogues(Dialogue[] dialogues, bool skippableByButton = true)
        {
            if (skippableByButton)
            {
                continueButton.gameObject.SetActive(true);
            }
            else
            {
                continueButton.gameObject.SetActive(false);
            }
            
            isShowingDialogues = true;
            foreach (var dialogue in dialogues)
            {
                speakerText.text = dialogue.speaker;
                dialogueText.text = "";
                dialoguePanel.SetActive(true);
                continueDialogue = false;
                for (int i = 0; i < dialogue.dialogue.Length; i++)
                {
                    if (continueDialogue) break;
                    dialogueText.text += dialogue.dialogue[i];
                    yield return new WaitForSeconds(0.01f);
                }
                dialogueText.text = dialogue.dialogue;
                continueDialogue = false;
                if (skippableByButton)
                {
                    yield return new WaitUntil(() => continueDialogue);
                }
                else
                {
                    yield return new WaitUntil(() => continueDialogueManual);
                    continueDialogueManual = false;
                }
                dialoguePanel.SetActive(false);
            }
            isShowingDialogues = false;
        }
        
        public void OnContinueButton()
        {
            continueDialogue = true;
        }
    }
    
    public class Dialogue
    {
        public string speaker;
        public string dialogue;
        
        public Dialogue(string speaker, string dialogue)
        {
            this.speaker = speaker;
            this.dialogue = dialogue;
        }
    }
}