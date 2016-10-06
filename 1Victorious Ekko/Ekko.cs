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

namespace Ekko
{
    static class Program
    {
        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Active E;
        public static Spell.Active R;
        public static Font Thm;
        public static GameObject EkkoREmitter { get; set; }
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Menu Menu, ComboMenu, JungleClearMenu, HarassMenu, Ulti, LaneClearMenu, Misc, KillSteals;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

// Menu

        private static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Ekko")) return;
            Chat.Print("Victorious Ekko Loaded!", Color.White);
            EkkoREmitter = ObjectManager.Get<Obj_GeneralParticleEmitter>().FirstOrDefault(x => x.Name.Equals("Ekko_Base_R_TrailEnd.troy"));
            Q = new Spell.Skillshot(SpellSlot.Q, 850, SkillShotType.Linear, 250, 2200, 60);
            Q.AllowedCollisionCount = int.MaxValue;
            W = new Spell.Skillshot(SpellSlot.W, 1600, SkillShotType.Circular, 1500, 500, 650);
            W.AllowedCollisionCount = int.MaxValue;
            E = new Spell.Active(SpellSlot.E, 450);
            R = new Spell.Active(SpellSlot.R, 375);
            Thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 16, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Menu = MainMenu.AddMenu("Victorious Ekko", "Ekko");
            Menu.AddGroupLabel("Mercedes7");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("CQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("CW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("CW2", new CheckBox("Use [W] No Prediction", false));
            ComboMenu.Add("CE", new CheckBox("Use [E] Combo"));
            ComboMenu.Add("EMode", new ComboBox("Combo Mode:", 0, "E To Target", "E To Mouse"));
            ComboMenu.Add("CTurret", new KeyBind("Don't Use [E] UnderTurret", false, KeyBind.BindTypes.PressToggle, 'T'));

            Ulti = Menu.AddSubMenu("Ulti Settings", "Ulti");
            Ulti.AddGroupLabel("Ulti Settings");
            Ulti.Add("RKs", new CheckBox("Use [R] Ks"));
            Ulti.Add("RAoe", new CheckBox("Use [R] Aoe"));
            Ulti.Add("MinR", new Slider("Min Hit Enemies Use [R] Aoe", 3, 1, 5));
            Ulti.Add("REscape", new CheckBox("Use [R] Low Hp"));
            Ulti.Add("RHp", new Slider("Below MyHp Use [R]", 20));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HW", new CheckBox("Use [W] Harass"));
            HarassMenu.Add("HW2", new CheckBox("Use [W] No Prediction", false));
            HarassMenu.Add("HE", new CheckBox("Use [E] Harass"));
            HarassMenu.Add("HTurret", new CheckBox("Don't [E] Under Turret"));
            HarassMenu.Add("MinE", new Slider("Limit Enemies Around Target Use [E] Harass", 5, 1, 5));
            HarassMenu.Add("HM", new Slider("Mana Harass", 50, 0, 100));

            LaneClearMenu = Menu.AddSubMenu("Laneclear Settings", "Clear");
            LaneClearMenu.AddGroupLabel("Laneclear Settings");
            LaneClearMenu.Add("LQ", new CheckBox("Use [Q] Laneclear"));
            LaneClearMenu.Add("MinQ", new Slider("Min Hit Minions Use [Q] LaneClear", 3, 1, 6));
            LaneClearMenu.Add("LE", new CheckBox("Use [E] Laneclear", false));
            LaneClearMenu.Add("LM", new Slider("Mana LaneClear", 60, 0, 100));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("JQ", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("JW", new CheckBox("Use [W] JungleClear"));
            JungleClearMenu.Add("JE", new CheckBox("Use [E] JungleClear"));
            JungleClearMenu.Add("JM", new Slider("Mana JungleClear", 20, 0, 100));

            KillSteals = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillSteals.Add("QKs", new CheckBox("Use [Q] Ks"));
            KillSteals.Add("EKs", new CheckBox("Use [E] Ks"));

            Misc = Menu.AddSubMenu("Misc Settings", "Draw");
            Misc.AddGroupLabel("Anti Gapcloser");
            Misc.Add("antiGap", new CheckBox("Use [Q] Anti Gapcloser"));
            Misc.Add("inter", new CheckBox("Use [W] Interupt", false));
            Misc.Add("Qcc", new CheckBox("Use [Q] Immobile"));
            Misc.Add("QPassive", new CheckBox("Auto [Q] Enemies With 2 Stacks"));
            Misc.AddGroupLabel("Drawings Settings");
            Misc.Add("Draw_Disabled", new CheckBox("Disabled Drawings", false));
            Misc.Add("DrawE", new CheckBox("Draw [E]"));
            Misc.Add("DrawQ", new CheckBox("Draw [Q]"));
            Misc.Add("DrawW", new CheckBox("Draw [W]", false));
            Misc.Add("DrawR", new CheckBox("Draw [R]"));
            Misc.Add("DrawTR", new CheckBox("Status UnderTuret"));
            Misc.AddGroupLabel("Skins Settings");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Misc.Add("skin.Id", new ComboBox("Skin Mode", 0, "Default", "1", "2", "3"));

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interupt;
            Obj_AI_Base.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;
            GameObject.OnCreate += Game_On_Create;
            GameObject.OnDelete += Game_On_Delete;

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

            KillSteal();
            Ultimate();
            Miscs();

            if (_Player.SkinId != Misc["skin.Id"].Cast<ComboBox>().CurrentValue)
            {
                if (checkSkin())
                {
                    Player.SetSkinId(SkinId());
                }
            }

            if (EkkoREmitter == null && R.IsReady())
            {
                EkkoREmitter = ObjectManager.Get<Obj_GeneralParticleEmitter>().FirstOrDefault(x => x.Name.Equals("Ekko_Base_R_TrailEnd.troy"));
            }
        }

// Drawings

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;
			
            if (Misc["Draw_Disabled"].Cast<CheckBox>().CurrentValue) return;
			
            if (Misc["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2, Radius = E.Range }.Draw(_Player.Position);
            }
			
            if (Misc["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2, Radius = W.Range }.Draw(_Player.Position);
            }

            if (Misc["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2, Radius = Q.Range }.Draw(_Player.Position);
            }
			
            if (Misc["DrawR"].Cast<CheckBox>().CurrentValue && EkkoREmitter != null)
            {
                new Circle() { Color = Color.Yellow, BorderWidth = 2, Radius = 375 }.Draw(EkkoREmitter.Position);
            }
			
            if (Misc["DrawTR"].Cast<CheckBox>().CurrentValue)
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue)
                {
                    DrawFont(Thm, "Use E Under Turret : Disable", (float)(ft[0] - 70), (float)(ft[1] + 50), SharpDX.Color.White);
                }
                else
                {
                    DrawFont(Thm, "Use E Under Turret : Enable", (float)(ft[0] - 70), (float)(ft[1] + 50), SharpDX.Color.Red);
                }
            }
        }

// Flee Mode

        private static void Flee()
        {
            if (E.IsReady())
            {
                var cursorPos = Game.CursorPos;
                var castPos = Player.Instance.Position.Distance(cursorPos) <= E.Range ? cursorPos : Player.Instance.Position.Extend(cursorPos, E.Range).To3D();
                Player.CastSpell(SpellSlot.E, Game.CursorPos);
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

//Damage

        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 60, 75, 90, 105, 120 }[Q.Level] + 0.2f * _Player.FlatMagicDamageMod));
        }

