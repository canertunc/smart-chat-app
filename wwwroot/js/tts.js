let __audio = null;
let __queue = [];
let __playing = false;

export function stopCurrent() {
  try { __audio?.pause(); __audio = null; } catch {}
  __queue = [];
  __playing = false;
}

function sanitize(s) {
  return String(s || "")
    .replace(/[_*`#~>\-]+/g, " ")
    .replace(/["“”]+/g, "")
    .replace(/\[(.*?)\]\((.*?)\)/g, "$1")
    .replace(/https?:\/\/\S+/g, "")
    .replace(/[^\P{C}\n\r\t]+/gu, " ")
    .replace(/\s+/g, " ")
    .trim();
}

export async function speak(text, { language = "en", speaker_wav = null } = {}) {
  const clean = sanitize(text);
  if (!clean) return null;

  const r = await fetch("/api/tts", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ text: clean, language, speaker_wav })
  });
  if (!r.ok) throw new Error("TTS failed: " + (await r.text().catch(()=>"")));

  const blob = await r.blob();
  const url = URL.createObjectURL(blob);
  stopCurrent();
  __audio = new Audio(url);
  __audio.play().catch(()=>{});
  return url;
}

// parçalama (maks. 240 karakter, ~400 token sınırının çok altında)
export function splitForTts(text, { maxChars = 240 } = {}) {
  const clean = sanitize(text);
  if (!clean) return [];

  let parts = clean.split(/(?<=[\.!\?…])\s+/g).filter(Boolean);

  const out = [];
  for (const p of parts) {
    if (p.length <= maxChars) { out.push(p); continue; }
    
    let s = p;
    while (s.length > maxChars) {
      const slice = s.slice(0, maxChars);
      let cutAt = slice.lastIndexOf(" ");
      if (cutAt < 120) cutAt = maxChars; 
      out.push(slice.slice(0, cutAt).trim());
      s = s.slice(cutAt).trim();
    }
    if (s) out.push(s);
  }
  return out;
}

export async function speakSequential(text, { language = "en" } = {}) {
  const clean = sanitize(text);
  if (!clean) return;

  if (clean.length <= 220) {
    try { await speak(clean, { language }); } catch(e){ console.warn(e); }
    return;
  }

  const chunks = splitForTts(clean, { maxChars: 240 });
  for (const ch of chunks) {
    try { await speak(ch, { language }); }
    catch (e) { console.warn("[TTS] skip chunk:", e?.message || e); }
  }
}
