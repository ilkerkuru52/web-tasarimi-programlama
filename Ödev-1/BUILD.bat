@echo off
chcp 65001 >nul
title KuraBird - Derleme

echo.
echo ========================================
echo   KuraBird - Derleme ve Yayinlama
echo ========================================
echo.

:: .NET SDK yüklü mü kontrol et
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [HATA] .NET SDK bulunamadi! Lutfen https://dotnet.microsoft.com/download adresinden indirin.
    pause
    exit /b 1
)

echo [1/4] Bagimliliklar yukleniyor...
dotnet restore KuraBird\KuraBird.csproj
dotnet restore KuraBirdInstaller\KuraBirdInstaller.csproj
echo.

echo [2/4] KuraBird oyunu derleniyor...
dotnet publish KuraBird\KuraBird.csproj -c Release -r win-x64 --self-contained true ^
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true ^
    -o publish\
if errorlevel 1 goto hata
echo.

echo [3/4] KuraBirdInstaller derleniyor...
dotnet publish KuraBirdInstaller\KuraBirdInstaller.csproj -c Release -r win-x64 --self-contained true ^
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true ^
    -o publish\
if errorlevel 1 goto hata
echo.

echo [4/4] Kontrol...
if exist publish\KuraBird.exe (
    echo [OK] publish\KuraBird.exe olusturuldu!
) else (
    echo [HATA] KuraBird.exe bulunamadi!
)
if exist publish\KuraBirdInstaller.exe (
    echo [OK] publish\KuraBirdInstaller.exe olusturuldu!
)

echo.
echo ========================================
echo  Sonraki Adim: Setup Paketi Olusturma
echo ========================================
echo.
echo Eger Inno Setup yuklu ise asagidaki komutu calistirin:
echo   "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" Installer\setup.iss
echo.
echo Ya da Inno Setup IDE'sinde Installer\setup.iss dosyasini acin
echo ve Build > Compile tusuna basin.
echo.
echo Cikti: Installer\KuraBirdSetup.exe
echo.
goto son

:hata
echo.
echo [HATA] Derleme basarisiz!
echo Hata detaylari icin yukaridaki mesajlari inceleyin.

:son
pause