        public static float EDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 40, 65, 90, 115, 140 }[E.Level] + 0.4f * _Player.FlatMagicDamageMod));
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 150, 300, 450 }[R.Level] + 1.5f * _Player.FlatMagicDamageMod));
        }

// Create + Delete

        public static void Game_On_Create(GameObject sender, EventArgs args)
        {
            var Emitter = sender as Obj_GeneralParticleEmitter;
            if (Emitter != null)
            {
                if (Emitter.Name.Equals("Ekko_Base_R_TrailEnd.troy"))
                {
                    EkkoREmitter = Emitter;
                }
            }
        }

        public static void Game_On_Delete(GameObject sender, EventArgs args)
        {
            var Emitter = sender as Obj_GeneralParticleEmitter;
            if (Emitter != null)
            {
                if (Emitter.Name.Equals("Ekko_Base_R_TrailEnd.troy"))
                {
                    EkkoREmitter = null;
                }
            }
        }

// Ultimate

        private static void Ultimate()
        {
            var Minr = Ulti["MinR"].Cast<Slider>().CurrentValue;
            var enemies = EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget() && !e.IsZombie).ToArray();
            var useRAoe = Ulti["RAoe"].Cast<CheckBox>().CurrentValue;

            if (useRAoe && R.IsReady())
            {
                if (EkkoREmitter != null)
                {
                    if (enemies != null)
                    {
                        int RCal = enemies.Where(e => e.Distance(EkkoREmitter.Position) <= R.Range).Count(); ;
                        if (RCal >= Minr)
                        {
                            R.Cast();
                        }
                    }
                }
            }
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

