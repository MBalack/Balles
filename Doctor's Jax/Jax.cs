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
using Circle = EloBuddy.SDK.Rendering.Circle;
using Color = System.Drawing.Color;

namespace Jax
{
    internal class Program
    {
        public const string ChampionName = "Jax";
        public static Menu Menu, ComboMenu, Autos, HarassMenu, LaneClearMenu, JungleClearMenu, Misc, KillStealMenu, Drawings;
        public static Item Botrk;
        public static Item Bil;
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

        static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Jax")) return;
            Chat.Print("Doctor's Jax Loaded!", Color.Yellow);
            Q = new Spell.Targeted(SpellSlot.Q, 700);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Active(SpellSlot.E,350);
            R= new Spell.Active(SpellSlot.R);
            Botrk = new Item( ItemId.Blade_of_the_Ruined_King);
            Bil = new Item(3144, 475f);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Menu = MainMenu.AddMenu("Doctor's Jax", "Jax");
            Menu.AddSeparator();
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("comboMode", new ComboBox("Combo Mode:", 1, "E => Q", "Q => E"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("ComboQ", new CheckBox("Combo [Q]"));
            ComboMenu.Add("ComboW", new CheckBox("Combo [W]"));
            ComboMenu.Add("ComboE", new CheckBox("Combo [E]"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("ComboR", new CheckBox("Combo [R]"));
            ComboMenu.Add("MinR", new Slider("Min Enemies Use [R]", 2, 1, 5));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Harass [Q]", false));
            HarassMenu.Add("HarassW", new CheckBox("Harass [W]"));
            HarassMenu.Add("HarassE", new CheckBox("Harass [E]"));
            HarassMenu.Add("ManaQ", new Slider("Min Mana For Harass", 30));

            LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("LaneClear Settings");
            LaneClearMenu.Add("LCQ", new CheckBox("Lane Clear [Q]", false));
            LaneClearMenu.Add("LCW", new CheckBox("Lane Clear [W]"));
            LaneClearMenu.Add("LCE", new CheckBox("Lane Clear [E]", false));
            LaneClearMenu.Add("ManaLC", new Slider("Min Mana LaneClear [Q]", 60));
            LaneClearMenu.AddSeparator();
            LaneClearMenu.AddGroupLabel("LastHit Settings");
            LaneClearMenu.Add("LHQ", new CheckBox("Lane Clear [Q]", false));
            LaneClearMenu.Add("LHW", new CheckBox("Lane Clear [W]"));
            LaneClearMenu.Add("ManaLH", new Slider("Min Mana LaneClear [Q]", 60));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("QJungle", new CheckBox("Spell [Q]"));
            JungleClearMenu.Add("WJungle", new CheckBox("Spell [W]"));
            JungleClearMenu.Add("EJungle", new CheckBox("Spell [E]"));
            JungleClearMenu.Add("MnJungle", new Slider("Min Mana For JungleClear", 30));

            Misc = Menu.AddSubMenu("Misc Settings", "Misc");
            Misc.AddGroupLabel("AntiGap Settings");
            Misc.Add("antiGap", new CheckBox("Use [E] AntiGapcloser"));
            Misc.AddGroupLabel("Items Settings");
            Misc.Add("BOTRK", new CheckBox("Use [Botrk]"));
            Misc.Add("ihp", new Slider("My HP Use BOTRK <=", 50));
            Misc.Add("ihpp", new Slider("Enemy HP Use BOTRK <=", 50));
            Misc.AddGroupLabel("Skin Changer");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer"));
            Misc.Add("skin.Id", new ComboBox("Skin Mode", 1, "Default", "1", "2", "3", "4", "5", "6", "7", "8"));

            Autos = Menu.AddSubMenu("Auto Spell Settings", "Autos");
            Autos.AddGroupLabel("Automatic Settings");
            Autos.Add("AutoE", new CheckBox("Auto [E] Enemies In Range"));
            Autos.Add("minE", new Slider("Min Enemies Auto [E]", 2, 1, 5));
            Autos.Add("AutoR", new CheckBox("Auto [R] If My HP =<"));
            Autos.Add("mauR", new Slider("My HP Auto [R]", 50));

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsQ", new CheckBox("[Q] KillSteal", false));
            KillStealMenu.Add("ign", new CheckBox("[Ignite] KillSteal"));

            Drawings = Menu.AddSubMenu("Draw Settings", "Draw");
            Drawings.AddGroupLabel("Drawing Settings");
            Drawings.Add("DrawQ", new CheckBox("Q Range"));
            Drawings.Add("DrawE", new CheckBox("E Range", false));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnTick += Game_OnTick;
            Orbwalker.OnPostAttack += ResetAttack;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Drawings["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (Drawings["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
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
            var item = Misc["BOTRK"].Cast<CheckBox>().CurrentValue;
            var Minhp = Misc["ihp"].Cast<Slider>().CurrentValue;
            var Minhpp = Misc["ihpp"].Cast<Slider>().CurrentValue;
            var useR = Autos["AutoR"].Cast<CheckBox>().CurrentValue;
            var MinR = Autos["mauR"].Cast<Slider>().CurrentValue;
            var useE = Autos["AutoE"].Cast<CheckBox>().CurrentValue;
            var MinE = Autos["minE"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(550, DamageType.Physical);
            foreach (var targeti in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(475) && !e.IsDead))
            {
                if (item && Bil.IsReady() && Bil.IsOwned() && Bil.IsInRange(targeti))
                {
                    Bil.Cast(targeti);
                }
                if ((item && Botrk.IsReady() && Botrk.IsOwned() && targeti.IsValidTarget(475)) && (Player.Instance.HealthPercent <= Minhp || targeti.HealthPercent < Minhpp))
                {
                    Botrk.Cast(targeti);
                }
            }
            if (useR && R.IsReady() && ObjectManager.Player.Position.CountEnemiesInRange(Q.Range) >= 1 && Player.Instance.HealthPercent <= MinR)
            {
                R.Cast();
            }
            if (useE && E.IsReady() && ObjectManager.Player.Position.CountEnemiesInRange(E.Range) >= MinE)
            {
                E.Cast();
            }
        }
		
        public static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var MinR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
            if (ComboMenu["comboMode"].Cast<ComboBox>().CurrentValue == 0)
            {
                if (target != null)
     	        {
                    if (useE && E.IsReady())
                    {
                        if (!Player.HasBuff("JaxCounterStrike") && target.IsValidTarget(Q.Range) || !Player.HasBuff("JaxCounterStrike") && target.IsValidTarget(E.Range))
                        {
                            E.Cast();
                        }
                        else if (Player.HasBuff("JaxCounterStrike") && target.IsValidTarget(E.Range))
                        {
                            E.Cast();
                        }
	    	    	}
                    if (useQ && Q.IsReady() && Player.Instance.GetAutoAttackRange() <= target.Distance(Player.Instance))
                    {
                        if (target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie)
                        {
                            Core.DelayAction(() => Q.Cast(target), 500);
                        }
                    }
                }
            }
            if (ComboMenu["comboMode"].Cast<ComboBox>().CurrentValue == 1)
            {
                if (target != null)
     	        {
                    if (useQ && Q.IsReady() && Player.Instance.GetAutoAttackRange() <= target.Distance(Player.Instance))
                    {
                        if (target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie)
                        {
                            Q.Cast(target);
                        }
                    }
                    if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                    {
                        E.Cast();
	    	    	}
                }
            }
            if (useR && R.IsReady())
            {
                if (ObjectManager.Player.Position.CountEnemiesInRange(Q.Range) >= MinR)
                {
                    R.Cast();
                }
            }
        }

        public static void ResetAttack(AttackableUnit e, EventArgs args)
        {
            if (!(e is AIHeroClient)) return;
            var target = TargetSelector.GetTarget(300, DamageType.Physical);
            var champ = (AIHeroClient)e;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var HasW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["ManaQ"].Cast<Slider>().CurrentValue;
            if (champ == null || champ.Type != GameObjectType.AIHeroClient || !champ.IsValid) return;
            if (target != null)
            {
                if (useW && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    W.Cast();
                    Orbwalker.ResetAutoAttack();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }

                if (HasW && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && _Player.ManaPercent >= mana)
                {
                    W.Cast();
                    Orbwalker.ResetAutoAttack();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }
            }
        }

        public static void JungleClear()
        {
            var useW = JungleClearMenu["WJungle"].Cast<CheckBox>().CurrentValue;
            var useQ = JungleClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var useE = JungleClearMenu["EJungle"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["MnJungle"].Cast<Slider>().CurrentValue;
            var jungleMonsters = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(Q.Range));
            if (jungleMonsters != null && Player.Instance.ManaPercent >= mana)
            {
                if (useQ && Q.IsReady() && Q.IsInRange(jungleMonsters))
		    	{
                    Q.Cast(jungleMonsters);
                }

                if (useW && W.IsReady() && jungleMonsters.IsValidTarget(275) && jungleMonsters.IsInAutoAttackRange(Player.Instance) && Player.Instance.Distance(jungleMonsters.ServerPosition) <= 225f)
		    	{
                    W.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, jungleMonsters);
                }

                if (useE && E.IsReady())
                {
                    if (!Player.HasBuff("JaxCounterStrike"))
                    {
                        E.Cast();
                    }

                    else if (Player.HasBuff("JaxCounterStrike") && jungleMonsters.IsValidTarget(E.Range))
                    {
                        Core.DelayAction(() => E.Cast(), 1000);
	    	    	}
	    		}
            }
        }

        public static void LaneClear()
        {
            var mana = LaneClearMenu["ManaLC"].Cast<Slider>().CurrentValue;
            var useW = LaneClearMenu["LCW"].Cast<CheckBox>().CurrentValue;
            var useQ = LaneClearMenu["LCQ"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["LCE"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            if (Player.Instance.ManaPercent <= mana) return;
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && Q.IsInRange(minion) && _Player.GetAutoAttackRange() <= minion.Distance(Player.Instance) && Player.Instance.GetSpellDamage(minion, SpellSlot.Q) >= minion.TotalShieldHealth())
		    	{
                    Q.Cast(minion);
                }

                if (useE && E.IsReady() && minion.IsValidTarget(E.Range) && _Player.CountEnemyMinionsInRange(E.Range) >= 2)
                {
                    E.Cast();
	    		}
                if (useW && W.IsReady() && minion.IsValidTarget(275) && minion.IsInAutoAttackRange(Player.Instance)
                && Player.Instance.Distance(minion.ServerPosition) <= 225f
                && Player.Instance.GetSpellDamage(minion, SpellSlot.W) + Player.Instance.GetAutoAttackDamage(minion)
                >= minion.TotalShieldHealth())
                {
                    W.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                }
            }
        }

        public static void LastHit()
        {
            var mana = LaneClearMenu["ManaLH"].Cast<Slider>().CurrentValue;
            var useQ = LaneClearMenu["LHQ"].Cast<CheckBox>().CurrentValue;
            var useW = LaneClearMenu["LHW"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            if (Player.Instance.ManaPercent <= mana) return;
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && Q.IsInRange(minion) && Player.Instance.GetAutoAttackRange() <= minion.Distance(Player.Instance) && Player.Instance.GetSpellDamage(minion, SpellSlot.Q) > minion.TotalShieldHealth())
		    	{
                    Q.Cast(minion);
                }

                if (useW && W.IsReady() && minion.IsValidTarget(275) && minion.IsInAutoAttackRange(Player.Instance)
                && Player.Instance.Distance(minion.ServerPosition) <= 225f
                && Player.Instance.GetSpellDamage(minion, SpellSlot.W) + Player.Instance.GetAutoAttackDamage(minion)
                >= minion.TotalShieldHealth())
                {
                    W.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                }
            }
        }

        public static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var ManaQ = HarassMenu["ManaQ"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target != null && Player.Instance.ManaPercent >= ManaQ)
     	    {
                if (useE && E.IsReady() && !Player.HasBuff("JaxCounterStrike") && target.IsValidTarget(Q.Range))
                {
                    E.Cast();
	    		}
                if (useE && E.IsReady() && Player.HasBuff("JaxCounterStrike") && target.IsValidTarget(E.Range))
                {
                    E.Cast();
	    		}
                if (useQ && Q.IsReady() && Player.Instance.GetAutoAttackRange() <= target.Distance(Player.Instance))
                {
                    if (target.IsValidTarget(Q.Range) && !target.IsDead && !target.IsZombie)
                    {
                        Core.DelayAction(() => Q.Cast(target), 500);
                    }
                }
     	    }
        }

