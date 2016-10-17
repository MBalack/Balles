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
            Q = new Spell.Skillshot(SpellSlot.Q, 825, SkillShotType.Circular, 300 , 1000 ,250);
            Q.AllowedCollisionCount = int.MaxValue;
            W = new Spell.Skillshot(SpellSlot.W, 800, SkillShotType.Linear);
            W.AllowedCollisionCount = int.MaxValue;
            E = new Spell.Active(SpellSlot.E, 600);
            R = new Spell.Skillshot(SpellSlot.R, 1200, SkillShotType.Linear, 200, 1950, 40);
            Botrk = new Item( ItemId.Blade_of_the_Ruined_King);
            Bil = new Item(3144, 475f);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Menu = MainMenu.AddMenu("Doctor's Corki", "Corki");
            Menu.AddGroupLabel("Mercedes7");
            SpellMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            SpellMenu.AddGroupLabel("Combo Settings");
            SpellMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            SpellMenu.Add("QMode", new ComboBox("Q Mode:", 0, "Fast [Q]", "[Q] After Attack"));
            SpellMenu.Add("ComboR", new CheckBox("Use [R] Combo"));
            SpellMenu.Add("RMode", new ComboBox("Q Mode:", 0, "Fast [R]", "[R] After Attack"));
            SpellMenu.Add("ComboE", new CheckBox("Use [E] Combo"));

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

            items = Menu.AddSubMenu("Items Settings", "Items");
            items.AddGroupLabel("Items Settings");
            items.Add("BOTRK", new CheckBox("Use [Botrk]"));
            items.Add("ihp", new Slider("My HP Use BOTRK <=", 50));
            items.Add("ihpp", new Slider("Enemy HP Use BOTRK <=", 50));

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnPostAttack += ResetAttack;
        }

// Game OnTick

        private static void Game_OnUpdate(EventArgs args)
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
            Item();
        }

