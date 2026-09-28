export const unitLabels = {
  1: "kom",
  2: "kg",
  3: "g",
  4: "l",
  5: "ml",
  6: "pak",
  7: "gajba",
  8: "paleta",
};

export const orderStatusLabels = {
  1: "Nacrt",
  2: "Ceka odobrenje",
  3: "Odobren",
  4: "U realizaciji",
  5: "Realizovan",
  6: "Otkazan",
};

export const orderTypeLabels = { 1: "Ulazni", 2: "Izlazni" };

export const movementTypeLabels = {
  1: "Pocetno stanje",
  2: "Ulaz",
  3: "Izlaz",
  4: "Prenos",
  5: "Korekcija",
};

export const movementTypeColors = {
  1: "#6B7280",
  2: "#1F5F4B",
  3: "#B4541A",
  4: "#1D5A87",
  5: "#7A5C2E",
};

export const issueReasonLabels = {
  1: "Prodaja",
  2: "Otpis",
  3: "Lom",
  4: "Interna potrosnja",
};

export const locationTypeLabels = { 1: "Centralni magacin", 2: "Prodajni objekat" };

const numberFormat = new Intl.NumberFormat("sr-RS", {
  minimumFractionDigits: 0,
  maximumFractionDigits: 3,
});

const currencyFormat = new Intl.NumberFormat("sr-RS", {
  style: "currency",
  currency: "RSD",
  maximumFractionDigits: 2,
});

export const formatQuantity = (value) => numberFormat.format(value ?? 0);
export const formatMoney = (value) => currencyFormat.format(value ?? 0);

export const formatDate = (value) =>
  value ? new Date(value).toLocaleDateString("sr-RS") : "—";

export const formatDateTime = (value) =>
  value
    ? new Date(value).toLocaleString("sr-RS", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
      })
    : "—";

export const startOfDayIso = (dateString) =>
  dateString ? new Date(`${dateString}T00:00:00`).toISOString() : null;

export const endOfDayIso = (dateString) =>
  dateString ? new Date(`${dateString}T23:59:59.999`).toISOString() : null;
