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

namespace Graves7
{
    internal static class Program
    {
        private static Menu Menu, ComboMenu, HarassMenu, ClearMenu, JungleMenu, Drawings, KillStealMenu, Items, Misc;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Item Botrk;
        public static Item Bil;
        public static Item Youmuu;
        public static Font Thm;
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Slider ComboMode;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if(Player.Instance.Hero != Champion.Graves) return;
            Chat.Print("Doctor's Graves Loaded!", Color.Orange);
            Q = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Linear, 250, 2000, 60);
            Q.AllowedCollisionCount = int.MaxValue;
            W = new Spell.Skillshot(SpellSlot.W, 950, SkillShotType.Circular, 250, 1650, 150);
            W.AllowedCollisionCount = int.MaxValue;
            E = new Spell.Skillshot(SpellSlot.E, 425, SkillShotType.Linear);
            R = new Spell.Skillshot(SpellSlot.R, 1500, SkillShotType.Linear, 250, 2100, 100);
            R.AllowedCollisionCount = int.MaxValue;
            Thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 32, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Botrk = new Item(ItemId.Blade_of_the_Ruined_King);
            Bil = new Item(3144, 475f);
            Youmuu = new Item(3142, 10);
            Menu = MainMenu.AddMenu("Graves","Graves7");
            Menu.AddGroupLabel("Doctor7");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "ComboMenu");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
			ComboMode = ComboMenu.Add("comboMode", new Slider("Min Stack Use [E] Reload", 1, 0, 1));
            ComboMenu.Add("ComboR", new CheckBox("Use [R] Aoe In Combo", false));
            ComboMenu.Add("MinR", new Slider("Min Enemies Use [R]", 2, 1, 5));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "HarassMenu");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.AddLabel("Harass [Q] On");
            foreach (var Selector in EntityManager.Heroes.Enemies)
            {
                HarassMenu.Add("haras" + Selector.ChampionName, new CheckBox("" + Selector.ChampionName));
            }
            HarassMenu.Add("HarassMana", new Slider("Min Mana Harass [Q]", 50));
            HarassMenu.AddSeparator();
            HarassMenu.AddGroupLabel("Spells Settings");
            HarassMenu.Add("HarassW", new CheckBox("Use [W]", false));
            HarassMenu.Add("ManaW", new Slider("Mana Harass [W]", 40));
            HarassMenu.Add("HarassAA", new CheckBox("Use [E] Reset AA", false));
            HarassMenu.Add("ManaHarass", new Slider("Mana [E] Harass", 50));	

            ClearMenu = Menu.AddSubMenu("Laneclear Settings", "LaneClearMenu");
            ClearMenu.AddGroupLabel("Laneclear Settings");
            ClearMenu.Add("QClear", new CheckBox("Use [Q]"));
            ClearMenu.Add("minQ", new Slider("Min Hit Minion [Q]", 3, 1, 6));		
            ClearMenu.Add("ClearMana", new Slider("Min Mana For [Q] LaneClear", 70));
            ClearMenu.Add("LaneAA", new CheckBox("Use [E] Reset AA", false));
            ClearMenu.Add("ELane", new Slider("Min Mana For [E] LaneClear", 70));			

            JungleMenu = Menu.AddSubMenu("JungleClear Settings", "JungleMenu");
            JungleMenu.AddGroupLabel("JungleClear Settings");
            JungleMenu.Add("QJungleClear", new CheckBox("Use [Q]"));
            JungleMenu.Add("JungleMana", new Slider("Min Mana For [Q] JungleClear", 30));
            JungleMenu.Add("WJungleClear", new CheckBox("Use [W]"));
            JungleMenu.Add("JungleManaW", new Slider("Min Mana For [W] JungleClear", 50));
            JungleMenu.Add("JungleAA", new CheckBox("Use [E]"));
            JungleMenu.Add("EJung", new Slider("Min Mana For [E] JungleClear", 30));
			
            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillStealMenu");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
            KillStealMenu.Add("KsW", new CheckBox("Use [W] KillSteal"));
            KillStealMenu.AddSeparator();
            KillStealMenu.AddLabel("Ultimate Settings");
            KillStealMenu.Add("KsR", new CheckBox("Use [R] KillSteal"));
            KillStealMenu.Add("minKsR", new Slider("Min [R] Range KillSteal", 100, 1, 1500));
            KillStealMenu.Add("RKb", new KeyBind("[R] Semi Manual Key", false, KeyBind.BindTypes.HoldActive, 'T'));

            Items = Menu.AddSubMenu("Items Settings", "Items");
            Items.AddGroupLabel("Items Settings");
            Items.Add("you", new CheckBox("Use [Youmuu]"));
            Items.Add("BOTRK", new CheckBox("Use [Botrk]"));
            Items.Add("ihp", new Slider("My HP Use BOTRK <=", 50));
            Items.Add("ihpp", new Slider("Enemy HP Use BOTRK <=", 50));
			
            Misc = Menu.AddSubMenu("Misc Settings", "Misc");
            Misc.AddLabel("Misc Settings");
            Misc.Add("AntiGap", new CheckBox("Use [E] AntiGap"));
            Misc.Add("AntiGapW", new CheckBox("Use [W] AntiGap"));
            Misc.Add("QStun", new CheckBox("Use [Q] Immoblie"));
            Misc.AddLabel("Skin Changer");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Misc.Add("skin.Id", new ComboBox("Skin Mode", 3, "Default", "1", "2", "3", "4", "5", "6", "7", "8"));

            Drawings = Menu.AddSubMenu("Drawings Settings", "DrawingMenu");
            Drawings.AddGroupLabel("Drawings");
            Drawings.Add("DrawQ", new CheckBox("[Q] Range"));
            Drawings.Add("DrawW", new CheckBox("[W] Range", false));
            Drawings.Add("DrawE", new CheckBox("[E] Range", false));
            Drawings.Add("DrawR", new CheckBox("[R] Range"));
            Drawings.Add("Draw_Disabled", new CheckBox("Disabled Drawings"));
            Drawings.Add("Notifications", new CheckBox("Alerter Can Kill [R]"));

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnPostAttack += ResetAttack;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }

        private static void Game_OnUpdate(EventArgs args)
		{
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
			
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
		    KillSteal();
            QStun();
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
		
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Drawings["Draw_Disabled"].Cast<CheckBox>().CurrentValue) return;
			
            if (Drawings["DrawQ"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = Q.Range }.Draw(Player.Instance.Position);
            }
			
            if (Drawings["DrawW"].Cast<CheckBox>().CurrentValue && W.IsReady())
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = W.Range }.Draw(Player.Instance.Position);
            }
			
            if (Drawings["DrawE"].Cast<CheckBox>().CurrentValue && E.IsReady())
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = E.Range }.Draw(Player.Instance.Position);
            }
			
            if (Drawings["DrawR"].Cast<CheckBox>().CurrentValue && R.IsReady())
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = R.Range }.Draw(Player.Instance.Position);
            }
			
            if (Drawings["Notifications"].Cast<CheckBox>().CurrentValue && R.IsReady())
            {
                var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (target.IsValidTarget(R.Range) && Player.Instance.GetSpellDamage(target, SpellSlot.R) > target.Health + target.AttackShield)
                {
                    DrawFont(Thm, "R Can Killable " + target.ChampionName, (float)(ft[0] - 140), (float)(ft[1] + 80), SharpDX.Color.Red);
                }
            }
        }

        public static void DrawFont(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }

        public static void Item()
        {
            var item = Items["BOTRK"].Cast<CheckBox>().CurrentValue;
            var yous = Items["you"].Cast<CheckBox>().CurrentValue;
            var Minhp = Items["ihp"].Cast<Slider>().CurrentValue;
            var Minhpp = Items["ihpp"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(550) && !e.IsDead))
            {
                if (item && Bil.IsReady() && Bil.IsOwned() && Bil.IsInRange(target))
                {
                    Bil.Cast(target);
                }
                if ((item && Botrk.IsReady() && Botrk.IsOwned() && target.IsValidTarget(475)) && (Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp))
                {
                    Botrk.Cast(target);
                }
				
                if (yous && Youmuu.IsReady() && Youmuu.IsOwned() && target.IsValidTarget(530) && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    Youmuu.Cast();
                }
            }
        }
