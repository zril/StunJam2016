using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Enemy
{
    public GameObject Obj { get; private set; }
    public int OrigX { get; private set; }
    public int OrigY { get; private set; }
    public int DeltaMove { get; private set; }
    public float Speed { get; private set; }
    public bool IsVertical { get; private set; }
    public float CurrentDir { get; set; }

    public Enemy(GameObject obj, int origX, int origY, int deltaMove, float speed, bool isVertical)
    {
        Obj = obj;
        OrigX = origX;
        OrigY = origY;
        DeltaMove = deltaMove;
        Speed = speed;
        IsVertical = isVertical;
        CurrentDir = 1;
    }
}
