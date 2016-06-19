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

namespace XinZhao7
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
        public static Spell.Targeted E;
        public static Spell.Active R;
        public static Spell.Targeted Ignite;
        public static Item Hydra;
        public static Item Tiamat;
        public static Item Titanic;
        public static Item Botrk;
        public static Item Bil;
        public const float YOff = 10;
        public const float XOff = 0;
        public const float Width = 107;
        public const float Thick = 9;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("XinZhao")) return;
            Chat.Print("Xinzhao7 Loaded!", Color.Orange);
            Bootstrap.Init(null);
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Targeted(SpellSlot.E, 600);
            R = new Spell.Active(SpellSlot.R, 500);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Tiamat = new Item( ItemId.Tiamat_Melee_Only, 400);
            Hydra = new Item( ItemId.Ravenous_Hydra_Melee_Only, 400);
            Titanic = new Item( ItemId.Titanic_Hydra, Player.Instance.GetAutoAttackRange());
            Botrk = new Item(ItemId.Blade_of_the_Ruined_King);
            Bil = new Item(3144, 475f);
            Menu = MainMenu.AddMenu("Xinzhao7", "Xinzhao");
            Menu.AddSeparator();
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddSeparator();
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            ComboMenu.Add("DisE", new Slider("Distance Use [E]", 300, 0, 600));
            ComboMenu.AddGroupLabel("Items Settings");
            ComboMenu.Add("hydra", new CheckBox("Use [Hydra] Reset AA"));
            ComboMenu.Add("titanic", new CheckBox("Use [Titanic]"));
            ComboMenu.Add("BOTRK", new CheckBox("Use [Botrk]"));
            ComboMenu.Add("ihp", new Slider("My HP Use BOTRK", 50));
            ComboMenu.Add("ihpp", new Slider("Enemy HP Use BOTRK", 50));

            Ulti = Menu.AddSubMenu("Ultimate Settings", "Ulti");
            Ulti.AddGroupLabel("Ultimate Enemies In Count");
            Ulti.Add("ultiR", new CheckBox("Use [R] Enemies In Range"));
            Ulti.Add("MinR", new Slider("Min Enemies Use [R]", 2, 1, 5));
            Ulti.AddGroupLabel("Ultimate My HP");
            Ulti.Add("ultiR2", new CheckBox("Use [R] If My HP"));
            Ulti.Add("MauR", new Slider("My HP Use [R]", 40));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HarassW", new CheckBox("Use [W] Harass", false) );
            HarassMenu.Add("ManaHR", new Slider("Mana For Harass", 40));

            LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("Lane Clear Settings");
            LaneClearMenu.Add("QLC", new CheckBox("Use [Q] LaneClear", false));
            LaneClearMenu.Add("WLC", new CheckBox("Use [W] LaneClear", false));
            LaneClearMenu.Add("ManaLC", new Slider("Mana For LaneClear", 50));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("QJungle", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("WJungle", new CheckBox("Use [W] JungleClear"));
            JungleClearMenu.Add("EJungle", new CheckBox("Use [E] JungleClear"));
            JungleClearMenu.Add("itemJC", new CheckBox("Use [Items] JungleClear"));
            JungleClearMenu.Add("ManaJC", new Slider("Mana For JungleClear", 30));

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsR", new CheckBox("Use [R] KillSteal"));
            KillStealMenu.Add("KsE", new CheckBox("Use [E] KillSteal", false));
            KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));

            Misc = Menu.AddSubMenu("Misc Settings", "Misc");
            Misc.AddGroupLabel("Skin Settings");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Misc.Add("skin.Id", new ComboBox("Skin Mode", 0, "Default", "1", "2", "3", "4", "5", "6"));
            Misc.AddGroupLabel("Drawing Settings");
            Misc.Add("DrawR", new CheckBox("R Range"));
            Misc.Add("DrawE", new CheckBox("E Range"));
            Misc.Add("Damage", new CheckBox("Damage Indicator [R]"));
            Misc.AddGroupLabel("Interrupt Settings");
            Misc.Add("inter", new CheckBox("Use [R] Interupt"));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnTick += Game_OnTick;
            Orbwalker.OnPostAttack += ResetAttack;
            Interrupter.OnInterruptableSpell += Interupt;
            Drawing.OnEndScene += Damage;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Misc["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Black, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
            if (Misc["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Black, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
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

        private static void Damage(EventArgs args)
        {
            foreach (
                var enemy in
                    EntityManager.Heroes.Enemies.Where(e => e.IsValid && e.IsHPBarRendered && e.TotalShieldHealth() > 10)
                )
            {
                var damage = RDamage(enemy);

                if (Misc["Damage"].Cast<CheckBox>().CurrentValue && R.IsReady())
                {
                    var dmgPer = (enemy.TotalShieldHealth() - damage > 0 ? enemy.TotalShieldHealth() - damage : 0) /
                                 enemy.TotalShieldMaxHealth();
                    var currentHPPer = enemy.TotalShieldHealth() / enemy.TotalShieldMaxHealth();
                    var initPoint = new Vector2((int)(enemy.HPBarPosition.X + XOff + dmgPer * Width),
                        (int)enemy.HPBarPosition.Y + YOff);
                    var endPoint = new Vector2((int)(enemy.HPBarPosition.X + XOff + currentHPPer * Width) + 1,
                        (int)enemy.HPBarPosition.Y + YOff);

                    EloBuddy.SDK.Rendering.Line.DrawLine(System.Drawing.Color.Aqua, Thick, initPoint, endPoint);
                }
            }
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 0, 75, 175, 275 }[Program.R.Level] + 1.0f * _Player.FlatPhysicalDamageMod));
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var disE = ComboMenu["DisE"].Cast<Slider>().CurrentValue;
            var item = ComboMenu["BOTRK"].Cast<CheckBox>().CurrentValue;
            var Minhp = ComboMenu["ihp"].Cast<Slider>().CurrentValue;
            var Minhpp = ComboMenu["ihpp"].Cast<Slider>().CurrentValue;
            if (target != null)
            {
                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && disE <= target.Distance(Player.Instance))
                {
                    E.Cast(target);
                }
                if (useQ && Q.IsReady() && target.IsValidTarget(_Player.AttackRange) && !target.IsDead && !target.IsZombie)
                {
                    Q.Cast();
                }
                if (useW && W.IsReady() && target.IsValidTarget(_Player.AttackRange) && !target.IsDead && !target.IsZombie)
                {
                    W.Cast();
                }
                if (item && Bil.IsReady() && Bil.IsOwned() && target.IsValidTarget(550))
                {
                    Bil.Cast(target);
                }
                if (item && Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp && Botrk.IsReady() && Botrk.IsOwned() && target.IsValidTarget(550))
                {
                    Botrk.Cast(target);
                }
            }
        }

        private static void Ultimate()
        {
            var useR = Ulti["ultiR"].Cast<CheckBox>().CurrentValue;
            var minR = Ulti["MinR"].Cast<Slider>().CurrentValue;
            var useR2 = Ulti["ultiR2"].Cast<CheckBox>().CurrentValue;
            var mauR = Ulti["MauR"].Cast<Slider>().CurrentValue;
            if (useR && !Player.Instance.IsInShopRange() && _Player.Position.CountEnemiesInRange(R.Range) >= minR)
            {
                R.Cast();
            }
            if (useR2 && _Player.HealthPercent <= mauR && _Player.Position.CountEnemiesInRange(R.Range) >= 1 && !Player.Instance.IsInShopRange())
            {
                R.Cast();
            }
        }

        private static void ResetAttack(AttackableUnit target, EventArgs args)
        {
            var Titan = ComboMenu["titanic"].Cast<CheckBox>().CurrentValue;
            var useriu = ComboMenu["hydra"].Cast<CheckBox>().CurrentValue;
            if (target != null)
            {
                if (useriu && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
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
            }
        }

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["QLC"].Cast<CheckBox>().CurrentValue;
            var useW = LaneClearMenu["WLC"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["ManaLC"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            if (Player.Instance.ManaPercent < mana) return;
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(_Player.AttackRange) && minions.Count() >= 3)
                {
                    Q.Cast();
                }
                if (useW && W.IsReady() && minion.IsValidTarget(_Player.AttackRange))
                {
                    W.Cast();
                }
            }
        }

        private static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["ManaHR"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(_Player.AttackRange, DamageType.Physical);
            if (target != null && Player.Instance.ManaPercent >= mana)
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(_Player.AttackRange) && !target.IsDead && !target.IsZombie)
                {
                    Q.Cast();
                }
                if (useW && W.IsReady() && target.IsValidTarget(_Player.AttackRange) && !target.IsDead && !target.IsZombie)
                {
                    W.Cast();
                }
            }
        }

        public static void JungleClear()
        {

            var useQ = JungleClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var useW = JungleClearMenu["WJungle"].Cast<CheckBox>().CurrentValue;
            var useE = JungleClearMenu["EJungle"].Cast<CheckBox>().CurrentValue;
            var useriu = JungleClearMenu["itemJC"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["ManaJC"].Cast<Slider>().CurrentValue;
            var jungleMonsters = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(Q.Range));
            if (jungleMonsters != null && Player.Instance.ManaPercent >= mana)
            {
                if (useQ && Q.IsReady() && jungleMonsters.IsValidTarget(_Player.AttackRange))
                {
                    Q.Cast();
                }
                if (useW && W.IsReady() && jungleMonsters.IsValidTarget(_Player.AttackRange))
                {
                    W.Cast();
                }
                if (useE && E.IsReady() && jungleMonsters.IsValidTarget(E.Range))
                {
                    E.Cast(jungleMonsters);
                }
                if (useriu && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                {
                    if (Hydra.IsOwned() && Hydra.IsReady() && jungleMonsters.IsValidTarget(_Player.AttackRange))
                    {
                        Hydra.Cast();
                    }

                    if (Tiamat.IsOwned() && Tiamat.IsReady() && jungleMonsters.IsValidTarget(_Player.AttackRange))
                    {
                        Tiamat.Cast();
                    }
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
            if (Inter && R.IsReady() && i.DangerLevel == DangerLevel.High && R.IsInRange(sender))
            {
                R.Cast();
            }
        }

        public static void Flee()
        {
            if (E.IsReady())
            {
                var CursorPos = Game.CursorPos;
                Obj_AI_Base fl = EntityManager.MinionsAndMonsters.Minions.FirstOrDefault(w => w.Distance(CursorPos) <= 250 && E.IsInRange(w));
                if (fl != default(Obj_AI_Base))
                {
                    E.Cast(fl);
                }
                else
                {
                    fl = EntityManager.Heroes.Enemies.FirstOrDefault(w => w.Distance(CursorPos) <= 250 && E.IsInRange(w));

                    if (fl != default(Obj_AI_Base))
                    {
                        E.Cast(fl);
                    }
                }
            }
        }

        private static void KillSteal()
        {
            var KsE = KillStealMenu["KsE"].Cast<CheckBox>().CurrentValue;
            var KsR = KillStealMenu["KsR"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(1200) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie && (hero.HealthPercent <= 25)))
            {
                if (KsE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.E))
                    {
                        E.Cast(target);
                    }
                }

                if (KsR && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    if (target.Health + target.AttackShield < RDamage(target))
                    {
                        R.Cast();
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
