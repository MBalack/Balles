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

namespace Bristana
{
    static class Program
    {
        public static Spell.Active Q;
        public static Spell.Skillshot W;
        public static Spell.Targeted E;
        public static Spell.Targeted R;
        public static Font Thm;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Menu Menu, SpellMenu, JungleMenu, HarassMenu, LaneMenu, Misc, Skin;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

// Menu

        private static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Tristana")) return;
            Chat.Print("Doctor's Tristana Loaded!", Color.Orange);
            uint level = (uint)Player.Instance.Level;
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Circular, 450, int.MaxValue, 180);
            E = new Spell.Targeted(SpellSlot.E, 550 + level * 7);
            R = new Spell.Targeted(SpellSlot.R, 550 + level * 7);
            Thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 32, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Menu = MainMenu.AddMenu("Tristana", "Tristana");
            Menu.AddGroupLabel("Tristana");
            SpellMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            SpellMenu.AddGroupLabel("Combo Settings");
            SpellMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            SpellMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            SpellMenu.AddGroupLabel("Combo [E] On");
            foreach (var target in EntityManager.Heroes.Enemies)
            {
                SpellMenu.Add("useECombo" + target.ChampionName, new CheckBox("" + target.ChampionName));
            }
            SpellMenu.AddGroupLabel("KillSteal Settings");
            SpellMenu.Add("ERKs", new CheckBox("KillSteal [ER]"));
            SpellMenu.Add("RKs", new CheckBox("Automatic [R] KillSteal"));
            SpellMenu.Add("RKb", new KeyBind(" Semi [R] KillSteal", false, KeyBind.BindTypes.HoldActive, 'R'));
            SpellMenu.Add("WKs", new CheckBox("Use [W] KillSteal", false));
            SpellMenu.Add("CTurret", new CheckBox("Dont Use [W] KillSteal Under Turet"));
            SpellMenu.Add("Attack", new Slider("Use [W] KillSteal If Can Kill Enemy With x Attack", 2, 1, 6));
            SpellMenu.Add("MinW", new Slider("Use [W] KillSteal If Enemies Around Target <", 2, 1, 5));
            SpellMenu.AddLabel("Always Use [W] KillSteal If Slider Enemies Around = 5");

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass", false));
            HarassMenu.Add("HarassE", new CheckBox("Use [E] Harass"));
            HarassMenu.AddSeparator();
            HarassMenu.AddGroupLabel("Use [E] On");
            foreach (var target in EntityManager.Heroes.Enemies)
            {
                HarassMenu.Add("HarassE" + target.ChampionName, new CheckBox("" + target.ChampionName));
            }
            HarassMenu.Add("manaHarass", new Slider("Min Mana For Harass", 50, 0, 100));

            LaneMenu = Menu.AddSubMenu("Laneclear Settings", "Clear");
            LaneMenu.AddGroupLabel("Laneclear Settings");
            LaneMenu.Add("ClearQ", new CheckBox("Use [Q] Laneclear", false));
            LaneMenu.Add("ClearE", new CheckBox("Use [E] Laneclear", false));
            LaneMenu.Add("manaFarm", new Slider("Min Mana For LaneClear", 50, 0, 100));

            JungleMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleMenu.AddGroupLabel("JungleClear Settings");
            JungleMenu.Add("jungleQ", new CheckBox("Use [Q] JungleClear"));
            JungleMenu.Add("jungleE", new CheckBox("Use [E] JungleClear"));
            JungleMenu.Add("jungleW", new CheckBox("Use [W] JungleClear", false));
            JungleMenu.Add("manaJung", new Slider("Min Mana For JungleClear", 50, 0, 100));

            Misc = Menu.AddSubMenu("Misc Settings", "Draw");
            Misc.AddGroupLabel("Anti Gapcloser");
            Misc.Add("antiGap", new CheckBox("Anti Gapcloser", false));
            Misc.Add("antiRengar", new CheckBox("Anti Rengar"));
            Misc.Add("antiKZ", new CheckBox("Anti Kha'Zix"));
            Misc.Add("inter", new CheckBox("Use [R] Interupt", false));
            Misc.AddGroupLabel("Drawings Settings");
            Misc.Add("Draw_Disabled", new CheckBox("Disabled Drawings", false));
            Misc.Add("DrawE", new CheckBox("Draw Attack Range"));
            Misc.Add("DrawW", new CheckBox("Draw [W]", false));
            Misc.Add("Notifications", new CheckBox("Notifications Can Kill With [R]"));

