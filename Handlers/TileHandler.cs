using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace WikiGen.Handlers;

class TileHandler : ContentHandler<ModTile>
{
    public override string Title => "Tiles";

    public override int GetId(ModTile modType) => modType.Type;
    public override string GetDisplayName(int type)
    {
        ModTile tile = GetModType(type);
        if (tile != null)
            return tile.Name;
        return null;
    }
    public override ModTile GetModType(int type) => ModContent.GetModTile(type);
    public override void LoadTexture(int type) => Main.instance.LoadTiles(type);
    public override Asset<Texture2D> GetTexture(int type) => TextureAssets.Tile[type];
}
