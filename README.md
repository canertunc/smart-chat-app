# .NET Tabanlı Akıllı Sohbet Uygulaması

## 📌 Proje Genel Bakış
Bu proje, **.NET ekosistemi** kullanarak hem **frontend** hem **backend** geliştirerek, çok yönlü bir **akıllı sohbet uygulaması** oluşturmayı hedefler.  
Kullanıcılar hem **yazılı** hem **sesli** olarak etkileşime geçebilir ve sistem **API dokümantasyonlarını anlayıp otomatik işlemler yapabilir**.
Ayrıca, Graph Neural Network (GNN) tabanlı bir öneri sistemi ile kullanıcıya veya şirkete özel tavsiyeler sunar dolayısıyla
bu sistem, veriler arasındaki ilişkileri graf yapıları üzerinden analiz ederek, daha isabetli ürün, hizmet veya satış stratejisi önerileri üretir.

---

## 🤖 AI Model Entegrasyonu

### Model Seçenekleri
- **Local LLM Modelleri**
  - Kendi sunucumuzda barındırılan, fine-tune edilebilir modeller (örn. LLaMA, Mistral, Phi-3)
  - Avantaj: Veri güvenliği yüksek, internet bağımlılığı yok
- **Bulut Tabanlı API’ler**
  - OpenAI API (GPT-4o, GPT-4.1 vb.)
  - Anthropic Claude API
  - Avantaj: Yüksek doğruluk, sürekli güncel model

**Çalışma Mantığı:** Burada iki farklı model türünü de uygulayabiliriz veya birini seçebiliriz.

---

## 🎙️ Ses ve Metin Desteği

### Ses İşleme Seçenekleri
1. **Hazır API Çözümleri**
   - VAPI, Azure Cognitive Services, Google Speech API
   - Hızlı geliştirme ve bakım kolaylığı
2. **Özel .NET Çözümü**
   - **Text-to-Speech (TTS)**: Model cevabını sesli okuma
   - **Speech-to-Text (STT)**: Kullanıcı sesini metne çevirme
   - Tamamen .NET tabanlı, veri güvenliği avantajı

**Senaryo:** Kullanıcı konuşur → STT ile metne çevrilir → AI cevap üretir → TTS ile sesli yanıt verilir.

---

## 📚 API Dokümantasyon Uzmanı

### Ana Özellikler
- Dokümantasyon analizi: API belgelerini okuyup anlama
- Endpoint yönetimi: GET, POST, PUT, DELETE metodlarını doğru kullanma
- Otomatik API çağrıları: "Son 10 siparişi getir" → API çağrısı yapılır
- MCP Server entegrasyonu: Model Context Protocol ile doğrudan API iletişimi

**Teknoloji:** .NET backend + MCP client kütüphanesi (MCP serverı baştan kendimiz yazabiliriz protocol kurallarına uymak koşuluyla)

---

## 📊 Akıllı Satış Analizi Sistemi

**Gerçek Zamanlı Veri Entegrasyonu:**
  .NET’in asenkron programlama ve SignalR teknolojisi sayesinde, satış ve müşteri verileri farklı kaynaklardan gerçek zamanlı olarak toplanır ve frontend’e hızlıca iletilir.

**Otomatik Grafik ve Raporlama:**
  Analiz edilen veriler .NET backend tarafından işlenip JSON formatında frontend’e sunulur. Frontend’de Chart.js veya Plotly ile görselleştirilirken, istenirse .NET grafik kütüphaneleri kullanılarak backend’de raporlar ve PDF çıktıları oluşturulur.

**GNN Tabanlı Öneri Sistemi:**
  Karmaşık satış verisi ilişkileri Graph Neural Network modelleri ile analiz edilir. Model eğitimi GPU destekli ML altyapılarında gerçekleştirilirken, güncellenen modeller .NET servisleri tarafından API olarak sunulur ve kişiselleştirilmiş öneriler anlık olarak kullanıcıya iletilir.

**Haftalık Model Güncellemeleri:**
  ETL süreçleri ve Azure Data Factory gibi araçlarla hazırlanan veriler, periyodik olarak ML eğitim ortamlarına aktarılır ve model performansı düzenli olarak iyileştirilir.

**Güvenlik ve Kullanıcı Yönetimi:**
  .NET Identity ve rol tabanlı erişim kontrolleri ile sistem güvenli ve ölçeklenebilir hale getirilir. Kullanıcı bilgileri ve geçmiş performans baz alınarak kişiselleştirilmiş stratejiler oluşturulur.

---

## 🎯 Hedef Kullanım Senaryoları
1. **API Dokümantasyon Danışmanlığı**
   - _"Bu API’de kullanıcı verilerini nasıl çekerim?"_
2. **Anlık Satış Analizi**
   - _"Bu ayın satış trendlerini grafikle göster"_
3. **Akıllı Öneriler**
   - _"Satışlarımı artırmak için ne öneriyorsun?"_
4. **Otomatik API İşlemleri**
   - _"Yeni müşteri kaydı oluştur"_

---

## 🚀 Ekstra Geliştirme Fikirleri
- **Rol bazlı kullanıcı yönetimi** (Admin, API geliştirici, satış yöneticisi)
- **Anomali tespiti**
- **Güvenlik Geliştirmeleri** (Güvenlik kısmı daha detaylı araştırılabilir.)
  - OpenID Connect (OIDC) ile kurumsal SSO entegrasyonu
  - JWT + mTLS ile API güvenliği
  - FIDO2/WebAuthn ile parolasız kimlik doğrulama
  - Hassas verilerin maskeleme ve şifreleme
