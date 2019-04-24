using System;

namespace CanTheSpam.Extensions
{
   public static class HelperExtensions
   {
      public static void PrintMessage(ConsoleColor foregroundColor, string message)
      {
         Console.ForegroundColor = foregroundColor;
         Console.WriteLine(message);
         Console.ResetColor();
      }

      public static bool IsNull(this object item)
      {
         return item == null;
      }

      public static bool IsNotNull(this object item)
      {
         return item != null;
      }

      public static bool IsNotEmptyOrWhiteSpace(this string value)
      {
         return !string.IsNullOrWhiteSpace(value);
      }

      public static bool IsEmptyOrWhiteSpace(this string value)
      {
         return string.IsNullOrWhiteSpace(value);
      }

      public static string RemoveSurroundingQuotes(this string value)
      {
         string returnValue = value;

         if (returnValue.StartsWith("\""))
         {
            returnValue = returnValue.Substring(1);
         }

         if (returnValue.EndsWith("\""))
         {
            int len = returnValue.Length - 1;
            returnValue = returnValue.Substring(0, len);
         }

         return returnValue;
      }
   }
}
