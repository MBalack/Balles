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
using Font = SharpDX.Direct3D9.Font;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace Yi
{
    class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, LaneClearMenu, Misc, Items, KillStealMenu, Drawings;
        public static Item Botrk;
        public static Item Bil;
        public static Item Youmuu;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell.Targeted Q;
        public static Spell.Active W;
        public static Spell.Active E;
        public static Spell.Active R;
        public static Spell.Targeted Ignite;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        private static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("MasterYi")) return;
            Chat.Print("Doctor's Yi Loaded!", Color.Orange);
            Bootstrap.Init(null);
            Q = new Spell.Targeted(SpellSlot.Q, 625);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Active(SpellSlot.E);
            R = new Spell.Active(SpellSlot.R);
            Botrk = new Item( ItemId.Blade_of_the_Ruined_King);
            Bil = new Item(3144, 475f);
            Youmuu = new Item(3142, 10);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Menu = MainMenu.AddMenu("Doctor's Yi", "Yi");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            ComboMenu.AddGroupLabel("Ultimate Settings");
            ComboMenu.Add("ComboR", new CheckBox("Use [R] Count Enemies Around"));
            ComboMenu.Add("MinR", new Slider("Min Enemies Use [R]", 2, 1, 5));
            ComboMenu.AddGroupLabel("Use [W] Low HP");
            ComboMenu.Add("minHealth", new Slider("Use [W] If My Hp <=", 50));
            ComboMenu.AddGroupLabel("Use [Q] Dodge Spell");
            ComboMenu.Add("dodge", new CheckBox("Use [Q] Dodge"));
            ComboMenu.Add("antiGap", new CheckBox("Use [Q] Anti Gap"));
            ComboMenu.Add("delay", new Slider("Use [Q] Dodge Delay", 1, 1, 1000));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HarassW", new CheckBox("Use [W] Harass"));
            HarassMenu.Add("HarassE", new CheckBox("Use [E] Harass"));
            HarassMenu.Add("ManaQ", new Slider("Mana Harass", 40));

            LaneClearMenu = Menu.AddSubMenu("Clear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("Clear Settings");
            LaneClearMenu.Add("QLC", new CheckBox("Use [Q] LaneClear"));
            LaneClearMenu.Add("ELC", new CheckBox("Use [E] LaneClear"));
            LaneClearMenu.Add("ManaLC", new Slider("Mana LaneClear", 50));
            LaneClearMenu.AddGroupLabel("JungleClear Settings");
            LaneClearMenu.Add("QJungle", new CheckBox("Use [Q] JungleClear"));
            LaneClearMenu.Add("EJungle", new CheckBox("Use [E] JungleClear"));
            LaneClearMenu.Add("MnJungle", new Slider("Mana JungleClear", 30));

            Misc = Menu.AddSubMenu("Skin Settings", "Misc");
            Misc.AddGroupLabel("Skin Changer");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer"));
            Misc.Add("skin.Id", new ComboBox("Skin Mode", 9, "Default", "1", "2", "3", "4", "5", "6", "7", "8" , "9", "10", "11", "12"));

            Items = Menu.AddSubMenu("Items Settings", "Items");
            Items.AddGroupLabel("Items Settings");
            Items.Add("you", new CheckBox("Use [Youmuu]"));
            Items.Add("BOTRK", new CheckBox("Use [Botrk]"));
            Items.Add("ihp", new Slider("My HP Use BOTRK <=", 50));
            Items.Add("ihpp", new Slider("Enemy HP Use BOTRK <=", 50));

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
            KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));

            Drawings = Menu.AddSubMenu("Draw Settings", "Draw");
            Drawings.AddGroupLabel("Drawing Settings");
            Drawings.Add("DrawQ", new CheckBox("[Q] Range"));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnTick += Game_OnTick;
            Obj_AI_Base.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;
            Orbwalker.OnPostAttack += ResetAttack;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;
            if (Drawings["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
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
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            KillSteal();
            Item();
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
                if ((item && Botrk.IsReady() && Botrk.IsOwned() && Botrk.IsInRange(target)) && (Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp))
                {
                    Botrk.Cast(target);
                }
                if (yous && Youmuu.IsReady() && Youmuu.IsOwned() && _Player.Distance(target) < 325 && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    Youmuu.Cast();
                }
            }
        }

        private static void Gapcloser_OnGapCloser(Obj_AI_Base sender, Gapcloser.GapcloserEventArgs args)
        {
            if (ComboMenu["antiGap"].Cast<CheckBox>().CurrentValue && Q.IsReady() && args.Sender.Distance(_Player) < 325)
            {
                Q.Cast(args.Sender);
            }
        }
		
        public static void Combo()
        {
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var SaveQ = ComboMenu["ComboQ2"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var MinR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q.Range) && !e.IsDead))
     	    {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (target.IsDashing() || _Player.Distance(target) > 325)
                    {
                        Q.Cast(target);
                    }
                }
                if (useE && E.IsReady() && _Player.Distance(target) < 275)
                {
                    E.Cast();
                }
                if (useR && R.IsReady() && _Player.CountEnemiesInRange(525) >= MinR)
                {
                    R.Cast();
	    		}
            }
        }


        public static void JungleClear()
        {
            var useQ = LaneClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["EJungle"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["MnJungle"].Cast<Slider>().CurrentValue;
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters(_Player.ServerPosition, 625).FirstOrDefault(x => x.IsValidTarget(Q.Range));
            if (_Player.ManaPercent < mana) return;
            if (monster != null)
            {
                if (useQ && Q.IsReady())
		    	{
                    Q.Cast(monster);
                }
                if (useE && E.IsReady())
		    	{
                    E.Cast();
                }
            }
        }

        public static void LaneClear()
        {
            var mana = LaneClearMenu["ManaLC"].Cast<Slider>().CurrentValue;
            var useQ = LaneClearMenu["QLC"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["ELC"].Cast<CheckBox>().CurrentValue;
            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(e => e.IsValidTarget(Q.Range)).ToArray();
            if (_Player.ManaPercent < mana) return;
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && _Player.CountEnemyMinionsInRange(625) >= 3)
                {
                    Q.Cast(minion);
                }
                if (useE && E.IsReady() && _Player.CountEnemyMinionsInRange(625) >= 3)
                {
                    E.Cast();
                }
            }
        }

        public static void WLogic()
        {
            var MinHealth = ComboMenu["minHealth"].Cast<Slider>().CurrentValue;
            if (!_Player.IsRecalling() && !_Player.IsInShopRange() && _Player.CountEnemiesInRange(1000) >= 1 && (_Player.HealthPercent < MinHealth || _Player.HasBuff("ZedR")))
            {
                W.Cast();
            }
        }

        public static void ResetAttack(AttackableUnit target, EventArgs args)
        {
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["ManaQ"].Cast<Slider>().CurrentValue;
            var useQC = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            if (useW && W.IsReady() && target.IsValidTarget(_Player.GetAutoAttackRange() - 50) && _Player.ManaPercent > mana && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                W.Cast();
                Orbwalker.ResetAutoAttack();
                Player.IssueOrder(GameObjectOrder.AttackTo, target);
            }
            if (useQC && W.IsReady() && target.IsValidTarget(_Player.GetAutoAttackRange() - 50) && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                W.Cast();
                Orbwalker.ResetAutoAttack();
                Player.IssueOrder(GameObjectOrder.AttackTo, target);
            }
        }

        public static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["ManaQ"].Cast<Slider>().CurrentValue;
            if (_Player.ManaPercent < mana) return;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q.Range) && !e.IsDead))
     	    {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && (target.IsDashing() || _Player.Distance(target) > 300))
                {
                    Q.Cast(target);
                }
                if (useE && E.IsReady() && _Player.Distance(target) < 275)
                {
                    E.Cast();
                }
            }
        }

        public static void Flee()
        {
            var Enemies = EntityManager.Heroes.Enemies.FirstOrDefault(e => e.IsValidTarget(Q.Range));
            var minions = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(m => m.IsValidTarget(Q.Range));
            if (Enemies != null)
            {
                if (Enemies.IsInRange(Game.CursorPos, 200))
                {
                    Q.Cast(Enemies);
                }
            }
            if (minions != null)
            {
                if (minions.IsInRange(Game.CursorPos, 200))
                {
                    Q.Cast(minions);
                }
            }
        }
        public static void QEvade()
        {
            var Enemies = EntityManager.Heroes.Enemies.FirstOrDefault(e => e.IsValidTarget(Q.Range));
            var minions = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(m => m.IsValidTarget(Q.Range));
            if (Enemies != null)
            {
                Core.DelayAction(() => Q.Cast(Enemies), ComboMenu["delay"].Cast<Slider>().CurrentValue);
            }
            if (minions != null)
            {
                Core.DelayAction(() => Q.Cast(minions), ComboMenu["delay"].Cast<Slider>().CurrentValue);
            }
        }

        private static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var useQ = ComboMenu["dodge"].Cast<CheckBox>().CurrentValue;
            if (useQ && sender.IsEnemy && Q.IsReady())
            {
                    if (_Player.Distance(sender) <= args.SData.CastRange)
                    {
                        if (args.SData.Name == "TahmKenchQ")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "KarthusLayWasteA1")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "KarmaQMissileMantra")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "KarmaWMantra")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "KarmaQ")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "JinxW")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "JarvanIVGoldenAegis")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "IllaoiE")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "HeimerdingerUltWDummySpell")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "HeimerdingerUltEDummySpell")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "HecarimUlt")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "GravesQLineSpell")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "GravesQLineMis")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "GravesClusterShot")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "FioraE")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "EliseHumanE")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "EkkoQ")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "EkkoR")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "EkkoW")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "DravenDoubleShot")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "InfectedCleaverMissileCast")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "DariusExecute")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "DariusAxeGrabCone")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "DariusNoxianTacticsONH")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "DariusCleave")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "PhosphorusBomb")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "BraumQ")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "BardQ")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "AatroxQ")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "AatroxE")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "AzirE")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "AzirQWrapper")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "AzirEWrapper")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "AzirQ")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "AzirR")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "Pulverize")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "AhriSeduce")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "CurseoftheSadMummy")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "InfernalGuardian")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "Incinerate")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "Volley")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "EnchantedCrystalArrow")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "BraumRWrapper")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "CassiopeiaPetrifyingGaze")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "FeralScream")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "Rupture")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "EzrealTrueshotBarrage")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "FizzMarinerDoom")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "GnarW")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "GnarBigQMissile")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "GnarQ")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "GnarR")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "GragasR")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "GragasE")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "RiftWalk")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "LeblancSlideM")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "LeblancSlide")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "LeonaSolarFlare")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "UFSlash")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "LuxMaliceCannon")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "LuxLightStrikeKugel")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "LuxLightBinding")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "yasuoq3w")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "VelkozE")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "VeigarEventHorizon")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "VeigarDarkMatter")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "VarusR")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "ThreshQ")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "ThreshE")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "ThreshRPenta")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "SonaR")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "ShenShadowDash")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "SejuaniGlacialPrisonCast")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "RivenMartyr")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "JavelinToss")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "NautilusSplashZone")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "NautilusAnchorDrag")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "NamiR")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "NamiQ")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "DarkBindingMissile")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "StaticField")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "RocketGrab")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "RocketGrabMissile")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "timebombenemybuff")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "NocturneUnspeakableHorror")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "SyndraQ")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "SyndraE")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "SyndraR")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "VayneCondemn")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "Dazzle")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "Overload")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "AbsoluteZero")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "IceBlast")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "LeblancChaosOrb")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "JudicatorReckoning")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "NullLance")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "Crowstorm")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "FiddlesticksDarkWind")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "BrandWildfire")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "Disintegrate")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "FlashFrost")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "Frostbite")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "AkaliMota")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "InfiniteDuress")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "PantheonW")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "blindingdart")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "JayceToTheSkies")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "IreliaEquilibriumStrike")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "maokaiunstablegrowth")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "nautilusgandline")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "runeprison")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "WildCards")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "BlueCardAttack")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "RedCardAttack")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "GoldCardAttack")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "AkaliShadowDance")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "Headbutt")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "PowerFist")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "BrandConflagration")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "CaitlynAceintheHole")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "DianaArc")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "DianaTeleport")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "FizzPiercingStrike")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "GarenQAttack")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "GarenR")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "IreliaGatotsu")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "IreliaEquilibriumStrike")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "JarvanIVCataclysm")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "SowTheWind")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "JaxLeapStrike")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "JaxCounterStrike")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "JaxEmpowerTwo")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "JayceThunderingBlow")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "KarmaSpiritBind")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "KatarinaR")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "JudicatorRighteousFury")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "KennenBringTheLight")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "LissandraW")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "LissandraR")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "LissandraQ")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "LuluQ")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "LuluW")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "LuluE")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "LuluR")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "SeismicShard")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "AlZaharMaleficVisions")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "AlZaharNetherGrasp")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "MaokaiUnstableGrowth")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "MordekaiserMaceOfSpades")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "MordekaiserChildrenOfTheGrave")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "SoulShackles")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "NamiW")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "NasusW")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "NautilusGrandLine")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "PoppyDevastatingBlow")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "PoppyHeroicCharge")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "NocturneParanoia")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "QuinnE")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "PuncturingTaunt")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "RenektonPreExecute")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "SpellFlux")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "SejuaniWintersClaw")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "SkarnerImpale")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "TwoShivPoisen")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "Fling")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "OrianaIzunaCommand")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "OrianaDetonateCommand")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "BusterShot")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "TrundleTrollSmash")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "MockingShout")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "UdyrBearStance")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "UrgotHeatseekingLineMissile")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "UrgotSwap2")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "VeigarBalefulStrike")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "VeigarPrimordialBurst")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "ViR")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "ViktorPowerTransfer")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "VladimirTransfusion")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "VolibearQ")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "HungeringStrike")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "XenZhaoComboTarget")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "XenZhaoSweep")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "YasuoQ3Mis")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "YasuoQ3")
                        {
                            QEvade();
                        }
                        if (args.SData.Name == "YasuoRKnockUpComboW")
                        {
                            QEvade();
                        }
                    }
            }
        }

        public static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(2500) && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && !e.IsDead && !e.IsZombie))
            {
                if (KsQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                    {
                        Q.Cast(target);
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