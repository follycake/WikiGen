using System.Xml.Linq;

namespace WikiGen;

class XTable : XElement
{
    public int RowCount { get; private set; }
    public XElement CurrentRow { get; private set; }
    
    public XTable() : base("table")
    {
    }

    public XElement AddRow()
    {
        CurrentRow = new XElement("tr");
        Add(CurrentRow);
        RowCount++;
        return CurrentRow;
    }

    public XElement AddRow<T>(params T[] data)
    {
        AddRow();
        foreach (T content in data)
            AddData(content);
        return CurrentRow;
    }

    public XElement AddData(object content)
    {
        if (CurrentRow == null)
            AddRow();
        XElement element = new("td", content);
        CurrentRow.Add(element);
        return element;
    }

    public XElement AddHeader(object content)
    {
        if (CurrentRow == null)
            AddRow();
        XElement element = new("th", content);
        CurrentRow.Add(element);
        return element;
    }

    public static XTable Create<T>(params T[][] data)
    {
        XTable table = new();
        foreach (T[] row in data)
            table.AddRow(row);
        return table;
    }
}
