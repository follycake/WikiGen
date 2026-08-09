using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace WikiGen.Handlers;

using static Elements;

class BuffHandler : ContentHandler<ModBuff>
{
    public override string Title => "Buffs";

    public override int GetId(ModBuff modType) => modType.Type;
    public override string GetDisplayName(int type) => Lang.GetBuffName(type);
    public override ModBuff GetModType(int type) => ModContent.GetModBuff(type);
    public override void LoadTexture(int type) { }
    public override Asset<Texture2D> GetTexture(int type) => TextureAssets.Buff[type];

    public override Page CreatePage(Page index, ModBuff buff)
    {
        Page page = base.CreatePage(index, buff);

        GetBuffText(buff.Type, out string name, out string tip, out _);
        page.Add(Heading(name));
        page.Add(RichParagraph(page, tip));

        return page;
    }
    
    static void GetBuffText(int buff, out string name, out string tip, out int rare)
    {
        string defName = Lang.GetBuffName(buff);
        string defTip = Lang.GetBuffDescription(buff);
        int defRare = Main.meleeBuff[buff] ? -10 : 0;
        try
        {
            name = defName;
            tip = defTip;
            rare = defRare;
            BuffLoader.ModifyBuffText(buff, ref name, ref tip, ref rare);
        }
        catch (Exception e)
        {
            name = defName;
            tip = defTip;
            rare = defRare;
            tip += "\n" + e;
        }
    }
}
