import api from "./client";

export async function fetchBlob(url, params) {
  try {
    const response = await api.get(url, { params, responseType: "blob" });
    return response.data;
  } catch (error) {
    if (error.response?.status === 429) {
      error.message = "Previse izvoza u kratkom roku. Sacekajte minut pa pokusajte ponovo.";
    } else if (error.response?.data instanceof Blob) {
      try {
        const parsed = JSON.parse(await error.response.data.text());
        if (parsed?.message) error.message = parsed.message;
      } catch {
        // telo nije JSON, zadrzava se postojeca poruka
      }
    }
    throw error;
  }
}

export function saveBlob(blob, fileName) {
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
