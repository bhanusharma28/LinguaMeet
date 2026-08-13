// Chrome's Web Speech API performs live recognition in the browser. This wrapper keeps
// recognition alive across normal pauses and reports production permission/network errors.
window.startSpeechRecognition = function (onFinal, onInterim, onStatus) {
  const Speech = window.SpeechRecognition || window.webkitSpeechRecognition;
  if (!Speech) {
    onStatus?.("unsupported", "Live captions are not supported by this browser.");
    return null;
  }

  const locales = {
    en: "en-IN", hi: "hi-IN", es: "es-ES", fr: "fr-FR", de: "de-DE",
    ar: "ar-SA", zh: "zh-CN", ja: "ja-JP", pt: "pt-BR", bn: "bn-IN",
    mr: "mr-IN", gu: "gu-IN", pa: "pa-IN", ta: "ta-IN", te: "te-IN",
    kn: "kn-IN", ml: "ml-IN", ur: "ur-IN", ru: "ru-RU", it: "it-IT",
    ko: "ko-KR", nl: "nl-NL", tr: "tr-TR", pl: "pl-PL", id: "id-ID",
    vi: "vi-VN", th: "th-TH", ne: "ne-NP",
  };

  const recognition = new Speech();
  recognition.continuous = true;
  recognition.interimResults = true;
  recognition.maxAlternatives = 1;
  recognition.lang = locales[meetingConfig.language] || "en-IN";

  let stopped = false;
  let restartTimer = null;

  const start = () => {
    if (stopped || !window.recognitionEnabled) return;
    clearTimeout(restartTimer);
    try {
      recognition.start();
    } catch (error) {
      if (error.name !== "InvalidStateError")
        onStatus?.("error", "Live captions could not start. Click Subtitles to retry.");
    }
  };

  recognition.onstart = () => onStatus?.("listening", "Live captions are listening.");
  recognition.onspeechstart = () => onStatus?.("speech", "Speech detected…");
  recognition.onresult = (event) => {
    let interim = "";
    for (let i = event.resultIndex; i < event.results.length; i++) {
      const text = event.results[i][0].transcript.trim();
      if (!text) continue;
      if (event.results[i].isFinal) onFinal(text);
      else interim += (interim ? " " : "") + text;
    }
    onInterim(interim);
  };
  recognition.onerror = (event) => {
    const messages = {
      "not-allowed": "Microphone or speech recognition permission was denied. Allow it in Chrome site settings, then click Subtitles.",
      "service-not-allowed": "Chrome's speech recognition service is blocked. Check browser policy and site permissions.",
      "audio-capture": "No working microphone was found. Check the selected Windows input device.",
      network: "Speech recognition could not reach Chrome's recognition service. Check the network and retry.",
      "language-not-supported": "Speech recognition is unavailable for the selected language.",
    };
    if (["not-allowed", "service-not-allowed", "audio-capture", "language-not-supported"].includes(event.error))
      stopped = true;
    if (event.error !== "no-speech" && event.error !== "aborted")
      onStatus?.("error", messages[event.error] || `Live captions stopped (${event.error}). Click Subtitles to retry.`);
  };
  recognition.onend = () => {
    if (!stopped && window.recognitionEnabled)
      restartTimer = setTimeout(start, 500);
  };

  start();
  return {
    stop() {
      stopped = true;
      clearTimeout(restartTimer);
      try { recognition.stop(); } catch {}
    },
  };
};
