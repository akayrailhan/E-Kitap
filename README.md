# E-Kitap

Aynı etkinliğe ait 10 Word bildirisini tek bir PDF e-kitapta birleştiren full-stack uygulama.

## Teknoloji

- Backend: ASP.NET Core 8 Web API, Clean Architecture, Entity Framework Core, MSSQL
- Frontend: React, TypeScript, Vite
- Authentication: Supabase Auth; API tarafında JWT bearer doğrulaması
- Word okuma: Open XML SDK
- PDF üretimi: QuestPDF Community
- Local development database: Docker SQL Server

## Proje yapısı

```text
backend/
  EBook.sln
  src/
    EBook.Domain/
    EBook.Application/
    EBook.Infrastructure/
    EBook.Api/
frontend/
docker-compose.yml
```

## Geliştirme ortamı

Gereksinimler: .NET 8 SDK veya üzeri, Node.js 20 veya üzeri, npm ve Docker Desktop.

```powershell
docker compose up -d
 dotnet build backend/EBook.sln
 Push-Location frontend
 npm install
 npm run build
 Pop-Location
```

MSSQL connection string ve Supabase ayarları ilerleyen aşamalarda `backend/src/EBook.Api/appsettings.Development.json` veya user secrets ile sağlanacaktır. Secret değerleri repoya eklenmez; `.env.example` dosyaları yalnızca değişken adlarını gösterir.

## Saklama yaklaşımı

İlk sürümde yüklenen orijinal DOCX dosyaları ve üretilen PDF, backend içindeki `wwwroot/uploads` altında kitap bazlı klasörlerde tutulacaktır. Dosya sistemi erişimi Application katmanındaki abstraction üzerinden yapılacak ve Infrastructure katmanındaki local adapter ile uygulanacaktır. Orijinal Word dosyaları değiştirilmeyecek; iletişim bilgileri yalnızca PDF üretim akışındaki metin üzerinde temizlenecektir.

## İşleme akışı

1. Kullanıcı kitap adını ve tam 10 adet `.docx` dosyasını gönderir.
2. Dosyalar yükleme sırasıyla kaydedilir ve `Papers.Order` alanı ile sıralanır.
3. Open XML SDK ile temel paragraf metni okunur.
4. E-posta adresleri ve telefon numaraları sunucu tarafında temizlenir.
5. Bildiri başlığı ilk uygun dolu paragraftan, yoksa dosya adından alınır.
6. QuestPDF tek PDF üretir; içindekiler, gerçek bildiri başlangıç sayfalarını ve tutarlı footer sayfa numaralarını içerir.
7. İşlem başarısız olursa kitap durumu `Failed` olarak saklanır ve kullanıcıya anlaşılır hata döndürülür.

## Kapsam dışı / bilinen sınırlamalar

İlk sürümde Word fontlarının, tablolarının, görsellerinin ve karmaşık biçimlerinin birebir korunması hedeflenmez. Dağıtık kuyruk, bulut dosya depolama, yönetim paneli ve kullanıcı profil tablosu bulunmaz. Supabase yalnızca authentication sağlayıcısıdır; iş verileri MSSQL’de tutulur ve ana iş tabloları `Books` ile `Papers` olacaktır.

## AI kullanımı

Proje geliştirme sürecinde yapay zekâ aracı mimari seçenekleri değerlendirmek, boilerplate oluşturmak, kod yazmak, test senaryoları önermek ve dokümantasyon hazırlamak için kullanılmaktadır. Üretilen kod proje gereksinimleri, build çıktıları ve testlerle doğrulanacaktır. Nihai tasarım kararları ve entegrasyon kontrolleri proje geliştiricisi tarafından gözden geçirilecektir.
