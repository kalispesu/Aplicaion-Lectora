using System;
using System.Collections.Generic;

namespace LectorPdf
{
    public class Book { public string Title { get; set; } public string FilePath { get; set; } public int LastPage { get; set; } = 1; }
    public class Bookmark { public int Page { get; set; } public string Title { get; set; } }
    public class Annotation { public string Id { get; set; } public int Page { get; set; } public string Text { get; set; } public string Comment { get; set; } public string Summary => $"Pág {Page}: { (Text?.Length>40? Text.Substring(0,40)+"...":Text) }"; }
    public class BookmarkDisplay { public int Page { get; set; } public string Display { get; set; } public Bookmark Source { get; set; } }
}
