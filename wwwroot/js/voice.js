(() => {
  const micBtn  = document.getElementById("micButton");
  const micIcon = document.getElementById("micIcon");
  const input   = document.getElementById("userInput");
  const sendBtn = document.getElementById("sendButton");

  if (!micBtn || !micIcon || !input || !sendBtn) {
    console.error("[VOICE] Missing DOM");
    return;
  }

  // ✅ SttController -> [Route("api/[controller]")] => /api/stt
  const STT_URL = "/api/stt";

  let stream = null, rec = null, chunks = [], mimeChosen = "", startedAt = 0, busy = false;

  function ui(on) {
    micIcon.classList.toggle("fa-microphone", !on);
    micIcon.classList.toggle("fa-stop", on);
    micIcon.style.filter = on ? "drop-shadow(0 0 4px #ff6b6b)" : "";
    //micBtn.disabled = on; // kayıt sırasında çift tıklama engeli
  }

  function pickMime() {
    const prefs = [
      "audio/ogg;codecs=opus","audio/ogg",
      "audio/webm;codecs=opus","audio/webm"
    ];
    for (const t of prefs) if (MediaRecorder.isTypeSupported(t)) return t;
    return "";
  }

  async function start() {
    if (busy) return;
    busy = true;
    try {
      stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      chunks = [];
      mimeChosen = pickMime();
      if (!mimeChosen) { alert("Tarayıcı ses kaydını desteklemiyor (OGG/WEBM)."); busy = false; return; }

      rec = new MediaRecorder(stream, { mimeType: mimeChosen, audioBitsPerSecond: 128000 });

      rec.ondataavailable = ev => { if (ev.data && ev.data.size > 0) chunks.push(ev.data); };
      rec.onerror = e => console.error("[VOICE] recorder error:", e);

      rec.onstop = async () => {
        try {
          await new Promise(r => setTimeout(r, 50)); // son chunk için minik bekleme
          if (chunks.length === 0) { console.warn("[VOICE] no chunks"); return; }

          const blob = new Blob(chunks, { type: mimeChosen });
          const ext  = mimeChosen.includes("ogg") ? "ogg" : "webm";

          if (blob.size < 4096) { // çok kısa/boş kayıt
            alert("Kayıt çok kısa/bozuk görünüyor. Biraz daha uzun konuşup tekrar dener misin?");
            return;
          }

          console.log("[VOICE] sending", { mimeChosen, ext, size: blob.size });

          const fd = new FormData();
          fd.append("file", blob, `recording.${ext}`); // ✅ alan adı: file

          const res = await fetch(STT_URL, { method: "POST", body: fd });
          if (!res.ok) {
            const err = await res.text().catch(() => "");
            console.error("[VOICE] /api/stt error:", res.status, err);
            alert(`STT failed: ${res.status} ${err}`);
            return;
          }

          const data = await res.json(); // { text: "..." }
          const transcript = (data?.text || "").trim() || "[BLANK_AUDIO]";
          input.value = transcript;
          sendBtn.click(); // LLM akışını tetikle
        } catch (e) {
          console.error("[VOICE] stop/onstop error:", e);
          alert("STT error: " + (e?.message || e));
        } finally {
          try { stream?.getTracks().forEach(t => t.stop()); } catch {}
          rec = null; stream = null; chunks = [];
          ui(false); busy = false;
        }
      };

      rec.start(); // tek parça (timeslice yok)
      startedAt = performance.now();
      ui(true);
      console.log("[VOICE] recorder started:", mimeChosen);
    } catch (err) {
      console.error("[VOICE] getUserMedia error:", err);
      alert("Mikrofona erişilemedi: " + (err?.message || err));
      stop(true);
    }
  }

  function stop(force=false) {
    if (!rec) return;
    const dur = performance.now() - startedAt;
    if (!force && dur < 800) {
      setTimeout(() => stop(true), 850 - dur);
      return;
    }
    try { try { rec.requestData(); } catch {} rec.stop(); } catch {}
  }

  micBtn.addEventListener("click", () => { if (!rec) start(); else stop(); });
  console.log("voice.js loaded ✔️ (single-chunk capture)");
})();
