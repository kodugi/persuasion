using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationUtilsNameSpace
{
    public class AnimationUtils
    {
        public static IEnumerator ExecuteAccordingToCountsPreset<T>(List<T> list, Action<T> action)
        {
            List<int> countsPreset = new List<int>() {1, 4, 50};
            List<float> delaysWithinGroup = new List<float>() { 0.01f, 0.005f, 0.001f};
            List<float> delaysBetweenGroups = new List<float>() { 1f, 1f, 0.5f};
            
            int sum = 0;
            foreach (int count in countsPreset)
            {
                sum += count;
            }

            int acc = 0;
            List<int> counts = new List<int>();
            for (int i = 0; i < countsPreset.Count - 1; i++)
            {
                counts.Add((int)Math.Round((float)countsPreset[i] * list.Count / sum));
                acc += counts[i];
            }
            counts.Add(list.Count - acc);

            int offset = 0;
            
            for(int i = 0; i < countsPreset.Count; i++)
            {
                for (int j = 0; j < counts[i]; j++)
                {
                    if (j + offset >= list.Count)
                    {
                        break;
                    }
                    T item = list[j + offset];
                    action(item);
                    if (counts[i] >= 10)
                    {
                        if (j % 10 == 0)
                        {
                            yield return new WaitForSeconds(delaysWithinGroup[i] * 10);
                        }
                    }
                    else
                    {
                        yield return new WaitForSeconds(delaysWithinGroup[i]);
                    }
                }

                offset += counts[i];

                yield return new WaitForSeconds(delaysBetweenGroups[i] - delaysWithinGroup[i] * countsPreset[i]);
            }
        }
    }
}