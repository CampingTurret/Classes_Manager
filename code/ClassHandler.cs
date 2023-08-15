using Sandbox.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using TerrorTown;

namespace TCT_Classes
{


	public class TTT_ClassHeader: BaseNetworkable
	{
		public string Name;

		public string Description { get; set; }
		public Color Color { get; set; }
		public TypeDescription TypeDescription { get; set; }	

		public TTT_ClassHeader(string name, Color color, string description, TypeDescription typeDescription) 
		{
			Name = name;
			Description = description;
			Color = color;
			TypeDescription = typeDescription;
			
		}

	}

	public abstract class TTT_Class : EntityComponent<TerrorTown.Player>
	{

		
		public abstract string Name { get; set; }
		public abstract Color Color { get; set; }

		public abstract string Description { get; set; }

		//Run on Ability trigger
		public abstract void ActiveAbility();

		//Run on start
		public abstract void RoundStartAbility();

		protected TTT_Class()
		{

		}

		protected void Add_Item_To_Player(ModelEntity item)
		{
			item.Position = Entity.Position;
		}
		protected void Add_Item_To_Player( TypeDescription item )
		{
			ModelEntity spawned = item.Create<ModelEntity>();
			if(spawned == null )
			{
				throw new Exception( "Type does not derive from a ModelEntity" );
			}
			else
			{
				spawned.Position = Entity.Position;
			}
		}

	}

	// To Do:
	//
	//		- UI
	//		- Enabled classes
	//		- Cleanup	
	//		- active ability
	//
	//
	//
	internal partial class ClassHandler
    {
		public static IList<TTT_ClassHeader> Registered_TTT_Classes { get; private set; } = new List<TTT_ClassHeader>();

		public static IList<TTT_Class> Enabled_TTT_Classes { get; private set; } = new List<TTT_Class>();


		private static string Select_Random_Class_Name()
		{
			TTT_ClassHeader selected = null;
			int AmountOfClasses = Registered_TTT_Classes.Count;
			int selection = Game.Random.Int( AmountOfClasses - 1 );
			Log.Info( selection );
			Log.Info( "------");
			Log.Info( AmountOfClasses );
			selected = Registered_TTT_Classes[selection];
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

	//	public static void RegisterClass( TTT_Class game )
	//	{
	//		Log.Info( "Attempting to register : " + game.Name );
	//		Registered_TTT_Classes.Add( game );
	//		// This part of the code loads the previous config if it exists.
	//		if ( Game.IsServer )
	//		{
	//			if ( FileSystem.Data.FileExists( "TTT_Classes_config.json" ) )
	//			{
	//				var config = FileSystem.Data.ReadJson<Dictionary<string, bool>>( "TTT_Classes_config.json" );
	//				if ( !config.ContainsKey( game.Name ) )
	//				{
	//					Enabled_TTT_Classes.Add( game );
	//					return;
	//				}
	//				if ( config[game.Name] )
	//				{
	//					Enabled_TTT_Classes.Add( game );
	//					return;
	//				}
	//			}
	//			else
	//			{
	//				Enabled_TTT_Classes.Add( game );
	//			}
	//		}
	//	}
		



		internal static TTT_ClassHeader Convert_TTT_Class_2_Header(TypeDescription typeDescription)
		{
			TTT_Class temp = typeDescription.Create<TTT_Class>();
			TTT_ClassHeader header = new TTT_ClassHeader(temp.Name, temp.Color, temp.Description, typeDescription);
			return header;
		}


		public static TTT_ClassHeader FindClass(String name )
		{
			foreach( TTT_ClassHeader C in Registered_TTT_Classes)
			{
				if ( C.Name == name )
				{
					return C;
				}
			}
			return null;
		}

		public static void AssignClass( string classToAssign, TerrorTown.Player ply ) 
		{
			Game.AssertServer();
			
			Log.Info( "-------" );
			Log.Info( "Correctly sent:" + classToAssign );
			TTT_ClassHeader classToApply = FindClass( classToAssign );
			Log.Info( "translated:" + classToApply.Name );
			ply.Components.Add( classToApply.TypeDescription.Create<TTT_Class>() );
			Log.Info( ply );
			Log.Info( ply.Components.Get<TTT_Class>() );
			Log.Info( "-------" );
			TTT_Class classOnPlayer = ply.Components.Get<TTT_Class>();
			classOnPlayer.RoundStartAbility();



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

			foreach ( TypeDescription type in list )
			{
				if ( !Registered_TTT_Classes.Where( ( TTT_ClassHeader x ) => x.TypeDescription.TargetType == type.TargetType ).Any() )
				{
					Registered_TTT_Classes.Add(Convert_TTT_Class_2_Header(type));
				}

			}
		}

	}
}


	

