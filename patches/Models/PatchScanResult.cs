namespace patches.Models;

public sealed class PatchScanResult
{
    public required PatchDefinition Definition { get; init; }

    public PatchVariant? Variant { get; init; }

    public PatchStatus Status { get; init; }

    public IntPtr Address { get; init; }

    public string Message { get; init; } = string.Empty;
}
