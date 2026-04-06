const api = (path, options = {}) =>
  fetch(path, {
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    ...options,
  });

function parseLocalDate(iso) {
  const part = String(iso).split("T")[0];
  const [y, m, d] = part.split("-").map(Number);
  return new Date(y, m - 1, d);
}

function daysUntil(dateStr) {
  const d = parseLocalDate(dateStr);
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  d.setHours(0, 0, 0, 0);
  return Math.round((d - today) / (1000 * 60 * 60 * 24));
}

function rowClass(medicine) {
  const expiringSoon = daysUntil(medicine.expiryDate) < 30;
  const lowStock = medicine.quantity < 10;
  if (expiringSoon) return "row-expiry";
  if (lowStock) return "row-low-stock";
  return "";
}

function formatMoney(n) {
  return Number(n).toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });
}

function formatDate(iso) {
  return parseLocalDate(iso).toLocaleDateString();
}

function formatDateTime(iso) {
  const d = new Date(iso);
  return d.toLocaleString();
}

async function loadMedicines(search = "") {
  const q = new URLSearchParams();
  if (search.trim()) q.set("search", search.trim());
  const url = "/api/medicines" + (q.toString() ? "?" + q.toString() : "");
  const res = await api(url);
  if (!res.ok) throw new Error("Failed to load medicines");
  return res.json();
}

function renderMedicines(list) {
  const tbody = document.getElementById("medicines-body");
  const empty = document.getElementById("medicines-empty");
  tbody.innerHTML = "";
  if (!list.length) {
    empty.classList.remove("hidden");
    return;
  }
  empty.classList.add("hidden");
  for (const m of list) {
    const tr = document.createElement("tr");
    tr.className = rowClass(m);
    tr.innerHTML = `
      <td>${escapeHtml(m.fullName)}</td>
      <td>${escapeHtml(m.brand)}</td>
      <td>${formatDate(m.expiryDate)}</td>
      <td>${m.quantity}</td>
      <td>${formatMoney(m.price)}</td>
    `;
    tbody.appendChild(tr);
  }
}

function escapeHtml(s) {
  const div = document.createElement("div");
  div.textContent = s;
  return div.innerHTML;
}

let searchDebounce;
document.getElementById("search-medicines").addEventListener("input", (e) => {
  clearTimeout(searchDebounce);
  searchDebounce = setTimeout(async () => {
    try {
      const list = await loadMedicines(e.target.value);
      renderMedicines(list);
    } catch (err) {
      console.error(err);
    }
  }, 250);
});

document.getElementById("btn-refresh").addEventListener("click", async () => {
  const search = document.getElementById("search-medicines").value;
  try {
    const list = await loadMedicines(search);
    renderMedicines(list);
  } catch (err) {
    console.error(err);
  }
});

document.getElementById("form-add-medicine").addEventListener("submit", async (e) => {
  e.preventDefault();
  const form = e.target;
  const msg = document.getElementById("add-message");
  msg.textContent = "";
  msg.className = "message";
  const fd = new FormData(form);
  const body = {
    fullName: fd.get("fullName"),
    brand: fd.get("brand"),
    expiryDate: fd.get("expiryDate"),
    quantity: parseInt(fd.get("quantity"), 10),
    price: parseFloat(fd.get("price")),
    notes: fd.get("notes") || "",
  };
  const res = await api("/api/medicines", {
    method: "POST",
    body: JSON.stringify(body),
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    msg.textContent = err.title || err.message || "Could not save.";
    msg.classList.add("error");
    return;
  }
  msg.textContent = "Medicine saved.";
  msg.classList.add("success");
  form.reset();
  const list = await loadMedicines(document.getElementById("search-medicines").value);
  renderMedicines(list);
  populateSaleMedicines(list);
});

async function loadSales() {
  const res = await api("/api/sales");
  if (!res.ok) throw new Error("Failed to load sales");
  return res.json();
}

function renderSales(list) {
  const tbody = document.getElementById("sales-body");
  const empty = document.getElementById("sales-empty");
  tbody.innerHTML = "";
  if (!list.length) {
    empty.classList.remove("hidden");
    return;
  }
  empty.classList.add("hidden");
  for (const s of list) {
    const tr = document.createElement("tr");
    const line = s.quantitySold * s.unitPrice;
    tr.innerHTML = `
      <td>${formatDateTime(s.saleDate)}</td>
      <td>${escapeHtml(s.medicineName)}</td>
      <td>${s.quantitySold}</td>
      <td>${formatMoney(s.unitPrice)}</td>
      <td>${formatMoney(line)}</td>
    `;
    tbody.appendChild(tr);
  }
}

document.getElementById("btn-refresh-sales").addEventListener("click", async () => {
  try {
    const sales = await loadSales();
    renderSales(sales);
  } catch (err) {
    console.error(err);
  }
});

function populateSaleMedicines(medicines) {
  const sel = document.getElementById("sale-medicine-select");
  const current = sel.value;
  sel.innerHTML = '<option value="">Select medicine…</option>';
  for (const m of medicines) {
    const opt = document.createElement("option");
    opt.value = String(m.id);
    opt.textContent = `${m.fullName} (stock: ${m.quantity})`;
    sel.appendChild(opt);
  }
  if (current && [...sel.options].some((o) => o.value === current)) sel.value = current;
}

document.getElementById("form-sale").addEventListener("submit", async (e) => {
  e.preventDefault();
  const msg = document.getElementById("sale-message");
  msg.textContent = "";
  msg.className = "message";
  const fd = new FormData(e.target);
  const medicineId = parseInt(fd.get("medicineId"), 10);
  const quantitySold = parseInt(fd.get("quantitySold"), 10);
  const res = await api("/api/sales", {
    method: "POST",
    body: JSON.stringify({ medicineId, quantitySold }),
  });
  const data = await res.json().catch(() => ({}));
  if (!res.ok) {
    msg.textContent = data.message || "Sale failed.";
    msg.classList.add("error");
    return;
  }
  msg.textContent = "Sale recorded. Stock updated.";
  msg.classList.add("success");
  e.target.reset();
  const list = await loadMedicines(document.getElementById("search-medicines").value);
  renderMedicines(list);
  populateSaleMedicines(list);
  const sales = await loadSales();
  renderSales(sales);
});

function setupTabs() {
  const tabs = document.querySelectorAll(".tab");
  const panels = {
    inventory: document.getElementById("panel-inventory"),
    add: document.getElementById("panel-add"),
    sales: document.getElementById("panel-sales"),
    "record-sale": document.getElementById("panel-record-sale"),
  };
  tabs.forEach((tab) => {
    tab.addEventListener("click", () => {
      const name = tab.dataset.tab;
      tabs.forEach((t) => {
        t.classList.toggle("active", t === tab);
        t.setAttribute("aria-selected", t === tab ? "true" : "false");
      });
      Object.entries(panels).forEach(([key, el]) => {
        const on = key === name;
        el.classList.toggle("active", on);
        el.hidden = !on;
      });
    });
  });
}

async function init() {
  setupTabs();
  try {
    const list = await loadMedicines();
    renderMedicines(list);
    populateSaleMedicines(list);
    const sales = await loadSales();
    renderSales(sales);
  } catch (err) {
    console.error(err);
  }
}

init();
