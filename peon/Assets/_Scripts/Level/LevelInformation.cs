using _Scripts.Level.UnitData;
using UnityEngine;

namespace _Scripts.Level
{
    [System.Serializable]
    class MaxUsage
    {
        public UnitDatas Target;
        public int MaxUsageTimes;
        public int CurrentUsageTimes;
    }

    [System.Serializable]
    class WaveInfo
    {
        public UnitData.UnitData[] UnitData;
        public MaxUsage[] MaxUsages;
    }

    class LevelInformation : MonoBehaviour
    {
        public SpawnPosition[] PlayerSpawnPositions;
        public SpawnPosition[] EnemySpawnPositions;
        public SpawnPosition[] TransferPositions;

        [Space]

        public WaveInfo[] WaveInfos;
    }
}
