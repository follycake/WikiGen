using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Humanizer;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace WikiGen.Handlers;

using static Elements;

class ItemHandler : ContentHandler<ModItem>
{
    public override string Title => "Items";

    public override int GetId(ModItem modType) => modType.Type;
    public override string GetDisplayName(int type) => Lang.GetItemName(type).ToString();
    public override ModItem GetModType(int type) => ModContent.GetModItem(type);
    public override void LoadTexture(int type) => Main.instance.LoadItem(type);
    public override Asset<Texture2D> GetTexture(int type) => TextureAssets.Item[type];

    public override string GetCategory(ModItem item, out int order)
    {
        ContentSamples.CreativeHelper.ItemGroup itemGroup = ContentSamples.CreativeHelper.ItemGroup.EverythingElse;
        item.ModifyResearchSorting(ref itemGroup);
        order = (int)itemGroup;
        return itemGroup.Humanize();
    }

    public XElement Display(Page page, Item item)
    {
        XElement element = Display(page, item.type);
        element.Add(Small(" x " + item.stack));
        return element;
    }

    public override Page CreatePage(Page index, ModItem item)
    {
        Page page = base.CreatePage(index, item);
        page.Add(GetTooltips(item).Select(line => RichParagraph(page, line.Text)));

        if (item.Item.createTile != -1)
        {
            page.Add(Heading("Places tile", 2));
            page.Add(page.Image(Tiles.GetImage(item.Item.createTile)));
        }

        if (item.Item.buffType != 0)
        {
            page.Add(Heading("Applies buff", 2));
            page.Add(Buffs.Display(page, item.Item.buffType));
            page.Add(Paragraph($"For {item.Item.buffTime / 60} second(s)"));
        }

        page.Add(Heading("Crafting", 2));

        static XTable RecipeTable()
        {
            XTable table = new();
            table.SetAttributeValue("class", "recipe");
            table.AddRow();
            table.AddHeader("Result");
            table.AddHeader("Ingredients");
            table.AddHeader("Station");
            return table;
        }

        static void AddRecipe(Page page, XTable table, Recipe recipe)
        {
            table.AddRow();
            table.AddData(Items.Display(page, recipe.createItem));

            var items = List();
            foreach (Item ingredient in recipe.requiredItem)
                items.Add(ListItem(Items.Display(page, ingredient)));
            table.AddData(items);

            var tiles = List();
            foreach (int tile in recipe.requiredTile)
            {
                var listItem = ListItem(page.Image(Tiles.GetImage(tile)));
                ModTile modTile = ModContent.GetModTile(tile);
                if (modTile != null)
                    listItem.Add(modTile.Name);
                tiles.Add(listItem);
            }
            table.AddData(tiles);
        }

        XTable crafting = RecipeTable();
        XTable usedIn = RecipeTable();

        foreach (Recipe recipe in Main.recipe)
        {
            if (recipe.createItem.type == item.Type)
                AddRecipe(page, crafting, recipe);
            if (recipe.requiredItem.Exists(ingredient => ingredient.type == item.Type))
                AddRecipe(page, usedIn, recipe);
            else if (item.Item.createTile != -1 && recipe.requiredTile.Contains(item.Item.createTile))
                AddRecipe(page, usedIn, recipe);
        }

        page.Add(Heading("How to craft", 3));
        if (crafting.RowCount <= 1)
            page.Add(Paragraph("This item is uncraftable."));
        else
            page.Add(crafting);
        page.Add(Heading("Used in", 3));
        if (usedIn.RowCount <= 1)
            page.Add(Paragraph("This item is not used to craft other items."));
        else
            page.Add(usedIn);

        XTable soldIn = new();
        soldIn.AddRow();
        soldIn.AddHeader("NPC");
        soldIn.AddHeader("Price");

        foreach (AbstractNPCShop shop in NPCShopDatabase.AllShops)
        {
            foreach (AbstractNPCShop.Entry entry in shop.ActiveEntries)
            {
                if (entry.Item.type != item.Type)
                    continue;
                soldIn.AddRow();
                soldIn.AddData(NPCs.Display(page, shop.NpcType));
                soldIn.AddData(Price(page, entry.Item.GetStoreValue()));
            }
        }

        if (soldIn.RowCount > 1)
        {
            page.Add(Heading("Sold in shops", 2));
            page.Add(soldIn);
        }

        XTable droppedBy = new();
        droppedBy.AddRow();
        droppedBy.AddHeader("NPC");
        droppedBy.AddHeader("Amount");
        droppedBy.AddHeader("Chance");

        List<DropRateInfo> drops = [];
        foreach (KeyValuePair<int, NPC> pair in ContentSamples.NpcsByNetId)
        {
            int netId = pair.Key;
            NPC npc = pair.Value;
            foreach (IItemDropRule dropRule in Main.ItemDropsDB.GetRulesForNPCID(netId))
            {
                drops.Clear();
                dropRule.ReportDroprates(drops, new DropRateInfoChainFeed(1f));
                foreach (DropRateInfo drop in drops)
                {
                    if (drop.itemId != item.Type)
                        continue;
                    droppedBy.AddRow();
                    droppedBy.AddData(NPCs.Display(page, npc.type));
                    droppedBy.AddData(drop.stackMin == drop.stackMax ? drop.stackMin.ToString() : drop.stackMin + " to " + drop.stackMax);
                    droppedBy.AddData(MathF.Round(drop.dropRate * 100f) + "%");
                }
            }
        }

        if (droppedBy.RowCount > 1)
        {
            page.Add(Heading("Dropped by", 2));
            page.Add(droppedBy);
        }
        return page;
    }

    readonly static string[] _tooltipNames = new string[30];
    readonly static string[] _tooltipLines = new string[30];
	readonly static bool[] _tooltipUnused = new bool[30];

    static List<TooltipLine> GetTooltips(ModItem modItem)
    {
        Item item = modItem.Item;
        int numTooltips = 0;
        string[] lines = modItem.Tooltip.ToString().ReplaceLineEndings("\n").Split('\n');
		try
        {
            int unusedYoyoLogo = 0;
            int unusedResearchLine = 0;
            Main.MouseText_DrawItemTooltip_GetLinesInfo(item, ref unusedYoyoLogo, ref unusedResearchLine, item.knockBack, ref numTooltips, _tooltipLines, _tooltipUnused, _tooltipUnused, _tooltipNames, out _);
            for (int i = 0; i < lines.Length; i++)
            {
                _tooltipNames[numTooltips] = "Tooltip" + i;
                _tooltipLines[numTooltips] = lines[i];
                numTooltips++;
            }
            string[] text = _tooltipLines;
            bool[] modifier = _tooltipUnused;
		    bool[] badModifier = _tooltipUnused;
            int oneDropLogo = 0;
            return ItemLoader.ModifyTooltips(item, ref numTooltips, _tooltipNames, ref text, ref modifier, ref badModifier, ref oneDropLogo, out _, -1);
		}
		catch (Exception e)
        {
			List<TooltipLine> tooltips = [];
			for (int i = 0; i < lines.Length; i++)
				tooltips.Add(new TooltipLine(modItem.Mod, "Tooltip" + i, lines[i]));
			tooltips.Add(new TooltipLine(modItem.Mod, "ERROR", "Failed to ModifyTooltips: " + e));
			return tooltips;
		}
	}
}
