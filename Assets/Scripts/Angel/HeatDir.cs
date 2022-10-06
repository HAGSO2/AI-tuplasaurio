using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatDir
{
    public enum Dir
    {
        Forward = 0,
        BackWard = 1,
        Right = 2,
        Left = 3
    }

    private readonly Dir _myDir;

    public HeatDir(Dir d)
    {
        _myDir = d;
    }

    private bool Opposite(Dir d)
    {
        switch (d)
        {
            case Dir.Forward:
                return _myDir == Dir.BackWard ? true : false;
                break;
            case Dir.BackWard:
                return _myDir == Dir.Forward ? true : false;
                break;
            case Dir.Right:
                return _myDir == Dir.Left ? true : false;
                break;
            case Dir.Left:
                return _myDir == Dir.Right ? true : false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(d), d, null);
        }
    }
    public static bool operator !=(HeatDir h1, HeatDir h2)
    {
        return h1!.Opposite(h2!._myDir);
    }

    public static bool operator ==(HeatDir h1, HeatDir h2)
    {
        return !(h1 != h2);
    }
}