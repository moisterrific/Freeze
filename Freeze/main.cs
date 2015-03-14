using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Data;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;

namespace AIO
{
    [ApiVersion(1, 17)]
    public class AIO : TerrariaPlugin
    {
        List<string> frozenplayer = new List<string>();
        public List<string> staffchatplayers = new List<string>();

        Color staffchatcolor = new Color(200, 50, 150);
        DateTime LastCheck = DateTime.UtcNow;

        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }
        public override string Author
        {
            get { return "Ancientgods"; }
        }
        public override string Name
        {
            get { return "AIO"; }
        }

        public override string Description
        {
            get { return "all-in-one plugin, now compatible with infinite chests!"; }
        }

        public override void Initialize()
        {
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
            Commands.ChatCommands.Add(new Command("aio.freeze", freeze, "freeze"));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);
            }
            base.Dispose(disposing);
        }

        public AIO(Main game)
            : base(game)
        {
            Order = 1;
        }


        public void OnUpdate(EventArgs e)
        {
            WorldGen.spawnMeteor = false;
            if ((DateTime.UtcNow - LastCheck).TotalSeconds >= 3)
            {
                LastCheck = DateTime.UtcNow;
                foreach (TSPlayer ts in TShock.Players)
                {
                    if (ts != null)
                    {
                        if (frozenplayer.Contains(ts.IP))
                        {
                            ts.SetBuff(47, 240, true);
                            ts.SetBuff(80, 240, true);
                            ts.SetBuff(23, 240, true);
                        }
                    }
                }
            }
        }



        public void freeze(CommandArgs args)
        {
            if (args.Player != null)
            {
                if (args.Parameters.Count != 1)
                {
                    args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /freeze [player]");
                    return;
                }
                var foundplr = TShock.Utils.FindPlayer(args.Parameters[0]);
                if (foundplr.Count == 0)
                {
                    args.Player.SendErrorMessage("Invalid player!");
                    return;
                }
                else if (foundplr.Count > 1)
                {
                    args.Player.SendErrorMessage(string.Format("More than one ({0}) player matched!", args.Parameters.Count));
                    return;
                }
                var plr = foundplr[0];
                if (!frozenplayer.Contains(plr.IP))
                {
                    frozenplayer.Add(plr.IP);
                    TSPlayer.All.SendInfoMessage(string.Format("{0} froze {1}", args.Player.Name, plr.Name));
                    return;
                }
                else
                {
                    frozenplayer.Remove(plr.IP);
                    TSPlayer.All.SendInfoMessage(string.Format("{0} unfroze {1}", args.Player.Name, plr.Name));
                    return;
                }
            }
        }
    }
}

