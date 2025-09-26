using System;
using UnityEngine;

public interface ICellObject
{
        public Action<GameObject> OnObjectPlaced { get; set; }
        public Action<GameObject> OnObjectRemoved { get; set; }
}