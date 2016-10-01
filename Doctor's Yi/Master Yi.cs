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

namespace Yi
{
    class Program
    {
        public static Menu Menu, ComboMenu, Evade, HarassMenu, LaneClearMenu, Misc, Items, KillStealMenu, Drawings;
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
            ComboMenu.Add("WLowHp", new CheckBox("Use [W] Low Hp"));
            ComboMenu.Add("minHealth", new Slider("Use [W] My Hp <=", 25));
            ComboMenu.AddGroupLabel("Use [Q] Dodge Spell");
            ComboMenu.Add("dodge", new CheckBox("Use [Q] Dodge"));
            ComboMenu.Add("antiGap", new CheckBox("Use [Q] Anti Gap"));
            ComboMenu.Add("delay", new Slider("Use [Q] Dodge Delay", 1, 1, 1000));

            Evade = Menu.AddSubMenu("Spell Dodge Settings", "Evade");
            Evade.AddGroupLabel("Dodge Settings");
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(a => a.Team != Player.Instance.Team))
            {
                foreach (var spell in enemy.Spellbook.Spells.Where(a => a.Slot == SpellSlot.Q || a.Slot == SpellSlot.W || a.Slot == SpellSlot.E || a.Slot == SpellSlot.R))
                {
                    if (spell.Slot == SpellSlot.Q)
                    {
                        Evade.Add(spell.SData.Name, new CheckBox(enemy.BaseSkinName + " : " + spell.Slot.ToString() + " : " + spell.Name, false));
                    }
                    else if (spell.Slot == SpellSlot.W)
                    {
                        Evade.Add(spell.SData.Name, new CheckBox(enemy.BaseSkinName + " : " + spell.Slot.ToString() + " : " + spell.Name, false));
                    }
                    else if (spell.Slot == SpellSlot.E)
                    {
                        Evade.Add(spell.SData.Name, new CheckBox(enemy.BaseSkinName + " : " + spell.Slot.ToString() + " : " + spell.Name, false));
                    }
                    else if (spell.Slot == SpellSlot.R)
                    {
                        Evade.Add(spell.SData.Name, new CheckBox(enemy.BaseSkinName + " : " + spell.Slot.ToString() + " : " + spell.Name, false));
                    }
                }
            }

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HarassW", new CheckBox("Use [W] Harass"));
            HarassMenu.Add("HarassE", new CheckBox("Use [E] Harass"));
            HarassMenu.Add("ManaQ", new Slider("Mana Harass", 40));

            LaneClearMenu = Menu.AddSubMenu("Clear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("Clear Settings");
            LaneClearMenu.Add("QLC", new CheckBox("Use [Q] LaneClear"));
            LaneClearMenu.Add("mine", new Slider("Min x Minions Killable Use Q", 3, 1, 4));
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
            Game.OnUpdate += Game_OnUpdate;
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

        private static void Game_OnUpdate(EventArgs args)
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
                if ((item && Botrk.IsReady() && Botrk.IsOwned() && target.IsValidTarget(475)) && (Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp))
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
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var MinR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target != null)
     	    {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && (_Player.Distance(target) > 250 || target.IsDashing() || Player.Instance.HealthPercent <= 20))
                {
                    Q.Cast(target);
                }
				
                if (useE && E.IsReady() && _Player.Distance(target) <= 250)
                {
                    E.Cast();
                }
				
                if (useR && R.IsReady() && (_Player.Position.CountEnemiesInRange(675) >= MinR || Player.Instance.HealthPercent <= 30))
                {
                    R.Cast();
	    		}

