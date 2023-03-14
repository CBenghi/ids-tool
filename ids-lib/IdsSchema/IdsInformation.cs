using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdsLib.IdsSchema
{
    public enum IdsVersion
    {
        Invalid,
        Ids0_5,
        Ids0_9,
        Ids1_0,
    }

    public class IdsInformation
    {
        public bool IsIds { get; internal set; } = false;
        public string SchemaLocation { get; internal set; }
        public string Message { get; internal set; }
        public IdsVersion Version
        {
            get
            {
                return SchemaLocation switch
                {
                    
                    "http://standards.buildingsmart.org/IDS/ids_05.xsd" => IdsVersion.Ids0_5, // todo: this is invalid and needs to be fixed in the IDS repository
                    "http://standards.buildingsmart.org/IDS  ids_09.xsd" => IdsVersion.Ids0_9, // todo: this is invalid and needs to be fixed in the IDS repository
                    "http://standards.buildingsmart.org/IDS http://standards.buildingsmart.org/IDS/ids_09.xsd" => IdsVersion.Ids0_9,
                    "http://standards.buildingsmart.org/IDS http://standards.buildingsmart.org/IDS/ids_1_0.xsd" => IdsVersion.Ids1_0,
                    _ => IdsVersion.Invalid,
                };
            }
        }   

        internal static IdsInformation Invalid(string InvalidMessage)
        {
            return new IdsInformation
            {
                SchemaLocation = IdsVersion.Invalid.ToString(),
                Message = InvalidMessage
            };
        }
    }
}
