using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace MoatCharms
{
    /// <summary>The loadout: pick which one charm you wear openly; the rest stay in the pocket.
    /// Numbered rows so 1-6 selects, 0 stows all — playable by keyboard alone.</summary>
    public class AmuletMenu : IClickableMenu
    {
        private readonly ModEntry Mod;
        private readonly List<AmuletDef> Owned;
        private readonly List<Rectangle> RowBounds = new();
        private readonly List<string> HeaderLines = new();

        private const int RowHeight = 76;

        public AmuletMenu(ModEntry mod)
            : base(0, 0, 1100, 400, showUpperRightCloseButton: true)
        {
            this.Mod = mod;
            this.Owned = Amulets.All
                .Where(def => Game1.player.Items.Any(i => i?.QualifiedItemId == def.QualifiedItemId))
                .ToList();

            var i18n = mod.Helper.Translation;
            DailyReading today = Omens.Today();
            this.HeaderLines.AddRange(Omens.Describe(today, i18n).Select(l => "• " + l));
            if (mod.WornAmulet()?.Id == "jingjok" && !today.RestingDay)
            {
                foreach (string line in Omens.Describe(Omens.Tomorrow(), i18n))
                    this.HeaderLines.Add(i18n.Get("menu.tomorrow", new { omen = line }).ToString());
            }

            int rows = System.Math.Max(this.Owned.Count, 1);
            this.height = 240 + this.HeaderLines.Count * 44 + rows * RowHeight + 70;
            this.width = 1100;
            this.xPositionOnScreen = (Game1.uiViewport.Width - this.width) / 2;
            this.yPositionOnScreen = (Game1.uiViewport.Height - this.height) / 2;
            this.initializeUpperRightCloseButton();
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);
            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true);

            var i18n = this.Mod.Helper.Translation;
            SpriteFont font = Game1.dialogueFont;
            int x = this.xPositionOnScreen + 60;
            int y = this.yPositionOnScreen + 120;

            Utility.drawTextWithShadow(b, i18n.Get("menu.title"), font, new Vector2(x, y), Game1.textColor);
            y += 60;

            foreach (string line in this.HeaderLines)
            {
                Utility.drawTextWithShadow(b, line, Game1.smallFont, new Vector2(x, y), new Color(90, 60, 20));
                y += 44;
            }
            y += 20;

            this.RowBounds.Clear();
            AmuletDef worn = this.Mod.WornAmulet();

            if (!this.Owned.Any())
            {
                Utility.drawTextWithShadow(b, i18n.Get("menu.empty"), Game1.smallFont, new Vector2(x, y), Game1.textColor * 0.8f);
                y += RowHeight;
            }

            Texture2D charms = Amulets.Texture();
            for (int n = 0; n < this.Owned.Count; n++)
            {
                AmuletDef def = this.Owned[n];
                var row = new Rectangle(x - 12, y - 8, this.width - 120, RowHeight - 8);
                this.RowBounds.Add(row);

                bool isWorn = worn?.Id == def.Id;
                if (row.Contains(Game1.getMouseX(), Game1.getMouseY()))
                    b.Draw(Game1.staminaRect, row, new Color(255, 220, 150) * 0.3f);

                b.Draw(charms, new Vector2(x, y), new Rectangle(def.SpriteIndex * 16, 0, 16, 16), Color.White, 0f, Vector2.Zero, 3.5f, SpriteEffects.None, 1f);

                string name = i18n.Get($"amulet.{def.Id}.name");
                string blessing = i18n.Get($"amulet.{def.Id}.blessing");
                string label = $"[{n + 1}]  {name} — {blessing}";
                if (isWorn)
                    label += "  " + i18n.Get("menu.worn");
                Utility.drawTextWithShadow(b, label, font, new Vector2(x + 76, y + 2), isWorn ? new Color(180, 120, 0) : Game1.textColor);

                y += RowHeight;
            }

            y += 16;
            Utility.drawTextWithShadow(b, i18n.Get("menu.footer"), Game1.smallFont, new Vector2(x, y), Game1.textColor * 0.7f);

            base.draw(b);
            this.drawMouse(b);
        }

        public override void receiveKeyPress(Keys key)
        {
            int digit = key switch
            {
                >= Keys.D0 and <= Keys.D9 => key - Keys.D0,
                >= Keys.NumPad0 and <= Keys.NumPad9 => key - Keys.NumPad0,
                _ => -1
            };

            if (digit == 0)
            {
                this.Mod.Wear(null);
                Game1.playSound("dwop");
                this.exitThisMenu();
                return;
            }
            if (digit >= 1 && digit <= this.Owned.Count)
            {
                this.Mod.Wear(this.Owned[digit - 1]);
                Game1.playSound("crystal");
                this.exitThisMenu();
                return;
            }

            base.receiveKeyPress(key);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            for (int n = 0; n < this.RowBounds.Count; n++)
            {
                if (this.RowBounds[n].Contains(x, y))
                {
                    this.Mod.Wear(this.Owned[n]);
                    Game1.playSound("crystal");
                    this.exitThisMenu();
                    return;
                }
            }
            base.receiveLeftClick(x, y, playSound);
        }
    }
}
