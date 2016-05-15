using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Block
{
    public GameObject Obj { get; set; }
    public bool IsFixed { get; set; }
    public bool IsDestrutible { get; set; }
    public bool IsKill { get; set; }
    public float Vspeed { get; set; }
    public float spawnTimer { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public Block (GameObject obj, int x, int y)
    {
        Obj = obj;
        IsFixed = true;
        IsDestrutible = false;
        IsKill = false;
        X = x;
        Y = y;
        spawnTimer = 0;
    }
}
