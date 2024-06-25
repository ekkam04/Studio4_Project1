// Copyright (c) 2023 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// Sample scripts are included only as examples and are not intended as production-ready.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Synty.Interface.Samples
{
    /// <summary>
    ///     Executes a list of animator actions and toggles a list of game objects on click.
    /// </summary>
    public class SampleButtonAction : MonoBehaviour
    {
        [Header("References")]
        public Button button;
        public List<GameObject> toggleObjects;
        public GameObject activateObject;

        [Header("Parameters")]
        public List<AnimatorActionData> animatorActions;
        public float activeTime = 1f;
        public bool runOnEnable;
        public bool applyRandomRotationToActivateObject;

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (button == null)
            {
                return;
            }

            button.onClick.AddListener(OnClick);
        }

        private void Reset()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (runOnEnable)
            {
                OnClick();
            }
        }

        private void OnClick()
        {
            if (activateObject)
            {
                StartCoroutine(C_ActivateObject());
            }

            foreach (GameObject toggleObject in toggleObjects)
            {
                toggleObject.SetActive(!toggleObject.activeSelf);
            }

            foreach (AnimatorActionData action in animatorActions)
            {
                action.Execute();
            }
        }

        private IEnumerator C_ActivateObject()
        {
            if (activateObject == null)
            {
                yield break;
            }

            activateObject.SetActive(true);
            if (applyRandomRotationToActivateObject)
            {
                activateObject.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            }

            yield return new WaitForSeconds(activeTime);

            activateObject.SetActive(false);
        }
    }
}
