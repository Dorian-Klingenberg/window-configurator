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

// Listen for completion signal from the configurator iframe
window.addEventListener("message", (event) => {
  const data = event.data;
  if (!data || data.type !== "window.configurator.session.completed") return;
  showCompletionConfirmation(data.sessionId, data.authoritativePrice, data.completedAt);
});

function showCompletionConfirmation(sessionId, price, completedAt) {
  const panel = document.getElementById("completionPanel");
  const sessionEl = document.getElementById("completionSessionId");
  const priceEl = document.getElementById("completionPrice");
  const timeEl = document.getElementById("completionTime");

  sessionEl.textContent = sessionId ?? "—";
  priceEl.textContent = typeof price === "number" ? `$${price.toFixed(2)}` : "—";
  timeEl.textContent = completedAt ? new Date(completedAt).toLocaleString() : "—";

  panel.classList.remove("hidden");
  panel.scrollIntoView({ behavior: "smooth", block: "start" });

  statusEl.textContent = "Quote complete! See your confirmation below.";
}
