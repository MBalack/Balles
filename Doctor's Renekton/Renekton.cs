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

namespace Renekton7
{
    internal class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, Ulti, LaneClearMenu, JungleClearMenu, KillStealMenu, Misc;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell.Active Q;
        public static Spell.Active W;
        public static Spell.Skillshot E;
        public static Spell.Active R;
        public static Spell.Targeted Ignite;
        public static Item Hydra;
        public static Item Tiamat;
        public static Item Titanic;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Renekton")) return;
            Chat.Print("Doctor's Renekton Loaded!", Color.Orange);
            Q = new Spell.Active(SpellSlot.Q, 325);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Skillshot(SpellSlot.E, 450, SkillShotType.Linear);
            R = new Spell.Active(SpellSlot.R);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Tiamat = new Item(ItemId.Tiamat_Melee_Only, 400);
            Hydra = new Item(ItemId.Ravenous_Hydra_Melee_Only, 400);
            Titanic = new Item(ItemId.Titanic_Hydra, Player.Instance.GetAutoAttackRange());

            Menu = MainMenu.AddMenu("Doctor's Renekton", "Renekton");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.AddGroupLabel("Combo [E] Settings");
            ComboMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            ComboMenu.Add("ComboE2", new CheckBox("Use [E2] Combo"));
            ComboMenu.Add("Edis", new Slider("Use [E2] If Enemy Distance >", 250, 0, 450));
            ComboMenu.AddGroupLabel("Items Settings");
            ComboMenu.Add("hydra", new CheckBox("Use [Hydra] Reset AA"));

            Ulti = Menu.AddSubMenu("Ultimate Settings", "Ulti");
            Ulti.AddGroupLabel("Ultimate Health Settings");
            Ulti.Add("ultiR", new CheckBox("Use [R] My Health"));
            Ulti.Add("MinR", new Slider("Min Health Use [R]", 50));
            Ulti.AddGroupLabel("Ultimate Enemies Count");
            Ulti.Add("ultiR2", new CheckBox("Use [R] Enemies In Range", false));
            Ulti.Add("MinE", new Slider("Min Enemies Use [R]", 3, 1, 5));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HarassW", new CheckBox("Use [W] Harass"));

            LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("Lane Clear Settings");
            LaneClearMenu.Add("QLC", new CheckBox("Use [Q] LaneClear"));
            LaneClearMenu.Add("WLC", new CheckBox("Use [W] LaneClear", false));
            LaneClearMenu.AddSeparator();
            LaneClearMenu.AddGroupLabel("LastHit Settings");
            LaneClearMenu.Add("LHQ", new CheckBox("Use [Q] LastHit", false));
            LaneClearMenu.Add("LHW", new CheckBox("Use [W] LastHit", false));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("QJungle", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("WJungle", new CheckBox("Use [W] JungleClear"));
            JungleClearMenu.Add("EJungle", new CheckBox("Use [E] JungleClear"));

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
            KillStealMenu.Add("KsW", new CheckBox("Use [W] KillSteal"));
            KillStealMenu.Add("KsE", new CheckBox("Use [E] KillSteal"));
            KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));

            Misc = Menu.AddSubMenu("Misc Settings", "Misc");
            Misc.AddGroupLabel("Skin Settings");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer"));
            Misc.Add("skin.Id", new ComboBox("Skin Mode", 0, "Default", "1", "2", "3", "4", "5", "6", "7"));
            Misc.AddGroupLabel("Drawing Settings");
            Misc.Add("DrawQ", new CheckBox("Q Range"));
            Misc.Add("DrawE", new CheckBox("E Range", false));
            Misc.Add("DrawE2", new CheckBox("Drawings Distance Use E2 If Distance Target >"));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnTick += Game_OnTick;
            Orbwalker.OnPostAttack += ResetAttack;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var E2dis = ComboMenu["Edis"].Cast<Slider>().CurrentValue;
            if (Misc["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }

            if (Misc["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }

            if (Misc["DrawE2"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = E2dis }.Draw(_Player.Position);
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
            Ultimate();
            if (_Player.SkinId != Misc["skin.Id"].Cast<ComboBox>().CurrentValue)
            {
                if (checkSkin())
                {
                    Player.SetSkinId(SkinId());
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

        public static bool PassiveW
        {
            get { return Player.Instance.HasBuff("renektonpreexecute"); }
        }

        public static bool PassiveE
        {
            get { return Player.Instance.HasBuff("RenekthonSliceAndDiceDelay"); }
        }

        public static bool Fury
        {
            get { return Player.Instance.HasBuff("renektonrageready"); }
        }

        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 0, 60, 90, 120, 150, 190 }[Q.Level] + 0.8f * _Player.FlatPhysicalDamageMod));
        }

        public static float WDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 0, 5, 15, 25, 35, 45 }[W.Level] + 0.75f * _Player.FlatPhysicalDamageMod));
        }

        public static float EDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 0, 30, 60, 90, 120, 150 }[E.Level] + 0.9f * _Player.FlatPhysicalDamageMod));
        }

        public static float GetDamage(AIHeroClient target)
        {
            if (target != null)
            {
                float Damage = 0;

                if (Q.IsReady()) { Damage += QDamage(target); }
                if (E.IsReady()) { Damage += EDamage(target); }
                if (W.IsReady()) { Damage += WDamage(target); }

                return Damage;
            }
            return 0;
        }

        private static void Combo()
        {
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var useE2 = ComboMenu["ComboE2"].Cast<CheckBox>().CurrentValue;
            var E2dis = ComboMenu["Edis"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q.Range) && !e.IsDead))
            {
                if (useQ && Q.IsReady() && !PassiveW && target.IsValidTarget(Q.Range))
                {
                    Q.Cast();
                }

                if (!PassiveE && useE && E.IsReady() && target.IsValidTarget(E.Range) && (200 <= target.Distance(Player.Instance) || !Q.IsReady() && !W.IsReady()))
                {
                    E.Cast(target.Position);
                }
				
                if (useE2 && E.IsReady() && target.IsValidTarget(E.Range) && PassiveE && E2dis <= target.Distance(Player.Instance))
                {
                    E.Cast(target.Position);
                }
            }
        }

        private static void Ultimate()
        {
            var useR = Ulti["ultiR"].Cast<CheckBox>().CurrentValue;
            var useR2 = Ulti["ultiR2"].Cast<CheckBox>().CurrentValue;
            var minR = Ulti["MinR"].Cast<Slider>().CurrentValue;
            var minE = Ulti["MinE"].Cast<Slider>().CurrentValue;
            if (useR && _Player.HealthPercent <= minR && _Player.Position.CountEnemiesInRange(500) >= 1 && !Player.Instance.IsInShopRange())
            {
                R.Cast();
            }
			
            if (useR2 && !Player.Instance.IsInShopRange() && _Player.Position.CountEnemiesInRange(450) >= minE)
            {
                R.Cast();
            }
        }

        public static void ResetAttack(AttackableUnit e, EventArgs args)
        {
            if (!(e is AIHeroClient)) return;
            var target = TargetSelector.GetTarget(300, DamageType.Physical);
            var champ = (AIHeroClient)e;
            var useriu = ComboMenu["hydra"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var HasW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            if (champ == null || champ.Type != GameObjectType.AIHeroClient || !champ.IsValid) return;
            if (target != null)
            {
                if (useW && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    W.Cast();
                    Orbwalker.ResetAutoAttack();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }
			
                if (HasW && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                {
                    W.Cast();
                    Orbwalker.ResetAutoAttack();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }

                if ((useriu && !W.IsReady() && !PassiveW) && (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)))
                {
                    if (Hydra.IsOwned(Player.Instance) && Hydra.IsReady() && target.IsValidTarget(250))
                    {
                        Hydra.Cast();
                    }

                    if (Tiamat.IsOwned(Player.Instance) && Tiamat.IsReady() && target.IsValidTarget(250))
                    {
                        Tiamat.Cast();
                    }

                    if (Titanic.IsOwned(Player.Instance) && Titanic.IsReady() && target.IsValidTarget(250))
                    {
                        Titanic.Cast();
                    }
                }
            }
        }

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["QLC"].Cast<CheckBox>().CurrentValue;
            var useW = LaneClearMenu["WLC"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && _Player.CountEnemyMinionsInRange(Q.Range) >= 2)
                {
                    Q.Cast();
                }

                if (useW && W.IsReady() && minion.IsValidTarget(275) && minion.IsInAutoAttackRange(Player.Instance)
                && Player.Instance.Distance(minion.ServerPosition) <= 225f
                && WDamage(minion) * 2 + Player.Instance.GetAutoAttackDamage(minion)
                >= minion.TotalShieldHealth())
                {
                    W.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                }
            }
        }

        public static void LastHit()
        {
            var useQ = LaneClearMenu["LHQ"].Cast<CheckBox>().CurrentValue;
            var useW = LaneClearMenu["LHW"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && Q.IsInRange(minion) && QDamage(minion) >= minion.TotalShieldHealth())
                {
                    Q.Cast();
                }

                if (useW && W.IsReady() && minion.IsValidTarget(275) && minion.IsInAutoAttackRange(Player.Instance)
                && Player.Instance.Distance(minion.ServerPosition) <= 225f
                && WDamage(minion) * 2 + Player.Instance.GetAutoAttackDamage(minion)
                >= minion.TotalShieldHealth())
                {
                    W.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                }
            }
        }

        private static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q.Range) && !e.IsDead))
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.Cast();
                }
            }
        }

        public static void JungleClear()
        {
            var useW = JungleClearMenu["WJungle"].Cast<CheckBox>().CurrentValue;
            var useQ = JungleClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var useE = JungleClearMenu["EJungle"].Cast<CheckBox>().CurrentValue;
            var jungleMonsters = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(Q.Range));
            if (jungleMonsters != null)
            {
                if (useQ && Q.IsReady() && Q.IsInRange(jungleMonsters))
                {
                    Q.Cast();
                }

                if (useW && W.IsReady() && jungleMonsters.IsValidTarget(275) && jungleMonsters.IsInAutoAttackRange(Player.Instance) && Player.Instance.Distance(jungleMonsters.ServerPosition) <= 225f)
                {
                    W.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, jungleMonsters);
                }

                if (useE && E.IsReady())
                {
                    if (!PassiveE)
                    {
                        E.Cast(jungleMonsters.Position);
                    }

                    if (PassiveE && jungleMonsters.IsValidTarget(E.Range) && !Q.IsReady() && !W.IsReady())
                    {
                        E.Cast(jungleMonsters.Position);
                    }
                }
            }
        }

        public static void Flee()
        {
            if (E.IsReady())
            {
                var cursorPos = Game.CursorPos;
                var castPos = Player.Instance.Position.Distance(cursorPos) <= E.Range ? cursorPos : Player.Instance.Position.Extend(cursorPos, E.Range).To3D();
                E.Cast(castPos);
            }

        }

        private static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var KsW = KillStealMenu["KsW"].Cast<CheckBox>().CurrentValue;
            var KsE = KillStealMenu["KsE"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(1200) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie))
            {
                if (KsQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (Fury)
                    {
                        if (target.Health + target.AttackShield <= QDamage(target) * 0.5)
                        {
                            Q.Cast();
                        }
                    }
                    else
                    {
                        if (target.Health + target.AttackShield <= QDamage(target))
                        {
                            Q.Cast();
                        }
                    }
                }

                if (KsW && W.IsReady() && target.IsValidTarget(250))
                {
                    if (Fury)
                    {
                        if (target.Health + target.AttackShield <= WDamage(target) * 3)
                        {
                            W.Cast();
                        }
                    }
                    else
                    {
                        if (target.Health + target.AttackShield <= WDamage(target) * 2)
                        {
                            W.Cast();
                        }
                    }
                }

                if (KsE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (target.Health + target.AttackShield <= EDamage(target))
                    {
                        E.Cast(target.Position);
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
