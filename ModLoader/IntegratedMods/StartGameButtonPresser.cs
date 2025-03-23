using System;
using System.Collections;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using UnityEngine;

namespace ModLoader.IntegratedMods
{
    //[ItemId("ModLoader.StartGameButtonPresser")]
    public class StartGameButtonPresser : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1);
            
            Log("Start");
            var startButton = GameObject.Find(
                "Platoons/CarrierPlatoon/AlliedCarrier/ControlPanel (1)/EquipPanel/Canvas/main/GamePanel/startGameButton");

            if (startButton == null)
            {
                LogError("Can't find start button");
                throw new Exception("Can't find start button");
            }

            var interactable = startButton.GetComponent<VRInteractable>();

            if (interactable == null)
            {
                throw new Exception("Can't find interactable on start button");
            }
            
            Log($"Pressing {interactable.interactableName}");
            interactable.StartInteraction();
            interactable.StopInteraction();
        }

        private void LogError(object message)
        {
            Debug.LogError($"[{nameof(StartGameButtonPresser)}]{message}");
        }

        private void Log(object message)
        {
            Debug.Log($"[{nameof(StartGameButtonPresser)}]{message}");
        }
    }
}