//Harass Mode

        private static void Harass()
        {
            var useQ = HarassMenu["HQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HW"].Cast<CheckBox>().CurrentValue;
            var useW2 = HarassMenu["HW2"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HE"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["HM"].Cast<Slider>().CurrentValue;
            var minE = HarassMenu["MinE"].Cast<Slider>().CurrentValue;
            var turret = HarassMenu["HTurret"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(W.Range, DamageType.Magical);

            if (Player.Instance.ManaPercent <= mana)
            {
                return;
            }

            if (target != null)
            {
                if (useQ && Q.CanCast(target))
                {
                    var Qpred = Q.GetPrediction(target);
                    if (Qpred.HitChance >= HitChance.High)
                    {
                        Q.Cast(Qpred.CastPosition);
                    }
                }

                if (useW && W.CanCast(target))
                {
                    if (useW2)
                    {
                        W.Cast(target.Position);
                    }
                    else
                    {
                        var Wpred = W.GetPrediction(target);
                        if (Wpred.HitChance >= HitChance.High)
                        {
                            W.Cast(Wpred.CastPosition);
                        }
                    }
                }

                if (useE && E.IsReady() && target.IsValidTarget(E.Range + 375) && target.Position.CountEnemiesInRange(R.Range) <= minE)
                {
                    if (turret)
                    {
                        if (!UnderTuret(target))
                        {
                            if (Player.CastSpell(SpellSlot.E, target.Position));
                            {
                                Orbwalker.ResetAutoAttack();
                                Core.DelayAction(() => EloBuddy.Player.IssueOrder(GameObjectOrder.AttackUnit, target),  500);
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (Player.CastSpell(SpellSlot.E, target.Position));
                        {
                            Orbwalker.ResetAutoAttack();
                            Core.DelayAction(() => EloBuddy.Player.IssueOrder(GameObjectOrder.AttackUnit, target),  500);
                            return;
                        }
                    }
                }
            }
        }

        private static bool UnderTuret(Obj_AI_Base target)
        {
            var tower = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(turret => turret != null && turret.Distance(target) <= 775 && turret.IsValid && turret.Health > 0 && !turret.IsAlly);
            return tower != null;
        }

//Combo Mode

        private static void Combo()
        {
            var useQ = ComboMenu["CQ"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["CW"].Cast<CheckBox>().CurrentValue;
            var useW2 = ComboMenu["CW2"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["CE"].Cast<CheckBox>().CurrentValue;
            var turret = ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue;
            var target = TargetSelector.GetTarget(W.Range, DamageType.Magical);

            if (target != null)
            {
                if (useQ && Q.CanCast(target))
                {
                    var Qpred = Q.GetPrediction(target);
                    if (Qpred.HitChance >= HitChance.High)
                    {
                        Q.Cast(Qpred.CastPosition);
                    }
                }

                if (useW && W.CanCast(target))
                {
                    if (useW2)
                    {
                        W.Cast(target.Position);
                    }
                    else
                    {
                        var Wpred = W.GetPrediction(target);
                        if (Wpred.HitChance >= HitChance.High)
                        {
                            W.Cast(Wpred.CastPosition);
                        }
                    }
                }

                if (useE && E.IsReady() && target.IsValidTarget(E.Range + 375) && (target.Distance(_Player.Position) >= 175 || Player.Instance.HealthPercent <= 20))
                {
                    if (ComboMenu["EMode"].Cast<ComboBox>().CurrentValue == 0)
                    {
                        if (turret)
                        {
                            if (!UnderTuret(target))
                            {
                                if (Player.CastSpell(SpellSlot.E, target.Position));
                                {
                                    Orbwalker.ResetAutoAttack();
                                    Core.DelayAction(() => EloBuddy.Player.IssueOrder(GameObjectOrder.AttackUnit, target),  500);
                                    return;
                                }
								
                            }
                        }
                        else
                        {
                                if (Player.CastSpell(SpellSlot.E, target.Position));
                                {
                                    Orbwalker.ResetAutoAttack();
                                    Core.DelayAction(() => EloBuddy.Player.IssueOrder(GameObjectOrder.AttackUnit, target),  500);
                                    return;
                                }
                        }
                        
                        if (ComboMenu["EMode"].Cast<ComboBox>().CurrentValue == 1)
                        {
                            if (turret)
                            {
                                if (!UnderTuret(target))
                                {
                                    if (Player.CastSpell(SpellSlot.E, Game.CursorPos));
                                    {
                                        Orbwalker.ResetAutoAttack();
                                        Core.DelayAction(() => EloBuddy.Player.IssueOrder(GameObjectOrder.AttackUnit, target),  500);
                                        return;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (Player.CastSpell(SpellSlot.E, Game.CursorPos));
                            {
                                Orbwalker.ResetAutoAttack();
                                Core.DelayAction(() => EloBuddy.Player.IssueOrder(GameObjectOrder.AttackUnit, target),  500);
                                return;
                            }
                        }
                    }
                }
            }
        }

//LaneClear Mode

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["LQ"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["LE"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["LM"].Cast<Slider>().CurrentValue;
            var Minq = LaneClearMenu["MinQ"].Cast<Slider>().CurrentValue;
            var minionQ = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(e => e.IsValidTarget(Q.Range));
            var quang = EntityManager.MinionsAndMonsters.GetLineFarmLocation(minionQ, Q.Width, (int) Q.Range);

            if (Player.Instance.ManaPercent <= mana)
            {
                return;
            }

            foreach (var minion in minionQ)
            {
                if (useQ && Q.CanCast(minion) && quang.HitNumber >= Minq)
                {
                    Q.Cast(quang.CastPosition);
                }

                if (useE && E.CanCast(minion) && EDamage(minion) + Player.Instance.GetAutoAttackDamage(minion) >= minion.Health)
                {
                    if (minion.Distance(_Player.Position) > Player.Instance.GetAutoAttackRange(minion))
                    {
                        if (Player.CastSpell(SpellSlot.E, minion.Position))
                        {
                            Orbwalker.ResetAutoAttack();
                            Core.DelayAction(() => EloBuddy.Player.IssueOrder(GameObjectOrder.AttackUnit, minion),  500);
                            return;
                        }
                    }
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

            if (monster != null)
            {
                if (useQ && Q.CanCast(monster))
                {
                    Q.Cast(monster.Position);
                }

                if (useW && W.IsReady() && monster.IsValidTarget(Q.Range))
                {
                    W.Cast(monster.Position);
                }

                if (useE && E.CanCast(monster))
                {
                    if (Player.CastSpell(SpellSlot.E, monster.Position))
                    {
                        Orbwalker.ResetAutoAttack();
                        Player.IssueOrder(GameObjectOrder.AttackUnit, monster);
                        return;
                    }
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
            var Enemies = EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q.Range) && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && !e.IsDead && !e.IsZombie);
            var useQ = KillSteals["QKs"].Cast<CheckBox>().CurrentValue;
            var useE = KillSteals["EKs"].Cast<CheckBox>().CurrentValue;
            var useR = Ulti["RKs"].Cast<CheckBox>().CurrentValue;
            foreach (var target in Enemies)
            {
                if (useQ && Q.CanCast(target))
                {
                    if (QDamage(target) >= target.Health)
                    {
                        var Qpred = Q.GetPrediction(target);
                        if (Qpred.HitChance >= HitChance.High)
                        {
                            Q.Cast(Qpred.CastPosition);
                        }
                    }
                }

                if (useE && E.IsReady() && target.Distance(_Player.Position) <= E.Range + 375)
                {
                    if (EDamage(target) >= target.Health)
                    {
                        if (Player.CastSpell(SpellSlot.E, target.Position))
                        {
                            Orbwalker.ResetAutoAttack();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                            return;
                        }
                    }
                }

                if (useR && R.IsReady() && EkkoREmitter != null)
                {
                    if (target.Health <= RDamage(target) && target.Distance(EkkoREmitter.Position) <= R.Range)
                    {
                        R.Cast();
                    }
                }
            }
        }

// OnProcessSpellCast

        private static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var useREs = Ulti["REscape"].Cast<CheckBox>().CurrentValue;
            var Hp = Ulti["RHp"].Cast<Slider>().CurrentValue;

            if (!sender.IsEnemy && (!(sender is AIHeroClient) || !(sender is Obj_AI_Turret)))
            {
                return;
            }

            if (!sender.IsValidTarget(Q.Range))
            {
                return;
            }

            if (useREs && R.IsReady())
            {
                if (Player.Instance.HealthPercent <= Hp)
                {
                    R.Cast();
                }

                if (sender.BaseAttackDamage >= Player.Instance.TotalShieldHealth() || sender.BaseAbilityDamage >= Player.Instance.TotalShieldHealth())
                {
                    R.Cast();
                }

                if (sender.GetAutoAttackDamage(Player.Instance) >= Player.Instance.TotalShieldHealth())
                {
                    R.Cast();
                }
            }

            if (args.SData.Name == "ZedR")
            {
                if (R.IsReady())
                {
                    Core.DelayAction(() => R.Cast(), 2000 - Game.Ping - 200);
                }
            }
        }

// Misc

        public static void Miscs()
        {
            var useQ = Misc["Qcc"].Cast<CheckBox>().CurrentValue;
            var Passive = Misc["QPassive"].Cast<CheckBox>().CurrentValue;
            var Enemies = EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q.Range) && e.GetBuffCount("EkkoStacks") == 2 && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && !e.IsDead && !e.IsZombie);

            foreach (var target in Enemies)
            {
                if (Passive && Q.CanCast(target))
                {
                    var Qpred = Q.GetPrediction(target);
                    if (Qpred.HitChance >= HitChance.Medium)
                    {
                        Q.Cast(Qpred.CastPosition);
                    }
                }
            }

            var targetQ = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (useQ && Q.CanCast(targetQ))
            {
                if (targetQ != null)
                {
                    if (targetQ.HasBuffOfType(BuffType.Stun) || targetQ.HasBuffOfType(BuffType.Snare) || targetQ.HasBuffOfType(BuffType.Knockup))
                    {
                        Q.Cast(targetQ.Position);
                    }
                }
            }
        }

// AntiGap

        private static void Gapcloser_OnGapCloser(Obj_AI_Base sender, Gapcloser.GapcloserEventArgs args)
        {
            if (Misc["antiGap"].Cast<CheckBox>().CurrentValue && sender.IsEnemy && args.Sender.Distance(_Player.Position) <= 325)
            {
                Q.Cast(args.Sender);
            }
        }
    }
}
