using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RngList<T> where T: RngItem {
    [Tooltip("Actual list containing the random bot part elements")]
    public List<T> list;

    [Tooltip("Should we use the last item as fallback? (in case none of the items were selected)")]
    public bool useLastAsFallback;

    // If the none subset is enabled, the list technically becomes of size n+1 with the added none subset with the specific weight
    public RngItem noneSubset;

    public T PickRandom() {
        // Convert to a list with indirection index to main list (since it is shuffled)
        List<(int, float)> indexed = list.Select((x, i) => (i, x)).Where(x => x.x.enabled).Select(x => (x.i, x.x.weight)).ToList();
        if (noneSubset.enabled) {
            indexed.Add((-1, noneSubset.weight));
        }
        //indexed = indexed.OrderByDescending(x => x.Item2).ToList();
        indexed = indexed.OrderByDescending(_ => UnityEngine.Random.value).ToList();

        float cursor = 0f;
        int index = -1;

        // Modified https://dev.to/jacktt/understanding-the-weighted-random-algorithm-581p
        // Normalize the values (so a list of weights [1,1,1] has a 1/3 chance to pick one element if none is not used)
        float total = indexed.Select(x => x.Item2).Sum();
        float val = Mathf.Ceil(UnityEngine.Random.value * total);
        for (int i = 0; i < indexed.Count; i++) {
            cursor += indexed[i].Item2;

            if (cursor >= val) {
                index = indexed[i].Item1;
                break;
            }
        }

        if (index == -1 && useLastAsFallback) {
            index = list.Count - 1;
        }

        return index >= 0 ? list[index] : null;
    }
}

