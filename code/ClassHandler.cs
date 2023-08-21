using Sandbox.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using TerrorTown;
using System.Xml.Linq;
using TerrorTown.GameMenu;
using Sandbox.UI.GameMenu;
using Sandbox.Engine;
using System.ComponentModel;
using TTT_Classes.UI;

namespace TTT_Classes
{
	public class TTT_ClassHeader: BaseNetworkable
	{
		public string Name;
		public string Description { get; set; }

		public Color Color { get; set; }
		
		public TypeDescription TypeDescription { get; set; }

		public float Frequency { get; set; }

		public bool hasActiveAbility { get; set; }
		public TTT_ClassHeader(string name, Color color, string description, TypeDescription typeDescription, float frequency, bool hasactiveAbility) 
		{
			Name = name;
			Description = description;
			Color = color;
			TypeDescription = typeDescription;
			Frequency = Math.Clamp( frequency, 0f, 1f );
			hasActiveAbility = hasactiveAbility;
		}

	}

	public abstract partial class TTT_Class : EntityComponent<TerrorTown.Player>
	{
		
		new public abstract string Name { get; set; }
		public virtual Color Color { get; set; } = new Color(0.2f,0.2f,0.2f);
		
		public abstract float Frequency { get; set; }
		public abstract string Description { get; set; }

		public RealTimeUntil AbilityCooldown;

		public RealTimeSince HoldButtonDown;
		private RealTimeSince lastServerCall;
		public virtual bool hasActiveAbility { get; set; } = false;
		public virtual float coolDownTimer { get; set; } = 60f;
		public virtual float buttonDownDuration { get; set; } = 1f;

		//Run on Ability Trigger
		/// <summary>
		/// Add code that runs when the ability button is held down
		/// </summary>
		public virtual void ActiveAbility() { if ( Entity.LifeState != LifeState.Alive ) return; }

		//Run on start
		/// <summary>
		/// Add code that runs when the round starts
		/// </summary>
		public virtual void RoundStartAbility() { }

		protected TTT_Class()
		{
			//Touching this causes errors!
		}

