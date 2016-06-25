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

namespace Trundle7
{
    internal class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, LaneClearMenu, JungleClearMenu, KillStealMenu, Items, Misc;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell.Active Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Targeted R;
        public static Spell.Targeted Ignite;
        public static Item Hydra;
        public static Item Tiamat;
        public static Item Titanic;
        public static Item Botrk;
        public static Item Bil;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Trundle")) return;
            Chat.Print("Trundle7 Loaded!", Color.Orange);
            Bootstrap.Init(null);
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Circular, 0, 2000, 900);
            E = new Spell.Skillshot(SpellSlot.E, 1000, SkillShotType.Circular, 500, int.MaxValue, 80);
            R = new Spell.Targeted(SpellSlot.R, 650);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Tiamat = new Item(ItemId.Tiamat_Melee_Only, 400);
            Hydra = new Item(ItemId.Ravenous_Hydra_Melee_Only, 400);
            Titanic = new Item(ItemId.Titanic_Hydra, Player.Instance.GetAutoAttackRange());
            Botrk = new Item( ItemId.Blade_of_the_Ruined_King);
            Bil = new Item(3144, 475f);

            Menu = MainMenu.AddMenu("Trundle7", "Trundle");
            Menu.AddSeparator();
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddSeparator();
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.AddGroupLabel("Combo [E] Settings");
            ComboMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            ComboMenu.Add("ekc", new Slider("Min Distance Use [E]", 300, 1, 1000));
            ComboMenu.AddGroupLabel("Ultimate Health Settings");
            ComboMenu.Add("ultiR", new CheckBox("Use [R] My Health"));
            ComboMenu.Add("MinR", new Slider("Min Health Use [R]", 60));
            ComboMenu.AddGroupLabel("Use [R] On");
            foreach (var target in EntityManager.Heroes.Enemies)
            {
                ComboMenu.Add("useRCombo" + target.ChampionName, new CheckBox("" + target.ChampionName));
            }

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HarassW", new CheckBox("Use [W] Harass", false));
            HarassMenu.Add("HarassE", new CheckBox("Use [E] Harass"));
            HarassMenu.Add("MHR", new Slider("Min Mana Harass", 40));

            LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("Lane Clear Settings");
            LaneClearMenu.Add("QLC", new CheckBox("Use [Q] LaneClear"));
            LaneClearMenu.Add("WLC", new CheckBox("Use [W] LaneClear", false));
            LaneClearMenu.Add("MLC", new Slider("Min Mana LaneClear", 60));
            LaneClearMenu.AddSeparator();
            LaneClearMenu.AddGroupLabel("Lasthit Settings");
            LaneClearMenu.Add("LHQ", new CheckBox("Use [Q] LastHit"));
            LaneClearMenu.Add("MLH", new Slider("Min Mana Lasthit", 60));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("QJungle", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("WJungle", new CheckBox("Use [W] JungleClear"));
            JungleClearMenu.Add("itemJC", new CheckBox("Use [Items] JungleClear"));
            JungleClearMenu.Add("MJC", new Slider("Min Mana JungleClear", 30));

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
            KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));

            Items = Menu.AddSubMenu("Items Settings", "Items");
            Items.AddGroupLabel("Items Settings");
            Items.Add("hydra", new CheckBox("Use [Hydra] Reset AA"));
            Items.Add("titanic", new CheckBox("Use [Titanic]"));
            Items.Add("BOTRK", new CheckBox("Use [Botrk]"));
            Items.Add("ihp", new Slider("My HP Use BOTRK <=", 50));
            Items.Add("ihpp", new Slider("Enemy HP Use BOTRK <=", 50));

            Misc = Menu.AddSubMenu("Misc Settings", "Misc");
            Misc.AddGroupLabel("Skin Settings");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Misc.Add("skin.Id", new ComboBox("Skin Mode", 0, "Default", "1", "2", "3", "4"));
            Misc.AddGroupLabel("Interrupt Settings");
            Misc.Add("inter", new CheckBox("Use [E] Interupt"));
            Misc.Add("AntiGap", new CheckBox("Use [E] Anti Gapcloser"));
            Misc.AddGroupLabel("Drawing Settings");
            Misc.Add("DrawW", new CheckBox("W Range"));
            Misc.Add("DrawE", new CheckBox("E Range", false));
            Misc.Add("DrawR", new CheckBox("R Range", false));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnTick += Game_OnTick;
            Orbwalker.OnPostAttack += ResetAttack;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Interrupter.OnInterruptableSpell += Interupt;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Misc["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (Misc["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
            if (Misc["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            KillSteal();
            Item();
            if (_Player.SkinId != Misc["skin.Id"].Cast<ComboBox>().CurrentValue)
            {
                if (checkSkin())
                {
                    Player.SetSkinId(SkinId());
                }
            }
        }

        public static void Interupt(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs i)
        {
            var Inter = Misc["inter"].Cast<CheckBox>().CurrentValue;
            if (!sender.IsEnemy || !(sender is AIHeroClient) || Player.Instance.IsRecalling())
            {
                return;
            }
            if (Inter && E.IsReady() && i.DangerLevel == DangerLevel.High && E.IsInRange(sender))
            {
                E.Cast(sender.Position);
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (Misc["AntiGap"].Cast<CheckBox>().CurrentValue && sender.IsEnemy && e.Sender.Distance(_Player) < 300)
            {
                E.Cast(sender.Position);
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

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var E2dis = ComboMenu["ekc"].Cast<Slider>().CurrentValue;
            var useR = ComboMenu["ultiR"].Cast<CheckBox>().CurrentValue;
            var minR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
            if (target != null)
            {
                if (useW && W.IsReady() && W.IsInRange(target) && !target.IsDead && !target.IsZombie)
                {
                    W.Cast(target);
                }
                if (useE && E.IsReady() && E.IsInRange(target) && E2dis <= target.Distance(Player.Instance))
                {
                    E.Cast(target);
                }
                if (useR && _Player.HealthPercent <= minR && target.IsValidTarget(450))
                {
                    if (ComboMenu["useRCombo" + target.ChampionName].Cast<CheckBox>().CurrentValue)
                    {
                        R.Cast(target);
                    }
                }
            }
        }

        public static void Item()
        {

            var item = Items["BOTRK"].Cast<CheckBox>().CurrentValue;
            var Minhp = Items["ihp"].Cast<Slider>().CurrentValue;
            var Minhpp = Items["ihpp"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(550, DamageType.Physical);
            if (target != null)
            {
                if (item && Bil.IsReady() && Bil.IsOwned() && target.IsValidTarget(550))
                {
                    Bil.Cast(target);
                }
                if ((item && Botrk.IsReady() && Botrk.IsOwned() && target.IsValidTarget(550)) && (Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp))
                {
                    Botrk.Cast(target);
                }
            }
        }

        private static void ResetAttack(AttackableUnit target, EventArgs args)
        {
            var Titan = Items["titanic"].Cast<CheckBox>().CurrentValue;
            var useriu = Items["hydra"].Cast<CheckBox>().CurrentValue;
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            if (target != null)
            {
                if (useriu && !W.IsReady() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    if (Hydra.IsOwned() && Hydra.IsReady() && target.IsValidTarget(_Player.AttackRange))
                    {
                        Hydra.Cast();
                    }

                    if (Tiamat.IsOwned() && Tiamat.IsReady() && target.IsValidTarget(_Player.AttackRange))
                    {
                        Tiamat.Cast();
                    }
                }
                if (Titan && target.IsValidTarget() && Titanic.IsReady())
                {
                    Titanic.Cast();
                }
                if (useQ && Q.IsReady() && target.IsValidTarget(325) && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    Player.CastSpell(SpellSlot.Q);
                    Orbwalker.ResetAutoAttack();
                    Orbwalker.ForcedTarget = target;
                }
            }
        }

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["QLC"].Cast<CheckBox>().CurrentValue;
            var useW = LaneClearMenu["WLC"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["MLC"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            if (Player.Instance.ManaPercent < mana) return;
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(400) && Player.Instance.GetSpellDamage(minion, SpellSlot.Q) >= minion.TotalShieldHealth())
                {
                    Q.Cast();
                }
                if (useW && W.IsReady() && minion.IsValidTarget(W.Range) && minions.Count() >= 3)
                {
                    W.Cast(minion);
                }
            }
        }

        public static void LastHit()
        {
            var useQ = LaneClearMenu["LHQ"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["MLH"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            if (Player.Instance.ManaPercent < mana) return;
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(350) && Player.Instance.GetSpellDamage(minion, SpellSlot.Q) >= minion.TotalShieldHealth())
                {
                    Q.Cast();
                }
            }
        }

        private static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["MHR"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(700, DamageType.Physical);
            if (Player.Instance.ManaPercent < mana) return;
            if (target != null)
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(_Player.AttackRange) && !target.IsDead && !target.IsZombie)
                {
                    Q.Cast();
                    Orbwalker.ForcedTarget = target;
                }
                if (useW && W.IsReady() && W.IsInRange(target) && !target.IsDead && !target.IsZombie)
                {
                    W.Cast(target);
                }
                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !target.IsDead && !target.IsZombie)
                {
                    E.Cast(target);
                }
            }
        }

        public static void JungleClear()
        {
            var useQ = JungleClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var useW = JungleClearMenu["WJungle"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["MJC"].Cast<Slider>().CurrentValue;
            var useriu = JungleClearMenu["itemJC"].Cast<CheckBox>().CurrentValue;
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(Q.Range));
            if (Player.Instance.ManaPercent < mana) return;
            if (monster != null)
            {
                if (useQ && Q.IsReady() && monster.IsValidTarget(325))
                {
                    Q.Cast();
                }
                if (useW && W.IsReady() && W.IsInRange(monster))
                {
                    W.Cast(monster);
                }
                if (useriu && !Q.IsReady())
                {
                    if (Hydra.IsOwned() && Hydra.IsReady() && monster.IsValidTarget(325))
                    {
                        Hydra.Cast();
                    }
                    if (Tiamat.IsOwned() && Tiamat.IsReady() && monster.IsValidTarget(325))
                    {
                        Tiamat.Cast();
                    }
                }
            }
        }

        public static void Flee()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target != null)
            {
                if (E.IsReady() && target.IsValidTarget(E.Range))
                {
                    E.Cast(target);
                }
            }
            if (W.IsReady())
            {
                Player.CastSpell(SpellSlot.W, Game.CursorPos);
            }
        }

        private static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(600) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie))
            {
                if (KsQ && Q.IsReady() && target.IsValidTarget(250))
                {
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                    {
                        Q.Cast();
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
