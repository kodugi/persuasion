using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationUtilsNameSpace
{
    public class AnimationUtils
    {
        
        public static IEnumerator ExecuteAccordingToCountsPreset<T>(List<T> list, Action<T> action, float delayWithinGroup = 0.001f, float delayBetweenGroups = 1.25f)
        {
            List<int> countsPreset = new List<int>() {1, 4, 9};
            
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
            foreach (int count in counts)
            {
                for (int i = 0; i < count; i++)
                {
                    if (i + offset >= list.Count)
                    {
                        break;
                    }
                    T item = list[i + offset];
                    action(item);
                    yield return new WaitForSeconds(delayWithinGroup);
                }

                offset += count;

                yield return new WaitForSeconds(delayBetweenGroups - delayWithinGroup * count);
            }
        }
    }
}