using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace WikiGen;

/// <summary>
/// Represents an absolute, rooted path on the website
/// </summary>
readonly struct PagePath
{
    public bool IsEmpty => string.IsNullOrEmpty(WikiPath);

    public string WikiPath { get; }
    public string FullPath => Path.Combine(WikiGen.WikiRoot, WikiPath.TrimStart('/'));

    public PagePath(string path)
    {
        if (!path.StartsWith('/'))
            path = "/" + path;
        WikiPath = path;
    }

    public string GetLinkTo(string path)
    {
        if (Path.IsPathRooted(path))
            return Path.GetRelativePath(Path.GetDirectoryName(WikiPath), path);
        return path;
    }

    public string GetLinkTo(PagePath path)
    {
        return Path.GetRelativePath(Path.GetDirectoryName(WikiPath), path.WikiPath);
    }

    public static implicit operator PagePath(string path) => new(path);
    public static PagePath operator +(PagePath a, string b) => new(a.WikiPath + b);
}

class Page
{
    public PagePath PagePath { get; }
    public PagePath Favicon { get; }
    public string Title { get; }

    public XDocument Document { get; }
    public XElement Body { get; }

    public Page(string title, PagePath path, PagePath favicon = default)
    {
        Title = title;
        PagePath = path;

        XElement style = new("link");
        style.SetAttributeValue("rel", "stylesheet");
        style.SetAttributeValue("href", PagePath.GetLinkTo("/style.css"));

        XElement icon = null;
        if (!favicon.IsEmpty)
        {
            Favicon = favicon;
            icon = new("link");
            icon.SetAttributeValue("rel", "icon");
            icon.SetAttributeValue("type", "image/x-icon");
            icon.SetAttributeValue("href", PagePath.GetLinkTo(favicon));
        }

        Body = new XElement("body");
        Document = new XDocument(
            new XElement("html",
                new XElement("head",
                    new XElement("title", title),
                    style,
                    icon
                ),
                Body
            )
        );
    }

    public void Add(XElement element)
    {
        Body.Add(element);
    }

    public void Add(IEnumerable<XElement> elements)
    {
        foreach (XElement element in elements)
            Add(element);
    }

    public XElement Hyperlink(PagePath path, string text) => Elements.Hyperlink(PagePath.GetLinkTo(path), text);

    public XElement Hyperlink(Page page, string text) => Hyperlink(page.PagePath, text);

    public XElement Hyperlink(Page page) => Hyperlink(page, page.Title);

    public XElement Image(PagePath src, string alt = null) => Image(src.WikiPath, alt);

    public XElement Image(string src, string alt = null)
    {
        XElement element = new("img");
        element.SetAttributeValue("src", PagePath.GetLinkTo(src));
        if (alt != null)
            element.SetAttributeValue("alt", alt);
        return element;
    }

    public void Save()
    {
        string fullPath = PagePath.FullPath;
        string dir = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(fullPath, "<!DOCTYPE html>\n" + Document.ToString());
    }
}
