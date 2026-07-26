# Moat: Charms of Chiang Mai — a Stardew Valley mod

Moat's amulet-craft, ported into Pelican Town: Lanna amulets, contested dawn omens,
and Lung Saeng the wandering amulet seller. C# SMAPI mod, no other mods required.

## Playing

- **K** opens the charm loadout — numbered menu, press **1–6** to wear a charm, **0** to tuck all away, Esc closes.
- Each dawn, one or two **omens** are read (HUD messages). They tilt the day's luck. The right amulet *answers* an omen — soothing a contrary one, or making a favorable one bloom.
- Roughly one day in seven is a **resting day**: charms should stay in the pocket. Wearing one openly that day costs you luck. Knowing when to show and when to stow is the craft.
- **Lung Saeng** sets up at the bus stop on **Fridays and Sundays** (configurable). His tray rotates 4 of the 6 amulets each week.
- The **jing-jok charm**, worn, whispers tomorrow's omens into the loadout menu.
- **Nang Kwak**, worn, sometimes beckons a tip-paying stranger at dawn.

## The amulets

| Amulet | Blessing while worn |
|---|---|
| Phra Somdej | +2 defense; answers teeth-dream and alms-round omens |
| Takrut | +2 immunity; answers the owl's cry |
| Sing | +2 attack; answers the howling soi dogs |
| Palad Khik | wider magnetism; answers the lottery dream |
| Nang Kwak | +1 farming, dawn tips; answers first-sale and rice-pot omens |
| Jing-jok | foresight: tomorrow's omens; answers the house gecko's chirps |

## Building

Needs the .NET 6 SDK (installed at `~/.dotnet`). From `MoatCharms/`:

```
dotnet build
```

The build auto-deploys into the Steam game's `Mods/MoatCharms` folder
(Pathoschild.Stardew.ModBuildConfig finds the game path itself).
Pixel art is generated, not hand-saved: edit `tools/make_sprites.py` and rerun it.

Day-to-day, use **`Moat Stardew.command`** on the Desktop (numbered menu:
play / rebuild / log / project / sprites).

## Design notes

- Omens are deterministic per save + day (`Omens.For`), so the gecko's foresight and the next dawn always agree.
- Lung Saeng is spawned at day start on seller days and removed at `DayEnding`, so he is never written into the save file.
- Amulets are ordinary inventory objects; "wearing" is `player.modData` + endless buffs, reapplied at each day start. Selling or losing the charm un-wears it automatically.
- All player-facing text lives in `MoatCharms/i18n/default.json`, in Moat's register: sweet, sincere, polite, transactional. Thai-side text can be added later as `i18n/th.json`.
