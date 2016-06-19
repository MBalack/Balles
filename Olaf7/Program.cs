using System;
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

namespace Olaf7
{
    internal class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, LaneClearMenu, LastHitMenu, JungleClearMenu, Misc, KillStealMenu, Skin, Drawings;
        public static Item Botrk;
        public static Item Hydra;
        public static Item Tiamat;
        public static Item Titanic;
        public static AIHeroClient PlayerInstance
        {
            get { return Player.Instance; }
        }
        private static float HealthPercent()
        {
            return (PlayerInstance.Health / PlayerInstance.MaxHealth) * 100;
        }
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Targeted E;
        public static Spell.Active R;
        public static Spell.Targeted Ignite;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
                if (!_Player.ChampionName.Contains("Olaf")) return;
                Chat.Print("Olaf7 Loaded!", Color.Red);
                Chat.Print("Doctor7", Color.GreenYellow);
                Bootstrap.Init(null);
                Q = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 250, 1550, 75);
                Q.AllowedCollisionCount = int.MaxValue;
                W = new Spell.Active(SpellSlot.W);
                E = new Spell.Targeted(SpellSlot.E,325);
                R= new Spell.Active(SpellSlot.R);
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
                Botrk = new Item( ItemId.Blade_of_the_Ruined_King);
                Tiamat = new Item( ItemId.Tiamat_Melee_Only, 400);
                Hydra = new Item( ItemId.Ravenous_Hydra_Melee_Only, 400);
                Titanic = new Item( ItemId.Titanic_Hydra, Player.Instance.GetAutoAttackRange());
                Menu = MainMenu.AddMenu("Olaf7", "Olaf");
                Menu.AddGroupLabel("Olaf7");
                Menu.AddLabel(" Leave Feedback For Any Bugs ");

                ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
                ComboMenu.AddGroupLabel("Combo Settings");
                ComboMenu.Add("ComboQ", new CheckBox("Use [Q]"));
                ComboMenu.Add("ComboW", new CheckBox("Use [W]"));
                ComboMenu.Add("ComboE", new CheckBox("Use [E]"));
                ComboMenu.Add("item", new CheckBox("Use [Item]"));

                HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
                HarassMenu.AddGroupLabel("Harass Settings");
                HarassMenu.Add("HarassQ", new CheckBox("Use [Q]"));
                HarassMenu.Add("HarassW", new CheckBox("Use [W]", false));
                HarassMenu.Add("HarassE", new CheckBox("Use [E]"));
                HarassMenu.Add("ManaQ", new Slider("Min Mana For Harass", 40));

                LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
                LaneClearMenu.AddGroupLabel("LaneClear Settings");
                LaneClearMenu.Add("ClearQ", new CheckBox("Use [Q]"));
                LaneClearMenu.Add("minq", new Slider("Min Minions Use [Q]", 3, 1, 6));
                LaneClearMenu.Add("ClearW", new CheckBox("Use [W]"));
                LaneClearMenu.Add("Wlc", new Slider("Health For [W] LaneClear", 80));
                LaneClearMenu.Add("ClearE", new CheckBox("Use [E]"));
                LaneClearMenu.Add("ManaLC", new Slider("Min Mana For LaneClear", 60));

                LastHitMenu = Menu.AddSubMenu("LastHit Settings", "LastHit");
                LastHitMenu.AddGroupLabel("LastHit Settings");
                LastHitMenu.Add("LastE", new CheckBox("Use [E] LastHit"));
                LastHitMenu.Add("LastAA", new CheckBox("Only [E] If Out AA Range", false));
                LastHitMenu.Add("LastQ", new CheckBox("Use [Q] LastHit", false));
                LastHitMenu.Add("LhMana", new Slider("Min Mana For LastHit", 60));


                JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
                JungleClearMenu.AddGroupLabel("JungleClear Settings");
                JungleClearMenu.Add("QJungle", new CheckBox("Use [Q]"));
                JungleClearMenu.Add("WJungle", new CheckBox("Use [W]"));
                JungleClearMenu.Add("EJungle", new CheckBox("Use [E]"));
                JungleClearMenu.Add("MnJungle", new Slider("Min Mana JungleClear", 30));

                Misc = Menu.AddSubMenu("Ultimate Settings", "Misc");
                Misc.AddGroupLabel("Ultimate Setting");
                Misc.Add("Ulti", new CheckBox("Use Ultimate"));
                Misc.AddGroupLabel("Use [R] On");
                Misc.Add("stun", new CheckBox("Stuns"));
                Misc.Add("rot", new CheckBox("Root"));
                Misc.Add("knockup", new CheckBox("Knock Ups"));
                Misc.Add("tunt", new CheckBox("Taunt"));
                Misc.Add("charm", new CheckBox("Charm", false));
                Misc.Add("snare", new CheckBox("Snare"));
                Misc.Add("sleep", new CheckBox("Sleep", false));
                Misc.Add("blind", new CheckBox("Blinds", false));
                Misc.Add("disarm", new CheckBox("Disarm", false));
                Misc.Add("fear", new CheckBox("Fear", false));
                Misc.Add("silence", new CheckBox("Silence", false));
                Misc.Add("frenzy", new CheckBox("Frenzy", false));
                Misc.Add("supperss", new CheckBox("Supperss", false));
                Misc.Add("slow", new CheckBox("Slows", false));
                Misc.Add("poison", new CheckBox("Poisons", false));
                Misc.Add("knockback", new CheckBox("Knock Backs", false));
                Misc.Add("nearsight", new CheckBox("NearSight", false));
                Misc.Add("poly", new CheckBox("Polymorph", false));
                Misc.AddGroupLabel("Ultimate Delay");
                Misc.Add("delay", new Slider("Humanizer Delay", 0, 0, 1000));

                KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
                KillStealMenu.AddGroupLabel("KillSteal Settings");
                KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
                KillStealMenu.Add("KsE", new CheckBox("Use [E] KillSteal"));
                KillStealMenu.Add("KsIgnite", new CheckBox("Use [Ignite] KillSteal"));

                Skin = Menu.AddSubMenu("Skin Changer", "SkinChanger");
                Skin.AddGroupLabel("Skin Settings");
                Skin.Add("checkSkin", new CheckBox("Use Skin Changer"));
                Skin.Add("skin.Id", new ComboBox("Skin Mode", 3, "Default", "1", "2", "3", "4", "5"));

                Drawings = Menu.AddSubMenu("Misc Settings", "Draw");
                Drawings.AddGroupLabel("Misc Setting");
                Drawings.Add("QStun", new CheckBox("Use [Q] If Enemy Has CC", false));
                Drawings.Add("AntiGap", new CheckBox("Use [Q] Anti Gapcloser"));
                Drawings.AddGroupLabel("Drawing Settings");
                Drawings.Add("DrawQ", new CheckBox("Q Range"));
                Drawings.Add("DrawE", new CheckBox("E Range", false));

                Drawing.OnDraw += Drawing_OnDraw;
                Game.OnTick += Game_OnTick;
                Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Drawings["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (Drawings["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
        }

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
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }
            KillSteal();
            RStun();
            Ult();

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
		
        public static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var item = ComboMenu["item"].Cast<CheckBox>().CurrentValue;
            if (target != null)
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie)
         	    {
                    Q.Cast(target);
	    		}
                if (useW && W.IsReady() && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie)
         	    {				
                    W.Cast();
	    		}
                if (E.IsReady() && useE && target.IsValidTarget(E.Range))
                {
                    E.Cast(target);
                }
                if (Player.Instance.HealthPercent <= 50 || target.HealthPercent < 50 && item && Botrk.IsReady() && Botrk.IsOwned())
                {
                    Botrk.Cast(target);
                }
                if (item && Hydra.IsOwned() && Hydra.IsReady() && Hydra.IsInRange(target))
                {
                    Hydra.Cast();
                }
                else if (item && Tiamat.IsOwned() && Tiamat.IsReady() && Tiamat.IsInRange(target))
                {
                    Tiamat.Cast();
                }
                if (item && target.IsValidTarget() && Titanic.IsReady())
                {
                    Titanic.Cast();
                }
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {	
            if (Drawings["AntiGap"].Cast<CheckBox>().CurrentValue && sender.IsEnemy && e.Sender.Distance(_Player)<300)
            {
                Q.Cast(e.Sender);
            }
        }

        public static void JungleClear()
        {

            var useQ = JungleClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var useW = JungleClearMenu["WJungle"].Cast<CheckBox>().CurrentValue;
            var useE = JungleClearMenu["EJungle"].Cast<CheckBox>().CurrentValue;
            var jungle =
                EntityManager.MinionsAndMonsters.GetJungleMonsters(_Player.Position, Q.Range)
                    .OrderByDescending(a => a.MaxHealth)
                    .FirstOrDefault();
            if (jungle != null)
            {
                if (Player.Instance.ManaPercent >= JungleClearMenu["MnJungle"].Cast<Slider>().CurrentValue)
                {
                    if (useQ && Q.IsReady())
                    {
                        Q.Cast(jungle);
                    }
                    if (useW && W.IsReady())
                    {
                        W.Cast();
                    }
                }
            }
            if (jungle != null)
            {
                if (useE && E.IsReady())
                {
                    E.Cast(jungle);
                }
            }
        }

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["ClearQ"].Cast<CheckBox>().CurrentValue;
            var useW = LaneClearMenu["ClearW"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["ClearE"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["ManaLC"].Cast<Slider>().CurrentValue;
            var manaW = LaneClearMenu["Wlc"].Cast<Slider>().CurrentValue;
            var MinQ = LaneClearMenu["minq"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useW && W.IsReady() && Player.Instance.ManaPercent > mana && Player.Instance.HealthPercent < manaW && minion.IsValidTarget(E.Range))
                {
                    W.Cast();
                }
                if (useE && E.IsReady() && !minion.IsValidTarget(_Player.AttackRange) && minion.Health < Player.Instance.GetSpellDamage(minion, SpellSlot.E))
                {
                    E.Cast(minion);
                }
                if (useQ && Q.IsReady() && Player.Instance.ManaPercent > mana && minion.IsValidTarget(Q.Range) && minions.Count() > MinQ)
                {
                    Q.Cast(minion);
                }
            }
        }

        public static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var ManaQ = HarassMenu["ManaQ"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target != null)
            {
                if (useQ && Q.IsReady() && Player.Instance.ManaPercent >= ManaQ)
     	        {
                    Q.Cast(target);
		    	}
                if (useW && W.IsReady() && target.IsValidTarget(300) && Player.Instance.ManaPercent >= ManaQ)
                {				
                    W.Cast();
		    	}
                if (E.IsReady() && useE && target.IsValidTarget(E.Range))
                {
                    E.Cast(target);
                }
            }
        }

        public static void LastHit()
        {
            var useE = LastHitMenu["LastE"].Cast<CheckBox>().CurrentValue;
            var useAA = LastHitMenu["LastAA"].Cast<CheckBox>().CurrentValue;
            var useQ = LastHitMenu["LastQ"].Cast<CheckBox>().CurrentValue;
            var LhM = LastHitMenu["LhMana"].Cast<Slider>().CurrentValue;
            var minion = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(m => m.IsValidTarget(E.Range) && (Player.Instance.GetSpellDamage(m, SpellSlot.E) >= m.TotalShieldHealth()));
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead && (Player.Instance.GetSpellDamage(m, SpellSlot.Q) >= m.TotalShieldHealth()));
            foreach (var minionq in minions)
            {
                if (useQ && Q.IsReady() && Player.Instance.ManaPercent > LhM && minionq.IsValidTarget(Q.Range))
                {
                    Q.Cast(minionq);
                }
            }
            if (minion == null) return;
            {
                if (useE && E.IsReady())
                {
                    E.Cast(minion);
                }
                if (useAA && E.IsReady() && Player.Instance.GetAutoAttackRange() <= minion.Distance(Player.Instance))
                {
                    E.Cast(minion);
                }
            }
        }

        public static void RStun()
		{
            var Rstun = Drawings["QStun"].Cast<CheckBox>().CurrentValue;
            if (Rstun && Q.IsReady())
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                if (target != null)
                {
                    if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) ||
                        target.HasBuffOfType(BuffType.Knockup))
                    {
                        Q.Cast(target.Position);
                    }
                }
            }
        }

        public static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var KsE = KillStealMenu["KsE"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(1200) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie && (hero.HealthPercent <= 25)))
            {
                if (KsQ && Q.IsReady())
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

                if (KsE && E.IsReady())
                {
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.E))
                    {
                        E.Cast(target);
                    }
                }
                if (Ignite != null && KillStealMenu["KsIgnite"].Cast<CheckBox>().CurrentValue && Ignite.IsReady())
                {
                    if (target.Health < _Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite))
                    {
                        Ignite.Cast(target);
                    }
                }
            }
        }
        private static void Ult()
        {
            var ulti = Misc["Ulti"].Cast<CheckBox>().CurrentValue;
            var Enemies = Player.CountEnemiesInRange(700);
            var cc = (Misc["silence"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Silence))
            || (Misc["snare"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Snare))
            || (Misc["supperss"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Suppression))
            || (Misc["sleep"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Sleep))
            || (Misc["poly"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Polymorph))
            || (Misc["frenzy"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Frenzy))
            || (Misc["disarm"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Disarm))
            || (Misc["nearsight"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.NearSight))
            || (Misc["knockback"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Knockback))
            || (Misc["knockup"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Knockup))
            || (Misc["slow"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Slow))
            || (Misc["poison"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Poison))
            || (Misc["blind"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Blind))
            || (Misc["charm"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Charm))
            || (Misc["tunt"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Taunt))
            || (Misc["stun"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Stun))
            || (Misc["rot"].Cast<CheckBox>().CurrentValue && Player.IsRooted)
            || (Misc["fear"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Fear));
            if (R.IsReady() && ulti && cc && Enemies >= 1)
            {
                Core.DelayAction(() => R.Cast(), Misc["delay"].Cast<Slider>().CurrentValue);
            }
        }
    }
}