// Thanks MarioGK has allowed me to use some his logic
        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (Misc["AntiGap"].Cast<CheckBox>().CurrentValue && E.IsReady() && sender.IsEnemy && sender.IsVisible && sender.IsInRange(Player.Instance, E.Range))
            {
                E.Cast(Player.Instance.Position.Shorten(sender.Position, E.Range));
            }	

            if (Misc["AntiGapW"].Cast<CheckBox>().CurrentValue && sender.IsEnemy && e.Sender.Distance(_Player) <= 300)
            {
                W.Cast(e.Sender);
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

        private static void ResetAttack(AttackableUnit target, EventArgs args)
        {
            var useJ = JungleMenu["JungleAA"].Cast<CheckBox>().CurrentValue;
            var manaJ = JungleMenu["EJung"].Cast<Slider>().CurrentValue;
            var useL = ClearMenu["LaneAA"].Cast<CheckBox>().CurrentValue;
            var mana = ClearMenu["ELane"].Cast<Slider>().CurrentValue;
			
            if (useJ && E.IsReady() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) && Player.Instance.ManaPercent >= manaJ)
            {
                Player.CastSpell(SpellSlot.E, Game.CursorPos);
                Orbwalker.ResetAutoAttack();
            }
			
            if (useL && E.IsReady() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && Player.Instance.ManaPercent >= mana)
            {
                Player.CastSpell(SpellSlot.E, Game.CursorPos);
                Orbwalker.ResetAutoAttack();
            }
			
            if (HarassMenu["HarassAA"].Cast<CheckBox>().CurrentValue && E.IsReady() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && Player.Instance.ManaPercent >= HarassMenu["ManaHarass"].Cast<Slider>().CurrentValue)
            {
                Player.CastSpell(SpellSlot.E, Game.CursorPos);
                Orbwalker.ResetAutoAttack();
            }
        }

        public static void Combo()
        {
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var MinR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;

            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(R.Range) && !e.IsDead && !e.IsZombie))
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
		    	}

                if (useW && W.IsReady() && target.IsValidTarget(W.Range))
     	        {
                    W.Cast(target);
		    	}
			
                if (useE && E.IsReady())
                {
                    if (ComboMode.CurrentValue == 1 && !Player.HasBuff("GravesBasicAttackAmmo2") && _Player.Position.CountEnemiesInRange(R.Range) >= 1)
                    {
                        Player.CastSpell(SpellSlot.E, Game.CursorPos);
                    }
				
                    if (ComboMode.CurrentValue == 0 && !Player.HasBuff("GravesBasicAttackAmmo2") && !Player.HasBuff("GravesBasicAttackAmmo1") && _Player.Position.CountEnemiesInRange(R.Range) >= 1)
                    {
      	        	    Player.CastSpell(SpellSlot.E, Game.CursorPos);
                    }
                }

                if (useR && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    var pred = R.GetPrediction(target);
                    if (pred.CastPosition.CountEnemiesInRange(R.Range) >= MinR && pred.HitChance >= HitChance.High)
                    {
                        R.Cast(pred.CastPosition);
                    }
		    	}
            }
        }

        private static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var ManaW = HarassMenu["ManaW"].Cast<Slider>().CurrentValue;
            if (Player.Instance.ManaPercent < ManaW) return;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(W.Range) && !e.IsDead && !e.IsZombie))
            {
                if (useQ && Q.IsReady() && HarassMenu["haras" + target.ChampionName].Cast<CheckBox>().CurrentValue)
                {
                    var predQ = Q.GetPrediction(target);
                    if (predQ.HitChance >= HitChance.Medium)
                    {
                        Q.Cast(predQ.CastPosition);
                    }
                }

                if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    W.Cast(target);
                }
	    	}
        }

        private static void LaneClear()
        {
            var useQ = ClearMenu["QClear"].Cast<CheckBox>().CurrentValue;
            var ManaQ = ClearMenu["ClearMana"].Cast<Slider>().CurrentValue;
            var MinQ = ClearMenu["minQ"].Cast<Slider>().CurrentValue;
            var minionQ = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(e => e.IsValidTarget(Q.Range)).ToArray();
            var quang = EntityManager.MinionsAndMonsters.GetLineFarmLocation(minionQ, Q.Width, (int) Q.Range);

            foreach (var minion in minionQ)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && Player.Instance.ManaPercent > ManaQ && quang.HitNumber >= MinQ)
                {
                    Q.Cast(quang.CastPosition);
                }
			}
        }
		
        private static void JungleClear()
        {
            var useQ = JungleMenu["QJungleClear"].Cast<CheckBox>().CurrentValue;
            var useW = JungleMenu["WJungleClear"].Cast<CheckBox>().CurrentValue;
            var jungMana = JungleMenu["JungleMana"].Cast<Slider>().CurrentValue;
            var jungManaW = JungleMenu["JungleManaW"].Cast<Slider>().CurrentValue;
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, Q.Range).OrderByDescending(a => a.MaxHealth).FirstOrDefault();
            if (monster == null && _Player.ManaPercent < jungMana) return;

            if (Q.IsReady() && useQ && monster.Distance(_Player) <= Q.Range)
            {
                Q.Cast(monster);
            }
			
            if (W.IsReady() && useW && monster.Distance(_Player) <= W.Range)
            {
                W.Cast(monster);
            }
        }
		
        public static void QStun()
		{
            var Qstun = Misc["Qstun"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target != null)
            {
                if (Qstun && Q.IsReady())
                {
                    if (target.IsRooted || target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup))
                    {
                        Q.Cast(target.Position);
                    }
                }
            }
		}

        private static void KillSteal()
		{
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var KsW = KillStealMenu["KsW"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(R.Range) && !e.HasBuff("BlitzcrankManaBarrierCD") && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && !e.IsDead && !e.IsZombie))
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
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.R) && target.IsInRange(Player.Instance, R.Range) && !target.IsInRange(Player.Instance, minKsR))
                    {
                        var pred = R.GetPrediction(target);
                        if (pred.HitChancePercent >= 70)
                        {
                            R.Cast(pred.CastPosition);
                        }
                    }
                }
				
                if (R.IsReady() && KillStealMenu["RKb"].Cast<KeyBind>().CurrentValue)
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
		}
    }
}