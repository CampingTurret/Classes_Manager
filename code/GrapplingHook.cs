using Editor;
using Sandbox;
using Sandbox.Internal;
using TerrorTown;

[Title( "Grappling Hook" ), Category( "Weapons" )]
public class GrapplingHook : Carriable
{
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";
	public override string WorldModelPath => "weapons/rust_pistol/rust_pistol.vmdl";

	private Particles rope { get; set; }

	public RealTimeSince shoot { get; set; }

	private Vector3 hookPos { get; set; }

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
		if ( Input.Released( "Attack1" ) )
		{
			hookPos = new Vector3();
			rope?.Destroy( true );
			rope = null;
		}
		if ( Input.Down( "Attack1" ) )
		{
			if ( hookPos == new Vector3() )
			{
				var glowRay = Owner.AimRay;
				var glowTrace = Trace.Ray( glowRay, 1000f );
				glowTrace = glowTrace.StaticOnly();
				var tr = glowTrace.Ignore( Owner ).Run();

				if ( !tr.Hit ) return;
				hookPos = tr.EndPosition;
				Log.Info( tr.Entity );
				var localOrigin1 = tr.Body.Transform.PointToLocal( hookPos );

				rope = Particles.Create( "particles/rope.vpcf" );
				rope.SetPosition( 0, hookPos );


			}
			DebugOverlay.Sphere( hookPos, 10f, Color.Green );
			shoot = 0;

			rope.SetPosition( 1, ((TerrorTown.Player)Owner).EyePosition - (Vector3.Up * 20f) );
			Owner.Velocity += ((hookPos - Owner.Position).Normal + new Vector3( 0, 0, 1 )) * 10f;
		}
	}
}
