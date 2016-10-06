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

namespace Doctor_s_WuKong
{
    static class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, Ulti, LaneClearMenu, JungleClearMenu, KillStealMenu, Misc;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell.Active Q;
        public static Spell.Active W;
        public static Spell.Targeted E;
        public static Spell.Active R;
        public static Font thm;
        public static Item Hydra;
        public static Item Tiamat;
        public static Spell.Targeted Ignite;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("MonkeyKing")) return;
            Chat.Print("Doctor's Wukong Loaded!", Color.Orange);
            Bootstrap.Init(null);
            Q = new Spell.Active(SpellSlot.Q, 300);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Targeted(SpellSlot.E, 650);
            R = new Spell.Active(SpellSlot.R, 375);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 15, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            Tiamat = new Item( ItemId.Tiamat_Melee_Only, 400);
            Hydra = new Item( ItemId.Ravenous_Hydra_Melee_Only, 400);
            Menu = MainMenu.AddMenu("Doctor's Wukong", "Doctor's Wukong");
            Menu.AddSeparator();
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            ComboMenu.Add("DisE", new Slider("Use [E] If Enemy Distance >", 250, 0, 650));
            ComboMenu.Add("CTurret", new KeyBind("Dont Use [E] UnderTurret", false, KeyBind.BindTypes.PressToggle, 'T'));
            ComboMenu.AddGroupLabel("Items Settings");
            ComboMenu.Add("hydra", new CheckBox("Use [Hydra] Reset AA"));

            Ulti = Menu.AddSubMenu("Ultimate Settings", "Ulti");
            Ulti.AddGroupLabel("Ultimate Enemies In Count");
            Ulti.Add("ultiR", new CheckBox("Use [R] Aoe"));
            Ulti.Add("MinR", new Slider("Min Enemies Use [R] Aoe", 2, 1, 5));
            Ulti.Add("follow", new CheckBox("Auto Move To Target While [R]", false));
            Ulti.AddGroupLabel("Ultimate My HP");
            Ulti.Add("ultiR2", new CheckBox("Use [R] If My HP <"));
            Ulti.Add("MauR", new Slider("My HP Use [R]", 40));
            Ulti.Add("wulti", new CheckBox("Use [W] If My HP <"));
            Ulti.Add("MauW", new Slider("My HP Use [W]", 40));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HarassE", new CheckBox("Use [E] Harass", false) );
            HarassMenu.Add("ManaHR", new Slider("Mana For Harass", 40));

            LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("Lane Clear Settings");
            LaneClearMenu.Add("QLC", new CheckBox("Use [Q] LaneClear", false));
            LaneClearMenu.Add("ELC", new CheckBox("Use [E] LaneClear", false));
            LaneClearMenu.Add("ManaLC", new Slider("Mana For LaneClear", 50));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("QJungle", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("EJungle", new CheckBox("Use [E] JungleClear"));
            JungleClearMenu.Add("ManaJC", new Slider("Mana For JungleClear", 30));

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsR", new CheckBox("Use [R] KillSteal"));
            KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal", false));
            KillStealMenu.Add("KsE", new CheckBox("Use [E] KillSteal", false));
            KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));

            Misc = Menu.AddSubMenu("Misc Settings", "Misc");
            Misc.AddGroupLabel("Skin Settings");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Misc.Add("skin.Id", new ComboBox("Skin Mode", 0, "Default", "1", "2", "3", "4", "5"));
            Misc.AddGroupLabel("Drawing Settings");
            Misc.Add("DrawR", new CheckBox("[R] Range"));
            Misc.Add("DrawE", new CheckBox("[E] Range"));
            Misc.Add("DrawTR", new CheckBox("Status UnderTurret"));
            Misc.AddGroupLabel("Interrupt/Anti Gap Settings");
            Misc.Add("inter", new CheckBox("Use [R] Interupt"));
            Misc.Add("AntiGap", new CheckBox("Use [W] Anti Gapcloser"));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Orbwalker.OnPostAttack += ResetAttack;
            Interrupter.OnInterruptableSpell += Interupt;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Misc["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Black, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
            if (Misc["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Black, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
            if (Misc["DrawTR"].Cast<CheckBox>().CurrentValue)
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue)
                {
                    DrawFont(thm, "Use E Under Turret : Disable", (float)(ft[0] - 70), (float)(ft[1] + 50), SharpDX.Color.White);
                }
                else
                {
                    DrawFont(thm, "Use E Under Turret : Enable", (float)(ft[0] - 70), (float)(ft[1] + 50), SharpDX.Color.Red);
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
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
                Ultimate();

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

        public static bool QPassive
        {
            get { return Player.Instance.HasBuff("monkeykingdoubleattack"); }
        }

        public static bool RActive
        {
            get { return Player.Instance.HasBuff("MonkeyKingSpinToWin"); }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var disE = ComboMenu["DisE"].Cast<Slider>().CurrentValue;
            var turret = ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue;
            if (target != null)
            {
                if (useE && E.IsReady() && !RActive && target.IsValidTarget(E.Range) && disE <= target.Distance(Player.Instance))
                {
                    if (turret)
                    {
                        if (!target.Position.UnderTuret())
                        {
                           E.Cast(target);
                        }
                    }
                    else
                    {
                       E.Cast(target);
                    }
                }

                if (useW && W.IsReady() && !RActive && target.IsValidTarget(375))
                {
                    W.Cast();
                }

            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {	
            if (Misc["AntiGap"].Cast<CheckBox>().CurrentValue && sender.IsEnemy && e.Sender.Distance(_Player) < 300)
            {
                W.Cast();
            }
        }

        private static void Ultimate()
        {
            var target = TargetSelector.GetTarget(R.Range + 175, DamageType.Physical);
            var useR = Ulti["ultiR"].Cast<CheckBox>().CurrentValue;
            var minR = Ulti["MinR"].Cast<Slider>().CurrentValue;
            var useR2 = Ulti["ultiR2"].Cast<CheckBox>().CurrentValue;
            var mauR = Ulti["MauR"].Cast<Slider>().CurrentValue;
            var auto = Ulti["follow"].Cast<CheckBox>().CurrentValue;
            var autow = Ulti["wulti"].Cast<CheckBox>().CurrentValue;
            var mauW = Ulti["MauW"].Cast<Slider>().CurrentValue;
            if (RActive)
            {
                Orbwalker.DisableAttacking = true;
            }
            else
            {
                Orbwalker.DisableAttacking = false;
            }

            if (target != null)
            {
                if (useR && R.IsReady() && _Player.Position.CountEnemiesInRange(R.Range) >= minR && !RActive)
                {
                    R.Cast();
                }
				
                if (useR2 && R.IsReady() && !RActive && _Player.HealthPercent <= mauR && _Player.Position.CountEnemiesInRange(R.Range) >= 1 && !Player.Instance.IsInShopRange())
                {
                    R.Cast();
                }
				
                if (autow && W.IsReady() && !RActive && _Player.HealthPercent <= mauW && _Player.Position.CountEnemiesInRange(R.Range) >= 1 && !Player.Instance.IsInShopRange())
                {
                    W.Cast();
                }
				
                if (auto && target.IsValidTarget() && RActive)
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, target.Position);
                }
            }
        }

        public static void ResetAttack(AttackableUnit e, EventArgs args)
        {
            if (!(e is AIHeroClient)) return;
            var target = TargetSelector.GetTarget(300, DamageType.Physical);
            var champ = (AIHeroClient)e;
            var useriu = ComboMenu["hydra"].Cast<CheckBox>().CurrentValue;
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            if (champ == null || champ.Type != GameObjectType.AIHeroClient || !champ.IsValid) return;
            if (target != null)
            {
                if (useQ && !RActive && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    Q.Cast();
                    Orbwalker.ResetAutoAttack();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }

                if ((useriu && !Q.IsReady() && !QPassive && !RActive) && (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)))
                {
                    if (Hydra.IsOwned(Player.Instance) && Hydra.IsReady() && target.IsValidTarget(250))
                    {
                        Hydra.Cast();
                    }

                    if (Tiamat.IsOwned(Player.Instance) && Tiamat.IsReady() && target.IsValidTarget(250))
                    {
                        Tiamat.Cast();
                    }
                }
            }
        }

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["QLC"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["ELC"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["ManaLC"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            if (Player.Instance.ManaPercent < mana) return;
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(300) && minion.IsInAutoAttackRange(Player.Instance)
                && Player.Instance.Distance(minion.ServerPosition) <= 300
                && Player.Instance.GetSpellDamage(minion, SpellSlot.Q) + Player.Instance.GetAutoAttackDamage(minion)
                >= minion.TotalShieldHealth())
                {
                    Q.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                }

                if (useE && E.IsReady() && minion.IsValidTarget(E.Range))
                {
                    E.Cast(minion);
                }
            }
        }

        private static void Harass()
        {
     	    var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["ManaHR"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target != null && Player.Instance.ManaPercent >= mana)
            {
                if (useQ && Q.IsReady() && !RActive && target.IsValidTarget(300) && _Player.Distance(target) > 175)
                {
                    Q.Cast();
                }

                if (useE && E.IsReady() && !RActive && target.IsValidTarget(E.Range))
                {
                    E.Cast(target);
                }
            }
        }

        public static void JungleClear()
        {
            var useQ = JungleClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var useE = JungleClearMenu["EJungle"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["ManaJC"].Cast<Slider>().CurrentValue;
            var jungleMonsters = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(625));
            if (jungleMonsters != null && Player.Instance.ManaPercent >= mana)
            {
                if (useE && E.IsReady() && jungleMonsters.IsValidTarget(E.Range))
                {
                    E.Cast(jungleMonsters);
                }

                if (useQ && Q.IsReady() && jungleMonsters.IsValidTarget(300))
                {
                    Q.Cast();
                }
            }
        }

        public static void Interupt(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs i)
        {
            var Inter = Misc["inter"].Cast<CheckBox>().CurrentValue;
            if (!sender.IsEnemy || !(sender is AIHeroClient) || Player.Instance.IsRecalling())
            {
                return;
            }
			
            if (Inter && R.IsReady() && !RActive && i.DangerLevel == DangerLevel.High && R.IsInRange(sender))
            {
                R.Cast();
            }
        }

        public static void Flee()
        {
            if (E.IsReady())
            {
                var CursorPos = Game.CursorPos;
                Obj_AI_Base fl = EntityManager.MinionsAndMonsters.Minions.FirstOrDefault(w => w.Distance(CursorPos) <= 250 && E.IsInRange(w));
                if (fl != default(Obj_AI_Base))
                {
                    E.Cast(fl);
                }
                else
                {
                    fl = EntityManager.Heroes.Enemies.FirstOrDefault(w => w.Distance(CursorPos) <= 250 && E.IsInRange(w));
                    if (fl != default(Obj_AI_Base))
                    {
                        E.Cast(fl);
                    }
                }
            }
        }

        public static void DrawFont(Font vFont, string vText, float jx, float jy, ColorBGRA jc)
        {
            vFont.DrawText(null, vText, (int)jx, (int)jy, jc);
        }

        public static bool UnderTuret(this Vector3 position)
        {
            return EntityManager.Turrets.Enemies.Where(a => a.Health > 0 && !a.IsDead).Any(a => a.Distance(position) < 950);
        }

        private static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var KsE = KillStealMenu["KsE"].Cast<CheckBox>().CurrentValue;
            var KsR = KillStealMenu["KsR"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(E.Range) && !hero.HasBuff("BlitzcrankManaBarrierCD") && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie))
            {
                if (KsE && E.IsReady() && !RActive && target.IsValidTarget(E.Range))
                {
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.E) + Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                    {
                        E.Cast(target);
                    }
                }

                if (KsQ && Q.IsReady() && !RActive && target.IsValidTarget(300))
                {
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                    {
                        Q.Cast();
                    }
                }

                if (KsR && R.IsReady() && !RActive && target.IsValidTarget(R.Range))
                {
                    if (target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.R))
                    {
                        R.Cast();
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