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
            Chat.Print("Doctor's Trundle Loaded!", Color.Orange);
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
            Menu = MainMenu.AddMenu("Doctor's Trundle", "Trundle");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("comboMode", new ComboBox("Combo Mode:", 0, "Always [Q]", "Only [Q] Reset AA"));
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
            Game.OnUpdate += Game_OnUpdate;
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

        private static void Game_OnUpdate(EventArgs args)
        {

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
            if (Inter && E.IsReady() && i.DangerLevel == DangerLevel.Medium && E.IsInRange(sender))
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
                if (ComboMenu["comboMode"].Cast<ComboBox>().CurrentValue == 0)
                {
                    if (Q.IsReady() && target.IsValidTarget(375))
                    {
                        Q.Cast();
                    }
                }

                if (useW && W.IsReady() && W.IsInRange(target))
                {
                    W.Cast(target.Position);
                }

                var pos = E.GetPrediction(target).CastPosition.Extend(Player.Instance.Position, -100);

                if (useE && E.IsReady() && E.IsInRange(target) && E2dis <= _Player.Position.Distance(target))
                {
                    E.Cast(pos.To3DWorld());
                }

                if (useR && Player.Instance.HealthPercent <= minR && target.IsValidTarget(R.Range))
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
            var target = TargetSelector.GetTarget(475, DamageType.Physical);
            if (target != null)
            {
                if (item && Bil.IsReady() && Bil.IsOwned() && target.IsValidTarget(475))
                {
                    Bil.Cast(target);
                }

                if ((item && Botrk.IsReady() && Botrk.IsOwned() && target.IsValidTarget(475)) && (Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp))
                {
                    Botrk.Cast(target);
                }
            }
        }

        public static void ResetAttack(AttackableUnit e, EventArgs args)
        {
            if (!(e is AIHeroClient)) return;
            var target = TargetSelector.GetTarget(300, DamageType.Physical);
            var champ = (AIHeroClient)e;
            var useriu = Items["hydra"].Cast<CheckBox>().CurrentValue;
            var HasQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var Hmana = HarassMenu["MHR"].Cast<Slider>().CurrentValue;
            if (champ == null || champ.Type != GameObjectType.AIHeroClient || !champ.IsValid) return;
            if (target != null)
            {
                if (ComboMenu["comboMode"].Cast<ComboBox>().CurrentValue == 1)
                {
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && Q.IsReady())
                    {
                        Q.Cast();
                        Orbwalker.ResetAutoAttack();
                        Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                    }
                }
			
                if (HasQ && Q.IsReady() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && Player.Instance.ManaPercent >= Hmana)
                {
                    Q.Cast();
                    Orbwalker.ResetAutoAttack();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }
			
                if ((useriu) && (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)))
                {
                    if (Hydra.IsOwned(Player.Instance) && Hydra.IsReady() && target.IsValidTarget(250))
                    {
                        Hydra.Cast();
                    }

                    if (Tiamat.IsOwned(Player.Instance) && Tiamat.IsReady() && target.IsValidTarget(250))
                    {
                        Tiamat.Cast();
                    }
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
                if (useQ && Q.IsReady() && minion.IsValidTarget(275) && minion.IsInAutoAttackRange(Player.Instance)
				&& Player.Instance.Distance(minion.ServerPosition) <= 225f
                && Player.Instance.GetSpellDamage(minion, SpellSlot.Q) + Player.Instance.GetAutoAttackDamage(minion)
                >= minion.TotalShieldHealth())
                {
                    Q.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                }

                if (useW && W.IsReady() && minion.IsValidTarget(W.Range) && minions.Count() >= 3)
                {
                    W.Cast(_Player.Position);
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
                if (useQ && Q.IsReady() && minion.IsValidTarget(275) && minion.IsInAutoAttackRange(Player.Instance)
				&& Player.Instance.Distance(minion.ServerPosition) <= 225f
                && Player.Instance.GetSpellDamage(minion, SpellSlot.Q) + Player.Instance.GetAutoAttackDamage(minion)
                >= minion.TotalShieldHealth())
                {
                    Q.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                }
            }
        }

        private static void Harass()
        {
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["MHR"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(700, DamageType.Physical);
            if (Player.Instance.ManaPercent < mana) return;
            if (target != null)
            {
                if (useW && W.IsReady() && W.IsInRange(target))
                {
                    W.Cast(target.Position);
                }

                var pos = E.GetPrediction(target).CastPosition.Extend(Player.Instance.Position, -100);
                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    E.Cast(pos.To3DWorld());
                }
            }
        }

        public static void JungleClear()
        {
            var useQ = JungleClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var useW = JungleClearMenu["WJungle"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["MJC"].Cast<Slider>().CurrentValue;
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(W.Range));
            if (Player.Instance.ManaPercent <= mana)
            {
                return;
            }

            if (monster != null)
            {
                if (useQ && Q.IsReady() && monster.IsValidTarget(300))
                {
                    Q.Cast();
                }

                if (useW && W.IsReady() && W.IsInRange(monster.Position))
                {
                    W.Cast(monster);
                }
            }
        }

        public static void Flee()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target != null)
            {
                var pos = E.GetPrediction(target).CastPosition.Extend(Player.Instance.Position, 100);

                if (E.IsReady() && target.IsValidTarget(E.Range))
                {
                    E.Cast(pos.To3DWorld());
                }
            }

            if (W.IsReady())
            {
                Player.CastSpell(SpellSlot.W, Player.Instance);
            }
        }

        private static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(475) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie))
            {
                if (KsQ && Q.IsReady() && target.IsValidTarget(250))
                {
                    if (target.Health <= Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                    {
                        Q.Cast();
                    }
                }

                if (Ignite != null && KillStealMenu["ign"].Cast<CheckBox>().CurrentValue && Ignite.IsReady())
                {
                    if (target.Health <= _Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite))
                    {
                        Ignite.Cast(target);
                    }
                }
            }
        }
    }
}
