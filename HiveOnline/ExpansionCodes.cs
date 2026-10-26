using HiveContracts;
using System.Collections.Generic;
using System.Linq;

namespace HiveOnline
{
    // Expansion bugs travel over the network and into the lobby as single letters (L, P, M).
    static class ExpansionCodes
    {
        private static readonly (BugType Type, char Code)[] Map =
        {
            (BugType.LadyBug, 'L'),
            (BugType.PillBug, 'P'),
            (BugType.Mosquito, 'M'),
        };

        public static string ToCodes(IEnumerable<BugType> expansions)
        {
            var selected = expansions?.ToHashSet() ?? new HashSet<BugType>();
            return new string(Map.Where(m => selected.Contains(m.Type)).Select(m => m.Code).ToArray());
        }

        public static BugType[] FromCodes(string codes)
        {
            codes ??= string.Empty;
            return Map.Where(m => codes.Contains(m.Code)).Select(m => m.Type).ToArray();
        }
    }
}
