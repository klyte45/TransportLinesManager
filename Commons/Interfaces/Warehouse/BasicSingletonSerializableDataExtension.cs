using Klyte.Commons.Utils;
using System;
using System.Xml.Serialization;

namespace Klyte.Commons.Interfaces
{
    public interface IDataExtensor
    {
        string SaveId { get; }
        IDataExtensor Deserialize(Type type, string data);
        string Serialize();
    }

    [XmlRoot("DataExtensor")]
    public abstract class DataExtensorBase<U> : IDataExtensor where U : DataExtensorBase<U>, new()
    {
        public abstract string SaveId { get; }

        public static U Instance
        {
            get {
                if (!ExtensorContainer.instance.Instances.TryGetValue(typeof(U), out IDataExtensor result) || result == null)
                {
                    ExtensorContainer.instance.Instances[typeof(U)] = new U();
                }
                return ExtensorContainer.instance.Instances[typeof(U)] as U;
            }
        }


        public IDataExtensor Deserialize(Type type, string data) => XmlUtils.DefaultXmlDeserialize<U>(data);
        public string Serialize() => XmlUtils.DefaultXmlSerialize((U) this, false);
    }
}
