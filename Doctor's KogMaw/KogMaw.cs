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

namespace KogMaw
{
    internal class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, LaneClearMenu, JungleClearMenu, KillStealMenu, Misc;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Spell.Targeted Ignite;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("KogMaw")) return;
            Chat.Print("Doctor's KogMaw Loaded!", Color.Orange);
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 250, 1650, 70);
            W = new Spell.Active(SpellSlot.W, (uint)Player.Instance.GetAutoAttackRange());
            E = new Spell.Skillshot(SpellSlot.E, 1200, SkillShotType.Linear, 500, 1400, 120);
            E.AllowedCollisionCount = int.MaxValue;
            R = new Spell.Skillshot(SpellSlot.R, 900 + 300 * (uint)Player.Instance.Spellbook.GetSpell(SpellSlot.R).Level, SkillShotType.Circular, 1200, int.MaxValue, 120);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Menu = MainMenu.AddMenu("Doctor's KogMaw", "KogMaw");
            Menu.AddGroupLabel("iDoctor Noob Dev");
            Menu.AddGroupLabel("Ideas Haxory");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            ComboMenu.AddGroupLabel("Ultimate Settings");
            ComboMenu.Add("ultiR", new CheckBox("Use [R] Combo"));
            ComboMenu.Add("RMode", new ComboBox("Ultimate Mode:", 1, "Always", "HP =< 50%", "HP =< 25%"));
            ComboMenu.Add("MinR", new Slider("Max Stacks [R] Combo", 5, 1, 10));
            ComboMenu.Add("ManaR", new Slider("Mana [R] Combo", 30));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HarassW", new CheckBox("Use [W] Harass"));
            HarassMenu.Add("HarassE", new CheckBox("Use [E] Harass"));
            HarassMenu.AddGroupLabel("Ultimate Settings");
            HarassMenu.Add("HRR", new CheckBox("Use [R] Harass"));
            HarassMenu.Add("MinRHR", new Slider("Max Stacks [R] Harass", 2, 1, 10));
            HarassMenu.Add("ManaHR", new Slider("Mana Harass", 50));

            LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("Lane Clear Settings");
            LaneClearMenu.Add("QLC", new CheckBox("Use [Q] LaneClear", false));
            LaneClearMenu.Add("ELC", new CheckBox("Use [E] LaneClear", false));
            LaneClearMenu.Add("minE", new Slider("Min Hit Minion Use [E]", 3, 1, 6));
            LaneClearMenu.AddGroupLabel("Ultimate Settings");
            LaneClearMenu.Add("RLC", new CheckBox("Use [R] LaneClear", false));
            LaneClearMenu.Add("MinRLC", new Slider("Max Stacks [R] LaneClear", 1, 1, 10));
            LaneClearMenu.Add("ManaLC", new Slider("Mana LaneClear", 50));
            LaneClearMenu.AddGroupLabel("Lasthit Settings");
            LaneClearMenu.Add("QLH", new CheckBox("Use [Q] Lasthit", false));
            LaneClearMenu.Add("ManaLH", new Slider("Mana Lasthit", 70));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("QJungle", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("WJungle", new CheckBox("Use [W] JungleClear"));
            JungleClearMenu.Add("EJungle", new CheckBox("Use [E] JungleClear"));
            JungleClearMenu.AddGroupLabel("Ultimate Settings");
            JungleClearMenu.Add("RJungle", new CheckBox("Use [R] JungleClear"));
            JungleClearMenu.Add("MinRJC", new Slider("Max Stacks [R] JungleClear", 1, 1, 10));
            JungleClearMenu.Add("ManaJC", new Slider("Mana JungleClear", 30));

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
            KillStealMenu.Add("KsR", new CheckBox("Use [R] KillSteal"));
            KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));

            Misc = Menu.AddSubMenu("Misc Settings", "Misc");
            Misc.AddGroupLabel("Misc Settings");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Misc.Add("skin.Id", new ComboBox("Skin Mode", 0, "Default", "1", "2", "3", "4", "5", "6", "7", "8"));
            Misc.Add("AntiGap", new CheckBox("Use [E] AntiGap"));
            Misc.AddGroupLabel("Drawing Settings");
            Misc.Add("DrawQ", new CheckBox("[Q] Range", false));
            Misc.Add("DrawW", new CheckBox("[W] Range"));
            Misc.Add("DrawE", new CheckBox("[E] Range", false));
            Misc.Add("DrawR", new CheckBox("[R] Range"));

            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Game.OnUpdate += Game_OnUpdate;
            Orbwalker.OnPostAttack += ResetAttack;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Misc["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2, Radius = R.Range }.Draw(_Player.Position);
            }
            if (Misc["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2, Radius = E.Range }.Draw(_Player.Position);
            }
            if (Misc["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2, Radius = _Player.GetAutoAttackRange() }.Draw(_Player.Position);
            }
            if (Misc["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2, Radius = Q.Range }.Draw(_Player.Position);
            }
        }

        public static int SkinId()
        {
            return Misc["skin.Id"].Cast<ComboBox>().CurrentValue;
        }
		
        public static bool checkSkin()
        {
            return Misc["checkSkin"].Cast<CheckBox>().CurrentValue;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            R = new Spell.Skillshot(SpellSlot.R, 900 + 300 * (uint)Player.Instance.Spellbook.GetSpell(SpellSlot.R).Level, SkillShotType.Circular, 1200, int.MaxValue, 120);
            W = new Spell.Active(SpellSlot.W, (uint)Player.Instance.GetAutoAttackRange());

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
                Ultimate();
            }

            KillSteal();

            if (_Player.SkinId != Misc["skin.Id"].Cast<ComboBox>().CurrentValue)
            {
                if (checkSkin())
                {
                    Player.SetSkinId(SkinId());
                }
            }
        }

        public static void ResetAttack(AttackableUnit e, EventArgs args)
        {
            if (!(e is AIHeroClient)) return;
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            var champ = (AIHeroClient)e;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            if (champ == null || champ.Type != GameObjectType.AIHeroClient || !champ.IsValid) return;
            if (target != null)
            {
                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    var Pred = E.GetPrediction(target);
                    if (Pred.HitChance >= HitChance.High)
                    {
                        E.Cast(Pred.CastPosition);
                    }
                }
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            if (target != null)
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    var Pred = Q.GetPrediction(target);
                    if (Pred.HitChance >= HitChance.High)
                    {
                        Q.Cast(Pred.CastPosition);
                    }
                }

                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && _Player.Distance(target) > Player.Instance.GetAutoAttackRange())
                {
                    var Pred = E.GetPrediction(target);
                    if (Pred.HitChance >= HitChance.High)
                    {
                        E.Cast(Pred.CastPosition);
                    }
                }
				
                if (useW && W.IsReady() && target.IsValidTarget(610 + 20 * (uint)Player.Instance.Spellbook.GetSpell(SpellSlot.W).Level))
                {
                    W.Cast();
                }
            }
        }

        private static void Ultimate()
        {
            var Rlimit = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
            var useR = ComboMenu["ultiR"].Cast<CheckBox>().CurrentValue;
            var mana = ComboMenu["ManaR"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            if (target != null)
            {
                if (useR && Player.Instance.GetBuffCount("KogMawLivingArtillery") < Rlimit && Player.Instance.ManaPercent > mana)
                {
                    if (ComboMenu["RMode"].Cast<ComboBox>().CurrentValue == 0)
                    {
                        var Rpred = R.GetPrediction(target);
                        if (Rpred.HitChance >= HitChance.High)
                        {
                            R.Cast(Rpred.CastPosition);
                        }
                    }
					
                    if (ComboMenu["RMode"].Cast<ComboBox>().CurrentValue == 1 && target.HealthPercent <= 50)
                    {
                        var Rpred = R.GetPrediction(target);
                        if (Rpred.HitChance >= HitChance.High)
                        {
                            R.Cast(Rpred.CastPosition);
                        }
                    }
					
                    if (ComboMenu["RMode"].Cast<ComboBox>().CurrentValue == 2 && target.HealthPercent <= 25)
                    {
                        var Rpred = R.GetPrediction(target);
                        if (Rpred.HitChance >= HitChance.High)
                        {
                            R.Cast(Rpred.CastPosition);
                        }
                    }
                }
            }
        }

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["QLC"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["ELC"].Cast<CheckBox>().CurrentValue;
            var useR = LaneClearMenu["RLC"].Cast<CheckBox>().CurrentValue;
            var Rlimit = LaneClearMenu["MinRLC"].Cast<Slider>().CurrentValue;
            var mana = LaneClearMenu["ManaLC"].Cast<Slider>().CurrentValue;
            var MinE = LaneClearMenu["minE"].Cast<Slider>().CurrentValue;
            var minionE = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(e => e.IsValidTarget(E.Range)).ToArray();
            var quang = EntityManager.MinionsAndMonsters.GetLineFarmLocation(minionE, E.Width, (int) E.Range);
            if (Player.Instance.ManaPercent < mana) return;
            foreach (var minions in minionE)
            {
                if (useQ && Q.IsReady() && minions.IsValidTarget(Q.Range) && Player.Instance.GetSpellDamage(minions, SpellSlot.Q) > minions.TotalShieldHealth() && _Player.Distance(minions) > 500)
                {
                    Q.Cast(minions);
                }
				
                if (useR && R.IsReady() && minions.IsValidTarget(R.Range) && Player.Instance.GetBuffCount("KogMawLivingArtillery") < Rlimit)
                {
                    R.Cast(minions);
                }

                if (useE && E.IsReady() && minions.IsValidTarget(E.Range) && quang.HitNumber >= MinE)
                {
                    E.Cast(quang.CastPosition);
                }
			}
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {	
            if (Misc["AntiGap"].Cast<CheckBox>().CurrentValue && sender.IsEnemy && e.Sender.Distance(_Player) <= 375)
            {
                E.Cast(e.Sender);
            }
        }

        private static void LastHit()
        {
            var useQ = LaneClearMenu["QLH"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["ManaLH"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            if (Player.Instance.ManaPercent < mana) return;
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && Player.Instance.GetSpellDamage(minion, SpellSlot.Q) >= minion.TotalShieldHealth())
                {
                    Q.Cast(minion);
                }
            }
        }

        private static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["ManaHR"].Cast<Slider>().CurrentValue;
            var Rlimit = HarassMenu["MinRHR"].Cast<Slider>().CurrentValue;
            var useR = HarassMenu["HRR"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            if (Player.Instance.ManaPercent <= mana) return;
            if (target != null)
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    var Pred = Q.GetPrediction(target);
                    if (Pred.HitChance >= HitChance.High)
                    {
                        Q.Cast(Pred.CastPosition);
                    }
                }

                if (useW && W.IsReady() && target.IsValidTarget(610 + 20 * (uint)Player.Instance.Spellbook.GetSpell(SpellSlot.W).Level))
                {
                    W.Cast();
                }
				
                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    var Pred = E.GetPrediction(target);
                    if (Pred.HitChance >= HitChance.High)
                    {
                        E.Cast(Pred.CastPosition);
                    }
                }
				
                if (useR && Player.Instance.GetBuffCount("KogMawLivingArtillery") < Rlimit)
                {
                    var Rpred = R.GetPrediction(target);
                    if (Rpred.HitChance >= HitChance.Medium)
                    {
                        R.Cast(Rpred.CastPosition);
                    }
                }
            }
        }

        public static void JungleClear()
        {
            var useQ = JungleClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var useE = JungleClearMenu["EJungle"].Cast<CheckBox>().CurrentValue;
            var useW = JungleClearMenu["WJungle"].Cast<CheckBox>().CurrentValue;
            var Rlimit = JungleClearMenu["MinRJC"].Cast<Slider>().CurrentValue;
            var useR = JungleClearMenu["RJungle"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["ManaJC"].Cast<Slider>().CurrentValue;
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, Q.Range).OrderByDescending(a => a.MaxHealth).FirstOrDefault();
            if (Player.Instance.ManaPercent <= mana) return;
            if (monster != null)
            {
                if (useQ && Q.IsReady() && monster.IsValidTarget(Q.Range))
                {
                    Q.Cast(monster);
                }
				
                if (useE && E.IsReady() && monster.IsValidTarget(E.Range))
                {
                    E.Cast(monster);
                }
				
                if (useW && W.IsReady() && monster.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
				
                if (useR && Player.Instance.GetBuffCount("KogMawLivingArtillery") < Rlimit)
                {
                    R.Cast(monster);
                }
            }
        }

        public static void Flee()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target != null)
            {
                if (E.IsReady() && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie)
                {
                    E.Cast(target);
                }
            }
        }

        public static float RDamege(Obj_AI_Base target)
        {
            float RDamege = 0;
            if (target.HealthPercent > 50)
            {
                RDamege = new[] { 0, 70, 110, 150 }[R.Level] + 0.65f * _Player.FlatPhysicalDamageMod + 0.25f * _Player.FlatMagicDamageMod;
            }
            else if (target.HealthPercent < 50)
            {
                RDamege = new[] { 0, 140, 220, 300 }[R.Level] + 1.3f * _Player.FlatPhysicalDamageMod + 0.5f * _Player.FlatMagicDamageMod;
            }
            else if (target.HealthPercent < 25)
            {
                RDamege = new[] { 0, 210, 330, 450 }[R.Level] + 1.95f * _Player.FlatPhysicalDamageMod + 0.75f * _Player.FlatMagicDamageMod;
            }
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical, RDamege);
        }

        private static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var KsR = KillStealMenu["KsR"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(R.Range) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie))
            {
                if (KsQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                    {
                        Q.Cast(target);
                    }
                }

                if (KsR && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    var Rpred = R.GetPrediction(target);
                    if (target.Health + target.AttackShield < RDamege(target) && Rpred.HitChance >= HitChance.Medium)
                    {
                        R.Cast(Rpred.CastPosition);
                    }
                }

                if (Ignite != null && KillStealMenu["ign"].Cast<CheckBox>().CurrentValue && Ignite.IsReady())
                {
                    if (target.Health < _Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite))
                    {
                        Ignite.Cast(target);
                    }
                }
            }
        }
    }
}
