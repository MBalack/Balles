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

namespace Pantheon
{
    internal class Program
    {
        public static Spell.Targeted Q;
        public static Spell.Targeted W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Spell.Targeted Ignite;
        public static Item Botrk;
        public static Item Bil;
        public static Item Youmuu;
        public static Font Thm;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Menu Menu, ComboMenu, JungleClearMenu, HarassMenu, LaneClearMenu, Misc, Items, KillSteals;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

// Menu

        private static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Pantheon")) return;
            Chat.Print("Doctor's Pantheon Loaded!", Color.White);
            Bootstrap.Init(null);
            Q = new Spell.Targeted(SpellSlot.Q, 600);
            W = new Spell.Targeted(SpellSlot.W, 600);
            E = new Spell.Skillshot(SpellSlot.E, 600, SkillShotType.Cone, 250, 2000, 70);
            E.AllowedCollisionCount = int.MaxValue;
            R = new Spell.Skillshot(SpellSlot.R, 2000, SkillShotType.Circular);
            R.AllowedCollisionCount = int.MaxValue;
            Youmuu = new Item(3142, 10);
            Botrk = new Item(ItemId.Blade_of_the_Ruined_King);
            Bil = new Item(3144, 475f);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 32, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Menu = MainMenu.AddMenu("Doctor's Pantheon", "Pantheon");
            Menu.AddGroupLabel("Mercedes7");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("CQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("CW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("CE", new CheckBox("Use [E] Combo"));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HW", new CheckBox("Use [W] Harass"));
            HarassMenu.Add("HE", new CheckBox("Use [E] Harass"));
            HarassMenu.Add("HM", new Slider("Mana Harass", 50, 0, 100));
            HarassMenu.AddGroupLabel("Auto Harass Settings");
            HarassMenu.Add("AutoQ", new CheckBox("Auto [Q] Harass"));
            HarassMenu.Add("AutoM", new Slider("Mana Auto Harass", 60, 0, 100));
            HarassMenu.AddGroupLabel("Auto [Q] On");
            foreach (var target in EntityManager.Heroes.Enemies)
            {
                HarassMenu.Add("HarassQ" + target.ChampionName, new CheckBox("" + target.ChampionName));
            }

            LaneClearMenu = Menu.AddSubMenu("Laneclear Settings", "Clear");
            LaneClearMenu.AddGroupLabel("Laneclear Settings");
            LaneClearMenu.Add("LQ", new CheckBox("Use [Q] Laneclear"));
            LaneClearMenu.Add("LW", new CheckBox("Use [W] Laneclear", false));
            LaneClearMenu.Add("LE", new CheckBox("Use [E] Laneclear", false));
            LaneClearMenu.Add("ME", new Slider("Mana LaneClear", 2, 1, 6));
            LaneClearMenu.Add("LM", new Slider("Mana LaneClear", 60, 0, 100));
            LaneClearMenu.AddGroupLabel("LastHit Settings");
            LaneClearMenu.Add("LHQ", new CheckBox("Use [Q] LastHit"));
            LaneClearMenu.Add("LHM", new Slider("Mana LastHit", 60, 0, 100));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("JQ", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("JW", new CheckBox("Use [W] JungleClear"));
            JungleClearMenu.Add("JE", new CheckBox("Use [E] JungleClear"));
            JungleClearMenu.Add("JM", new Slider("Mana JungleClear", 20, 0, 100));

            Items = Menu.AddSubMenu("Items Settings", "Items");
            Items.AddGroupLabel("Items Settings");
            Items.Add("you", new CheckBox("Use [Youmuu]"));
            Items.Add("BOTRK", new CheckBox("Use [Botrk]"));
            Items.Add("ihp", new Slider("My HP Use BOTRK <=", 50));
            Items.Add("ihpp", new Slider("Enemy HP Use BOTRK <=", 50));

            Misc = Menu.AddSubMenu("Misc Settings", "Draw");
            Misc.AddGroupLabel("Anti Gapcloser");
            Misc.Add("antiGap", new CheckBox("Use [W] Anti Gapcloser", false));
            Misc.Add("inter", new CheckBox("Use [W] Interupt"));
            Misc.AddGroupLabel("Drawings Settings");
            Misc.Add("Draw_Disabled", new CheckBox("Disabled Drawings", false));
            Misc.Add("Draw", new CheckBox("Draw [Q/W/E]"));
            Misc.Add("Notifications", new CheckBox("Draw Text Can Kill With R"));
            Misc.AddGroupLabel("Skins Settings");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Misc.Add("skin.Id", new ComboBox("Skin Mode", 6, "1", "2", "3", "4", "5", "6", "7", "8", "9"));

            KillSteals = Menu.AddSubMenu("KillSteal Changer", "KillSteal");
            KillSteals.Add("Q", new CheckBox("Use [Q] KillSteal"));
            KillSteals.Add("W", new CheckBox("Use [W] KillSteal"));
            KillSteals.Add("ign", new CheckBox("Use [Ignite] KillSteal"));

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interupt;
            Orbwalker.OnUnkillableMinion += Orbwalker_CantLasthit;
            Spellbook.OnCastSpell += OnCastSpell;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }

            if (ECasting)
            {
                Orbwalker.DisableAttacking = true;
                Orbwalker.DisableMovement = true;
            }
            else
            {
                Core.DelayAction(() => Orbwalker.DisableAttacking = false, 1500);
                Core.DelayAction(() => Orbwalker.DisableMovement = false, 1500);
            }

            KillSteal();
            Item();
            AutoHarass();

            if (_Player.SkinId != Misc["skin.Id"].Cast<ComboBox>().CurrentValue)
            {
                if (checkSkin())
                {
                    Player.SetSkinId(SkinId());
                }
            }
        }

// Drawings

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;
			
