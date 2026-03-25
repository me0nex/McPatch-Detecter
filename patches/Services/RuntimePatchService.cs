using patches.Models;

namespace patches.Services;

public sealed class RuntimePatchService
{
    public IReadOnlyList<PatchScanResult> Scan(IReadOnlyList<MemoryRegion> regions, IEnumerable<PatchDefinition> patches)
    {
        return patches.Select(patch => ScanPatch(regions, patch)).ToList();
    }

    private static PatchScanResult ScanPatch(IReadOnlyList<MemoryRegion> regions, PatchDefinition definition)
    {
        foreach (var variant in definition.Variants)
        {
            var originalPattern = BytePatternParser.ParsePattern(variant.SearchPattern);
            foreach (var region in regions)
            {
                var originalIndex = FindPattern(region.Bytes, originalPattern);
                if (originalIndex >= 0)
                {
                    var targetIndex = originalIndex + variant.PatchOffset;
                    if (targetIndex < 0 || targetIndex + variant.PatchBytes.Length > region.Bytes.Length)
                    {
                        continue;
                    }

                    var currentBytes = region.Bytes.Skip(targetIndex).Take(variant.PatchBytes.Length).ToArray();
                    return new PatchScanResult
                    {
                        Definition = definition,
                        Variant = variant,
                        Status = currentBytes.SequenceEqual(variant.PatchBytes) ? PatchStatus.Patched : PatchStatus.NotPatched,
                        Address = region.BaseAddress + targetIndex,
                        Message = currentBytes.SequenceEqual(variant.PatchBytes)
                            ? $"{variant.Label}: patched"
                            : $"{variant.Label}: no patched"
                    };
                }

                var patchedPattern = BuildPatchedPattern(originalPattern, variant.PatchOffset, variant.PatchBytes);
                var patchedIndex = FindPattern(region.Bytes, patchedPattern);
                if (patchedIndex < 0)
                {
                    continue;
                }

                return new PatchScanResult
                {
                    Definition = definition,
                    Variant = variant,
                    Status = PatchStatus.Patched,
                    Address = region.BaseAddress + patchedIndex + variant.PatchOffset,
                    Message = $"{variant.Label}: patched"
                };
            }
        }

        return new PatchScanResult
        {
            Definition = definition,
            Status = PatchStatus.NotFound,
            Message = "Signature was not found in scanned Bedrock sections."
        };
    }

    private static byte?[] BuildPatchedPattern(byte?[] originalPattern, int patchOffset, byte[] patchBytes)
    {
        var clone = originalPattern.ToArray();
        for (var i = 0; i < patchBytes.Length; i++)
        {
            var index = patchOffset + i;
            if (index < 0 || index >= clone.Length)
            {
                break;
            }

            clone[index] = patchBytes[i];
        }

        return clone;
    }

    private static int FindPattern(byte[] data, IReadOnlyList<byte?> pattern)
    {
        if (pattern.Count == 0 || data.Length < pattern.Count)
        {
            return -1;
        }

        for (var i = 0; i <= data.Length - pattern.Count; i++)
        {
            var matched = true;
            for (var j = 0; j < pattern.Count; j++)
            {
                if (pattern[j].HasValue && data[i + j] != pattern[j])
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return i;
            }
        }

        return -1;
    }
}
