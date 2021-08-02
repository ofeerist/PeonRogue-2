using System.Collections.Generic;
using _Scripts.Level.UnitData;

namespace _Scripts.Level
{
    [System.Serializable]
    class Wave
    {
        public List<UnitData.UnitData> WaveEnemies = new List<UnitData.UnitData>();

        public static Wave Generate(UnitData.UnitData[] unitData, float maxPower, MaxUsage[] maxUsages)
        {
            var wave = new Wave();

            var dict = new Dictionary<UnitDatas, MaxUsage>();

            for (int k = 0; k < maxUsages.Length; k++)
            {
                dict.Add(maxUsages[k].Target, maxUsages[k]);
                maxUsages[k].CurrentUsageTimes = 0;
            }

            int i = 0;
            float currentPower = 0;
            while(++i < 100)
            {
                var data = unitData[UnityEngine.Random.Range(0, unitData.Length)];
                
                if (data.UnitPower + currentPower <= maxPower)
                {
                    var maxUsage = dict[data.Type];

                    if (maxUsage.CurrentUsageTimes < maxUsage.MaxUsageTimes)
                    {
                        maxUsage.CurrentUsageTimes += 1;

                        currentPower += data.UnitPower;
                        wave.WaveEnemies.Add(data);
                    }
                    
                }
            }

            return wave;
        }

    }
}
