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
using Font = SharpDX.Direct3D9.Font;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace Irelia
{
    static class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, JungleClearMenu, LaneClearMenu, KillStealMenu, Misc;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell.Targeted Q;
        public static Spell.Active W;
        public static Spell.Targeted E;
        public static Spell.Skillshot R;
        public static Spell.Targeted Ignite;
        public static Font thm;
        public static Item Botrk;
        public static Item Bil;
        public static Item Sheen;
        public static Item Tryn;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Irelia")) return;
            Chat.Print("Doctor's Irelia Loaded!", Color.Orange);
            Bootstrap.Init(null);
            Q = new Spell.Targeted(SpellSlot.Q, 625);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Targeted(SpellSlot.E, 425);
            R = new Spell.Skillshot(SpellSlot.R, 900, SkillShotType.Linear, 250, 1600, 120);
            R.AllowedCollisionCount = int.MaxValue;
            Botrk = new Item(ItemId.Blade_of_the_Ruined_King);
            Sheen = new Item(ItemId.Sheen);
            Tryn = new Item(ItemId.Trinity_Force);
            Bil = new Item(3144, 475f);
            Ignite = new Spell.Targeted(_Player.GetSpellSlotFromName("summonerdot"), 600);
            thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 16, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Menu = MainMenu.AddMenu("Doctor's Irelia", "Irelia");
            Menu.AddGroupLabel("Doctor7");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("ComboQ2", new CheckBox("Use [Q] Minions Approaching Enemies"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("DisQ", new Slider("Use [Q] If Enemy Distance >", 125, 0, 625));
            ComboMenu.AddLabel("[Q] Distance < 125 = Always [Q]");
            ComboMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            ComboMenu.Add("AlwaysE", new CheckBox("Only Use [E] If Can Stun Target", false));
            ComboMenu.Add("CTurret", new KeyBind("Dont Use [Q] UnderTurret", false, KeyBind.BindTypes.PressToggle, 'T'));
            ComboMenu.AddGroupLabel("Ultimate Settings");
            ComboMenu.Add("useRCombo", new CheckBox("Use [R] Combo"));
            ComboMenu.Add("RHeatlh", new CheckBox("Use [R] If MyHP <"));
            ComboMenu.Add("MauR", new Slider("MyHP Use [R] <", 50));
            ComboMenu.Add("RShen", new CheckBox("Use [R] Sheen"));
            ComboMenu.Add("KsR", new CheckBox("Use [R] KillSteal"));
            ComboMenu.Add("Rminion", new CheckBox("Use [R] On Minion If No Enemies Around"));
            ComboMenu.AddGroupLabel("Interrupt Settings");
            ComboMenu.Add("interQ", new CheckBox("Use [E] Interrupt"));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HarassQ2", new CheckBox("Use [Q] Minions Approaching Enemies"));
            HarassMenu.Add("HarassW", new CheckBox("Use [W] Harass"));
            HarassMenu.Add("DisQ2", new Slider("Use [Q] If Enemy Distance >", 125, 0, 625));
            HarassMenu.AddLabel("[Q] Distance < 125 = Always [Q]");
            HarassMenu.Add("HarassE", new CheckBox("Use [E] Harass"));
            HarassMenu.Add("AlwaysEH", new CheckBox("Only Use [E] If Can Stun Target"));
            HarassMenu.Add("ManaQ", new Slider("Mana Harass", 40));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("QJungle", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("WJungle", new CheckBox("Use [W] JungleClear"));
            JungleClearMenu.Add("EJungle", new CheckBox("Use [E] JungleClear"));
            JungleClearMenu.Add("JungleMana", new Slider("Mana JungleClear", 20));

            LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("Lane Clear Settings");
            LaneClearMenu.Add("LaneClearQ", new CheckBox("Use [Q] LaneClear"));
            LaneClearMenu.Add("LaneClearW", new CheckBox("Use [W] LaneClear"));
            LaneClearMenu.Add("MauW", new Slider("Use [W] If HealthPercent <", 50));
            LaneClearMenu.Add("LaneClearE", new CheckBox("Use [E] LaneClear"));
            LaneClearMenu.Add("ManaLC", new Slider("Mana LaneClear", 50));
            LaneClearMenu.AddGroupLabel("LastHit Settings");
            LaneClearMenu.Add("LastHitQ", new CheckBox("Use [Q] LastHit"));
            LaneClearMenu.Add("LastHitE", new CheckBox("Use [E] LastHit"));
            LaneClearMenu.Add("ManaLH", new Slider("Mana LastHit", 50));

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
            KillStealMenu.Add("KsE", new CheckBox("Use [E] KillSteal"));
            KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));

            Misc = Menu.AddSubMenu("Misc Changer", "Misc");
            Misc.AddGroupLabel("Items Settings");
            Misc.Add("BOTRK", new CheckBox("Use [Botrk]"));
            Misc.Add("ihp", new Slider("My HP Use BOTRK <=", 50));
            Misc.Add("ihpp", new Slider("Enemy HP Use BOTRK <=", 50));
            Misc.AddGroupLabel("Flee Settings");
            Misc.Add("FleeQ", new CheckBox("Only Use [Q] Flee If Killalble Minion"));
            Misc.AddGroupLabel("Skin Changer");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Misc.Add("skin.Id", new ComboBox("Skin Mode", 4, "Default", "1", "2", "3", "4", "5", "6"));
            Misc.AddGroupLabel("Drawing Settings");
            Misc.Add("DrawQ", new CheckBox("[Q] Range"));
            Misc.Add("DrawR", new CheckBox("[R] Range"));
            Misc.Add("DrawTR", new CheckBox("Status Under Turret"));
            Misc.Add("Draw_Disabled", new CheckBox("Disabled Drawings"));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnTick += Game_OnTick;
            Interrupter.OnInterruptableSpell += Interupt;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;
            if (Misc["Draw_Disabled"].Cast<CheckBox>().CurrentValue)
                return;
            if (Misc["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (Misc["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = R.Range }.Draw(_Player.Position);
            }
            if (Misc["DrawTR"].Cast<CheckBox>().CurrentValue)
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue)
                {
                    DrawFont(thm, "Combo & Farm Under Turret : Disable", (float)(ft[0] - 110), (float)(ft[1] + 50), SharpDX.Color.White);
                }
                else
                {
                    DrawFont(thm, "Combo & Farm Under Turret : Enable", (float)(ft[0] - 110), (float)(ft[1] + 50), SharpDX.Color.Red);
                }
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            KillSteal();
            Item();
            RBaxam();
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

        private static bool UnderTuret(Obj_AI_Base target)
        {
            var tower = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(turret => turret != null && turret.Distance(target) <= 775 && turret.IsValid && turret.Health > 0 && !turret.IsAlly);
            return tower != null;
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useQ2 = ComboMenu["ComboQ2"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var disQ = ComboMenu["DisQ"].Cast<Slider>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var HealthE = ComboMenu["AlwaysE"].Cast<CheckBox>().CurrentValue;
            var turret = ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue;
            foreach (var minion in EntityManager.MinionsAndMonsters.GetLaneMinions().Where(m => m.IsValidTarget(Q.Range) && Player.Instance.GetSpellDamage(m, SpellSlot.Q) > m.TotalShieldHealth()))
            {
                var targetQ = TargetSelector.GetTarget(1200, DamageType.Physical);
                if (useQ2 && minion.IsValidTarget(Q.Range) && Player.Instance.Mana > Q.Handle.SData.Mana * 2 && _Player.Distance(targetQ) > Q.Range && minion.Distance(targetQ) < _Player.Distance(targetQ))
                {
                    if (turret)
                    {
                        if (!UnderTuret(minion))
                        {
                            Q.Cast(minion);
                        }
                    }
                    else
                    {
                        Q.Cast(minion);
                    }
                }
            }
            if (target == null) return;
            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && disQ <= target.Distance(Player.Instance))
            {
                if (turret)
                {
                    if (!UnderTuret(target))
                    {
                        Q.Cast(target);
                    }
                }
                else
                {
                    Q.Cast(target);
                }
            }
            if (useE && E.IsReady() && target.IsValidTarget(E.Range))
            {
                if (HealthE)
                {
                    if (target.HealthPercent > _Player.HealthPercent || target.HealthPercent < 30)
                    {
                        E.Cast(target);
                    }
                }
                else
                {
                    E.Cast(target);
                }
            }
            if (useW && W.IsReady() && target.IsValidTarget(E.Range) && Player.Instance.GetAutoAttackRange() > target.Distance(Player.Instance))
            {
                W.Cast();
            }
            var target2 = EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(R.Range) && !e.IsDead && !e.IsZombie);
            var Rhealth = ComboMenu["RHeatlh"].Cast<CheckBox>().CurrentValue;
            var mauR = ComboMenu["MauR"].Cast<Slider>().CurrentValue;
            var RSheen = ComboMenu["RShen"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["useRCombo"].Cast<CheckBox>().CurrentValue;
            if (!useR) return;
            foreach (var targetR in target2)
            {
                if (Rhealth && R.IsReady() && targetR.IsValidTarget(R.Range))
                {
                    if (_Player.HealthPercent < mauR)
                    {
                        R.Cast(targetR);
                    }
                }
                if (RSheen)
                {
                    if (targetR.IsValidTarget(R.Range) && R.IsReady() && Player.Instance.GetAutoAttackRange() < targetR.Distance(Player.Instance) && !_Player.HasBuff("Sheen") && (Sheen.IsOwned() || Tryn.IsOwned()))
                    {
                        R.Cast(targetR);
                    }
                }
                else
                {
                    if (targetR.IsValidTarget(R.Range) && _Player.HasBuff("IreliaTranscendentBlades"))
                    {
                        R.Cast(targetR);
                    }
                }
                if (!Sheen.IsOwned() || !Tryn.IsOwned())
                {
                    if (targetR.IsValidTarget(R.Range) && _Player.HasBuff("IreliaTranscendentBlades"))
                    {
                        R.Cast(targetR);
                    }
                }
            }
        }

        private static void RBaxam()
        {
            var useR = ComboMenu["Rminion"].Cast<CheckBox>().CurrentValue;
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(m => m.IsValidTarget(Q.Range)).OrderBy(m => m.Health).FirstOrDefault();
            if (minion == null) return;
            if (useR && _Player.HasBuff("IreliaTranscendentBlades") && _Player.Position.CountEnemiesInRange(1200) == 0)
            {
                R.Cast(minion);
            }
        }

        private static void LaneClear()
        {
            var laneQMN = LaneClearMenu["ManaLC"].Cast<Slider>().CurrentValue;
            var minW = LaneClearMenu["MauW"].Cast<Slider>().CurrentValue;
            var useQ = LaneClearMenu["LaneClearQ"].Cast<CheckBox>().CurrentValue;
            var useW = LaneClearMenu["LaneClearW"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["LaneClearE"].Cast<CheckBox>().CurrentValue;
            var turret = ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue;
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(m => m.IsValidTarget(Q.Range)).OrderBy(m => m.Health).FirstOrDefault();
            if (Player.Instance.ManaPercent < laneQMN) return;
            if (minion != null)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && minion.Health < Player.Instance.GetSpellDamage(minion, SpellSlot.Q))
                {
                    if (turret)
                    {
                        if (!UnderTuret(minion))
                        {
                            Q.Cast(minion);
                        }
                    }
                    else
                    {
                        Q.Cast(minion);
                    }
                }
                if (useW && W.IsReady() && minion.IsValidTarget(W.Range) && _Player.HealthPercent <= minW)
                {
                    W.Cast();
                }
                if (useE && E.IsReady() && minion.IsValidTarget(E.Range) && minion.Health < Player.Instance.GetSpellDamage(minion, SpellSlot.E))
                {
                    E.Cast(minion);
                }
            }
        }

        private static void LastHit()
        {
            var useQ = LaneClearMenu["LastHitQ"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["LastHitE"].Cast<CheckBox>().CurrentValue;
            var laneQMN = LaneClearMenu["ManaLH"].Cast<Slider>().CurrentValue;
            var turret = ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue;
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(m => m.IsValidTarget(Q.Range)).OrderBy(m => m.Health).FirstOrDefault();
            if (Player.Instance.ManaPercent < laneQMN) return;
            if (minion != null)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && minion.Health < Player.Instance.GetSpellDamage(minion, SpellSlot.Q))
                {
                    if (turret)
                    {
                        if (!UnderTuret(minion))
                        {
                            Q.Cast(minion);
                        }
                    }
                    else
                    {
                        Q.Cast(minion);
                    }
                }
                if (useE && E.IsReady() && minion.IsValidTarget(E.Range) && minion.Health < Player.Instance.GetSpellDamage(minion, SpellSlot.E))
                {
                    E.Cast(minion);
                }
            }
        }

        public static void JungleClear()
        {
            var useQ = JungleClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var useE = JungleClearMenu["EJungle"].Cast<CheckBox>().CurrentValue;
            var useW = JungleClearMenu["WJungle"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["JungleMana"].Cast<Slider>().CurrentValue;
            var monters = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(R.Range));
            if (Player.Instance.ManaPercent <= mana) return;
            if (monters != null)
            {
                if (useQ && Q.IsReady() && monters.IsValidTarget(Q.Range))
                {
                    Q.Cast(monters);
                }
                if (useE && E.IsReady() && monters.IsValidTarget(E.Range))
                {
                    E.Cast(monters);
                }
                if (useW && W.IsReady() && monters.IsValidTarget(E.Range) && Player.Instance.GetAutoAttackRange() <= monters.Distance(Player.Instance))
                {
                    W.Cast();
                }
            }
        }

        private static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useQ2 = HarassMenu["HarassQ2"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var HealthE = HarassMenu["AlwaysEH"].Cast<CheckBox>().CurrentValue;
            var disQ = HarassMenu["DisQ2"].Cast<Slider>().CurrentValue;
            var ManaQ = HarassMenu["ManaQ"].Cast<Slider>().CurrentValue;
            var turret = ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue;
            var target = TargetSelector.GetTarget(1200, DamageType.Physical);
            if (Player.Instance.ManaPercent < ManaQ) return;
            foreach (var minion in EntityManager.MinionsAndMonsters.GetLaneMinions().Where(m => m.IsValidTarget(Q.Range) && Player.Instance.GetSpellDamage(m, SpellSlot.Q) > m.TotalShieldHealth()))
            {
                var targetQ = TargetSelector.GetTarget(1200, DamageType.Physical);
                if (useQ2 && minion.IsValidTarget(Q.Range) && Player.Instance.Mana > Q.Handle.SData.Mana * 2 && _Player.Distance(targetQ) > Q.Range && minion.Distance(targetQ) < _Player.Distance(targetQ))
                {
                    if (turret)
                    {
                        if (!UnderTuret(minion))
                        {
                            Q.Cast(minion);
                        }
                    }
                    else
                    {
                        Q.Cast(minion);
                    }
                }
            }
            if (target == null) return;
            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && disQ <= target.Distance(Player.Instance))
            {
                if (turret)
                {
                    if (!UnderTuret(target))
                    {
                        Q.Cast(target);
                    }
                }
                else
                {
                    Q.Cast(target);
                }
            }
            if (useE && E.IsReady() && target.IsValidTarget(E.Range))
            {
                if (HealthE)
                {
                    if (target.HealthPercent > _Player.HealthPercent || target.HealthPercent < 30)
                    {
                        E.Cast(target);
                    }
                }
                else
                {
                    E.Cast(target);
                }
            }
            if (useW && W.IsReady() && target.IsValidTarget(E.Range) && Player.Instance.GetAutoAttackRange() > target.Distance(Player.Instance))
            {
                W.Cast();
            }
        }

