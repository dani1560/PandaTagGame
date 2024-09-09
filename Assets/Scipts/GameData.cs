using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="GameData",menuName ="Settings/GameData",order =0)]
public class GameData : ScriptableObject
{
    public bool[] hats;
    public bool[] skin;
}
