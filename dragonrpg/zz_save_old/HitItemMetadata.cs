using UnityEngine;

public class HitItemMetadata {
   RaycastHit itemHit;
   public RaycastHit ItemHit {
      get { return this.itemHit; }
      set { this.itemHit = value; }
   }

   Layer layerHit;
   public Layer LayerHit {
      get { return this.layerHit; }
      set { this.layerHit = value; }
   }

   public override string ToString() {
      return string.Format(
         "[ ItemHit: {0}, LayerHit: {1} ]",
         ItemHit.transform, LayerHit
      );
   }

   public override bool Equals(object obj) {
      bool result = false;

      HitItemMetadata other = obj as HitItemMetadata;
      if (other != null) {
         result = this.LayerHit == other.LayerHit;
         result &= this.itemHit.collider != null && other.itemHit.collider != null;
         result &= this.itemHit.collider.Equals(other.itemHit.collider);
      }

      return result;
   }

   public override int GetHashCode() {
      return base.GetHashCode(); // not really using hashcode at this time
   }

}
