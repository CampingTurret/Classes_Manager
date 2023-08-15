using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrorTown;

namespace TCT_Classes
{
	public class Test_Class :TTT_Class
	{

		public override string Name { get; set; } = "TestClass";
		public override string Description { get; set; } = "TestClass";

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
	public class Test_Class2 : TTT_Class
	{

		public override string Name { get; set; } = "TestClass2";
		public override string Description { get; set; } = "TestClass2";

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

	public class Test_Class3 : TTT_Class
	{

		public override string Name { get; set; } = "TestClass3";
		public override string Description { get; set; } = "TestClass3";

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
