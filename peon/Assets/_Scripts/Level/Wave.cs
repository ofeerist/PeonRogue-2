using System.Collections.Generic;


namespace Game.Level
{
    [System.Serializable]
    class Wave
    {
        public List<UnitData.UnitData> WaveEnemies = new List<UnitData.UnitData>();


        public static Wave Generate(UnitData.UnitData[] unitData, float maxPower, MaxUsage[] maxUsages)
        {
            var wave = new Wave();

            for (int k = 0; k < maxUsages.Length; k++)
                maxUsages[k].CurrentUsageTimes = 0;
            
            int i = 0;
            float currentPower = 0;
            while(++i < 100)
            {
                var data = unitData[UnityEngine.Random.Range(0, unitData.Length)];

                if (data.UnitPower + currentPower <= maxPower)
                    for (int j = 0; j < maxUsages.Length; j++)
                    {
                        if (data.Type == maxUsages[j].Target)
                        {
                            if (maxUsages[j].CurrentUsageTimes < maxUsages[j].MaxUsageTimes)
                            {
                                maxUsages[j].CurrentUsageTimes += 1;

                                currentPower += data.UnitPower;
                                wave.WaveEnemies.Add(data);
                            }
                        }
                    }
            }

            return wave;
        }

    }
}
