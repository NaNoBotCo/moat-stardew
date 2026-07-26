using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.Menus;

namespace MoatCharms
{
    /// <summary>Lung Saeng, the wandering amulet seller. Sets up at the bus stop on seller days,
    /// leaves before nightfall (and before the save is written, so he never gets tangled in it).</summary>
    public class Seller
    {
        public const string InternalName = "MoatLungSaeng";
        private const string LocationName = "BusStop";
        private static readonly Point Tile = new(14, 24);

        private readonly ModEntry Mod;
        private NPC Npc;
        private bool ShopPending;

        public Seller(ModEntry mod)
        {
            this.Mod = mod;
        }

        /// <summary>Minimal character record so the game treats him as a well-mannered stranger:
        /// no calendar, no social tab, no gift log — he is only passing through.</summary>
        public static void EditCharacterData(IDictionary<string, CharacterData> data, ITranslationHelper i18n)
        {
            data[InternalName] = new CharacterData
            {
                DisplayName = i18n.Get("seller.name"),
                Age = NpcAge.Adult,
                Manner = NpcManner.Polite,
                Optimism = NpcOptimism.Positive,
                CanBeRomanced = false,
                CanSocialize = "FALSE",
                CanReceiveGifts = false,
                CanGreetNearbyCharacters = false,
                PerfectionScore = false,
                SocialTab = SocialTabBehavior.HiddenAlways,
                Calendar = CalendarBehavior.HiddenAlways,
                Home = new List<CharacterHomeData>
                {
                    new() { Id = "Default", Location = LocationName, Tile = Tile }
                }
            };
        }

        private bool IsSellerDay()
        {
            string day = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
            return this.Mod.Config.SellerDays.Any(d => string.Equals(d, day, StringComparison.OrdinalIgnoreCase));
        }

        public void OnDayStarted()
        {
            if (!Context.IsMainPlayer || !this.IsSellerDay())
                return;

            try
            {
                GameLocation loc = Game1.getLocationFromName(LocationName);
                if (loc == null || loc.characters.Any(c => c?.Name == InternalName))
                    return;

                this.Npc = new NPC(
                    new AnimatedSprite($"Characters\\{InternalName}", 0, 16, 32),
                    new Vector2(Tile.X, Tile.Y) * 64f,
                    2,
                    InternalName);
                loc.characters.Add(this.Npc);

                Game1.addHUDMessage(new HUDMessage(this.Mod.Helper.Translation.Get("seller.arrived"), HUDMessage.newQuest_type));
            }
            catch (Exception ex)
            {
                this.Mod.Monitor.Log($"Could not set up the amulet seller today: {ex}", LogLevel.Warn);
            }
        }

        public void OnDayEnding()
        {
            if (!Context.IsMainPlayer)
                return;
            GameLocation loc = Game1.getLocationFromName(LocationName);
            if (loc != null)
            {
                foreach (NPC npc in loc.characters.Where(c => c?.Name == InternalName).ToList())
                    loc.characters.Remove(npc);
            }
            this.Npc = null;
        }

        /// <summary>Action-button press near the seller: a word of greeting, then the tray of charms.</summary>
        public bool TryInteract(ButtonPressedEventArgs e)
        {
            if (this.Npc == null || Game1.currentLocation?.Name != LocationName)
                return false;

            Vector2 grab = e.Cursor.GrabTile;
            Vector2 sellerTile = this.Npc.Tile;
            float cursorDist = Vector2.Distance(grab, sellerTile);
            float playerDist = Vector2.Distance(Game1.player.Tile, sellerTile);
            if (cursorDist > 1.5f || playerDist > 2.5f)
                return false;

            var i18n = this.Mod.Helper.Translation;
            int variant = 1 + Game1.random.Next(4);
            string text = i18n.Get($"seller.greet.{variant}");
            this.ShopPending = true;
            Game1.DrawDialogue(new Dialogue(this.Npc, null, text));
            return true;
        }

        /// <summary>When his dialogue box closes, open the shop.</summary>
        public void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!this.ShopPending || e.NewMenu != null || e.OldMenu is not DialogueBox)
                return;
            this.ShopPending = false;

            try
            {
                var stock = new Dictionary<ISalable, ItemStockInformation>();
                int week = (int)(Game1.stats.DaysPlayed / 7u);
                for (int i = 0; i < 4; i++)
                {
                    AmuletDef def = Amulets.All[(week + i) % Amulets.All.Count];
                    Item item = ItemRegistry.Create(def.QualifiedItemId);
                    stock[item] = new ItemStockInformation(def.Price, int.MaxValue);
                }
                Game1.activeClickableMenu = new ShopMenu($"{ModEntry.ModId}.shop", stock);
            }
            catch (Exception ex)
            {
                this.Mod.Monitor.Log($"Could not open the amulet tray: {ex}", LogLevel.Warn);
            }
        }
    }
}
