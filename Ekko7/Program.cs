using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;
using Color = System.Drawing.Color;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace Ekko7
{

    class Program
    {

        public static Obj_AI_Base _UltEkko;
        public static Spell.Skillshot Q, Q2, W, E, R;
        public static Spell.Active R1;
        public static Menu Menu,
        ComboMenu, 
        HarassMenu,  
        ClearMenu, 
        JungleMenu, 
        Misc,
        Skin,
        Drawings;
		
        public static AIHeroClient Player
        {
            get { return ObjectManager.Player; }
        }	

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Load;
        }
		
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Drawings["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = Q.Range }.Draw(Player.Position);
            }
            if (Drawings["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = W.Range }.Draw(Player.Position);
            }
            if (Drawings["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = E.Range }.Draw(Player.Position);
            }
        }
		
        private static void LaneClear()
        {
            var useQ = ClearMenu["QClear"].Cast<CheckBox>().CurrentValue;
            var ManaQ = ClearMenu["ClearMana"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && Player.ManaPercent > ManaQ && minions.Count() >= 3)
                {
                    Q.Cast(minion);
                }
			}
        }
		
        private static void JungleClear()
        {
            var useQ = JungleMenu["QJungleClear"].Cast<CheckBox>().CurrentValue;
            var useE = JungleMenu["EJungleClear"].Cast<CheckBox>().CurrentValue;
            var jungMana = JungleMenu["JungleMana"].Cast<Slider>().CurrentValue;
            var qmonster = ObjectManager.Get<Obj_AI_Minion>().OrderBy(m => m.CampNumber).Where(m => m.IsMonster && m.IsEnemy && !m.IsDead);
            var objAiMinions = qmonster as IList<Obj_AI_Minion> ?? qmonster.ToList();
            foreach (var jmob in objAiMinions)
            {
                if (useQ && jmob.IsValidTarget(Q.Range) && objAiMinions.Count() > 1 && Player.ManaPercent > jungMana)
                {
                    Q.Cast(jmob.Position);
                }

                if (useE && jmob.IsValidTarget(E.Range) && Player.ManaPercent > jungMana)
                {
                    E.Cast(jmob.Position);
                }
            }
        }
		
        private static void Harass()
        {
            if (Player.ManaPercent < HarassMenu["HarassMana"].Cast<Slider>().CurrentValue)
                return;
            var useQ = HarassMenu["QHarass"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["WHarass"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["EHarass"].Cast<CheckBox>().CurrentValue;

            var Targetq = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var Wtarget = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            if (!target.IsValidTarget(Q.Range) && target == null)
                return;
			
            if (Wtarget != null && Wtarget.IsValidTarget(W.Range) && W.IsReady())
            {
                if (useW && Player.ManaPercent > HarassMenu["HarassMana"].Cast<Slider>().CurrentValue)
                {
                    W.Cast(Wtarget.Position);
                }
            }

            if (useE && E.IsReady() && Player.ManaPercent > HarassMenu["HarassMana"].Cast<Slider>().CurrentValue)
            {
                var etarget = TargetSelector.GetTarget(E.Range + 425, DamageType.Magical);
                if (etarget != null && etarget.IsValidTarget(E.Range + 425))
                {
                    var vec = Player.ServerPosition.Extend(etarget.ServerPosition, E.Range - 10);

                    if (vec.Distance(target.ServerPosition) < 425 && ShouldE((Vector3)vec))
                    {
                        E.Cast((Vector3)vec);
                    }
                }
            }

            if (useQ && Q.IsReady() && Player.Distance(Targetq) <= Q.Range && Player.ManaPercent > HarassMenu["HarassMana"].Cast<Slider>().CurrentValue)
            {
                Q.Cast(Targetq);
            }
        }

        private static void Combo()
        {
            var useQ = ComboMenu["UseQCombo"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["UseECombo"].Cast<CheckBox>().CurrentValue;
            var useWpred = ComboMenu["UseWCombo"].Cast<CheckBox>().CurrentValue;
            var Whit = ComboMenu["Whit"].Cast<Slider>().CurrentValue;
            var Wtarget = TargetSelector.GetTarget(W.Range, DamageType.Magical);

            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (!target.IsValidTarget(Q.Range) && target == null)
                return;

            if (Wtarget != null && Wtarget.IsValidTarget(W.Range) && W.IsReady())
            {
                if (useWpred)
                {
                    var pred = W.GetPrediction(Wtarget);
                    if (pred.HitChance >= HitChance.High && pred.CastPosition.CountEnemiesInRange(500) >= Whit)
                    {
                        W.Cast(pred.CastPosition);
                    }
                }
            }

            if (useE && E.IsReady())
            {
                var etarget = TargetSelector.GetTarget(E.Range + 425, DamageType.Magical);
                if (etarget != null && etarget.IsValidTarget(E.Range + 425))
                {
                    var vec = Player.ServerPosition.Extend(etarget.ServerPosition, E.Range - 10);

                    if (vec.Distance(target.ServerPosition) < 425)
                    {
                        E.Cast((Vector3)vec);
                    }
                }
            }

            if (useQ && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                var q2Target = TargetSelector.GetTarget(Q2.Range, DamageType.Magical);

                if (qTarget == null || q2Target == null)
                    return;

                var pred = Prediction.Position.PredictLinearMissile(
                    qTarget,
                    Q.Range,
                    Q.Width,
                    Q.CastDelay,
                    Q.Speed,
                    Q.AllowedCollisionCount);
                var pred2 = Prediction.Position.PredictLinearMissile(
                    q2Target,
                    Q2.Range,
                    Q2.Width,
                    Q2.CastDelay,
                    Q2.Speed,
                    Q2.AllowedCollisionCount);
                if (pred.HitChance >= HitChance.High)
                {
                    Q.Cast(pred.CastPosition);
                }
                else if (pred2.HitChance >= HitChance.High)
                {
                    Q2.Cast(pred2.CastPosition);
                }
            }
        }		

        private static bool ShouldE(Vector3 vec)
        {
            var maxEnemies = HarassMenu["DontE"].Cast<Slider>().CurrentValue;

            if (Player.HealthPercent <= HarassMenu["EHP"].Cast<Slider>().CurrentValue)
                return false;

            if (vec.CountEnemiesInRange(600) >= maxEnemies)
                return false;

            return true;
        }
        private static void Flee()
        {
            if (E.IsReady())
            {
                var vec = Player.ServerPosition.Extend(Game.CursorPos, E.Range);
                E.Cast((Vector3)vec);
            }
        }

        private static void Rhit()
        {
            if (Misc["UseRHit"].Cast<CheckBox>().CurrentValue && R.IsReady() && _UltEkko != null)
            {
                if ((EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget()).Where(x => Prediction.Position.PredictCircularMissile(x, R.Range, R.Radius, R.CastDelay, R.Speed).UnitPosition.Distance(_UltEkko.ServerPosition) < 400).ToList().Count >= Misc["RHit"].Cast<Slider>().CurrentValue))
                {
                    R1.Cast();
                    return;
                }
            }
        }

        private static void KillSteal()
        {
            if (Misc["RKill"].Cast<CheckBox>().CurrentValue && R.IsReady() && _UltEkko != null)
            {
                if ((from enemie in EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget()).Where(x => Prediction.Position.PredictCircularMissile(x, R.Range, R.Radius, R.CastDelay, R.Speed).UnitPosition.Distance(_UltEkko.ServerPosition) < 400)
                     select enemie).Any())
                {
                    R1.Cast();
                    return;
                }
            }
        }

		
        private static void AIHeroClient_OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            if (!sender.IsEnemy || sender.IsMe || sender is Obj_AI_Minion || !args.Target.IsMe || sender == null
                || args == null)
            {
                return;
            }

            var safeNet = Misc["R_Safe_Net2"].Cast<Slider>().CurrentValue;

            if (safeNet >= Player.HealthPercent)
            {
                R1.Cast();
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

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
            Rhit();
            if (Player.SkinId != Skin["skin.Id"].Cast<ComboBox>().CurrentValue)
            {
                if (checkSkin())
                {
                    Player.SetSkinId(SkinId());
                }
            }
        }
        public static int SkinId()
        {
            return Skin["skin.Id"].Cast<ComboBox>().CurrentValue;
        }
        public static bool checkSkin()
        {
            return Skin["checkSkin"].Cast<CheckBox>().CurrentValue;
        }
        
		private static void Load(EventArgs args)
        {
            if (Player.ChampionName != "Ekko")
            {
                return;
            }
            Chat.Print("Ekko7 Loaded!", Color.Yellow);
            Chat.Print("Doctor7 Good Luck!", Color.GreenYellow);
            Q = new Spell.Skillshot(SpellSlot.Q, 800, SkillShotType.Linear, (int).25, 1700, 60);
            Q2 = new Spell.Skillshot(SpellSlot.Q, 1050, SkillShotType.Linear, (int).5f, 1200, 120);
            W = new Spell.Skillshot(SpellSlot.W, 1600, SkillShotType.Circular, (int).5f, int.MaxValue, 350);
            E = new Spell.Skillshot(SpellSlot.E, 352,SkillShotType.Linear);
            R = new Spell.Skillshot(SpellSlot.R, 375, SkillShotType.Circular, (int).1f, int.MaxValue, 375);
            R1 = new Spell.Active(SpellSlot.R);
            Menu = MainMenu.AddMenu("Ekko7", "ekko");
            Menu.AddGroupLabel("EKKO7");
            Menu.AddSeparator();
            Menu.AddLabel("FEATURES ADDON");
            Menu.AddSeparator();
            Menu.AddLabel("Combo Settings");
            Menu.AddLabel("Harass Settings");
            Menu.AddLabel("LaneClear Settings");
            Menu.AddLabel("JungleClear Settings");
            Menu.AddLabel("Flee Settings");
            Menu.AddLabel("Ultimate Settings");
            Menu.AddLabel("Drawings Settings");
            Menu.AddLabel("KillSteal Settings");

            ComboMenu = Menu.AddSubMenu("Combo Settings", "combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("UseQCombo", new CheckBox("Spell [Q]"));
            ComboMenu.Add("UseECombo", new CheckBox("Spell [E]"));
            ComboMenu.Add("UseWCombo", new CheckBox("Spell [W]"));
            ComboMenu.Add("Whit", new Slider("Min W Enemies", 1, 1, 5));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("QHarass", new CheckBox("Spell [Q]"));
            HarassMenu.Add("WHarass", new CheckBox("Spell [W]", false));
            HarassMenu.Add("EHarass", new CheckBox("Spell [E]", false));
            HarassMenu.Add("HarassMana", new Slider("Min Mana For Harass", 50));
            HarassMenu.AddSeparator(18);
            HarassMenu.AddLabel("E Harass Settings");
            HarassMenu.Add("DontE", new Slider("Don't Use E >= Enemies", 3, 1, 5 ));
            HarassMenu.Add("EHP", new Slider("Dont Use E HP <= %", 20));
			
            ClearMenu = Menu.AddSubMenu("LaneClear Settings", "Clear Settings");
            ClearMenu.AddLabel("Lane Clear");
            ClearMenu.Add("QClear", new CheckBox("Spell [Q]"));
            ClearMenu.Add("ClearMana", new Slider("Min Mana For LaneClear", 60, 0, 100));
			
            JungleMenu = Menu.AddSubMenu("JungleClear Settings", "Jungle Settings");
            JungleMenu.AddLabel("Jungle Clear");
            JungleMenu.Add("QJungleClear", new CheckBox("Spell [Q]"));
            JungleMenu.Add("EJungleClear", new CheckBox("Spell [E]"));
            JungleMenu.Add("JungleMana", new Slider("Min Mana For JungleClear", 30, 0, 100));

            Misc = Menu.AddSubMenu("Ultimate Settings", "Misc");
            Misc.AddGroupLabel("Ultimate Settings");
            Misc.AddSeparator(18);
            Misc.AddLabel("Ultimate Settings");
            Misc.Add("UseRHit", new CheckBox("Use R Count", false));
            Misc.Add("RHit", new Slider("R Count Enemies >= {0}", 3, 2, 5));
            Misc.Add("R_Safe_Net2", new Slider("R If HP <= %", 25, 0, 100));
            Misc.Add("RKill", new CheckBox("R Killable", false));
			
            Skin = Menu.AddSubMenu("Skin Changer", "SkinChanger");
            Skin.Add("checkSkin", new CheckBox("Use Skin Changer"));
            Skin.Add("skin.Id", new ComboBox("Skin Mode", 0, "Classic", "1", "2"));

            Drawings = Menu.AddSubMenu("Draw Settings", "Draw");
            Drawings.AddGroupLabel("Drawing Settings");
            Drawings.Add("DrawQ", new CheckBox("Q Range"));
            Drawings.Add("DrawW", new CheckBox("W Range", false));
            Drawings.Add("DrawE", new CheckBox("E Range", false));
            Drawings.Add("DrawR", new CheckBox("R Range"));

            AttackableUnit.OnDamage += AIHeroClient_OnDamage;
			Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
        }
    }
}
