using UnityEngine;

namespace DeadWrongGames.ZUtils
{
    public class CryptoKeyContainer : ScriptableObject
    {
        [HideInInspector] public string IV;
        [HideInInspector] public string Key;
    }
}