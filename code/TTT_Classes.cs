using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrorTown;
using GoldDeagle;



namespace TCT_Classes
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

	public class DemolitionExpert : TTT_Class
	{

		public override string Name { get; set; } = "DemolitionExpert";
		public override string Description { get; set; } = "Time to blow it all up! You start with a C4.";
		public override float Frequency { get; set; } = 0.5f;
		public override Color Color { get; set; } = Color.FromRgb(0x8c8f9c);


		//Run on start
		public override void RoundStartAbility()
		{
	
			Add_Item_To_Player( new TerrorTown.C4() );

		}
	}
	public class Magician : TTT_Class
	{

		public override string Name { get; set; } = "Magician";
		public override string Description { get; set; } = "Now you see me, now you don't... You start with a teleporter.";
		public override float Frequency { get; set; } = 1f;
		public override Color Color { get; set; } = Color.FromRgb( 0xbc3ddb );


		//Run on start
		public override void RoundStartAbility()
		{
			Add_Item_To_Player( new TerrorTown.Teleporter() );

		}
	}
	public class WallHack : TTT_Class
	{

		public override string Name { get; set; } = "WallHacker";
		public override string Description { get; set; } = "0PP0n3n7s: L0c47ED! You start with a radar.";
		public override float Frequency { get; set; } = 1f;
		public override Color Color { get; set; } = Color.Red.Darken( 0.25f );


		//Run on start
		public override void RoundStartAbility()
		{
			Add_Item_To_Player( new TerrorTown.Radar() );
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
		public override float coolDownTimer { get; set; } = 15f;
		public override float buttonDownDuration { get; set; } = 2f;

		public override void ActiveAbility()
		{
			Entity.Position += new Vector3(0,0,1);
			Entity.Velocity += Entity.AimRay.Forward.Normal *800f;
		}
		public override void RoundStartAbility()
		{
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

		private string fartsound ;

		private SoundEvent fart { get; set; }
		private Particles Particle { get; set; }

		public override void RoundStartAbility()
		{
			base.RoundStartAbility();
			fart = Cloud.SoundEvent( "smartmario.smallfart" );
			fart.Create();
			fartsound = fart.ResourceName;
	}
		public override void ActiveAbility()
		{
			// Following code inspired by the implementation of the discombobulator in TTT by Three Thieves
			Particle = Particles.Create( "particles/discombob_bomb.vpcf" );
			Particle.SetPosition( 0, Entity.Position );
			ParticleCleanupSystem.RegisterForCleanup( Particle );

			
			Entity.PlaySound( "mgfart" );
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

}
