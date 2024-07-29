# .Net MAUI QR Code Scanner

Bu proje, QR kodlarını tarayabilen ve taranan koda göre farklı sesler oynatabilen bir mobil uygulamadır. Uygulama, QR kodu içeriğine göre belirli ses dosyalarını çalar ve kullanıcıya QR kodun formatı ve değeri hakkında bilgi verir.

## Teknolojiler ve Kütüphaneler:

- **.NET MAUI:** Uygulamanın temel çerçevesi olarak kullanılmıştır. Mobil ve masaüstü uygulamaları geliştirmek için bir çerçevedir.
- **ZXing.Net.Maui:** QR kodları ve diğer barkod türlerini taramak için kullanılan kütüphanedir. Bu kütüphane, çeşitli barkod formatlarını destekler.
- **IAudioManager:** Ses dosyalarını çalmak için kullanılan kütüphanedir. Bu proje, ses dosyalarını uygulama paketinden yükler ve oynatır.

## Kurulum:

1) **.NET MAUI Projesi Oluşturun:**

2) **Gerekli Kütüphaneleri Yükleyin:**
   - [ZXing.Net.Maui](https://github.com/Redth/ZXing.Net.Maui) ve [IAudioManager](https://github.com/jfversluis/Plugin.Maui.Audio) kütüphanelerini NuGet üzerinden yükleyin.
```bash
dotnet add package ZXing.Net.Maui
dotnet add package IAudioManager
```

3) **Kaynak Dosyalarını Ekleyin:**
- Resources/Images klasörüne qr1.png ve qr2.png görsellerini ekleyin.
- Resources/Raq klasörüne sound_one.mp3, sound_two.mp3 ve default.mp3 ses dosyalarını ekleyin.

4) **Kod Yapılandırması:**
- MainPage.xaml ve MainPage.xaml.cs dosyalarını aşağıdaki gibi düzenleyin.

## Kullanım: 
1) **QR Kod Tarama:**
- Uygulama açıldığında, kamera ekranında QR kodları taramaya başlar.
- Tarama sonucu, QR kodun formatı ve değeri kullanıcıya gösterilir.

2) **Sesli Geri Bildirim:**
- Belirli QR kod değerlerine göre ("sound_one" ve "sound_two") ilgili ses dosyaları çalınır.
- QR kod değeri bilinmeyenler için varsayılan ses dosyası çalınır.

3) **Kamera Yönlendirme:**
- Kullanıcı, kamera yönünü (ön/arka) değiştirebilir.

## Kamera Erişimi İçin Gerekli İzinler:

- **AndroidManifest.xml:**
  
  ```xml
  <uses-permission android:name="android.permission.CAMERA" />
  ```
  
  - **Info.plist:**
   
  ```xml
  <key>NSCameraUsageDescription</key>
  <string>This app needs access to the camera to take photos.</string>
  ```

## MauiProgram.cs
```csharp
   builder.Services.AddSingleton(AudioManager.Current);
   builder.Services.AddTransient<MainPage>();
```

## MainPage.xaml.cs:

