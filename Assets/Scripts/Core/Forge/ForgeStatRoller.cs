using System;
using System.Collections.Generic;
using System.Linq;
using Core.Items;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Forge
{
    public static class ForgeStatRoller
    {
        public const int StepPercent = 2;

        // Uniformly choose a positive integer composition of the budget, in units of 2%.
        public static List<ItemStatState> Roll(int totalPercent, int count)
        {
            if (count < 1 || count > EquipmentStats.Ids.Count || totalPercent < count * StepPercent || totalPercent % StepPercent != 0)
                throw new ArgumentOutOfRangeException(nameof(totalPercent), "The stat budget must fit positive 2% bonuses.");
            var units = totalPercent / StepPercent;
            var cuts = new HashSet<int>();
            while (cuts.Count < count - 1) cuts.Add(Random.Range(1, units));
            var boundaries = new List<int>(cuts) { 0, units };
            boundaries.Sort();
            var available = new List<string>(EquipmentStats.Ids);
            var result = new List<ItemStatState>(count);
            for (var i = 0; i < count; i++)
            {
                var index = Random.Range(0, available.Count);
                result.Add(new ItemStatState { StatId = available[index], Value = (boundaries[i + 1] - boundaries[i]) * .02f });
                available.RemoveAt(index);
            }
            return result;
        }

        public static float Boost(float value, float bonus) =>
            Mathf.Max(.02f, Mathf.Round(value * (1f + bonus) * 50f) * .02f);

        // Preserve the saved stat IDs and their relative strengths, with positive 2% steps
        // and an exact total. Largest remainders resolve rounding; ties keep saved order.
        public static List<ItemStatState> Rebalance(IReadOnlyList<ItemStatState> stats, int totalPercent)
        {
            if (stats == null || stats.Count == 0 || !EquipmentStats.Validate(stats, out _) ||
                totalPercent < stats.Count * StepPercent || totalPercent % StepPercent != 0)
                throw new ArgumentException("Cannot rebalance these equipment bonuses.", nameof(stats));
            var units = new int[stats.Count];
            var remaining = totalPercent / StepPercent;
            var active = Enumerable.Range(0, stats.Count).ToList();
            while (active.Count > 0)
            {
                var weight = active.Sum(i => (double)stats[i].Value);
                double Share(int i) => remaining * (weight > 0 ? stats[i].Value / weight : 1d / active.Count);
                var minimums = active.Where(i => Share(i) < 1d).ToArray();
                if (minimums.Length > 0)
                {
                    foreach (var i in minimums) { units[i] = 1; remaining--; active.Remove(i); }
                    continue;
                }
                var shares = active.ToDictionary(i => i, Share);
                foreach (var i in active) units[i] = (int)Math.Floor(shares[i]);
                var extra = remaining - active.Sum(i => units[i]);
                foreach (var i in active.OrderByDescending(i => shares[i] - units[i]).ThenBy(i => i).Take(extra))
                    units[i]++;
                break;
            }
            return stats.Select((stat, i) => new ItemStatState { StatId = stat.StatId, Value = units[i] * .02f }).ToList();
        }
    }
}
