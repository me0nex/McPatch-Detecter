using patches.Models;

namespace patches.Services;

public static class McPatchCatalog
{
    public static IReadOnlyList<PatchDefinition> Create()
    {
        return
        [
            new PatchDefinition
            {
                Id = "gui-scale",
                DisplayName = "GuiScale",
                Category = "visual",
                Description = "McPatch patch. Forces a smaller GUI scale. For newer versions this follows patcher-style runtime value editing.",
                Source = "McPatch + patcher",
                Variants =
                [
                    new PatchVariant
                    {
                        Label = "McPatch / patcher",
                        SearchPattern = "00 00 ? ? 00 00 A0 40 00 00 C0 40",
                        PatchOffset = 0,
                        PatchBytes = BytePatternParser.ParseExactBytes("00 00 E0")
                    }
                ]
            },
            new PatchDefinition
            {
                Id = "teleport-rotation",
                DisplayName = "TeleportRotation",
                Category = "movement",
                Description = "Removes camera snap/rotation during teleport. Uses the old McPatch signature and the newer patcher variant.",
                Source = "McPatch + patcher",
                Variants =
                [
                    new PatchVariant
                    {
                        Label = "McPatch",
                        SearchPattern = "E8 ? ? ? ? 48 8B 03 48 8D 94 24 ? ? ? ? 48 8B 4B",
                        PatchOffset = 0,
                        PatchBytes = BytePatternParser.ParseExactBytes("90 90 90 90 90")
                    },
                    new PatchVariant
                    {
                        Label = "patcher",
                        SearchPattern = "E8 ? ? ? ? 48 8B 03 48 8D 54 24 ? 48 8B 4B",
                        PatchOffset = 0,
                        PatchBytes = BytePatternParser.ParseExactBytes("90 90 90 90 90")
                    }
                ]
            },
            new PatchDefinition
            {
                Id = "item-use-delay",
                DisplayName = "ItemUseDelay",
                Category = "combat",
                Description = "Removes the delay after attack before using items again.",
                Source = "McPatch + patcher",
                Variants =
                [
                    new PatchVariant
                    {
                        Label = "McPatch",
                        SearchPattern = "FF 15 ? ? ? ? 48 8B ? 48 8B ? 48 8B ? ? ? ? ? FF 15 ? ? ? ? 32 DB",
                        PatchOffset = 0,
                        PatchBytes = BytePatternParser.ParseExactBytes("90 90 90 90 90 90")
                    },
                    new PatchVariant
                    {
                        Label = "patcher",
                        SearchPattern = "FF 15 ? ? ? ? 48 8B 06 48 8B CE 48 8B 80 ? ? ? ? FF 15 ? ? ? ? 40 84 FF 75",
                        PatchOffset = 0,
                        PatchBytes = BytePatternParser.ParseExactBytes("90 90 90 90 90 90")
                    }
                ]
            },
            new PatchDefinition
            {
                Id = "minimal-view-bobbing",
                DisplayName = "MinimalViewBobbing",
                Category = "visual",
                Description = "Disables or heavily reduces walking camera bobbing.",
                Source = "McPatch + patcher",
                Variants =
                [
                    new PatchVariant
                    {
                        Label = "McPatch",
                        SearchPattern = "FF 15 ? ? ? ? 80 7C 24 60 ? 0F 84 ? ? ? ? 48 89",
                        PatchOffset = 0,
                        PatchBytes = BytePatternParser.ParseExactBytes("90 90 90 90 90 90")
                    },
                    new PatchVariant
                    {
                        Label = "patcher",
                        SearchPattern = "FF 15 ? ? ? ? 80 7C 24 ? ? 0F 84 ? ? ? ? F3 0F 10 4C 24 ? F3 0F 59 0D",
                        PatchOffset = 0,
                        PatchBytes = BytePatternParser.ParseExactBytes("90 90 90 90 90 90")
                    }
                ]
            },
            new PatchDefinition
            {
                Id = "no-hurt-cam",
                DisplayName = "NoHurtCam",
                Category = "visual",
                Description = "Removes hurt camera shake.",
                Source = "McPatch + patcher",
                Variants =
                [
                    new PatchVariant
                    {
                        Label = "McPatch",
                        SearchPattern = "EB ? 0F 57 C0 F3 0F 11 0B",
                        PatchOffset = 0,
                        PatchBytes = BytePatternParser.ParseExactBytes("90 90")
                    }
                ]
            },
            new PatchDefinition
            {
                Id = "no-jump-delay",
                DisplayName = "NoJumpDelay",
                Category = "movement",
                Description = "McPatch jump-delay patch. Includes the latest McPatch signature and the modern patcher variant.",
                Source = "McPatch + patcher",
                Variants =
                [
                    new PatchVariant
                    {
                        Label = "McPatch latest",
                        SearchPattern = "C7 47 ? ? ? ? ? 48 8B 9C 24 ? ? ? ? 0F 28 74 24 ? 48 81 C4",
                        PatchOffset = 0,
                        PatchBytes = BytePatternParser.ParseExactBytes("C7 47 10 00 00 00 00")
                    },
                    new PatchVariant
                    {
                        Label = "patcher",
                        SearchPattern = "C7 47 ? ? ? ? ? 48 8B 9C 24 ? ? ? ? 0F 28 74 24 ? 48 81 C4",
                        PatchOffset = 3,
                        PatchBytes = BytePatternParser.ParseExactBytes("00 00 00 00")
                    },
                    new PatchVariant
                    {
                        Label = "McPatch 1.16.100.4",
                        SearchPattern = "89 90 ? ? ? ? C3 CC CC CC CC CC 48 8B 41 ? 88 90 ? ? ? ? C3 CC CC CC CC CC 48 8B 49 ? E9 ? ? ? ? CC CC CC CC CC CC CC 48 8B 49",
                        PatchOffset = 0,
                        PatchBytes = BytePatternParser.ParseExactBytes("90 90 90 90 90 90")
                    }
                ]
            },
            new PatchDefinition
            {
                Id = "third-person-nametag",
                DisplayName = "ThirdPersonNametag",
                Category = "utility",
                Description = "Shows nametags in third person.",
                Source = "McPatch + patcher",
                Variants =
                [
                    new PatchVariant
                    {
                        Label = "McPatch / patcher",
                        SearchPattern = "0F 84 ? ? ? ? 49 8B 45 ? 49 8B CD 48 8B 80 ? ? ? ? FF 15 ? ? ? ? 84 C0 0F 85",
                        PatchOffset = 0,
                        PatchBytes = BytePatternParser.ParseExactBytes("90 90 90 90 90 90")
                    }
                ]
            },
        ];
    }
}
