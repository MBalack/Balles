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
using Color = System.Drawing.Color;

namespace Borki
{
    class Program
    {

        public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Active E;
        public static Spell.Skillshot R;
        public static Item Botrk;
        public static Spell.Targeted Ignite;


        public static Menu Menu,
        SpellMenu,
        HarassMenu,
        ClearMenu,
        KillstealMenu,
        JungleMenu,
        Skin,
        Misc;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
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
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
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

        public static int SkinId()
        {
            return Skin["skin.Id"].Cast<ComboBox>().CurrentValue;
        }
        public static bool checkSkin()
        {
            return Skin["checkSkin"].Cast<CheckBox>().CurrentValue;
        }
		
        private static void Flee()
        {
            if (W.IsReady())
            {
                var cursorPos = Game.CursorPos;
                var castPos = Player.Instance.Position.Distance(cursorPos) <= W.Range ? cursorPos : Player.Instance.Position.Extend(cursorPos, W.Range).To3D();
                W.Cast(castPos);
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (Misc["AntiGap"].Cast<CheckBox>().CurrentValue && W.IsReady() && sender.IsEnemy && sender.IsVisible && sender.IsInRange(Player.Instance, W.Range))
            {
                W.Cast(Player.Instance.Position.Shorten(sender.Position, W.Range));
            }
        }
		
        private static void JungleClear()
        {
            var minions = EntityManager.MinionsAndMonsters.GetJungleMonsters(ObjectManager.Player.ServerPosition, Q.Range).FirstOrDefault(x => x.IsValidTarget(Q.Range));
            if (minions != null)
            {
                if (Q.IsReady() && JungleMenu["JungleQ"].Cast<CheckBox>().CurrentValue && _Player.ManaPercent >= JungleMenu["manaJung"].Cast<Slider>().CurrentValue)
                {
                    Q.Cast(minions);
                }
                if (R.IsReady() && JungleMenu["JungleR"].Cast<CheckBox>().CurrentValue && _Player.ManaPercent >= JungleMenu["manaJung"].Cast<Slider>().CurrentValue
                    && R.Handle.Ammo > JungleMenu["RocketJung"].Cast<Slider>().CurrentValue)
                {
                    R.Cast(minions);
                }
                if (E.IsReady() && JungleMenu["JungleE"].Cast<CheckBox>().CurrentValue && _Player.ManaPercent >= JungleMenu["manaJung"].Cast<Slider>().CurrentValue)
                {
                    E.Cast();
                }
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            var item = SpellMenu["item"].Cast<CheckBox>().CurrentValue;
            if (target != null)
            {
                if (SpellMenu["ComboQ"].Cast<CheckBox>().CurrentValue && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }
                if (SpellMenu["ComboR"].Cast<CheckBox>().CurrentValue && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    R.Cast(target);
                }
                if (SpellMenu["ComboE"].Cast<CheckBox>().CurrentValue && target.IsValidTarget(E.Range) && E.IsReady() && ObjectManager.Player.IsInAutoAttackRange(target))
                {
                    E.Cast();
                }
                if (Player.Instance.HealthPercent <= 50 || target.HealthPercent < 50 && item && Botrk.IsReady() && Botrk.IsOwned())
                {
                    Botrk.Cast(target);
                }
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            var mana = HarassMenu["manaHarass"].Cast<Slider>().CurrentValue;
            if (_Player.ManaPercent < mana)
                return;
            if (target != null)
            {
                if (HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }
                if (HarassMenu["HarassR"].Cast<CheckBox>().CurrentValue && R.IsReady() && target.IsValidTarget(R.Range) && R.Handle.Ammo > HarassMenu["RocketHarass"].Cast<Slider>().CurrentValue)
                {
                    R.Cast(target);
                }
                if (HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue && E.IsReady() && target.IsValidTarget(E.Range) && ObjectManager.Player.IsInAutoAttackRange(target))
                {
                    E.Cast();
                }
            }
        }

        private static void LaneClear()
        {
            var minionQ = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(m => m.IsValidTarget(R.Range))
                .FirstOrDefault(unit => EntityManager.MinionsAndMonsters.EnemyMinions.Count(m => m.Distance(unit) < Q.Radius) > 2);
            if (_Player.ManaPercent < ClearMenu["manaClear"].Cast<Slider>().CurrentValue)
                return;
            if (minionQ != null)
            {
                if (ClearMenu["ClearR"].Cast<CheckBox>().CurrentValue && minionQ.IsValidTarget(R.Range) && R.Handle.Ammo > ClearMenu["RocketClear"].Cast<Slider>().CurrentValue && R.IsReady())
                {
                    R.Cast(minionQ);
                }
                if (ClearMenu["ClearQ"].Cast<CheckBox>().CurrentValue && Q.IsReady() && minionQ.IsValidTarget(Q.Range))
                {
                    Q.Cast(minionQ);
                }
                if (ClearMenu["ClearE"].Cast<CheckBox>().CurrentValue && E.IsReady() && minionQ.IsValidTarget(E.Range))
                {
                    E.Cast();
                }
            }
        }

        private static void KillSteal()
        {

            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(1200) && !e.IsDead && !e.IsZombie && (e.HealthPercent <= 25)))
            {
                if (KillstealMenu["RKs"].Cast<CheckBox>().CurrentValue && R.IsReady() && target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.R))
                {
                    R.Cast(target);
                }

                if (KillstealMenu["QKs"].Cast<CheckBox>().CurrentValue && Q.IsReady() && target.Health + target.AttackShield < Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                {
                    Q.Cast(target);
                }
                if (Ignite != null && KillstealMenu["ign"].Cast<CheckBox>().CurrentValue && Ignite.IsReady())
                {
                    if (target.Health < _Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite))
                    {
                        Ignite.Cast(target);
                    }
                }
            }
        }


        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Misc["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
            if (Misc["drawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (Misc["drawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
            if (Misc["drawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
        }

		
        private static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Corki")) return;
            Chat.Print("Borki Loaded!", Color.GreenYellow);
            Chat.Print("Good Luck!", Color.GreenYellow);
            Bootstrap.Init(null);
            Q = new Spell.Skillshot(SpellSlot.Q, 825, SkillShotType.Circular, 300 , 1000 ,250);
            W = new Spell.Skillshot(SpellSlot.W, 800, SkillShotType.Linear);
            E = new Spell.Active(SpellSlot.E, 600);
            R = new Spell.Skillshot(SpellSlot.R, 1200, SkillShotType.Linear, 200, 1950, 40);
            Botrk = new Item( ItemId.Blade_of_the_Ruined_King);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);

            Menu = MainMenu.AddMenu("Borki", "Borki");
            Menu.AddGroupLabel("BORKI");
            Menu.AddSeparator();
            Menu.AddLabel("Good Luck!");

            SpellMenu = Menu.AddSubMenu("Spells Settings", "Combo");
            SpellMenu.AddGroupLabel("Combo Settings");
            SpellMenu.Add("ComboQ", new CheckBox("Spell [Q]"));
            SpellMenu.Add("ComboE", new CheckBox("Spell [E]"));
            SpellMenu.Add("ComboR", new CheckBox("Spell [R]"));
            SpellMenu.Add("item", new CheckBox("Use [BOTRK]"));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Spell [Q]", false));
            HarassMenu.Add("HarassR", new CheckBox("Spell [R]"));
            HarassMenu.Add("HarassE", new CheckBox("Spell [E]"));
            HarassMenu.Add("manaHarass", new Slider("Min Mana For Harass", 50, 0, 100));
            HarassMenu.Add("RocketHarass", new Slider("Save Rockets [R]", 3, 0, 6));

            ClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            ClearMenu.AddGroupLabel("Laneclear Settings");
            ClearMenu.Add("ClearQ", new CheckBox("Spell [Q]", false));
            ClearMenu.Add("ClearR", new CheckBox("Spell [R]", false));
            ClearMenu.Add("ClearE", new CheckBox("Spell [E]", false));
            ClearMenu.Add("manaClear", new Slider("Min Mana For LaneClear", 65, 0, 100));
            ClearMenu.Add("RocketClear", new Slider("Save Rockets [R]", 3, 0, 6));
			
            JungleMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleMenu.AddGroupLabel("JungleClear Settings");
            JungleMenu.Add("JungleQ", new CheckBox("Spell [Q]"));
            JungleMenu.Add("JungleR", new CheckBox("Spell [R]"));
            JungleMenu.Add("JungleE", new CheckBox("Spell [E]"));
            JungleMenu.Add("manaJung", new Slider("Min Mana For JungleClear", 30, 0, 100));
            JungleMenu.Add("RocketJung", new Slider("Save Rockets [R]", 3, 0, 6));

            KillstealMenu = Menu.AddSubMenu("KillSteal Settings", "KS");
            KillstealMenu.AddGroupLabel("KillSteal Settings");
            KillstealMenu.Add("RKs", new CheckBox("Spell [R]"));
            KillstealMenu.Add("QKs", new CheckBox("Spell [Q]"));
            KillstealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));

            Misc = Menu.AddSubMenu("Misc Settings", "Misc");
            Misc.AddGroupLabel("Misc Settings");
            Misc.Add("AntiGap", new CheckBox("Use [W] AntiGapcloser"));
            Misc.AddSeparator();
            Misc.Add("drawQ", new CheckBox("Range [Q]"));
            Misc.Add("drawW", new CheckBox("Range [W]", false));
            Misc.Add("drawE", new CheckBox("Range [E]"));
            Misc.Add("drawR", new CheckBox("Range [R]"));

            Skin = Menu.AddSubMenu("Skin Changer", "SkinChanger");
            Skin.Add("checkSkin", new CheckBox("Use Skin Changer"));
            Skin.Add("skin.Id", new ComboBox("Skin Mode", 0, "Default", "1", "2", "3", "4", "5", "6", "7"));


            Game.OnTick += Game_OnTick;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;

        }
    }
}