using System.IO;
using System.Xml.Serialization;

namespace SME.Shared
{
    public static class PolicyReader
    {

        /// <summary>
        /// Deserialize XML file to policy object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T ReadXml<T>(string path) where T : IPolicy
        {
            T policyObject = default(T);
            if (string.IsNullOrEmpty(path)) return default(T);
            StreamReader xmlStream = new StreamReader(path);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            policyObject = (T)serializer.Deserialize(xmlStream);
            return policyObject;
        }

        /*
        public static string SerializeObject<T>(this T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }*/
    }
}
