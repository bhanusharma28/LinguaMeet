(async () => {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(meetingConfig.apiUrl + "/hubs/meeting", {
      accessTokenFactory: () => meetingConfig.token,
    })
    .withAutomaticReconnect()
    .build();

  let captionsOn = true;
  let translationOn = true;
  let speechRecognition = null;

  function showNotice(message, persistent = false) {
    notice.style.display = "block";
    notice.textContent = message;
    if (!persistent)
      setTimeout(() => {
        if (notice.textContent === message) notice.style.display = "none";
      }, 3000);
  }

  function startRecognitionIfNeeded() {
    if (speechRecognition || !window.recognitionEnabled) return;

    const ownCaption = document.getElementById(
      "caption-" + meetingConfig.userId,
    );
    speechRecognition = startSpeechRecognition(
      async (text) => {
        if (captionsOn) ownCaption.textContent = text;
        try {
          await connection.invoke(
            "SendTranscript",
            meetingConfig.roomCode,
            meetingConfig.meetingId,
            text,
            meetingConfig.language,
            meetingConfig.language,
          );
        } catch (error) {
          console.error("Final transcript could not be saved.", error);
          showNotice("Speech was recognized, but the transcript could not be saved. Reconnecting…", true);
        }
      },
      (text) => {
        if (captionsOn) ownCaption.textContent = text;
      },
      (status, message) => {
        if (status === "error" || status === "unsupported") {
          speechRecognition?.stop();
          speechRecognition = null;
          showNotice(message, true);
        }
        else if (status === "listening") showNotice(message);
      },
    );
  }

  function currentMediaState() {
    return {
      microphoneOn: !!LinguaRtc.stream.getAudioTracks()[0]?.enabled,
      cameraOn: !!LinguaRtc.stream.getVideoTracks()[0]?.enabled,
    };
  }

  function renderLocalMediaState() {
    const state = currentMediaState();
    localTile.classList.toggle("mic-off", !state.microphoneOn);
    localTile.classList.toggle("camera-off", !state.cameraOn);
    muteBtn.classList.toggle("active", !state.microphoneOn);
    cameraBtn.classList.toggle("active", !state.cameraOn);
    return state;
  }

  async function publishMediaState() {
    const state = renderLocalMediaState();
    if (connection.state !== signalR.HubConnectionState.Connected) return;
    try {
      await connection.invoke(
        "UpdateMediaState",
        meetingConfig.roomCode,
        state.microphoneOn,
        state.cameraOn,
      );
    } catch (error) {
      console.warn("Media status could not be broadcast.", error);
    }
  }

  // Attach controls before connecting, so local controls remain usable if SignalR has a problem.
  muteBtn.onclick = async () => {
    const track = LinguaRtc.stream.getAudioTracks()[0];
    if (!track) return alert("Microphone is unavailable in view-only mode.");
    track.enabled = !track.enabled;
    window.recognitionEnabled = track.enabled;
    if (track.enabled) {
      startRecognitionIfNeeded();
    } else if (speechRecognition) {
      speechRecognition.stop();
      speechRecognition = null;
    }
    await publishMediaState();
  };
  cameraBtn.onclick = async () => {
    const track = LinguaRtc.stream.getVideoTracks()[0];
    if (!track) return alert("Camera is unavailable in view-only mode.");
    track.enabled = !track.enabled;
    await publishMediaState();
  };
  subtitlesBtn.onclick = () => {
    captionsOn = !captionsOn;
    subtitlesBtn.classList.toggle("active", captionsOn);
    document
      .querySelectorAll(".tile-caption")
      .forEach(
        (element) => (element.style.display = captionsOn ? "block" : "none"),
      );
    if (captionsOn && window.recognitionEnabled && !speechRecognition)
      startRecognitionIfNeeded();
  };
  translationBtn.onclick = () => {
    translationOn = !translationOn;
    translationBtn.classList.toggle("active", translationOn);
  };
  peopleBtn.onclick = () =>
    alert(LinguaRtc.peers.size + 1 + " participant(s) connected");

  connection.on("ReceiveTranscript", (userId, name, original, translated) => {
    const element = document.getElementById("caption-" + userId);
    if (element && captionsOn)
      element.textContent = translationOn ? translated : original;
    setTimeout(() => {
      if (element) element.textContent = "";
    }, 7000);
  });

  connection.onreconnected(() => {
    showNotice("Meeting reconnected. Live captions resumed.");
    if (window.recognitionEnabled && !speechRecognition) startRecognitionIfNeeded();
  });

  try {
    await LinguaRtc.start(connection);
    const initialState = renderLocalMediaState();
    await connection.start();
    try {
      await connection.invoke(
        "JoinMeeting",
        meetingConfig.roomCode,
        meetingConfig.language,
        initialState.microphoneOn,
        initialState.cameraOn,
      );
    } catch {
      // Compatibility with an API process that was started before media-state support was added.
      await connection.invoke(
        "JoinMeeting",
        meetingConfig.roomCode,
        meetingConfig.language,
      );
    }
  } catch (error) {
    notice.style.display = "block";
    notice.textContent =
      "Could not connect to the meeting. Local camera and microphone controls are still available.";
    console.error(error);
    return;
  }

  window.recognitionEnabled = currentMediaState().microphoneOn;
  startRecognitionIfNeeded();

  if (!LinguaRtc.mediaAvailable) {
    notice.style.display = "block";
    notice.textContent =
      "View-only mode: your camera, microphone, and speech recognition are off.";
  } else if (window.recognitionEnabled && !speechRecognition) {
    notice.style.display = "block";
    notice.textContent =
      "Live speech recognition is unavailable. Video meetings still work.";
  }

  window.addEventListener("beforeunload", () => {
    if (connection.state === signalR.HubConnectionState.Connected)
      connection.invoke("LeaveMeeting", meetingConfig.roomCode);
  });
})();