// Use Items

        public static void Item()
        {
            var item = Misc["BOTRK"].Cast<CheckBox>().CurrentValue;
            var Minhp = Misc["ihp"].Cast<Slider>().CurrentValue;
            var Minhpp = Misc["ihpp"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(450, DamageType.Physical);
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
            }
        }

        public static Obj_AI_Base QFlee(Vector3 pos)
        {
            int distance = 250000;
            Obj_AI_Base unit = null;
            foreach (var source in EntityManager.MinionsAndMonsters.CombinedAttackable.Where(e => !e.IsDead && e.Health > 0 && e.Distance(Player.Instance) < 625))
            {
                int dist = (int) source.Distance(pos);
                if (dist >= distance) continue;
                distance = dist;
                unit = source;
            }
            if (unit != null) return unit;
            foreach (var source in EntityManager.Heroes.Enemies.Where(e => !e.IsDead && e.Health > 0 && e.Distance(Player.Instance) < 625))
            {
                int dist = (int) source.Distance(pos);
                if (dist >= distance) continue;
                distance = dist;
                unit = source;
            }
            return unit;
        }

        private static void Flee()
        {
            var Flee = Misc["FleeQ"].Cast<CheckBox>().CurrentValue;
            var e = QFlee(Game.CursorPos);
            if (Q.IsReady() && e != null && e.Distance(Game.CursorPos) < Player.Instance.Distance(Game.CursorPos))
            {
                if (Flee)
                {
                    if (e.Health < Player.Instance.GetSpellDamage(e, SpellSlot.Q))
                    {
                        Q.Cast(e);
                        return;
                    }
                }
                else
                {
                    Q.Cast(e);
                    return;
                }
            }
        }

        public static void Interupt(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs i)
        {
            var InterQ = ComboMenu["interQ"].Cast<CheckBox>().CurrentValue;
            if (!sender.IsEnemy || !(sender is AIHeroClient) || Player.Instance.IsRecalling())
            {
                return;
            }
            if (InterQ && E.IsReady() && i.DangerLevel == DangerLevel.High && E.IsInRange(sender) && sender.HealthPercent > _Player.HealthPercent)
            {
                E.Cast(sender);
            }
        }

        public static void DrawFont(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 0, 80, 120, 160 }[R.Level] + 0.5f * _Player.FlatMagicDamageMod + 0.6f * _Player.FlatPhysicalDamageMod));
        }

        private static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var KsE = KillStealMenu["KsE"].Cast<CheckBox>().CurrentValue;
            var KsR = ComboMenu["KsR"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(R.Range) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie))
            {
                if (KsQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                    {
                        Q.Cast(target);
                    }
                }
                if (KsE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.E))
                    {
                        E.Cast(target);
                    }
                }
                if (KsR && R.IsReady())
                {
                    if (target.Health + target.AttackShield < RDamage(target) * 4 && (_Player.Distance(target) > 325 || !Q.IsReady()))
                    {
                        R.Cast(target);
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
