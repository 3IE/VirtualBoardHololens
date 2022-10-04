using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class PrintVar : MonoBehaviour
{
    private static TMP_Text _text;
    private static Dictionary<uint, string> _lines = new Dictionary<uint, string>();
    private static StringBuilder _textToPrint = new StringBuilder();
    private static uint _lineId = 0;
    private void Awake() 
        => _text = GetComponent<TMP_Text>();
    public static void print(uint n, params string[] args) { // Prints on a specific line
        _lineId = Math.Max(_lineId, n);
        _lines[n] =  string.Join("\n", args);
        _textToPrint.Clear();
        _textToPrint.AppendJoin("\n", _lines.Values);
        _text.text = _textToPrint.ToString();
    }
    public static void print(params string[] args) { // Prints on the next line available
        _lines[++_lineId] =  string.Join("\n", args);
        _textToPrint.AppendJoin("\n\n", _lines.Values);
        _text.text = _textToPrint.ToString();
    }
    public static void Clear() {
        _lines.Clear();
        _textToPrint.Clear();
        _text.text = "";
    }
}