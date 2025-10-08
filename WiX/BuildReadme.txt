WiX build notes:
- Replace PUT-GUID-HERE with real GUIDs (use guidgen.exe or Visual Studio Create GUID tool).
- Place compiled binaries into the BinFiles directory and then run candle & light to build the MSI.
Example:
  candle.exe Product.wxs Components.wxs -dLectorPDF_Bin=../BinFiles
  light.exe Product.wixobj Components.wixobj -o LectorPDF_Setup.msi
