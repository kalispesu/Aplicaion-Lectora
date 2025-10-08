using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Forms; // for OpenFileDialog
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Markup;
using System.Windows.Resources;
using System.Windows.Controls;

namespace LectorPdf
{
    public partial class MainWindow : Window
    {
        private PdfiumViewer.PdfViewer pdfViewer;
        private ObservableCollection<Book> library = new ObservableCollection<Book>();
        private LibraryManager libManager;
        private Book currentBook;
        private bool highlightMode = false;

        public string CurrentUser { get; private set; }
        private bool darkMode = false;

        public MainWindow(string currentUser)
        {
            InitializeComponent();
            CurrentUser = currentUser;
            this.Title = $"LectorPDF - {currentUser}";

            libManager = new LibraryManager();
            library = new ObservableCollection<Book>(libManager.LoadLibrary());
            lstLibrary.ItemsSource = library;
            lstBookmarks.ItemsSource = new ObservableCollection<BookmarkDisplay>();
            lstAnnotations.ItemsSource = new ObservableCollection<Annotation>();

            pdfViewer = new PdfiumViewer.PdfViewer();
            pdfViewer.ShowToolbar = false;
            var host = winFormsHost as WindowsFormsHost;
            host.Child = pdfViewer;

            pdfViewer.PageChanged += PdfViewer_PageChanged;
            pdfViewer.Renderer.MouseUp += Renderer_MouseUp;

            this.Closing += MainWindow_Closing;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyTheme(false); // default to light theme
        }

        private void ApplyTheme(bool useDark)
        {
            try
            {
                var dict = new ResourceDictionary();
                dict.Source = new Uri("Themes/Styles.xaml", UriKind.Relative);
                // Styles.xaml contains two ResourceDictionaries inside; we load and pick the right one
                var rd = (ResourceDictionary)Application.LoadComponent(new Uri("Themes/Styles.xaml", UriKind.Relative));
                // Try to read the inner dictionaries by key
                if (useDark && rd.Contains("DarkTheme")) {
                    var dark = (ResourceDictionary)rd["DarkTheme"]; // not directly usable
                }
            }
            catch { /* ignore: fallback to default styles */ }

            // Simple approach: swap brushes by setting window background and foreground
            if (useDark)
            {
                this.Background = (System.Windows.Media.Brush)new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255,18,18,18));
                this.Foreground = System.Windows.Media.Brushes.WhiteSmoke;
                darkMode = true;
                btnToggleTheme.Content = "Modo claro";
            }
            else
            {
                this.Background = System.Windows.Media.Brushes.White;
                this.Foreground = System.Windows.Media.Brushes.Black;
                darkMode = false;
                btnToggleTheme.Content = "Modo nocturno";
            }
        }

        private void btnToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            ApplyTheme(!darkMode);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            libManager.SaveLibrary(library.ToList());
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "PDF files|*.pdf";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var path = dlg.FileName;
                var book = new Book { FilePath = path, Title = Path.GetFileNameWithoutExtension(path), LastPage = 1 };
                library.Add(book);
                libManager.SaveLibrary(library.ToList());
            }
        }

        private void lstLibrary_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (lstLibrary.SelectedItem is Book book) OpenBook(book);
        }

        private void OpenBook(Book book)
        {
            try
            {
                currentBook = book;
                var doc = PdfiumViewer.PdfDocument.Load(book.FilePath);
                pdfViewer.Document = doc;
                txtPage.Text = book.LastPage.ToString();
                UpdatePageTexts();

                var anns = libManager.LoadAnnotations(book.FilePath);
                lstAnnotations.ItemsSource = new ObservableCollection<Annotation>(anns);

                var bms = libManager.LoadBookmarks(book.FilePath);
                lstBookmarks.ItemsSource = new ObservableCollection<BookmarkDisplay>(bms.Select(b => new BookmarkDisplay { Page = b.Page, Display = $"Pág {b.Page} - {b.Title}", Source = b }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir PDF: " + ex.Message);
            }
        }

        private void UpdatePageTexts()
        {
            if (pdfViewer?.Document == null) return;
            txtPageCount.Text = $"de {pdfViewer.Document.PageCount}";
        }

        private void PdfViewer_PageChanged(object sender, EventArgs e)
        {
            if (currentBook != null)
            {
                currentBook.LastPage = pdfViewer.Page + 1;
                txtPage.Text = currentBook.LastPage.ToString();
            }
            UpdatePageTexts();
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (pdfViewer.Document == null) return;
            if (pdfViewer.Page > 0) pdfViewer.Page--;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (pdfViewer.Document == null) return;
            if (pdfViewer.Page < pdfViewer.Document.PageCount - 1) pdfViewer.Page++;
        }

        private void txtPage_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (int.TryParse(txtPage.Text, out int p) && pdfViewer.Document != null)
            {
                var pageIndex = Math.Max(0, Math.Min(pdfViewer.Document.PageCount - 1, p - 1));
                pdfViewer.Page = pageIndex;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (currentBook != null)
            {
                libManager.SaveLibrary(library.ToList());
                libManager.SaveAnnotations(currentBook.FilePath, ((IEnumerable<Annotation>)lstAnnotations.ItemsSource)?.ToList() ?? new List<Annotation>());
                MessageBox.Show("Guardado");
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is Book book)
            {
                library.Remove(book);
                libManager.RemoveBookMetadata(book.FilePath);
                libManager.SaveLibrary(library.ToList());
            }
        }

        private void btnAddBookmark_Click(object sender, RoutedEventArgs e)
        {
            if (currentBook == null) return;
            var page = pdfViewer.Page + 1;
            var bm = new Bookmark { Page = page, Title = $"Página {page}" };
            libManager.AddBookmark(currentBook.FilePath, bm);
            lstBookmarks.ItemsSource = new ObservableCollection<BookmarkDisplay>(libManager.LoadBookmarks(currentBook.FilePath).Select(b => new BookmarkDisplay { Page = b.Page, Display = $"Pág {b.Page} - {b.Title}", Source = b }));
        }

        private void btnGoToBookmark_Click(object sender, RoutedEventArgs e)
        {
            if (lstBookmarks.SelectedItem is BookmarkDisplay b) pdfViewer.Page = b.Page - 1;
        }

        private void btnHighlightMode_Click(object sender, RoutedEventArgs e)
        {
            highlightMode = !highlightMode;
            btnHighlightMode.Content = highlightMode ? "Resaltar (ON)" : "Resaltar";
        }

        private void Renderer_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!highlightMode || pdfViewer.Document == null || currentBook == null) return;
            var selection = pdfViewer.Renderer.SelectedText;
            if (!string.IsNullOrWhiteSpace(selection))
            {
                var ann = new Annotation
                {
                    Page = pdfViewer.Page + 1,
                    Text = selection,
                    Comment = "",
                    Id = Guid.NewGuid().ToString()
                };
                var list = ((IEnumerable<Annotation>)lstAnnotations.ItemsSource)?.ToList() ?? new List<Annotation>();
                list.Add(ann);
                lstAnnotations.ItemsSource = new ObservableCollection<Annotation>(list);
                libManager.SaveAnnotations(currentBook.FilePath, list);
            }
        }
    }
}
