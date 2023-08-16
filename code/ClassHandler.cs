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

		public float Frequency { get; set; }
		public TTT_ClassHeader(string name, Color color, string description, TypeDescription typeDescription, float frequency) 
		{
			Name = name;
			Description = description;
			Color = color;
			TypeDescription = typeDescription;
			Frequency = Math.Clamp( frequency, 0f, 1f );
			
		}

	}

	public abstract class TTT_Class : EntityComponent<TerrorTown.Player>
	{
		
		public abstract string Name { get; set; }
		public abstract Color Color { get; set; }

		public abstract float Frequency { get; set; }
		public abstract string Description { get; set; }

		private RealTimeUntil AbilityCooldown;

		public bool HasActiveAbility = false;
		public float CoolDownTimer;

		//Run on Ability Trigger
		public abstract void ActiveAbility();

		//Run on start
		public abstract void RoundStartAbility();

		protected TTT_Class()
		{
			//Touching this causes errors!
		}

		protected void Add_Item_To_Player(ModelEntity item)
		{
			if(item is Carriable)
			{
				Log.Info("Adding :" + item.Name + ": to :" + Entity.Name);
				Entity.Inventory.AddItem(item);
				((Carriable)item).Droppable = false;
				((Carriable)item).DropOnDeath = false;
			}
			else
			{
				Log.Info( "Spawning :" + item.Name + ": on :" + Entity.Name );
				item.Position = Entity.Position;
			}
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
				if ( spawned is Carriable )
				{
					Entity.Inventory.AddItem( spawned );
					((Carriable)spawned).Droppable = false;
					((Carriable)spawned).DropOnDeath = false;
				}
				else
				{
					spawned.Position = Entity.Position;
				}
			}
		}
		[GameEvent.Client.Frame]
		protected void Look_For_Active_Button_Press()
		{
			if (Game.LocalClient.Pawn == Entity)
			{
				if ( Input.Down("Spray") )
				{
					Log.Info( "Active" );
				}
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
			int AmountOfClasses = Registered_TTT_Classes.Count;
			float totalFrequency = Registered_TTT_Classes.Sum( x => x.Frequency );	
			float selection = Game.Random.Float(totalFrequency);
			float frequencyTrack = 0 ;
			foreach (TTT_ClassHeader header in Registered_TTT_Classes)
			{
				frequencyTrack = frequencyTrack + header.Frequency;

				if( frequencyTrack > selection )
				{
					Log.Info( header.Name);
					Log.Info( "------" );
					return header.Name;
				}
			}
			throw new Exception("How Did you do this?  --- Random select not functioning");
			
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
		



		internal static TTT_ClassHeader Convert_TTT_Class_2_Header(TypeDescription typeDescription)
		{
			TTT_Class temp = typeDescription.Create<TTT_Class>();
			TTT_ClassHeader header = new TTT_ClassHeader(temp.Name, temp.Color, temp.Description, typeDescription,temp.Frequency);
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

		[ConCmd.Client( "class_testing" )]
		public static void test()
		{
			//foreach ( var panel in Game.RootPanel.Children )
			//{
			//	Log.Info( panel );
			//}
			var health = Game.RootPanel.ChildrenOfType<TerrorTown.Health>().FirstOrDefault();
			Log.Info( health );
			health.SetProperty( "style", "height:15vh" );
			foreach(var child in health.Children)
			{
				Log.Info( child );
				Log.Info( "next" );
			}
			var green_bar = health.Children.Where( x => x.HasClass( "bar" ) ).FirstOrDefault();
			green_bar.SetProperty( "style", "");
			Log.Info( health.Children.Where( x => x.HasClass( "bar" ) ).FirstOrDefault() );
			var panel = health.AddChild<ShowClass>();
			panel.Init( Game.Random.Int( 0, 10 ) );
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
		public static void Initialise_TTT_Class( MyGame _game )
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


	

