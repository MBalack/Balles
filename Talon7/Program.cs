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

namespace Talon7
{
    internal class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, LaneClearMenu, JungleClearMenu, Misc, KillStealMenu, Items;
        public static Item Botrk;
        public static Item Hydra;
        public static Item Tiamat;
        public static Item Bil;
        public static Item Youmuu;
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
        public static Spell.Active Q;
        public static Spell.Skillshot W;
        public static Spell.Targeted E;
        public static Spell.Active R;
        public static Spell.Targeted Ignite;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Talon")) return;
            Chat.Print("Doctor's Talon Loaded!", Color.Orange);
            Bootstrap.Init(null);
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Skillshot(SpellSlot.W, 700, SkillShotType.Cone, 1, 2300, 80);
            W.AllowedCollisionCount = int.MaxValue;
            E = new Spell.Targeted(SpellSlot.E, 700);
            R = new Spell.Active(SpellSlot.R);
            Botrk = new Item(ItemId.Blade_of_the_Ruined_King);
            Tiamat = new Item(ItemId.Tiamat_Melee_Only, 400);
            Hydra = new Item(ItemId.Ravenous_Hydra_Melee_Only, 400);
            Bil = new Item(3144, 475f);
            Youmuu = new Item(3142, 10);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Menu = MainMenu.AddMenu("Talon", "Talon");
            Menu.AddSeparator();
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q]"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W]"));
            ComboMenu.Add("ComboR", new CheckBox("Always Use [R] On Combo"));
            ComboMenu.Add("riu", new CheckBox("Use [Hydra] Reset AA"));
            ComboMenu.AddGroupLabel("[E] Combo Settings");
            ComboMenu.Add("ComboE", new CheckBox("Use [E]"));
            ComboMenu.Add("myhp", new Slider("MyHP Use [E] >", 40, 0, 100));
            ComboMenu.Add("DisE", new Slider("Distance Use [E]", 300, 0, 700));
            ComboMenu.AddGroupLabel("Ultimate Settings");
            ComboMenu.Add("autor", new CheckBox("Use [R] Escape"));
            ComboMenu.Add("mau", new Slider("MyHP Use [R] Escape <", 20, 0, 100));
            ComboMenu.Add("rcount", new CheckBox("Use [R] Aoe" , false));
            ComboMenu.Add("cou", new Slider("Min Enemies Around Use [R] Aoe", 2, 1, 5));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q]", false));
            HarassMenu.Add("HarassW", new CheckBox("Use [W]"));
            HarassMenu.Add("ManaW", new Slider("Min Mana Harass", 40));

            LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("Lane Clear Settings");
            LaneClearMenu.Add("LaneQ", new CheckBox("Use [Q]", false));
            LaneClearMenu.Add("LaneW", new CheckBox("Use [W]"));
            LaneClearMenu.Add("ManaLC", new Slider("Min Mana LaneClear", 60));
            LaneClearMenu.AddSeparator();
            LaneClearMenu.AddGroupLabel("Lasthit Settings");
            LaneClearMenu.Add("LastW", new CheckBox("Use [W] Lasthit"));
            LaneClearMenu.Add("LastQ", new CheckBox("Use [Q] Lasthit", false));
            LaneClearMenu.Add("LhMana", new Slider("Min Mana LaneClear", 60));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("QJungle", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("WJungle", new CheckBox("Use [W] JungleClear"));
            JungleClearMenu.Add("MnJungle", new Slider("Min Mana JungleClear [Q]", 30));

            Misc = Menu.AddSubMenu("Misc Settings", "Misc");
            Misc.AddGroupLabel("AntiGap Settings");
            Misc.Add("AntiGap", new CheckBox("Use [W] AntiGapcloser"));
            Misc.Add("Rstun", new CheckBox("Use [W] Immobile"));
            Misc.AddSeparator();
            Misc.AddGroupLabel("Drawing Settings");
            Misc.Add("DrawW", new CheckBox("W Range"));
            Misc.Add("DrawE", new CheckBox("E Range", false));
            Misc.AddSeparator();
            Misc.AddGroupLabel("Skin Changer");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer"));
            Misc.Add("skin.Id", new ComboBox("Skin Mode", 4, "Default", "1", "2", "3", "4"));

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
            KillStealMenu.Add("KsW", new CheckBox("Use [W] KillSteal"));
            KillStealMenu.Add("KsR", new CheckBox("Use [R] KillSteal"));
            KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));

            Items = Menu.AddSubMenu("Items Settings", "Items");
            Items.AddGroupLabel("Items Settings");
            Items.Add("you", new CheckBox("Use [Youmuu]"));
            Items.Add("BOTRK", new CheckBox("Use [BOTRK]"));
            Items.Add("ihp", new Slider("My HP Use BOTRK <=", 50));
            Items.Add("ihpp", new Slider("Enemy HP Use BOTRK <=", 50));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnTick += Game_OnTick;
            Orbwalker.OnPostAttack += ResetAttack;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Misc["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (Misc["DrawE"].Cast<CheckBox>().CurrentValue)
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
            AutoR();
            Item();
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

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (Misc["AntiGap"].Cast<CheckBox>().CurrentValue && sender.IsEnemy && e.Sender.Distance(_Player) < 300)
            {
                W.Cast(e.Sender);
            }
        }

        public static void Combo()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var disE = ComboMenu["DisE"].Cast<Slider>().CurrentValue;
            var hp = ComboMenu["myhp"].Cast<Slider>().CurrentValue;
            if (target != null)
            {
                if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    if (_Player.Distance(target) < Player.Instance.GetAutoAttackRange(target) && !Q.IsReady())
                    {
                        W.Cast(target);
                    }
                    else if (_Player.Distance(target) > Player.Instance.GetAutoAttackRange(target))
                    {
                        W.Cast(target);
                    }
                }
                if (useE && E.IsReady() && E.IsInRange(target) && disE <= target.Distance(Player.Instance) && Player.Instance.HealthPercent >= hp)
                {
                    E.Cast(target);
                }
                if (useR && R.IsReady() && !Q.IsReady() && target.IsValidTarget(325))
                {
                    R.Cast();
                }
            }
        }

        public static void AutoR()
        {
            var useR = ComboMenu["autor"].Cast<CheckBox>().CurrentValue;
            var mau = ComboMenu["mau"].Cast<Slider>().CurrentValue;
            var Rcount = ComboMenu["rcount"].Cast<CheckBox>().CurrentValue;
            var MinR = ComboMenu["cou"].Cast<Slider>().CurrentValue;
            if (useR && R.IsReady())
            {
                if (Player.Instance.HealthPercent < mau && _Player.Position.CountEnemiesInRange(500) >= 1)
                {
                    R.Cast();
                }
            }
            if (Rcount && R.IsReady() && _Player.Position.CountEnemiesInRange(400) >= MinR)
            {
                R.Cast();
            }
        }

        public static void JungleClear()
        {
            var useW = JungleClearMenu["WJungle"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["MnJungle"].Cast<Slider>().CurrentValue;
            var jungleMonsters = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(W.Range));
            if (Player.Instance.ManaPercent < mana) return;
            if (jungleMonsters != null)
            {
                if (useW && W.IsReady() && jungleMonsters.IsValidTarget(500))
                {
                    W.Cast(jungleMonsters);
                }
            }
        }

        public static void LaneClear()
        {
            var mana = LaneClearMenu["ManaLC"].Cast<Slider>().CurrentValue;
            var useW = LaneClearMenu["LaneW"].Cast<CheckBox>().CurrentValue;
            var useQ = LaneClearMenu["LaneQ"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            if (Player.Instance.ManaPercent < mana) return;
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(300))
                {
                    Q.Cast();
                }
                if (useW && W.IsReady() && minion.IsValidTarget(W.Range))
                {
                    W.Cast(minion);
                }
            }
        }

        public static void Item()
        {

            var item = Items["BOTRK"].Cast<CheckBox>().CurrentValue;
            var yous = Items["you"].Cast<CheckBox>().CurrentValue;
            var Minhp = Items["ihp"].Cast<Slider>().CurrentValue;
            var Minhpp = Items["ihpp"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (target != null)
            {
                if (item && Bil.IsReady() && Bil.IsOwned() && target.IsValidTarget(450))
                {
                    Bil.Cast(target);
                }
                if ((item && Botrk.IsReady() && Botrk.IsOwned() && target.IsValidTarget(450)) && (Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp))
                {
                    Botrk.Cast(target);
                }
                if (yous && Youmuu.IsReady() && Youmuu.IsOwned() && target.IsValidTarget(550) && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    Youmuu.Cast();
                }
            }
        }

        public static void Harass()
        {
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var ManaW = HarassMenu["ManaW"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (Player.Instance.ManaPercent < ManaW) return;
            if (target != null)
            {
                if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    if (_Player.Distance(target) < Player.Instance.GetAutoAttackRange(target) && !Q.IsReady())
                    {
                        W.Cast(target);
                    }
                    else if (_Player.Distance(target) > Player.Instance.GetAutoAttackRange(target))
                    {
                        W.Cast(target);
                    }
                }
            }
        }

        public static void RStun()
        {
            var Rstun = Misc["Rstun"].Cast<CheckBox>().CurrentValue;
            if (Rstun && W.IsReady())
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                if (target != null)
                {
                    if (target.IsRooted || target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup))
                    {
                        W.Cast(target.Position);
                    }
                }
            }
        }

        private static void ResetAttack(AttackableUnit target, EventArgs args)
        {
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useriu = ComboMenu["riu"].Cast<CheckBox>().CurrentValue;
            var jungQ = JungleClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["MnJungle"].Cast<Slider>().CurrentValue;
            var HasQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var ManaW = HarassMenu["ManaW"].Cast<Slider>().CurrentValue;

            if (useQ && Q.IsReady() && target.IsValidTarget(300) && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Q.Cast();
                Orbwalker.ResetAutoAttack();
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
            if (jungQ && Q.IsReady() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) && Player.Instance.ManaPercent > mana)
            {
                Q.Cast();
                Orbwalker.ResetAutoAttack();
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
            if (HasQ && Q.IsReady() && target.IsValidTarget(300) && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && Player.Instance.ManaPercent > ManaW)
            {
                Q.Cast();
                Orbwalker.ResetAutoAttack();
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
            if ((useriu && !Q.IsReady()) && (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)))
            {
                if (Hydra.IsOwned() && Hydra.IsReady() && target.IsValidTarget(325))
                {
                    Hydra.Cast();
                }

                if (Tiamat.IsOwned() && Tiamat.IsReady() && target.IsValidTarget(325))
                {
                    Tiamat.Cast();
                }
            }
        }

        public static void LastHit()
        {
            var useW = LaneClearMenu["LastW"].Cast<CheckBox>().CurrentValue;
            var useQ = LaneClearMenu["LastQ"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["LhMana"].Cast<Slider>().CurrentValue;
            var minion = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(m => m.IsValidTarget(Q.Range) && (Player.Instance.GetSpellDamage(m, SpellSlot.Q) >= m.TotalShieldHealth()));
            var minions = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(m => m.IsValidTarget(W.Range) && (Player.Instance.GetSpellDamage(m, SpellSlot.W) >= m.TotalShieldHealth()));
            if (Player.Instance.ManaPercent > mana)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(200))
                {
                    Q.Cast();
                }

                if (useW && W.IsReady() && minions.IsValidTarget(W.Range))
                {
                    W.Cast(minions);
                }
            }
        }

        public static float WDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 0, 60, 110, 160, 210, 260 }[Program.R.Level] + 0.6f * _Player.FlatPhysicalDamageMod));
        }
        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 0, 240, 340, 440 }[Program.R.Level] + 0.75f * _Player.FlatPhysicalDamageMod));
        }

        public static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var KsW = KillStealMenu["KsW"].Cast<CheckBox>().CurrentValue;
            var KsR = KillStealMenu["KsR"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(W.Range) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie))
            {
                if (KsW && W.IsReady())
                {
                    if (target != null)
                    {
                        if (target.Health + target.AttackShield < WDamage(target))
                        {
                            W.Cast(target);
                        }
                    }
                }

                if (KsR && R.IsReady() && target.IsValidTarget(500))
                {
                    if (target != null)
                    {
                        if (target.Health + target.AttackShield < RDamage(target))
                        {
                            R.Cast();
                        }
                    }
                }

                if (Ignite != null && KillStealMenu["ign"].Cast<CheckBox>().CurrentValue && Ignite.IsReady())
                {
                    if (target.Health + target.AttackShield < _Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite))
                    {
                        Ignite.Cast(target);
                    }
                }
            }
        }
    }
}
