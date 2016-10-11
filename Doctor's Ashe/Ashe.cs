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

namespace Ashe
{
    internal class Program
    {
        public static Spell.Active Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Item Botrk;
        public static Item Bil;
        public static Font Thm;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Menu Menu, ComboMenu, JungleClearMenu, HarassMenu, LaneClearMenu, Misc, Items, Skin;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

// Menu

        private static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Ashe")) return;
            Chat.Print("Doctor's Ashe Loaded!", Color.Orange);
            Q = new Spell.Active(SpellSlot.Q, 600);
            W = new Spell.Skillshot(SpellSlot.W, 1200, SkillShotType.Linear, 0, int.MaxValue, 60);
            W.AllowedCollisionCount = 0;
            E = new Spell.Skillshot(SpellSlot.E, 10000, SkillShotType.Linear);
            E.AllowedCollisionCount = int.MaxValue;
            R = new Spell.Skillshot(SpellSlot.R, 3000, SkillShotType.Linear, 250, 1600, 100);
            R.AllowedCollisionCount = -1;
            Botrk = new Item(ItemId.Blade_of_the_Ruined_King);
            Bil = new Item(3144, 475f);
            Thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 32, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Menu = MainMenu.AddMenu("Doctor's Ashe", "Ashe");
            Menu.AddGroupLabel("Mercedes7");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Reset AA"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("ComboMode", new ComboBox("W Mode:", 1, "Fast [W]", "[W] Reset AA"));
            ComboMenu.Add("ComboR", new CheckBox("Use [R] Combo"));
            ComboMenu.Add("KeepCombo", new CheckBox("Keep Mana For [R]", false));
            ComboMenu.AddGroupLabel("Ultimate Aoe Settings");
            ComboMenu.Add("RAoe", new CheckBox("Use [R] Aoe"));
            ComboMenu.Add("minRAoe", new Slider("Use [R] Aoe If Hit x Enemies", 2, 1, 5));
            ComboMenu.AddGroupLabel("Ultimate Selected Target Settings");
            ComboMenu.Add("ComboSL", new KeyBind("Use [R] On Selected Target", false, KeyBind.BindTypes.HoldActive, 'Y'));
            ComboMenu.AddGroupLabel("KillSteal Settings");
            ComboMenu.Add("RKs", new CheckBox("Use [R] KillSteal"));
            ComboMenu.Add("WKs", new CheckBox("Use [W] KillSteal"));
            ComboMenu.Add("RKb", new KeyBind("Semi Manual [R] KillSteal", false, KeyBind.BindTypes.HoldActive, 'T'));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HarassW", new CheckBox("Use [W] Harass"));
            HarassMenu.Add("KeepHarass", new CheckBox("Keep Mana For [R]", false));
            HarassMenu.Add("manaHarass", new Slider("Mana Harass", 50, 0, 100));

            LaneClearMenu = Menu.AddSubMenu("Laneclear Settings", "Clear");
            LaneClearMenu.AddGroupLabel("Laneclear Settings");
            LaneClearMenu.Add("ClearQ", new CheckBox("Use [Q] Laneclear", false));
            LaneClearMenu.Add("ClearW", new CheckBox("Use [W] Laneclear", false));
            LaneClearMenu.Add("minw", new Slider("Number Hit Minions Use [W]", 2, 1, 6));
            LaneClearMenu.Add("manaFarm", new Slider("Mana LaneClear", 60, 0, 100));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("jungleQ", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("jungleW", new CheckBox("Use [W] JungleClear"));
            JungleClearMenu.Add("manaJung", new Slider("Mana JungleClear", 20, 0, 100));

            Items = Menu.AddSubMenu("Items Settings", "Items");
            Items.AddGroupLabel("Items Settings");
            Items.Add("BOTRK", new CheckBox("Use [Botrk]"));
            Items.Add("ihp", new Slider("My HP Use BOTRK <=", 50));
            Items.Add("ihpp", new Slider("Enemy HP Use BOTRK <=", 50));

            Misc = Menu.AddSubMenu("Misc Settings", "Draw");
            Misc.AddGroupLabel("Anti Gapcloser");
            Misc.Add("antiGap", new CheckBox("Anti Gapcloser", false));
            Misc.Add("antiRengar", new CheckBox("Anti Rengar KhaZix", false));
            Misc.Add("inter", new CheckBox("Use [R] Interupt"));
            Misc.AddGroupLabel("Drawings Settings");
            Misc.Add("Draw_Disabled", new CheckBox("Disabled Drawings", false));
            Misc.Add("DrawW", new CheckBox("Draw [W]", false));
            Misc.Add("Notifications", new CheckBox("Alerter Can Kill With [R]"));

            Skin = Menu.AddSubMenu("Skin Changer", "SkinChanger");
            Skin.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Skin.Add("skin.Id", new ComboBox("Skin Mode", 6, "1", "2", "3", "4", "5", "6", "7", "8", "9"));

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interupt;
            Orbwalker.OnPostAttack += ResetAttack;
            GameObject.OnCreate += GameObject_OnCreate;

        }

