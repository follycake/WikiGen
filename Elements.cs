using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using WikiGen.Handlers;

namespace WikiGen;

static class Elements
{
    public static readonly BuffHandler Buffs = new();
    public static readonly ItemHandler Items = new();
    public static readonly NPCHandler NPCs = new();
	public static readonly TileHandler Tiles = new();

	public static XElement Div(object content = null) => new("div", content);

	public static XElement Div(params object[] content) => new("div", content);

    public static XElement Heading(string text, int number = 1) => new("h" + number, text);

    public static XElement Paragraph(string text) => new("p", text);

    public static IEnumerable<XElement> Paragraphs(string text)
	{
        foreach (string str in text.Split('\n'))
            yield return Paragraph(str);
    }

    public static XElement RichParagraph(Page page, string text)
	{
		var p = Paragraph(null);
        p.SetAttributeValue("class", "rich-text");
        p.Add(" "); // Avoids new line issue
		Color baseColor = Color.Black;
		List<TextSnippet> snippets = ChatManager.ParseMessage(text, baseColor);
		foreach (TextSnippet snippet in snippets)
		{
			if (snippet.TextOriginal.StartsWith("[i:"))
			{
				string icon = snippet.TextOriginal[3..^1];
				if (int.TryParse(icon, out int type))
					p.Add(page.Image(Items.GetImage(type)));
				else if (icon.Contains('/'))
				{
					string modName = icon[..icon.IndexOf('/')];
					string itemName = icon[(icon.LastIndexOf('/') + 1)..];
					if (ModLoader.TryGetMod(modName, out Mod mod) && mod.TryFind(itemName, out ModItem item))
						p.Add(page.Image(Items.GetImage(item.Type)));
				}
				continue;
			}
			if (snippet.Color != baseColor && snippet.Color != Color.White)
			{
				var span = new XElement("span", snippet.Text);
				span.SetAttributeValue("style", "color: #" + snippet.Color.Hex3());
				p.Add(span);
				continue;
			}
			p.Add(snippet.Text);
		}
		return p;
	}

    public static XElement List(IEnumerable<object> elements)
    {
        XElement list = List();
        foreach (object element in elements)
            list.Add(ListItem(element));
        return list;
    }

    public static XElement List() => new("ul");

    public static XElement ListItem(object content = null) => new("li", content);

	public static XElement Small(object content = null) => new("small", content);

	public static XElement Hyperlink(string href, string text)
	{
		XElement element = new("a", text);
		element.SetAttributeValue("href", href);
		return element;
	}
	
	public static XElement Price(Page page, int price)
	{
		// price = copper + silver * 100 + gold * 100 * 100 + platinum * 100 * 100 * 100
		const int p = 100 * 100 * 100;
		const int g = 100 * 100;
		const int s = 100;

		int platinum = price / p;
		int gold = (price - platinum * p) / g;
		int silver = (price - platinum * p - gold * g) / s;
		int copper = price - platinum * p - gold * g - silver * s;

		XElement element = Div();
		if (platinum > 0)
		{
			element.Add(page.Image(Items.GetImage(ItemID.PlatinumCoin)));
			element.Add(" " + platinum + " ");
		}
		if (gold > 0)
		{
			element.Add(page.Image(Items.GetImage(ItemID.GoldCoin)));
			element.Add(" " + gold + " ");
		}
		if (silver > 0)
		{
			element.Add(page.Image(Items.GetImage(ItemID.SilverCoin)));
			element.Add(" " + silver + " ");
		}
		if (copper > 0)
		{
			element.Add(page.Image(Items.GetImage(ItemID.CopperCoin)));
			element.Add(" " + copper + " ");
		}
		return element;
	}
}
