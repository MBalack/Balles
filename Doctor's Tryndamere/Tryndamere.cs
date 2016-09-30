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
using Font = SharpDX.Direct3D9.Font;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace Tryndamere
{
    static class Program
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
        public static Font thm;
        public static Font thn;
        public static Item Hydra;
        public static Item Tiamat;
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
            if (!_Player.ChampionName.Contains("Tryndamere")) return;
            Chat.Print("Doctor's Tryndamere Loaded!", Color.Orange);
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Active(SpellSlot.W, 800);
            E = new Spell.Skillshot(SpellSlot.E, 660, SkillShotType.Linear, 250, 700, (int)92.5);
            E.AllowedCollisionCount = int.MaxValue;
            R = new Spell.Active(SpellSlot.R, 500);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 15, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            thn = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 22, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Tiamat = new Item(ItemId.Tiamat_Melee_Only, 400);
            Hydra = new Item(ItemId.Ravenous_Hydra_Melee_Only, 400);
            Botrk = new Item(ItemId.Blade_of_the_Ruined_King);
            Bil = new Item(3144, 475f);
            Menu = MainMenu.AddMenu("Doctor's Tryndamere", "Tryndamere");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("ComboW2", new CheckBox("Only Use [W] If [E] Is CoolDown", false));
            ComboMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            ComboMenu.Add("DisE", new Slider("Use [E] If Enemy Distance >", 300, 0, 600));
            ComboMenu.Add("CTurret", new KeyBind("Don't Use [E] UnderTurret", false, KeyBind.BindTypes.PressToggle, 'T'));
            ComboMenu.AddGroupLabel("Items Settings");
            ComboMenu.Add("hydra", new CheckBox("Use [Hydra] Reset AA"));
            ComboMenu.Add("BOTRK", new CheckBox("Use [Botrk]"));
            ComboMenu.Add("ihp", new Slider("My HP Use BOTRK", 50));
            ComboMenu.Add("ihpp", new Slider("Enemy HP Use BOTRK", 50));

            Ulti = Menu.AddSubMenu("Q/R Settings", "Ulti");
            Ulti.AddGroupLabel("Use [R] Low Hp");
            Ulti.Add("ultiR", new CheckBox("Use [R] No Die"));
            Ulti.Add("MauR", new Slider("My HP Use [R] <=", 15));
            Ulti.AddGroupLabel("Use [Q] Low Hp");
            Ulti.Add("Q", new CheckBox("Use [Q]"));
            Ulti.Add("Q2", new CheckBox("Only Use [Q] If [R] Is CoolDown"));
            Ulti.Add("QHp", new Slider("My HP Use [Q] <=", 30));
            Ulti.Add("Ps", new Slider("Min Fury Use [Q]", 5));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassW", new CheckBox("Use [W] Harass"));
            HarassMenu.Add("HarassW2", new CheckBox("Only Use [W] If [E] Is CoolDown"));
            HarassMenu.Add("HarassE", new CheckBox("Use [E] Harass"));
            HarassMenu.Add("DistanceE", new Slider("Use [E] If Enemy Distance >", 300, 0, 600));
            HarassMenu.Add("HTurret", new CheckBox("Don't Use [E] UnderTurret"));

            LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("Lane Clear Settings");
            LaneClearMenu.Add("E", new CheckBox("Use [E] LaneClear", false));
            LaneClearMenu.Add("mine", new Slider("Min Hit Minions Use [E]", 2, 1, 6));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("EJungle", new CheckBox("Use [E] JungleClear"));

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsE", new CheckBox("Use [E] KillSteal", false));
            KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));

            Misc = Menu.AddSubMenu("Misc Settings", "Misc");
            Misc.AddGroupLabel("Skin Settings");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Misc.Add("skin.Id", new ComboBox("Skin Mode", 0, "Default", "1", "2", "3", "4", "5", "6", "7", "8"));
            Misc.AddGroupLabel("Drawing Settings");
            Misc.Add("DrawE", new CheckBox("E Range"));
            Misc.Add("DrawW", new CheckBox("W Range"));
            Misc.Add("Damage", new CheckBox("Damage Indicator"));
            Misc.Add("DrawTR", new CheckBox("Draw Text Under Turret"));
            Misc.Add("DrawTime", new CheckBox("Draw Time [R]"));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Orbwalker.OnPostAttack += ResetAttack;
            Drawing.OnEndScene += Damage;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Misc["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }

            if (Misc["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Red, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }

            if (Misc["DrawTR"].Cast<CheckBox>().CurrentValue)
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue)
                {
                    DrawFont(thm, "Use E Under Turret : Disable", (float)(ft[0] - 70), (float)(ft[1] + 50), SharpDX.Color.White);
                }
                else
                {
                    DrawFont(thm, "Use E Under Turret : Enable", (float)(ft[0] - 70), (float)(ft[1] + 50), SharpDX.Color.Red);
                }
            }

            if (Misc["DrawTime"].Cast<CheckBox>().CurrentValue)
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (Player.Instance.HasBuff("Undying Rage"))
                {
                    DrawFont(thn, "Undying Rage : " + RTime(Player.Instance), (float)(ft[0] - 125), (float)(ft[1] + 100), SharpDX.Color.Orange);
                }
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
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(e => e.IsValid && e.IsHPBarRendered && e.TotalShieldHealth() > 10))
            {
                var damage = Player.Instance.GetAutoAttackDamage(enemy) * 2;

                if (Misc["Damage"].Cast<CheckBox>().CurrentValue)
                {
                    var dmgPer = (enemy.TotalShieldHealth() - damage > 0 ? enemy.TotalShieldHealth() - damage : 0) / enemy.TotalShieldMaxHealth();
                    var currentHPPer = enemy.TotalShieldHealth() / enemy.TotalShieldMaxHealth();
                    var initPoint = new Vector2((int)(enemy.HPBarPosition.X + XOff + dmgPer * Width), (int)enemy.HPBarPosition.Y + YOff);
                    var endPoint = new Vector2((int)(enemy.HPBarPosition.X + XOff + currentHPPer * Width) + 1, (int)enemy.HPBarPosition.Y + YOff);
                    EloBuddy.SDK.Rendering.Line.DrawLine(System.Drawing.Color.Orange, Thick, initPoint, endPoint);
                }
            }
        }

        public static float RTime(Obj_AI_Base target)
        {
            if (target.HasBuff("Undying Rage"))
            {
                return Math.Max(0, target.GetBuff("Undying Rage").EndTime) - Game.Time;
            }
            return 0;
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useW2 = ComboMenu["ComboW2"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var disE = ComboMenu["DisE"].Cast<Slider>().CurrentValue;
            var item = ComboMenu["BOTRK"].Cast<CheckBox>().CurrentValue;
            var Minhp = ComboMenu["ihp"].Cast<Slider>().CurrentValue;
            var Minhpp = ComboMenu["ihpp"].Cast<Slider>().CurrentValue;
            var turret = ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue;
            if (target != null)
            {
                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && (disE <= target.Distance(Player.Instance) || Player.Instance.HealthPercent <= 20))
                {
                    if (turret)
                    {
                        if (!target.Position.UnderTuret())
                        {
                            E.Cast(target.Position);
                        }
                    }
                    else
                    {
                        E.Cast(target.Position);
                    }
                }

                if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    if (_Player.Distance(target) <= target.GetAutoAttackRange() && Player.Instance.HealthPercent <= 60)
                    {
                        W.Cast();
                    }

                    if (useW2)
                    {
                        if (!target.IsFacing(Player.Instance) && _Player.Distance(target) >= 325 && !E.IsReady())
                        {
                            W.Cast();
                        }
                    }
                    else
                    {
                        if (!target.IsFacing(Player.Instance) && _Player.Distance(target) >= 325)
                        {
                            W.Cast();
                        }
                    }
                }

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

        private static void Ultimate()
        {
            var useQ = Ulti["Q"].Cast<CheckBox>().CurrentValue;
            var useQ2 = Ulti["Q2"].Cast<CheckBox>().CurrentValue;
            var mauQ = Ulti["QHp"].Cast<Slider>().CurrentValue;
            var useR = Ulti["ultiR"].Cast<CheckBox>().CurrentValue;
            var mauR = Ulti["MauR"].Cast<Slider>().CurrentValue;
            var passive = Ulti["Ps"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(E.Range) && !e.IsDead))
            {
                if (!Player.Instance.HasBuff("JudicatorIntervention") && !Player.Instance.HasBuff("kindredrnodeathbuff"))
                {
                    if (useR && R.IsReady() && !Player.Instance.IsInShopRange() && (target.IsValidTarget(E.Range) || _Player.IsUnderEnemyturret()))
                    {
                        if (Player.Instance.HealthPercent <= mauR)
                        {
                            R.Cast();
                        }

                        if (target.GetAutoAttackDamage(_Player) >= _Player.Health)
                        {
                            R.Cast();
                        }

                        if (Player.Instance.HasBuff("ZedR"))
                        {
                            Core.DelayAction(() => R.Cast(), 500);
                        }
                    }

                    if (useQ && Q.IsReady() && Player.Instance.HasBuff("TryndamereQ") && RTime(Player.Instance) <= 1 && Player.Instance.Mana >= passive && (target.IsValidTarget(E.Range) || _Player.IsUnderEnemyturret()))
                    {
                        if (useQ2)
                        {
                            if (!R.IsReady() && (Player.Instance.HealthPercent <= mauQ || Player.Instance.HasBuff("ZedR")))
                            {
                                Q.Cast();
                            }
                        }
                        else
                        {
                            if ((Player.Instance.HealthPercent <= mauQ || Player.Instance.HasBuff("ZedR")))
                            {
                                Q.Cast();
                            }
                        }
                    }
                }
            }
        }

        public static void ResetAttack(AttackableUnit e, EventArgs args)
        {
            if (!(e is AIHeroClient)) return;
            var target = TargetSelector.GetTarget(300, DamageType.Physical);
            var champ = (AIHeroClient)e;
            var useriu = ComboMenu["hydra"].Cast<CheckBox>().CurrentValue;
            if (champ == null || champ.Type != GameObjectType.AIHeroClient || !champ.IsValid) return;
            if (target != null)
            {
                if (useriu && (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)))
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
            var useE = LaneClearMenu["E"].Cast<CheckBox>().CurrentValue;
            var minE = LaneClearMenu["mine"].Cast<Slider>().CurrentValue;
            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(e => e.IsValidTarget(E.Range));
            var ECanCast = EntityManager.MinionsAndMonsters.GetLineFarmLocation(minions, E.Width, (int) E.Range);
            foreach (var minion in minions)
            {
                if (useE && E.IsReady() && minion.IsValidTarget(E.Range) && ECanCast.HitNumber >= minE)
                {
                    E.Cast(ECanCast.CastPosition);
                }
            }
        }

        private static void Harass()
        {
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var useW2 = HarassMenu["HarassW2"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var disE = HarassMenu["DistanceE"].Cast<Slider>().CurrentValue;
            var turret = HarassMenu["HTurret"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (target != null)
            {
                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && (disE <= target.Distance(Player.Instance) || Player.Instance.HealthPercent <= 20))
                {
                    if (turret)
                    {
                        if (!target.Position.UnderTuret())
                        {
                            E.Cast(target.Position);
                        }
                    }
                    else
                    {
                        E.Cast(target.Position);
                    }
                }

                if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    if (_Player.Distance(target) <= target.GetAutoAttackRange() && Player.Instance.HealthPercent <= 60)
                    {
                        W.Cast();
                    }

                    if (useW2)
                    {
                        if (!target.IsFacing(Player.Instance) && _Player.Distance(target) >= 325 && !E.IsReady())
                        {
                            W.Cast();
                        }
                    }
                    else
                    {
                        if (!target.IsFacing(Player.Instance) && _Player.Distance(target) >= 325)
                        {
                            W.Cast();
                        }
                    }
                }
            }
        }

        public static void JungleClear()
        {
            var useE = JungleClearMenu["EJungle"].Cast<CheckBox>().CurrentValue;
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(E.Range));
            if (monster != null)
            {
                if (useE && E.IsReady() && monster.IsValidTarget(E.Range))
                {
                    E.Cast(monster.Position);
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

        public static void DrawFont(Font vFont, string vText, float jx, float jy, ColorBGRA jc)
        {
            vFont.DrawText(null, vText, (int)jx, (int)jy, jc);
        }

        public static bool UnderTuret(this Vector3 position)
        {
            return EntityManager.Turrets.Enemies.Where(a => a.Health > 0 && !a.IsDead).Any(a => a.Distance(position) < 950);
        }

        private static void KillSteal()
        {
            var KsE = KillStealMenu["KsE"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(R.Range) && !hero.HasBuff("FioraW") && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.HasBuff("SpellShield") && !hero.HasBuff("NocturneShield") && !hero.IsDead && !hero.IsZombie))
            {
                if (KsE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (target.TotalShieldHealth() <= Player.Instance.GetSpellDamage(target, SpellSlot.E))
                    {
                        E.Cast(target.Position);
                    }
                }

                if (Ignite != null && KillStealMenu["ign"].Cast<CheckBox>().CurrentValue && Ignite.IsReady())
                {
                    if (target.TotalShieldHealth() <= _Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite))
                    {
                        Ignite.Cast(target);
                    }
                }
            }
        }
    }
}