            if (Misc["Draw_Disabled"].Cast<CheckBox>().CurrentValue) return;
			
            if (Misc["Draw"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2, Radius = E.Range }.Draw(_Player.Position);
            }
			
            if (Misc["Notifications"].Cast<CheckBox>().CurrentValue)
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(2000) && RDamage(e) >= e.TotalShieldHealth()))
                {
                    if (R.IsReady() && _Player.Distance(target) >= 810)
                    {
                        DrawFont(Thm, "[R] Can Killable " + target.ChampionName, (float)(ft[0] - 140), (float)(ft[1] + 80), SharpDX.Color.Red);  
                    }
                }
            }
        }

// Skin Changer

        public static int SkinId()
        {
            return Misc["skin.Id"].Cast<ComboBox>().CurrentValue;
        }

        public static bool checkSkin()
        {
            return Misc["checkSkin"].Cast<CheckBox>().CurrentValue;
        }

// EBuff

        public static bool ECasting
        {
            get { return Player.Instance.HasBuff("PantheonESound") ; }
        }

// OnCastSpell

        private static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (!sender.Owner.IsMe)
            {
                return;
            }

            if (args.Slot == SpellSlot.Q || args.Slot == SpellSlot.W)
            {
                if (ECasting)
                {
                    args.Process = false;
                }
            }
        }

// Damage Lib

        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 0, 65, 105, 145, 185, 225 }[Q.Level] + 1.4f * _Player.FlatPhysicalDamageMod));
        }

        public static float WDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 50, 75, 100, 125, 150 }[W.Level] + 1.0f * _Player.FlatMagicDamageMod));
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 200, 350, 500 }[R.Level] + 0.5f * _Player.FlatMagicDamageMod));
        }

// Interrupt

        public static void Interupt(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs i)
        {
            var Inter = Misc["inter"].Cast<CheckBox>().CurrentValue;
            if (!sender.IsEnemy || !(sender is AIHeroClient) || Player.Instance.IsRecalling())
            {
                return;
            }
			
            if (Inter && W.IsReady() && i.DangerLevel == DangerLevel.High && _Player.Distance(sender) <= W.Range)
            {
                W.Cast(sender);
            }
        }

