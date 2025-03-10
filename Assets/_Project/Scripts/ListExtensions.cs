using System;
using System.Collections.Generic;
using System.Linq;

public static class ListExtensions
{
   private static Random rng;

   public static IList<T> Shuffle<T>(this IList<T> list) {
      if (rng == null) rng = new Random();
      int count = list.Count;
      while (count > 1) {
         --count;
         int index = rng.Next(count + 1);
         (list[index], list[count]) = (list[count], list[index]);
      }
      return list;
   }
}

public static class StringExtensions
{
   public static int ComputeFNV1aHash(this string str)
   {
      uint hash = 2166136261;
      foreach (char c in str) {
         hash = (hash ^ c) * 16777619;
      }
      return unchecked((int)hash);
   }
}
