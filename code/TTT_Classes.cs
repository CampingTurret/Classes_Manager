using Sandbox;
using Sandbox.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrorTown;
using GoldDeagle;
using Sandbox.Physics;
using SmartMario1_Items;
using Sandbox.ModelEditor.Nodes;

namespace TTT_Classes
{


	
	public class Bountyhunter : TTT_Class
	{

		public override string Name { get; set; } = "Bountyhunter";
		public override string Description { get; set; } = "You've got a gun with the bad guys' name on it. You start with a Golden Deagle.";
		public override float Frequency { get; set; } = 1f;

		public override Color Color { get; set; } = Color.FromRgb( 0xd4af37 );



		//Run on start
		public override void RoundStartAbility()
		{

			Add_Item_To_Player( new GoldDeagle.GoldenDeagle());
		}
	}
	public class Visionary : TTT_Class
	{

		public override string Name { get; set; } = "Visionary";
		public override string Description { get; set; } = "You can see the dead's final moments. You start with a Visualiser.";

		public override float Frequency { get; set; } = 1f;
		public override Color Color { get; set; } = Color.FromRgb( 0x82f1f5 );


		//Run on start
		public override void RoundStartAbility()
		{
			Add_Item_To_Player( new TerrorTown.Visualiser() );
		}
	}

	public class ExplosionResist : EntityComponent<TerrorTown.Player>
	{
		[Event("Player.PreTakeDamage")]
		public void CheckExplode( DamageInfo info, TerrorTown.Player ply )
		{
			if ( ply != Entity ) return;
			if ( info.Tags.Contains( "explosion" ) || info.Tags.Contains( "blast" ) )
			{
				ply.PendingDamage.Damage = Math.Clamp(info.Damage, 0f, 25f);
			}
		}

		public void AddIcon()
		{
			StatusIcon icon = new StatusIcon
			{
				IconPath = "ui/effects/ExplosionResist.png"
			};
			Entity.StatusIcons.AddIcon( icon );
		}
	}

	public class DemolitionExpert : TTT_Class
	{

		public override string Name { get; set; } = "DemolitionExpert";
		public override string Description { get; set; } = "Time to blow it all up! You start with a C4 and explosion resistance.";
		public override float Frequency { get; set; } = 0.7f;
		public override Color Color { get; set; } = Color.FromRgb(0x8c8f9c);


		//Run on start
		public override void RoundStartAbility()
		{
			Add_Item_To_Player( new TerrorTown.C4() );
			var comp = new ExplosionResist();
			Entity.Components.Add( comp );
			comp.AddIcon();
		}
	}
	public class Magician : TTT_Class
	{

		public override string Name { get; set; } = "Magician";
		public override string Description { get; set; } = "Now you see me, now you don't... You start with a teleporter and can magically hover forward with your active ability! Watch out for pits, you still take fall damage.";
		public override float Frequency { get; set; } = 1f;
		public override Color Color { get; set; } = Color.FromRgb( 0xbc3ddb );
		public override bool hasActiveAbility { get; set; } = true;
		public override float coolDownTimer { get; set; } = 20f;
		public override float buttonDownDuration { get; set; } = 0f;

		public override bool hasDuration { get; set; } = true;

		public override float Duration { get; set; } = 5f;

		private bool Active { get; set; } = false;
		private RealTimeSince SetActive { get; set; }

		public override void ActiveAbility()
		{
			base.ActiveAbility();
			Active = true;
			SetActive = 0;
		}

		[GameEvent.Tick.Server]
		public void DoDuration()
		{
			if ( Active )
			{
				if ( SetActive > Duration )
				{
					Active = false;
					return;
				}
				Entity.Velocity = new Vector3( Entity.Velocity.x, Entity.Velocity.y ) + Entity.AimRay.Forward.Normal;
			}
		}

		//Run on start
		public override void RoundStartAbility()
		{
			Active = false;
			Add_Item_To_Player( new TerrorTown.Teleporter() );
		}
	}
	public class WallHack : TTT_Class
	{

		public override string Name { get; set; } = "WallHacker";
		public override string Description { get; set; } = "0PP0n3n7s: L0c47ED! You start with a radar and can initiate a scan with your active ability.";
		public override float Frequency { get; set; } = 1f;
		public override Color Color { get; set; } = Color.Red.Darken( 0.25f );

		public Radar radargiven { get; set; }
		public override bool hasActiveAbility { get; set; } = true;
		public override float coolDownTimer { get; set; } = 30f;
		public override float buttonDownDuration { get; set; } = 0.5f;

		public override void ActiveAbility()
		{
			radargiven.Touch( Entity );
			Entity.StatusIcons.RemoveIcon( Entity.StatusIcons.StatusIcons.FirstOrDefault() );
		}

		//Run on start
		public override void RoundStartAbility()
		{
			radargiven = new TerrorTown.Radar();
			Add_Item_To_Player( radargiven );
		}
	}
	public class Junkie : TTT_Class
	{

