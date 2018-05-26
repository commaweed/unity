public class HitItemChangeEvent {

   HitItemMetadata oldItem;
   public HitItemMetadata OldItem {
      get { return this.oldItem; }
      set { this.oldItem = value; }
   }

   HitItemMetadata newItem;
   public HitItemMetadata NewItem {
      get { return this.newItem; }
      set { this.newItem = value; }
   }

   public override string ToString() {
      return string.Format(
         "HitItemChangeEvent: OldItem: {0}   NewItem: {1}",
        this.oldItem == null ? "null" : this.oldItem.ToString(),
        this.newItem == null ? "null" : this.newItem.ToString()
      );
   }

   public bool AreHitItemsEqual() {
      if (oldItem == newItem) {
         return true;
      }
      if (oldItem == null || newItem == null) {
         return false;
      }
      return oldItem.Equals(newItem);
   }

   public bool DidLayerChange() {
      if (AreHitItemsEqual()) { 
         return false;
      }
      if (oldItem == null || newItem == null) {
         return true;
      }
      return oldItem.LayerHit != newItem.LayerHit;
   }
}