            Skin = Menu.AddSubMenu("Skin Changer", "SkinChanger");
            Skin.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Skin.Add("skin.Id", new ComboBox("Skin Mode", 0, "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11"));

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interupt;
            GameObject.OnCreate += GameObject_OnCreate;

        }

// Flee Mode

        private static void Flee()
        {
            if (W.IsReady())
            {
                var cursorPos = Game.CursorPos;
                var castPos = Player.Instance.Position.Distance(cursorPos) <= W.Range ? cursorPos : Player.Instance.Position.Extend(cursorPos, W.Range).To3D();
                W.Cast(castPos);
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

        public static void Interupt(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs i)
        {
            var Inter = Misc["inter"].Cast<CheckBox>().CurrentValue;
            if (!sender.IsEnemy || !(sender is AIHeroClient) || Player.Instance.IsRecalling())
            {
                return;
            }
            if (Inter && R.IsReady() && i.DangerLevel == DangerLevel.High && R.IsInRange(sender))
            {
                R.Cast(sender);
            }
        }

//Harass Mode

        private static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["manaHarass"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(E.Range) && !e.IsDead && !e.IsZombie))
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(E.Range))
                {
                    Q.Cast();
                }
                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && target.HealthPercent > 10 && _Player.ManaPercent > mana)
                {
                    if (HarassMenu["HarassE" + target.ChampionName].Cast<CheckBox>().CurrentValue)
                    {
                        E.Cast(target);
                    }
                }
            }
        }

//Combo Mode

        private static void Combo()
        {
            var useQ = SpellMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useE = SpellMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(E.Range) && !e.IsDead && !e.IsZombie))
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(E.Range))
                {
                    Q.Cast();
                }
                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && target.HealthPercent > 10)
                {
                    if (SpellMenu["useECombo" + target.ChampionName].Cast<CheckBox>().CurrentValue)
                    {
                        E.Cast(target);
                    }
                }
            }
        }

//LaneClear Mode

        private static void LaneClear()
        {
            var useQ = LaneMenu["ClearQ"].Cast<CheckBox>().CurrentValue;
            var useE = LaneMenu["ClearE"].Cast<CheckBox>().CurrentValue;
            var mana = LaneMenu["manaFarm"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useE && E.IsReady() && minion.HealthPercent > 70 && minion.IsValidTarget(E.Range) && _Player.ManaPercent > mana)
                {
                    E.Cast(minion);
                }
                if (useQ && Q.IsReady() && minion.IsValidTarget(E.Range) && minions.Count() >= 3)
                {
                    Q.Cast();
                }
            }
        }

// JungleClear Mode

        private static void JungleClear()
        {
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(E.Range));
            var useQ = JungleMenu["jungleQ"].Cast<CheckBox>().CurrentValue;
            var useW = JungleMenu["jungleW"].Cast<CheckBox>().CurrentValue;
            var useE = JungleMenu["jungleE"].Cast<CheckBox>().CurrentValue;
            var mana = JungleMenu["manaJung"].Cast<Slider>().CurrentValue;
            if (monster != null)
            {
                if (useQ && Q.IsReady() && monster.IsValidTarget(E.Range))
                {
                    Q.Cast();
                }
                if (useW && W.IsReady() && monster.IsValidTarget(W.Range) && _Player.ManaPercent > mana)
                {
                    W.Cast(monster.Position);
                }
                if (useE && E.IsReady() && monster.IsValidTarget(E.Range) && _Player.ManaPercent > mana)
                {
                    E.Cast(monster);
                }
            }
        }

        public static void DrawFont(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }

// Anti Rengar

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var renga = EntityManager.Heroes.Enemies.Find(e => e.ChampionName.Equals("Rengar"));
            var khazix = EntityManager.Heroes.Enemies.Find(e => e.ChampionName.Equals("Khazix"));
            if (renga != null)
            {
                if (sender.Name == ("Rengar_LeapSound.troy") && Misc["antiRengar"].Cast<CheckBox>().CurrentValue && sender.Position.Distance(_Player) < 300)
                {
                    R.Cast(renga);
                }
            }
            if (khazix != null)
            {
                if (sender.Name == ("Khazix_Base_E_Tar.troy") && Misc["antiKZ"].Cast<CheckBox>().CurrentValue && sender.Position.Distance(_Player) < 300)
                {
                    R.Cast(khazix);
                }
            }
        }

