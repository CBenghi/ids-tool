using IdsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using static IdsLib.Check;

namespace idsTool.tests
{
	public class MainFunctionTests
	{
        private const string schemaFile = @"C:\Data\Dev\XbimPrivate\Xbim.Xids\Xbim.InformationSpecifications.NewTests\bsFiles\ids.xsd";
        private const string idsFIle = @"C:\Data\Dev\XbimPrivate\Xbim.Xids\Xbim.InformationSpecifications.NewTests\bsFiles\IDS_ucms_prefab_pipes_IFC2x3.ids";

        [Fact]
		public void CanRunAsPublic()
		{
            var c = new CheckOptions
            {
                SchemaFiles = new List<string> { schemaFile },
                InputSource = idsFIle
            };

            // to adjust once we fix the xml file in the other repo.
            var t = new StringWriter();
			var ret = Run(c, t);
			Assert.Equal(Status.Ok, ret);
		}

		[Fact]
		public void DoesNotBlockFiles()
		{
			// prepare the file to delete in the end
			var tmp = Path.GetTempFileName();
			File.Copy(idsFIle, tmp, true);

            var c = new CheckOptions
            {
                SchemaFiles = new List<string> { schemaFile },
                InputSource = tmp
            };
            var t = new StringWriter();
			var ret = Run(c, t);

			Assert.Equal(Status.Ok, ret);

			File.Delete(tmp);
		}
	}
}