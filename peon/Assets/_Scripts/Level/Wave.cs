using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Game.Level
{
    [System.Serializable]
    class Wave
    {
        public List<UnitData.UnitData> WaveEnemys = new List<UnitData.UnitData>();

        public static Wave Generate(UnitData.UnitData[] unitData, float maxPower, MaxUsage[] maxUsages)
        {
            var wave = new Wave();

            int i = 0;
            float currentPower = 0;
            while(++i < 1000)
            {
                var data = unitData[UnityEngine.Random.Range(0, unitData.Length)];
                if (data.UnitPower + currentPower <= maxPower)
                    for (int j = 0; j < maxUsages.Length; j++)
                    {
                        if (data.GetType() == maxUsages[j].Target.GetType())
                        {
                            var field = data.GetType().GetField("UsageTimes", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                            if ((int)field.GetValue(null) < maxUsages[j].MaxUsageTimes)
                            {
                                field.SetValue(null, (int)field.GetValue(null) + 1);

                                wave.WaveEnemys.Add(data);
                            }
                        }
                    }
            }

            return wave;
        }
    }
}
