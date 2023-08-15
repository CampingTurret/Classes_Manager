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
		public override string Description { get; set; } = "Test";

		public override Color Color { get; set; }


		public override void ActiveAbility()
		{

		}

		//Run on start
		public override void RoundStartAbility()
		{

			Add_Item_To_Player( new GoldDeagle.GoldenDeagle());
			Add_Item_To_Player( new TerrorTown.Radar() );
		}
	}
	public class Visionary : TTT_Class
	{

		public override string Name { get; set; } = "Visionary";
		public override string Description { get; set; } = "Test";

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

		public override Color Color { get; set; }

		public override void ActiveAbility()
		{

		}

		//Run on start
		public override void RoundStartAbility()
		{
			Entity.Inventory.AddItem( new TerrorTown.C4() );
			
		}
	}
	public class Test4 : TTT_Class
	{

		public override string Name { get; set; } = "Test4";
		public override string Description { get; set; } = "Test";

		public override Color Color { get; set; }

		public override void ActiveAbility()
		{

		}

		//Run on start
		public override void RoundStartAbility()
		{
			Entity.Inventory.AddItem( new TerrorTown.C4() );

		}
	}
}
