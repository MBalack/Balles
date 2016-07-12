using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace Borki7
{
    static class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Active E;
        public static Spell.Skillshot R;
        public static Item Botrk;
        public static Item Bil;
        public static Spell.Targeted Ignite;
        public static readonly Item Qss = new Item(ItemId.Quicksilver_Sash);
        public static readonly Item Simitar = new Item(ItemId.Mercurial_Scimitar);
        public static Menu Menu, SpellMenu, HarassMenu, ClearMenu, KillstealMenu, JungleMenu, items, Misc;

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

// Menu

        private static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Corki")) return;
            Chat.Print("Doctor's Corki Loaded!", Color.Orange);
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, 825, SkillShotType.Circular, 300 , 1000 ,250);
            W = new Spell.Skillshot(SpellSlot.W, 800, SkillShotType.Linear);
            E = new Spell.Active(SpellSlot.E, 600);
            R = new Spell.Skillshot(SpellSlot.R, 1200, SkillShotType.Linear, 200, 1950, 40);
            Botrk = new Item( ItemId.Blade_of_the_Ruined_King);
            Bil = new Item(3144, 475f);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Menu = MainMenu.AddMenu("Borki7", "Corki");
            Menu.AddGroupLabel("Doctor7");

            SpellMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            SpellMenu.AddGroupLabel("Combo Settings");
            SpellMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            SpellMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            SpellMenu.Add("ComboR", new CheckBox("Use [R] Combo"));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HarassR", new CheckBox("Use [R] Harass"));
            HarassMenu.Add("HarassE", new CheckBox("Use [E Harass]"));
            HarassMenu.Add("manaHarass", new Slider("Min Mana Harass", 50, 0, 100));
            HarassMenu.Add("RocketHarass", new Slider("Save Rockets [R]", 3, 0, 6));

            ClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            ClearMenu.AddGroupLabel("Laneclear Settings");
            ClearMenu.Add("ClearQ", new CheckBox("Use [Q] LaneClear", false));
            ClearMenu.Add("ClearR", new CheckBox("Use [R] LaneClear", false));
            ClearMenu.Add("ClearE", new CheckBox("Use [E] LaneClear", false));
            ClearMenu.Add("manaClear", new Slider("Min Mana LaneClear", 65, 0, 100));
            ClearMenu.Add("RocketClear", new Slider("Save Rockets [R]", 3, 0, 6));
			
            JungleMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleMenu.AddGroupLabel("JungleClear Settings");
            JungleMenu.Add("JungleQ", new CheckBox("Use [Q] JungleClear"));
            JungleMenu.Add("JungleR", new CheckBox("Use [R] JungleClear"));
            JungleMenu.Add("JungleE", new CheckBox("Use [E] JungleClear"));
            JungleMenu.Add("manaJung", new Slider("Min Mana JungleClear", 30, 0, 100));
            JungleMenu.Add("RocketJung", new Slider("Save Rockets [R]", 3, 0, 6));

            KillstealMenu = Menu.AddSubMenu("KillSteal Settings", "KS");
            KillstealMenu.AddGroupLabel("KillSteal Settings");
            KillstealMenu.Add("RKs", new CheckBox("Use [R] KillSteal"));
            KillstealMenu.Add("QKs", new CheckBox("Use [Q] KillSteal"));
            KillstealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));

            Misc = Menu.AddSubMenu("Misc Settings", "Misc");
            Misc.AddGroupLabel("Drawings Settings");
            Misc.Add("Draw_Disabled", new CheckBox("Disabled Drawings", false));
            Misc.Add("drawQ", new CheckBox("Range [Q]"));
            Misc.Add("drawW", new CheckBox("Range [W]", false));
            Misc.Add("drawE", new CheckBox("Range [E]"));
            Misc.Add("drawR", new CheckBox("Range [R]"));
            Misc.AddGroupLabel("Skin Changer");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Misc.Add("skin.Id", new ComboBox("Skin Mode", 0, "Default", "1", "2", "3", "4", "5", "6", "7"));

            items = Menu.AddSubMenu("Items Settings", "Items");
            items.AddGroupLabel("Items Settings");
            items.Add("BOTRK", new CheckBox("Use [Botrk]"));
            items.Add("ihp", new Slider("My HP Use BOTRK <=", 50));
            items.Add("ihpp", new Slider("Enemy HP Use BOTRK <=", 50));
            items.AddGroupLabel("Qss Settings");
            items.Add("Qss", new CheckBox("Use Qss"));
            items.AddGroupLabel("Qss On CC");
            items.Add("stun", new CheckBox("Stuns"));
            items.Add("rot", new CheckBox("Root"));
            items.Add("tunt", new CheckBox("Taunt"));
            items.Add("snare", new CheckBox("Snare"));
            items.Add("charm", new CheckBox("Charm", false));
            items.Add("slow", new CheckBox("Slows", false));
            items.Add("blind", new CheckBox("Blinds", false));
            items.Add("fear", new CheckBox("Fear", false));
            items.Add("silence", new CheckBox("Silence", false));
            items.Add("supperss", new CheckBox("Supperss", false));
            items.Add("poly", new CheckBox("Polymorph", false));
            items.Add("delay", new Slider("Humanizer Qss Delay", 0, 0, 1500));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

        }

