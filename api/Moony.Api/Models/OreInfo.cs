namespace Moony.Api.Models;

public static class OreInfo
{
    // Ore TypeID → (Rarity, SortOrder within rarity tier)
    private static readonly Dictionary<int, (int Rarity, int Sort)> OreMap = new()
    {
        // R64
        [45510] = (64, 0), // Xenotime
        [45511] = (64, 1), // Monazite
        [45512] = (64, 2), // Loparite
        [45513] = (64, 3), // Ytterbite
        // R32
        [45502] = (32, 0), // Carnotite
        [45503] = (32, 1), // Zircon
        [45504] = (32, 2), // Pollucite
        [45506] = (32, 3), // Cinnabar
        // R16
        [45498] = (16, 0), // Otavite
        [45499] = (16, 1), // Sperrylite
        [45500] = (16, 2), // Vanadinite
        [45501] = (16, 3), // Chromite
        // R8
        [45494] = (8, 0),  // Cobaltite
        [45495] = (8, 1),  // Euxenite
        [45496] = (8, 2),  // Titanite
        [45497] = (8, 3),  // Scheelite
        // R4
        [45490] = (4, 0),  // Zeolites
        [45491] = (4, 1),  // Sylvite
        [45492] = (4, 2),  // Bitumens
        [45493] = (4, 3),  // Coesite
    };

    public static int GetRarity(int oreTypeId) =>
        OreMap.TryGetValue(oreTypeId, out var info) ? info.Rarity : 0;

    public static int GetSortOrder(int oreTypeId) =>
        OreMap.TryGetValue(oreTypeId, out var info) ? info.Sort : 99;
}
