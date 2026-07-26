using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace MoatCharms
{
    /// <summary>One omen: a sign read at dawn. Sign is +1 (favorable) or -1 (contrary). An amulet can "answer" it.</summary>
    public class Omen
    {
        public string Id;
        public int Sign;
        public string AnswerAmulet; // short amulet id that resolves this omen, or null

        public Omen(string id, int sign, string answer = null)
        {
            this.Id = id;
            this.Sign = sign;
            this.AnswerAmulet = answer;
        }
    }

    /// <summary>The day's reading: two omens drawn at dawn, sometimes a "resting day" for charms.</summary>
    public class DailyReading
    {
        public Omen First;
        public Omen Second;
        public bool RestingDay; // charms should stay in the pocket today

        /// <summary>Net luck tilt in [-2, +2], given which amulet (short id) is worn.</summary>
        public int NetTilt(string wornId)
        {
            int Score(Omen o)
            {
                if (o == null)
                    return 0;
                bool answered = o.AnswerAmulet != null && o.AnswerAmulet == wornId && !this.RestingDay;
                if (o.Sign > 0)
                    return answered ? 2 : 1;   // a favorable omen, answered, blooms
                return answered ? 0 : -1;      // a contrary omen, answered, is soothed
            }
            return Math.Clamp(Score(this.First) + Score(this.Second), -2, 2);
        }
    }

    public static class Omens
    {
        public static readonly List<Omen> Pool = new()
        {
            // favorable
            new Omen("coucal",    +1),
            new Omen("housegecko",+1, "jingjok"),
            new Omen("almsround", +1, "somdej"),
            new Omen("lottery",   +1, "paladkhik"),
            new Omen("firstsale", +1, "nangkwak"),
            // contrary — each can be soothed by the right charm
            new Omen("teeth",     -1, "somdej"),
            new Omen("owl",       -1, "takrut"),
            new Omen("ricepot",   -1, "nangkwak"),
            new Omen("dogs",      -1, "sing"),
        };

        /// <summary>Deterministic reading for a given in-game day (so foresight and reality agree).</summary>
        public static DailyReading For(int daysPlayed)
        {
            var rng = new Random(unchecked((int)Game1.uniqueIDForThisGame + daysPlayed * 977));
            var reading = new DailyReading();

            reading.RestingDay = rng.Next(7) == 0;
            if (reading.RestingDay)
                return reading;

            var shuffled = Pool.OrderBy(_ => rng.Next()).ToList();
            reading.First = shuffled[0];
            // second omen appears most days; contested days (opposite signs) are the interesting ones
            if (rng.Next(4) > 0)
                reading.Second = shuffled.FirstOrDefault(o => o.Id != reading.First.Id);
            return reading;
        }

        public static DailyReading Today() => For((int)Game1.stats.DaysPlayed);
        public static DailyReading Tomorrow() => For((int)Game1.stats.DaysPlayed + 1);

        /// <summary>Human-readable lines for a reading.</summary>
        public static List<string> Describe(DailyReading r, ITranslationHelper i18n)
        {
            var lines = new List<string>();
            if (r.RestingDay)
            {
                lines.Add(i18n.Get("omen.resting"));
                return lines;
            }
            if (r.First != null)
                lines.Add(i18n.Get($"omen.{r.First.Id}"));
            if (r.Second != null)
                lines.Add(i18n.Get($"omen.{r.Second.Id}"));
            return lines;
        }
    }
}
