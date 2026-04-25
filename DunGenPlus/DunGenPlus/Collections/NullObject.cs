using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunGenPlus.Collections {

  // https://stackoverflow.com/questions/4632945/why-doesnt-dictionarytkey-tvalue-support-null-key
  internal struct NullObject<T> where T: UnityEngine.Object {
    public T Item;
    private bool isNull;

    public NullObject(T item, bool isNull) {
      this.Item = item;
      this.isNull = isNull;
    }


    public NullObject(T item) : this(item, item == null){

    }


    public static implicit operator T(NullObject<T> nullObject) {
      return nullObject.Item;
    }

    public static implicit operator NullObject<T>(T item) {
      return new NullObject<T>(item);
    }

    public override string ToString() {
      return (Item != null) ? Item.name : "NULL";
    }

    public override bool Equals(object obj) {
      if (obj == null) return isNull;
      if (!(obj is NullObject<T>)) return false;
      var no = (NullObject<T>)obj;
      if (isNull) return no.isNull;
      if (no.isNull) return false;
      return Item.Equals(no.Item);
    }

    public override int GetHashCode(){
      if (isNull) return 0;
      var result = Item.GetHashCode();
      if (result >= 0) result++;
      return result;
    }

  }
}
