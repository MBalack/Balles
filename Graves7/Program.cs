using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace Graves7
{
    internal static class Program
    {
        private static Menu Menu, ComboMenu, HarassMenu, ClearMenu, JungleMenu, Drawings, KillStealMenu, Skin, Misc;
        public static AIHeroClient PlayerInstance
        {
            get { return Player.Instance; }
        }
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Slider ComboMode;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if(Player.Instance.Hero != Champion.Graves)
                return;
            Chat.Print("Graves7 Loaded!", Color.Orange);
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Linear, 250, 2000, 60);
            Q.AllowedCollisionCount = int.MaxValue;
            W = new Spell.Skillshot(SpellSlot.W, 950, SkillShotType.Circular, 250, 1650, 150);
            W.AllowedCollisionCount = int.MaxValue;
            E = new Spell.Skillshot(SpellSlot.E, 425, SkillShotType.Linear);
            R = new Spell.Skillshot(SpellSlot.R, 1500, SkillShotType.Linear, 250, 2100, 100);
            R.AllowedCollisionCount = int.MaxValue;

            Menu = MainMenu.AddMenu("Graves7","Graves7");
            Menu.AddGroupLabel("Graves7");
            Menu.AddLabel(" Please Select Target Before Play ! ");

            ComboMenu = Menu.AddSubMenu("Combo Settings", "ComboMenu");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddLabel("Use [Q] On");
            foreach (var enemies in EntityManager.Heroes.Enemies)
            {
                ComboMenu.Add("combo" + enemies.ChampionName, new CheckBox("" + enemies.ChampionName));
            }
            ComboMenu.AddSeparator();        
			ComboMode = ComboMenu.Add("comboMode", new Slider("Min Stack Use [E]", 1, 0, 1));
            ComboMenu.Add("ComboW", new CheckBox("Use [W]"));
            ComboMenu.Add("ComboR", new CheckBox("Use [R]", false));
            ComboMenu.Add("MinR", new Slider("Min Enemies Use [R]", 2, 0, 5));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "HarassMenu");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.AddLabel("Harass [Q] On");
            foreach (var enemies in EntityManager.Heroes.Enemies)
            {
                HarassMenu.Add("haras" + enemies.ChampionName, new CheckBox("" + enemies.ChampionName));
            }
            HarassMenu.Add("HarassMana", new Slider("Min Mana Harass [Q]", 50));
            HarassMenu.AddSeparator();
            HarassMenu.AddGroupLabel("Spells Settings");
            HarassMenu.Add("HarassW", new CheckBox("Use [W]", false));
            HarassMenu.Add("ManaW", new Slider("Min Mana Harass [W]", 40));
            HarassMenu.Add("HarassAA", new CheckBox("Use [E] Reset AA", false));
            HarassMenu.Add("ManaHarass", new Slider("Min Mana For [E] Harass", 50));	

            ClearMenu = Menu.AddSubMenu("Laneclear Settings", "LaneClearMenu");
            ClearMenu.AddGroupLabel("Laneclear Settings");
            ClearMenu.Add("QClear", new CheckBox("Use [Q]"));
            ClearMenu.Add("ClearMana", new Slider("Min Mana For [Q] LaneClear", 70));
            ClearMenu.Add("LaneAA", new CheckBox("Use [E] Reset AA", false));
            ClearMenu.Add("ELane", new Slider("Min Mana For [E] LaneClear", 70));			

            JungleMenu = Menu.AddSubMenu("JungleClear Settings", "JungleMenu");
            JungleMenu.AddGroupLabel("JungleClear Settings");
            JungleMenu.Add("QJungleClear", new CheckBox("Use [Q]"));
            JungleMenu.Add("JungleMana", new Slider("Min Mana For [Q] JungleClear", 30));
            JungleMenu.Add("WJungleClear", new CheckBox("Use [W]"));
            JungleMenu.Add("JungleManaW", new Slider("Min Mana For [W] JungleClear", 50));
            JungleMenu.Add("JungleAA", new CheckBox("Use [E]"));
            JungleMenu.Add("EJung", new Slider("Min Mana For [E] JungleClear", 30));
			
            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillStealMenu");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
            KillStealMenu.Add("KsW", new CheckBox("Use [W] KillSteal"));
            KillStealMenu.AddSeparator();
            KillStealMenu.AddLabel("Ultimate Settings");
            KillStealMenu.Add("KsR", new CheckBox("Use [R] KillSteal"));
            KillStealMenu.Add("minKsR", new Slider("Min [R] Range KillSteal", 100, 1, 1500));
            KillStealMenu.Add("maxKsR", new Slider("Max [R] Range KillSteal", 1500, 1, 1500));
			
            Skin = Menu.AddSubMenu("Skin Changer", "SkinChanger");
            Skin.Add("checkSkin", new CheckBox("Use Skin Changer"));
            Skin.Add("skin.Id", new ComboBox("Skin Mode", 3, "Default", "1", "2", "3", "4", "5", "6", "7", "8"));
			
            Misc = Menu.AddSubMenu("Misc Settings", "Misc");
            Misc.AddLabel("AntiGap Settings");
            Misc.Add("AntiGap", new CheckBox("Use [E] AntiGapcloser"));
            Misc.Add("AntiGapW", new CheckBox("Use [W] AntiGapcloser"));
            Misc.AddSeparator();
            Misc.AddLabel("Spells On CC Settings");
            Misc.Add("QStun", new CheckBox("Use [Q] If Enemy Has CC"));


            Drawings = Menu.AddSubMenu("Drawings Settings", "DrawingMenu");
            Drawings.AddGroupLabel("Drawings");
            Drawings.Add("drawQ", new CheckBox("[Q] Range"));
            Drawings.Add("drawW", new CheckBox("[W] Range", false));
            Drawings.Add("drawE", new CheckBox("[E] Range", false));
            Drawings.Add("drawR", new CheckBox("[R] Range"));


            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnPostAttack += ResetAttack;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }

        private static void Game_OnTick(EventArgs args)
		{
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
				JungleClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
			if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
			    KillSteal();
                QStun();
            if (_Player.SkinId != Skin["skin.Id"].Cast<ComboBox>().CurrentValue)
            {
                if (checkSkin())
                {
                    Player.SetSkinId(SkinId());
                }
            }
		}
		
        public static int SkinId()
        {
            return Skin["skin.Id"].Cast<ComboBox>().CurrentValue;
        }
        public static bool checkSkin()
        {
            return Skin["checkSkin"].Cast<CheckBox>().CurrentValue;
        }
		
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Drawings["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.White, BorderWidth = 5f, Radius = Q.Range }.Draw(Player.Instance.Position);
            }
            if (Drawings["drawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.White, BorderWidth = 5f, Radius = W.Range }.Draw(Player.Instance.Position);
            }
            if (Drawings["drawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.White, BorderWidth = 5f, Radius = E.Range }.Draw(Player.Instance.Position);
            }
            if (Drawings["drawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.White, BorderWidth = 5f, Radius = R.Range }.Draw(Player.Instance.Position);
            }
        }
// Thanks MarioGK has allowed me to use some his logic
        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (Misc["AntiGap"].Cast<CheckBox>().CurrentValue && E.IsReady() && sender.IsEnemy && sender.IsVisible && sender.IsInRange(Player.Instance, E.Range))
            {
                E.Cast(Player.Instance.Position.Shorten(sender.Position, E.Range));
            }	

            if (Misc["AntiGapW"].Cast<CheckBox>().CurrentValue && sender.IsEnemy && e.Sender.Distance(_Player)<300)
            {
                W.Cast(e.Sender);
            }
        }
		
        private static void Flee()
        {
            if (E.IsReady())
            {
                var cursorPos = Game.CursorPos;
                var castPos = Player.Instance.Position.Distance(cursorPos) <= E.Range ? cursorPos : Player.Instance.Position.Extend(cursorPos, E.Range).To3D();
                E.Cast(castPos);
			}
		}

        public static void Combo()
        {
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var MinR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
            var Selector = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var enemies = EntityManager.Heroes.Enemies.OrderByDescending
                (a => a.HealthPercent).Where(a => !a.IsMe && a.IsValidTarget() && a.Distance(_Player) <= 800);
            if (Selector == null)
            {
                return;
            }
            if (Q.IsReady() && Selector.IsValidTarget(Q.Range))
            foreach (var qenemies in enemies)
            {
                if (ComboMenu["combo" + qenemies.ChampionName].Cast<CheckBox>().CurrentValue)
                {
                    var predQ = Q.GetPrediction(Selector);
                    if (predQ.HitChance >= HitChance.Medium)
                    {
                        Q.Cast(predQ.CastPosition);
                    }
                }
			}
            var Wpred = W.GetPrediction(Selector);
            if (useW && W.IsReady() && Selector.IsValidTarget(W.Range))
     	    {
                {				
                    if (Wpred.HitChance >= HitChance.Medium)
                    {
                        W.Cast(Wpred.CastPosition);
                    }
				}
			}
            if (ComboMode.CurrentValue == 1 && E.IsReady() && !Player.HasBuff("GravesBasicAttackAmmo2") && _Player.Position.CountEnemiesInRange(R.Range) >= 1)
            {
                Player.CastSpell(SpellSlot.E, Game.CursorPos);
            }
            if (ComboMode.CurrentValue == 0 && E.IsReady() && !Player.HasBuff("GravesBasicAttackAmmo2") && !Player.HasBuff("GravesBasicAttackAmmo1") && _Player.Position.CountEnemiesInRange(R.Range) >= 1)
            {
      		    Player.CastSpell(SpellSlot.E, Game.CursorPos);
            }
            if (useR && R.IsReady() && Selector.IsValidTarget(R.Range))
            {
                var pred = R.GetPrediction(Selector);
                if (pred.CastPosition.CountEnemiesInRange(R.Range) >= MinR && pred.HitChance >= HitChance.High)
                {
                    R.Cast(pred.CastPosition);
                }
			}
        }

        private static void Harass()
        {
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["HarassMana"].Cast<Slider>().CurrentValue;
            var ManaW = HarassMenu["ManaW"].Cast<Slider>().CurrentValue;
            var Selector = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var enemies = EntityManager.Heroes.Enemies.OrderByDescending
                (a => a.HealthPercent).Where(a => !a.IsMe && a.IsValidTarget() && a.Distance(_Player) <= 950);
            if (Selector == null)
            {
                return;
            }
            if (Q.IsReady() && Player.Instance.ManaPercent >= ManaW && Selector.IsValidTarget(Q.Range))
            foreach (var henemies in enemies)
            {
                if (HarassMenu["haras" + henemies.ChampionName].Cast<CheckBox>().CurrentValue)
                {
                    var predQ = Q.GetPrediction(Selector);
                    if (predQ.HitChance >= HitChance.Medium)
                    {
                        Q.Cast(predQ.CastPosition);
                    }
                }
			}
            if (useW && Player.Instance.ManaPercent >= ManaW && W.IsReady() && Selector.IsValidTarget(W.Range))
            {
                if (Selector != null)
                {
                    var Wpred = W.GetPrediction(Selector);
				    foreach (var qenemies in enemies)
					{
                        if (Wpred.HitChance >= HitChance.Medium)
                        {
                            W.Cast(Wpred.CastPosition);
                        }
					}
				}
            }
		}

        private static void LaneClear()
        {
            var useQ = ClearMenu["QClear"].Cast<CheckBox>().CurrentValue;
            var ManaQ = ClearMenu["ClearMana"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && Player.Instance.ManaPercent > ManaQ && minions.Count() >= 3)
                {
                    Q.Cast(minion);
                }
			}
        }
		
        private static void JungleClear()
        {
            var useQ = JungleMenu["QJungleClear"].Cast<CheckBox>().CurrentValue;
            var useW = JungleMenu["WJungleClear"].Cast<CheckBox>().CurrentValue;
            var jungMana = JungleMenu["JungleMana"].Cast<Slider>().CurrentValue;
            var jungManaW = JungleMenu["JungleManaW"].Cast<Slider>().CurrentValue;
            var monster =
                EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, Q.Range)
                    .OrderByDescending(a => a.MaxHealth)
                    .FirstOrDefault();
            if (monster == null) return;
            if (Q.IsReady() && useQ && monster.Distance(_Player) <= Q.Range && _Player.ManaPercent > jungMana)
            {
                Q.Cast(monster);
            }
            if (W.IsReady() && useW && monster.Distance(_Player) <= W.Range && _Player.ManaPercent > jungManaW)
            {
                W.Cast(monster);
            }
        }
		
        private static void ResetAttack(AttackableUnit target, EventArgs args)
        {
            var useJ = JungleMenu["JungleAA"].Cast<CheckBox>().CurrentValue;
            var manaJ = JungleMenu["EJung"].Cast<Slider>().CurrentValue;
            var useL = ClearMenu["LaneAA"].Cast<CheckBox>().CurrentValue;
            var mana = ClearMenu["ELane"].Cast<Slider>().CurrentValue;
            if (useJ && E.IsReady() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) && Player.Instance.ManaPercent >= manaJ)
            {
                Player.CastSpell(SpellSlot.E, Game.CursorPos);
                Orbwalker.ResetAutoAttack();
            }
            if (useL && E.IsReady() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && Player.Instance.ManaPercent >= mana)
            {
                Player.CastSpell(SpellSlot.E, Game.CursorPos);
                Orbwalker.ResetAutoAttack();
            }
            if (HarassMenu["HarassAA"].Cast<CheckBox>().CurrentValue && E.IsReady() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && Player.Instance.ManaPercent >= HarassMenu["ManaHarass"].Cast<Slider>().CurrentValue)
            {
                Player.CastSpell(SpellSlot.E, Game.CursorPos);
                Orbwalker.ResetAutoAttack();
            }
        }
		
        public static void QStun()
		{
            var Qstun = Misc["Qstun"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target != null)
            {
                if (Qstun && Q.IsReady())
                {
                    if (target.IsRooted || target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup))
                    {
                        Q.Cast(target.Position);
                    }
                }
            }
		}

        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 0, 200, 350, 480 }[Program.R.Level] + 1.2f * _Player.FlatPhysicalDamageMod));
        }

        private static void KillSteal()
		{
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var KsW = KillStealMenu["KsW"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(1200) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie && (hero.HealthPercent <= 25)))
            {
                if (KsQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                    {
                        var Qpred = Q.GetPrediction(target);
                        if (Qpred.HitChancePercent >= 70)
                        {
                            Q.Cast(Qpred.CastPosition);
                        }
                    }
                }

                if (KsW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.W))
                    {
                        var Wpred = W.GetPrediction(target);
                        if (Wpred.HitChancePercent >= 70)
                        {
                            W.Cast(Wpred.CastPosition);
                        }
					}
                }
                var KsR = KillStealMenu["KsR"].Cast<CheckBox>().CurrentValue;
                var minKsR = KillStealMenu["minKsR"].Cast<Slider>().CurrentValue;
                var maxKsR = KillStealMenu["maxKsR"].Cast<Slider>().CurrentValue;
                if (KsR && R.IsReady())
                {
                    if (target != null)
                    {
                        if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.R) && target.IsInRange(Player.Instance, maxKsR) && !target.IsInRange(Player.Instance, minKsR))
                        {
                            var pred = R.GetPrediction(target);
                            if (pred.HitChancePercent >= 50)
                            {
                                R.Cast(pred.CastPosition);
                            }
                        }
                    }
                }
            }
		}
    }
}