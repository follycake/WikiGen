using System.Xml.Linq;

namespace WikiGen;

public static class Utility
{
    public static XElement WithAttribute(this XElement element, string name, object value)
    {
        element.SetAttributeValue(name, value);
        return element;
    }

    public static XElement WithClass(this XElement element, string value) => WithAttribute(element, "class", value);

    public static XElement WithId(this XElement element, string value) => WithAttribute(element, "id", value);
}
