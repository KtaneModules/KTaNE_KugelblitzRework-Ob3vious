using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class KugelblitzColor
{
    public static KugelblitzColor None = new KugelblitzColor(new Color(0, 0, 0, 0), "");
    public static KugelblitzColor Black = new KugelblitzColor(new Color(0, 0, 0), "K");
    public static KugelblitzColor White = new KugelblitzColor(new Color(1, 1, 1), "W");
    public static KugelblitzColor Red = new KugelblitzColor(new Color(1, 0, 0), "R");
    public static KugelblitzColor Orange = new KugelblitzColor(new Color(1, 0.5f, 0), "O");
    public static KugelblitzColor Yellow = new KugelblitzColor(new Color(1, 1, 0), "Y");
    public static KugelblitzColor Green = new KugelblitzColor(new Color(0, 1, 0), "G");
    public static KugelblitzColor Blue = new KugelblitzColor(new Color(0, 0.75f, 1), "B");
    public static KugelblitzColor Indigo = new KugelblitzColor(new Color(0.125f, 0, 1), "I");
    public static KugelblitzColor Violet = new KugelblitzColor(new Color(0.75f, 0, 1), "V");

    private Color _color;
    private string _name;

    public KugelblitzColor(Color color, string name)
    {
        _color = color;
        _name = name;
    }

    public Color GetColor() { return _color; }
    public string GetName() { return _name; }

    public static KugelblitzColor GetColorFromIndex(int index)
    {
        switch (index)
        {
            case 0:
                return Red;
            case 1:
                return Orange;
            case 2:
                return Yellow;
            case 3:
                return Green;
            case 4:
                return Blue;
            case 5:
                return Indigo;
            case 6:
                return Violet;
            default:
                return None;
        }
    }

    public static KugelblitzColor GetRandomHue(string name)
    {
        List<float> values = new List<float> { 0f, 1f, Rnd.Range(0f, 1f) }.Shuffle();
        return new KugelblitzColor(new Color(values[0], values[1], values[2]), name);
    }
}
