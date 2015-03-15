using System;
using System.Collections.Generic;
using System.Reflection;
using TShockAPI;
using Terraria;
using System.Timers;
using TerrariaApi.Server;

namespace Freeze
{
    [ApiVersion(1, 17)]
    public class Freeze : TerrariaPlugin
    {
        Timer timer = new Timer(1000) { Enabled = true };
        List<string> frozenplayer = new List<string>();

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
            get { return "Freeze"; }
        }

        public override string Description
        {
            get { return "Allows you to freeze players on the spot"; }
        }

        public override void Initialize()
        {
            timer.Elapsed += timer_Elapsed;
            Commands.ChatCommands.Add(new Command("freeze.use", freeze, "freeze"));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }

        public Freeze(Main game)
            : base(game)
        {
            Order = 1;
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (TSPlayer ts in TShock.Players)
            {
                if (ts == null)
                    continue;

                if (frozenplayer.Contains(ts.IP))
                    frozenplayer.Disable("Manually frozen with command", false)
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

