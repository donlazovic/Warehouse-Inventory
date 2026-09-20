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
  6: "Storniran",
};

export const orderTypeLabels = { 1: "Ulazni", 2: "Izlazni" };

export const movementTypeLabels = {
  1: "Pocetno stanje",
  2: "Ulaz",
  3: "Izlaz",
  4: "Prenos",
  5: "Korekcija",
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
