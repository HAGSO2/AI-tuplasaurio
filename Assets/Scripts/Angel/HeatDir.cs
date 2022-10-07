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
        Left = 3,
        Unknow = 4
    }

    private Dir _myDir;
    public Transform NextPoint { get; private set; }

    public HeatDir(Dir d)
    {
        _myDir = d;
    }
    public HeatDir(Dir d,Transform t)
    {
        _myDir = d;
        NextPoint = t;
    }

    public bool NotNull(){return _myDir != Dir.Unknow;}
    
    private bool Opposite(Dir d)
    {
        switch (d)
        {
            case Dir.Forward:
                return _myDir == Dir.BackWard;
            case Dir.BackWard:
                return _myDir == Dir.Forward;
            case Dir.Right:
                return _myDir == Dir.Left;
            case Dir.Left:
                return _myDir == Dir.Right;
            default:
                throw new ArgumentOutOfRangeException(nameof(d), d, null);
        }
    }
    public static bool operator !=(HeatDir h1, HeatDir h2)
    {
        if (h1!._myDir == Dir.Unknow || h2!._myDir == Dir.Unknow)
            return false;
        return h1!.Opposite(h2!._myDir);
    }

    public static bool operator ==(HeatDir h1, HeatDir h2)
    {
        return !(h1 != h2);
    }

    public override bool Equals(object obj)
    {
        return ((HeatDir)obj)._myDir == _myDir;
    }

    public override int GetHashCode()
    {
        return (int)_myDir;
    }
}