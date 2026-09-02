using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using System.Linq;
using System.IO;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;

namespace WikiGen.Handlers;

using static Elements;

class NPCHandler : ContentHandler<ModNPC>
{
    public override string Title => "NPCs";
    public override string ImageExtension => "webp";

    public override int GetId(ModNPC modType) => modType.Type;
    public override string GetDisplayName(int type) => Lang.GetNPCName(type).ToString();
    public override ModNPC GetModType(int type) => ModContent.GetModNPC(type);
    public override void LoadTexture(int type) => Main.instance.LoadNPC(type);
    public override Asset<Texture2D> GetTexture(int type) => TextureAssets.Npc[type];

    public override bool IsUnlisted(ModNPC modType)
    {
        return modType.NPC.dontCountMe;
    }

    public override void StoreImage(int type, Texture2D texture, Stream stream)
    {
        int frames = Main.npcFrameCount[type];
        int w = texture.Width;
        int h = texture.Height / frames;
        int offsetX = 0;
        int offsetY = 0;
        ModNPC modNpc = GetModType(type);
        if (frames == 1 && modNpc != null)
        {
            modNpc.FindFrame(h);
            offsetX = modNpc.NPC.frame.X;
            offsetY = modNpc.NPC.frame.Y;
            w = modNpc.NPC.frame.Width;
            h = modNpc.NPC.frame.Height;
            if (w <= 0)
                w = texture.Width - offsetX;
            if (h <= 0)
                h = texture.Height - offsetY;
        }
        Rgba32[] data = new Rgba32[w * h];

        using Image<Rgba32> webp = new(w, h);
        webp.Metadata.GetWebpMetadata().RepeatCount = 0;
        for (int i = 0; i < frames; i++)
        {
            texture.GetData(0, new Microsoft.Xna.Framework.Rectangle(0, i * h, w, h), data, 0, data.Length);
            if (i == 0)
            {
                webp.ProcessPixelRows(p =>
                {
                    for (int y = 0; y < p.Height; y++)
                        data.AsSpan().Slice(y * p.Width, p.Width).CopyTo(p.GetRowSpan(y));
                });
            }
            else
                webp.Frames.AddFrame(data).Metadata.GetWebpMetadata().BlendMethod = SixLabors.ImageSharp.Formats.Webp.WebpBlendMethod.Source;
        }
        webp.SaveAsWebp(stream);
    }

    public override Page CreatePage(Page index, ModNPC npc)
    {
        Page page = base.CreatePage(index, npc);
        BestiaryEntry bestiaryEntry = Main.BestiaryDB.FindEntryByNPCID(npc.Type);
        BestiaryUICollectionInfo collectionInfo = new() { OwnerEntry = bestiaryEntry, UnlockState = BestiaryEntryUnlockState.CanShowDropsWithDropRates_4 };
        foreach (IBestiaryInfoElement info in bestiaryEntry.Info)
        {
            if (info is SpawnConditionBestiaryInfoElement spawnCondition)
            {
                page.Add(Heading("Found in", 2));
                page.Add(Paragraph(Language.GetTextValue(spawnCondition.GetDisplayNameKey())));
            }
            if (info is FlavorTextBestiaryInfoElement flavorText)
            {
                string text = ((UIText)flavorText.ProvideUIElement(collectionInfo).Children.First()).Text;
                page.Add(Heading("Description", 2));
                page.Add(RichParagraph(page, text));
            }
        }

        string key = npc.GetLocalizationKey("Census.SpawnCondition");
        if (Language.Exists(key))
        {
            page.Add(Heading("Spawn requirements", 2));
            page.Add(RichParagraph(page, Language.GetTextValue(key)));
        }

        page.Add(Heading("Shops", 2));
        bool any = false;
        foreach (AbstractNPCShop shop in NPCShopDatabase.AllShops)
        {
            if (shop.NpcType != npc.Type)
                continue;
            any = true;
            page.Add(Heading(shop.Name, 3));
            XTable table = new();
            table.AddRow();
            table.AddHeader("Item");
            table.AddHeader("Price");
            table.AddHeader("Requirements");
            foreach (AbstractNPCShop.Entry entry in shop.ActiveEntries)
            {
                table.AddRow();
                table.AddData(Items.Display(page, entry.Item));
                table.AddData(Price(page, entry.Item.GetStoreValue()));
                var list = List();
                foreach (Condition condition in entry.Conditions)
                    list.Add(ListItem(condition.Description.ToString()));
                table.AddData(list);
            }
            page.Add(table);
        }
        if (!any)
            page.Add(Paragraph("This NPC doesn't have any shops."));
        return page;
    }
}
