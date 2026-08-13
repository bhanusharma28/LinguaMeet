// WebRTC carries media. SignalR separately carries camera/microphone UI state.
window.LinguaRtc = {
  peers: new Map(),
  stream: new MediaStream(),
  mediaAvailable: false,
  states: new Map(),
  async start(connection) {
    if (sessionStorage.getItem("mediaMode") !== "view-only") {
      try {
        this.stream = await navigator.mediaDevices.getUserMedia({
          video: true,
          audio: true,
        });
        this.stream
          .getAudioTracks()
          .forEach(
            (t) =>
              (t.enabled = sessionStorage.getItem("micEnabled") !== "false"),
          );
        this.stream
          .getVideoTracks()
          .forEach(
            (t) =>
              (t.enabled = sessionStorage.getItem("cameraEnabled") !== "false"),
          );
        this.mediaAvailable = true;
        localVideo.srcObject = this.stream;
      } catch {
        sessionStorage.setItem("mediaMode", "view-only");
      }
    }
    connection.on(
      "ParticipantJoined",
      async (id, userId, name, language, micOn, cameraOn) => {
        this.states.set(id, { micOn, cameraOn });
        const pc = this.createPeer(id, userId, name, connection);
        this.updateTile(id, micOn, cameraOn);
        const offer = await pc.createOffer();
        await pc.setLocalDescription(offer);
        await connection.invoke(
          "SendOffer",
          meetingConfig.roomCode,
          id,
          JSON.stringify(offer),
          this.stream.getAudioTracks()[0]?.enabled ?? false,
          this.stream.getVideoTracks()[0]?.enabled ?? false,
        );
      },
    );
    connection.on("MediaStateChanged", (id, micOn, cameraOn) => {
      this.states.set(id, { micOn, cameraOn });
      this.updateTile(id, micOn, cameraOn);
    });
    connection.on(
      "ReceiveOffer",
      async (id, userId, name, offer, micOn, cameraOn) => {
      this.states.set(id, { micOn, cameraOn });
      const pc = this.createPeer(id, userId, name, connection);
      this.ensureTile(id, userId, name);
      this.updateTile(id, micOn, cameraOn);
      await pc.setRemoteDescription(JSON.parse(offer));
      const answer = await pc.createAnswer();
      await pc.setLocalDescription(answer);
      await connection.invoke(
        "SendAnswer",
        meetingConfig.roomCode,
        id,
        JSON.stringify(answer),
      );
      },
    );
    connection.on(
      "ReceiveAnswer",
      async (id, answer) =>
        await this.peers.get(id)?.setRemoteDescription(JSON.parse(answer)),
    );
    connection.on(
      "ReceiveIceCandidate",
      async (id, candidate) =>
        await this.peers.get(id)?.addIceCandidate(JSON.parse(candidate)),
    );
    connection.on("ParticipantLeft", (id) => {
      this.peers.get(id)?.close();
      this.peers.delete(id);
      document.getElementById("tile-" + id)?.remove();
    });
  },
  createPeer(id, userId, name, connection) {
    if (this.peers.has(id)) return this.peers.get(id);
    const pc = new RTCPeerConnection({
      iceServers: [{ urls: "stun:stun.l.google.com:19302" }],
    });
    if (this.mediaAvailable)
      this.stream
        .getTracks()
        .forEach((track) => pc.addTrack(track, this.stream));
    else {
      pc.addTransceiver("audio", { direction: "recvonly" });
      pc.addTransceiver("video", { direction: "recvonly" });
    }
    pc.onicecandidate = (e) => {
      if (e.candidate)
        connection.invoke(
          "SendIceCandidate",
          meetingConfig.roomCode,
          id,
          JSON.stringify(e.candidate),
        );
    };
    pc.ontrack = (e) => {
      const tile = this.ensureTile(id, userId, name);
      tile.querySelector("video").srcObject = e.streams[0];
      const state = this.states.get(id);
      if (state) this.updateTile(id, state.micOn, state.cameraOn);
    };
    this.peers.set(id, pc);
    return pc;
  },
  ensureTile(id, userId, name) {
    let tile = document.getElementById("tile-" + id);
    if (tile) return tile;
    tile = document.createElement("article");
    tile.className = "video-tile";
    tile.id = "tile-" + id;
    const video = document.createElement("video");
    video.autoplay = true;
    video.playsInline = true;
    const off = document.createElement("div");
    off.className = "camera-off-panel";
    const avatar = document.createElement("div");
    avatar.className = "participant-avatar";
    avatar.textContent = (name || "P")
      .split(/\s+/)
      .slice(0, 2)
      .map((x) => x[0])
      .join("")
      .toUpperCase();
    off.appendChild(avatar);
    const mic = document.createElement("div");
    mic.className = "mic-status";
    mic.textContent = "Mic off";
    const label = document.createElement("div");
    label.className = "tile-name";
    label.textContent = name || "Participant";
    const caption = document.createElement("div");
    caption.className = "tile-caption";
    caption.id = "caption-" + userId;
    tile.append(video, off, mic, label, caption);
    videoGrid.appendChild(tile);
    return tile;
  },
  updateTile(id, micOn, cameraOn) {
    const tile = document.getElementById("tile-" + id);
    if (!tile) return;
    tile.classList.toggle("mic-off", !micOn);
    tile.classList.toggle("camera-off", !cameraOn);
  },
};
