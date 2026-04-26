# StayFinder

StayFinder, Murat Yucedag egitmenliginde M&Y Egitim Akademi 10. Donem FullStack .NET Bootcamp kapsaminda gelistirilen 6. projedir. Proje, "Case 6 - RapidAPI Booking Integration Project" gereksinimlerine uygun olarak veritabani ve admin paneli kullanmadan, verileri RapidAPI uzerinden dinamik olarak ceken bir otel arama ve dashboard uygulamasidir.

## Proje Hakkinda

Bu projede kullanici bir sehir adi, giris tarihi, cikis tarihi ve kisi sayisi girerek otel aramasi yapabilir. Uygulama once RapidAPI uzerindeki Booking API akisi ile destination ID bilgisini alir, daha sonra bu ID uzerinden otel listesini getirir. Kullanici listelenen otellerden birini sectiginde ilgili otelin detay bilgileri yine RapidAPI uzerinden cekilir.

Projedeki tum sayfa icerikleri Ingilizce hazirlanmistir. Case gereksinimi geregi Turkiye odakli sehir veya icerik kullanilmamistir; varsayilan ornek sehir olarak Paris tercih edilmistir.

## Kullanilan Teknolojiler

- ASP.NET Core MVC
- .NET 8
- C#
- Razor View Engine
- HTML, CSS, JavaScript
- RapidAPI

## Sayfalar

### Dashboard

Ana giris sayfasidir. RapidAPI uzerinden alinan farkli veri kaynaklari kart yapisinda gosterilir.

- Hava durumu bilgisi
- Doviz kuru bilgileri
- Yakit fiyatlari
- Kripto para bilgileri
- Guncel haber basliklari
- Gunluk yemek onerisi

### Hotel Listing

Kullanicinin otel aramasi yaptigi sayfadir.

Kullanici inputlari:

- Destination
- Check-in date
- Check-out date
- Guests

Arama yapildiginda uygulama once destination bilgisinden destination ID degerini alir, ardindan otel listesini ceker. Oteller kart yapisinda gosterilir.

Her otel kartinda temel olarak su bilgiler yer alir:

- Hotel name
- Rating
- Short description
- Basic information
- Cover photo
- Price and currency

### Hotel Detail

Listeleme sayfasindan secilen otelin detaylarini gosterir. Otel ID degeri ile Hotel Detail API cagrisi yapilir.

Detay sayfasinda yer alan bilgiler:

- Hotel name
- Description
- Check-in and check-out information
- 2-3 hotel photos
- Basic hotel information
- Available room and facility information when provided by the API

## API Akisi

### 1. Destination ID Alma

Kullanici bir sehir adi girer. Uygulama "Search Hotel Destination" endpoint'ini kullanarak bu sehir icin gelen sonuclari alir ve ilk sonucu kullanir. Bu sonuc icinden destination ID bilgisi elde edilir.

### 2. Otel Listeleme

Destination ID alindiktan sonra "Search Hotels" endpoint'i su parametrelerle cagrilir:

- Destination ID
- Search type
- Check-in date
- Check-out date
- Adult count
- Language: en-us
- Currency: EUR

### 3. Otel Detay

Kullanici bir otele tikladiginda ilgili hotel ID ile "Hotel Detail" endpoint'i cagrilir ve detay verileri sayfaya aktarilir.

## RapidAPI Key Ayari

RapidAPI anahtari su dosyada placeholder olarak tutulur:

`Booking Rapid/Controllers/HomeController.cs`

Dosyadaki asagidaki degeri kendi RapidAPI key degerinizle degistirin:

```csharp
private readonly string rapidApiKey = "RapidApiKey";
```

## Kurulum

Projeyi calistirmak icin:

```bash
dotnet restore
dotnet run --project "Booking Rapid/Booking Rapid.csproj"
```

Uygulama calistiktan sonra tarayicida verilen local adres uzerinden acilabilir.

## Proje Gereksinimlerine Uygunluk

- Veritabani kullanilmamistir.
- Admin paneli bulunmamaktadir.
- Otel verileri RapidAPI uzerinden cekilmektedir.
- Dashboard verileri RapidAPI uzerinden dinamik olarak getirilmektedir.
- Sayfa icerikleri Ingilizce hazirlanmistir.
- Otel listeleme ve otel detay akisi Booking API mantigina gore kurgulanmistir.

## Gelistirici

GitHub: nullablege
