LectorPDF - Proyecto WPF (base)
--------------------------------
Archivos incluidos:
- App.xaml
- LoginWindow.xaml / LoginWindow.xaml.cs
- UserManager.cs
- MainWindow.xaml / MainWindow.xaml.cs
- Models.cs
- LibraryManager.cs

Requisitos:
- Visual Studio 2015+ (WPF .NET Framework 4.5)
- NuGet packages: PdfiumViewer, Newtonsoft.Json
- Incluir pdfium.dll nativo para x86 y x64 cuando empaquetes

Cómo compilar:
1) Crear nuevo proyecto WPF en Visual Studio (.NET Framework 4.5)
2) Reemplazar los archivos generados por los de este proyecto
3) Instalar los paquetes NuGet mencionados
4) Compilar para AnyCPU o para x86/x64 y distribuir las dll de pdfium correspondientes