// Game OnTick

        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
            KillSteal();
            Qsss();
            Item();
            if (_Player.SkinId != Misc["skin.Id"].Cast<ComboBox>().CurrentValue)
            {
                if (checkSkin())
                {
                    Player.SetSkinId(SkinId());
                }
            }
        }

// Skin Changer

        public static int SkinId()
        {
            return Misc["skin.Id"].Cast<ComboBox>().CurrentValue;
        }

        public static bool checkSkin()
        {
            return Misc["checkSkin"].Cast<CheckBox>().CurrentValue;
        }

// Combo Mode

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            var useQ = SpellMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useR = SpellMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var useE = SpellMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            if (target != null)
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }
                if (useR && R.IsReady() && target.IsValidTarget(R.Range) && R.Handle.Ammo >= 1)
                {
                    var Pred = R.GetPrediction(target);
                    if (Pred.HitChance >= HitChance.Medium)
                    {
                        R.Cast(Pred.CastPosition);
                    }
                }
                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    E.Cast();
                }
            }
        }

// Harass Mode

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            var mana = HarassMenu["manaHarass"].Cast<Slider>().CurrentValue;
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useR = HarassMenu["HarassR"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var Rocket = HarassMenu["RocketHarass"].Cast<Slider>().CurrentValue;
            if (_Player.ManaPercent < mana) return;
            if (target != null)
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }
                if (useR && R.IsReady() && target.IsValidTarget(R.Range) && R.Handle.Ammo > Rocket)
                {
                    var Pred = R.GetPrediction(target);
                    if (Pred.HitChance >= HitChance.High)
                    {
                        R.Cast(Pred.CastPosition);
                    }
                }
                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    E.Cast();
                }
            }
        }

// LaneClear

        private static void LaneClear()
        {
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(m => m.IsValidTarget(R.Range)).FirstOrDefault(x => EntityManager.MinionsAndMonsters.EnemyMinions.Count(m => m.Distance(x) < Q.Radius) > 2);
            var mana = ClearMenu["manaClear"].Cast<Slider>().CurrentValue
            var useQ = ClearMenu["ClearQ"].Cast<CheckBox>().CurrentValue;
            var useR = ClearMenu["ClearR"].Cast<CheckBox>().CurrentValue;
            var useE = ClearMenu["ClearE"].Cast<CheckBox>().CurrentValue;
            var Rocket = ClearMenu["RocketClear"].Cast<Slider>().CurrentValue
            if (_Player.ManaPercent < mana) return;
            if (minion != null)
            {
                if (useR && R.IsReady() && minion.IsValidTarget(R.Range) && R.Handle.Ammo > Rocket)
                {
                    R.Cast(minion);
                }
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range))
                {
                    Q.Cast(minion);
                }
                if (useE && E.IsReady() && minion.IsValidTarget(E.Range) && minion.Count() >= 2)
                {
                    E.Cast();
                }
            }
        }

// KillSteal

        private static void KillSteal()
        {
            var useQ = KillstealMenu["QKs"].Cast<CheckBox>().CurrentValue;
            var useR = KillstealMenu["RKs"].Cast<CheckBox>().CurrentValue;
            var Ignites = KillstealMenu["ign"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(R.Range) && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && !e.IsDead && !e.IsZombie))
            {
                if (useR && R.IsReady() && target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.R))
                {
                    R.Cast(target);
                }
                if (useQ && Q.IsReady() && target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                {
                    Q.Cast(target);
                }
                if (Ignite != null && Ignites && Ignite.IsReady())
                {
                    if (target.Health < _Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite))
                    {
                        Ignite.Cast(target);
                    }
                }
            }
        }

