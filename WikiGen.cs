using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using WikiGen.Common.Configs;
using WikiGen.Handlers;

namespace WikiGen;

using static Elements;

// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
public partial class WikiGen : Mod
{
	public static WikiGen Instance { get; private set; }
	public static Mod TargetMod { get; private set; }
	public static string WikiRoot { get; private set; }

	public override void Load()
	{
		Instance = this;
		WikiRoot = Path.Combine(Main.SavePath, "WikiGen");
		if (ModLoader.TryGetMod(ModContent.GetInstance<WikiGenConfig>().TargetModName, out Mod mod))
			TargetMod = mod;
	}

	public void GenerateWiki()
	{
		if (Directory.Exists(WikiRoot))
		{
			foreach (string file in Directory.EnumerateFiles(WikiRoot))
				File.Delete(file);
			foreach (string dir in Directory.EnumerateDirectories(WikiRoot))
			{
				if (Path.GetFileName(dir).Equals(".git", StringComparison.InvariantCultureIgnoreCase))
					continue;
				Directory.Delete(dir, true);
			}
		}
		else
			Directory.CreateDirectory(WikiRoot);
		CopyToWiki(this, "style.css", "/style.css");
		CopyToWiki(TargetMod, "icon_small.png", "/icon.png");
		CopyToWiki(TargetMod, "icon.png", "/icon_medium.png");
		ExtractTexture(TargetMod, "icon_workshop", "/icon_large.png");
		IndexPage();

		//File.WriteAllText(new PagePath("/files.txt").FullPath, string.Join('\n', TargetMod.GetFileNames()));
	}

	static void CopyToWiki(Mod mod, string modFile, PagePath wikiFile)
	{
		if (!mod.FileExists(modFile))
			return;
		using Stream fileStream = File.OpenWrite(wikiFile.FullPath);
		using Stream modStream = mod.GetFileStream(modFile, true);
		modStream.CopyTo(fileStream);
	}

	static void ExtractTexture(Mod mod, string modFile, PagePath wikiFile)
	{
		Texture2D texture = mod.Assets.Request<Texture2D>(modFile, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
		using Stream stream = File.OpenWrite(wikiFile.FullPath);
		texture.SaveAsPng(stream, texture.Width, texture.Height);
	}

	static void IndexPage()
	{
		Page index = new(TargetMod.DisplayNameClean + " Wiki!", "/index.html", "/icon.png");
		index.Add(XTable.Create([index.Image("/icon_medium.png"), Heading(index.Title)]));
		index.Add(Paragraph("Version: " + TargetMod.Version));
		index.Add(index.Image("/icon_large.png"));

		index.Add(Heading("Table of contents", 2));
		var tableOfContents = List();
		index.Add(tableOfContents);

		CreatePages(index, tableOfContents, NPCs);
		CreatePages(index, tableOfContents, Buffs);
		CreatePages(index, tableOfContents, Items);

		// Description
		tableOfContents.Add(ListItem(Hyperlink("#description", "Description")));
		index.Add(Heading("Description", 2).WithId("description"));
		index.Add(RichParagraph(index, Encoding.UTF8.GetString(TargetMod.GetFileBytes("description.txt"))));
		index.Save();
	}

	static void CreatePages<T>(Page index, XElement tableOfContents, ContentHandler<T> handler) where T : ModType
	{
		List<Page> pages = [];
		foreach (T content in TargetMod.GetContent<T>())
		{
			Page page = handler.CreatePage(index, content);
			page.Save();
			pages.Add(page);
		}
		pages.Sort((a, b) => a.Title.CompareTo(b.Title));

		string id = handler.Title.ToLowerInvariant();
		index.Add(Heading(handler.Title, 2).WithId(id));
		tableOfContents.Add(ListItem(Hyperlink("#" + id, handler.Title)));

		XTable table = new();
		foreach (Page page in pages)
		{
			table.AddRow();
			table.AddData(index.Image(Path.ChangeExtension(page.PagePath.WikiPath, handler.ImageExtension)));
			table.AddData(index.Hyperlink(page));
		}
		index.Add(table);
	}
}
