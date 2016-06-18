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
using Color = System.Drawing.Color;

namespace Hecarim7
{
    internal class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, Auto, LaneClearMenu, KillStealMenu, Skin, Drawings;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell.Active Q;
        public static Spell.Active W;
        public static Spell.Active E;
        public static Spell.Skillshot R;
        public static Spell.Targeted Ignite;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Hecarim")) return;
            Chat.Print("Hecarim7 Loaded!", Color.Orange);
            Bootstrap.Init(null);

			Q = new Spell.Active(SpellSlot.Q, 350);
            W = new Spell.Active(SpellSlot.W, 525);
            E = new Spell.Active(SpellSlot.E, 450);
            R= new Spell.Skillshot(SpellSlot.R, 1000, SkillShotType.Linear, 250, 800, 200);
            R.AllowedCollisionCount = int.MaxValue;
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);

			Menu = MainMenu.AddMenu("Hecarim7", "Hecarim");
            Menu.AddSeparator();
			ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddSeparator();
            ComboMenu.AddLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Spell [Q]"));
            ComboMenu.Add("ComboW", new CheckBox("Spell [W]"));
            ComboMenu.Add("ComboE", new CheckBox("Spell [E]"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("ComboR", new CheckBox("Spell [R]"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("MinR", new Slider("Min Enemies Use [R]", 3, 0, 5));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Spell [Q]"));
            HarassMenu.Add("ManaQ", new Slider("Min Mana Harass [Q]", 40));
            HarassMenu.AddSeparator();
            HarassMenu.Add("HarassW", new CheckBox("Spell [W]", false) );
            HarassMenu.Add("ManaW", new Slider("Min Mana Harass [W]<=", 40));
                
			Auto = Menu.AddSubMenu("Auto Harass Settings", "Auto Harass");
			Auto.AddLabel("Auto Harass Settings");
            Auto.Add("AutoQ", new CheckBox("Auto [Q]"));
            Auto.Add("ManaQ", new Slider("Min Mana Auto [Q]", 60));

            LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            LaneClearMenu.AddLabel("LastHit Settings");
            LaneClearMenu.Add("LastQ", new CheckBox("Spell [Q] LastHit"));
            LaneClearMenu.Add("LhMana", new Slider("Min Mana Lasthit [Q]", 60));
            LaneClearMenu.AddSeparator();
            LaneClearMenu.AddLabel("Lane Clear Settings");
            LaneClearMenu.Add("LastQLC", new CheckBox("LaneClear With [Q]"));
            LaneClearMenu.Add("ManaLC", new Slider("Min Mana [Q] LaneClear", 50));
            LaneClearMenu.Add("LastWLC", new CheckBox("LaneClear With [W]"));
            LaneClearMenu.Add("ManaLCW", new Slider("Min Mana [W] LaneClear", 70));			

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddLabel("KillSteal Settings");
            KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
            KillStealMenu.Add("KsW", new CheckBox("Use [W] KillSteal"));
            KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));
            KillStealMenu.AddSeparator();
            KillStealMenu.AddLabel("Ultimate Settings");
            KillStealMenu.Add("KsR", new CheckBox("Use [R] KillSteal"));
            KillStealMenu.Add("minKsR", new Slider("Min [R] Range KillSteal", 500, 1, 700));

            Skin = Menu.AddSubMenu("Skin Changer", "SkinChanger");
            Skin.Add("checkSkin", new CheckBox("Use Skin Changer"));
            Skin.Add("skin.Id", new ComboBox("Skin Mode", 8, "Default", "1", "2", "3", "4", "5", "6", "7", "8"));

            Drawings = Menu.AddSubMenu("Draw Settings", "Draw");
            Drawings.AddGroupLabel("Drawing Settings");
            Drawings.Add("DrawQ", new CheckBox("Q Range"));
            Drawings.Add("DrawW", new CheckBox("W Range", false));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnTick += Game_OnTick;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Drawings["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (Drawings["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
			{
                Harass();
			}
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
			{
                LastHit();
			}
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
			{
				Combo();
			}
                KillSteal();
                AutoQ();
				
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
		
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(700, DamageType.Mixed);
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var MinR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
            if (target != null)
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.Cast();
                }
                if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
                if (useE && E.IsReady() && target.IsValidTarget(800))
                {
                    E.Cast();
                }
            }
            var Selector = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            if (Selector != null)
            {
                if (useR && R.IsReady() && Selector.IsValidTarget(R.Range))
                {
                    var pred = R.GetPrediction(Selector);
                    if (pred.CastPosition.CountEnemiesInRange(R.Width) >= MinR && pred.HitChance >= HitChance.High)
                    {
                        R.Cast(pred.CastPosition);
                    }
		    	}
	    	}
        }


        private static void LaneClear()
        {
            var laneQMN = LaneClearMenu["ManaLC"].Cast<Slider>().CurrentValue;
            var laneWMN = LaneClearMenu["ManaLCW"].Cast<Slider>().CurrentValue;
            var useQLH = LaneClearMenu["LastQLC"].Cast<CheckBox>().CurrentValue;
            var useWLH = LaneClearMenu["LastWLC"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQLH && Q.IsReady() && Player.Instance.ManaPercent > laneQMN && minion.IsValidTarget(Q.Range) && minions.Count() >= 3)
                {
                    Q.Cast();
                }
                if (useWLH && W.IsReady() && Player.Instance.ManaPercent > laneWMN && minion.IsValidTarget(W.Range) && minions.Count() >= 3)
                {
                    W.Cast();
                }
            }
        }

        private static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var ManaQ = HarassMenu["ManaQ"].Cast<Slider>().CurrentValue;
            var ManaW = HarassMenu["ManaW"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(700, DamageType.Mixed);
            if (target != null)
            {
                if (useQ && Q.IsReady() && Player.Instance.ManaPercent > ManaQ && target.IsValidTarget(Q.Range))
                {
                    Q.Cast();
                }
                if (useW && W.IsReady() && Player.Instance.ManaPercent > ManaW && target.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
            }
        }

        private static void AutoQ()
        {
            var useQ = Auto["AutoQ"].Cast<CheckBox>().CurrentValue;
            var mana = Auto["ManaQ"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Mixed);
            if (useQ && Q.IsReady() && Player.Instance.ManaPercent > mana && target.IsValidTarget(Q.Range))
            {
                Q.Cast();
            }
        }

        private static void LastHit()
        {
            var useQ = LaneClearMenu["LastQ"].Cast<CheckBox>().CurrentValue;
            var LhM = LaneClearMenu["LhMana"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && Player.Instance.ManaPercent > LhM && minion.IsValidTarget(Q.Range) && minion.Health < QDamage(minion))
                {
                    Q.Cast();
                }
            }
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 0, 150, 250, 350 }[Program.R.Level] + 1.0f * _Player.FlatMagicDamageMod));
        }

        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 0, 35, 60, 90, 120, 200 }[Program.Q.Level] + 0.5f * _Player.FlatPhysicalDamageMod + 0.0f * _Player.FlatMagicDamageMod
                    ));
        }

        private static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var KsW = KillStealMenu["KsW"].Cast<CheckBox>().CurrentValue;
            var minKsR = KillStealMenu["minKsR"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(1200) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie && (hero.HealthPercent <= 25)))
            {
                if (KsQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (target.Health + target.AttackShield < QDamage(target))
                    {
                        Q.Cast();
                    }
                }

                if (KsW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.W))
                    {
                        W.Cast();
					}
                }
                var KsR = KillStealMenu["KsR"].Cast<CheckBox>().CurrentValue;
                if (KsR && R.IsReady())
                {
                    if (target != null)
                    {
                        if (target.Health + target.AttackShield < RDamage(target) && !target.IsInRange(Player.Instance, minKsR))
                        {
                            var pred = R.GetPrediction(target);
                            if (pred.HitChancePercent >= 10)
                            {
                                R.Cast(pred.CastPosition);
                            }
                        }
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
