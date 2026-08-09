using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WikiGen.Common.Systems;

public class WikiSystem : ModSystem
{
    public override void PostSetupRecipes()
    {
        if (Main.netMode == NetmodeID.SinglePlayer && WikiGen.TargetMod != null)
            Main.RunOnMainThread(WikiGen.Instance.GenerateWiki);
    }
}