        private static void Game_OnUpdate(EventArgs args)
        {
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
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }

            if (ComboMenu["ComboSL"].Cast<KeyBind>().CurrentValue)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
			
            KillSteal();
            Item();
            RSelected();
			
            if (_Player.SkinId != Skin["skin.Id"].Cast<ComboBox>().CurrentValue)
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
			
            if (Misc["DrawW"].Cast<CheckBox>().CurrentValue && W.IsReady())
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2, Radius = W.Range }.Draw(_Player.Position);
            }
			
            if (Misc["Notifications"].Cast<CheckBox>().CurrentValue && R.IsReady())
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (target.IsValidTarget(W.Range) && Player.Instance.GetSpellDamage(target, SpellSlot.R) > target.Health + target.AttackShield)
                {
                    DrawFont(Thm, "[R] Can Killable " + target.ChampionName, (float)(ft[0] - 140), (float)(ft[1] + 80), SharpDX.Color.Red);
                }
            }
        }

        public static bool QReady
        {
            get { return Player.Instance.GetBuffCount("AsheQCastReady") == 4; }
        }

// Flee Mode

        private static void Flee()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (W.IsReady() && target.IsValidTarget(W.Range))
            {
                W.Cast(target);
            }
        }

// Skin Changer

        public static int SkinId()
        {
            return Skin["skin.Id"].Cast<ComboBox>().CurrentValue;
        }

        public static bool checkSkin()
        {
            return Skin["checkSkin"].Cast<CheckBox>().CurrentValue;
        }

// Interrupt

        private static void Interupt(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs i)
        {
            var Inter = Misc["inter"].Cast<CheckBox>().CurrentValue;
            if (!sender.IsEnemy || !(sender is AIHeroClient) || Player.Instance.IsRecalling())
            {
                return;
            }
			
            if (Inter && R.IsReady() && i.DangerLevel == DangerLevel.High && _Player.Distance(sender) <= 1600)
            {
                R.Cast(sender);
            }
        }

// Anti Rengar + Khazix

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var rengar = EntityManager.Heroes.Enemies.Find(e => e.ChampionName.Equals("Rengar"));
            var khazix = EntityManager.Heroes.Enemies.Find(e => e.ChampionName.Equals("Khazix"));
            if (rengar != null)
            {
                if (sender.Name == ("Rengar_LeapSound.troy") && Misc["antiRengar"].Cast<CheckBox>().CurrentValue && R.IsReady() && sender.Position.Distance(_Player) <= 300)
                {
                    R.Cast(rengar);
                }
            }

            if (khazix != null)
            {
                if (sender.Name == ("Khazix_Base_E_Tar.troy") && Misc["antiRengar"].Cast<CheckBox>().CurrentValue && R.IsReady() && sender.Position.Distance(_Player) <= 300)
                {
                    R.Cast(khazix);
                }
            }
        }

//Harass Mode

        private static void Harass()
        {
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var Keep = HarassMenu["KeepHarass"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["manaHarass"].Cast<Slider>().CurrentValue;
            if (_Player.ManaPercent < mana) return;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(W.Range) && !e.IsDead && !e.IsZombie))
            {
                if (useW && W.IsReady() && target.IsValidTarget(W.Range) && !Orbwalker.IsAutoAttacking)
                {
                    var WPred = W.GetPrediction(target);
                    if (Keep)
                    {
                        if (R.IsReady())
                        {
                            if (Player.Instance.Mana > W.Handle.SData.Mana + R.Handle.SData.Mana && WPred.HitChance >= HitChance.High)
                            {
                                W.Cast(WPred.CastPosition);
                            }
                        }
                        else
                        {
                            if (WPred.HitChance >= HitChance.High)
                            {
                                W.Cast(WPred.CastPosition);
                            }
                        }
                    }
                    else
                    {
                        if (WPred.HitChance >= HitChance.High)
                        {
                            W.Cast(WPred.CastPosition);
                        }
                    }
                }
            }
        }

