using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunGenPlus.Collections {

  
  public struct EventCallbackScenario {
    public bool IsDevDebug;

    public EventCallbackScenario(bool isDevDebug){
      IsDevDebug = isDevDebug;
    }

  }


  public class ExtenderEvent<T> {

    internal event ParameterEvent onParameterEvent;

    /// <summary>
    /// Calls listeners.
    /// </summary>
    /// <param name="param"></param>
    public void Invoke(T param1, EventCallbackScenario param2) {
      onParameterEvent?.Invoke(param1, param2);
    }

    /// <summary>
    /// Adds listener.
    /// </summary>
    /// <param name="listener"></param>
    public void AddListener(ParameterEvent listener) {
      onParameterEvent += listener;
    }

    /// <summary>
    /// Removes listener.
    /// </summary>
    /// <param name="listener"></param>
    public void RemoveListener(ParameterEvent listener) {
      onParameterEvent -= listener;
    }

    public delegate void ParameterEvent(T param1, EventCallbackScenario param2);
  }

}
