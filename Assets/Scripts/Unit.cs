using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Unit
{
    public string name;

    public int cost;

    public GameObject prefab;

    [HideInInspector]
    public AudioSource source;
}