//Combo Mode

        private static void Combo()
        {
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var RAoe = ComboMenu["RAoe"].Cast<CheckBox>().CurrentValue;
            var MinR = ComboMenu["minRAoe"].Cast<Slider>().CurrentValue;
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var Keep = ComboMenu["KeepCombo"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(2350) && !e.IsDead && !e.IsZombie))
            {
                if (ComboMenu["ComboMode"].Cast<ComboBox>().CurrentValue == 1)
                {
                    if (useW && W.IsReady() && target.IsValidTarget(W.Range) && (_Player.Distance(target) > Player.Instance.GetAutoAttackRange(target) || Player.Instance.HealthPercent < 20))
                    {
                        var WPred = W.GetPrediction(target);
                        if (Keep)
                        {
                            if (R.IsReady())
                            {
                                if (Player.Instance.Mana > W.Handle.SData.Mana + R.Handle.SData.Mana && WPred.HitChance >= HitChance.High)
                                {
                                    W.Cast(WPred.CastPosition);
                                }
                            }
                            else
                            {
                                if (WPred.HitChance >= HitChance.High)
                                {
                                    W.Cast(WPred.CastPosition);
                                }
                            }
                        }
                        else
                        {
                            if (WPred.HitChance >= HitChance.High)
                            {
                                W.Cast(WPred.CastPosition);
                            }
                        }
                    }
                }

                if (ComboMenu["ComboMode"].Cast<ComboBox>().CurrentValue == 0)
                {
                    if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                    {
                        var Pred = W.GetPrediction(target);
                        if (Keep)
                        {
                            if (R.IsReady())
                            {
                                if (Player.Instance.Mana > W.Handle.SData.Mana + R.Handle.SData.Mana && Pred.HitChance >= HitChance.High)
                                {
                                    W.Cast(Pred.CastPosition);
                                }
                            }
                            else
                            {
                                if (Pred.HitChance >= HitChance.High)
                                {
                                    W.Cast(Pred.CastPosition);
                                }
                            }
                        }
                        else
                        {
                            if (Pred.HitChance >= HitChance.High)
                            {
                                W.Cast(Pred.CastPosition);
                            }
                        }
                    }
                }

                if (useR && R.IsReady() && target.IsValidTarget(W.Range) && _Player.HealthPercent <= 70)
                {
                    var RPred = R.GetPrediction(target);
                    if (RPred.HitChance >= HitChance.High)
                    {
                        R.Cast(RPred.CastPosition);
                    }
                }

                if (RAoe && R.IsReady() && target.IsValidTarget(2300))
                {
                    var RPred = R.GetPrediction(target);
                    if (RPred.CastPosition.CountEnemiesInRange(325) >= MinR && RPred.HitChance >= HitChance.Medium)
                    {
                        R.Cast(RPred.CastPosition);
                    }
		    	}
            }
        }

        private static void RSelected()
        {
            var targetS = TargetSelector.SelectedTarget;
            var useSL = ComboMenu["ComboSL"].Cast<KeyBind>().CurrentValue;

            if (targetS == null)
            {
                return;
            }

            if (useSL && R.IsReady() && targetS.IsValidTarget(2800))
            {
                var RPred = R.GetPrediction(targetS);
                if (RPred.HitChance >= HitChance.High)
                {
                    R.Cast(RPred.CastPosition);
                }
            }
        }

