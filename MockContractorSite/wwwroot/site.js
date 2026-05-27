const statusEl = document.getElementById("status");
const frameEl = document.getElementById("configuratorFrame");

async function startSession(ctaVariant) {
  statusEl.textContent = "Starting your quote session...";

  try {
    const response = await fetch("/api/session-launch", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ ctaVariant })
    });

    if (!response.ok) {
      throw new Error(`Launch failed (${response.status})`);
    }

    const payload = await response.json();
    frameEl.src = payload.launchUrl;
    statusEl.textContent = `Session started from CTA variant ${ctaVariant}.`;
  } catch (err) {
    statusEl.textContent = `Unable to start session: ${err.message}`;
  }
}

document.querySelectorAll("[data-cta-variant]").forEach((button) => {
  button.addEventListener("click", () => startSession(button.dataset.ctaVariant));
});
