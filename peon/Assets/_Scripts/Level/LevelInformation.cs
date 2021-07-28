using Game.Level.UnitData;
using UnityEngine;

namespace Game.Level
{
    [System.Serializable]
    class MaxUsage
    {
        public UnitDatas Target;
        public int MaxUsageTimes;
        public int CurrentUsageTimes;
    }

    class LevelInformation : MonoBehaviour
    {
        public UnitData.UnitData[] UnitData;
        public SpawnPosition[] PlayerSpawnPositions;
        public RewardPosition[] RewardPositions;
        public TransferPosition[] TransferPositions;
        public MaxUsage[] MaxUsages;
    }
}