		public override string Name { get; set; } = "Junkie";
		public override string Description { get; set; } = "You took a hit of the good stuff, now you run 50% faster!";
		public override float Frequency { get; set; } = 1f;
		public override Color Color { get; set; } = Color.Yellow;

		//Run on start
		public override void RoundStartAbility()
		{
			((TerrorTown.WalkController)Entity.MovementController).SpeedMultiplier = 1.5f;
		}
	}
	public class Hunter : TTT_Class
	{

		public override string Name { get; set; } = "Hunter";
		public override string Description { get; set; } = "After stalking it's prey, the hunter pounces! You can use your active ability to launch yourself forward.";
		public override float Frequency { get; set; } = 1f;
		public override Color Color { get; set; } = Color.FromRgb( 0x80461B );

		public override bool hasActiveAbility { get; set; } = true;
		public override float coolDownTimer { get; set; } = 12f;
		public override float buttonDownDuration { get; set; } = 1f;

		public override void ActiveAbility()
		{

			Entity.Position += new Vector3(0,0,1);
			Entity.Velocity += Vector3.Lerp(Entity.AimRay.Forward.Normal, Vector3.Up, 0.25f) * 1000f;
		}
		public override void RoundStartAbility()
		{
			Entity.Components.RemoveAny<TerrorTown.FallDamageComponent>();
		}
	}

	public class DurationTestJetpack : TTT_Class
	{

		public override string Name { get; set; } = "Faulty Jetpacker";
		public override string Description { get; set; } = "You came prepared with your trusty jetpack! Use your active to activate it. Just don't depend on it too much...";
		public override float Frequency { get; set; } = 0.80f;
		public override Color Color { get; set; } = Color.FromRgb( 0xfa8211 );

		public override bool hasActiveAbility { get; set; } = true;
		public override float coolDownTimer { get; set; } = 20f;
		public override float buttonDownDuration { get; set; } = 1.5f;

		public override bool hasDuration { get; set; } = true;

		public override float Duration { get; set; } = 5f;

		private bool Active { get; set; } = false;
		private RealTimeSince SetActive { get; set; }

		public override void ActiveAbility()
		{
			Active = true;
			SetActive = 0;
		}

		[GameEvent.Tick.Server]
		public void DoDuration()
		{
			if (Active)
			{
				if (SetActive > Duration )
				{
					Active = false;
					Entity.Velocity += Vector3.Lerp( Vector3.Lerp(Entity.AimRay.Forward.Normal, Vector3.Down, 0.5f), Vector3.Up, Game.Random.Float(0, 1) ) * Game.Random.Float(1000, 1750);
					if (Game.Random.Int(4) == 4)
					{
						var expl = new TerrorTown.ExplosionEntity();
						expl.Damage = 35f;
						expl.Radius = 300f;
						expl.ForceScale = 10f;
						expl.Position = Entity.Position;
						expl.Explode(null);
					}
					return;
				}
				Entity.Position += new Vector3( 0, 0, 1 );
				Entity.Velocity += Vector3.Lerp(Entity.AimRay.Forward.Normal, Vector3.Up, Game.Random.Float( 0.45f, 0.65f ) ) * 20f;
			}
		}

		public override void RoundStartAbility()
		{
			Active = false;
			Entity.Components.RemoveAny<TerrorTown.FallDamageComponent>();
		}
	}


	public class FartClass : TTT_Class
	{

		public override string Name { get; set; } = "Gassy";
		public override string Description { get; set; } = "You're gassy! You can use your active ability to fart and push other players around.";
		public override float Frequency { get; set; } = 1f;
		public override Color Color { get; set; } = new Color( 0, 0.75f, 0 );

		public override bool hasActiveAbility { get; set; } = true;
		public override float coolDownTimer { get; set; } = 20f;
		public override float buttonDownDuration { get; set; } = 0f;
		private Particles Particle { get; set; }

		public override void RoundStartAbility()
		{
	}
		public override void ActiveAbility()
		{
			base.ActiveAbility();
			// Following code inspired by the implementation of the discombobulator in TTT by Three Thieves
			Particle = Particles.Create( "particles/discombob_bomb.vpcf" );
			Particle.SetPosition( 0, Entity.Position );
			ParticleCleanupSystem.RegisterForCleanup( Particle );

			
			Entity.PlaySound( "classfart" );
			foreach ( Entity item in Sandbox.Entity.FindInSphere( Entity.Position, 200f ) )
			{
				if ( item == Entity ) { continue; }
				Vector3 normal = (item.Position - Entity.Position).Normal;
				normal.z = Math.Abs( normal.z ) + 1f;
				item.Velocity += normal * 256f;
				ModelEntity modelEntity = item as ModelEntity;
				if ( modelEntity != null )
				{
					modelEntity.PhysicsBody.Velocity += normal * 50f;
				}

			}

		}
	}

	public class Miniman : TTT_Class
	{
		public override string Name { get; set; } = "Miniman";
		public override string Description { get; set; } = "You're small and vulnerable, but fast!";
		public override float Frequency { get; set; } = 1f;
		public override Color Color { get; set; } = Color.FromRgb( 0xf5d4a9 );

