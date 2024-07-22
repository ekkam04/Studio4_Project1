using System.Collections.Generic;
using UnityEngine;

namespace Ekkam
{
    public class PropManager : MonoBehaviour
    {
        public List<PropData> propDataList = new List<PropData>();

        public void ShowProp(string propKey, string senderName, Transform parent)
        {
            foreach (var propData in propDataList)
            {
                if (propData.propKey == propKey)
                {
                    var prop = Instantiate(propData.propPrefab, parent);
                    prop.name = senderName + "_" + propKey;
                    break;
                }
            }
        }

        public void HideProp(string propKey, string senderName)
        {
            foreach (var propData in propDataList)
            {
                if (propData.propKey == propKey)
                {
                    var prop = GameObject.Find(senderName + "_" + propKey);
                    if (prop != null)
                    {
                        Destroy(prop);
                    }
                    break;
                }
            }
        }
    }
    
    [System.Serializable]
    public struct PropData
    {
        public string propKey;
        public GameObject propPrefab;
    }
}