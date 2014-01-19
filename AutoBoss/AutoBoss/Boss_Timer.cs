﻿using System;
using System.Linq;
using System.Text;
using System.Timers;
using System.Collections.Generic;

using Terraria;
using TShockAPI;

namespace Auto_Boss
{
    public class timerObj
    {

        public int count;
        public int maxCount;
        public string type;
        public bool enabled;

        public timerObj(int c, string t, bool e, int m)
        {
            count = c;
            type = t;
            enabled = e;
            maxCount = m;
        }
    }

    public class Boss_Timer
    {
        public static Timer boss_Timer = new Timer(Boss_Tools.boss_Config.Message_Interval * 1000);

        public static bool dayBossEnabled = false;
        public static bool nightBossEnabled = false;
        public static bool specialBossEnabled = false;

        public static bool dayMinionEnabled = false;
        public static bool nightMinionEnabled = false;
        public static bool specialMinionEnabled = false;

        public static bool MinionTimerRunning = false;

        internal static timerObj ticker = new timerObj(-1, "day", false, 10);
        
        public static void boss_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool bossActive = false;
            foreach (KeyValuePair<int, int> pair in Boss_Tools.boss_List)
                if (Main.npc[pair.Key].type == pair.Value && Main.npc[pair.Key].active)
                    bossActive = true;

            if (!Boss_Tools.Bosses_Toggled)
            {
                Log.ConsoleInfo("[AutoBoss+] Timer Disabled: Boss toggle disabled");
                boss_Timer.Enabled = false;
                Boss_Tools.Bosses_Toggled = false;
                ticker.count = -1;
                return;
            }

            if (TShock.Players[0] == null && TShock.Players.Length == 0)
            {
                Log.ConsoleInfo("[AutoBoss+] Timer Disabled: No players online");
                boss_Timer.Enabled = false;
                Boss_Tools.Bosses_Toggled = false;
                ticker.count = -1;
                return;
            }

            if (Main.dayTime && dayBossEnabled && !ticker.enabled)
            {
                ticker.type = "day";
                ticker.maxCount = Boss_Tools.boss_Config.DayTimer_Text.Length - 1;
                ticker.enabled = true;
            }
            if (!Main.dayTime && !Main.raining && !Main.bloodMoon && !Main.eclipse && !Main.pumpkinMoon &&
                !Main.snowMoon && Main.invasionType == 0 && nightBossEnabled && !ticker.enabled)
            {
                ticker.type = "night";
                ticker.maxCount = Boss_Tools.boss_Config.NightTimer_Text.Length - 1;
                ticker.enabled = true;
            }

            if (Main.raining || Main.bloodMoon || Main.eclipse || Main.pumpkinMoon ||
                            Main.snowMoon || Main.invasionType > 0 && specialBossEnabled && !ticker.enabled)
            {
                ticker.type = "special";
                ticker.maxCount = Boss_Tools.boss_Config.SpecialTimer_Text.Length - 1;
                ticker.enabled = true;
            }

            if (ticker.enabled)
            {
                if (!bossActive)
                {
                    ticker.count++;

                    if (ticker.type == "day")
                    {
                        if (Boss_Tools.boss_Config.Enable_DayTimer_Text)
                        {
                            if (ticker.count != ticker.maxCount)
                                TSPlayer.All.SendMessage(Boss_Tools.boss_Config.DayTimer_Text[ticker.count],
                                    Color.YellowGreen);

                            else if (ticker.count >= ticker.maxCount)
                            {
                                TSPlayer.All.SendMessage(Boss_Tools.boss_Config.DayTimer_Text[ticker.maxCount],
                                    Color.Crimson);

                                //if (!MinionTimerRunning)
                                //    startMinionTimer();

                                Boss_Events.start_BossBattle_Day();

                                if (Boss_Tools.boss_Config.Continuous_Boss)
                                    ticker.count = -1;
                                else
                                {
                                    TSPlayer.All.SendMessage(Boss_Tools.boss_Config.DayTimer_Finished,
                                        Color.LightBlue);
                                    boss_Timer.Enabled = false;
                                }
                            }
                        }
                    }
                    if (ticker.type == "night")
                    {
                        if (Boss_Tools.boss_Config.Enable_NightTimer_Text)
                        {
                            if (ticker.count != ticker.maxCount)
                                TSPlayer.All.SendMessage(Boss_Tools.boss_Config.NightTimer_Text[ticker.count],
                                    Color.DarkMagenta);

                            else if (ticker.count >= ticker.maxCount)
                            {
                                TSPlayer.All.SendMessage(Boss_Tools.boss_Config.NightTimer_Text[ticker.maxCount],
                                   Color.Crimson);

                                //if (!MinionTimerRunning)
                                //    startMinionTimer();

                                Boss_Events.start_BossBattle_Night();
                                if (Boss_Tools.boss_Config.Continuous_Boss)
                                    ticker.count = -1;
                                else
                                {
                                    TSPlayer.All.SendMessage(Boss_Tools.boss_Config.NightTimer_Finished,
                                        Color.LightBlue);
                                    boss_Timer.Enabled = false;
                                }
                            }
                        }
                    }
                    if (ticker.type == "special")
                    {
                        if (Boss_Tools.boss_Config.Enable_SpecialTimer_Text)
                        {
                            if (ticker.count != ticker.maxCount)
                                TSPlayer.All.SendMessage(Boss_Tools.boss_Config.SpecialTimer_Text[ticker.count],
                                   Color.Orange);

                            else if (ticker.count >= ticker.maxCount)
                            {
                                TSPlayer.All.SendMessage(Boss_Tools.boss_Config.SpecialTimer_Text[ticker.maxCount],
                                    Color.Crimson);

                                //if (!MinionTimerRunning)
                                //    startMinionTimer();

                                Boss_Events.start_BossBattle_Special();
                                if (Boss_Tools.boss_Config.Continuous_Boss)
                                    ticker.count = -1;
                                else
                                {
                                    boss_Timer.Enabled = false;
                                    TSPlayer.All.SendMessage(Boss_Tools.boss_Config.SpecialTimer_Finished,
                                        Color.LightBlue);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void startMinionTimer()
        {
            MinionTimerRunning = true;

            Random rndTime = new Random();
            Timer minionTimer = new Timer(rndTime.Next
                (Boss_Tools.boss_Config.Minions_Spawn_Timer[0] / Boss_Tools.boss_Config.Minions_Spawn_Timer[1])
                * 1000);
            minionTimer.Enabled = true;
            minionTimer.Elapsed += Minion_Elapsed_Event;
        }

        public static void Minion_Elapsed_Event(object sender, ElapsedEventArgs args)
        {
            if (Boss_Events.Bosses_Active && Main.dayTime && dayMinionEnabled)
                Boss_Events.start_DayMinion_Spawns();

            if (Boss_Events.Bosses_Active && !Main.dayTime && !Main.raining && !Main.bloodMoon && !Main.eclipse
                && !Main.pumpkinMoon && !Main.snowMoon && Main.invasionType == 0 && nightMinionEnabled)
                Boss_Events.start_NightMinion_Spawns();

            if (Boss_Events.Bosses_Active && Main.raining || Main.bloodMoon || Main.eclipse || Main.pumpkinMoon ||
                                    Main.snowMoon || Main.invasionSize > 0 && specialMinionEnabled)
                Boss_Events.start_SpecialMinion_Spawns();
        }
    }
}