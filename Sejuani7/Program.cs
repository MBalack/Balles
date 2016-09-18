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

namespace Sejuani7
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
        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Active E;
        public static Spell.Skillshot R;
        public static Spell.Skillshot F;
        public static Spell.Targeted Ignite;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Sejuani")) return;
            Chat.Print("Doctor's Sejuani Loaded!", Color.Orange);
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, 650, SkillShotType.Linear, 0, 1600, 70);
            Q.AllowedCollisionCount = -1;
            W = new Spell.Active(SpellSlot.W, 350);
            E = new Spell.Active(SpellSlot.E, 1000);
            R = new Spell.Skillshot(SpellSlot.R, 1200, SkillShotType.Linear, 250, 1600, 110);
            R.AllowedCollisionCount = -1;
            F = new Spell.Skillshot(_Player.GetSpellSlotFromName("summonerflash"), 425, SkillShotType.Linear, 0, int.MaxValue, 60);
            F.AllowedCollisionCount = int.MaxValue;
            Thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 32, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Thn = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 20, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Ignite = new Spell.Targeted(_Player.GetSpellSlotFromName("summonerdot"), 600);
            Menu = MainMenu.AddMenu("Sejuani", "Sejuani");
            Menu.AddGroupLabel("Doctor7");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("ComboFQ", new KeyBind("[Q] + [Flash] Target ", false, KeyBind.BindTypes.HoldActive, 'T'));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("DisQ", new Slider("Use [Q] If Enemy Distance >", 10, 0, 650));
            ComboMenu.AddLabel("[Q] Distance < 125 = Always [Q]");
            ComboMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            ComboMenu.Add("DisE", new Slider("Use [E] If Enemy Distance > ", 10, 0, 1000));
            ComboMenu.AddLabel("[E] Distance < 125 = Always [E]");
            ComboMenu.AddGroupLabel("Ultimate Settings");
            ComboMenu.Add("ComboR", new CheckBox("Use [R] Combo"));
            ComboMenu.Add("MinR", new Slider("Min Hit Enemies Use [R]", 3, 0, 5));
            ComboMenu.AddGroupLabel("Interrupt Settings");
            ComboMenu.Add("inter", new CheckBox("Use [R] Interrupt", false));
            ComboMenu.Add("interQ", new CheckBox("Use [Q] Interrupt", false));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HarassW", new CheckBox("Use [W] Harass"));
            HarassMenu.Add("DisQ2", new Slider("Use [Q] If Enemy Distance >", 350, 0, 650));
            HarassMenu.AddLabel("[Q] Distance < 125 = Always [Q]");
            HarassMenu.Add("HarassE", new CheckBox("Use [E] Harass"));
            HarassMenu.Add("DisE2", new Slider("Use [E] If Enemy Distance >", 350, 0, 1000));
            HarassMenu.AddLabel("[E] Distance < 125 = Always [E]");
            HarassMenu.Add("ManaQ", new Slider("Min Mana Harass", 40));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("QJungle", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("WJungle", new CheckBox("Use [W] JungleClear"));
            JungleClearMenu.Add("EJungle", new CheckBox("Use [E] JungleClear"));
            JungleClearMenu.Add("JungleMana", new Slider("Min Mana JungleClear", 20));

            LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("Lane Clear Settings");
            LaneClearMenu.Add("LastQLC", new CheckBox("Always [Q] LaneClear"));
            LaneClearMenu.Add("CantLC", new CheckBox("Only [Q] Killable Minion", false));
            LaneClearMenu.Add("LastWLC", new CheckBox("Use [W] LaneClear"));
            LaneClearMenu.Add("LaneE", new CheckBox("Use [E] LaneClear"));
            LaneClearMenu.Add("ManaLC", new Slider("Min Mana LaneClear", 50));

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
            KillStealMenu.Add("KsE", new CheckBox("Use [E] KillSteal"));
            KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));
            KillStealMenu.AddSeparator();
            KillStealMenu.AddGroupLabel("Ultimate Settings");
            KillStealMenu.Add("KsR", new CheckBox("Use [R] KillSteal"));
            KillStealMenu.Add("minKsR", new Slider("Min [R] Distance KillSteal", 100, 1, 1175));
            KillStealMenu.Add("RKb", new KeyBind("[R] Semi Manual Key", false, KeyBind.BindTypes.HoldActive, 'T'));
            KillStealMenu.AddGroupLabel("Recommended Distance 600");

            Skin = Menu.AddSubMenu("Skin Changer", "SkinChanger");
            Skin.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Skin.Add("skin.Id", new ComboBox("Skin Mode", 4, "Default", "1", "2", "3", "4", "5", "6"));

            Drawings = Menu.AddSubMenu("Draw Settings", "Draw");
            Drawings.AddGroupLabel("Drawing Settings");
            Drawings.Add("DrawQ", new CheckBox("[Q] Range"));
            Drawings.Add("DrawW", new CheckBox("[W] Range"));
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
            if (Drawings["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = W.Range }.Draw(_Player.Position);
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
                if (target != null && target.IsValidTarget(R.Range) && RDamage(target) > target.Health + target.AttackShield)
                {
                    DrawFont(Thm, "R Can Killable " + target.ChampionName, (float)(ft[0] - 140), (float)(ft[1] + 80), SharpDX.Color.Red);
                }
            }
            if (Drawings["DrawRhit"].Cast<CheckBox>().CurrentValue && target != null && R.IsReady() && target.IsValidTarget(R.Range))
            {
                var RPred = R.GetPrediction(target);
                var MinR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
                if (RPred.CastPosition.CountEnemiesInRange(400) >= MinR)
                {
                    Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                    DrawFont(Thm, "[R] Can Hit " + RPred.CastPosition.CountEnemiesInRange(400), (float)(ft[0] - 90), (float)(ft[1] + 20), SharpDX.Color.Orange);
                }
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
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
                RLogic();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            KillSteal();
            FlashQ();
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
            var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var disE = ComboMenu["DisE"].Cast<Slider>().CurrentValue;
            var disQ = ComboMenu["DisQ"].Cast<Slider>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            if (target == null) return;
            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && disQ <= target.Distance(Player.Instance))
            {
                Q.Cast(target);
            }
            if (useE && E.IsReady() && target.IsValidTarget(E.Range) && disE <= target.Distance(Player.Instance) && target.HasBuff("SejuaniFrost"))
            {
                E.Cast();
            }
        }

        private static void RLogic()
        {
            var target2 = EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(R.Range) && !e.IsDead);
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var MinR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            foreach (var targetR in target2)
            {
                if (useR && R.IsReady() && targetR.IsValidTarget(R.Range))
                {
                    var RPred = R.GetPrediction(targetR);
                    if (RPred.CastPosition.CountEnemiesInRange(400) >= MinR && RPred.HitChance >= HitChance.High)
                    {
                        R.Cast(RPred.CastPosition);
                    }
                }
                if (useW && W.IsReady() && targetR.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
            }
        }


        private static void FlashQ()
        {
            var targetF = TargetSelector.SelectedTarget;
            var useFQ = ComboMenu["ComboFQ"].Cast<KeyBind>().CurrentValue;
            if (targetF != null)
            {
                if (useFQ && Q.IsReady() && F.IsReady() && targetF.IsValidTarget(1000))
                {
                    Player.CastSpell(SpellSlot.Q, targetF.Position);
                }
                if (Player.HasBuff("SejuaniArcticAssault") && targetF.IsValidTarget(425))
                {
                    Player.CastSpell(_Player.GetSpellSlotFromName("summonerflash"), targetF.Position);
                }
            }
        }


        private static void LaneClear()
        {
            var laneQMN = LaneClearMenu["ManaLC"].Cast<Slider>().CurrentValue;
            var useQLH = LaneClearMenu["LastQLC"].Cast<CheckBox>().CurrentValue;
            var useQ = LaneClearMenu["CantLC"].Cast<CheckBox>().CurrentValue;
            var useWLH = LaneClearMenu["LastWLC"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["LaneE"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            if (Player.Instance.ManaPercent < laneQMN) return;
            foreach (var minion in minions)
            {
                if (useQLH && Q.IsReady() && minion.IsValidTarget(Q.Range))
                {
                    Q.Cast(minion);
                }
                else if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && minion.Health < QDamage(minion))
                {
                    Q.Cast(minion);
                }
                if (useWLH && W.IsReady() && minion.IsValidTarget(W.Range) && minions.Count() >= 3)
                {
                    W.Cast();
                }
                if (useE && E.IsReady() && minion.IsValidTarget(E.Range) && minions.Count() >= 3 && minion.HasBuff("SejuaniFrost"))
                {
                    E.Cast();
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
                if (useE && E.IsReady() && monters.IsValidTarget(E.Range) && monters.HasBuff("SejuaniFrost"))
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
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var disE = HarassMenu["DisE2"].Cast<Slider>().CurrentValue;
            var disQ = HarassMenu["DisQ2"].Cast<Slider>().CurrentValue;
            var ManaQ = HarassMenu["ManaQ"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            if (Player.Instance.ManaPercent < ManaQ) return;
            if (target != null)
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && disQ <= target.Distance(Player.Instance))
                {
                    Q.Cast(target);
                }
                if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && disE <= target.Distance(Player.Instance) && target.HasBuff("SejuaniFrost"))
                {
                    E.Cast();
                }
            }
        }

        private static void Flee()
        {
            if (Q.IsReady())
            {
                var cursorPos = Game.CursorPos;
                var castPos = Player.Instance.Position.Distance(cursorPos) <= Q.Range ? cursorPos : Player.Instance.Position.Extend(cursorPos, Q.Range).To3D();
                Q.Cast(castPos);
            }
        }

        public static void Interupt(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs i)
        {
            var Inter = ComboMenu["inter"].Cast<CheckBox>().CurrentValue;
            var InterQ = ComboMenu["interQ"].Cast<CheckBox>().CurrentValue;
            if (!sender.IsEnemy || !(sender is AIHeroClient) || Player.Instance.IsRecalling())
            {
                return;
            }
            if (Inter && R.IsReady() && i.DangerLevel == DangerLevel.High && R.IsInRange(sender))
            {
                R.Cast(sender.Position);
            }
            if (InterQ && Q.IsReady() && i.DangerLevel == DangerLevel.High && Q.IsInRange(sender))
            {
                Q.Cast(sender.Position);
            }
        }

        public static void DrawFont(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 150, 250, 350 }[R.Level] + 0.8f * _Player.FlatMagicDamageMod));
        }

        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 80, 125, 170, 215, 260 }[Q.Level] + 0.4f * _Player.FlatMagicDamageMod));
        }

        public static float EDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 80, 105, 130, 155, 180 }[E.Level] + 0.5f * _Player.FlatMagicDamageMod));
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
                    if (target.Health + target.AttackShield < QDamage(target))
                    {
                        Q.Cast(target);
                    }
                }
                if (KsE && E.IsReady() && target.IsValidTarget(E.Range) && target.HasBuff("SejuaniFrost"))
                {
                    if (target.Health + target.AttackShield < EDamage(target))
                    {
                        E.Cast();
                    }
                }
                if (KsR && R.IsReady())
                {
                    if (target.Health + target.AttackShield < RDamage(target) && !target.IsInRange(Player.Instance, minKsR))
                    {
                        R.Cast(target);
                    }
                }
                if (R.IsReady() && KillStealMenu["RKb"].Cast<KeyBind>().CurrentValue)
                {
                    if (target.Health + target.AttackShield < RDamage(target))
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
                    if (target.Health < _Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite))
                    {
                        Ignite.Cast(target);
                    }
                }
            }
        }
    }
}