// Combo Mode

        private static void ResetAttack(AttackableUnit e, EventArgs args)
        {
            if (!(e is AIHeroClient)) return;
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            var champ = (AIHeroClient)e;
            var useQ = SpellMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useR = SpellMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            if (champ == null || champ.Type != GameObjectType.AIHeroClient || !champ.IsValid) return;
            if (target != null)
            {
                if (SpellMenu["QMode"].Cast<ComboBox>().CurrentValue == 1)
                {
                    if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && _Player.Distance(target) < Player.Instance.GetAutoAttackRange(target) && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    {
                        var Pred = Q.GetPrediction(target);
                        if (Pred.HitChance >= HitChance.High)
                        {
                            Q.Cast(Pred.CastPosition);
                        }
                    }
                }

                if (SpellMenu["RMode"].Cast<ComboBox>().CurrentValue == 1)
                {
                    if (useR && R.IsReady() && R.Handle.Ammo >= 1 && target.IsValidTarget(R.Range) && _Player.Distance(target) < Player.Instance.GetAutoAttackRange(target) && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    {
                        var RPred = R.GetPrediction(target);
                        if (RPred.HitChance >= HitChance.High)
                        {
                            R.Cast(RPred.CastPosition);
                        }
                    }
                }

            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            var useQ = SpellMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useR = SpellMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var useE = SpellMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            if (target != null)
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !Orbwalker.IsAutoAttacking)
                {
                    if (SpellMenu["QMode"].Cast<ComboBox>().CurrentValue == 0)
                    {
                        var Pred = Q.GetPrediction(target);
                        if (Pred.HitChance >= HitChance.Medium)
                        {
                            Q.Cast(Pred.CastPosition);
                        }
                    }
                    else
                    {
                        if (_Player.Distance(target) > Player.Instance.GetAutoAttackRange(target))
                        {
                            var Pred = Q.GetPrediction(target);
                            if (Pred.HitChance >= HitChance.Medium)
                            {
                                Q.Cast(Pred.CastPosition);
                            }
                        }
                    }
                }

                if (useR && R.IsReady() && target.IsValidTarget(R.Range) && R.Handle.Ammo >= 1 && !Orbwalker.IsAutoAttacking)
                {
                    if (SpellMenu["RMode"].Cast<ComboBox>().CurrentValue == 0)
                    {
                        var RPred = R.GetPrediction(target);
                        if (RPred.HitChance >= HitChance.Medium)
                        {
                            R.Cast(RPred.CastPosition);
                        }
                    }
                    else
                    {
                        if (_Player.Distance(target) > Player.Instance.GetAutoAttackRange(target))
                        {
                            var RPred = R.GetPrediction(target);
                            if (RPred.HitChance >= HitChance.Medium)
                            {
                                R.Cast(RPred.CastPosition);
                            }
                        }
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
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            var mana = HarassMenu["manaHarass"].Cast<Slider>().CurrentValue;
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useR = HarassMenu["HarassR"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var Rocket = HarassMenu["RocketHarass"].Cast<Slider>().CurrentValue;

            if (Player.Instance.ManaPercent < mana)
            {
                return;
            }

            if (target != null)
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !Orbwalker.IsAutoAttacking)
                {
                    var Pred = Q.GetPrediction(target);
                    if (Pred.HitChance >= HitChance.High)
                    {
                        Q.Cast(Pred.CastPosition);
                    }
                }
				
                if (useR && R.IsReady() && target.IsValidTarget(R.Range) && R.Handle.Ammo > Rocket)
                {
                    var Pred = R.GetPrediction(target);
                    if (Pred.HitChance >= HitChance.High)
                    {
                        R.Cast(Pred.CastPosition);
                    }
                }
				
                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !Orbwalker.IsAutoAttacking)
                {
                    E.Cast();
                }
            }
        }

// LaneClear

        private static void LaneClear()
        {
            var mana = ClearMenu["manaClear"].Cast<Slider>().CurrentValue;
            var useQ = ClearMenu["ClearQ"].Cast<CheckBox>().CurrentValue;
            var useR = ClearMenu["ClearR"].Cast<CheckBox>().CurrentValue;
            var useE = ClearMenu["ClearE"].Cast<CheckBox>().CurrentValue;
            var Rocket = ClearMenu["RocketClear"].Cast<Slider>().CurrentValue;
            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, _Player.Position, R.Range).ToArray();
            var QCal = EntityManager.MinionsAndMonsters.GetCircularFarmLocation(minions, Q.Width, (int)Q.Range);

            if (Player.Instance.ManaPercent < mana)
            {
                return;
            }

            foreach (var minion in minions)
            {
                if (useR && R.IsReady() && minion.IsValidTarget(R.Range) && R.Handle.Ammo > Rocket)
                {
                    R.Cast(minion.Position);
                }

                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && QCal.HitNumber >= 2 && !Orbwalker.IsAutoAttacking)
                {
                    Q.Cast(QCal.CastPosition);
                }
				
                if (useE && E.IsReady() && minion.IsValidTarget(E.Range) && !Orbwalker.IsAutoAttacking)
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
                if (useR && R.IsReady() && target.Health <= Player.Instance.GetSpellDamage(target, SpellSlot.R))
                {
                    var RPred = R.GetPrediction(target);
                    if (RPred.HitChance >= HitChance.Medium)
                    {
                        R.Cast(RPred.CastPosition);
                    }
                }
				
                if (useQ && Q.IsReady() && target.Health <= Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                {
                    var Pred = Q.GetPrediction(target);
                    if (Pred.HitChance >= HitChance.Medium)
                    {
                        Q.Cast(Pred.CastPosition);
                    }
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

            if (Player.Instance.ManaPercent < mana)
            {
                return;
            }

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
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(475) && !e.IsDead))
            {
                if (item && Bil.IsReady() && Bil.IsOwned() && Bil.IsInRange(target))
                {
                    Bil.Cast(target);
                }
				
                if ((item && Botrk.IsReady() && Botrk.IsOwned() && target.IsValidTarget(475)) && (Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp))
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

            if (Misc["drawQ"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 0.5f, Radius = Q.Range }.Draw(_Player.Position);
            }

            if (Misc["drawW"].Cast<CheckBox>().CurrentValue && W.IsLearned)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 0.5f, Radius = W.Range }.Draw(_Player.Position);
            }

            if (Misc["drawE"].Cast<CheckBox>().CurrentValue && E.IsLearned)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 0.5f, Radius = E.Range }.Draw(_Player.Position);
            }

            if (Misc["drawR"].Cast<CheckBox>().CurrentValue && R.IsLearned)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 0.5f, Radius = R.Range }.Draw(_Player.Position);
            }
        }
    }
}