using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MagicKeyboardUtilities.App.Config;

namespace MagicKeyboardUtilities.App.Core;

/// <summary>
/// Remapping engine - handles key code translation
/// Traceability: Section 4.2 INPUT HOOK FLOW Step 3 "Event Filtering & Remapping"
/// Evidence: "Check remapping table, apply transform if match found"
/// Note: Actual mapping table is UNKNOWN due to encryption in original binary
/// </summary>
public class RemappingEngine
{
    private readonly ILogger<RemappingEngine> _logger;
    private readonly Dictionary<int, int> _mappingTable;

    public RemappingEngine(ILogger<RemappingEngine> logger)
    {
        _logger = logger;
        _mappingTable = new Dictionary<int, int>();
    }

    /// <summary>
    /// Load remapping table from configuration
    /// </summary>
    public void LoadMappings(List<KeyRemapping> mappings)
    {
        _mappingTable.Clear();
        
        foreach (var mapping in mappings)
        {
            if (_mappingTable.ContainsKey(mapping.FromVk))
            {
                _logger.LogWarning("Duplicate mapping for VK {VK}, overwriting", mapping.FromVk);
            }
            
            _mappingTable[mapping.FromVk] = mapping.ToVk;
            _logger.LogInformation("Loaded mapping: VK {From} -> VK {To} ({Desc})", 
                mapping.FromVk, mapping.ToVk, mapping.Description);
        }

        _logger.LogInformation("Loaded {Count} key mappings", _mappingTable.Count);
    }

    /// <summary>
    /// Try to remap a virtual key code
    /// Returns true if remapping was applied, false otherwise
    /// </summary>
    public bool TryRemap(int vkCode, out int remappedVk)
    {
        if (_mappingTable.TryGetValue(vkCode, out var mappedVk))
        {
            remappedVk = mappedVk;
            _logger.LogDebug("Remapped VK {From} -> VK {To}", vkCode, remappedVk);
            return true;
        }

        remappedVk = vkCode;
        return false;
    }

    /// <summary>
    /// Get all active mappings
    /// </summary>
    public IReadOnlyDictionary<int, int> GetMappings() => _mappingTable;

    /// <summary>
    /// Clear all mappings
    /// </summary>
    public void ClearMappings()
    {
        _mappingTable.Clear();
        _logger.LogInformation("Cleared all mappings");
    }
}