```csharp
private readonly IAudioManager audioManager;

public MainPage(IAudioManager audioManager)
{
    InitializeComponent();

    // BarcodeReaderOptions, barkod tarayıcı ayarlarını yapılandırmak için kullanılır
    barcodeReader.Options = new BarcodeReaderOptions
    {
        // Barkod formatlarını ayarlar. Tüm formatları kullanmak için BarcodeFormats.All kullanılır.
        Formats = ZXing.Net.Maui.BarcodeFormats.All, //Tüm formatları kullan
        //ZXing.Net.Maui.BarcodeFormat.Code128, veya sadece belirli formatı kullan

        AutoRotate = true, // Barkodları otomatik olarak döndürmeye izin verir
        Multiple = true,  // Birden fazla barkodun aynı anda okunmasına izin verir
    };


    this.audioManager = audioManager;
}

private void CopyClicked(object sender, EventArgs e)
{
    if (!string.IsNullOrEmpty(QRDeger.Text))
    {
        //Metni panoya kopyalar
        Clipboard.SetTextAsync(QRDeger.Text);
    }
}
private void DondurClicked(object sender, EventArgs e)
{
    // Mevcut kamera konumunu al
    var currentLocation = barcodeReader.CameraLocation;

    // Kamera konumunu değiştir
    // Eğer mevcut konum arka kamera ise, ön kamera olarak değiştirir veya tersine döndür
    barcodeReader.CameraLocation = currentLocation == CameraLocation.Rear
        ? CameraLocation.Front
        : CameraLocation.Rear;
}

// Barkodlar okunduğunda:
private async void barcodeReader_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
{
    // Ses dosyalarını yüklemek için audioManager'ı kullanarak ses oynatıcılar oluşturur
    var player1 = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("sound_one.mp3"));
    var player2 = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("sound_two.mp3"));
    var player3 = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("default.mp3"));

    // Her bir barkod sonucunu işler
    foreach (var barcode in e.Results)
    {
        // Eğer barkod null ise, kullanıcıya hata mesajı gösterir
        if (barcode is null)
        {
            await DisplayAlert("Hata", "Barkod Okunamadı", "Tamam");
            return;
        }
        else
        {     // Barkod okunduysa, UI üzerinde gösterir
            Dispatcher.DispatchAsync(async () =>
            {                        
                // Barkod formatı ve değerini gösterir
                QRDeger.Text = $"Format: {barcode.Format} -> {barcode.Value}";

                // Barkodun değerine göre uygun sesi çalar
                switch (barcode.Value)
                {
                    case "sound_one": // QR kodu "sound_one" değerine sahipse
                        player1.Play(); // sound_one.mp3 dosyasını oynatır
                        break;
                    case "sound_two\n": // QR kodu "sound_two" değerine sahipse
                        player2.Play(); // sound_two.mp3 dosyasını oynatır
                        break;
                    default: 
                        // QR kodu "sound_one" veya "sound_two" değerlerinden farklıysa
                        player3.Play(); // default.mp3 dosyasını oynatır
                        break;
                }
            });
        }
    }
}
```
## MainPage.xaml: 

```xaml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="QrCode.MainPage"
           xmlns:zxing="clr-namespace:ZXing.Net.Maui.Controls;assembly=ZXing.Net.MAUI.Controls"
             Title="QR - Barcode Scanner"
             BackgroundImageSource="bg.jpg">

   
        <VerticalStackLayout Padding="30,30" Margin="20" HorizontalOptions="Center" VerticalOptions="Center">

            <VerticalStackLayout Padding="60" Margin="0,0,0,50">
                <Label
      x:Name="QRDeger"
      Style="{StaticResource SubHeadline}"
      TextColor="{DynamicResource White}"
      HorizontalTextAlignment="Center"
      VerticalTextAlignment="Center"
      FontSize="Medium" />
            </VerticalStackLayout>

            <VerticalStackLayout WidthRequest="300" HeightRequest="200">
                <zxing:CameraBarcodeReaderView
                x:Name="barcodeReader"
                BarcodesDetected="barcodeReader_BarcodesDetected" 
                WidthRequest="300" 
                HeightRequest="200"
                HorizontalOptions="StartAndExpand" 
                VerticalOptions="StartAndExpand">

                </zxing:CameraBarcodeReaderView>
            </VerticalStackLayout>

            <VerticalStackLayout Padding="110" WidthRequest="500" Spacing="20">
                <Button 
        x:Name="CopyBtn"
        Text="Taranan Değeri Kopyala"
        Clicked="CopyClicked"
        HorizontalOptions="Fill"
        VerticalOptions="End"
        BackgroundColor="{DynamicResource PrimaryColor}"
        TextColor="White" />

                <Button 
        Clicked="DondurClicked"
        Text="Kamerayı Döndür"
        HorizontalOptions="Fill"
        VerticalOptions="End"
        Margin="0,0,0,100" />
            </VerticalStackLayout>

        </VerticalStackLayout>
</ContentPage>
```

## Android Ekran Görüntüsü:
![alt text](https://github.com/abdullaheroll/Net-MAUI-QR-Code-Scanner/blob/main/AndroidPrt.png)


## Windows Ekran Görüntüsü:
![alt text](https://github.com/abdullaheroll/Net-MAUI-QR-Code-Scanner/blob/main/windowsPrt.png)
