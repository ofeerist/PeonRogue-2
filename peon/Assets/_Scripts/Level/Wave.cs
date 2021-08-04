using System.Collections.Generic;
using _Scripts.Level.UnitData;
using UnityEngine;

namespace _Scripts.Level
{
    [System.Serializable]
    class Wave
    {
        public List<UnitData.UnitData> WaveEnemies = new List<UnitData.UnitData>();
        public List<int> Indexes = new List<int>();

        public static Wave Generate(UnitData.UnitData[] unitData, float maxPower, MaxUsage[] maxUsages)
        {
            var wave = new Wave();

            var dict = new Dictionary<UnitDatas, MaxUsage>();

            foreach (var t in maxUsages)
            {
                dict.Add(t.Target, t);
                t.CurrentUsageTimes = 0;
            }

            int i = 0;
            float currentPower = 0;
            while(++i < 100)
            {
                var rnd = Random.Range(0, unitData.Length);
                var data = unitData[rnd];
                
                if (data.UnitPower + currentPower <= maxPower)
                {
                    var maxUsage = dict[data.Type];

                    if (maxUsage.CurrentUsageTimes < maxUsage.MaxUsageTimes)
                    {
                        maxUsage.CurrentUsageTimes += 1;

                        currentPower += data.UnitPower;
                        
                        wave.Indexes.Add(rnd);
                        wave.WaveEnemies.Add(data);
                    }
                    
                }
            }

            return wave;
        }

        public static Wave CreateByIndexes(UnitData.UnitData[] unitData, int[] indexes)
        {
            var wave = new Wave();
            
            foreach (var index in indexes)
                wave.WaveEnemies.Add(unitData[index]);
            
            return wave;
        }

    }
}
