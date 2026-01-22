using System.Collections.Generic;
using MagicKeyboardUtilities.App.Config;

namespace MagicKeyboardUtilities.App.Core
{
    public class RemappingEngine
    {
        private List<RemapEntry> _mappings;

        public RemappingEngine(List<RemapEntry> mappings)
        {
            _mappings = mappings;
        }

        public RemapEntry? FindMap(int vkCode)
        {
            foreach (var map in _mappings)
            {
                if (map.FromVk == vkCode) return map;
            }
            return null;
        }
    }
}
