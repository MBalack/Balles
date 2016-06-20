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

namespace Ezreal7
{
    internal class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, Auto, LaneClearMenu, JungleClearMenu, Misc, Items, KillStealMenu, Drawings;
        public static Item Botrk;
        public static Item Bil;
        public static readonly Item Qss = new Item(ItemId.Quicksilver_Sash);
        public static readonly Item Simitar = new Item(ItemId.Mercurial_Scimitar);
        public static AIHeroClient PlayerInstance
        {
            get { return Player.Instance; }
        }
        private static float HealthPercent()
        {
            return (PlayerInstance.Health / PlayerInstance.MaxHealth) * 100;
        }
        private static readonly Item Tear = new Item(ItemId.Tear_of_the_Goddess);
        private static readonly Item Manamune = new Item(ItemId.Manamune);
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Spell.Targeted Ignite;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
                if (!_Player.ChampionName.Contains("Ezreal")) return;
                Chat.Print("Ezreal7 Loaded!", Color.Orange);
                Bootstrap.Init(null);
                Q = new Spell.Skillshot(SpellSlot.Q, 1150, SkillShotType.Linear, 250, 2000, 60);
                W = new Spell.Skillshot(SpellSlot.W,1000,SkillShotType.Linear,250,1550,80);
                W.AllowedCollisionCount = int.MaxValue;
                E = new Spell.Skillshot(SpellSlot.E,475,SkillShotType.Linear,250,2000,100);
                R= new Spell.Skillshot(SpellSlot.R,5000,SkillShotType.Linear,1000,2000,160);
                R.AllowedCollisionCount = int.MaxValue;
                Botrk = new Item( ItemId.Blade_of_the_Ruined_King);
                Bil = new Item(3144, 475f);
                Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
                Menu = MainMenu.AddMenu("Ezreal7", "Ezreal");
                Menu.AddGroupLabel("Doctor7");
                ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
                ComboMenu.AddGroupLabel("Combo Settings");
                ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
                ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
                ComboMenu.AddGroupLabel("Ultimate Settings");
                ComboMenu.Add("ComboR", new CheckBox("Use [R] Combo"));
                ComboMenu.AddSeparator();
                ComboMenu.Add("MinRangeR", new Slider("Min Distance Cast [R]", 1000, 0, 5000));
                ComboMenu.AddSeparator();
                ComboMenu.Add("MaxRangeR", new Slider("Max Distance Cast [R]", 3000, 0, 5000));
                ComboMenu.AddSeparator();
                ComboMenu.Add("MinR", new Slider("Min Enemies Use [R]", 2, 0, 5));

                HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
                HarassMenu.AddGroupLabel("Harass Settings");
                HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
                HarassMenu.Add("ManaQ", new Slider("Min Mana Harass [Q]", 40));
                HarassMenu.AddSeparator();
                HarassMenu.Add("HarassW", new CheckBox("Use [W] Harass", false) );
                HarassMenu.Add("ManaW", new Slider("Min Mana Harass [W]<=", 40));
                HarassMenu.AddSeparator();
                HarassMenu.AddGroupLabel("Harass On");
                foreach (var enemies in EntityManager.Heroes.Enemies)
                {
                    HarassMenu.Add("haras" + enemies.ChampionName, new CheckBox("" + enemies.ChampionName));
                }

                Auto = Menu.AddSubMenu("Auto Harass Settings", "Auto Harass");
				Auto.AddGroupLabel("Auto Harass Settings");
                Auto.Add("AutoQ", new CheckBox("Auto Harass [Q]"));
                Auto.Add("AutomanaQ", new Slider("Min Mana Auto [Q]", 60));
                Auto.AddSeparator();
                Auto.Add("AutoW", new CheckBox("Auto Harass [W]", false));
                Auto.Add("AutomanaW", new Slider("Min Mana Auto [W]", 60));
                Auto.AddSeparator();
                Auto.AddGroupLabel("Auto Harass On");
                foreach (var enemies in EntityManager.Heroes.Enemies)
                {
                    Auto.Add("harass" + enemies.ChampionName, new CheckBox("" + enemies.ChampionName));
                }

                LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
                LaneClearMenu.AddGroupLabel("LastHit Settings");
                LaneClearMenu.Add("LastQ", new CheckBox("Always [Q] LastHit"));
                LaneClearMenu.Add("LhMana", new Slider("Min Mana Lasthit [Q]", 60));
                LaneClearMenu.AddSeparator();
                LaneClearMenu.Add("LhAA", new CheckBox("[Q] LastHit If Out Range AA", false));
                LaneClearMenu.Add("AAMana", new Slider("Min Mana Lasthit [Q] If Out Range AA", 50));
                LaneClearMenu.AddSeparator();
                LaneClearMenu.AddGroupLabel("Lane Clear Settings");
                LaneClearMenu.Add("LastQLC", new CheckBox("Always LaneClear With [Q]", false));
                LaneClearMenu.Add("ManaLC", new Slider("Min Mana LaneClear With [Q]", 70));
                LaneClearMenu.AddSeparator();
                LaneClearMenu.Add("LastAA", new CheckBox("[Q] LaneClear If Out Range AA"));
                LaneClearMenu.Add("ManaLA", new Slider("Min Mana LastHit [Q] If Out Range AA", 50));

                JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
                JungleClearMenu.AddGroupLabel("JungleClear Settings");
                JungleClearMenu.Add("QJungle", new CheckBox("Use [Q] JungleClear"));
                JungleClearMenu.Add("MnJungle", new Slider("Min Mana JungleClear [Q]", 30));

                Misc = Menu.AddSubMenu("Misc Settings", "Misc");
                Misc.AddGroupLabel("AntiGap Settings");
                Misc.Add("AntiGap", new CheckBox("Use [E] AntiGapcloser"));
                Misc.AddGroupLabel("Ultimate On CC Settings");
                Misc.Add("Rstun", new CheckBox("Use [R] If Enemy Has CC"));
                Misc.AddGroupLabel("Auto Stacks Settings");
                Misc.Add("Stack", new CheckBox("Auto Stacks In Shop"));
                Misc.AddGroupLabel("Skin Changer");
                Misc.Add("checkSkin", new CheckBox("Use Skin Changer"));
                Misc.Add("skin.Id", new ComboBox("Skin Mode", 8, "Default", "1", "2", "3", "4", "5", "6", "7", "8"));

                Items = Menu.AddSubMenu("Items Settings", "Items");
                Items.AddGroupLabel("Items Settings");
                Items.Add("BOTRK", new CheckBox("Use [Botrk]"));
                Items.Add("ihp", new Slider("My HP Use BOTRK <=", 50));
                Items.Add("ihpp", new Slider("Enemy HP Use BOTRK <=", 50));
                Items.AddGroupLabel("Qss Settings");
                Items.Add("Qss", new CheckBox("Use Qss"));
                Items.AddGroupLabel("Qss On CC");
                Items.Add("stun", new CheckBox("Stuns"));
                Items.Add("rot", new CheckBox("Root"));
                Items.Add("tunt", new CheckBox("Taunt"));
                Items.Add("snare", new CheckBox("Snare"));
                Items.Add("charm", new CheckBox("Charm", false));
                Items.Add("slow", new CheckBox("Slows", false));
                Items.Add("blind", new CheckBox("Blinds", false));
                Items.Add("fear", new CheckBox("Fear", false));
                Items.Add("silence", new CheckBox("Silence", false));
                Items.Add("supperss", new CheckBox("Supperss", false));
                Items.Add("poly", new CheckBox("Polymorph", false));
                Items.Add("delay", new Slider("Humanizer Qss Delay", 0, 0, 1500));

                KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
                KillStealMenu.AddGroupLabel("KillSteal Settings");
                KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
                KillStealMenu.Add("KsW", new CheckBox("Use [W] KillSteal"));
                KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));
                KillStealMenu.AddSeparator();
                KillStealMenu.AddGroupLabel("Ultimate Settings");
                KillStealMenu.Add("KsR", new CheckBox("Use [R] KillSteal"));
                KillStealMenu.Add("minKsR", new Slider("Min [R] Distance KillSteal", 900, 1, 5000));
                KillStealMenu.Add("RKb", new KeyBind("[R] KillSteal Key", false, KeyBind.BindTypes.HoldActive, 'T'));

                Drawings = Menu.AddSubMenu("Draw Settings", "Draw");
                Drawings.AddGroupLabel("Drawing Settings");
                Drawings.Add("DrawQ", new CheckBox("Q Range"));
                Drawings.Add("DrawW", new CheckBox("W Range", false));
                Drawings.Add("DrawE", new CheckBox("E Range", false));
                Drawings.Add("Notifications", new CheckBox("Notifications Can Kill R"));

                Drawing.OnDraw += Drawing_OnDraw;
                Game.OnTick += Game_OnTick;
                Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Drawings["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (Drawings["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (Drawings["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
            if (Drawings["Notifications"].Cast<CheckBox>().CurrentValue && R.IsReady())
            {
                var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                if (target.IsValidTarget(1800))
                {
                    if (Player.Instance.GetSpellDamage(target, SpellSlot.R) > target.Health + target.AttackShield)
                    {
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, Color.Red, " Useeeeee RRRRRRRR Cannnnnnnn Killlllllllll: " + target.ChampionName);
                    }
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
            Stacks();
            AutoHarass();
            RStun();
            Item();
            Qsss();
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
            var Minhp = Items["ihp"].Cast<Slider>().CurrentValue;
            var Minhpp = Items["ihpp"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(550, DamageType.Physical);
            if (target != null)
            {
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

        public static void CastQss()
        {
            if (Qss.IsOwned() && Qss.IsReady())
            {
                Qss.Cast();
            }

            if (Simitar.IsOwned() && Simitar.IsReady())
            {
                Simitar.Cast();
            }
        }

        private static void Qsss()
        {
            if (!Items["Qss"].Cast<CheckBox>().CurrentValue) return;
            if (Items["snare"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Snare))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["tunt"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Taunt))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["stun"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Stun))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["poly"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Polymorph))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["blind"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Blind))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["fear"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Fear))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["charm"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Charm))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["supperss"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Suppression))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["silence"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Silence))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["rot"].Cast<CheckBox>().CurrentValue && _Player.IsRooted)
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["slow"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Slow))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
        }
		
        public static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var MaxRangeR = ComboMenu["MaxRangeR"].Cast<Slider>().CurrentValue;
            var MinRangeR = ComboMenu["MinRangeR"].Cast<Slider>().CurrentValue;
            var MinR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
            if (target != null)
     	    {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    var Qpred = Q.GetPrediction(target);
                    if (Qpred.HitChance >= HitChance.High)
                    {
                        Q.Cast(Qpred.CastPosition);
                    }
                }
                if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    var Wpred = W.GetPrediction(target);
                    if (Wpred.HitChance >= HitChance.High)
                    {
                        W.Cast(Wpred.CastPosition);
                    }
	    		}

                if (useR && R.IsReady() && target.IsValidTarget(R.Range) && target.IsInRange(Player.Instance, MaxRangeR) && !target.IsInRange(Player.Instance, MinRangeR))
                {
                    var pred = R.GetPrediction(target);
                    if (pred.CastPosition.CountEnemiesInRange(R.Width) > MinR && pred.HitChance >= HitChance.High)
                    {
                        R.Cast(pred.CastPosition);
                    }
                }
            }
        }

        public static void JungleClear()
        {

            var useQ = JungleClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["MnJungle"].Cast<Slider>().CurrentValue;
            var jungleMonsters = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(Q.Range));
            if (jungleMonsters != null)
            {
                if (useQ && Q.IsReady() && Q.IsInRange(jungleMonsters) && Player.Instance.ManaPercent >= mana)
		    	{
                    if (Player.Instance.GetAutoAttackRange() >= jungleMonsters.Distance(Player.Instance))
                    {
                        Q.Cast(jungleMonsters);
                    }
                }
            }
        }

        private static void Flee()
        {
            if (E.IsReady())
            {
                var cursorPos = Game.CursorPos;
                var castPos = Player.Instance.Position.Distance(cursorPos) <= E.Range ? cursorPos : Player.Instance.Position.Extend(cursorPos, E.Range).To3D();
                E.Cast(castPos);
            }
        }

        public static void LaneClear()
        {
            var laneQMN = LaneClearMenu["ManaLC"].Cast<Slider>().CurrentValue;
            var laneQAA = LaneClearMenu["ManaLA"].Cast<Slider>().CurrentValue;
            var useQLH = LaneClearMenu["LastQLC"].Cast<CheckBox>().CurrentValue;
            var useAA = LaneClearMenu["LastAA"].Cast<CheckBox>().CurrentValue;
            var minions = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(m => m.IsValidTarget(Q.Range) && (Player.Instance.GetSpellDamage(m, SpellSlot.Q) >= m.TotalShieldHealth() && m.IsEnemy && !m.IsDead && m.IsValid));
            if (minions != null)
            {
                if (Player.Instance.ManaPercent >= laneQMN)
                {
                    if (useQLH && Q.IsReady() && _Player.GetAutoAttackDamage(minions) < minions.TotalShieldHealth())
                    {
                        Q.Cast(minions);
                    }
                }
                else if (Player.Instance.ManaPercent >= laneQAA && Player.Instance.GetAutoAttackRange() <= minions.Distance(Player.Instance))
                {
                    if (useAA && Q.IsReady())
                    {
                        Q.Cast(minions);
                    }
                }
            }
        }

        public static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var ManaQ = HarassMenu["ManaQ"].Cast<Slider>().CurrentValue;
            var ManaW = HarassMenu["ManaW"].Cast<Slider>().CurrentValue;
            var Selector = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var enemies = EntityManager.Heroes.Enemies.OrderByDescending
                (a => a.HealthPercent).Where(a => !a.IsMe && a.IsValidTarget() && a.Distance(_Player) <= 1050);
            if (useQ && Player.Instance.ManaPercent >= ManaQ && Q.IsReady() && Selector.IsValidTarget(Q.Range))
            {
                if (Selector != null)
                {
                    var Qpred = Q.GetPrediction(Selector);
				    foreach (var qenemies in enemies)
					{
                        if (HarassMenu["haras" + qenemies.ChampionName].Cast<CheckBox>().CurrentValue && Qpred.HitChancePercent >= 80)
                        {
                            Q.Cast(Qpred.CastPosition);
                        }
                    }
                }
            }
            if (useW && Player.Instance.ManaPercent >= ManaW && W.IsReady() && Selector.IsValidTarget(W.Range))
            {
                if (Selector != null)
                {
                    var Wpred = W.GetPrediction(Selector);
				    foreach (var qenemies in enemies)
					{
                        if (HarassMenu["haras" + qenemies.ChampionName].Cast<CheckBox>().CurrentValue && Wpred.HitChancePercent >= 80)
                        {
                            W.Cast(Wpred.CastPosition);
                        }
                    }
                }
            }
        }

        public static void LastHit()
        {
            var useQ = LaneClearMenu["LastQ"].Cast<CheckBox>().CurrentValue;
            var useAA = LaneClearMenu["LhAA"].Cast<CheckBox>().CurrentValue;
            var LAA = LaneClearMenu["AAMana"].Cast<Slider>().CurrentValue;
            var LhM = LaneClearMenu["LhMana"].Cast<Slider>().CurrentValue;
            var minion = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(m => m.IsValidTarget(Q.Range) && (Player.Instance.GetSpellDamage(m, SpellSlot.Q) >= m.TotalShieldHealth() && m.IsEnemy && !m.IsDead && m.IsValid));
            if (minion != null)
            {
                if (Player.Instance.ManaPercent >= LhM && _Player.GetAutoAttackDamage(minion) < minion.TotalShieldHealth())
                {
                    if (useQ && Q.IsReady())
                    {
                        Q.Cast(minion);
                    }
                }
                else if (Player.Instance.ManaPercent >= LAA && Player.Instance.GetAutoAttackRange() <= minion.Distance(Player.Instance))
                {
                    if (useAA && Q.IsReady())
                    {
                        Q.Cast(minion);
                    }
                }
            }
        }

        public static void AutoHarass()
        {
            var useQ = Auto["AutoQ"].Cast<CheckBox>().CurrentValue;
            var useW = Auto["AutoW"].Cast<CheckBox>().CurrentValue;
            var automana = Auto["AutomanaQ"].Cast<Slider>().CurrentValue;
            var automanaw = Auto["AutomanaW"].Cast<Slider>().CurrentValue;
            var Selector = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var enemies = EntityManager.Heroes.Enemies.OrderByDescending
                (a => a.HealthPercent).Where(a => !a.IsMe && a.IsValidTarget() && a.Distance(_Player) <= 1000);
            if (useQ && Q.IsReady() && Selector.IsValidTarget(Q.Range) && automana <= Player.Instance.ManaPercent && !Tru() && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                if (Selector != null)
                {
                    var predQ = Q.GetPrediction(Selector);
				    foreach (var qenemies in enemies)
					{
                        if (Auto["harass" + qenemies.ChampionName].Cast<CheckBox>().CurrentValue && predQ.HitChancePercent >= 80)
                        {
                            Q.Cast(predQ.CastPosition);
                        }
                    }
                }
            }

            if (useW && W.IsReady() && Selector.IsValidTarget(W.Range) && automanaw <= Player.Instance.ManaPercent && !Tru() && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                if (Selector != null)
                {
                    var predW = W.GetPrediction(Selector);
				    foreach (var qenemies in enemies)
					{
                        if (Auto["harass" + qenemies.ChampionName].Cast<CheckBox>().CurrentValue && predW.HitChancePercent >= 80)
                        {
                            W.Cast(predW.CastPosition);
                        }
                    }
                }
            }
        }
