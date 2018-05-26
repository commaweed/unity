using System.Collections;
using System.Collections.Generic;

public static class Utility  {

   // use fisher-yates shuffle
   public static T[] ShuffleArray<T>(T[] array, int seed) {
      System.Random prng = new System.Random(seed);

      for (int i=0; i < array.Length - 1; i++) {
         int randomIndex = prng.Next(i, array.Length);
         // swap
         T tempItem = array[randomIndex];
         array[randomIndex] = array[i];
         array[i] = tempItem;
      }

      // it has now been shuffled
      return array;
   }
}
