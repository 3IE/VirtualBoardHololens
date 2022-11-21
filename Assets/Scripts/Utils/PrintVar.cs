using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class PrintVar : MonoBehaviour
{
    private static          TMP_Text                 _text;
    private static readonly Dictionary<uint, string> _lines       = new();
    private static readonly StringBuilder            _textToPrint = new();
    private static          uint                     _lineId;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    /// <summary>
    ///     Prints on a specific line
    /// </summary>
    /// <param name="n">Line to print on</param>
    /// <param name="args">Content to print</param>
    public static void print(uint n, params string[] args)
    {
        _lineId   = Math.Max(_lineId, n);
        _lines[n] = string.Join("\n", args);
        _textToPrint.Clear();
        _textToPrint.AppendJoin("\n", _lines.Values);
        _text.text = _textToPrint.ToString();
    }

    /// <summary>
    ///     Prints on the next line available
    /// </summary>
    /// <param name="args">Content to print</param>
    public static void print(params string[] args)
    {
        _lines[++_lineId] = string.Join("\n", args);
        _textToPrint.AppendJoin("\n\n", _lines.Values);
        _text.text = _textToPrint.ToString();
    }

    /// <summary>
    ///     Prints on the next line available, useful for in editor events
    /// </summary>
    /// <param name="args">Content to print</param>
    public void print(string s)
    {
        _lines[++_lineId] = s;
        _textToPrint.AppendJoin("\n\n", _lines.Values);
        _text.text = _textToPrint.ToString();
    }

    /// <summary>
    ///     Clears the text
    /// </summary>
    public static void Clear()
    {
        _lines.Clear();
        _textToPrint.Clear();
        _text.text = "";
    }
}
