using System;
using System.Collections.Generic;
using System.Text;
using FastReport.Data;
using FastReport.Engine;

namespace FastReport.Code
{
  /// <summary>
  /// This class is used to pass find arguments to some methods of the <b>CodeUtils</b> class.
  /// </summary>
  public class FindTextArgs
  {
    /// <summary>
    /// The start position of the search. After the search, this property points to
    /// the begin of an expression.
    /// </summary>
    public int StartIndex;

    /// <summary>
    /// After the search, this property points to the end of an expression.
    /// </summary>
    public int EndIndex;

    /// <summary>
    /// The char sequence used to find the expression's begin.
    /// </summary>
    public string OpenBracket;

    /// <summary>
    /// The char sequence used to find the expression's end.
    /// </summary>
    public string CloseBracket;

    /// <summary>
    /// The text with embedded expressions.
    /// </summary>
    public string Text;
    
    /// <summary>
    /// The last found expression.
    /// </summary>
    public string FoundText;
  }
  
  /// <summary>
  /// This static class contains methods that may be used to find expressions embedded 
  /// in the object's text.
  /// </summary>
  public static class CodeUtils
  {
    // adjusts StartIndex to the next char after end of string. Returns true if string is correct.
    private static bool SkipString(FindTextArgs args)
    {
      if (args.Text[args.StartIndex] == '"')
        args.StartIndex++;
      else
        return true;
      
      while (args.StartIndex < args.Text.Length)
      {
        if (args.Text[args.StartIndex] == '"')
        {
          if (args.Text[args.StartIndex - 1] != '\\')
          {
            args.StartIndex++;
            return true;
          }  
        }
        args.StartIndex++;
      }
      return false;
    }

    // find matching open and close brackets starting from StartIndex. Takes strings into account.
    // Returns true if matching brackets found. Also returns FoundText with text inside brackets, 
    // StartIndex pointing to the OpenBracket and EndIndex pointing to the next char after CloseBracket.
    private static bool FindMatchingBrackets(FindTextArgs args, bool skipLeadingStrings)
    {
      if (!skipLeadingStrings)
      {
        args.StartIndex = args.Text.IndexOf(args.OpenBracket, args.StartIndex);
        if (args.StartIndex == -1)
          return false;
      }  

      int saveStartIndex = 0;
      int brCount = 0;

      while (args.StartIndex < args.Text.Length)
      {
        if (!SkipString(args))
          return false;
        if (args.StartIndex + args.OpenBracket.Length > args.Text.Length)
          return false;
        if (args.Text.Substring(args.StartIndex, args.OpenBracket.Length) == args.OpenBracket)
        {
          if (brCount == 0)
            saveStartIndex = args.StartIndex;
          brCount++;
        }  
        else if (args.Text.Substring(args.StartIndex, args.CloseBracket.Length) == args.CloseBracket)
        {
          brCount--;
          if (brCount == 0)
          {
            args.EndIndex = args.StartIndex + args.CloseBracket.Length;
            args.StartIndex = saveStartIndex;
            args.FoundText = args.Text.Substring(args.StartIndex + args.OpenBracket.Length,
              args.EndIndex - args.StartIndex - args.OpenBracket.Length - args.CloseBracket.Length);
            return true;
          }
        }
        args.StartIndex++;
      }
      return false;
    }

    // determines whether given index is inside brackets, or is after OpenBracket
    internal static bool IndexInsideBrackets(FindTextArgs args)
    {
      int pos = args.StartIndex;
      args.StartIndex = 0;

      while (args.StartIndex < pos)
      {
        // find open bracket
        args.StartIndex = args.Text.IndexOf(args.OpenBracket, args.StartIndex);
        if (args.StartIndex == -1)
          return false;

        // missing close bracket
        if (!FindMatchingBrackets(args, false))
          return true;
        // pos is inside brackets
        if (args.StartIndex < pos && args.EndIndex > pos)
          return true;
        args.StartIndex = args.EndIndex;
      }
      return false;
    }

    /// <summary>
    /// Returns expressions found in the text.
    /// </summary>
    /// <param name="text">Text that may contain expressions.</param>
    /// <param name="openBracket">The char sequence used to find the start of expression.</param>
    /// <param name="closeBracket">The char sequence used to find the end of expression.</param>
    /// <returns>Array of expressions if found; otherwise return an empty array.</returns>
    public static string[] GetExpressions(string text, string openBracket, string closeBracket)
    {
      List<string> expressions = new List<string>();
      FindTextArgs args = new FindTextArgs();
      args.Text = text;
      args.OpenBracket = openBracket;
      args.CloseBracket = closeBracket;

      while (args.StartIndex < text.Length)
      {
        if (!FindMatchingBrackets(args, false))
          break;
        expressions.Add(args.FoundText);
        args.StartIndex = args.EndIndex;
      }
      
      return expressions.ToArray();
    }

    /// <summary>
    /// Gets first expression found in the text. 
    /// </summary>
    /// <param name="args">Object with find arguments.</param>
    /// <param name="skipStrings">Indicates whether to skip strings.</param>
    /// <returns>The expression if found; otherwise, returns an empty string.</returns>
    public static string GetExpression(FindTextArgs args, bool skipStrings)
    {
      while (args.StartIndex < args.Text.Length)
      {
        if (!FindMatchingBrackets(args, skipStrings))
          break;
        return args.FoundText;
      }
      return "";
    }
  }
}
