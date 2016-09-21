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

namespace Malphite
{
    internal class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, JungleClearMenu, LaneClearMenu, KillStealMenu, Skin, Drawings;
        public static Font Thm;
        public static Font Thn;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell.Targeted Q;
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
            if (!_Player.ChampionName.Contains("Malphite")) return;
            Chat.Print("Doctor's Malphite Loaded!", Color.Orange);
            Bootstrap.Init(null);
            Q = new Spell.Targeted(SpellSlot.Q, 625);
            W = new Spell.Active(SpellSlot.W, 250);
            E = new Spell.Active(SpellSlot.E, 400);
            R = new Spell.Skillshot(SpellSlot.R, 1000, SkillShotType.Circular, 250, 700, 270);
            Thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 32, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Thn = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 20, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Ignite = new Spell.Targeted(_Player.GetSpellSlotFromName("summonerdot"), 600);
            Menu = MainMenu.AddMenu("Malphite", "Malphite");
            Menu.AddGroupLabel("Doctor7");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            ComboMenu.Add("DisQ", new Slider("Use [Q] If Enemy Distance >", 10, 0, 650));
            ComboMenu.AddLabel("[Q] Distance < 125 = Always [Q]");
            ComboMenu.AddGroupLabel("Ultimate Settings");
            ComboMenu.Add("ComboFQ", new KeyBind("Use [R] Selected Target", false, KeyBind.BindTypes.HoldActive, 'T'));
            ComboMenu.Add("ComboR", new CheckBox("Use [R] Aoe"));
            ComboMenu.Add("MinR", new Slider("Min Hit Enemies Use [R] Aoe", 3, 1, 5));
            ComboMenu.AddGroupLabel("Interrupt Settings");
            ComboMenu.Add("inter", new CheckBox("Use [R] Interrupt", false));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HarassW", new CheckBox("Use [W] Harass"));
            HarassMenu.Add("HarassE", new CheckBox("Use [E] Harass"));
            HarassMenu.Add("DisQ2", new Slider("Use [Q] If Enemy Distance >", 350, 0, 650));
            HarassMenu.AddLabel("[Q] Distance < 125 = Always [Q]");
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
            LaneClearMenu.Add("LaneClearE", new CheckBox("Use [E] LaneClear"));
            LaneClearMenu.Add("ManaLC", new Slider("Mana LaneClear", 50));
            LaneClearMenu.AddGroupLabel("LastHit Settings");
            LaneClearMenu.Add("LastHitQ", new CheckBox("Use [Q] LastHit"));
            LaneClearMenu.Add("ManaLH", new Slider("Mana LastHit", 50));

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
            KillStealMenu.Add("KsE", new CheckBox("Use [E] KillSteal"));
            KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));
            KillStealMenu.AddSeparator();
            KillStealMenu.AddGroupLabel("Ultimate Settings");
            KillStealMenu.Add("KsR", new CheckBox("Use [R] KillSteal"));
            KillStealMenu.Add("minKsR", new Slider("Min [R] Distance KillSteal", 100, 1, 1000));
            KillStealMenu.Add("RKb", new KeyBind("[R] Semi Manual Key", false, KeyBind.BindTypes.HoldActive, 'Y'));
            KillStealMenu.AddGroupLabel("Recommended Distance 600");

            Skin = Menu.AddSubMenu("Skin Changer", "SkinChanger");
            Skin.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Skin.Add("skin.Id", new ComboBox("Skin Mode", 4, "Default", "1", "2", "3", "4", "5", "6", "7"));

            Drawings = Menu.AddSubMenu("Draw Settings", "Draw");
            Drawings.AddGroupLabel("Drawing Settings");
            Drawings.Add("DrawQ", new CheckBox("[Q] Range"));
            Drawings.Add("DrawE", new CheckBox("[E] Range"));
            Drawings.Add("DrawR", new CheckBox("[R] Range"));
            Drawings.Add("DrawRhit", new CheckBox("[R] Draw Hit"));
            Drawings.Add("Notifications", new CheckBox("Notifications Killable [R]"));
            Drawings.Add("Draw_Disabled", new CheckBox("Disabled Drawings"));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnTick += Game_OnTick;
            Interrupter.OnInterruptableSpell += Interupt;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;
            if (Drawings["Draw_Disabled"].Cast<CheckBox>().CurrentValue)
                return;
            if (Drawings["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (Drawings["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = E.Range }.Draw(_Player.Position);
            }
            if (Drawings["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = R.Range }.Draw(_Player.Position);
            }
            var target = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            if (Drawings["Notifications"].Cast<CheckBox>().CurrentValue && R.IsReady())
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (target != null && target.IsValidTarget(R.Range) && Player.Instance.GetSpellDamage(target, SpellSlot.R) > target.Health + target.AttackShield)
                {
                    DrawFont(Thm, "R Can Killable " + target.ChampionName, (float)(ft[0] - 140), (float)(ft[1] + 80), SharpDX.Color.Red);
                }
            }
            if (Drawings["DrawRhit"].Cast<CheckBox>().CurrentValue && target != null && R.IsReady() && target.IsValidTarget(R.Range))
            {
                var RPred = R.GetPrediction(target);
                var MinR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
                if (RPred.CastPosition.CountEnemiesInRange(250) >= MinR)
                {
                    Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                    DrawFont(Thm, "[R] Can Hit " + RPred.CastPosition.CountEnemiesInRange(250), (float)(ft[0] - 90), (float)(ft[1] + 20), SharpDX.Color.Orange);
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
            RSelect();
            if (_Player.SkinId != Skin["skin.Id"].Cast<ComboBox>().CurrentValue)
            {
                if (checkSkin())
                {
                    Player.SetSkinId(SkinId());
                }
            }
            if (ComboMenu["ComboFQ"].Cast<KeyBind>().CurrentValue)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
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
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var MinR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var disQ = ComboMenu["DisQ"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(R.Range) && !e.IsDead))
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && disQ < target.Distance(Player.Instance))
                {
                    Q.Cast(target);
                }
				
                if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
				
                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    E.Cast();
                }

                if (useR && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    var RPred = R.GetPrediction(target);
                    if (RPred.CastPosition.CountEnemiesInRange(250) >= MinR && RPred.HitChance >= HitChance.High)
                    {
                        R.Cast(RPred.CastPosition);
                    }
                }
            }
        }

        private static void RSelect()
        {
            var targetF = TargetSelector.SelectedTarget;
            var useFQ = ComboMenu["ComboFQ"].Cast<KeyBind>().CurrentValue;
            if (targetF == null) return;
            if (useFQ && R.IsReady())
            {
                if (targetF.IsValidTarget(R.Range))
                {
                    R.Cast(targetF.Position);
                }
            }
        }

        private static void LaneClear()
        {
            var mana = LaneClearMenu["ManaLC"].Cast<Slider>().CurrentValue;
            var useQ = LaneClearMenu["LaneClearQ"].Cast<CheckBox>().CurrentValue;
            var useW = LaneClearMenu["LaneClearW"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["LaneClearE"].Cast<CheckBox>().CurrentValue;
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(a => a.Distance(Player.Instance) <= Q.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (Player.Instance.ManaPercent < mana) return;
            if (minion != null)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && minion.Health < Player.Instance.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }
				
                if (useW && W.IsReady() && minion.IsValidTarget(W.Range) && _Player.Position.CountEnemyMinionsInRange(Q.Range) >= 3)
                {
                    W.Cast();
                }
				
                if (useE && E.IsReady() && minion.IsValidTarget(E.Range) && _Player.Position.CountEnemyMinionsInRange(E.Range) >= 3)
                {
                    E.Cast();
                }
            }
        }

        private static void LastHit()
        {
            var mana = LaneClearMenu["ManaLH"].Cast<Slider>().CurrentValue;
            var useQ = LaneClearMenu["LastHitQ"].Cast<CheckBox>().CurrentValue;
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(a => a.Distance(Player.Instance) <= Q.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (Player.Instance.ManaPercent < mana) return;
            if (minion != null)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && minion.Health < Player.Instance.GetSpellDamage(minion, SpellSlot.Q) && _Player.Distance(minion) > 175)
                {
                    Q.Cast(minion);
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
                    E.Cast();
                }
				
                if (useW && W.IsReady() && monters.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
            }
        }

        private static void Harass()
        {
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var ManaQ = HarassMenu["ManaQ"].Cast<Slider>().CurrentValue;
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var disQ = HarassMenu["DisQ2"].Cast<Slider>().CurrentValue;
            if (Player.Instance.ManaPercent < ManaQ) return;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q.Range) && !e.IsDead))
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && disQ <= target.Distance(Player.Instance))
                {
                    Q.Cast(target);
                }

                if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    W.Cast();
                }

                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    E.Cast();
                }
            }
        }

        private static void Flee()
        {
            if (R.IsReady())
            {
                var cursorPos = Game.CursorPos;
                var castPos = Player.Instance.Position.Distance(cursorPos) <= R.Range ? cursorPos : Player.Instance.Position.Extend(cursorPos, R.Range).To3D();
                R.Cast(castPos);
            }
        }

        public static void Interupt(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs i)
        {
            var Inter = ComboMenu["inter"].Cast<CheckBox>().CurrentValue;
            if (!sender.IsEnemy || !(sender is AIHeroClient) || Player.Instance.IsRecalling())
            {
                return;
            }
            if (Inter && R.IsReady() && i.DangerLevel == DangerLevel.High && R.IsInRange(sender))
            {
                R.Cast(sender.Position);
            }
        }

        public static void DrawFont(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }

        private static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var KsE = KillStealMenu["KsE"].Cast<CheckBox>().CurrentValue;
            var KsR = KillStealMenu["KsR"].Cast<CheckBox>().CurrentValue;
            var minKsR = KillStealMenu["minKsR"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(R.Range) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie))
            {
                if (KsQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (target.Health + target.AttackShield <= Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                    {
                        Q.Cast(target);
                    }
                }
				
                if (KsE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (target.Health + target.AttackShield <= Player.Instance.GetSpellDamage(target, SpellSlot.E))
                    {
                        E.Cast();
                    }
                }
				
                if (KsR && R.IsReady())
                {
                    if (target.Health + target.AttackShield <= Player.Instance.GetSpellDamage(target, SpellSlot.R) && !target.IsInRange(Player.Instance, minKsR))
                    {
                        R.Cast(target);
                    }
                }
				
                if (R.IsReady() && KillStealMenu["RKb"].Cast<KeyBind>().CurrentValue)
                {
                    if (target.Health + target.AttackShield <= Player.Instance.GetSpellDamage(target, SpellSlot.R))
                    {
                        var pred = R.GetPrediction(target);
                        if (pred.HitChancePercent >= 70)
                        {
                            R.Cast(pred.CastPosition);
                        }
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
