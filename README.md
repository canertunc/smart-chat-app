# 🤖 RAG-Based Chatbot

PDF belgelerinizden akıllı sorular sormanızı sağlayan gelişmiş bir chatbot uygulaması. Retrieval-Augmented Generation (RAG) teknolojisi kullanarak, yüklediğiniz PDF dosyalarının içeriğinden doğru ve alakalı cevaplar üretir. Ayrıca PostgreSQL veritabanındaki statik soru-cevap çiftleri ile hızlı yanıtlar.

## ✨ Özellikler

- **PDF Dosya Yükleme**: PDF belgelerinizi kolayca yükleyin
- **Akıllı Metin Analizi**: Belgeleri otomatik olarak analiz eder ve parçalar
- **AI-Powered Responses**: TinyLlama modeli ile doğal dil cevapları
- **Semantic Search**: En alakalı bilgileri bulur ve sunar
- **Statik Soru-Cevap**: PostgreSQL veritabanındaki önceden tanımlanmış sorulara anında cevap
- **Statik Soru-Cevap Yönetimi**: PostgreSQL veritabanındaki soru-cevap çiftlerini görüntüleme, ekleme ve silme işlemleri
- **Northwind Database**: Entity Framework ile veri çekme/gönderme işlemleri
- **Modern Web Arayüzü**: Responsive ve kullanıcı dostu tasarım

## 🛠️ Teknolojiler

### Backend
- **ASP.NET Core 8.0** - Web framework
- **Entity Framework Core** - ORM
- **PostgreSQL** - Veritabanı
- **iText7** - PDF işleme

### AI & ML
- **Python** - Machine Learning işlemleri
- **Sentence Transformers** - Text embedding (e5-base-v2 modeli)
- **TinyLlama** - Language model
- **PyTorch** - Deep learning framework

### Frontend
- **Bootstrap 5** - UI framework
- **JavaScript** - İnteraktif özellikler
- **Font Awesome** - İkonlar

## 🚀 Kurulum

### Gereksinimler
- .NET 8.0 SDK
- Python 3.8+
- PostgreSQL 13+

### 1. Projeyi Klonlayın
```bash
git clone https://github.com/canertunc/rag-chat-net.git
```

### 2. Python Sanal Ortamı Kurun
```bash
cd Helpers
python -m venv venv
venv\Scripts\activate  # Windows
# source venv/bin/activate  # Linux/Mac
pip install -r requirements.txt
```

### 3. Veritabanını Yapılandırın
`appsettings.json` dosyasında PostgreSQL bağlantı dizginizi güncelleyin:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ragchatbot;Username=your_username;Password=your_password"
  }
}
```

### 4. Veritabanı Migration'ları Çalıştırın
```bash
dotnet ef database update
```

### 5. (Opsiyonel) Statik Soru-Cevap Verilerini Ekleyin
Statik soru-cevap çiftlerini doğrudan PostgreSQL veritabanına ekleyebilirsiniz:

**A) SQL ile Doğrudan Ekleme:**
```sql
INSERT INTO "ChatMessages" ("QuestionMessage", "AnswerMessage") VALUES 
('Merhaba', 'Merhaba! Size nasıl yardımcı olabilirim?'),
('Nasılsın', 'Teşekkürler, iyiyim. Siz nasılsınız?'),
('Bu sistem nasıl çalışıyor', 'Bu sistem RAG teknolojisi kullanarak PDF belgelerinizden sorular cevaplayabilir.');
```

**B) Projeden Test:**
- Uygulamayı başlattıktan sonra chat sayfasında yukarıdaki sorulardan birini yazın
- PDF yüklenmediği için sistem otomatik olarak veritabanından cevap arayacaktır
- Eğer soru veritabanında varsa, anında hazır cevabı gösterecektir

### 6. (Opsiyonel) Northwind Database Kurulumu
Eğitim modülü için ayrı bir Northwind veritabanı gereklidir:
```sql
-- PostgreSQL'de yeni veritabanı oluşturun
CREATE DATABASE northwind;
-- Northwind schema ve verilerini içe aktarın
-- https://github.com/pthom/northwind_psql adresinden SQL dosyalarını indirip çalıştırın
```

### 7. Uygulamayı Başlatın
```bash
dotnet run
```

Uygulama `https://localhost:5001` adresinde çalışacaktır.

## 📖 Kullanım

### 🔄 İki Çalışma Modu

**1. RAG Modu (PDF Tabanlı)**
1. **PDF Yükleme**: Ana sayfada bulunan 📎 ikonuna tıklayarak PDF dosyanızı yükleyin
2. **Bekleme**: Dosya işlenirken bekleyin (embedding oluşturma süreci)
3. **Soru Sorma**: Metin kutusuna PDF içeriği hakkında sorularınızı yazın
4. **Cevap Alma**: AI, yüklediğiniz belgedeki bilgilere dayalı cevaplar verecektir

**2. Statik Mod (Veritabanı Tabanlı)**
- PDF yüklenmediğinde sistem otomatik olarak PostgreSQL veritabanındaki `ChatMessages` tablosunu kontrol eder
- Önceden tanımlanmış soru-cevap çiftleri varsa bunları kullanır
- Anında ve hızlı cevaplar için ideal
- Sık sorulan sorular için önceden hazırlanmış cevaplar
- **Test için:** Chat arayüzünde "Merhaba", "Nasılsın" gibi basit sorular yazarak test edebilirsiniz

**3. Northwind Database Modülü**
- Ayrı bir sekmede klasik Northwind veritabanı ile çalışma deneyimi
- Orders tablosuyla veri gönderme ve çekme işlemleri

## 🔧 Yapılandırma

### Embedding Modeli Değiştirme
`Helpers/Embeeding.py` dosyasında model adını değiştirebilirsiniz:
```python
model = SentenceTransformer("intfloat/e5-base-v2")  # Farklı bir model kullanın
```

### LLM Modeli Değiştirme
`Helpers/local_llm.py` dosyasında farklı bir Hugging Face modeli seçebilirsiniz:
```python
model_path = "TinyLlama/TinyLlama-1.1B-Chat-v1.0"  # Farklı bir model
```
