using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buffs;

namespace MoatCharms
{
    /// <summary>One amulet type: identity, shop price, sprite cell, and the blessing it grants while worn.</summary>
    public class AmuletDef
    {
        public string Id;                 // short id, e.g. "somdej"
        public int SpriteIndex;           // cell in assets/charms.png
        public int Price;
        public Func<BuffEffects> Effects; // blessing while worn (null = no passive buff)

        public string ItemId => $"{ModEntry.ModId}_{this.Id}";
        public string QualifiedItemId => $"(O){this.ItemId}";
    }

    public static class Amulets
    {
        public const string CharmsAsset = "Mods/NaNoBotCo.MoatCharms/Charms";
        public const int OmenSpriteIndex = 6;
        public const int RestingSpriteIndex = 7;

        public static readonly List<AmuletDef> All = new()
        {
            new AmuletDef { Id = "somdej",    SpriteIndex = 0, Price = 8000, Effects = () => new BuffEffects { Defense = { Value = 2 } } },
            new AmuletDef { Id = "takrut",    SpriteIndex = 1, Price = 4500, Effects = () => new BuffEffects { Immunity = { Value = 2 } } },
            new AmuletDef { Id = "sing",      SpriteIndex = 2, Price = 4500, Effects = () => new BuffEffects { Attack = { Value = 2 } } },
            new AmuletDef { Id = "paladkhik", SpriteIndex = 3, Price = 3000, Effects = () => new BuffEffects { MagneticRadius = { Value = 96 } } },
            new AmuletDef { Id = "nangkwak",  SpriteIndex = 4, Price = 5000, Effects = () => new BuffEffects { FarmingLevel = { Value = 1 } } },
            new AmuletDef { Id = "jingjok",   SpriteIndex = 5, Price = 2500, Effects = null }, // blessing = foresight, handled in the menu
        };

        public static AmuletDef Get(string shortId)
            => All.FirstOrDefault(a => a.Id == shortId);

        public static AmuletDef GetByItemId(string itemId)
            => All.FirstOrDefault(a => a.ItemId == itemId || a.QualifiedItemId == itemId);

        public static Texture2D Texture()
            => Game1.content.Load<Texture2D>(CharmsAsset);

        /// <summary>Inject the amulets into Data/Objects so they exist as real items.</summary>
        public static void EditObjectData(IDictionary<string, StardewValley.GameData.Objects.ObjectData> data, ITranslationHelper i18n)
        {
            foreach (AmuletDef def in All)
            {
                data[def.ItemId] = new StardewValley.GameData.Objects.ObjectData
                {
                    Name = $"Moat Amulet {def.Id}",
                    DisplayName = i18n.Get($"amulet.{def.Id}.name"),
                    Description = i18n.Get($"amulet.{def.Id}.desc"),
                    Type = "Basic",
                    Category = 0,
                    Price = def.Price / 4,
                    Texture = CharmsAsset,
                    SpriteIndex = def.SpriteIndex,
                    Edibility = -300,
                    ExcludeFromRandomSale = true,
                    ContextTags = new List<string> { "moat_amulet" }
                };
            }
        }
    }
}
