using DunGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DunGenPlus.Components.Props
{
  public class SpawnSyncedObjectCycle : MonoBehaviour, IDungeonCompleteReceiver {

    internal static int cycle;
    internal static Dictionary<int, int> cycleDictionary;

    [Tooltip("The SpawnSyncedObject reference.\n\nWhen the dungeon generation finishes, the spawnPrefab of the referenced SpawnSyncedObject will change to one of the Props based on a cycle. The starting value is random.\n\nThis is designed for the scenario where you have multiple very similar networked gameobjects that serve the same purpose, and you just want them all to spawn equally for diversity sake.")]
    public SpawnSyncedObject Spawn;
    [Tooltip("The unique id for this script's cycle.\n\nWhen the dungeon generation finishes, a random cycle value is calculated for each Id. Each script will reference their Id's corresponding cycle value to determine their Prop, and advance the cycle value by 1.")]
    public int Id;
    [Tooltip("The list of props that would selected based on a cycle.")]
    public List<GameObject> Props = new List<GameObject>();

    void Reset(){
      Spawn = GetComponent<SpawnSyncedObject>();
    }

    internal static void UpdateCycle(int value){
      Plugin.logger.LogDebug($"Updating SpawnSyncedObject start cycle to {value}");
      cycle = value;
      cycleDictionary = new Dictionary<int, int>();
    }

    internal int GetCycle(int id){
      if (!cycleDictionary.TryGetValue(id, out var value)){
        value = cycle;
        cycleDictionary.Add(id, value);
      }

      cycleDictionary[id] = value + 1;
      Plugin.logger.LogDebug($"Cycle{id}: {value}");
      return value;
    }

    public void OnDungeonComplete(Dungeon dungeon) {
      var index = GetCycle(Id) % Props.Count;
      var prefab = Props[index];
      Spawn.spawnPrefab = prefab;
    }
  }
}
