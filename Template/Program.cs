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

namespace Champion
{
    internal class Program
    {
        //public static Spell.Active Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Item Botrk;
        public static Item Bil;
        //public static Font Thm;
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
            if (!_Player.ChampionName.Contains("//////////")) return;
            Chat.Print("Doctor's /// Loaded!", Color.Orange);
            Bootstrap.Init(null);
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
            ComboMenu.Add("CQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("CW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("CE", new CheckBox("Use [E] Combo"));
            ComboMenu.Add("CR", new CheckBox("Use [R] Combo"));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HW", new CheckBox("Use [W] Harass"));
            HarassMenu.Add("HE", new CheckBox("Use [E] Harass"));
            HarassMenu.Add("HM", new Slider("Mana Harass", 50, 0, 100));

            LaneClearMenu = Menu.AddSubMenu("Laneclear Settings", "Clear");
            LaneClearMenu.AddGroupLabel("Laneclear Settings");
            LaneClearMenu.Add("LQ", new CheckBox("Use [Q] Laneclear", false));
            LaneClearMenu.Add("LW", new CheckBox("Use [W] Laneclear", false));
            LaneClearMenu.Add("LE", new CheckBox("Use [E] Laneclear", false));
            LaneClearMenu.Add("LM", new Slider("Mana LaneClear", 60, 0, 100));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("JQ", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("JW", new CheckBox("Use [W] JungleClear"));
            JungleClearMenu.Add("JE", new CheckBox("Use [E] JungleClear"));
            JungleClearMenu.Add("JM", new Slider("Mana JungleClear", 20, 0, 100));

            Items = Menu.AddSubMenu("Items Settings", "Items");
            Items.AddGroupLabel("Items Settings");
            Items.Add("BOTRK", new CheckBox("Use [Botrk]"));
            Items.Add("ihp", new Slider("My HP Use BOTRK <=", 50));
            Items.Add("ihpp", new Slider("Enemy HP Use BOTRK <=", 50));

            Misc = Menu.AddSubMenu("Misc Settings", "Draw");
            Misc.AddGroupLabel("Anti Gapcloser");
            Misc.Add("antiGap", new CheckBox("Anti Gapcloser", false));
            Misc.Add("inter", new CheckBox("Use [R] Interupt"));
            Misc.AddGroupLabel("Drawings Settings");
            Misc.Add("Draw_Disabled", new CheckBox("Disabled Drawings", false));
            Misc.Add("DrawE", new CheckBox("Draw [E]"));
            Misc.Add("DrawW", new CheckBox("Draw [W]", false));

            Skin = Menu.AddSubMenu("Skin Changer", "SkinChanger");
            Skin.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Skin.Add("skin.Id", new ComboBox("Skin Mode", 6, "1", "2", "3", "4", "5", "6", "7", "8"));

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            //Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            //Interrupter.OnInterruptableSpell += Interupt;
            //Orbwalker.OnPostAttack += ResetAttack;

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
            Item();

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
			
            if (Misc["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2, Radius = E.Range }.Draw(_Player.Position);
            }
			
            if (Misc["DrawW"].Cast<CheckBox>().CurrentValue && W.IsReady())
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2, Radius = W.Range }.Draw(_Player.Position);
            }
			
            //if (Misc["Notifications"].Cast<CheckBox>().CurrentValue && R.IsReady())
            {
                var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (target.IsValidTarget(W.Range) && Player.Instance.GetSpellDamage(target, SpellSlot.R) > target.Health + target.AttackShield)
                {
                    DrawFont(Thm, "[R] Can Killable " + target.ChampionName, (float)(ft[0] - 140), (float)(ft[1] + 80), SharpDX.Color.Red);
                }
            }
        }

        //public static bool QReady
        {
            //get { return Player.Instance.GetBuffCount("AsheQCastReady") == 4; }
        }

// Flee Mode

        private static void Flee()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
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

        //public static void Interupt(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs i)
        {
            var Inter = Misc["inter"].Cast<CheckBox>().CurrentValue;
            if (!sender.IsEnemy || !(sender is AIHeroClient) || Player.Instance.IsRecalling())
            {
                return;
            }
			
            if (Inter && R.IsReady() && i.DangerLevel == DangerLevel.High && _Player.Distance(sender) <= 1200)
            {
                R.Cast(sender);
            }
        }

//Harass Mode

        private static void Harass()
        {
            var useQ = HarassMenu["HQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HW"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HE"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["HM"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(W.Range) && !e.IsDead && !e.IsZombie))
            {

            }
        }

//Combo Mode

        private static void Combo()
        {
            var useQ = ComboMenu["CQ"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["CW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["CE"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["CR"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(2000) && !e.IsDead && !e.IsZombie))
            {

            }
        }

//ResetAttack

        //public static void ResetAttack(AttackableUnit e, EventArgs args)
        {
            if (!(e is AIHeroClient)) return;
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            var champ = (AIHeroClient)e;
            if (champ == null || champ.Type != GameObjectType.AIHeroClient || !champ.IsValid) return;
            if (target != null)
            {

            }
        }

//LaneClear Mode

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["LQ"].Cast<CheckBox>().CurrentValue;
            var useW = LaneClearMenu["LW"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["LE"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["LM"].Cast<Slider>().CurrentValue;
            var minionQ = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(e => e.IsValidTarget(W.Range));
            var quang = EntityManager.MinionsAndMonsters.GetLineFarmLocation(minionQ, W.Width, (int) W.Range);
            foreach (var minion in minionQ)
            {

            }
        }

// JungleClear Mode

        private static void JungleClear()
        {
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(W.Range));
            var useQ = JungleClearMenu["JQ"].Cast<CheckBox>().CurrentValue;
            var useW = JungleClearMenu["JW"].Cast<CheckBox>().CurrentValue;
            var useE = JungleClearMenu["JE"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["JM"].Cast<Slider>().CurrentValue;
            if (monster != null)
            {

            }
        }

        public static void DrawFont(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }

// KillSteal

        private static void KillSteal()
        {
            var target = EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(2000) && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && !e.IsDead && !e.IsZombie);
            var useR = ComboMenu["RKs"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["WKs"].Cast<CheckBox>().CurrentValue;
            var RKey = ComboMenu["RKb"].Cast<KeyBind>().CurrentValue;
            foreach (var target2 in target)
            {

            }
        }

// AntiGap

        private static void Gapcloser_OnGapCloser(Obj_AI_Base sender, Gapcloser.GapcloserEventArgs args)
        {
            if (Misc["antiGap"].Cast<CheckBox>().CurrentValue && sender.IsEnemy && args.Sender.Distance(_Player) <= 325)
            {
                R.Cast(args.Sender);
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
