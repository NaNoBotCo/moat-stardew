using System;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buffs;

namespace MoatCharms
{
    public class ModConfig
    {
        public string MenuKey { get; set; } = "K";
        public string[] SellerDays { get; set; } = { "Fri", "Sun" };
    }

    public class ModEntry : Mod
    {
        public const string ModId = "NaNoBotCo.MoatCharms";
        public const string WornKey = ModId + "/worn";
        public const string WornBuffId = ModId + ".worn";
        public const string OmenBuffId = ModId + ".omen";

        public static ModEntry Instance;

        public ModConfig Config;
        private Seller Seller;
        private SButton MenuButton = SButton.K;

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            this.Config = helper.ReadConfig<ModConfig>();
            if (!Enum.TryParse(this.Config.MenuKey, ignoreCase: true, out this.MenuButton))
                this.MenuButton = SButton.K;

            this.Seller = new Seller(this);

            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.DayEnding += this.OnDayEnding;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.Display.MenuChanged += this.Seller.OnMenuChanged;
        }

        /*********
        ** Assets
        *********/
        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(asset => Amulets.EditObjectData(asset.AsDictionary<string, StardewValley.GameData.Objects.ObjectData>().Data, this.Helper.Translation));
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Characters"))
            {
                e.Edit(asset => Seller.EditCharacterData(asset.AsDictionary<string, StardewValley.GameData.Characters.CharacterData>().Data, this.Helper.Translation));
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(Amulets.CharmsAsset))
            {
                e.LoadFromModFile<Microsoft.Xna.Framework.Graphics.Texture2D>("assets/charms.png", AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo($"Characters/{Seller.InternalName}"))
            {
                e.LoadFromModFile<Microsoft.Xna.Framework.Graphics.Texture2D>("assets/seller.png", AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo($"Portraits/{Seller.InternalName}"))
            {
                e.LoadFromModFile<Microsoft.Xna.Framework.Graphics.Texture2D>("assets/seller_portrait.png", AssetLoadPriority.Exclusive);
            }
        }

        /*********
        ** Day cycle
        *********/
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            DailyReading reading = Omens.Today();

            // dawn reading in the corner of the eye
            foreach (string line in Omens.Describe(reading, this.Helper.Translation))
                Game1.addHUDMessage(new HUDMessage(line, HUDMessage.newQuest_type));

            this.ApplyBlessings();

            // Nang Kwak beckons customers
            if (this.WornAmulet()?.Id == "nangkwak" && !reading.RestingDay && Game1.random.NextDouble() < 0.2)
            {
                int tip = 64 + Game1.random.Next(4) * 48;
                Game1.player.Money += tip;
                Game1.addHUDMessage(new HUDMessage(this.Helper.Translation.Get("nangkwak.tip", new { amount = tip }), HUDMessage.achievement_type));
            }

            this.Seller.OnDayStarted();
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            this.Seller.OnDayEnding();
        }

        /*********
        ** Wearing
        *********/
        public AmuletDef WornAmulet()
        {
            if (!Game1.player.modData.TryGetValue(WornKey, out string id) || string.IsNullOrEmpty(id))
                return null;
            AmuletDef def = Amulets.Get(id);
            // charm must actually be on your person
            if (def != null && !Game1.player.Items.Any(i => i?.QualifiedItemId == def.QualifiedItemId))
                return null;
            return def;
        }

        public void Wear(AmuletDef def)
        {
            if (def == null)
                Game1.player.modData.Remove(WornKey);
            else
                Game1.player.modData[WornKey] = def.Id;
            this.ApplyBlessings();

            string msg = def == null
                ? this.Helper.Translation.Get("wear.none")
                : this.Helper.Translation.Get("wear.some", new { name = this.Helper.Translation.Get($"amulet.{def.Id}.name").ToString() });
            Game1.addHUDMessage(new HUDMessage(msg, HUDMessage.newQuest_type));
        }

        /// <summary>Recompute and apply both the worn-amulet buff and the day's omen tilt.</summary>
        public void ApplyBlessings()
        {
            Game1.player.buffs.Remove(WornBuffId);
            Game1.player.buffs.Remove(OmenBuffId);

            DailyReading reading = Omens.Today();
            AmuletDef worn = this.WornAmulet();

            // worn amulet's passive blessing (charms rest on resting days)
            if (worn?.Effects != null && !reading.RestingDay)
            {
                Game1.player.applyBuff(new Buff(
                    id: WornBuffId,
                    source: "moat",
                    displaySource: this.Helper.Translation.Get($"amulet.{worn.Id}.name"),
                    duration: Buff.ENDLESS,
                    iconTexture: Amulets.Texture(),
                    iconSheetIndex: worn.SpriteIndex,
                    effects: worn.Effects(),
                    isDebuff: false,
                    displayName: this.Helper.Translation.Get($"amulet.{worn.Id}.name"),
                    description: this.Helper.Translation.Get($"amulet.{worn.Id}.blessing")));
            }

            // the day's tilt: omens, or the cost of flaunting charms on a resting day
            int tilt = reading.RestingDay
                ? (worn != null ? -1 : 0)
                : reading.NetTilt(worn?.Id);

            if (tilt != 0)
            {
                var fx = new BuffEffects();
                fx.LuckLevel.Value = tilt;
                Game1.player.applyBuff(new Buff(
                    id: OmenBuffId,
                    source: "moat",
                    displaySource: this.Helper.Translation.Get("omen.buffsource"),
                    duration: Buff.ENDLESS,
                    iconTexture: Amulets.Texture(),
                    iconSheetIndex: reading.RestingDay ? Amulets.RestingSpriteIndex : Amulets.OmenSpriteIndex,
                    effects: fx,
                    isDebuff: tilt < 0,
                    displayName: this.Helper.Translation.Get(tilt > 0 ? "omen.tilt.up" : "omen.tilt.down"),
                    description: this.Helper.Translation.Get("omen.buffdesc")));
            }
        }

        /*********
        ** Input
        *********/
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (Context.IsPlayerFree && e.Button == this.MenuButton)
            {
                Game1.activeClickableMenu = new AmuletMenu(this);
                return;
            }

            if (Context.IsPlayerFree && e.Button.IsActionButton() && this.Seller.TryInteract(e))
                this.Helper.Input.Suppress(e.Button);
        }
    }
}