                if (useW && Player.Instance.HasBuff("Meditate"))
                {
                    if (_Player.Distance(target) <= Player.Instance.GetAutoAttackRange(target))
                    {
                        Player.IssueOrder(GameObjectOrder.AttackTo, target);
                    }
                    else
                    {
                        if (useQ && Q.IsReady())
                        {
                            Q.Cast(target);
                        }
                    }
                }
            }
        }


        public static void JungleClear()
        {
            var useQ = LaneClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["EJungle"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["MnJungle"].Cast<Slider>().CurrentValue;
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters(_Player.ServerPosition, 625).FirstOrDefault(x => x.IsValidTarget(Q.Range));
            if (monster != null)
            {
                if (useQ && Q.IsReady() && _Player.ManaPercent > mana && monster.IsValidTarget(Q.Range))
		    	{
                    Q.Cast(monster);
                }
				
                if (useE && E.IsReady() && monster.IsValidTarget(225))
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
            var minE = LaneClearMenu["mine"].Cast<Slider>().CurrentValue;
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, _Player.Position, Q.Range).ToArray();
            foreach (var minions in minion)
            {
                if (useQ && Q.IsReady() && Player.Instance.ManaPercent >= mana)
                {
                    int ECal = minion.Where(e => e.Distance(_Player.Position) <= Q.Range && Player.Instance.GetSpellDamage(e, SpellSlot.Q) >= e.Health).Count(); ;
                    if (ECal >= minE)
                    {
                        Q.Cast(minions);
                    }
                }

                if (useE && E.IsReady() && _Player.Position.CountEnemyMinionsInRange(625) >= 3)
                {
                    E.Cast();
                }
            }
        }

        public static void ResetAttack(AttackableUnit e, EventArgs args)
        {
            if (!(e is AIHeroClient)) return;
            var target = TargetSelector.GetTarget(300, DamageType.Physical);
            var champ = (AIHeroClient)e;
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["ManaQ"].Cast<Slider>().CurrentValue;
            var useQC = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            if (champ == null || champ.Type != GameObjectType.AIHeroClient || !champ.IsValid) return;
            if (target != null)
            {
                if (useW && Player.Instance.Distance(target) <= Player.Instance.GetAutoAttackRange() - 50 && _Player.ManaPercent > mana && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                {
                    W.Cast();
                    Orbwalker.ResetAutoAttack();
                    Player.IssueOrder(GameObjectOrder.AttackTo, target);
                }

                if (useQC && Player.Instance.Distance(target) <= Player.Instance.GetAutoAttackRange() - 50 && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    W.Cast();
                    Orbwalker.ResetAutoAttack();
                    Player.IssueOrder(GameObjectOrder.AttackTo, target);
                }
            }
        }

        public static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["ManaQ"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target != null)
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && _Player.ManaPercent > mana && (target.IsDashing() || _Player.Distance(target) > 250))
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
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters().FirstOrDefault(j => j.IsValidTarget(Q.Range));
            if (Enemies != null && Q.IsReady())
            {
                if (Enemies.IsInRange(Game.CursorPos, 250))
                {
                    Q.Cast(Enemies);
                }
            }

            else if (minions != null && Q.IsReady())
            {
                if (minions.IsInRange(Game.CursorPos, 250))
                {
                    Q.Cast(minions);
                }
            }

            else if (monster != null && Q.IsReady())
            {
                if (monster.IsInRange(Game.CursorPos, 250))
                {
                    Q.Cast(monster);
                }
            }
        }

        public static void QEvade()
        {
            var Enemies = EntityManager.Heroes.Enemies.FirstOrDefault(e => e.IsValidTarget(Q.Range));
            var minions = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(m => m.IsValidTarget(Q.Range));
            if (Q.IsReady())
            {
                if (Enemies != null)
                {
                    Core.DelayAction(() => Q.Cast(Enemies), ComboMenu["delay"].Cast<Slider>().CurrentValue);
                }
                if (minions != null)
                {
                    Core.DelayAction(() => Q.Cast(minions), ComboMenu["delay"].Cast<Slider>().CurrentValue);
                }
            }
        }

        private static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var useW = ComboMenu["WLowHp"].Cast<CheckBox>().CurrentValue;
            var MinHealth = ComboMenu["minHealth"].Cast<Slider>().CurrentValue;
            if (useW && !_Player.IsRecalling() && !_Player.IsInShopRange() && _Player.CountEnemiesInRange(425) >= 1)
            {
                if (Player.Instance.HealthPercent <= MinHealth)
                {
                    W.Cast();
                }
            }

            if ((args.Slot == SpellSlot.Q || args.Slot == SpellSlot.W || args.Slot == SpellSlot.E ||
                 args.Slot == SpellSlot.R) && sender.IsEnemy && Q.IsReady())
            {
                if (args.SData.TargettingType == SpellDataTargetType.Unit || args.SData.TargettingType == SpellDataTargetType.SelfAndUnit || args.SData.TargettingType == SpellDataTargetType.Self)
                {
                    if ((args.Target.NetworkId == Player.Instance.NetworkId && args.Time < 1.5 ||
                         args.End.Distance(Player.Instance.ServerPosition) <= Player.Instance.BoundingRadius*3) &&
                        Evade[args.SData.Name].Cast<CheckBox>().CurrentValue)
                    {
                        QEvade();
                    }
                }
                else if (args.SData.TargettingType == SpellDataTargetType.LocationAoe)
                {
                    var castvector =
                        new Geometry.Polygon.Circle(args.End, args.SData.CastRadius).IsInside(
                            Player.Instance.ServerPosition);

                    if (castvector && Evade[args.SData.Name].Cast<CheckBox>().CurrentValue)
                    {
                        QEvade();
                    }
                }

                else if (args.SData.TargettingType == SpellDataTargetType.Cone)
                {
                    var castvector =
                        new Geometry.Polygon.Arc(args.Start, args.End, args.SData.CastConeAngle, args.SData.CastRange)
                            .IsInside(Player.Instance.ServerPosition);

                    if (castvector && Evade[args.SData.Name].Cast<CheckBox>().CurrentValue)
                    {
                        QEvade();
                    }
                }

                else if (args.SData.TargettingType == SpellDataTargetType.SelfAoe)
                {
                    var castvector =
                        new Geometry.Polygon.Circle(sender.ServerPosition, args.SData.CastRadius).IsInside(
                            Player.Instance.ServerPosition);

                    if (castvector && Evade[args.SData.Name].Cast<CheckBox>().CurrentValue)
                    {
                        QEvade();
                    }
                }
                else
                {
                    var castvector =
                        new Geometry.Polygon.Rectangle(args.Start, args.End, args.SData.LineWidth).IsInside(
                            Player.Instance.ServerPosition);

                    if (castvector && Evade[args.SData.Name].Cast<CheckBox>().CurrentValue)
                    {
                        QEvade();
                    }
                }

                if (args.SData.Name == "yasuoq3w")
                {
                    QEvade();
                }

                if (args.SData.Name == "ZedR")
                {
                    if (Q.IsReady())
                    {
                        Core.DelayAction(() => QEvade(), 2000 - Game.Ping - 200);
                    }
                    else
                    {
                        if (W.IsReady())
                        {
                            Core.DelayAction(() => W.Cast(), 500);
                        }
                    }
                }

                if (args.SData.Name == "KarthusFallenOne")
                {
                    Core.DelayAction(() => QEvade(), 2000 - Game.Ping - 200);
                }

                if (args.SData.Name == "SoulShackles")
                {
                    Core.DelayAction(() => QEvade(), 2000 - Game.Ping - 200);
                }

                if (args.SData.Name == "AbsoluteZero")
                {
                    Core.DelayAction(() => QEvade(), 2000 - Game.Ping - 200);
                }
    
                if (args.SData.Name == "NocturneUnspeakableHorror")
                {
                    Core.DelayAction(() => QEvade(), 2000 - Game.Ping - 200);
                }
            }
        }

        public static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q.Range) && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && !e.IsDead && !e.IsZombie))
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