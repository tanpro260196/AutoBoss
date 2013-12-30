﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TerrariaApi;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using System.Reflection;

namespace Auto_Boss
{
    [ApiVersion(1, 14)]
    public class AutoBoss : TerrariaPlugin
    {
        #region TerrariaPlugin
        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public override string Author
        {
            get { return "WhiteX"; }
        }

        public override string Description
        {
            get { return "Automatic boss spawner"; }
        }

        public override string Name
        {
            get { return "AutoBoss+"; }
        }

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.GamePostInitialize.Register(this, Boss_Tools.PostInitialize);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreet);
                ServerApi.Hooks.GamePostInitialize.Deregister(this, Boss_Tools.PostInitialize);
            }
            base.Dispose(disposing);
        }

        public AutoBoss(Main game)
            : base(game)
        {
            Order = -10;
            Boss_Tools.boss_Config = new Boss_Config();
        }
        #endregion

        #region OnInitialize
        public void OnInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("boss.root", Boss_Commands.Boss_Command, "boss")
                {
                    HelpText = "Toggles automatic boss spawns; Reloads the configuration; Lists bosses and minions spawned by the plugin"
                });
            Boss_Tools.SetupConfig();
        }
        #endregion

        #region NetGreetPlayer
        public void OnGreet(GreetPlayerEventArgs args)
        {
            if (Boss_Tools.boss_Config.AutoStart_Enabled)
                if (TShock.Players[0].Index == args.Who)
                    if (!Boss_Timer.boss_Timer.Enabled)
                        Boss_Timer.Run();
        }
        #endregion
    }
}