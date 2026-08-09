using System.IO;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace WikiGen.Handlers;

abstract class ContentHandler<TModType> where TModType : ModType
{
    public abstract string Title { get; }
    public virtual string ImageExtension => "png";
    
    public abstract int GetId(TModType modType);
    public abstract string GetDisplayName(int type);
    public abstract TModType GetModType(int type);
    public abstract void LoadTexture(int type);
    public abstract Asset<Texture2D> GetTexture(int type);

    public virtual Texture2D GetImageTexture(int type, out bool dispose)
    {
        dispose = false;
        LoadTexture(type);
        return GetTexture(type).Value;
    }

    public virtual void StoreImage(int type, Texture2D texture, Stream stream)
    {
        texture.SaveAsPng(stream, texture.Width, texture.Height);
    }

    public PagePath GetImage(int type)
    {
        TModType modType = GetModType(type);
        PagePath path = $"/{Title}/{(modType != null ? modType.Name : type.ToString())}.{ImageExtension}";
        string fullPath = path.FullPath;
        if (!File.Exists(fullPath))
        {
            Texture2D texture = GetImageTexture(type, out bool dispose);
            try
            {
                string dir = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                using Stream stream = File.OpenWrite(fullPath);
                StoreImage(type, texture, stream);
            }
            finally
            {
                if (dispose)
                    texture?.Dispose();
            }
        }
        return path;
    }

    public PagePath GetPageLink(TModType modType)
    {
        return $"/{Title}/{modType.Name}.html";
    }

    public virtual Page CreatePage(Page index, TModType modType)
    {
        Page page = new(GetDisplayName(GetId(modType)), GetPageLink(modType), index.Favicon);
        page.Add(page.Hyperlink(index.PagePath + ("#" + Title.ToLowerInvariant()), "Return home"));
        page.Add(XTable.Create([page.Image(GetImage(GetId(modType))), Elements.Heading(page.Title), Elements.Paragraph(modType.Name)]));
        return page;
    }
    
    public XElement Display(Page page, int type)
    {
        XElement element = Elements.Div();
        element.Add(page.Image(GetImage(type)));
        element.Add(" ");
        TModType modType = GetModType(type);
        if (modType != null)
            element.Add(page.Hyperlink(GetPageLink(modType), GetDisplayName(type)));
        else
            element.Add(GetDisplayName(type));
        return element;
    }
}