//Use Q ResetAttack

        public static void ResetAttack(AttackableUnit e, EventArgs args)
        {
            if (!(e is AIHeroClient)) return;
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var champ = (AIHeroClient)e;
            var Keep2 = HarassMenu["KeepHarass"].Cast<CheckBox>().CurrentValue;
            var useQ2 = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var Keep = ComboMenu["KeepCombo"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            if (champ == null || champ.Type != GameObjectType.AIHeroClient || !champ.IsValid) return;
            if (target != null)
            {
                if (ComboMenu["ComboMode"].Cast<ComboBox>().CurrentValue == 1)
                {
                    if (useW && W.IsReady() && !QReady && target.IsValidTarget(W.Range) && _Player.Distance(target) < Player.Instance.GetAutoAttackRange(target) && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && Player.Instance.HealthPercent > 20)
                    {
                        var WPred = W.GetPrediction(target);
                        if (Keep)
                        {
                            if (R.IsReady())
                            {
                                if (Player.Instance.Mana > W.Handle.SData.Mana + R.Handle.SData.Mana && WPred.HitChance >= HitChance.High)
                                {
                                    W.Cast(WPred.CastPosition);
                                }
                            }
                            else
                            {
                                if (WPred.HitChance >= HitChance.High)
                                {
                                    W.Cast(WPred.CastPosition);
                                }
                            }
                        }
                        else
                        {
                            if (WPred.HitChance >= HitChance.High)
                            {
                                W.Cast(WPred.CastPosition);
                            }
                        }
                    }
                }

                if (useQ && QReady && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && target.IsValidTarget(650))
                {
                    if (Keep)
                    {
                        if (R.IsReady())
                        {
                            if (Player.Instance.Mana > Q.Handle.SData.Mana + R.Handle.SData.Mana)
                            {
                                Q.Cast();
                            }
                        }
                        else
                        {
                            Q.Cast();
                        }
                    }
                    else
                    {
                        Q.Cast();
                    }
                }

                if (useQ2 && QReady && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && target.IsValidTarget(650))
                {
                    if (Keep2)
                    {
                        if (R.IsReady())
                        {
                            if (Player.Instance.Mana > Q.Handle.SData.Mana + R.Handle.SData.Mana)
                            {
                                Q.Cast();
                            }
                        }
                        else
                        {
                            Q.Cast();
                        }
                    }
                    else
                    {
                        Q.Cast();
                    }
                }
            }
        }

//LaneClear Mode

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["ClearQ"].Cast<CheckBox>().CurrentValue;
            var useW = LaneClearMenu["ClearW"].Cast<CheckBox>().CurrentValue;
            var minW = LaneClearMenu["minw"].Cast<Slider>().CurrentValue;
            var mana = LaneClearMenu["manaFarm"].Cast<Slider>().CurrentValue;
            var minionQ = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(e => e.IsValidTarget(W.Range));
            var quang = EntityManager.MinionsAndMonsters.GetLineFarmLocation(minionQ, W.Width, (int) W.Range);
            if (_Player.ManaPercent < mana) return;
            foreach (var minion in minionQ)
            {
                if (useW && W.IsReady() && minion.IsValidTarget(W.Range) && quang.HitNumber >= minW)
                {
                    W.Cast(quang.CastPosition);
                }
				
                if (useQ && minion.IsValidTarget(Q.Range) && QReady && _Player.Position.CountEnemyMinionsInRange(Q.Range) >= 2)
                {
                    Q.Cast();
                }
            }
        }

// JungleClear Mode

        private static void JungleClear()
        {
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(W.Range));
            var useQ = JungleClearMenu["jungleQ"].Cast<CheckBox>().CurrentValue;
            var useW = JungleClearMenu["jungleW"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["manaJung"].Cast<Slider>().CurrentValue;
            if (_Player.ManaPercent < mana) return;
            if (monster != null)
            {
                if (useQ && QReady && monster.IsValidTarget(Q.Range))
                {
                    Q.Cast();
                }
				
                if (useW && W.IsReady() && monster.IsValidTarget(W.Range))
                {
                    W.Cast(monster);
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
            var target = EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(2350) && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && !e.IsDead && !e.IsZombie);
            var useR = ComboMenu["RKs"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["WKs"].Cast<CheckBox>().CurrentValue;
            var RKey = ComboMenu["RKb"].Cast<KeyBind>().CurrentValue;
            foreach (var target2 in target)
            {
                if ((useR || RKey) && R.IsReady())
                {
                    if (target2.Health + target2.AttackShield <= Player.Instance.GetSpellDamage(target2, SpellSlot.R) && target2.IsValidTarget(2300) && !target2.IsInRange(Player.Instance, 700))
                    {
                        var RPred = R.GetPrediction(target2);
                        if (RPred.HitChance >= HitChance.Medium)
                        {
                            R.Cast(RPred.CastPosition);
                        }
                    }
                }
				
                if (useW && W.IsReady())
                {
                    if (target2.Health + target2.AttackShield <= Player.Instance.GetSpellDamage(target2, SpellSlot.W) && target2.IsValidTarget(W.Range))
                    {
                        var WPred = W.GetPrediction(target2);
                        if (WPred.HitChance >= HitChance.Medium)
                        {
                            W.Cast(WPred.CastPosition);
                        }
                    }
                }
            }
        }

// AntiGap

        private static void Gapcloser_OnGapCloser(Obj_AI_Base sender, Gapcloser.GapcloserEventArgs args)
        {
            if (Misc["antiGap"].Cast<CheckBox>().CurrentValue && sender.IsEnemy && args.Sender.Distance(_Player) <= 325)
            {
                W.Cast(args.Sender);
            }
        }

// Use Items

        public static void Item()
        {
            var item = Items["BOTRK"].Cast<CheckBox>().CurrentValue;
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
            }
        }
    }
}