// EDamage

        private static float EDamage(Obj_AI_Base target)
        {
            float Edamage = 0;
            if (target.HasBuff("tristanaecharge"))
            {
                Edamage += (float)(Player.Instance.GetSpellDamage(target, SpellSlot.E) * (target.GetBuffCount("tristanaecharge") * 0.30)) + Player.Instance.GetSpellDamage(target, SpellSlot.E);
            }

            return Edamage;
        }

// KillSteal

        private static void KillSteal()
        {
            var target = EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(W.Range) && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && !e.IsDead && !e.IsZombie);
            var targetE = EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(R.Range) && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && e.HasBuff("tristanaecharge") && !e.IsDead && !e.IsZombie);
            var RKill = SpellMenu["RKs"].Cast<CheckBox>().CurrentValue;
            var WKill = SpellMenu["WKs"].Cast<CheckBox>().CurrentValue;
            var WAttack = SpellMenu["Attack"].Cast<Slider>().CurrentValue;
            var minW = SpellMenu["MinW"].Cast<Slider>().CurrentValue;
            foreach (var target2 in target)
            {
                if (RKill && R.IsReady() || SpellMenu["RKb"].Cast<KeyBind>().CurrentValue)
                {
                    if (target2.Health + target2.AttackShield < Player.Instance.GetSpellDamage(target2, SpellSlot.R) && target2.IsValidTarget(R.Range))
                    {
                        R.Cast(target2);
                    }
                }
                if (WKill && W.IsReady())
                {
                    if (target2.Health + target2.AttackShield < Player.Instance.GetAutoAttackDamage(target2) * WAttack && Player.Instance.Mana > W.Handle.SData.Mana * 2 && Player.Instance.HealthPercent > 25 && target2.Position.CountEnemiesInRange(400) <= minW)
                    {
                        var turret = SpellMenu["CTurret"].Cast<CheckBox>().CurrentValue;
                        if (target2.HasBuff("tristanaecharge"))
                        {
                            if (target2.Health + target2.AttackShield > EDamage(target2))
                            {
                                if (turret)
                                {
                                    if (!target2.Position.UnderTuret())
                                    {
                                       W.Cast(target2.ServerPosition);
                                    }
                                }
                                else
                                {
                                   W.Cast(target2.ServerPosition);
                                }
                            }
                        }
                        else
                        {
                            if (turret)
                            {
                                if (!target2.Position.UnderTuret())
                                {
                                   W.Cast(target2.ServerPosition);
                                }
                            }
                            else
                            {
                               W.Cast(target2.ServerPosition);
                            }
                        }
                    }
                }
            }
            foreach (var target3 in targetE)
            {
                if (SpellMenu["ERKs"].Cast<CheckBox>().CurrentValue && R.IsReady())
                {
                    if (target3.Health + target3.AttackShield < Player.Instance.GetSpellDamage(target3, SpellSlot.R) + EDamage(target3))
                    {
                        R.Cast(target3);
                    }
                }
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
            if (Misc["DrawW"].Cast<CheckBox>().CurrentValue && W.IsReady())
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2, Radius = W.Range }.Draw(_Player.Position);
            }
            if (Misc["Notifications"].Cast<CheckBox>().CurrentValue && R.IsReady())
            {
                var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (target.IsValidTarget(1000) && Player.Instance.GetSpellDamage(target, SpellSlot.R) > target.Health + target.AttackShield)
                {
                    DrawFont(Thm, "[R] Can Killable " + target.ChampionName, (float)(ft[0] - 140), (float)(ft[1] + 80), SharpDX.Color.Red);
                }
            }
        }

// Under Turet

        public static bool UnderTuret(this Vector3 position)
        {
            return EntityManager.Turrets.Enemies.Where(a => a.Health > 0 && !a.IsDead).Any(a => a.Distance(position) < 950);
        }

// Game Update

        private static void Game_OnUpdate(EventArgs args)
        {
            uint level = (uint)Player.Instance.Level;
            E = new Spell.Targeted(SpellSlot.E, 550 + level * 7);
            R = new Spell.Targeted(SpellSlot.R, 550 + level * 7);

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
            if (_Player.SkinId != Skin["skin.Id"].Cast<ComboBox>().CurrentValue)
            {
                if (checkSkin())
                {
                    Player.SetSkinId(SkinId());
                }
            }
        }

        private static void Gapcloser_OnGapCloser(Obj_AI_Base sender, Gapcloser.GapcloserEventArgs args)
        {
            if (Misc["antiGap"].Cast<CheckBox>().CurrentValue && R.IsReady() && args.Sender.Distance(_Player) < 325)
            {
                R.Cast(args.Sender);
            }
        }
    }
}
