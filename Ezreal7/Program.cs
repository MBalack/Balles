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

namespace Ezreal7
{
    internal class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, Auto, LaneClearMenu, JungleClearMenu, Misc, Items, KillStealMenu, Drawings;
        public static Item Botrk;
        public static Item Bil;
        public static Font Thm;
        public static Font Thn;
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
            Thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 32, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Thn = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 15, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Menu = MainMenu.AddMenu("Ezreal7", "Ezreal");
            Menu.AddGroupLabel("Doctor7");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.AddGroupLabel("Ultimate Settings");
            ComboMenu.Add("ComboR", new CheckBox("Use [R] Combo"));
            ComboMenu.Add("MinR", new Slider("Min Enemies Use [R]", 2, 0, 5));
            ComboMenu.AddSeparator();
            ComboMenu.Add("MinRangeR", new Slider("Min Distance Cast [R]", 1000, 0, 5000));
            ComboMenu.AddLabel("Recommended Distance 1000");

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("ManaQ", new Slider("Min Mana Harass [Q]", 40));
            HarassMenu.AddSeparator();
            HarassMenu.Add("HarassW", new CheckBox("Use [W] Harass", false) );
            HarassMenu.Add("ManaW", new Slider("Min Mana Harass [W]<=", 40));
            HarassMenu.AddSeparator();
            HarassMenu.AddGroupLabel("Harass On");
            foreach (var target in EntityManager.Heroes.Enemies)
            {
                HarassMenu.Add("haras" + target.ChampionName, new CheckBox("" + target.ChampionName));
            }

            Auto = Menu.AddSubMenu("Auto Harass Settings", "Auto Harass");
			Auto.AddGroupLabel("Auto Harass Settings");
            Auto.Add("Key", new KeyBind("Auto Harass", false, KeyBind.BindTypes.PressToggle, 'H'));
            Auto.Add("AutoQ", new CheckBox("Use [Q]"));
            Auto.Add("AutomanaQ", new Slider("Min Mana Auto [Q]", 60));
            Auto.AddSeparator();
            Auto.Add("AutoW", new CheckBox("Use [W]", false));
            Auto.Add("AutomanaW", new Slider("Min Mana Auto [W]", 60));
            Auto.AddSeparator();
            Auto.AddGroupLabel("Autu Harass On");
            foreach (var target in EntityManager.Heroes.Enemies)
            {
                Auto.Add("harass" + target.ChampionName, new CheckBox("" + target.ChampionName));
            }

            LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("LastHit Settings");
            LaneClearMenu.Add("LastQ", new CheckBox("Always [Q] LastHit", false));
            LaneClearMenu.Add("LhAA", new CheckBox("Only [Q] If Orbwalker Cant Killable Minion"));
            LaneClearMenu.Add("LhMana", new Slider("Min Mana Lasthit [Q]", 60));
            LaneClearMenu.AddSeparator();
            LaneClearMenu.AddGroupLabel("Lane Clear Settings");
            LaneClearMenu.Add("LastQLC", new CheckBox("Always LaneClear With [Q]", false));
            LaneClearMenu.Add("CantLC", new CheckBox("Only [Q] If Orbwalker Cant Killable Minion"));
            LaneClearMenu.Add("ManaLC", new Slider("Min Mana LaneClear With [Q]", 70));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("QJungle", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("MnJungle", new Slider("Min Mana JungleClear [Q]", 30));

            Misc = Menu.AddSubMenu("Misc Settings", "Misc");
            Misc.AddGroupLabel("AntiGap Settings");
            Misc.Add("AntiGap", new CheckBox("Use [E] AntiGapcloser", false));
            Misc.AddGroupLabel("Ultimate On CC Settings");
            Misc.Add("Rstun", new CheckBox("Use [R] Enemy Immobile"));
            Misc.AddGroupLabel("Auto Stacks Settings");
            Misc.Add("Stack", new CheckBox("Auto Stacks In Shop"));
            Misc.Add("Stackk", new CheckBox("Auto Stacks If Enemies Around = 0", false));
            Misc.Add("Stackkm", new Slider("Min Mana Auto Stack", 80));
            Misc.AddGroupLabel("Skin Changer");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer", false));
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
            KillStealMenu.AddLabel("Recommended Distance 900");
            KillStealMenu.Add("RKb", new KeyBind("[R] Semi Manual Key", false, KeyBind.BindTypes.HoldActive, 'T'));

            Drawings = Menu.AddSubMenu("Draw Settings", "Draw");
            Drawings.AddGroupLabel("Drawing Settings");
            Drawings.Add("DrawQ", new CheckBox("[Q] Range"));
            Drawings.Add("DrawW", new CheckBox("[W] Range", false));
            Drawings.Add("DrawE", new CheckBox("[E] Range", false));
            Drawings.Add("Notifications", new CheckBox("Notifications Can Kill [R]"));
            Drawings.Add("DrawAT", new CheckBox("Draw Auto Harass"));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnTick += Game_OnTick;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Orbwalker.OnUnkillableMinion += Orbwalker_CantLasthit;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;
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
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (target.IsValidTarget(1600) && Player.Instance.GetSpellDamage(target, SpellSlot.R) > target.Health + target.AttackShield)
                {
                    DrawFont(Thm, "R Can Killable " + target.ChampionName, (float)(ft[0] - 140), (float)(ft[1] + 80), SharpDX.Color.Red);
                }
            }
            if (Drawings["DrawAT"].Cast<CheckBox>().CurrentValue)
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (Auto["Key"].Cast<KeyBind>().CurrentValue)
                {
                    DrawFont(Thn, "Auto Harass : Enable", (float)(ft[0] - 60), (float)(ft[1] + 20), SharpDX.Color.White);
                }
            }
        }

        public static void DrawFont(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
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
                if ((item && Botrk.IsReady() && Botrk.IsOwned() && target.IsValidTarget(550)) && (Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp))
                {
                    Botrk.Cast(target);
                }
            }
        }

        public static void CastQss()
        {
            if (Qss.IsOwned() && Qss.IsReady() && _Player.CountEnemiesInRange(1000) >= 1)
            {
                Core.DelayAction(() => Qss.Cast(), Items["delay"].Cast<Slider>().CurrentValue);
            }

            if (Simitar.IsOwned() && Simitar.IsReady() && _Player.CountEnemiesInRange(1000) >= 1)
            {
                Core.DelayAction(() => Simitar.Cast(), Items["delay"].Cast<Slider>().CurrentValue);
            }
        }

        private static void Qsss()
        {
            if (!Items["Qss"].Cast<CheckBox>().CurrentValue) return;
            if (Items["snare"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Snare))
            {
                CastQss();
            }
            if (Items["tunt"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Taunt))
            {
                CastQss();
            }
            if (Items["stun"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Stun))
            {
                CastQss();
            }
            if (Items["poly"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Polymorph))
            {
                CastQss();
            }
            if (Items["blind"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Blind))
            {
                CastQss();
            }
            if (Items["fear"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Fear))
            {
                CastQss();
            }
            if (Items["charm"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Charm))
            {
                CastQss();
            }
            if (Items["supperss"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Suppression))
            {
                CastQss();
            }
            if (Items["silence"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Silence))
            {
                CastQss();
            }
            if (Items["rot"].Cast<CheckBox>().CurrentValue && _Player.IsRooted)
            {
                CastQss();
            }
            if (Items["slow"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Slow))
            {
                CastQss();
            }
        }
		
        public static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
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

                if (useR && R.IsReady() && target.IsValidTarget(R.Range) && target.IsInRange(Player.Instance, 2500) && !target.IsInRange(Player.Instance, MinRangeR))
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
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters(_Player.ServerPosition, 700).FirstOrDefault(x => x.IsValidTarget(Q.Range));
            if (monster != null)
            {
                if (useQ && Q.IsReady() && Player.Instance.ManaPercent >= mana)
		    	{
                    Q.Cast(monster);
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
            var useQLH = LaneClearMenu["LastQLC"].Cast<CheckBox>().CurrentValue;
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
            }
        }

        private static void Orbwalker_CantLasthit(Obj_AI_Base target, Orbwalker.UnkillableMinionArgs args)
        {
            var useCant = LaneClearMenu["CantLC"].Cast<CheckBox>().CurrentValue;
            var laneQMN = LaneClearMenu["ManaLC"].Cast<Slider>().CurrentValue;
            var useAA = LaneClearMenu["LhAA"].Cast<CheckBox>().CurrentValue;
            var LhM = LaneClearMenu["LhMana"].Cast<Slider>().CurrentValue;
            var unit = (useCant && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && Player.Instance.ManaPercent >= laneQMN)
            || (useAA  && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit)&& Player.Instance.ManaPercent >= LhM);
            if (target == null) return;
            if (unit && Q.IsReady() && target.IsValidTarget(Q.Range))
            {
                if (Player.Instance.GetSpellDamage(target, SpellSlot.Q) >= Prediction.Health.GetPrediction(target, Q.CastDelay))
                {
                    Q.Cast(target);
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
            if (Selector != null)
            {
                if (useQ && Player.Instance.ManaPercent >= ManaQ && Q.IsReady() && Selector.IsValidTarget(Q.Range))
                {
                    var Qpred = Q.GetPrediction(Selector);
                    if (HarassMenu["haras" + Selector.ChampionName].Cast<CheckBox>().CurrentValue && Qpred.HitChancePercent >= 80)
                    {
                        Q.Cast(Qpred.CastPosition);
                    }
                }
                if (useW && Player.Instance.ManaPercent >= ManaW && W.IsReady() && Selector.IsValidTarget(W.Range))
                {
                    var Wpred = W.GetPrediction(Selector);
                    if (HarassMenu["haras" + Selector.ChampionName].Cast<CheckBox>().CurrentValue && Wpred.HitChancePercent >= 80)
                    {
                        W.Cast(Wpred.CastPosition);
                    }
                }
            }
        }

        public static void LastHit()
        {
            var useQ = LaneClearMenu["LastQ"].Cast<CheckBox>().CurrentValue;
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
            }
        }

        public static void AutoHarass()
        {
            var useQ = Auto["AutoQ"].Cast<CheckBox>().CurrentValue;
            var useW = Auto["AutoW"].Cast<CheckBox>().CurrentValue;
            var key = Auto["Key"].Cast<KeyBind>().CurrentValue;
            var automana = Auto["AutomanaQ"].Cast<Slider>().CurrentValue;
            var automanaw = Auto["AutomanaW"].Cast<Slider>().CurrentValue;
            var Selector = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (Selector == null) return;
            if (key && Selector.IsValidTarget(W.Range) && !Tru(_Player.Position) && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                if (useQ && Q.IsReady() && automana <= Player.Instance.ManaPercent)
                {
                    var predQ = Q.GetPrediction(Selector);
                    if (Auto["harass" + Selector.ChampionName].Cast<CheckBox>().CurrentValue && predQ.HitChancePercent >= 80)
                    {
                        Q.Cast(predQ.CastPosition);
                    }
                }
                if (useW && W.IsReady() && automanaw <= Player.Instance.ManaPercent)
                {
                    var predW = W.GetPrediction(Selector);
                    if (Auto["harass" + Selector.ChampionName].Cast<CheckBox>().CurrentValue && predW.HitChancePercent >= 80)
                    {
                        W.Cast(predW.CastPosition);
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
                var target = TargetSelector.GetTarget(1600, DamageType.Physical);
                if (target != null)
                {
                    if ((!target.IsInRange(Player.Instance, 800)) && (target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup)))
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

        public static bool Tru(Vector3 position)
        {
            return ObjectManager.Get<Obj_AI_Turret>().Any(turret => turret.IsValidTarget(950) && turret.IsEnemy);
        }

        public static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var KsW = KillStealMenu["KsW"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(R.Range) && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && !e.IsDead && !e.IsZombie))
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
            var mana = Misc["Stackkm"].Cast<Slider>().CurrentValue;
            if (Misc["Stackk"].Cast<CheckBox>().CurrentValue && Q.IsReady() &&
            (!Player.Instance.IsInShopRange() && _Player.CountEnemiesInRange(1400) <= 0 && !_Player.IsRecalling() && Player.Instance.ManaPercent >= mana && !EntityManager.MinionsAndMonsters.CombinedAttackable.Any(x => x.IsValidTarget(Q.Range))
            && (Tear.IsOwned() || Manamune.IsOwned())))
            {
                Q.Cast(Game.CursorPos);
            }
        }
    }
}
