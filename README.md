# Aplicaion-Lectora
Agrega y lee tus libros en pdf a gusto
LectorPDF - Distribuible para Windows 7 (x86/x64)

Contenido del ZIP:
- Proyecto fuente completo (archivos .xaml y .cs)
- Themes/Styles.xaml (estilos y modo nocturno)
- WiX/ (scripts Product.wxs y Components.wxs para crear MSI)
- BinFiles/ (coloca aquí tus binarios compilados: LectorPDF.exe, PdfiumViewer.dll, Newtonsoft.Json.dll, pdfium.dll)

Pasos para obtener el instalador MSI:
1) Compila el proyecto en Visual Studio para x86 y x64. Copia los archivos resultantes a BinFiles/ (usa la versión Release).
   - Asegúrate de incluir pdfium.dll x86 en BinFiles (para x86 installer) o distribuir ambas, y ajustar Components.wxs para seleccionar el correcto.
2) Instala WiX Toolset 3.11 y añade su bin (candle.exe, light.exe) al PATH.
3) Edita WiX/Product.wxs y WiX/Components.wxs: sustituye PUT-GUID-HERE por GUIDs reales.
4) Desde WiX folder ejecuta:
   candle.exe Product.wxs Components.wxs
   light.exe Product.wixobj Components.wixobj -o LectorPDF_Setup.msi
5) Firma digitalmente LectorPDF_Setup.msi (opcional) con signtool.

Nota: Los archivos en BinFiles son placeholders en este ZIP. Reemplázalos por tus binarios compilados antes de ejecutar WiX.
