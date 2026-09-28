# E-Kitap Oluşturucu (Conference E-Book Generator)

Aynı etkinliğe ait tam 10 adet Word (.docx) bildirisini tek bir PDF e-kitaba dönüştüren, içindekiler tablosunu gerçek sayfa numaralarıyla derleyen, sunucu tarafında iletişim bilgilerini (e-posta ve telefon) temizleyen full-stack kurumsal web uygulaması.

---

## 📑 İçindekiler
- [Teknoloji Yığını](#-teknoloji-yığını)
- [Mimari Tasarım (Clean Architecture)](#-mimari-tasarım-clean-architecture)
- [Veri Modeli (MSSQL & EF Core)](#-veri-modeli-mssql--ef-core)
- [Dosya Saklama Düzeni & Güvenlik](#-dosya-saklama-düzeni--güvenlik)
- [Word ve PDF İşleme Akışı](#-word-ve-pdf-işleme-akışı)
- [API Uç Noktaları (Endpoints)](#-api-uç-noktaları-endpoints)
- [Frontend ve Kullanıcı Deneyimi](#-frontend-ve-kullanıcı-deneyimi)
- [Kurulum ve Çalıştırma Kılavuzu](#-kurulum-ve-çalıştırma-kılavuzu)
- [Birim Testleri](#-birim-testleri)
- [Yapay Zekâ (AI) Kullanım Beyanı](#-yapay-zekâ-ai-kullanım-beyanı)

---

## 🛠 Teknoloji Yığını

| Katman | Teknoloji | Açıklama |
| :--- | :--- | :--- |
| **Backend** | ASP.NET Core 8 Web API | C# .NET 8, Dependency Injection, RESTful API |
| **Mimari** | Clean Architecture | Domain, Application, Infrastructure, API katmanları |
| **Veritabanı** | MSSQL Server 2022 & EF Core 8 | Code-First Migration, Fluent API, Docker Container |
| **Word İşleme** | DocumentFormat.OpenXml (Open XML SDK) | Orijinal dosyalara zarar vermeden metin ve paragraf çıkarma |
| **PDF Motoru** | QuestPDF Community | Kapak, dinamik sayfa numaralı İçindekiler (TOC) ve mizanpaj |
| **Frontend** | React 18, TypeScript, Vite | Modüler mimari, responsive (mobil & masaüstü) arayüz |
| **Kimlik Doğrulama** | Supabase Auth (Opsiyonel Bonus) | JWT Bearer desteği, misafir (guest) modu ve demo hesabı |
| **Test** | xUnit, Moq, FluentAssertions | Generation servisi durum geçişleri ve doğrulama testleri |

---

## 🏗 Mimari Tasarım (Clean Architecture)

Proje, iş mantığını dış bağımlılıklardan (veritabanı, dosya sistemi, harici kütüphaneler) izole eden **Clean Architecture** prensibiyle inşa edilmiştir:

```text
backend/
├── src/
│   ├── EBook.Domain/               # Bağımsız çekirdek: Varlıklar (Book, Paper), Enum'lar
│   ├── EBook.Application/          # Arayüzler (Abstractions), Servisler (Upload, Status, Generation)
│   ├── EBook.Infrastructure/       # Dış bağımlılıklar: EF Core, OpenXML, QuestPDF, LocalFileStorage
│   └── EBook.Api/                  # Giriş noktası: Controllers, Startup DI, Otomatik Migration
└── tests/
    └── EBook.Infrastructure.Tests/ # Birim testleri
frontend/                           # Modüler React + TypeScript SPA
docker-compose.yml                  # Yerel MSSQL veritabanı konteyneri
```

---

## 🗄 Veri Modeli (MSSQL & EF Core)

Vaka kuralı doğrultusunda **yalnızca iki ana tablo** tasarlanmış ve aralarındaki 1-N (One-to-Many) ilişki Fluent API ile yapılandırılmıştır:

### 1. `Books` Tablosu
| Kolon | Tip | Açıklama |
| :--- | :--- | :--- |
| `Id` | `UNIQUEIDENTIFIER` (PK) | Kitabın benzersiz kimliği (`Guid.NewGuid()`) |
| `Name` | `NVARCHAR(200)` | Kitabın başlığı |
| `OwnerUserId`| `NVARCHAR(200)` | Kullanıcı kimliği (Giriş yapanın Supabase `sub` UUID'si; misafir için `"guest-anonymous"`) |
| `Status` | `NVARCHAR(20)` | Durum takibi: `Draft`, `Processing`, `Completed`, `Failed` |
| `PdfPath` | `NVARCHAR(500)` | Üretilen nihai PDF dosyasının göreceli yolu |
| `CreatedAt` | `DATETIMEOFFSET` | Oluşturulma tarihi |
| `CompletedAt`| `DATETIMEOFFSET` | PDF üretiminin bittiği tarih |
| `ErrorMessage`| `NVARCHAR(2000)`| Üretim başarısız olursa kaydedilen hata detayı |

### 2. `Papers` Tablosu
| Kolon | Tip | Açıklama |
| :--- | :--- | :--- |
| `Id` | `UNIQUEIDENTIFIER` (PK) | Bildirinin benzersiz kimliği |
| `BookId` | `UNIQUEIDENTIFIER` (FK) | Bağlı olduğu kitabın kimliği (Cascade Delete) |
| `OriginalFileName` | `NVARCHAR(255)` | Yüklenen orijinal `.docx` dosya adı |
| `StoredFilePath` | `NVARCHAR(500)` | Sunucuda saklandığı göreceli dosya yolu |
| `Title` | `NVARCHAR(500)` | DOCX'ten çıkarılan veya dosya adından türetilen bildiri başlığı |
| `Order` | `INT` | 1'den 10'a kadar derleme ve içindekiler sırası |
| `CreatedAt` | `DATETIMEOFFSET` | Yükleme tarihi |

> **Otomatik Migration:** Uygulama ayağa kalktığında (`Program.cs`) bekleyen migration'lar otomatik olarak MSSQL veritabanına uygulanır.

---

## 📂 Dosya Saklama Düzeni & Güvenlik

Case dokümanında belirtildiği üzere dosya saklama yaklaşımı olarak kolaylık ve taşınabilirlik sağlayan `wwwroot/uploads` mimarisi seçilmiştir:

```text
backend/src/EBook.Api/wwwroot/uploads/
└── {BookId}/                        # Kitaba özel izole klasör (GUID N formatında)
    ├── 01-Bildiri_Adi.docx          # Sıra numarasıyla adlandırılmış orijinal Word dosyaları
    ├── 02-Bildiri_Adi.docx
    ├── ...
    └── ebook.pdf                    # Üretilen nihai e-kitap PDF'i
```

### Neden Bu Yaklaşım Seçildi?
1. **İzolasyon & Çakışma Önleme:** Her kitap benzersiz bir `Guid` klasöründe tutulduğundan, farklı kullanıcıların veya kitapların aynı isimli dosyaları birbirinin üzerine yazılmaz.
2. **Sıra Bütünlüğü:** Dosyalar diske `{order:D2}-{safeFileName}` formatında yazılarak fiziksel diskte de derleme sırası garanti altına alınır.
3. **Orijinal Dosya Sadakati:** Kullanıcının yüklediği orijinal `.docx` dosyaları **kesinlikle değiştirilmez**. İletişim bilgisi temizliği yalnızca bellek üzerindeki veri akışında yapılır.
4. **Temizlik & Silme Kolaylığı:** Bir kitap silindiğinde veya üretim başarısız olduğunda, yalnızca ilgili `BookId` klasörü kaldırılarak artık dosya kalması engellenir.

---

## ⚙️ Word ve PDF İşleme Akışı

1. **Yükleme Doğrulaması:**
   - Kitap adı zorunludur.
   - Tam olarak **10 adet** dosya seçilmelidir.
   - Dosyaların tamamı `.docx` formatında ve her biri en fazla 10 MB olmalıdır.
2. **Metin Ayrıştırma (OpenXML):**
   - Word dosyası açılarak gövde paragrafları (`w:p`) ve metin düğümleri (`w:t`) okunur.
   - Bildiri başlığı: İlk anlamlı paragraf 200 karakterden kısaysa başlık kabul edilir; aksi halde dosya adı başlık olarak atanır.
3. **Sunucu Taraflı İletişim Bilgisi Temizliği (`ContactSanitizer`):**
   - E-posta adresleri RFC 5322 uyumlu regex ile tespit edilir ve metinden arındırılır.
   - Türkiye ve uluslararası telefon numarası formatları (`+90...`, `0(5xx)...`, `(0312)...`, vb.) temizlenir.
   - Diğer akademik içerik, yazar adları ve metin yapısı aynen korunur.
4. **QuestPDF ile PDF Derleme:**
   - **Kapak Sayfası:** Kitap adı ve alt başlık mizanpajı.
   - **İçindekiler (TOC):** Her bildirinin başlığı ve dinamik bölüm bağlantısı (`SectionLink`).
   - **Gerçek Sayfa Numaraları:** QuestPDF'in `BeginPageNumberOfSection` motoru sayesinde içindekiler tablosundaki her başlık, bildirinin başladığı gerçek sayfa numarasıyla eşleşir.
   - **Tutarlı Numaralandırma:** Tüm sayfalarda yalın ve tutarlı sayfa numarası (`1`, `2`, `3`...) alt bilgi olarak yer alır.

---

## 🔌 API Uç Noktaları (Endpoints)

| Metot | Uç Nokta | Açıklama | Başarılı Yanıt |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/books` | Kitap adı ve 10 `.docx` dosyasını yükler, taslak kaydı açar. | `201 Created` |
| `POST` | `/api/books/{id}/create` | PDF üretim sürecini başlatır (Arka planda veya eşzamanlı). | `202 Accepted` |
| `GET` | `/api/books/{id}` | Kitabın güncel durumunu (`Status`, `Papers`, `ErrorMessage`) döner. | `200 OK` |
| `GET` | `/api/books` | Giriş yapmış kullanıcının oluşturduğu kitap geçmişini listeler. | `200 OK` |
| `GET` | `/api/books/{id}/pdf` | Üretilen PDF'i tarayıcıda önizlemek için akış (stream) olarak döner. | `200 OK (application/pdf)` |
| `GET` | `/api/books/{id}/pdf?download=true` | PDF'i doğrudan bilgisayara dosya eki olarak indirir. | `200 OK (attachment)` |

---

## 💻 Frontend ve Kullanıcı Deneyimi

React, TypeScript ve Vite ile geliştirilen kullanıcı arayüzü; masaüstü, tablet ve mobil cihazlar için optimize edilmiştir:

- **Etkileşimli Yükleme:** Sürükle-bırak (Drag & Drop) ve dosya seçici.
- **Sıralama Kontrolleri:** Yüklenen 10 bildiriyi yukarı (`▲`), aşağı (`▼`) taşıma ve listeden çıkarma (`✕`).
- **Canlı Durum Takibi (Loading Experience):**
  - Hareketli yükleme animasyonu (spinner) ve yüzde ilerleme çubuğu (%0 - %100).
  - 5 aşamalı görsel durum kontrol listesi (Sunucuya aktarım, DOCX okuma, İletişim temizliği, İçindekiler eşleme, PDF derleme).
- **Entegre PDF Görüntüleyici:**
  - Tarayıcı içi gömülü `iframe` ile indirmeden doğrudan okuma.
  - Tek tıkla yerel cihaza PDF indirme ve yeni sekmede açma.
  - Yan panelde tıklanabilir İçindekiler Listesi.
- **📚 Kitaplarım & Kullanıcı İzolasyonu:**
  - Giriş yapan kullanıcılar, sadece kendilerinin oluşturduğu e-kitapları geçmiş modalında listeleyebilir, durumlarını inceleyebilir ve tek tıkla yeniden açabilir.
  - Misafir kullanıcıların veya başka kullanıcıların oluşturduğu kitaplar gizli tutulur.
- **Kimlik Doğrulama & Misafir Modu:**
  - Case kuralları gereği kimlik doğrulama zorunlu değildir; varsayılan olarak "Misafir Modu"nda tüm özellikler eksiksiz çalışır.
  - İsteğe bağlı olarak Supabase ile kayıt/giriş yapılabilir (`test@example.com` / `123456` hazır test hesabı entegredir).
- **Hata Yönetimi:** Anlaşılır hata mesajları, tek tıkla yeniden deneme (`Retry`) ve yeni kitap başlatma desteği.

---

## 🚀 Kurulum ve Çalıştırma Kılavuzu

### Ön Gereksinimler
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) ve npm
- [Docker Desktop](https://www.docker.com/) (MSSQL Server için)

### 1. Depoyu Klonlayın
```bash
git clone https://github.com/akayrailhan/E-Kitap.git
cd E-Kitap
```

### 2. Ortam Değişkenlerini (.env) Hazırlayın
Proje kök dizininde ve `frontend/` klasöründe hazır şablon `.env.example` dosyaları bulunmaktadır. Güvenlik amacıyla bu dosyalarda gerçek şifre yer almaz; değer alanlarındaki yönlendirmelere göre kendi değerlerinizi girmelisiniz:

**a) Kök Dizin / Backend Yapılandırması:**
Kök dizindeki `.env.example` dosyasını `.env` olarak kopyalayın:
```bash
# Windows PowerShell:
Copy-Item .env.example .env

# Bash / macOS / Linux:
cp .env.example .env
```
Ardından oluşturulan `.env` dosyasını açıp değerleri düzenleyin:
- `MSSQL_SA_PASSWORD`: `BURAYA_GUCLU_MSSQL_SIFRENIZI_YAZIN` yerine kendi belirleyeceğiniz güçlü bir şifre yazın (SQL Server en az 8 karakter, büyük harf, küçük harf, rakam ve sembol gerektirir; örn: `GucluSifre!2026`).
- `ConnectionStrings__DefaultConnection`: `Password=` kısmına da aynı belirlediğiniz şifreyi yazın.
- `Supabase__...`: Supabase kullanmayacaksanız bu satırları olduğu gibi bırakabilirsiniz.

**b) Frontend Yapılandırması (İsteğe Bağlı - Supabase Auth):**
```bash
# Windows PowerShell:
Copy-Item frontend/.env.example frontend/.env

# Bash / macOS / Linux:
cp frontend/.env.example frontend/.env
```
- Supabase kullanmak isterseniz `BURAYA_SUPABASE_PROJECT_ID_YAZIN` ve `BURAYA_SUPABASE_ANON_PUBLIC_KEY_YAZIN` alanlarına kendi Supabase panelinizden aldığınız değerleri yazın.
- Supabase tanımlanmazsa uygulama hiçbir hata vermeden doğrudan **Misafir Modu**nda çalışmaya devam eder.

### 3. MSSQL Veritabanını Başlatın
```bash
docker compose up -d
```
*(MSSQL konteyneri `localhost:1433` portunda ayağa kalkar. Konteynerin ilk açılışta veritabanı motorunu hazır hale getirmesi yaklaşık 10-15 saniye sürer).*

### 4. Backend'i Çalıştırın
```bash
dotnet run --project backend/src/EBook.Api/EBook.Api.csproj --launch-profile http
```
> **Not:** Uygulama açılışında veritabanı migration'ları otomatik olarak uygulanır (`EBookDbContext.Database.MigrateAsync`). Swagger arayüzüne `http://localhost:5290/swagger` adresinden erişebilirsiniz.

### 5. Frontend'i Çalıştırın
Yeni bir terminal açarak:
```bash
cd frontend
npm install
npm run dev
```
Tarayıcınızda **`http://localhost:5173`** adresini açarak uygulamayı kullanmaya başlayabilirsiniz.

> 💡 **Hızlı Test / Demo Hesabı:**
> - Misafir olarak doğrudan kitap adı girip 10 adet `.docx` dosyası yükleyebilir ve PDF üretebilirsiniz.
> - Kullanıcı girişini ve "Kitaplarım" geçmişi deneyimini test etmek için üstteki **"Giriş Yap / Kayıt Ol"** butonuna basıp **"Demo Bilgilerini Doldur"** butonuna tıklayabilir (`test@example.com` / `123456`) veya kendi hesabınızı oluşturabilirsiniz.

---

## 🧪 Birim Testleri

Generation servisi durum geçişleri, 10 bildiri validasyonu ve hata yakalama mekanizmaları xUnit ile test edilmiştir:

```bash
dotnet test backend/EBook.sln
```
* **Kapsam:** 8/8 Birim Testi Başarılı (Status geçişleri, retry mekanizması, 10 dosya kuralı doğrulaması).

---

## 🤖 Yapay Zekâ (AI) Kullanım Beyanı

Bu projeyi geliştirirken mimari alternatifleri değerlendirmek, başlangıç şablonlarını oluşturmak, birim test senaryoları tasarlamak ve dokümantasyon hazırlamak amacıyla yapay zekâ asistanından yararlanılmıştır.

Buna rağmen; Clean Architecture sınırları, Entity Framework Core veri modeli tasarımı, OpenXML ve QuestPDF entegrasyon kararları, hata ayıklama süreçleri ve nihai kod kontrolleri şahsım tarafından yürütülmüş ve doğrulanmıştır. Teslim edilen çalışmanın tüm mimari ve işlevsel sorumluluğu bana aittir.
