using Plugin.Maui.Audio;
using System.Diagnostics;
using ZXing;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;


namespace QrCode
{
    public partial class MainPage : ContentPage
    {
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
    }


}
