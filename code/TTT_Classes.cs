﻿using Sandbox;
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
		public override string Description { get; set; } = "Test";
		public override float Frequency { get; set; } = 0.1f;

		public override Color Color { get; set; }


		public override void ActiveAbility()
		{

		}

		//Run on start
		public override void RoundStartAbility()
		{

			Add_Item_To_Player( new GoldDeagle.GoldenDeagle());
		}
	}
	public class Visionary : TTT_Class
	{

		public override string Name { get; set; } = "Visionary";
		public override string Description { get; set; } = "Test";

		public override float Frequency { get; set; } = 1f;
		public override Color Color { get; set; }

		public override void ActiveAbility()
		{
			
		}

		//Run on start
		public override void RoundStartAbility()
		{

			Add_Item_To_Player( new TerrorTown.Visualiser() );
		}
	}

	public class DemolitionExpert : TTT_Class
	{

		public override string Name { get; set; } = "DemolitionExpert";
		public override string Description { get; set; } = "Test";
		public override float Frequency { get; set; } = 0.5f;
		public override Color Color { get; set; }

		public override void ActiveAbility()
		{

		}

		//Run on start
		public override void RoundStartAbility()
		{
	
			Add_Item_To_Player( new TerrorTown.C4() );

		}
	}
	public class Magician : TTT_Class
	{

		public override string Name { get; set; } = "Magician";
		public override string Description { get; set; } = "Test";
		public override float Frequency { get; set; } = 1f;
		public override Color Color { get; set; }

		public override void ActiveAbility()
		{

		}

		//Run on start
		public override void RoundStartAbility()
		{
			Add_Item_To_Player( new TerrorTown.Teleporter() );

		}
	}
	public class WallHack : TTT_Class
	{

		public override string Name { get; set; } = "WallHack";
		public override string Description { get; set; } = "Test";
		public override float Frequency { get; set; } = 0.7f;
		public override Color Color { get; set; }

		public override void ActiveAbility()
		{

		}

		//Run on start
		public override void RoundStartAbility()
		{
			Add_Item_To_Player( new TerrorTown.Radar() );


		}
	}
	public class Junkie : TTT_Class
	{

		public override string Name { get; set; } = "Junkie";
		public override string Description { get; set; } = "Test";
		public override float Frequency { get; set; } = 0.8f;
		public override Color Color { get; set; }

		public override void ActiveAbility()
		{

		}

		//Run on start
		public override void RoundStartAbility()
		{
			((TerrorTown.WalkController)Entity.MovementController).SpeedMultiplier = 1.5f;
		}
	}
	
}