		/// <summary>
		/// Add a item to the player that owns this class
		/// </summary>
		protected void Add_Item_To_Player(ModelEntity item)
		{
			if(item is Carriable)
			{
				Entity.Inventory.AddItem(item);
				((Carriable)item).Droppable = false;
				((Carriable)item).DropOnDeath = false;
			}
			else
			{
				item.Touch( Entity );
			}
		}
		/// <summary>
		/// Add a item to the player that owns this class
		/// </summary>
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
					spawned.Touch( Entity );
				}
			}
		}

		/// <summary>
		/// Converts html hex color codes to Colors
		/// </summary>
		protected Color Hex_To_Color(string html_color_code)
		{
			if ( html_color_code.Length != 7 )
			{
				throw new ArgumentException( " Hex_To_Color: no hex color code provided" );
			}
			if ( !html_color_code.StartsWith( "#" ))
			{
				throw new ArgumentException( " Hex_To_Color: no hex color code provided" );
			}
			float r = Int32.Parse(  html_color_code.Substring(1,2), System.Globalization.NumberStyles.HexNumber ) /255f;
			float g = Int32.Parse( html_color_code.Substring( 3, 2 ), System.Globalization.NumberStyles.HexNumber )/255f;
			float b = Int32.Parse( html_color_code.Substring( 5, 2 ), System.Globalization.NumberStyles.HexNumber )/255f;
			return new Color(r,g,b); 
		}
		public void Startup()
		{
			RoundStartAbility();
			if(hasActiveAbility)
			{
				HoldButtonDown = 0;
			}
		}

		[GameEvent.Tick.Client]
		protected void Look_For_Active_Button_Press()
		{
			if ( Game.LocalClient.Pawn == Entity && hasActiveAbility && Entity.LifeState == LifeState.Alive)
			{
				if ( AbilityCooldown )
				{
					if ( Input.Down( "Spray" ) )
					{
						if ( HoldButtonDown > buttonDownDuration )
						{
							if ( lastServerCall > 0.1 )
							{
								ConsoleSystem.Run( "class_runablity_consolecommand", Name );
								lastServerCall = 0;
							}
						}
						return;
					}
				}
				HoldButtonDown = 0;
			}
		}

		[ClientRpc]
		public void SetCooldown(IClient target)
		{
			if (Game.LocalClient == target)
			{
				AbilityCooldown = coolDownTimer;
				HoldButtonDown = 0;
			}
		}

	}
	internal partial class ClassHandler
	{
		public static IList<TTT_ClassHeader> Registered_TTT_Classes { get; private set; } = new List<TTT_ClassHeader>();

		// Classes are disabled when they have less than 0 frequency. By saving it this way we can remember both whether a class is enabled and it's frequency in a single float.
		public static IEnumerable<TTT_ClassHeader> Enabled_TTT_Classes { get { return Registered_TTT_Classes.Where( x => x.Frequency > 0 ); } }


		private static string Select_Random_Class_Name()
		{
			int AmountOfClasses = Enabled_TTT_Classes.Count();
			float totalFrequency = Enabled_TTT_Classes.Sum( x => x.Frequency );
			float selection = Game.Random.Float( totalFrequency );
			float frequencyTrack = 0;
			Log.Info( Enabled_TTT_Classes );
			Log.Info( Registered_TTT_Classes );
			foreach ( TTT_ClassHeader header in Enabled_TTT_Classes )
			{
				frequencyTrack = frequencyTrack + header.Frequency;
				Log.Info( "Helo" );
				if ( frequencyTrack > selection )
				{
					return header.Name;
				}
			}
			throw new Exception( "How Did you do this?  --- Random select not functioning" );

		}

		[ConCmd.Server( "class_runablity_consolecommand" )]
		public static void RunActiveAbility( string className )
		{
			TerrorTown.Player commandCaller = (TerrorTown.Player)ConsoleSystem.Caller.Pawn;

			TTT_Class assginedClass = null;
			IEnumerable<TTT_Class> classesAssignedToCaller = commandCaller.Components.GetAll<TTT_Class>();
			foreach ( TTT_Class c in classesAssignedToCaller )
			{
				if ( c.Name == className )
				{
					assginedClass = c;
				}
			}

			if ( assginedClass == null )
			{
				throw new Exception( "No matching class found on player:" + commandCaller.Name + ":" + className );
			}

			if ( assginedClass.AbilityCooldown )
			{
				assginedClass.AbilityCooldown = assginedClass.coolDownTimer;
				assginedClass.HoldButtonDown = 0;
				assginedClass.ActiveAbility();
				assginedClass.SetCooldown( ConsoleSystem.Caller );
			}
			else
			{
				Log.Info( "No" );
			}
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


		internal static TTT_ClassHeader Convert_TTT_Class_2_Header( TypeDescription typeDescription )
		{
			TTT_Class temp = typeDescription.Create<TTT_Class>();
			TTT_ClassHeader header = new TTT_ClassHeader( temp.Name, temp.Color, temp.Description, typeDescription, temp.Frequency, temp.hasActiveAbility );
			return header;
		}


		public static TTT_ClassHeader FindClass( String name )
		{
			foreach ( TTT_ClassHeader C in Registered_TTT_Classes )
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

			//Log.Info( "-------" );
			//Log.Info( "Correctly sent:" + classToAssign );
			TTT_ClassHeader classToApply = FindClass( classToAssign );
			//Log.Info( "translated:" + classToApply.Name );
			ply.Components.Add( classToApply.TypeDescription.Create<TTT_Class>() );
			//Log.Info( ply );
			//Log.Info( ply.Components.Get<TTT_Class>() );
			//Log.Info( "-------" );
			TTT_Class classOnPlayer = ply.Components.Get<TTT_Class>();
			classOnPlayer.Startup();
		}

		[ConCmd.Client( "class_toggle_ui" )]
		public static void ToggleUI()
		{
			Game.AssertClient();
			var panel = Game.RootPanel.ChildrenOfType<UI.ClassSelectorUI>().FirstOrDefault();
			if ( panel == null )
			{
				var newpanel = Game.RootPanel.AddChild<UI.ClassSelectorUI>();
				newpanel.SetLocalClassList( Registered_TTT_Classes );
			}
			else
			{
				panel.Delete();
			}
		}

		[ConCmd.Server( "class_save_config" )]
		public static void Save_Settings()
		{
			if ( !ValidateUser( ConsoleSystem.Caller.Pawn as TerrorTown.Player ) ) { Log.Error( "Insufficient permissions" ); return; }
			Dictionary<string, float> classPairs = new Dictionary<string, float>();

			foreach ( TTT_ClassHeader header in Registered_TTT_Classes )
			{
				classPairs.Add( header.Name, header.Frequency );
			}

			string settings = Json.Serialize( classPairs );
			FileSystem.Data.WriteAllText( "TTT_Class_settings.json", settings );
			Event.Run( "class_full_sync", settings );

		}
		[ConCmd.Server( "class_load_config" )]
		public static void Load_Settings() 
		{
			if ( Game.IsServer )
			{
				Game.AssertServer();
				if ( !FileSystem.Data.FileExists( "TTT_Class_settings.json" ) )
				{
					return;
				}
				string settings = FileSystem.Data.ReadAllText( "TTT_Class_settings.json" );

				Dictionary<string, float> classPairs = Json.Deserialize<Dictionary<string, float>>( settings );
				foreach ( TTT_ClassHeader header in Registered_TTT_Classes )
				{
					bool exists = classPairs.TryGetValue( header.Name, out float freqtoadd );

					if ( exists )
					{
						header.Frequency = freqtoadd;
					}
				}

				Event.Run( "class_full_sync", settings );
			}
		}
		public static void Generate_Registered_Classes()
		{

			Registered_TTT_Classes.Clear();
			Registered_TTT_Classes = new List<TTT_ClassHeader>();
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
					Registered_TTT_Classes.Add( Convert_TTT_Class_2_Header( type ) );
				}

			}
		}


		[ConCmd.Client( "class_get_class_chance" )]
		public static float Get_Class_Chance( string className )
		{

			TTT_ClassHeader c = FindClass( className );
			
			float freq = c.Frequency;
			float TotalFreq = Enabled_TTT_Classes.Sum( x => x.Frequency );

			return (float) (freq / TotalFreq);
				
			

		}

		[ConCmd.Server("class_set_frequency")]
		public static void SetFrequency(string className, float frequency )
		{
			if ( !ValidateUser( ConsoleSystem.Caller.Pawn as TerrorTown.Player ) ) { Log.Error( "Insufficient permissions" ); return; }
			var ttt_class = FindClass( className );
			if ( ttt_class == null ) { Log.Error( className + " was not found on this server." ); return; }
			ttt_class.Frequency = Math.Clamp(frequency, -1, 1);
		}

		//[ConCmd.Client( "class_testing" )]
		//public static void test()
		//{
		//	foreach(var panel in Game.RootPanel.Children)
		//	{
		//		Log.Info( panel );
		//	}
		//	foreach(var panel2 in Game.RootPanel.ChildrenOfType<Health>().FirstOrDefault().Children)
		//	{
		//		Log.Info( panel2 );
		//		Log.Info( "bu" );
		//	}
		//}


		[ConCmd.Client( "class_client_delete_ui" )]
		protected static void ClientRemoveUI()
		{
			if ( Game.IsClient )
			{
				Game.RootPanel.ChildrenOfType<ShowClass>().FirstOrDefault()?.DisableUI();
				Game.RootPanel.ChildrenOfType<ActiveCooldown>().FirstOrDefault()?.DisableActiveUI();
			}
		}

		[ConCmd.Client( "class_client_readd_ui" )]
		protected static void ClientReaddUI()
		{
			if ( Game.IsClient )
			{
				// For now, delete class when died
				Game.RootPanel.ChildrenOfType<ActiveCooldown>().FirstOrDefault()?.Delete();
				var panel = Game.RootPanel.ChildrenOfType<ShowClass>().FirstOrDefault();
				panel?.Init( "none", Color.Gray );
				
			}
		}


		[Event( "Player.PostOnKilled" )]
		public static void RemoveUIOnDeath( DamageInfo lastFound, TerrorTown.Player ply )
		{
			if ( Game.IsServer )
			{
				((IClient)ply.Owner)?.SendCommandToClient( "class_client_delete_ui" );
			}
		}

		[Event( "Player.PostRespawn" )]
		public static void PostRezUI( TerrorTown.Player ply )
		{
			if ( Game.IsServer )
			{
				((IClient)ply.Owner)?.SendCommandToClient( "class_client_readd_ui" );
			}
		}

		[ConCmd.Client( "class_add_class_ui" )]
		public static void AddClassUI(string classname)
		{
			// This line removes the previous class if it exists
			Game.RootPanel.ChildrenOfType<TerrorTown.Health>().FirstOrDefault().Children.Where( x => x.HasClass( "seg" ) ).FirstOrDefault()?.ChildrenOfType<UI.ShowClass>().FirstOrDefault()?.Delete();

			// Finding the assigned class
			TTT_ClassHeader class_header = FindClass( classname );
			if ( class_header == null ) { Log.Error( "Could not find given class for UI update." ); return; }

			// These lines alter the existing UI to accomedate our new element.
			var health = Game.RootPanel.ChildrenOfType<TerrorTown.Health>().FirstOrDefault();
			health?.SetProperty( "style", "border-radius: 0px; border-top-left-radius: 0px; border-top-right-radius: 0px; border-bottom-left-radius: 12px; border-bottom-right-radius: 12px;" );

			// This adds our new element.
			var panel = Game.RootPanel.AddChild<UI.ShowClass>();
			panel.Init( class_header.Name, class_header.Color );
			Game.RootPanel.ChildrenOfType<Chat>().FirstOrDefault()?.SetProperty( "style", "bottom:196px;" );
			if ( class_header.hasActiveAbility )
			{
				Game.RootPanel.AddChild<UI.ActiveCooldown>();
				Game.RootPanel.ChildrenOfType<Chat>().FirstOrDefault()?.SetProperty( "style", "bottom:256px;" );
			}
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

		[Event( "class_full_sync" )]
		[ClientRpc]
		public static void FullClientSync( string classJson )
		{
			var classPairs = Json.Deserialize<Dictionary<string, float>>( classJson );

			foreach ( TTT_ClassHeader header in Registered_TTT_Classes )
			{
				if ( classPairs.TryGetValue( header.Name, out float freqtoadd ) )
				{
					header.Frequency = freqtoadd;
				}
			}
		}

		[TerrorTown.ChatCmd( "classes", PermissionLevel.Moderator )]
		public static void OpenUIChat1()
		{
			ConsoleSystem.Caller.SendCommandToClient( "class_toggle_ui" );
		}

		[TerrorTown.ChatCmd( "class", PermissionLevel.Moderator )]
		public static void OpenUIChat2()
		{
			ConsoleSystem.Caller.SendCommandToClient( "class_toggle_ui" );
		}

		[GameEvent.Server.ClientJoined]
		public static void OnJoin( ClientJoinedEvent _e )
		{
			Dictionary<string, float> classPairs = new Dictionary<string, float>();

			foreach ( TTT_ClassHeader header in Registered_TTT_Classes )
			{
				classPairs.Add( header.Name, header.Frequency );
			}

			string settings = Json.Serialize( classPairs );
			Event.Run( "class_full_sync", settings );
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
			Generate_Registered_Classes();
			Load_Settings();
		}

	}
}


	

