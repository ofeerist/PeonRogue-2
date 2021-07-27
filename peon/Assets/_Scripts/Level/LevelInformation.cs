using UnityEngine;

namespace Game.Level
{
    [System.Serializable]
    class MaxUsage
    {
        public UnitData.UnitData Target;
        public int MaxUsageTimes;
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
