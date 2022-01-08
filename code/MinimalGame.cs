
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Threading.Tasks;
using Sandbox.Internal;
using System.Collections.Generic;
using System.Text.Json;

public partial class MinimalGame : Sandbox.Game
{
	public MinimalGame()
	{
		
		if ( IsServer )
		{

			new MinimalHudEntity();

		}

	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		var player = new MinimalPlayer();
		client.Pawn = player;

		player.Respawn();
	}

}
