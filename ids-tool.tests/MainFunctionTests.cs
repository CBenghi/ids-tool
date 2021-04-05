using IdsLib;
using System;
using System.Collections.Generic;
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
			c.CheckSchema = new List<string> { @"C:\Data\Dev\BuildingSmart\IDS\Development\Third production release\ids.xsd" };
			c.InputSource = @"C:\Data\Dev\BuildingSmart\IDS\Development\Third production release";

			// to adjust once we fix the xml file in the other repo.
			var ret = CheckOptions.Run(c);
			Assert.Equal(Status.ContentError, ret);
		}
	}
}