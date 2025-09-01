# .NET Tabanlı Akıllı Sohbet Uygulaması

## 📌 Proje Genel Bakış
Bu proje, **.NET ekosistemi** kullanarak hem **frontend** hem **backend** geliştirerek, çok yönlü bir **akıllı sohbet uygulaması** oluşturmayı hedefler.  
Kullanıcılar hem **yazılı** hem **sesli** olarak etkileşime geçebilir.**.
Ayrıca, Graph Neural Network (GNN) tabanlı bir öneri sistemi ile kullanıcıya veya şirkete özel tavsiyeler sunar dolayısıyla
bu sistem, veriler arasındaki ilişkileri graf yapıları üzerinden analiz ederek, daha isabetli ürün, hizmet veya satış stratejisi önerileri üretir.

---

## 🤖 AI Model Entegrasyonu

### Model
- **Local LLM Modelleri**
  - Kendi sunucumuzda barındırılan, Phi-3-mini
  - Avantaj: Veri güvenliği yüksek, internet bağımlılığı yok
  
---

## 🎙️ Ses ve Metin Desteği

### Ses İşleme

**Özel .NET Çözümü**
   - **Text-to-Speech (TTS)**: Model cevabını sesli okuma
   - **Speech-to-Text (STT)**: Kullanıcı sesini metne çevirme
   - Tamamen .NET tabanlı, veri güvenliği avantajı

**Senaryo:** Kullanıcı konuşur → STT ile metne çevrilir → AI cevap üretir → TTS ile sesli yanıt verilir.

---

## 📊 Akıllı Satış Analizi Sistemi

**Gerçek Zamanlı Veri Entegrasyonu:**
  .NET’in asenkron programlama ve SignalR teknolojisi sayesinde, satış ve müşteri verileri farklı kaynaklardan gerçek zamanlı olarak toplanır ve frontend’e hızlıca iletilir.

**GNN Tabanlı Öneri Sistemi:**
  Karmaşık satış verisi ilişkileri Graph Neural Network modelleri ile analiz edilir. Model eğitimi GPU destekli ML altyapılarında gerçekleştirilirken, güncellenen modeller .NET servisleri tarafından API olarak sunulur ve kişiselleştirilmiş öneriler anlık olarak kullanıcıya iletilir.

**Güvenlik ve Kullanıcı Yönetimi:**
  .NET Identity ve rol tabanlı erişim kontrolleri ile sistem güvenli ve ölçeklenebilir hale getirilir. Kullanıcı bilgileri ve geçmiş performans baz alınarak kişiselleştirilmiş stratejiler oluşturulur.

---

## 🚀 Ekstra Geliştirme Fikirleri
- **Rol bazlı kullanıcı yönetimi** (Admin, API geliştirici, satış yöneticisi)
- **Anomali tespiti**
- **Güvenlik Geliştirmeleri** (Güvenlik kısmı daha detaylı araştırılabilir.)
  - OpenID Connect (OIDC) ile kurumsal SSO entegrasyonu
  - JWT + mTLS ile API güvenliği
  - FIDO2/WebAuthn ile parolasız kimlik doğrulama
  - Hassas verilerin maskeleme ve şifreleme
