using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace LectorPdf
{
    public class LibraryManager
    {
        private string dataFolder;
        private string libraryFile;

        public LibraryManager()
        {
            dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LectorPDF");
            if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);
            libraryFile = Path.Combine(dataFolder, "library.json");
        }

        public List<Book> LoadLibrary() { if (!File.Exists(libraryFile)) return new List<Book>(); var txt = File.ReadAllText(libraryFile); return JsonConvert.DeserializeObject<List<Book>>(txt) ?? new List<Book>(); }
        public void SaveLibrary(List<Book> books) { File.WriteAllText(libraryFile, JsonConvert.SerializeObject(books, Formatting.Indented)); }

        public List<Annotation> LoadAnnotations(string pdfPath) { var fname = GetMetaPath(pdfPath, "annotations.json"); if (!File.Exists(fname)) return new List<Annotation>(); return JsonConvert.DeserializeObject<List<Annotation>>(File.ReadAllText(fname)) ?? new List<Annotation>(); }
        public void SaveAnnotations(string pdfPath, List<Annotation> annotations) { var fname = GetMetaPath(pdfPath, "annotations.json"); File.WriteAllText(fname, JsonConvert.SerializeObject(annotations, Formatting.Indented)); }

        public void AddBookmark(string pdfPath, Bookmark bm) { var list = LoadBookmarks(pdfPath); list.Add(bm); SaveBookmarks(pdfPath, list); }
        public List<Bookmark> LoadBookmarks(string pdfPath) { var fname = GetMetaPath(pdfPath, "bookmarks.json"); if (!File.Exists(fname)) return new List<Bookmark>(); return JsonConvert.DeserializeObject<List<Bookmark>>(File.ReadAllText(fname)) ?? new List<Bookmark>(); }
        public void SaveBookmarks(string pdfPath, List<Bookmark> marks) { var fname = GetMetaPath(pdfPath, "bookmarks.json"); File.WriteAllText(fname, JsonConvert.SerializeObject(marks, Formatting.Indented)); }

        private string GetMetaPath(string pdfPath, string metaFile) { var pdfHash = GetSafeFileName(pdfPath); var folder = Path.Combine(dataFolder, pdfHash); if (!Directory.Exists(folder)) Directory.CreateDirectory(folder); return Path.Combine(folder, metaFile); }
        public void RemoveBookMetadata(string pdfPath) { var folder = Path.Combine(dataFolder, GetSafeFileName(pdfPath)); if (Directory.Exists(folder)) Directory.Delete(folder, true); }
        private string GetSafeFileName(string path) { var h = Math.Abs(path.GetHashCode()).ToString(); return h; }
    }
}
