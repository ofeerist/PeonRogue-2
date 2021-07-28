﻿using Game.Level.UnitData;
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

    [System.Serializable]
    class WaveInfo
    {
        public UnitData.UnitData[] UnitData;
        public MaxUsage[] MaxUsages;
    }

    class LevelInformation : MonoBehaviour
    {
        public SpawnPosition[] PlayerSpawnPositions;
        public RewardPosition[] RewardPositions;
        public TransferPosition[] TransferPositions;

        [Space]

        public WaveInfo[] WaveInfos;
    }
}