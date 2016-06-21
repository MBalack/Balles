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
using Color = System.Drawing.Color;

namespace Bristana
{
    class Program
    {
        public static Spell.Active Q;
        public static Spell.Skillshot W;
        public static Spell.Targeted E;
        public static Spell.Targeted R;
        public static Item Botrk;
        public static Item Bil;
        public static readonly Item Qss = new Item(ItemId.Quicksilver_Sash);
        public static readonly Item Simitar = new Item(ItemId.Mercurial_Scimitar);

        public static Menu Menu,
        SpellMenu,
        JungleMenu,
        HarassMenu,
        LaneMenu,
        Misc,
        Items,
        Skin;

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
            Item();
            Qsss();

            if (_Player.SkinId != Skin["skin.Id"].Cast<ComboBox>().CurrentValue)
            {
                if (checkSkin())
                {
                    Player.SetSkinId(SkinId());
                }
            }

        }

        public static void CastQss()
        {
            if (Qss.IsOwned() && Qss.IsReady())
            {
                Qss.Cast();
            }

            if (Simitar.IsOwned() && Simitar.IsReady())
            {
                Simitar.Cast();
            }
        }

        private static void Qsss()
        {
            if (!Items["Qss"].Cast<CheckBox>().CurrentValue) return;
            if (Items["snare"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Snare))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["tunt"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Taunt))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["stun"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Stun))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["poly"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Polymorph))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["blind"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Blind))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["fear"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Fear))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["charm"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Charm))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["supperss"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Suppression))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["silence"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Silence))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["rot"].Cast<CheckBox>().CurrentValue && _Player.IsRooted)
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
            if (Items["slow"].Cast<CheckBox>().CurrentValue && Player.HasBuffOfType(BuffType.Slow))
            {
                Core.DelayAction(() => CastQss(), Items["delay"].Cast<Slider>().CurrentValue);
            }
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

        public static int SkinId()
        {
            return Skin["skin.Id"].Cast<ComboBox>().CurrentValue;
        }
        public static bool checkSkin()
        {
            return Skin["checkSkin"].Cast<CheckBox>().CurrentValue;
        }

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

        private static void Gapcloser_OnGapCloser(Obj_AI_Base sender, Gapcloser.GapcloserEventArgs args)
        {
            if (Misc["antiGap"].Cast<CheckBox>().CurrentValue && args.Sender.Distance(_Player) < 300)
            {
                R.Cast(args.Sender);
            }
        }

        public static void Item()
        {
            var item = Items["BOTRK"].Cast<CheckBox>().CurrentValue;
            var Minhp = Items["ihp"].Cast<Slider>().CurrentValue;
            var Minhpp = Items["ihpp"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (target != null)
            {
                if (item && Bil.IsReady() && Bil.IsOwned() && target.IsValidTarget(550))
                {
                    Bil.Cast(target);
                }
                else if (item && Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp && Botrk.IsReady() && Botrk.IsOwned() && target.IsValidTarget(550))
                {
                    Botrk.Cast(target);
                }
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target != null)
            {
                if (HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue && Q.IsReady() && target.IsValidTarget(E.Range))
                {
                    Q.Cast();
                }
                if (E.IsReady() && target.IsValidTarget(E.Range) && _Player.ManaPercent > HarassMenu["manaHarass"].Cast<Slider>().CurrentValue)
                {
                    if (HarassMenu["HarassE" + target.ChampionName].Cast<CheckBox>().CurrentValue)
                    {
                        E.Cast(target);
                    }
                }
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target != null)
            {
                if (SpellMenu["ComboQ"].Cast<CheckBox>().CurrentValue && Q.IsReady() && target.IsValidTarget(E.Range))
                {
                    Q.Cast();
                }
                if (E.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (SpellMenu["useECombo" + target.ChampionName].Cast<CheckBox>().CurrentValue)
                    {
                        E.Cast(target);
                    }
                }
            }
        }

        private static void KillSteal()
        {
            var target = EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(R.Range) && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && !e.IsDead && !e.IsZombie && e.HealthPercent <= 25);
            foreach (var target2 in target)
            {
                if (SpellMenu["RKs"].Cast<CheckBox>().CurrentValue && R.IsReady() || SpellMenu["RKb"].Cast<KeyBind>().CurrentValue)
                {
                    if (target2.Health + target2.AttackShield < Player.Instance.GetSpellDamage(target2, SpellSlot.R))
                    {
                        R.Cast(target2);
                    }
                }
            }
        }

        private static void LaneClear()
        {
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().OrderByDescending(m => m.Distance(Player)).FirstOrDefault(m => m.IsValidTarget(E.Range));
            var turret = EntityManager.Turrets.Enemies.FirstOrDefault(t => !t.IsDead && t.Distance(_Player) < _Player.AttackRange);
            if (turret != null)
            {
                if (LaneMenu["ClearTower"].Cast<CheckBox>().CurrentValue && E.IsReady() && turret.IsValidTarget(E.Range) && _Player.ManaPercent > LaneMenu["manaFarm"].Cast<Slider>().CurrentValue)
                {
                    E.Cast(turret);
                }
            }
            if (minion != null)
            {
                if (LaneMenu["ClearE"].Cast<CheckBox>().CurrentValue && E.IsReady() && !turret.IsValidTarget(E.Range) && minion.IsValidTarget(E.Range) && _Player.ManaPercent > LaneMenu["manaFarm"].Cast<Slider>().CurrentValue)
                {
                    E.Cast(minion);
                }
                if (LaneMenu["ClearQ"].Cast<CheckBox>().CurrentValue && Q.IsReady() && minion.IsValidTarget(E.Range))
                {
                    Q.Cast();
                }
            }
        }

        private static void JungleClear()
        {
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, E.Range).OrderByDescending(m => m.MaxHealth).FirstOrDefault();
            if (monster != null)
            {
                if (Q.IsReady() && JungleMenu["jungleQ"].Cast<CheckBox>().CurrentValue && monster.IsValidTarget(E.Range))
                {
                    Q.Cast();
                }
                if (W.IsReady() && JungleMenu["jungleW"].Cast<CheckBox>().CurrentValue && monster.IsValidTarget(W.Range))
                {
                    W.Cast(monster.ServerPosition);
                }
                if (E.IsReady() && JungleMenu["jungleE"].Cast<CheckBox>().CurrentValue && monster.IsValidTarget(E.Range) && _Player.ManaPercent > JungleMenu["manaJung"].Cast<Slider>().CurrentValue)
                {
                    E.Cast(monster);
                }
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var renga = EntityManager.Heroes.Enemies.Find(r => r.ChampionName.Equals("Rengar"));
            var khazix = EntityManager.Heroes.Enemies.Find(r => r.ChampionName.Equals("Khazix"));
            if (renga != null)
            {
                if (sender.Name == ("Rengar_LeapSound.troy") && Misc["antiRengar"].Cast<CheckBox>().CurrentValue && sender.Position.Distance(_Player) < R.Range)
                    R.Cast(renga);
            }
            if (khazix != null)
            {
                if (sender.Name == ("Khazix_Base_E_Tar.troy") && Misc["antiKZ"].Cast<CheckBox>().CurrentValue && sender.Position.Distance(_Player) <= 400)
                    R.Cast(khazix);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Misc["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
            if (Misc["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }
            if (Misc["Notifications"].Cast<CheckBox>().CurrentValue && R.IsReady())
            {
                var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
                if (target.IsValidTarget(R.Range))
                {
                    if (Player.Instance.GetSpellDamage(target, SpellSlot.R) > target.Health)
                    {
                        Drawing.DrawText(Drawing.Width * 0.1f, Drawing.Height * 0.5f, Color.Red, " Useeeeee RRRRRRRR Cannnnnnnn Killlllllllll: " + target.ChampionName);
                    }
                }
            }
        }

        private static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Tristana")) return;
            Chat.Print("Bristana Loaded!", Color.Orange);
            Bootstrap.Init(null);
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Circular, 450, int.MaxValue, 180);
            E = new Spell.Targeted(SpellSlot.E, 550);
            R = new Spell.Targeted(SpellSlot.R, 550);
            Botrk = new Item(ItemId.Blade_of_the_Ruined_King);
            Bil = new Item(3144, 475f);

            Menu = MainMenu.AddMenu("Bristana", "Bristana");
            Menu.AddGroupLabel("Bristana");
            Menu.AddLabel(" Good Luck! ");

            SpellMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            SpellMenu.AddGroupLabel("Combo Settings");
            SpellMenu.Add("ComboQ", new CheckBox("Combo [Q]"));
            SpellMenu.Add("ComboER", new CheckBox("Combo [ER]"));
            SpellMenu.AddSeparator();
            SpellMenu.Add("RKs", new CheckBox("Combo [R]"));
            SpellMenu.Add("RKb", new KeyBind(" Semi [R] KillSteal", false, KeyBind.BindTypes.HoldActive, 'R'));
            SpellMenu.AddSeparator();
            SpellMenu.AddGroupLabel("Combo [E] On");
            foreach (var target in EntityManager.Heroes.Enemies)
            {
                SpellMenu.Add("useECombo" + target.ChampionName, new CheckBox("" + target.ChampionName));
            }

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Harass [Q]", false));
            HarassMenu.AddGroupLabel("Harass [E] on");
            foreach (var target in EntityManager.Heroes.Enemies)
            {
                HarassMenu.Add("HarassE" + target.ChampionName, new CheckBox("" + target.ChampionName));
            }
            HarassMenu.Add("manaHarass", new Slider("Min Mana For Harass", 50, 0, 100));

            LaneMenu = Menu.AddSubMenu("Laneclear Settings", "Clear");
            LaneMenu.AddGroupLabel("Laneclear Settings");
            LaneMenu.Add("ClearQ", new CheckBox("Laneclear [Q]", false));
            LaneMenu.Add("ClearE", new CheckBox("Laneclear [E]", false));
            LaneMenu.Add("ClearTower", new CheckBox("Laneclear [E] Turret", false));
            LaneMenu.Add("manaFarm", new Slider("Min Mana For LaneClear", 50, 0, 100));

            JungleMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleMenu.AddGroupLabel("JungleClear Settings");
            JungleMenu.Add("jungleQ", new CheckBox("JungleClear [Q]"));
            JungleMenu.Add("jungleE", new CheckBox("JungleClear [E]"));
            JungleMenu.Add("jungleW", new CheckBox("JungleClear [W]", false));
            JungleMenu.Add("manaJung", new Slider("Min Mana For JungleClear", 50, 0, 100));

            Items = Menu.AddSubMenu("Items Settings", "Items");
            Items.AddGroupLabel("Items Settings");
            Items.Add("BOTRK", new CheckBox("Use [Botrk]"));
            Items.Add("ihp", new Slider("My HP Use BOTRK <=", 50));
            Items.Add("ihpp", new Slider("Enemy HP Use BOTRK <=", 50));
            Items.AddGroupLabel("Qss Settings");
            Items.Add("Qss", new CheckBox("Use Qss"));
            Items.AddGroupLabel("Qss On CC");
            Items.Add("stun", new CheckBox("Stuns"));
            Items.Add("rot", new CheckBox("Root"));
            Items.Add("tunt", new CheckBox("Taunt"));
            Items.Add("snare", new CheckBox("Snare"));
            Items.Add("charm", new CheckBox("Charm", false));
            Items.Add("slow", new CheckBox("Slows", false));
            Items.Add("blind", new CheckBox("Blinds", false));
            Items.Add("fear", new CheckBox("Fear", false));
            Items.Add("silence", new CheckBox("Silence", false));
            Items.Add("supperss", new CheckBox("Supperss", false));
            Items.Add("poly", new CheckBox("Polymorph", false));
            Items.Add("delay", new Slider("Humanizer Qss Delay", 0, 0, 1500));

            Misc = Menu.AddSubMenu("Misc Settings", "Draw");
            Misc.AddGroupLabel("Anti Gapcloser");
            Misc.Add("antiGap", new CheckBox("Anti Gapcloser"));
            Misc.Add("antiRengar", new CheckBox("Anti Rengar"));
            Misc.Add("antiKZ", new CheckBox("Anti Kha'Zix"));
            Misc.Add("inter", new CheckBox("Use [R] Interupt"));
            Misc.AddGroupLabel("Drawings Settings");
            Misc.Add("DrawE", new CheckBox("Draw E"));
            Misc.Add("DrawW", new CheckBox("Draw W", false));
            Misc.Add("Notifications", new CheckBox("Notifications Can Kill R"));

            Skin = Menu.AddSubMenu("Skin Changer", "SkinChanger");
            Skin.Add("checkSkin", new CheckBox("Use Skin Changer"));
            Skin.Add("skin.Id", new ComboBox("Skin Mode", 0, "Classic", "Riot Tristana", "Earnest Elf Tristana", "Firefighter Tristana", "Guerilla Tristana", "Rocket Tristana", "Color Tristana", "Color Tristana", "Color Tristana", "Color Tristana", "Dragon Trainer Tristana"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interupt;
            GameObject.OnCreate += GameObject_OnCreate;

        }
    }
}
