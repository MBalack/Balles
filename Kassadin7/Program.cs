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
using Font = SharpDX.Direct3D9.Font;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace Kassadin7
{
    internal class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, LaneClearMenu, LastHitMenu, JungleClearMenu, KillStealMenu, Misc;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell.Targeted Q;
        public static Spell.Active W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Item Seraph;
        public static Spell.Targeted Ignite;
        public static Font thm;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Kassadin")) return;
            Chat.Print("Doctor's Kassadin Loaded!", Color.Orange);
            Bootstrap.Init(null);
            Q = new Spell.Targeted(SpellSlot.Q, 650);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Skillshot(SpellSlot.E, 600, SkillShotType.Cone, 500, int.MaxValue, 10);
            E.AllowedCollisionCount = int.MaxValue;
            R = new Spell.Skillshot(SpellSlot.R, 700, SkillShotType.Circular, 500, int.MaxValue, 150);
            R.AllowedCollisionCount = int.MaxValue;
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 15, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Seraph = new Item(3040);
            Menu = MainMenu.AddMenu("Kassadin", "Kassadin");
            Menu.AddGroupLabel(" Doctor7 ");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            ComboMenu.AddGroupLabel("Ultimate Settings");
            ComboMenu.Add("ComboR", new CheckBox("Use [R] Combo"));
            ComboMenu.Add("CTurret", new KeyBind("Dont Use [R] UnderTurret", false, KeyBind.BindTypes.PressToggle, 'T'));
            ComboMenu.Add("CMinR", new Slider("Limit Enemies Around Use [R]", 2, 1, 5));
            ComboMenu.Add("Cihp", new Slider("MyHP Use [R] >", 20));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HarassW", new CheckBox("Use [W] Harass"));
            HarassMenu.Add("HarassE", new CheckBox("Use [E] Harass"));
            HarassMenu.Add("ManaHR", new Slider("Mana For Harass", 40));
            HarassMenu.AddGroupLabel("Ultimate Settings");
            HarassMenu.Add("HarassR", new CheckBox("Use [R] Harass"));
            HarassMenu.Add("StackRH", new Slider("Use [R] Stacks Limit Harass", 5, 1, 5));
            HarassMenu.Add("MinR", new Slider("Limit Enemies Around Use [R]", 3, 1, 5));
            HarassMenu.Add("ihp", new Slider("MyHP Use [R] >", 30));

            LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("Lane Clear Settings");
            LaneClearMenu.Add("QLC", new CheckBox("Use [Q] LaneClear", false));
            LaneClearMenu.Add("WLC", new CheckBox("Use [W] LaneClear"));
            LaneClearMenu.Add("ELC", new CheckBox("Use [E] LaneClear", false));
            LaneClearMenu.Add("MinELC", new Slider("Min Hit Minions Use [E]", 2, 1, 3));
            LaneClearMenu.Add("RLC", new CheckBox("Use [R] LaneClear", false));
            LaneClearMenu.Add("StackRL", new Slider("Use [R] Stacks Limit LaneClear", 1, 1, 5));
            LaneClearMenu.Add("ManaLC", new Slider("Mana For LaneClear", 50));

            LastHitMenu = Menu.AddSubMenu("LastHit Settings", "LastHit");
            LastHitMenu.AddGroupLabel("LastHit Settings");
            LastHitMenu.Add("QLH", new CheckBox("Use [Q] LastHit"));
            LastHitMenu.Add("WLH", new CheckBox("Use [W] LastHit"));
            LastHitMenu.Add("ManaLH", new Slider("Mana For LaneClear", 50));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("QJungle", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("WJungle", new CheckBox("Use [W] JungleClear"));
            JungleClearMenu.Add("EJungle", new CheckBox("Use [E] JungleClear"));
            JungleClearMenu.Add("RJungle", new CheckBox("Use [R] JungleClear"));
            JungleClearMenu.Add("StackRJ", new Slider("Use [R] Stacks Limit LaneClear", 3, 1, 5));
            JungleClearMenu.Add("ManaJC", new Slider("Mana For JungleClear", 30));

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsR", new CheckBox("Use [R] KillSteal", false));
            KillStealMenu.Add("KsE", new CheckBox("Use [E] KillSteal"));
            KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
            KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));

            Misc = Menu.AddSubMenu("Misc Settings", "Misc");
            Misc.AddGroupLabel("Skin Settings");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Misc.Add("skin.Id", new ComboBox("Skin Mode", 5, "Default", "1", "2", "3", "4", "5"));
            Misc.AddGroupLabel("Drawing Settings");
            Misc.Add("DrawR", new CheckBox("R Range", false));
            Misc.Add("DrawQ", new CheckBox("Q Range"));
            Misc.Add("DrawE", new CheckBox("E Range", false));
            Misc.Add("DrawTR", new CheckBox("DrawText Status [R]"));
            Misc.AddGroupLabel("Interrupt Settings");
            Misc.Add("inter", new CheckBox("Use [Q] Interupt"));
            Misc.Add("AntiGap", new CheckBox("Use [E] Anti Gapcloser"));
            Misc.AddGroupLabel("Seraph Settings");
            Misc.Add("dts", new CheckBox("Use Seraph"));
            Misc.Add("Hp", new Slider("HP For Seraph", 30, 0, 100));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnTick += Game_OnTick;
            Interrupter.OnInterruptableSpell += Interupt;
            Orbwalker.OnPostAttack += ResetAttack;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Misc["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
            if (Misc["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (Misc["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
            if (Misc["DrawTR"].Cast<CheckBox>().CurrentValue)
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue)
                {
                    DrawFont(thm, "Use R Under Turret : Disable", (float)(ft[0] - 70), (float)(ft[1] + 50), SharpDX.Color.White);
                }
                else
                {
                    DrawFont(thm, "Use R Under Turret : Enable", (float)(ft[0] - 70), (float)(ft[1] + 50), SharpDX.Color.Red);
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
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            KillSteal();
            Dtc();
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
        public static bool EReady
        {
            get { return Player.HasBuff("ForcePulseAvailable"); }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var turret = ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue;
            var minR = ComboMenu["CMinR"].Cast<Slider>().CurrentValue;
            var Minhp = ComboMenu["Cihp"].Cast<Slider>().CurrentValue;
            if (target != null)
            {
                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && EReady)
                {
                    E.Cast(target);
                }
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (_Player.Distance(target) < Player.Instance.GetAutoAttackRange(target) && !W.IsReady())
                    {
                        Q.Cast(target);
                    }
                    else if (_Player.Distance(target) > Player.Instance.GetAutoAttackRange(target))
                    {
                        Q.Cast(target);
                    }
                }
                if (useR && R.IsReady() && target.IsValidTarget(R.Range) && target.Position.CountEnemiesInRange(R.Range) <= minR && _Player.HealthPercent >= Minhp)
                {
                    if (turret)
                    {
                        if (!UnderTuret(target))
                        {
                           R.Cast(target);
                        }
                    }
                    else
                    {
                       R.Cast(target);
                    }
                }
            }
        }

        private static void ResetAttack(AttackableUnit target, EventArgs args)
        {
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var HasW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var JungleW = JungleClearMenu["WJungle"].Cast<CheckBox>().CurrentValue;
            if (useW && W.IsReady() && target.IsValidTarget(300) && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                W.Cast();
                Orbwalker.ResetAutoAttack();
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
            if (HasW && W.IsReady() && target.IsValidTarget(300) && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                W.Cast();
                Orbwalker.ResetAutoAttack();
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
            if (JungleW && W.IsReady() && target.IsValidTarget(300) && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                W.Cast();
                Orbwalker.ResetAutoAttack();
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
        }

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["QLC"].Cast<CheckBox>().CurrentValue;
            var useW = LaneClearMenu["WLC"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["ELC"].Cast<CheckBox>().CurrentValue;
            var useR = LaneClearMenu["RLC"].Cast<CheckBox>().CurrentValue;
            var MinE = LaneClearMenu["MinELC"].Cast<Slider>().CurrentValue;
            var minRs = LaneClearMenu["StackRL"].Cast<Slider>().CurrentValue;
            var mana = LaneClearMenu["ManaLC"].Cast<Slider>().CurrentValue;
            var minionQ = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(e => e.IsValidTarget(E.Range));
            var quang = EntityManager.MinionsAndMonsters.GetLineFarmLocation(minionQ, E.Width, (int) E.Range);
            foreach (var minions in minionQ)
            {
                if (useW && W.IsReady() && minions.IsValidTarget(175) && Player.Instance.GetSpellDamage(minions, SpellSlot.W) + _Player.GetAutoAttackDamage(minions) > minions.TotalShieldHealth())
                {
                    W.Cast();
                }
                if (Player.Instance.ManaPercent < mana) return;
                if (useQ && Q.IsReady() && minions.IsValidTarget(Q.Range) && Player.Instance.GetSpellDamage(minions, SpellSlot.Q) > minions.TotalShieldHealth())
                {
                    Q.Cast(minions);
                }
                if (useE && E.IsReady() && EReady && minions.IsValidTarget(E.Range) && quang.HitNumber >= MinE)
                {
                    E.Cast(minions);
                }
                if (useR && R.IsReady() && minions.IsValidTarget(R.Range) && !UnderTuret(minions) && Player.Instance.GetBuffCount("RiftWalk") < minRs)
                {
                    R.Cast(minions);
                }
            }
        }

        private static void LastHit()
        {
            var useQ = LastHitMenu["QLH"].Cast<CheckBox>().CurrentValue;
            var useW = LastHitMenu["WLH"].Cast<CheckBox>().CurrentValue;
            var mana = LastHitMenu["ManaLH"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && Player.Instance.GetSpellDamage(minion, SpellSlot.W) + _Player.GetAutoAttackDamage(minion) > minion.TotalShieldHealth() && Player.Instance.ManaPercent >= mana)
                {
                    Q.Cast(minion);
                }
                if (useW && W.IsReady() && minion.IsValidTarget(300) && Player.Instance.GetSpellDamage(minion, SpellSlot.W) > minion.TotalShieldHealth() && !Q.IsReady())
                {
                    W.Cast();
                }
            }
        }

        private static bool UnderTuret(Obj_AI_Base target)
        {
            var tower = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(turret => turret != null && turret.Distance(target) <= 775 && turret.IsValid && turret.Health > 0 && !turret.IsAlly);
            return tower != null;
        }

        private static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var useR = HarassMenu["HarassR"].Cast<CheckBox>().CurrentValue;
            var minRs = HarassMenu["StackRH"].Cast<Slider>().CurrentValue;
            var turret = ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue;
            var minR = HarassMenu["MinR"].Cast<Slider>().CurrentValue;
            var Minhp = HarassMenu["ihp"].Cast<Slider>().CurrentValue;
            var mana = HarassMenu["ManaHR"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            if (Player.Instance.ManaPercent < mana) return;
            if (target != null)
            {
                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && EReady)
                {
                    E.Cast(target);
                }
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie)
                {
                    if (_Player.Distance(target) < Player.Instance.GetAutoAttackRange(target) && !W.IsReady())
                    {
                        Q.Cast(target);
                    }
                    else if (_Player.Distance(target) > Player.Instance.GetAutoAttackRange(target))
                    {
                        Q.Cast(target);
                    }
                }
                if (useR && R.IsReady() && target.IsValidTarget(R.Range) && target.Position.CountEnemiesInRange(R.Range) <= minR && _Player.HealthPercent >= Minhp && Player.Instance.GetBuffCount("RiftWalk") < minRs)
                {
                    if (turret)
                    {
                        if (!UnderTuret(target))
                        {
                           R.Cast(target);
                        }
                    }
                    else
                    {
                       R.Cast(target);
                    }
                }
            }
        }

        public static void JungleClear()
        {
            var useQ = JungleClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var useE = JungleClearMenu["EJungle"].Cast<CheckBox>().CurrentValue;
            var useR = JungleClearMenu["RJungle"].Cast<CheckBox>().CurrentValue;
            var minRs = JungleClearMenu["StackRJ"].Cast<Slider>().CurrentValue;
            var mana = JungleClearMenu["ManaJC"].Cast<Slider>().CurrentValue;
            var jungleMonsters = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(Q.Range));
            if (Player.Instance.ManaPercent < mana) return;
            if (jungleMonsters != null)
            {
                if (useQ && Q.IsReady() && jungleMonsters.IsValidTarget(Q.Range))
                {
                    Q.Cast(jungleMonsters);
                }
                if (useE && E.IsReady() && jungleMonsters.IsValidTarget(E.Range) && EReady)
                {
                    E.Cast(jungleMonsters);
                }
                if (useR && R.IsReady() && jungleMonsters.IsValidTarget(R.Range) && Player.Instance.GetBuffCount("RiftWalk") < minRs)
                {
                    R.Cast(jungleMonsters);
                }
            }
        }

        public static void DrawFont(Font vFont, string vText, float jx, float jy, ColorBGRA jc)
        {
            vFont.DrawText(null, vText, (int)jx, (int)jy, jc);
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (Misc["AntiGap"].Cast<CheckBox>().CurrentValue && sender.IsEnemy && e.Sender.Distance(_Player) < 300)
            {
                E.Cast(e.Sender);
            }
        }

        public static void Interupt(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs i)
        {
            var Inter = Misc["inter"].Cast<CheckBox>().CurrentValue;
            if (!sender.IsEnemy || !(sender is AIHeroClient) || Player.Instance.IsRecalling())
            {
                return;
            }
            if (Inter && Q.IsReady() && i.DangerLevel == DangerLevel.High && Q.IsInRange(sender))
            {
                Q.Cast(sender);
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

        private static void Dtc()
        {
            if (!_Player.IsDead && Misc["dts"].Cast<CheckBox>().CurrentValue)
            {
                if (Seraph.IsOwned() && Seraph.IsReady() && _Player.HealthPercent <= Misc["Hp"].Cast<Slider>().CurrentValue && _Player.Position.CountEnemiesInRange(R.Range) >= 1)
                {
                    Seraph.Cast();
                }
            }
        }

        private static void KillSteal()
        {
            var KsE = KillStealMenu["KsE"].Cast<CheckBox>().CurrentValue;
            var KsR = KillStealMenu["KsR"].Cast<CheckBox>().CurrentValue;
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(Q.Range) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie && (hero.HealthPercent <= 25)))
            {
                if (KsE && E.IsReady() && target.IsValidTarget(E.Range) && EReady)
                {
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.E))
                    {
                        E.Cast(target);
                    }
                }
                if (KsR && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.R))
                    {
                        R.Cast(target);
                    }
                }
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
