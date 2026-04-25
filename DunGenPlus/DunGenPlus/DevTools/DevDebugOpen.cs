using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace DunGenPlus.DevTools {
  internal class DevDebugOpen : MonoBehaviour {

    public static bool IsSinglePlayerInShip(){
      var startOfRound = StartOfRound.Instance;
      var roundManager = RoundManager.Instance;
      if (startOfRound && roundManager) {
        return startOfRound.connectedPlayersAmount == 0 && startOfRound.inShipPhase;
      }
      return false;
    }

    public void Update(){
      if (IfKeyPress(Keyboard.current.mKey, Keyboard.current.leftAltKey) && DevDebugManager.Instance == null && IsSinglePlayerInShip()){
        Instantiate(Assets.DevDebugPrefab);
      }
    }

    bool IfKeyPress(KeyControl key, KeyControl hold){
      if (hold.isPressed && key.wasPressedThisFrame) return true;
      return false;
    }

  }
}
