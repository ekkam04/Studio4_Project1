/// ---------------------------------------------
/// Omni Animation Packs
/// Copyright (c) Opsive. All Rights Reserved.
/// https://omnianimation.ai
/// ---------------------------------------------

namespace Opsive.OmniAnimation.Packs.Shared
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Spawns the character and items.
    /// </summary>
    public class Spawner : MonoBehaviour
    {
        /// <summary>
        /// Information about the item spawn.
        /// </summary>
        [Serializable]
        public struct ItemInfo
        {
            [Tooltip("The item prefab.")]
            public GameObject ItemPrefab;
            [Tooltip("The parent identifying component.")]
            public int SpawnParentID;
            [Tooltip("The local position of the item spawn.")]
            public Vector3 LocalSpawnPosition;
            [Tooltip("The local rotation of the item spawn.")]
            public Quaternion LocalSpawnRotation;
            [Tooltip("The local scale of the item spawn.")]
            public Vector3 LocalSpawnScale;
        }

        [Tooltip("The character prefab.")]
        [SerializeField] protected GameObject m_CharacterPrefab;
        [Tooltip("The position that the character should be spawned.")]
        [SerializeField] protected Vector3 m_SpawnPosition;
        [Tooltip("The rotation that the character should be spawned.")]
        [SerializeField] protected Quaternion m_SpawnRotation;
        [Tooltip("Any items that should spawn with the character.")]
        [SerializeField] protected ItemInfo[] m_Items;

        /// <summary>
        /// Spawns the objects.
        /// </summary>
        /// <returns>The spawned character and item.</returns>
        public (GameObject, GameObject) SpawnObjects()
        {
            var character = UnityEngine.Object.Instantiate(m_CharacterPrefab);
            character.transform.SetPositionAndRotation(m_SpawnPosition, m_SpawnRotation);

            GameObject item = null;
            if (m_Items != null) {
                var spawnIdentifiers = character.GetComponentsInChildren<SpawnIdentifier>();
                for (int i = 0; i < m_Items.Length; i++) {
                    for (int j = 0; j < spawnIdentifiers.Length; ++j) {
                        if (m_Items[i].SpawnParentID != spawnIdentifiers[j].ID) {
                            continue;
                        }

                        item = UnityEngine.Object.Instantiate(m_Items[i].ItemPrefab, spawnIdentifiers[j].transform);
                        item.transform.localPosition = m_Items[i].LocalSpawnPosition;
                        item.transform.localRotation = m_Items[i].LocalSpawnRotation;
                        item.transform.localScale = m_Items[i].LocalSpawnScale;
                        break;
                    }
                }
            }

            return (character, item);
        }
    }
}