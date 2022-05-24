using IdsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using static IdsLib.CheckOptions;

namespace idsTool.tests
{
	public class MainFunctionTests
	{
		[Fact]
		public void CanRunAsPublic()
		{
			CheckOptions c = new CheckOptions();
			c.CheckSchema = new List<string> { @"C:\Data\Dev\XbimPrivate\Xbim.Xids\Xbim.InformationSpecifications.NewTests\bsFiles\ids_06.xsd" };
			c.InputSource = @"C:\Data\Dev\XbimPrivate\Xbim.Xids\Xbim.InformationSpecifications.NewTests\bsFiles\IDS_ucms_prefab_pipes_IFC2x3.xml";

			// to adjust once we fix the xml file in the other repo.
			var t = new StringWriter();
			var ret = CheckOptions.Run(c, t);

			Assert.Equal(Status.Ok, ret);
		}
	}
}