// AntiGap

        private static void Gapcloser_OnGapCloser(Obj_AI_Base sender, Gapcloser.GapcloserEventArgs args)
        {
            var useW = Misc["antiGap"].Cast<CheckBox>().CurrentValue;
            if (useW && W.IsReady() && sender.IsEnemy && args.Sender.Distance(_Player) <= 325)
            {
                W.Cast(sender);
            }
        }

//Harass Mode

        private static void Harass()
        {
            var useQ = HarassMenu["HQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HW"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HE"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["HM"].Cast<Slider>().CurrentValue;

            if (Player.Instance.ManaPercent <= mana)
            {
                return;
            }

            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q.Range) && !e.IsDead && !e.IsZombie))
            {
                if (useW && W.CanCast(target) && !target.HasBuffOfType(BuffType.Stun))
                {
                    W.Cast(target);
                }

                if (useQ && Q.CanCast(target) && (_Player.Distance(target) >= 275 || !W.IsReady() && !E.IsReady()))
                {
                    Q.Cast(target);
                }

                if (useE && E.CanCast(target))
                {
                    var pred = E.GetPrediction(target);
                    if (!W.IsReady() || !W.IsLearned)
                    {
                        if (pred.HitChance >= HitChance.Medium)
                        {
                            E.Cast(pred.CastPosition);
                        }
                    }

                    else if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup))
                    {
                        E.Cast(target.Position);
                    }
                }
            }
        }

//Combo Mode

        private static void Combo()
        {
            var useQ = ComboMenu["CQ"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["CW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["CE"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q.Range) && !e.IsDead && !e.IsZombie))
            {
                if (useW && W.CanCast(target) && !target.HasBuffOfType(BuffType.Stun))
                {
                    W.Cast(target);
                }

                if (useQ && Q.CanCast(target) && (_Player.Distance(target) >= 275 || !W.IsReady() && !E.IsReady()))
                {
                    Q.Cast(target);
                }

                if (useE && E.CanCast(target))
                {
                    var pred = E.GetPrediction(target);
                    if (!Q.IsReady() && !W.IsReady())
                    {
                        if (pred.HitChance >= HitChance.Medium)
                        {
                            E.Cast(pred.CastPosition);
                        }
                    }

                    if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup))
                    {
                        E.Cast(target.Position);
                    }
                }
            }
        }

//LaneClear Mode

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["LQ"].Cast<CheckBox>().CurrentValue;
            var useW = LaneClearMenu["LW"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["LE"].Cast<CheckBox>().CurrentValue;
            var MinE = LaneClearMenu["ME"].Cast<Slider>().CurrentValue;
            var mana = LaneClearMenu["LM"].Cast<Slider>().CurrentValue;
            var minionQ = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(e => e.IsValidTarget(Q.Range));
            var quang = EntityManager.MinionsAndMonsters.GetLineFarmLocation(minionQ, E.Width, (int) E.Range);

            if (Player.Instance.ManaPercent <= mana)
            {
                return;
            }

            if (ECasting)
            {
                return;
            }

            foreach (var minion in minionQ)
            {
                if (useQ && Q.CanCast(minion))
                {
                    if (minion.Health <= QDamage(minion))
                    {
                        Q.Cast(minion);
                    }
                }

                if (useW && W.CanCast(minion))
                {
                    if (minion.Health <= WDamage(minion))
                    {
                        W.Cast(minion);
                    }
                }

                if (useE && E.CanCast(minion))
                {
                    if (quang.HitNumber >= MinE)
                    {
                        E.Cast(quang.CastPosition);
                    }
                }
            }
        }

