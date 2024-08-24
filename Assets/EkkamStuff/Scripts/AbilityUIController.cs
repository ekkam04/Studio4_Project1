using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ekkam
{
    public class AbilityUIController : MonoBehaviour
    {
        public List<Ability> abilities = new List<Ability>();
        public Button cycleNextButton;
        public Button cyclePreviousButton;
        public Button useAbilityButton;
        public TMP_Text abilityNameText;
        public RawImage abilityIcon;
        
        private int currentAbilityIndex;
        private Ability currentAbility;
        
        private void Start()
        {
            cycleNextButton.onClick.AddListener(CycleNextAbility);
            cyclePreviousButton.onClick.AddListener(CyclePreviousAbility);
            useAbilityButton.onClick.AddListener(UseAbility);
            
            if (abilities.Count > 0)
            {
                currentAbility = abilities[0];
                abilityNameText.text = currentAbility.abilityName;
            }
        }
        
        public void CycleNextAbility()
        {
            currentAbilityIndex++;
            if (currentAbilityIndex >= abilities.Count)
            {
                currentAbilityIndex = 0;
            }

            currentAbility = abilities[currentAbilityIndex];
            abilityNameText.text = currentAbility.abilityName;
            abilityIcon.texture = currentAbility.icon;
        }
        
        public void CyclePreviousAbility()
        {
            currentAbilityIndex--;
            if (currentAbilityIndex < 0)
            {
                currentAbilityIndex = abilities.Count - 1;
            }

            currentAbility = abilities[currentAbilityIndex];
            abilityNameText.text = currentAbility.abilityName;
            abilityIcon.texture = currentAbility.icon;
        }
        
        public void UseAbility()
        {
            NetworkManager.instance.myPlayer.AbilityButton(currentAbility, abilities);
        }
    }
}