// Thanks MarioGK has allowed me to use some his logic
        public static void RStun()
		{
            var Rstun = Misc["Rstun"].Cast<CheckBox>().CurrentValue;
            if (Rstun && R.IsReady())
            {
                var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                if (target != null)
                {
                    if (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup) && target.IsInRange(Player.Instance, 2000) && !target.IsInRange(Player.Instance, 800))
                    {
                        R.Cast(target.Position);
                    }
                }
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (Misc["AntiGap"].Cast<CheckBox>().CurrentValue && E.IsReady() && sender.IsEnemy && sender.IsVisible && sender.IsInRange(Player.Instance, E.Range))
            {
                E.Cast(Player.Instance.Position.Shorten(sender.Position, E.Range));
            }
        }

        public static bool Tru()
        {
            var player = Player.Instance;
            return ObjectManager.Get<Obj_AI_Turret>().Any(turret => turret.Team != _Player.Team && turret.Health > 0 && turret.Distance(_Player) < 1000);
        }

        public static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var KsW = KillStealMenu["KsW"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(R.Range) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie && (hero.HealthPercent <= 25)))
            {
                if (KsQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                    {
                        var Qpred = Q.GetPrediction(target);
                        if (Qpred.HitChancePercent >= 70)
                        {
                            Q.Cast(Qpred.CastPosition);
                        }
                    }
                }

                if (KsW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.W))
                    {
                        var Wpred = W.GetPrediction(target);
                        if (Wpred.HitChancePercent >= 70)
                        {
                            W.Cast(Wpred.CastPosition);
                        }
                    }
                }
                var KsR = KillStealMenu["KsR"].Cast<CheckBox>().CurrentValue;
                var minKsR = KillStealMenu["minKsR"].Cast<Slider>().CurrentValue;
                if (KsR && R.IsReady())
                {
                    if (target != null)
                    {
                        if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.R) && target.IsInRange(Player.Instance, 3500) && !target.IsInRange(Player.Instance, minKsR))
                        {
                            var pred = R.GetPrediction(target);
                            if (pred.HitChancePercent >= 90)
                            {
                                R.Cast(pred.CastPosition);
                            }
                        }
                    }
                }
                if (R.IsReady() && KillStealMenu["RKb"].Cast<KeyBind>().CurrentValue)
                {
                    if (target != null)
                    {
                        if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.R))
                        {
                            var pred = R.GetPrediction(target);
                            if (pred.HitChancePercent >= 70)
                            {
                                R.Cast(pred.CastPosition);
                            }
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
        
		public static void Stacks()
		{	
            if (Misc["Stack"].Cast<CheckBox>().CurrentValue && Q.IsReady() &&
            (Player.Instance.IsInShopRange()) && (Tear.IsOwned() || Manamune.IsOwned()))
            {
                Q.Cast(Game.CursorPos);
            }
        }
    }
}
