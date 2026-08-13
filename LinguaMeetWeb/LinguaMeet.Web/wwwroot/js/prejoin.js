let previewStream = null;
let microphoneOn = true;
let cameraOn = true;

sessionStorage.setItem("mediaMode", "view-only");
sessionStorage.setItem("micEnabled", "false");
sessionStorage.setItem("cameraEnabled", "false");

function setDeviceButton(button, isOn) {
  button.classList.toggle("is-off", !isOn);
  button.querySelector(".device-status").textContent = isOn ? "On" : "Off";
}

function updateDeviceButtons() {
  setDeviceButton(previewMic, microphoneOn);
  setDeviceButton(previewCamera, cameraOn);
}

previewMic.onclick = () => {
  microphoneOn = !microphoneOn;
  previewStream?.getAudioTracks().forEach((track) => {
    track.enabled = microphoneOn;
  });
  sessionStorage.setItem("micEnabled", microphoneOn);
  updateDeviceButtons();
};

previewCamera.onclick = () => {
  cameraOn = !cameraOn;
  previewStream?.getVideoTracks().forEach((track) => {
    track.enabled = cameraOn;
  });
  sessionStorage.setItem("cameraEnabled", cameraOn);
  updateDeviceButtons();
};

if (!navigator.mediaDevices?.getUserMedia) {
  permissionMessage.textContent =
    "Media devices are unavailable. You can join in view-only mode.";
  viewOnlyHelp.classList.remove("d-none");
} else {
  navigator.mediaDevices
    .getUserMedia({ video: true, audio: true })
    .then((stream) => {
      previewStream = stream;
      previewVideo.srcObject = stream;
      permissionMessage.textContent = "Camera and microphone ready";
      previewMic.disabled = false;
      previewCamera.disabled = false;
      sessionStorage.setItem("mediaMode", "full");
      sessionStorage.setItem("micEnabled", "true");
      sessionStorage.setItem("cameraEnabled", "true");
      updateDeviceButtons();
    })
    .catch(() => {
      permissionMessage.textContent =
        "Camera/microphone is busy or unavailable. View-only mode is ready.";
      viewOnlyHelp.classList.remove("d-none");
    });
}
