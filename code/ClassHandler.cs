using Sandbox.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using TerrorTown;
using System.Security.Principal;
using System.Globalization;

namespace TCT_Classes
{
	public abstract class TTT_Class : BaseNetworkable
	{
		public abstract string Name { get; set; }
		public abstract Color Color { get; set; }

		public abstract string Description { get; set; }
		public abstract IList<ModelEntity> ClassItems { get; set; }

		//Run on Ability trigger
		public abstract void ActiveAbility();

		//Run on start
		public abstract void RoundStartAbility();

		protected TTT_Class()
		{
			ClassHandler.RegisterClass( this );
		}


	}

	// To Do:
	//
	//		- UI
	//		- Copy sanders code were aplicable  (https://github.com/SmartMario1/Sbox-TTT-Minigame-Manager/blob/main/code/MinigameManager.cs)
	//		- Cleanup	
	//		- Player side
	//
	//
	//
	internal partial class ClassHandler
    {
		public static IList<TTT_Class> Registered_TTT_Classes { get; private set; } = new List<TTT_Class>();

		public static IList<TTT_Class> Enabled_TTT_Classes { get; private set; } = new List<TTT_Class>();


		private static string Select_Random_Class_Name()
		{
			TTT_Class selected = null;
			int AmountOfClasses = Enabled_TTT_Classes.Count;
			int selection = Game.Random.Int( AmountOfClasses - 1 );
			Log.Info( selection );
			Log.Info( "------");
			Log.Info( AmountOfClasses );
			selected = Enabled_TTT_Classes[selection];
			return selected.Name;
		}


		private static bool ValidateUser( TerrorTown.Player ply )
		{

			// Copied from SmartMario's MiniGame_Manager
			Game.AssertServer();
			if ( ply.UserData.PermissionLevel == PermissionLevel.Moderator ||
				ply.UserData.PermissionLevel == PermissionLevel.Admin ||
				ply.UserData.PermissionLevel == PermissionLevel.SuperAdmin ||
				ply.Client.SteamId == Game.SteamId )
			{
				return true;
			}
			return false;





		}

		public static void RegisterClass( TTT_Class game )
		{
			Log.Info( "Attempting to register : " + game.Name );
			Registered_TTT_Classes.Add( game );
			// This part of the code loads the previous config if it exists.
			if ( Game.IsServer )
			{
				if ( FileSystem.Data.FileExists( "TTT_Classes_config.json" ) )
				{
					var config = FileSystem.Data.ReadJson<Dictionary<string, bool>>( "TTT_Classes_config.json" );
					if ( !config.ContainsKey( game.Name ) )
					{
						Enabled_TTT_Classes.Add( game );
						return;
					}
					if ( config[game.Name] )
					{
						Enabled_TTT_Classes.Add( game );
						return;
					}
				}
				else
				{
					Enabled_TTT_Classes.Add( game );
				}
			}
		}


		public static TTT_Class FindClass(String name )
		{
			foreach(TTT_Class C in Registered_TTT_Classes)
			{
				if ( C.Name == name )
				{
					return C;
				}
			}
			return null;
		}

		[ClientRpc]
		public static void AssignClass( string ClassToAssign, TerrorTown.Player Ply ) 
		{ 
			if(Game.LocalClient.Pawn == Ply )
			{
				Log.Info( "-------" );
				Log.Info( "Correctly sent:" + ClassToAssign);
				TTT_Class ClassToApply = FindClass(ClassToAssign);
				Log.Info( "translated:" + ClassToApply.Name );
				Log.Info( "-------" );
			}

			else
			{
				Log.Info( "Not For me :" + Ply.Name + ":" + ((TerrorTown.Player)Game.LocalClient.Pawn).Name );
			}
		
		}



		[Event( "Game.Round.Start" )]
		public static void OnRoundStart()
		{
			if ( Game.IsServer )
			{



				IList<TerrorTown.Player> Players = new List<TerrorTown.Player>();
				foreach ( IClient client in Game.Clients )
				{
					Players.Add( (TerrorTown.Player)client.Pawn );
				}


				foreach ( TerrorTown.Player ply in Players )
				{


					AssignClass(Select_Random_Class_Name(), ply);
				};
			}

		}
		// Instantiation code (mostly) copied from Teams class in TTT by Three Thieves
		[Event( "Game.Initialized" )]
		public static void InitialiseMinigames( MyGame _game )
		{
			List<TypeDescription> list = (from type in GlobalGameNamespace.TypeLibrary.GetTypes<TTT_Class>()
										  where type.TargetType.IsSubclassOf( typeof( TTT_Class ) )
										  select type).ToList();

			Log.Info( list );
			if ( list.Count() == 0 )
			{
				Log.Error( "No classes were found." );
				throw new Exception( "No classes were found." );
			}
			foreach ( TypeDescription TTTC in list )
			{
				if ( !Registered_TTT_Classes.Where( ( TTT_Class x ) => x.GetType() == TTTC.TargetType ).Any() )
				{
					TTTC.Create<TTT_Class>();
				}
			}

		}

	}
}


	

