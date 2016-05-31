using System;
using System.Collections.Generic;
using System.Reflection;
using TShockAPI;
using Terraria;
using System.Timers;
using TerrariaApi.Server;

namespace Freeze
{
	[ApiVersion(1, 23)]
	public class Freeze : TerrariaPlugin
	{
		public static List<FrozenPlayer> FrozenPlayers = new List<FrozenPlayer>();
		public static Dictionary<string, string> IpNames = new Dictionary<string, string>();
		Timer timer = new Timer(1000) { Enabled = true };

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
			ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
			ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
			Commands.ChatCommands.Add(new Command("freeze.use", freeze, "freeze"));
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
			}
			base.Dispose(disposing);
		}

		public Freeze(Main game) : base(game)
		{
			Order = 1;
		}

		public static bool IsFrozen(TSPlayer ts, out string name)
		{
			name = null;
			if (ts != null)
			{
				if (IpNames.ContainsKey(ts.IP))
				{
					name = IpNames[ts.IP];
					return true;
				}
			}
			return false;
		}

		public void OnLeave(LeaveEventArgs e)
		{
			TSPlayer ts = TShock.Players[e.Who];
			int i = FrozenPlayers.FindIndex(f => f.Index == e.Who);
			if (i >= 0)
			{
				if (!IpNames.ContainsKey(ts.IP))
					IpNames.Add(ts.IP, ts.Name);

				FrozenPlayers.RemoveAt(i);
			}
		}

		public void OnJoin(JoinEventArgs e)
		{
			string name;
			TSPlayer ts = TShock.Players[e.Who];

			if (IsFrozen(ts, out name))
			{
				FrozenPlayers.Add(new FrozenPlayer(ts.Index, true));
				IpNames.Remove(ts.IP);

				TShock.Log.ConsoleInfo(string.Format("{0} a.k.a. ({1}) has been re-frozen", ts.Name, name));

				if (name != ts.Name)
				{
					foreach (TSPlayer tsplr in TShock.Players)
					{
						if (tsplr == null)
							continue;

						if (tsplr.Group.HasPermission("freeze.use"))
							tsplr.SendSuccessMessage(string.Format("{0} a.k.a. ({1}) has been re-frozen", ts.Name, name));
					}
				}
			}
		}

		void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			foreach (FrozenPlayer fp in FrozenPlayers)
			{
				if (fp == null)
					continue;

				fp.TSPlayer.SetBuff(47, 180, true); //frozen debuff (Can't move)
				fp.TSPlayer.SetBuff(156, 180, true); //stoned debuff (Can't move)
				fp.TSPlayer.SetBuff(149, 180, true); //webbed debuff (Can't move)
				fp.TSPlayer.SetBuff(163, 180, true); //obstructed debuff (Complete darkness)

				if (fp.HasMoved())
					fp.TSPlayer.Teleport(fp.LastTileX * 16, fp.LastTileY * 16);

				else
				{
					fp.LastTileX = fp.TSPlayer.TileX;
					fp.LastTileY = fp.TSPlayer.TileY;
				}
			}
		}

		public class FrozenPlayer
		{
			public int Index { get; set; }
			public TSPlayer TSPlayer { get { return TShock.Players[Index]; } }
			public float LastTileX { get; set; }
			public float LastTileY { get; set; }
			public bool HasMoved()
			{
				return (Math.Abs(LastTileX - TSPlayer.TileX) + Math.Abs(LastTileY - TSPlayer.TileY)) > 1;
			}

			public FrozenPlayer(int index, bool spawn = false)
			{
				Index = index;
				if (spawn)
				{
					LastTileX = Main.spawnTileX;
					LastTileY = Main.spawnTileY - 3;
				}
				else
				{
					LastTileX = TSPlayer.TileX;
					LastTileY = TSPlayer.TileY;
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
				if (FrozenPlayers.FindIndex(p => p.Index == plr.Index) < 0)
				{
					FrozenPlayers.Add(new FrozenPlayer(plr.Index));
					TSPlayer.All.SendInfoMessage(string.Format("{0} froze {1}", args.Player.Name, plr.Name));
					return;
				}
				else
				{
					FrozenPlayers.RemoveAll(p => p.Index == plr.Index);
					TSPlayer.All.SendInfoMessage(string.Format("{0} unfroze {1}", args.Player.Name, plr.Name));
					return;
				}
			}
		}
	}
}