# E-Kitap Oluşturucu (Conference E-Book Generator)

Aynı etkinliğe ait tam 10 adet Word (.docx) bildirisini tek bir PDF e-kitaba dönüştüren, içindekiler tablosunu gerçek sayfa numaralarıyla derleyen, sunucu tarafında iletişim bilgilerini (e-posta ve telefon) temizleyen full-stack kurumsal web uygulaması.

---

## 📑 İçindekiler
- [Teknoloji Yığını](#-teknoloji-yığını)
- [Mimari Tasarım (Clean Architecture)](#-mimari-tasarım-clean-architecture)
- [Veri Modeli (MSSQL & EF Core)](#-veri-modeli-mssql--ef-core)
- [Dosya Saklama Düzeni & Güvenlik](#-dosya-saklama-düzeni--güvenlik)
- [Word ve PDF İşleme Akışı](#-word-ve-pdf-işleme-akışı)
- [İletişim Bilgisi Temizliği ve Testler](#-iletişim-bilgisi-temizliği-ve-testler)
- [API Uç Noktaları (Endpoints)](#-api-uç-noktaları-endpoints)
- [Frontend ve Kullanıcı Deneyimi](#-frontend-ve-kullanıcı-deneyimi)
- [Masaüstü ve Mobil Tasarım Kararları](#-masaüstü-ve-mobil-tasarım-kararları)
- [Kurulum ve Çalıştırma Kılavuzu](#-kurulum-ve-çalıştırma-kılavuzu)
- [Birim Testleri](#-birim-testleri)
- [Bilinen Eksikler ve Sınırlar (Known Limitations)](#-bilinen-eksikler-ve-sınırlar-known-limitations)
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
3. **QuestPDF ile PDF Derleme:**
   - **Kapak Sayfası:** Kitap adı ve alt başlık mizanpajı.
   - **İçindekiler (TOC):** Her bildirinin başlığı ve dinamik bölüm bağlantısı (`SectionLink`).
   - **Gerçek Sayfa Numaraları:** QuestPDF'in `BeginPageNumberOfSection` motoru sayesinde içindekiler tablosundaki her başlık, bildirinin başladığı gerçek sayfa numarasıyla eşleşir.
   - **Tutarlı Numaralandırma:** Tüm sayfalarda yalın ve tutarlı sayfa numarası (`1`, `2`, `3`...) alt bilgi olarak yer alır.

---

## 🧹 İletişim Bilgisi Temizliği ve Testler

Vaka şartnamesine göre; Word içeriklerindeki e-posta adresleri ve telefon numaraları nihai PDF'te **kesinlikle görünmemeli**, ancak metin içindeki diğer tüm akademik bilgiler, yazarlar, kurumlar ve bilimsel analizler **asla bozulmadan aynen korunmalıdır**.

### Temizleme Yaklaşımı (`ContactSanitizer`)
İletişim bilgilerinin ayıklanması sunucu tarafında, yüksek performanslı `.NET 8` kaynak üreticili (`[GeneratedRegex]`) derleme zamanı Regex modelleriyle yürütülür:

1. **E-Posta Temizleme Deseni:**
   RFC 5322 uyumlu e-posta adresi ayrıştırıcı regex:
   ```csharp
   (?<![\w.+-])[\w.!#$%&'*+/=?^`{|}~-]+@[\w](?:[\w-]{0,61}[\w])?(?:\.[\w](?:[\w-]{0,61}[\w])?)+
   ```
2. **Telefon Numarası Temizleme Deseni:**
   Türkiye alan kodlu (`0(5xx)...`, `0312...`, `(0212)...`), uluslararası (`+90...`) ve boşluklu/tireli telefon formatlarını tespit eden regex:
   ```csharp
   (?<!\w)(?:\+?\d[\d\s().-]{7,}\d)(?!\w)
   ```

### Örnek Birim Testleri (`ContactSanitizerTests.cs`)
Temizleme mantığının metin bütünlüğünü bozmadığı xUnit birim testleriyle doğrulanmıştır:

* **Test 1 — İletişim Bilgisini Temizleme & Çevre Metni Koruma:**
  ```csharp
  // Girdi metni:
  "İletişim: author@example.org, +90 (555) 123-45-67. Yıl: 2026"

  // Çıktı metni (E-posta ve telefon kaldırıldı, çevre metin korundu):
  "İletişim: , . Yıl: 2026"
  // Doğrulama: 'author@example.org' ve '555' temizlendi; 'İletişim:' ve 'Yıl: 2026' korundu.
  ```
* **Test 2 — İletişim Bilgisi Olmayan Metinlerin Aynen Korunması:**
  ```csharp
  // Girdi metni:
  "Bu paragraf iletişim bilgisi içermez."

  // Çıktı metni (Metin bütünlüğü bozulmadan korundu):
  "Bu paragraf iletişim bilgisi içermez."
  ```

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

### 📱 Masaüstü ve Mobil Tasarım Kararları

1. **Masaüstü Deneyimi (1180px Çift Sütun Izgarası):**
   - **Giriş ve Düzenleme Aşaması (`.editor-grid`):** Sol sütunda (1.15fr) kitap adı girişi ve sürükle-bırak yükleme alanı yer alırken, sağ sütunda (0.85fr) yüklenen 10 bildirinin sıralama kontrolleri (`▲`, `▼`, `✕`) eşzamanlı izlenir. Kullanıcı iki işlemi tek ekranda kaydırma yapmadan yönetebilir.
   - **PDF Önizleme Aşaması (`.viewer-grid`):** Sol alanda 780px yüksekliğinde gömülü PDF tarayıcı görüntüleyicisi, sağ alanda ise QuestPDF ile üretilen sayfa numaralarına göre oluşturulmuş tıklanabilir İçindekiler Kenar Çubuğu bulunur.

2. **Mobil / Dar Ekran Deneyimi (720px ve 600px Breakpoint'leri):**
   - **Dikey Akış Düzeni:** İki sütunlu ızgara, dar ekranlarda dikey tek sütun akışına (`display: block` / `flex-direction: column`) dönüşür. Sürükle-bırak alanı, dosya listesi ve PDF görüntüleyici ekran genişliğini %100 kaplar.
   - **Dokunmatik Ergonomi (Touch Target):** Sıralama okları, dosya çıkarma butonları ve işlem düğmeleri mobil parmak dokunuşuna uygun olarak minimum 44px dokunma hedefine genişletilmiştir.
   - **Taşma Önleme:** Sağ üstteki kimlik doğrulama/profil alanı mobil ekranda sayfa başlığının üzerine sağa yaslı akış olarak yerleşir; yatay kaydırma çubuğu oluşması (horizontal scroll) engellenir.
   - **PDF Görüntüleyici Yüksekliği:** Mobil cihazlarda dikey alanı kilitlememek için iframe yüksekliği 420px-480px bandına çekilmiş, "Yeni Sekmede Aç" ve "PDF İndir" butonları mobil kullanıcılar için tam genişlikte sunulmuştur.

3. **Tipografi ve Editoryal Görsel Dil:**
   - Kitap ve yayıncılık temasını yansıtan klasik Georgia serif başlıklar ile modern, okunabilir sans-serif (`Trebuchet MS`, `Segoe UI`) gövde metinleri dengelenmiştir.
   - Okuma yorgunluğunu önleyen yumuşak kağıt dokusu (`--paper: #f7f4ee`), editoryal mürekkep rengi (`--ink: #24201d`) ve yayıncı kiremit vurgusu (`--accent: #c65b2e`) kullanılmıştır.

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

Generation servisi durum geçişleri, 10 bildiri validasyonu, iletişim bilgisi temizliği ve hata yakalama mekanizmaları xUnit ile test edilmiştir:

```bash
dotnet test backend/EBook.sln
```
* **Kapsam:** 8/8 Birim Testi Başarılı (Status geçişleri, retry mekanizması, 10 dosya kuralı doğrulaması, ContactSanitizer e-posta/telefon temizliği).

---

## ⚠️ Bilinen Eksikler ve Sınırlar (Known Limitations)

1. **Word Biçim Sadakati (Kapsam Dışı):**
   Vaka şartnamesinde açıkça ifade edildiği üzere *"kusursuz Word biçim sadakati beklenmez"*. Bu doğrultuda Word belgelerindeki karmaşık tablolar, çizimler, matematiksel formül nesneleri veya gömülü vektör görseller PDF'e aktarılmaz; yalnızca ana metin gövdesi, başlık hiyerarşisi ve temel paragraf yapısı alınarak QuestPDF standardında temiz bir editoryal mizanpaja dönüştürülür.
2. **Dağıtık Kuyruk Mimarisi (Distributed Queue):**
   Gereksinimler kapsamında bulut tabanlı kuyruk servisi (RabbitMQ, Kafka, AWS SQS) zorunlu tutulmadığı için PDF üretim iş parçacığı ASP.NET Core `IServiceScopeFactory` ve arka plan `Task.Run` asenkron işleyicisiyle yürütülmektedir. Çok yüksek eşzamanlı istek alan kurumsal ortamlarda Hangfire veya RabbitMQ gibi harici bir kuyruk sistemi entegre edilebilir.
3. **Bulut Depolama (Cloud Storage):**
   Dosya saklama yaklaşımı olarak kolaylık ve taşınabilirlik açısından `wwwroot/uploads` tercih edilmiştir. Çok sunuculu (load-balanced) ortamlarda `IFileStorage` arayüzünün AWS S3 veya Azure Blob Storage adaptörüne genişletilmesi gerekir.
4. **E-Posta Doğrulama & Supabase Limitleri:**
   Kullanıcı giriş sistemi vaka için zorunlu olmayıp artı özellik olarak entegre edilmiştir. Supabase ücretsiz katmanında saatlik doğrulama e-postası kotası bulunduğundan, değerlendirme sürecini kolaylaştırmak için sistem varsayılan olarak kesintisiz **Misafir Modu**nda çalışır ve hazır demo hesabı (`test@example.com` / `123456`) sağlanmıştır.

---

## 🤖 Yapay Zekâ (AI) Kullanım Beyanı

Bu projeyi geliştirirken mimari alternatifleri değerlendirmek, başlangıç şablonlarını oluşturmak, birim test senaryoları tasarlamak ve dokümantasyon hazırlamak amacıyla yapay zekâ asistanından yararlanılmıştır.

Buna rağmen; Clean Architecture sınırları, Entity Framework Core veri modeli tasarımı, OpenXML ve QuestPDF entegrasyon kararları, hata ayıklama süreçleri ve nihai kod kontrolleri şahsım tarafından yürütülmüş ve doğrulanmıştır. Teslim edilen çalışmanın tüm mimari ve işlevsel sorumluluğu bana aittir.