// Qss Items

        public static void CastQss()
        {
            if (Qss.IsOwned() && Qss.IsReady())
            {
                Core.DelayAction(() => Qss.Cast(), items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Simitar.IsOwned() && Simitar.IsReady())
            {
                Core.DelayAction(() => Simitar.Cast(), items["delay"].Cast<Slider>().CurrentValue);
            }
        }

// Qss Buff

        private static void Qsss()
        {
            if (!items["Qss"].Cast<CheckBox>().CurrentValue) return;
            if (items["snare"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Snare))
            {
                CastQss();
            }
            if (items["tunt"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Taunt))
            {
                CastQss();
            }
            if (items["stun"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Stun))
            {
                CastQss();
            }
            if (items["poly"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Polymorph))
            {
                CastQss();
            }
            if (items["blind"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Blind))
            {
                CastQss();
            }
            if (items["fear"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Fear))
            {
                CastQss();
            }
            if (items["charm"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Charm))
            {
                CastQss();
            }
            if (items["supperss"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Suppression))
            {
                CastQss();
            }
            if (items["silence"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Silence))
            {
                CastQss();
            }
            if (items["rot"].Cast<CheckBox>().CurrentValue && _Player.IsRooted)
            {
                CastQss();
            }
            if (items["slow"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Slow))
            {
                CastQss();
            }
        }

// Flee

        private static void Flee()
        {
            if (W.IsReady())
            {
                var cursorPos = Game.CursorPos;
                var castPos = Player.Instance.Position.Distance(cursorPos) <= W.Range ? cursorPos : Player.Instance.Position.Extend(cursorPos, W.Range).To3D();
                W.Cast(castPos);
            }
        }

// JungleClear

        private static void JungleClear()
        {
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters(_Player.ServerPosition, R.Range).FirstOrDefault(x => x.IsValidTarget(R.Range));
            var useQ = JungleMenu["JungleQ"].Cast<CheckBox>().CurrentValue;
            var useR = JungleMenu["JungleR"].Cast<CheckBox>().CurrentValue;
            var useE = JungleMenu["JungleE"].Cast<CheckBox>().CurrentValue;
            var mana = JungleMenu["manaJung"].Cast<Slider>().CurrentValue;
            var Rocket = JungleMenu["RocketJung"].Cast<Slider>().CurrentValue;
            if (_Player.ManaPercent < mana) return;
            if (monster != null)
            {
                if (useQ && Q.IsReady() && monster.IsValidTarget(Q.Range))
                {
                    Q.Cast(monster);
                }
                if (useR && R.IsReady() && monster.IsValidTarget(R.Range) && R.Handle.Ammo > Rocket)
                {
                    R.Cast(monster);
                }
                if (useE && E.IsReady() && monster.IsValidTarget(E.Range))
                {
                    E.Cast();
                }
            }
        }

// Use Items

        public static void Item()
        {
            var item = items["BOTRK"].Cast<CheckBox>().CurrentValue;
            var Minhp = items["ihp"].Cast<Slider>().CurrentValue;
            var Minhpp = items["ihpp"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(450, DamageType.Physical);
            if (target != null)
            {
                if (item && Bil.IsReady() && Bil.IsOwned() && target.IsValidTarget(450))
                {
                    Bil.Cast(target);
                }
                if ((item && Botrk.IsReady() && Botrk.IsOwned() && target.IsValidTarget(450)) && (Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp))
                {
                    Botrk.Cast(target);
                }
            }
        }

// Drawings

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;
            if (Misc["Draw_Disabled"].Cast<CheckBox>().CurrentValue) return;
            if (Misc["drawQ"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (Misc["drawW"].Cast<CheckBox>().CurrentValue && W.IsReady())
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = W.Range }.Draw(_Player.Position);
            }
            if (Misc["drawE"].Cast<CheckBox>().CurrentValue && E.IsReady())
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = E.Range }.Draw(_Player.Position);
            }
            if (Misc["drawR"].Cast<CheckBox>().CurrentValue && R.IsReady())
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}