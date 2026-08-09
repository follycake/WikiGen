using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;

namespace WikiGen.Common.Configs;

public class WikiGenConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [Header("General")]
    [DefaultValue("ExampleMod")]
    [ReloadRequired]
    public string TargetModName;

    [CustomModConfigItem(typeof(OpenFolderElement))]
    public string OpenWikiFolder => WikiGen.WikiRoot;
}

public class OpenFolderElement : ConfigElement<string>
{
    public override void OnBind()
    {
        base.OnBind();

        UIAutoScaleTextTextPanel<string> button = new("Open");
        button.SetPadding(0f);
        button.Width.Set(120 + 24 + 12, 0f);
        button.UseInnerDimensions = true;
        button.PaddingLeft = 36;
        button.PaddingRight = 6;
        button.Height.Set(30, 0f);
        button.Left.Set(-4, 0f);
        button.HAlign = 1f;

        button.Append(new UIImage(UICommon.DropdownIconTexture)
        {
            MarginLeft = -36,
            MarginTop = 0,
            RemoveFloatingPointsFromDrawPosition = true
        });

        button.OnLeftClick += (_, _) => {
            string dir = Value;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            Process.Start(new ProcessStartInfo(dir) { UseShellExecute = true });
        };
        Append(button);
    }
}
