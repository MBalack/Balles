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

namespace Ryze
{
    class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Targeted E;
        public static Spell.Targeted W;
        public static Spell.Active R;
        public static Item Seraph;
        public static Spell.Targeted Ignite;
        public static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }
        public static Menu menu, ComboMenu, HarassMenu, JungleMenu, ClearMenu, LastHitMenu, KsMenu, Autos, Draws, Misc;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        static void Main(string[] args)
        {

            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Ryze")) return;
            Chat.Print("Ryze7 Loaded!", Color.Orange);
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, 900, SkillShotType.Linear, 250, 1500, 50);
            Q.AllowedCollisionCount = 0;
            W = new Spell.Targeted(SpellSlot.W, 600);
            E = new Spell.Targeted(SpellSlot.E, 600);
            R = new Spell.Active(SpellSlot.R);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Seraph = new Item(3040);
            menu = MainMenu.AddMenu("Ryze7", "Ryze");

            ComboMenu = menu.AddSubMenu("Combo Settings", "Combo");
			ComboMenu.Add("ComboQ", new CheckBox("Spell [Q]"));
            ComboMenu.Add("ComboW", new CheckBox("Spell [W]"));
            ComboMenu.Add("ComboE", new CheckBox("Spell [E]"));
            ComboMenu.Add("ComboR", new CheckBox("Spell [R]"));
            ComboMenu.Add("Human", new Slider("Humanizer Delay", 1, 0, 1000));

            HarassMenu = menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.Add("HQ", new CheckBox("Spell [Q]"));
            HarassMenu.Add("HW", new CheckBox("Spell [W]"));
            HarassMenu.Add("HE", new CheckBox("Spell [E]"));
            HarassMenu.Add("HarassMana", new Slider("Min Mana For Harass", 50, 0, 100));

            LastHitMenu = menu.AddSubMenu("LastHit Settings", "LastHit");
            LastHitMenu.Add("LHQ", new CheckBox("Spell [Q]"));
            LastHitMenu.Add("LHW", new CheckBox("Spell [W]", false));
            LastHitMenu.Add("LHE", new CheckBox("Spell [E]", false));
            LastHitMenu.Add("LastHitMana", new Slider("Min Mana For LastHit", 50, 0, 100));

            ClearMenu = menu.AddSubMenu("LaneClear Settings", "LaneClear");
            ClearMenu.Add("LCQ", new CheckBox("Spell [Q]"));
            ClearMenu.Add("LCW", new CheckBox("Spell [W]"));
            ClearMenu.Add("LCE", new CheckBox("Spell [E]"));
            ClearMenu.Add("LCR", new CheckBox("Spell [R]", false));
            ClearMenu.Add("LaneClearMana", new Slider("Min Mana For LaneClear", 50, 0, 100));

            JungleMenu = menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleMenu.Add("JQ", new CheckBox("Spell [Q]"));
            JungleMenu.Add("JW", new CheckBox("Spell [W]"));
            JungleMenu.Add("JE", new CheckBox("Spell [E]"));
            JungleMenu.Add("JR", new CheckBox("Spell [R]", false));
            JungleMenu.Add("JungleClearMana", new Slider("Min Mana For JungleClear", 30, 0, 100));

            KsMenu = menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KsMenu.AddGroupLabel("KillSteal Setting");
            KsMenu.Add("KsQ", new CheckBox("Spell [Q]"));
            KsMenu.Add("KsW", new CheckBox("Spell [W]"));
            KsMenu.Add("KsE", new CheckBox("Spell [E]"));
            KsMenu.Add("KsIgnite", new CheckBox("Use [Ignite] KillSteal"));
			
            Misc = menu.AddSubMenu("Misc Settings", "Misc");
            Misc.AddGroupLabel("AntiGap Setting");
            Misc.Add("gapw", new CheckBox("AntiGap [W]"));
            Misc.AddGroupLabel("Seraph Settings");
            Misc.Add("dts", new CheckBox("Use Seraph"));
            Misc.Add("Hp", new Slider("HP For Seraph", 30, 0, 100));
            Misc.AddGroupLabel("Skin Changer");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer"));
            Misc.Add("skin.Id", new ComboBox("Skin Mode", 3, "Default", "1", "2", "3", "4", "5", "6", "7", "8"));
			
            Autos = menu.AddSubMenu("Stacks Settings", "Stacks");
            Autos.Add("AutoStack", new KeyBind("Auto Stack", false, KeyBind.BindTypes.PressToggle, 'T'));
            Autos.Add("MaxStack", new Slider("Keep Max Stacks", 2, 1, 5));
            Autos.Add("StackMana", new Slider("Min Mana AutoStack", 70, 0, 100));

            Draws = menu.AddSubMenu("Drawings Settings", "Draw");
            Draws.AddSeparator(10);
            Draws.AddGroupLabel("Drawings Setting");
            Draws.Add("DrawQ", new CheckBox("Q Range"));
            Draws.Add("DrawW", new CheckBox("W / E Range"));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnTick += Game_OnTick;
            Gapcloser.OnGapcloser += AntiGapCloser;
		}


        private static void Game_OnTick(EventArgs args)
        {
            Orbwalker.DisableAttacking = false;

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
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
                AutoStack();
                KillSteal();
                Dtc();

            if (_Player.SkinId != Misc["skin.Id"].Cast<ComboBox>().CurrentValue)
            {
                if (checkSkin())
                {
                    Player.SetSkinId(SkinId());
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Draws["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 6f, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (Draws["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 6f, Radius = W.Range }.Draw(_Player.Position);
            }
        }
		
        private static void AntiGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (!e.Sender.IsValidTarget() || e.Sender.Type != Player.Type || !e.Sender.IsEnemy)
            {
                return;
            }
            if (W.IsReady() && W.IsInRange(sender) && (Misc["gapw"].Cast<CheckBox>().CurrentValue))
            {
                W.Cast(sender);
            }
        }

        public static void QCast(Obj_AI_Base target)
        {
            var QPred = Q.GetPrediction(target);
            if (target.IsValidTarget(Q.Range) && Q.IsReady() && !target.IsDead)
            {
                Core.DelayAction(() => Q.Cast(QPred.CastPosition), ComboMenu["Human"].Cast<Slider>().CurrentValue);
            }
		    else
            {
                if (target.IsValidTarget(Q.Range) && Q.IsReady() && target.IsRooted && !target.IsDead)
                {
                    Core.DelayAction(() => Q.Cast(target.ServerPosition), ComboMenu["Human"].Cast<Slider>().CurrentValue);
                }
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
		
        private static void AutoStack()
        {
            var stacks = Player.GetBuffCount("ryzepassivestack");
            var mana = Autos["StackMana"].Cast<Slider>().CurrentValue;
            var ats = Autos["AutoStack"].Cast<KeyBind>().CurrentValue;
            var max = Autos["MaxStack"].Cast<Slider>().CurrentValue;
            if (ats && Player.CountEnemiesInRange(Q.Range) <= 0 && !Player.IsRecalling() && _Player.ManaPercent >= mana)
            {
                if (Q.IsReady() && stacks <= max && !EntityManager.MinionsAndMonsters.CombinedAttackable.Any(x => x.IsValidTarget(Q.Range)))
                {
                    Q.Cast(Player.ServerPosition.Extend(Game.CursorPos, Q.Range).To3D());
                }
            }
        }

        public static void Combo()
        {
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;

            var target = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            Orbwalker.ForcedTarget = target;
            if (target != null && target.IsValidTarget())
            {
				if (W.IsReady() || E.IsReady())
		 	    {
		 	       Orbwalker.DisableAttacking = true;
				}
                if (useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useE && E.IsReady())
                    Core.DelayAction(() => E.Cast(target), ComboMenu["Human"].Cast<Slider>().CurrentValue);
                else if (!E.IsReady() && useW && W.IsReady())
                    Core.DelayAction(() => W.Cast(target), ComboMenu["Human"].Cast<Slider>().CurrentValue);
                else if (!E.IsReady() && useR && R.IsReady() && Player.CountEnemiesInRange(W.Range + 200) >= 1)
                    Core.DelayAction(() => R.Cast(), ComboMenu["Human"].Cast<Slider>().CurrentValue);
                if (!R.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useE && E.IsReady())
                    Core.DelayAction(() => E.Cast(target), ComboMenu["Human"].Cast<Slider>().CurrentValue);
                if (!E.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useW && W.IsReady())
                    Core.DelayAction(() => W.Cast(target), ComboMenu["Human"].Cast<Slider>().CurrentValue);
                else if (!W.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useE && E.IsReady())
                    Core.DelayAction(() => E.Cast(target), ComboMenu["Human"].Cast<Slider>().CurrentValue);
                else if (!E.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useW && W.IsReady())
                    Core.DelayAction(() => W.Cast(target), ComboMenu["Human"].Cast<Slider>().CurrentValue);
                else if (!W.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useE && E.IsReady())
                    Core.DelayAction(() => E.Cast(target), ComboMenu["Human"].Cast<Slider>().CurrentValue);
                else if (!E.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useW && W.IsReady())
                    Core.DelayAction(() => W.Cast(target), ComboMenu["Human"].Cast<Slider>().CurrentValue);
                else if (!W.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useE && E.IsReady())
                    Core.DelayAction(() => E.Cast(target), ComboMenu["Human"].Cast<Slider>().CurrentValue);
                else if (!E.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useW && W.IsReady())
                    Core.DelayAction(() => W.Cast(target), ComboMenu["Human"].Cast<Slider>().CurrentValue);
                else if (!W.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useE && E.IsReady())
                    Core.DelayAction(() => E.Cast(target), ComboMenu["Human"].Cast<Slider>().CurrentValue);
                else if (!E.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useW && W.IsReady())
                    Core.DelayAction(() => W.Cast(target), ComboMenu["Human"].Cast<Slider>().CurrentValue);
            }
        }

        private static void Dtc()
        {
            if (!Player.IsDead && Misc["dts"].Cast<CheckBox>().CurrentValue)
            {
                if (Seraph.IsOwned() && Seraph.IsReady() && Player.HealthPercent <= Misc["Hp"].Cast<Slider>().CurrentValue && Player.Position.CountEnemiesInRange(600) >= 1)
                {
                    Seraph.Cast();
                }
            }
        }

        public static void Harass()
        {
            var useQ = HarassMenu["HQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HW"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HE"].Cast<CheckBox>().CurrentValue;

            var target = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            Orbwalker.ForcedTarget = target;
            if (target != null && target.IsValidTarget() && _Player.ManaPercent > HarassMenu["HarassMana"].Cast<Slider>().CurrentValue)
            {
                if (useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useE && E.IsReady())
                    E.Cast(target);
                else if (!E.IsReady() && useW && W.IsReady())
                    W.Cast(target);
                if (!W.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useE && E.IsReady())
                    E.Cast(target);
                if (!E.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useW && W.IsReady())
                    W.Cast(target);
                else if (!W.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useE && E.IsReady())
                    E.Cast(target);
                else if (!E.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useW && W.IsReady())
                    W.Cast(target);
                else if (!W.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useE && E.IsReady())
                    E.Cast(target);
                else if (!E.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useW && W.IsReady())
                    W.Cast(target);
                else if (!W.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useE && E.IsReady())
                    E.Cast(target);
                else if (!E.IsReady() && useQ && Q.IsReady())
                    QCast(target);
                else if (!Q.IsReady() && useW && W.IsReady())
                    W.Cast(target);
            }
        }

        public static void KillSteal()
        {
            var useQ = KsMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var useW = KsMenu["KsW"].Cast<CheckBox>().CurrentValue;
            var useE = KsMenu["KsE"].Cast<CheckBox>().CurrentValue;
            foreach (AIHeroClient e in EntityManager.Heroes.Enemies)
            {
                if (e.IsValidTarget(Q.Range))
                {
                    if (useQ && Q.IsReady() && (_Player.GetSpellDamage(e, SpellSlot.Q) >= e.Health))
                    {
                        Q.Cast(e);
                    }
                    if (useW && W.IsReady() && (_Player.GetSpellDamage(e, SpellSlot.W) >= e.Health))
                    {
                        W.Cast(e);
                    }
                    if (useE && E.IsReady() && (_Player.GetSpellDamage(e, SpellSlot.E) >= e.Health))
                    {
                        E.Cast(e);
                    }
                    if (Ignite != null && KsMenu["KsIgnite"].Cast<CheckBox>().CurrentValue && Ignite.IsReady())
                    {
                        if (e.Health < _Player.GetSummonerSpellDamage(e, DamageLibrary.SummonerSpells.Ignite))
                        {
                            Ignite.Cast(e);
                        }
                    }
                }
            }
        }

        public static void LastHit()
        {
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().OrderByDescending(m => m.Distance(Player)).FirstOrDefault(m => m.IsValidTarget(Q.Range));
            var useQ = LastHitMenu["LHQ"].Cast<CheckBox>().CurrentValue;
            var useW = LastHitMenu["LHW"].Cast<CheckBox>().CurrentValue;
            var useE = LastHitMenu["LHE"].Cast<CheckBox>().CurrentValue;
            {
                if (_Player.ManaPercent > LastHitMenu["LastHitMana"].Cast<Slider>().CurrentValue)
                {
                    if (useQ && Q.IsReady() && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.Q) && _Player.GetAutoAttackDamage(minion) < minion.TotalShieldHealth())
                    {
                        Q.Cast(minion);
                    }
                    if (useW && W.IsReady() && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.W))
                    {
                        W.Cast(minion);
                    }
                    if (useE && E.IsReady() && minion.Health <= _Player.GetSpellDamage(minion, SpellSlot.E))
                    {
                        E.Cast(minion);
                    }
                }
            }
        }
		
        public static void LaneClear()
        {
            var useQ = ClearMenu["LCQ"].Cast<CheckBox>().CurrentValue;
            var useW = ClearMenu["LCW"].Cast<CheckBox>().CurrentValue;
            var useE = ClearMenu["LCE"].Cast<CheckBox>().CurrentValue;
            var useR = ClearMenu["LCR"].Cast<CheckBox>().CurrentValue;
            foreach (var minion in EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, _Player.Position, Q.Range))
            {
                if (_Player.ManaPercent > ClearMenu["LaneClearMana"].Cast<Slider>().CurrentValue)
                {
                if (useQ && Q.IsReady())
                    Q.Cast(minion);
                else if (!Q.IsReady() && useE && E.IsReady())
                    E.Cast(minion);
                else if (!E.IsReady() && useW && W.IsReady())
                    W.Cast(minion);
                else if (!E.IsReady() && useR && R.IsReady())
                    R.Cast();
                if (!R.IsReady() && useQ && Q.IsReady())
                    Q.Cast(minion);
                else if (!Q.IsReady() && useE && E.IsReady())
                    E.Cast(minion);
                if (!E.IsReady() && useQ && Q.IsReady())
                    Q.Cast(minion);
                else if (!Q.IsReady() && useW && W.IsReady())
                    W.Cast(minion);
                else if (!W.IsReady() && useQ && Q.IsReady())
                    Q.Cast(minion);
                }
            }
        }

        public static void JungleClear()
        {
            var jungle = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.ServerPosition, Q.Range).OrderByDescending(a => a.MaxHealth).FirstOrDefault();
            var useQ = JungleMenu["JQ"].Cast<CheckBox>().CurrentValue;
            var useW = JungleMenu["JW"].Cast<CheckBox>().CurrentValue;
            var useE = JungleMenu["JE"].Cast<CheckBox>().CurrentValue;
            var useR = JungleMenu["JR"].Cast<CheckBox>().CurrentValue;
            {
                if (jungle != null && jungle.IsValidTarget() && _Player.ManaPercent > JungleMenu["JungleClearMana"].Cast<Slider>().CurrentValue)
                {
                    if (useQ && Q.IsReady())
                    {
                        Q.Cast(jungle);
                    }
                    if (useW && W.IsReady())
                    {
                        W.Cast(jungle);
                    }
                    if (useE && E.IsReady())
                    {
                        E.Cast(jungle);
                    }
                    if (useR && _Player.Distance(jungle) <= W.Range && R.IsReady())
                    {
                        R.Cast();
                    }
                }
            }
        }
    }
}
