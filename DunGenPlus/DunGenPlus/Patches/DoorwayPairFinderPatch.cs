using DunGen;
using DunGenPlus.Generation;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunGenPlus.Patches {

  internal class DoorwayPairFinderPatch {

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DoorwayPairFinder), "GetPotentialDoorwayPairsForNonFirstTile")]
    public static void GenerateBranchPathsPatch(ref DoorwayPairFinder __instance, ref IEnumerable<DoorwayPair> __result){
      if (DunGenPlusGenerator.Active) {
        __result = DunGenPlusGenerator.GetPotentialDoorwayPairsForNonFirstTileAlternate(__instance); 
      }
    }

    

  }

}
