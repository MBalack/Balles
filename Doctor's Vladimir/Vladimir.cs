using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Vladimir
{
    class Program
    {
        public static Menu Menu, ComboMenu, Evade, HarassMenu, LaneClearMenu, Misc, KillStealMenu, Drawings;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static readonly List<string> DodgeSpells = new List<string>() { "SorakaQ", "SorakaE", "TahmKenchW", "TahmKenchQ", "Bushwhack", "ForcePulse", "KarthusFallenOne", "KarthusWallOfPain", "KarthusLayWasteA1", "KarmaWMantra", "KarmaQMissileMantra", "KarmaSpiritBind", "KarmaQ", "JinxW", "JinxE", "JarvanIVGoldenAegis", "HowlingGaleSpell", "SowTheWind", "ReapTheWhirlwind", "IllaoiE", "HeimerdingerUltWDummySpell", "HeimerdingerUltEDummySpell", "HeimerdingerW", "HeimerdingerE", "HecarimUlt", "HecarimRampAttack", "GravesQLineSpell", "GravesQLineMis", "GravesClusterShot", "GravesSmokeGrenade", "GangplankR", "GalioIdolOfDurand", "GalioResoluteSmite", "FioraE", "EvelynnR", "EliseHumanE", "EkkoR", "EkkoW", "EkkoQ", "DravenDoubleShot", "InfectedCleaverMissileCast", "DariusExecute", "DariusAxeGrabCone", "DariusNoxianTacticsONH", "DariusCleave", "PhosphorusBomb", "MissileBarrage", "BraumQ", "BrandFissure", "BardR", "BardQ", "AatroxQ", "AatroxE", "AzirE", "AzirEWrapper", "AzirQWrapper", "AzirQ", "AzirR", "Pulverize", "AhriSeduce", "CurseoftheSadMummy", "InfernalGuardian", "Incinerate", "Volley", "EnchantedCrystalArrow", "BraumRWrapper", "CassiopeiaPetrifyingGaze", "FeralScream", "Rupture", "EzrealEssenceFlux", "EzrealMysticShot", "EzrealTrueshotBarrage", "FizzMarinerDoom", "GnarW", "GnarBigQMissile", "GnarQ", "GnarR", "GragasQ", "GragasE", "GragasR", "RiftWalk", "LeblancSlideM", "LeblancSlide", "LeonaSolarFlare", "UFSlash", "LuxMaliceCannon", "LuxLightStrikeKugel", "LuxLightBinding", "yasuoq3w", "VelkozE", "VeigarEventHorizon", "VeigarDarkMatter", "VarusR", "ThreshQ", "ThreshE", "ThreshRPenta", "SonaQ", "SonaR", "ShenShadowDash", "SejuaniGlacialPrisonCast", "RivenMartyr", "JavelinToss", "NautilusSplashZone", "NautilusAnchorDrag", "NamiR", "NamiQ", "DarkBindingMissile", "StaticField", "RocketGrab", "RocketGrabMissile", "timebombenemybuff", "NocturneUnspeakableHorror", "SyndraQ", "SyndraE", "SyndraR", "VayneCondemn", "Dazzle", "Overload", "AbsoluteZero", "IceBlast", "LeblancChaosOrb", "JudicatorReckoning", "KatarinaQ", "NullLance", "Crowstorm", "FiddlesticksDarkWind", "BrandWildfire", "Disintegrate", "FlashFrost", "Frostbite", "AkaliMota", "InfiniteDuress", "PantheonW", "blindingdart", "JayceToTheSkies", "IreliaEquilibriumStrike", "maokaiunstablegrowth", "nautilusgandline", "runeprison", "WildCards", "BlueCardAttack", "RedCardAttack", "GoldCardAttack", "AkaliShadowDance", "Headbutt", "PowerFist", "BrandConflagration", "CaitlynYordleTrap", "CaitlynAceintheHole", "CassiopeiaNoxiousBlast", "CassiopeiaMiasma", "CassiopeiaTwinFang", "Feast", "DianaArc", "DianaTeleport", "EliseHumanQ", "EvelynnE", "Terrify", "FizzPiercingStrike", "Parley", "GarenQAttack", "GarenR", "IreliaGatotsu", "IreliaEquilibriumStrike", "SowTheWind", "JarvanIVCataclysm", "JaxLeapStrike", "JaxEmpowerTwo", "JaxCounterStrike", "JayceThunderingBlow", "KarmaSpiritBind", "NetherBlade", "KatarinaR", "JudicatorRighteousFury", "KennenBringTheLight", "LeblancChaosOrbM", "BlindMonkRKick", "LeonaZenithBlade", "LeonaShieldOfDaybreak", "LissandraW", "LissandraQ", "LissandraR", "LuluQ", "LuluW", "LuluE", "LuluR", "SeismicShard", "AlZaharMaleficVisions", "AlZaharNetherGrasp", "MaokaiUnstableGrowth", "MordekaiserMaceOfSpades", "MordekaiserChildrenOfTheGrave", "SoulShackles", "NamiW", "NasusW", "NautilusGrandLine", "Takedown", "NocturneParanoia", "PoppyDevastatingBlow", "PoppyHeroicCharge", "QuinnE", "PuncturingTaunt", "RenektonPreExecute", "SpellFlux", "SejuaniWintersClaw", "TwoShivPoisen", "Fling", "SkarnerImpale", "SonaHymnofValor", "SwainTorment", "SwainDecrepify", "BlindingDart", "OrianaIzunaCommand", "OrianaDetonateCommand", "DetonatingShot", "BusterShot", "TrundleTrollSmash", "TrundlePain", "MockingShout", "Expunge", "UdyrBearStance", "UrgotHeatseekingLineMissile", "UrgotSwap2", "VeigarBalefulStrike", "VeigarPrimordialBurst", "ViR", "ViktorPowerTransfer", "VladimirTransfusion", "VolibearQ", "HungeringStrike", "XenZhaoComboTarget", "XenZhaoSweep", "YasuoQ3W", "YasuoQ3Mis", "YasuoQ3", "YasuoRKnockUpComboW" };
        public static Font Thn;
        public static Spell.Targeted Q;
        public static Spell.Active W;
        public static Spell.Active E;
        public static Spell.Skillshot R;
        public static Spell.Targeted Ignite;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        private static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Vladimir")) return;
            Chat.Print("Doctor's Vladimir Loaded!", Color.Orange);
            Q = new Spell.Targeted(SpellSlot.Q, 600);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Active(SpellSlot.E, 600);
            R = new Spell.Skillshot(SpellSlot.R, 700, SkillShotType.Circular, 250, 1200, 150);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Thn = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 15, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Menu = MainMenu.AddMenu("Doctor's Vladimir", "Vladimir");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            ComboMenu.Add("ComboR", new CheckBox("Use [R] Combo"));
            ComboMenu.AddLabel("Use [R] Aoe");
            ComboMenu.Add("ComboR2", new CheckBox("Use [R] Aoe"));
            ComboMenu.Add("MinR", new Slider("Use [R] Aoe Enemies >=", 2, 1, 5));
            ComboMenu.AddLabel("Auto [Q-W] Low HP");
            ComboMenu.Add("Wtoggle", new CheckBox("Auto [W] Low MyHp"));
            ComboMenu.Add("minHealth", new Slider("Use [W] My Hp <", 20));
            ComboMenu.Add("AutoQ", new CheckBox("Auto [Q] Low MyHp On Enemies", false));
            ComboMenu.Add("AutoQm", new CheckBox("Auto [Q] Low MyHp On Minions", false));
            ComboMenu.Add("healthQ", new Slider("Auto [Q] MyHp <", 30));
            ComboMenu.AddLabel("Use [W] Dodge Spell");
            ComboMenu.Add("dodge", new CheckBox("Use [W] Dodge"));
            ComboMenu.Add("antiGap", new CheckBox("Use [W] Anti Gap"));
            ComboMenu.Add("healthgap", new Slider("Use [W] AntiGap My Hp <", 50));

            Evade = Menu.AddSubMenu("Spell Dodge List", "Evade");
            Evade.AddLabel("Dodge Settings");
            foreach (AIHeroClient enemies in EntityManager.Heroes.Enemies)
            {
                Evade.AddLabel(enemies.BaseSkinName);
                {
                    foreach (SpellDataInst skill in enemies.Spellbook.Spells)
                    {
                        if (DodgeSpells.Any(s => s == skill.SData.Name))
                        {
                            Evade.Add(skill.Name, new CheckBox(enemies.BaseSkinName + " : " + skill.Slot.ToString() + " : " + skill.Name));
                            Evade.AddLabel(skill.Name);
                        }
                    }
                }
            }

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HarassE", new CheckBox("Use [E] Harass"));
            HarassMenu.Add("Autoqh", new KeyBind("Auto [Q] Harass", false, KeyBind.BindTypes.PressToggle, 'T'));

            LaneClearMenu = Menu.AddSubMenu("Clear Settings", "LaneClear");
            LaneClearMenu.AddLabel("Clear Settings");
            LaneClearMenu.Add("QLC", new CheckBox("Use [Q] LaneClear"));
            LaneClearMenu.Add("ELC", new CheckBox("Use [E] LaneClear"));
            LaneClearMenu.Add("minE", new Slider("Min Hit Minions Use [E]", 3, 1, 6));
            LaneClearMenu.AddLabel("LastHit Settings");
            LaneClearMenu.Add("QLH", new CheckBox("Use [Q] LastHit"));
            LaneClearMenu.AddLabel("JungleClear Settings");
            LaneClearMenu.Add("QJungle", new CheckBox("Use [Q] JungleClear"));
            LaneClearMenu.Add("EJungle", new CheckBox("Use [E] JungleClear"));

            Misc = Menu.AddSubMenu("Skin Settings", "Misc");
            Misc.AddLabel("Skin Changer");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer"));
            Misc.Add("skin.Id", new ComboBox("Skin Mode", 7, "Default", "1", "2", "3", "4", "5", "6", "7", "8"));

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddLabel("KillSteal Settings");
            KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
            KillStealMenu.Add("KsE", new CheckBox("Use [E] KillSteal"));
            KillStealMenu.Add("KsR", new CheckBox("Use [R] KillSteal"));
            KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));

            Drawings = Menu.AddSubMenu("Drawings Settings", "Draw");
            Drawings.AddLabel("Drawing Settings");
            Drawings.Add("DrawQ", new CheckBox("[Q] Range"));
            Drawings.Add("DrawE", new CheckBox("[E] Range"));
            Drawings.Add("DrawR", new CheckBox("[R] Range"));
            Drawings.Add("DrawAT", new CheckBox("Draw Auto Harass"));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnTick += Game_OnTick;
            Obj_AI_Base.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;

            if (Drawings["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
			
            if (Drawings["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
			
            if (Drawings["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }

            if (Drawings["DrawAT"].Cast<CheckBox>().CurrentValue)
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (HarassMenu["Autoqh"].Cast<KeyBind>().CurrentValue)
                {
                    DrawFont(Thn, "Auto [Q] Harass : Enable", (float)(ft[0] - 60), (float)(ft[1] + 20), SharpDX.Color.Red);
                }
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
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
			
            KillSteal();
            AutoQ();
            WLogic();

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

        public static bool EActive()
        {
            return _Player.HasBuff("VladimirE");
        }

        public static bool Frenzy()
        {
            return _Player.HasBuff("vladimirqfrenzy");
        }

        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 80, 100, 120, 140, 160 }[Program.Q.Level] + 0.45f * _Player.FlatMagicDamageMod));
        }

        public static float EDamage(Obj_AI_Base target)
        {
            return Player.Instance.GetSpellDamage(target, SpellSlot.E);
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 168, 280, 392 }[Program.R.Level] + 0.75f * _Player.FlatMagicDamageMod));
        }

        public static void AutoQ()
        {
            var AutoQH = HarassMenu["Autoqh"].Cast<KeyBind>().CurrentValue;
            var AutoQ = ComboMenu["Autoq"].Cast<CheckBox>().CurrentValue;
            var AutoQm = ComboMenu["Autoqm"].Cast<CheckBox>().CurrentValue;
            var health = ComboMenu["healthQ"].Cast<Slider>().CurrentValue;
            var Enemies = EntityManager.Heroes.Enemies.FirstOrDefault(e => e.IsValidTarget(Q.Range));
            var minions = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(m => m.IsValidTarget(Q.Range));
            if (!Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)
            && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)
            && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)
            && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) 
            && !Orbwalker.IsAutoAttacking)
            {
                if (Q.IsReady() && _Player.HealthPercent <= health)
                {
                    if (AutoQ && Enemies != null && Enemies.IsValidTarget(Q.Range))
                    {
                        Q.Cast(Enemies);
                    }
					
                    if (AutoQm && minions != null && minions.IsValidTarget(Q.Range))
                    {
                        Q.Cast(minions);
                    }
                }
				
                if (Enemies != null)
                {
                    if (AutoQH && Q.IsReady() && Enemies.IsValidTarget(Q.Range))
                    {
                        Q.Cast(Enemies);
                    }
                }
            }
        }

        private static void Gapcloser_OnGapCloser(Obj_AI_Base sender, Gapcloser.GapcloserEventArgs args)
        {
            var AntiGap = ComboMenu["antiGap"].Cast<CheckBox>().CurrentValue;
            var HealthGap = ComboMenu["healthgap"].Cast<Slider>().CurrentValue;
            if (AntiGap && W.IsReady() && args.Sender.Distance(_Player) < 325)
            {
                if (_Player.HealthPercent <= HealthGap)
                {
                    W.Cast();
                }
            }
        }
		
        public static void Combo()
        {
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var useR2 = ComboMenu["ComboR2"].Cast<CheckBox>().CurrentValue;
            var MinR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(R.Range) && !e.IsDead))
     	    {
                if (useR2 && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    var pred = R.GetPrediction(target);
                    if (pred.CastPosition.CountEnemiesInRange(350) >= MinR && pred.HitChance >= HitChance.High)
                    {
                        R.Cast(pred.CastPosition);
                    }
                }

                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !EActive())
                {
                    Q.Cast(target);
                }

                if (useW && W.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (EActive() && _Player.Distance(target) <= 375)
                    {
                        W.Cast();
                    }
                    else if (target.IsValidTarget(325))
                    {
                        W.Cast();
                    }
					
                }

                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !EActive())
                {
                    E.Cast();
                }

                if (useR && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    if (QDamage(target) * 2 + RDamage(target) + EDamage(target) >= target.Health)
                    {
                        var pred1 = R.GetPrediction(target);
                        if (pred1.HitChance >= HitChance.Medium)
                        {
                            R.Cast(pred1.CastPosition);
                        }
                    }
                }
            }
        }


        public static void JungleClear()
        {
            var useQ = LaneClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["EJungle"].Cast<CheckBox>().CurrentValue;
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters(_Player.ServerPosition, 600).FirstOrDefault(x => x.IsValidTarget(Q.Range));
            if (monster != null)
            {
                if (useQ && Q.IsReady() && monster.IsValidTarget(Q.Range))
		    	{
                    Q.Cast(monster);
                }
				
                if (useE && E.IsReady() && monster.IsValidTarget(E.Range) && !EActive())
		    	{
                    E.Cast();
                }
            }
        }

        public static void LastHit()
        {
            var useQ = LaneClearMenu["QLH"].Cast<CheckBox>().CurrentValue;
            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(e => e.IsValidTarget(Q.Range));
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady())
                {
                    if (QDamage(minion) >= minion.TotalShieldHealth())
                    {
                        Q.Cast(minion);
                    }
                }
            }
        }

        public static void LaneClear()
        {
            var useQ = LaneClearMenu["QLC"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["ELC"].Cast<CheckBox>().CurrentValue;
            var MinE = LaneClearMenu["minE"].Cast<Slider>().CurrentValue;
            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(e => e.IsValidTarget(Q.Range));
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady())
                {
                    if (QDamage(minion) >= minion.TotalShieldHealth())
                    {
                        Q.Cast(minion);
                    }
                }

                if (useE && E.IsReady() && _Player.CountEnemyMinionsInRange(E.Range) >= MinE && !EActive())
                {
                    E.Cast();
                }
            }
        }

        public static void WLogic()
        {
            var useW = ComboMenu["Wtoggle"].Cast<CheckBox>().CurrentValue;
            var MinHealth = ComboMenu["minHealth"].Cast<Slider>().CurrentValue;
            if (useW && W.IsReady() && !_Player.IsRecalling() && !_Player.IsInShopRange())
            {
                if (_Player.CountEnemiesInRange(600) >= 1 && _Player.HealthPercent < MinHealth)
                {
                    W.Cast();
                }
            }
        }

        public static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(R.Range) && !e.IsDead))
     	    {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }
				
                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && !EActive())
                {
                    E.Cast();
                }
            }
        }

        public static void Flee()
        {
            var Enemies = EntityManager.Heroes.Enemies.FirstOrDefault(e => e.IsValidTarget(Q.Range));
            if (Enemies != null && Q.IsReady())
            {
                if (Q.IsReady() && Q.IsInRange(Enemies))
                {
                    Q.Cast(Enemies);
                }
            }
			
            if (W.IsReady())
            {
                W.Cast();
            }
        }


        private static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var useW = ComboMenu["dodge"].Cast<CheckBox>().CurrentValue;
            if (useW && !_Player.IsDead && sender.IsEnemy && W.IsReady())
            {
                if (_Player.Distance(sender) <= args.SData.CastRange && sender.IsValidTarget() && Evade[args.SData.Name].Cast<CheckBox>().CurrentValue)
                {
                    W.Cast();
                }

                if (args.SData.Name == "KarthusFallenOne")
                {
                    Core.DelayAction(() => W.Cast(), 2000 - Game.Ping - 200);
                }

                if (args.SData.Name == "ZedR")
                {
                    Core.DelayAction(() => W.Cast(), 2000 - Game.Ping - 200);
                }

                if (args.SData.Name == "SoulShackles")
                {
                    Core.DelayAction(() => W.Cast(), 2000 - Game.Ping - 200);
                }

                if (args.SData.Name == "AbsoluteZero")
                {
                    Core.DelayAction(() => W.Cast(), 2000 - Game.Ping - 200);
                }

                if (args.SData.Name == "NocturneUnspeakableHorror")
                {
                    Core.DelayAction(() => W.Cast(), 2000 - Game.Ping - 200);
                }
            }
        }

        public static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var KsR = KillStealMenu["KsR"].Cast<CheckBox>().CurrentValue;
            var KsE = KillStealMenu["KsE"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(R.Range) && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && !e.IsDead && !e.IsZombie))
            {
                if (KsQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (target.Health + target.AttackShield <= QDamage(target))
                    {
                        Q.Cast(target);
                    }
                }
				
                if (KsR && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    if (target.Health + target.AttackShield <= RDamage(target))
                    {
                        var pred1 = R.GetPrediction(target);
                        if (pred1.HitChance >= HitChance.High)
                        {
                            R.Cast(pred1.CastPosition);
                        }
                    }
                }
				
                if (KsE && E.IsReady() && E.IsInRange(target))
                {
                    if (target.Health + target.AttackShield <= EDamage(target) && !EActive())
                    {
                        E.Cast();
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

        public static void DrawFont(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }
    }
}