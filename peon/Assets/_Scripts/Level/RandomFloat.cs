using UnityEngine;

namespace _Scripts.Level
{
    [System.Serializable]
    class RandomFloat
    {
        public float _min;
        public float _max;

        public float GetValue() { return Random.Range(_min, _max); }
    }

    [System.Serializable]
    class RandomInt
    {
        public int _min;
        public int _max;

        public int GetValue() { return Random.Range(_min, _max); }
    }
}