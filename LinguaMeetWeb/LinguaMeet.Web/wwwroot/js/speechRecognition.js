// Web Speech support varies by browser/OS. Each supported app language has a BCP-47 locale.
window.startSpeechRecognition = function (onFinal, onInterim) {
  const Speech = window.SpeechRecognition || window.webkitSpeechRecognition;
  if (!Speech) return null;
  const map = {
    en: "en-IN",
    hi: "hi-IN",
    es: "es-ES",
    fr: "fr-FR",
    de: "de-DE",
    ar: "ar-SA",
    zh: "zh-CN",
    ja: "ja-JP",
    pt: "pt-BR",
    bn: "bn-IN",
    mr: "mr-IN",
    gu: "gu-IN",
    pa: "pa-IN",
    ta: "ta-IN",
    te: "te-IN",
    kn: "kn-IN",
    ml: "ml-IN",
    ur: "ur-IN",
    ru: "ru-RU",
    it: "it-IT",
    ko: "ko-KR",
    nl: "nl-NL",
    tr: "tr-TR",
    pl: "pl-PL",
    id: "id-ID",
    vi: "vi-VN",
    th: "th-TH",
    ne: "ne-NP",
  };
  const r = new Speech();
  r.continuous = true;
  r.interimResults = true;
  r.lang = map[meetingConfig.language] || "en-IN";
  r.onresult = (e) => {
    let interim = "";
    for (let i = e.resultIndex; i < e.results.length; i++) {
      const text = e.results[i][0].transcript;
      if (e.results[i].isFinal) onFinal(text);
      else interim += text;
    }
    onInterim(interim);
  };
  r.onerror = (e) => {
    if (e.error === "language-not-supported") {
      window.recognitionEnabled = false;
      document.getElementById("notice").style.display = "block";
      document.getElementById("notice").textContent =
        "Speech recognition for this language is unavailable in this browser. Video and translated captions still work.";
    }
  };
  r.onend = () => {
    if (window.recognitionEnabled)
      try {
        r.start();
      } catch {}
  };
  r.start();
  return r;
};
