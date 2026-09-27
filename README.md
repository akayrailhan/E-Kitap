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
Copy-Item .env.example .env
docker compose up -d
dotnet restore backend/EBook.sln
dotnet build backend/EBook.sln
Push-Location frontend
npm install
npm run build
Pop-Location
```

Clone eden geliştirici root `.env.example` dosyasını `.env` olarak kopyalayıp local MSSQL parolasını ve Supabase proje değerlerini kendi ortamına göre doldurmalıdır. Root `.env` dosyası Docker Compose ve ASP.NET Core backend tarafından okunur; gerçek secret değerleri Git’e eklenmez. Frontend için Supabase değerleri `frontend/.env.example` dosyasından `frontend/.env.local` dosyasına kopyalanır.

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

Ben bu projeyi geliştirirken yapay zekâ aracından mimari seçenekleri değerlendirmek, başlangıç kodlarını oluşturmak, bazı fonksiyonları geliştirmek, test senaryoları önermek ve dokümantasyon hazırlamak için yararlandım. Buna rağmen Clean Architecture sınırları, kullanılacak kütüphaneler, veri modeli ve iş akışıyla ilgili kararları ben verdim. Üretilen kodu build, migration, çalışma zamanı kontrolleri ve gereksinimler üzerinden denetledim; gerekli gördüğüm yerlerde fonksiyonlar ve mimari üzerinde değişiklikler yaptım. Son uygulama kararlarının, entegrasyon kontrollerinin ve teslim edilen kodun sorumluluğu bana aittir.
