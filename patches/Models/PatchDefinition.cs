namespace patches.Models;

public sealed class PatchDefinition
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public required string Category { get; init; }

    public required string Description { get; init; }

    public required string Source { get; init; }

    public required IReadOnlyList<PatchVariant> Variants { get; init; }
}

public sealed class PatchVariant
{
    public required string Label { get; init; }

    public required string SearchPattern { get; init; }

    public required int PatchOffset { get; init; }

    public required byte[] PatchBytes { get; init; }
}
