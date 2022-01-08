
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Threading.Tasks;
using Sandbox.Internal;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public class NFTPayment
{

	[JsonPropertyName( "symbol" )]
	public string Symbol { get; set; }
	[JsonPropertyName( "decimals" )]
	public int Decimals { get; set; }
	[JsonPropertyName( "usd_price" )]
	public string USD { get; set; }

}

public class NFTLastSale
{

	[JsonPropertyName( "total_price" )]
	public string Price { get; set; }
	[JsonPropertyName( "payment_token" )]
	public NFTPayment Payment { get; set; }

}


public class NFTAsset
{

	[JsonPropertyName( "token_id" )]
	public string Id { get; set; }
	[JsonPropertyName( "image_url" )]
	public string URL { get; set; }
	[JsonPropertyName( "last_sale" )]
	public NFTLastSale LastSale { get; set; }

}

public class NFTCore
{
	[JsonPropertyName( "assets" )]
	public List<NFTAsset> Assets { get; set; }

}


public class NFT
{

	public int Id { get; set; }
	public string URL { get; set; }
	public float Price { get; set; } // I'm fine if the funny NFT simulator game doesn't use 'decimal' for the price
	public Texture Image { get; set; }

	NFT( int id, string url, float price, Texture image = null )
	{

		Id = id;
		URL = url;
		Price = price;
		Image = image;

	}

	public static async void LoadNFT( List<NFT> targetList, string collection, int amount = 1, int offset = 0)
	{

		amount = Math.Min( amount, 50 ); // API allows up to 50

		Log.Info( "Sending HTTP request." );

		var nft = new Http( new Uri( $"https://api.opensea.io/api/v1/assets?order_by=sale_price&order_direction=desc&format=json&offset={offset}&limit={amount}&collection={collection}" ) );
		var json = await nft.GetStreamAsync();

		// We don't need most of what's provided 
		JsonSerializerOptions options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

		Log.Info( "Deserializing JSON..." );

		var jsonResult = await JsonSerializer.DeserializeAsync<NFTCore>( json, options );

		Log.Info( $"Received {jsonResult.Assets.Count} NFTs assets out of {amount} requested." );

		int loaded = 0;

		foreach( var asset in jsonResult.Assets )
		{

			int id = Int32.Parse( asset.Id );
			string url = asset.URL;
			// The 'price' they provide is stored like this: 88800000000000000000, they then provide you with the decimal places and the USD value of the token used
			double longPrice = double.Parse( asset.LastSale.Price );
			// Dividing the number by 10 to the power of the decimal places provided we get 88,8
			double price = longPrice / ( Math.Pow( 10, asset.LastSale.Payment.Decimals ) );
			// Most of the time the token will be ETH, which is around $3,000.00
			float tokenPrice = float.Parse( asset.LastSale.Payment.USD );
			// 88,8 * $3,000 = $266,400
			float realPrice = (float)Math.Round( (float)price * tokenPrice, 2 );

			if ( Host.IsServer )
			{
				// TextureLoader is not available for the server
				targetList.Add( new NFT( id, url, realPrice ) );

			}
			else
			{
				// Realistically we will never use this on the client, but I include it anyways so I can come back to look at the code.
				var http = new Http( new Uri( url ) );
				var stream = await http.GetStreamAsync();

				Texture image = Sandbox.TextureLoader.Image.Load( stream );

				targetList.Add( new NFT( id, url, realPrice, image ) );

			}

			loaded++;

			Log.Info( $"({loaded}/{jsonResult.Assets.Count}) Loaded NFT - ID: {id} - Price: ${realPrice} - URL: {url}" );

		}

	}

}

public partial class MinimalGame : Sandbox.Game
{

	public static List<NFT> LowValueNFT = new List<NFT>();
	public static List<NFT> MediumValueNFT = new List<NFT>();
	public static List<NFT> HighValueNFT = new List<NFT>();

	[ServerCmd("testconnection")]
	public static void TryConnection()
	{

		NFT.LoadNFT( LowValueNFT, "boredapeyachtclub", 30, 4000 + Rand.Int( 2000 ) );
		NFT.LoadNFT( MediumValueNFT, "boredapeyachtclub", 15, 1500 + Rand.Int( 1000 ) );
		NFT.LoadNFT( HighValueNFT, "boredapeyachtclub", 5, Rand.Int( 100 ) );


	}

	[ClientRpc]
	public static async void NFTEvent( string[] URL )
	{

		foreach( string link in URL )
		{

			var http = new Http( new Uri( link ) );
			var stream = await http.GetStreamAsync();

			Texture image = Sandbox.TextureLoader.Image.Load( stream );

			Event.Run( "nft.image", image );

		}

	}

}
