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
			foreach(var panel in Game.RootPanel.Children)
			{
				Log.Info( panel );
			}
		}

		[ConCmd.Client( "class_add_class_ui" )]
		public static void AddClassUI(string classname)
		{
			// This line removes the previous class if it exists
			Game.RootPanel.ChildrenOfType<TerrorTown.Health>().FirstOrDefault().Children.Where( x => x.HasClass( "seg" ) ).FirstOrDefault().ChildrenOfType<UI.ShowClass>().FirstOrDefault()?.Delete();

			// Finding the assigned class
			TTT_ClassHeader class_header = FindClass( classname );
			if ( class_header == null ) { Log.Error( "Could not find given class for UI update." ); return; }

			// These lines alter the existing UI to accomedate our new element.
			var health = Game.RootPanel.ChildrenOfType<TerrorTown.Health>().FirstOrDefault();
			var top_bar = health.Children.Where( x => x.HasClass( "seg" ) ).FirstOrDefault();
			top_bar.SetProperty( "style", "flex-direction: column-reverse; height: 96px;" );

			// This adds our new element.
			var panel = top_bar.AddChild<UI.ShowClass>();
			panel.Init( class_header.Name, class_header.Color );
		}

		[ClientRpc]
		public static void AnnounceClasses()
		{
			TTT_Class classOnPlayer = Game.LocalClient.Pawn.Components.Get<TTT_Class>();
			if ( classOnPlayer == null )
			{
				Log.Error( "We don't have an assigned class!" );
				return;
			}
			var announcement = Game.RootPanel.AddChild<UI.ClassAnnouncement>();
			announcement.SetClass( classOnPlayer );
		}

		[TerrorTown.ChatCmd( "class_desc", PermissionLevel.User )]
		public static void RequestDescription()
		{
			var attached_class = ConsoleSystem.Caller.Pawn.Components.Get<TTT_Class>();
			if ( attached_class == null )
			{
				Chat.AddChatEntry( To.Single( (Entity)ConsoleSystem.Caller.Pawn ), null, "You don't have an assigned class!" );
			}
			else 
			{
				Chat.AddChatEntry( To.Single( (Entity)ConsoleSystem.Caller.Pawn ), null, attached_class.Description );
			}
		}

		[Event( "Game.Round.Start" )]
		public static void OnRoundStart()
		{
			if ( Game.IsServer )
			{
				foreach ( IClient client in Game.Clients )
				{
					TerrorTown.Player ply = client.Pawn as TerrorTown.Player;
					string assigned_class = Select_Random_Class_Name();

					AssignClass(assigned_class, ply);
					client.SendCommandToClient( "class_add_class_ui \"" + assigned_class + "\"" );
				};
				AnnounceClasses();
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


	

