using UnityEngine;

namespace _Scripts.Level.UnitData
{
    class UnitData : ScriptableObject
    {
        public float UnitPower;
        public Unit.Unit Prefab;

        public UnitDatas Type;

        public virtual void SetData(Unit.Unit unit) { }
    }
}
