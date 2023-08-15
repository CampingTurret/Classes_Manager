using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCT_Classes
{
	public class Test_Class :TTT_Class
	{

		public override string Name { get; set; } = "TestClass";
		public override string Description { get; set; } = "TestClass";

		public override Color Color { get; set; }

		public override IList<ModelEntity> ClassItems { get; set; }

		public override void ActiveAbility()
		{

		}

		//Run on start
		public override void RoundStartAbility()
		{
					
		}
	}
}
