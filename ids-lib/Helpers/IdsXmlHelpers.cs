using IdsLib.IdsSchema;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IdsLib.Helpers
{
    public class IdsXmlHelpers
    {
        public static async Task<IdsInformation> GetIdsInformationAsync(FileInfo fileInformation)
        {
            if (fileInformation is null) 
                throw new ArgumentNullException(nameof(fileInformation));
            using var fs = fileInformation.OpenRead();
            return await GetIdsInformationAsync(fs);
        }

        private enum elementName
        {
            undefined,
            ids,
        }

        public static async Task<IdsInformation> GetIdsInformationAsync(Stream stream)
        {
            var ret = new IdsInformation();
            var settings = new XmlReaderSettings
            {
                Async = true
            };

            // First element has to be an IDS
            // var currentElement = elementName.undefined;

            using XmlReader reader = XmlReader.Create(stream, settings);
            try
            {
                while (await reader.ReadAsync())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (!ret.IsIds && reader.LocalName != "ids")
                                return IdsInformation.Invalid("ids element not found in file.");

                            switch (reader.LocalName)
                            {
                                case "ids":
                                    ret.IsIds = true;
                                    //currentElement = elementName.ids;
                                    ret.SchemaLocation = reader.GetAttribute("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
                                    return ret;
                                default:
                                    break;
                            }
                            Debug.WriteLine("Start Element {0}", reader.Name);
                            break;
                        case XmlNodeType.Attribute:
                            Debug.WriteLine("Attribute Node: {0}", await reader.GetValueAsync());
                            break;
                        case XmlNodeType.Text:
                            Debug.WriteLine("Text Node: {0}", await reader.GetValueAsync());
                            break;
                        case XmlNodeType.EndElement:
                            Debug.WriteLine("End Element {0}", reader.Name);
                            break;
                        default:
                            Debug.WriteLine("Other node {0} with value {1}", reader.NodeType, reader.Value);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                return IdsInformation.Invalid(ex.Message);
            }
            return null;
        }
    }
}
