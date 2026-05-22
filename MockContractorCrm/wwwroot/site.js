const form = document.getElementById("opportunityForm");
const rowsEl = document.getElementById("opportunityRows");
const statusEl = document.getElementById("status");

async function refresh() {
  const response = await fetch("/api/opportunities");
  const opportunities = await response.json();
  rowsEl.innerHTML = "";

  opportunities.forEach((o) => {
    const tr = document.createElement("tr");
    const statusClass = o.status === "Quote Started" ? "status-quote" : "status-open";
    tr.innerHTML = `
      <td>${o.opportunityNumber}</td>
      <td>${o.customerName}<br><small>${o.customerEmail}</small></td>
      <td class="${statusClass}">${o.status}</td>
      <td>
        <button data-id="${o.id}" ${o.launchUrl ? "" : ""}>Start Quote</button>
        ${o.launchUrl ? `<a href="${o.launchUrl}" target="_blank" rel="noopener">Open Configurator</a>` : ""}
      </td>`;
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