// LastHit Mode

        private static void Orbwalker_CantLasthit(Obj_AI_Base target, Orbwalker.UnkillableMinionArgs args)
        {
            var useQ = LaneClearMenu["LHQ"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["LHM"].Cast<Slider>().CurrentValue;
            var unit = (useQ && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) && Player.Instance.ManaPercent >= mana);
            if (target == null)
            {
                return;
            }

            if (unit && Q.IsReady() && target.IsValidTarget(Q.Range))
            {
                if (QDamage(target) >= Prediction.Health.GetPrediction(target, Q.CastDelay) && !Orbwalker.IsAutoAttacking)
                {
                    Q.Cast(target);
                }
            }
        }
// JungleClear Mode

        private static void JungleClear()
        {
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(Q.Range));
            var useQ = JungleClearMenu["JQ"].Cast<CheckBox>().CurrentValue;
            var useW = JungleClearMenu["JW"].Cast<CheckBox>().CurrentValue;
            var useE = JungleClearMenu["JE"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["JM"].Cast<Slider>().CurrentValue;
            if (Player.Instance.ManaPercent <= mana)
            {
                return;
            }

            if (ECasting)
            {
                return;
            }

            if (monster != null)
            {
                if (useQ && Q.CanCast(monster))
                {
                    Q.Cast(monster);
                }

                if (useW && W.CanCast(monster))
                {
                    W.Cast(monster);
                }

                if (useE && E.CanCast(monster))
                {
                    E.Cast(monster.Position);
                }
            }
        }

        public static void DrawFont(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }

// KillSteal

        private static void KillSteal()
        {
            var Enemies = EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q.Range) && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && !e.IsZombie);
            var useQ = KillSteals["Q"].Cast<CheckBox>().CurrentValue;
            var useW = KillSteals["W"].Cast<CheckBox>().CurrentValue;
            var useIG = KillSteals["ign"].Cast<CheckBox>().CurrentValue;
            foreach (var target in Enemies)
            {
                if (useQ && Q.CanCast(target))
                {
                    if (target.HealthPercent > 15)
                    {
                        if (target.TotalShieldHealth() <= QDamage(target))
                        {
                            Q.Cast(target);
                        }
                    }
                    else
                    {
                        if (target.TotalShieldHealth() <= QDamage(target) * 1.5f)
                        {
                            Q.Cast(target);
                        }
                    }
                }

                if (useW && W.CanCast(target))
                {
                    if (target.TotalShieldHealth() <= WDamage(target) + _Player.GetAutoAttackDamage(target))
                    {
                        W.Cast(target);
                    }
                }

                if (useIG && Ignite != null && Ignite.IsReady())
                {
                    if (target.TotalShieldHealth() <= _Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite))
                    {
                        Ignite.Cast(target);
                    }
                }
            }
        }

// Auto Harass

        private static void AutoHarass()
        {
            var useQ = HarassMenu["AutoQ"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["AutoM"].Cast<Slider>().CurrentValue;

            if (Player.Instance.ManaPercent <= mana)
            {
                return;
            }

            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q.Range) && !e.IsDead && !e.IsZombie))
            {
                if (useQ && Q.CanCast(target))
                { 
                    if (HarassMenu["HarassQ" + target.ChampionName].Cast<CheckBox>().CurrentValue)
                    {
                        Q.Cast(target);
                    }
                }
            }
        }

// Use Items

        public static void Item()
        {
            var item = Items["BOTRK"].Cast<CheckBox>().CurrentValue;
            var yous = Items["you"].Cast<CheckBox>().CurrentValue;
            var Minhp = Items["ihp"].Cast<Slider>().CurrentValue;
            var Minhpp = Items["ihpp"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(475) && !e.IsDead))
            {
                if (item && Bil.IsReady() && Bil.IsOwned() && Bil.IsInRange(target))
                {
                    Bil.Cast(target);
                }

                if ((item && Botrk.IsReady() && Botrk.IsOwned() && target.IsValidTarget(475)) && (Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp))
                {
                    Botrk.Cast(target);
                }

                if (yous && Youmuu.IsReady() && Youmuu.IsOwned() && _Player.Distance(target) <= 325 && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    Youmuu.Cast();
                }
            }
        }
    }
}
