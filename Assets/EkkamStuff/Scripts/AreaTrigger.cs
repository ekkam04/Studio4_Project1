using UnityEngine;

namespace Ekkam
{
    public class AreaTrigger : MonoBehaviour
    {
        public bool isTriggered;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isTriggered)
            {
                isTriggered = true;
                Debug.Log("Player entered the area");
            }
        }
    }
}