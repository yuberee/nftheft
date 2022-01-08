using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System;
using Sandbox.Internal;
using Sandbox.InternalTests;
using Sandbox.Utility;
using System.Threading;
using System.Threading.Tasks;


public class NFTContainer : Panel
{
	public NFTContainer( Texture image )
	{

		var panel = Add.Panel( "NFT" );

		panel.Style.SetBackgroundImage( image );

	}

	public override void Tick()
	{

	}

}


public partial class MinimalHudEntity : Sandbox.HudEntity<RootPanel>
{
	public MinimalHudEntity()
	{
		if ( IsClient )
		{

			RootPanel.StyleSheet.Load( "MinimalHud.scss" );

		}

	}

	[Event( "nft.image" )]
	public async void MakeImage( Texture image )
	{

		Panel panel = new NFTContainer( image );
		RootPanel.AddChild( panel );

	}

}
