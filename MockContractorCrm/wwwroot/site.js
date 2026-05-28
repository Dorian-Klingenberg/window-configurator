const form = document.getElementById("opportunityForm");
const rowsEl = document.getElementById("opportunityRows");
const statusEl = document.getElementById("status");

async function refresh() {
  const response = await fetch("/api/opportunities");
  const opportunities = await response.json();
  rowsEl.innerHTML = "";

  opportunities.forEach((o) => {
    const tr = document.createElement("tr");
    const statusClass =
      o.status === "Quote Submitted" ? "status-submitted" :
      o.status === "Quote Started" ? "status-quote" : "status-open";

    const priceDisplay = o.authoritativeTotalPrice != null
      ? `$${o.authoritativeTotalPrice.toFixed(2)}`
      : o.status === "Quote Submitted" ? "—" : "";

    const itemCountDisplay = o.completedItemCount != null ? o.completedItemCount : "";

    const actionHtml = o.status === "Quote Submitted"
      ? `<span class="badge-complete">&#10003; Submitted</span>`
      : o.launchUrl
        ? `<button class="btn-refresh" data-id="${o.id}">Refresh Status</button>
           <a href="${o.launchUrl}" target="_blank" rel="noopener">Open Configurator</a>`
        : `<button data-id="${o.id}">Start Quote</button>`;

    tr.innerHTML = `
      <td>${o.opportunityNumber}</td>
      <td>${o.customerName}<br><small>${o.customerEmail}</small></td>
      <td class="${statusClass}">${o.status}</td>
      <td>${itemCountDisplay}</td>
      <td>${priceDisplay}</td>
      <td>${actionHtml}</td>`;
    rowsEl.appendChild(tr);
  });
}

form.addEventListener("submit", async (e) => {
  e.preventDefault();
  statusEl.textContent = "Creating opportunity...";
  const customerName = document.getElementById("customerName").value;
  const customerEmail = document.getElementById("customerEmail").value;

  const response = await fetch("/api/opportunities", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ customerName, customerEmail })
  });

  if (!response.ok) {
    statusEl.textContent = "Unable to create opportunity.";
    return;
  }

  form.reset();
  statusEl.textContent = "Opportunity created.";
  await refresh();
});

rowsEl.addEventListener("click", async (e) => {
  const target = e.target;
  if (!(target instanceof HTMLButtonElement)) return;

  const id = target.getAttribute("data-id");
  if (!id) return;

  if (target.classList.contains("btn-refresh")) {
    statusEl.textContent = "Checking quote status...";
    const response = await fetch(`/api/opportunities/${id}/refresh-status`, {
      method: "POST",
      headers: { "Content-Type": "application/json" }
    });

    if (!response.ok) {
      const err = await response.json().catch(() => ({}));
      statusEl.textContent = err.error ?? "Unable to refresh status.";
      return;
    }

    const payload = await response.json();
    if (payload.status === "Completed" || payload.status === "Submitted") {
      const price = payload.totalPrice != null ? ` · Total: $${payload.totalPrice.toFixed(2)}` : "";
      statusEl.textContent = `Quote complete — ${payload.itemCount} item(s)${price}`;
    } else {
      statusEl.textContent = `Session status: ${payload.status}`;
    }
    await refresh();
    return;
  }

  // Default: Start Quote
  statusEl.textContent = "Starting quote session...";
  const response = await fetch(`/api/opportunities/${id}/start-quote`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({})
  });

  if (!response.ok) {
    statusEl.textContent = "Unable to start quote session.";
    return;
  }

  const payload = await response.json();
  statusEl.textContent = "Quote session started.";
  window.open(payload.launchUrl, "_blank", "noopener");
  await refresh();
});

refresh();