        public static void Flee()
        {
            if (Q.IsReady())
            {
                var CursorPos = Game.CursorPos;
                Obj_AI_Base JumpPlace = EntityManager.Heroes.Allies.FirstOrDefault(w => w.Distance(CursorPos) <= 250 && Q.IsInRange(w));
                if (JumpPlace != default(Obj_AI_Base))
                {
                    Q.Cast(JumpPlace);
                }
                else
                {
                    JumpPlace = EntityManager.MinionsAndMonsters.Minions.FirstOrDefault(w => w.Distance(CursorPos) <= 250 && Q.IsInRange(w));

                    if (JumpPlace != default(Obj_AI_Base))
                    {
                        Q.Cast(JumpPlace);
                    }
                    var Ward2 = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(a => a.IsAlly && a.Distance(CursorPos) < 300);
                    if (Ward2 != null)
                    {
                        Q.Cast(Ward2);
                    }
                    else if (JumpWard() != default(InventorySlot))
                    {
                        var Ward = JumpWard();
                        CursorPos = _Player.Position.Extend(CursorPos, 600).To3D();
                        Ward.Cast(CursorPos);
                        Core.DelayAction(() => WardJump(CursorPos), Game.Ping + 100);
                    }
                }
            }
        }

        public static void WardJump(Vector3 cursorpos)
        {
            var jumpPos = Game.CursorPos;
            var Ward = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(a => a.IsAlly && a.Distance(cursorpos) < 300);
            if (Ward != null)
            {
				Q.Cast(Ward);
			}
        }

        public static ItemId[] WardIds = {ItemId.Warding_Totem_Trinket, ItemId.Greater_Stealth_Totem_Trinket, ItemId.Greater_Vision_Totem_Trinket, ItemId.Sightstone, ItemId.Ruby_Sightstone, (ItemId) 2043, (ItemId)3340, (ItemId)2303,
                (ItemId) 2049, (ItemId) 2045};

        public static InventorySlot JumpWard()
        {
            return WardIds.Select(wardId => Player.Instance.InventoryItems.FirstOrDefault(a => a.Id == wardId)).FirstOrDefault(slot => slot != null && slot.CanUseItem());
        }
        public static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(Q.Range) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.IsDead && !hero.IsZombie))
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