		//Run on start
		public override void RoundStartAbility()
		{
			((TerrorTown.WalkController)Entity.MovementController).SpeedMultiplier = 1.75f;
			Entity.LocalScale = 0.75f;
		}

		[GameEvent.Tick.Server]
		public void Tick()
		{
			if ( Entity.Health > 70 ) Entity.Health = 70;
		}
	}

	public class BoringClass : TTT_Class
	{
		public override string Name { get; set; } = "Mr. Responsible";
		public override string Description { get; set; } = "You're a boring person who brought body armor to the fight.";
		public override float Frequency { get; set; } = 0.8f;
		public override Color Color { get; set; } = Color.FromRgb( 0x4A412A );

		//Run on start
		public override void RoundStartAbility()
		{
			Add_Item_To_Player( new BodyArmour());
		}
	}

	public class Spiderman : TTT_Class
	{
		public override string Name { get; set; } = "Spiderman";
		public override string Description { get; set; } = "Pizzatime! You have a grappling hook.";
		public override float Frequency { get; set; } = 1f;
		public override Color Color { get; set; } = Color.FromRgb( 0xa71814 );

		//Run on start
		public override void RoundStartAbility()
		{
			Add_Item_To_Player(new SmartMario1_Items.GrapplingHook());
			Entity.Components.RemoveAny<TerrorTown.FallDamageComponent>();
		}
	}

	public class Bigifier : TTT_Class
	{   
		public override string Name { get; set; } = "Mermaid Man";
		public override string Description { get; set; } = "Set to W for Wumbo! Aim your laser at someone and make them grow temporarily by holding your active ability button!";
		public override float Frequency { get; set; } = 1f;
		public override Color Color { get; set; } = Color.FromRgb( 0x8ca6c6 );

		public override bool hasActiveAbility { get; set; } = true;
		public override float coolDownTimer { get; set; } = 45f;
		public override float buttonDownDuration { get; set; } = 0.4f;

		private RealTimeSince grow { get; set; }

		private TerrorTown.Player grown_person { get; set; }

		private ModelEntity glowingEntity = null;

		private void RemoveGlowing()
		{
			glowingEntity?.Components.RemoveAny<Glow>();
			if ( glowingEntity != null )
			{
				foreach ( var child in glowingEntity.Children )
				{
					var ent = child as ModelEntity;
					if ( ent == null ) continue;
					ent?.Components?.RemoveAny<Glow>();
				}
			}
		}

		public override void ActiveAbility()
		{
			if ( glowingEntity == null )
			{
				var glowRay = Entity.AimRay;
				var glowTrace = Trace.Ray( glowRay, 450f );
				glowTrace = glowTrace.DynamicOnly();
				var tr = glowTrace.Ignore(Entity).Run();
				if ( tr.Entity is TerrorTown.Player modelEntity )
				{
					if ( modelEntity != Entity && modelEntity.IsValid() )
					{
						glowingEntity = modelEntity;
					}
				}
			}

			var ply = glowingEntity as TerrorTown.Player;
			if ( ply == null ) { Log.Info("Player didn't hit anyone!"); return; }
			ply.LocalScale += 0.5f;
			ply.PlaySound( "grow" );
			grown_person = ply;
			grow = 0;
			RemoveGlowing();
			glowingEntity = null;
		}

		[GameEvent.Tick.Server]
		public void ServerTick()
		{
			if (grown_person != null)
			{
				if (grow > 30)
				{
					grown_person.LocalScale -= 0.5f;
					grown_person.PlaySound( "shrink" );
					grown_person = null;
				}
			}
		}

		[GameEvent.Tick.Client]
		public void ClientTick()
		{
			// This block of code represents a targeting system, useful for when you don't want your player to miss an active ability with a target.
			if (Entity == Game.LocalPawn && Input.Down("Spray"))
			{
				var glowRay = Entity.AimRay;
				var glowTrace = Trace.Ray( glowRay, 300f );
				glowTrace = glowTrace.DynamicOnly();
				var tr = glowTrace.Ignore(Entity).Run();

				bool glowFound = false;

				if ( tr.Entity is TerrorTown.Player modelEntity )
				{
					if ( modelEntity != Entity && modelEntity.IsValid() )
					{
						if ( modelEntity != glowingEntity )
						{
							Glow glowComponent = new() { Color = Color.Green, ObscuredColor = Color.Green };
							modelEntity.Components.Add( glowComponent );
							foreach ( var child in modelEntity.Children )
							{
								var ent = child as ModelEntity;
								if ( ent == null ) continue;
								ent.Components.Add( new Glow() { Color = Color.Green, ObscuredColor = Color.Green } );
							}
							RemoveGlowing();
							glowingEntity = modelEntity;
						}
						glowFound = true;
					}
				}
				if ( !glowFound && glowingEntity != null )
				{
					RemoveGlowing();
					glowingEntity = null;
				}
			}
			if (Input.Released("Spray"))
			{
				if ( glowingEntity != null )
				{
					RemoveGlowing();
					glowingEntity = null;
				}
			}
		}
	}
}
