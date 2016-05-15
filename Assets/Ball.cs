using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class Ball
{
    public GameObject Obj { get; set; }
    public float Speed { get; set; }
    public bool Vertical { get; set; }
    public float LifeTime { get; set; }

    public Ball(GameObject obj, float speed)
    {
        Obj = obj;
        Speed = speed;
        Vertical = false;
        LifeTime = 3f;
    }
}
