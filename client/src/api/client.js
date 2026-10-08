import axios from "axios";

const baseURL = import.meta.env.VITE_API_URL ?? "https://localhost:7001";

export const TOKEN_KEY = "wh.accessToken";
export const REFRESH_KEY = "wh.refreshToken";

export const tokenStore = {
  get access() {
    return localStorage.getItem(TOKEN_KEY);
  },
  get refresh() {
    return localStorage.getItem(REFRESH_KEY);
  },
  set({ accessToken, refreshToken }) {
    localStorage.setItem(TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_KEY, refreshToken);
  },
  clear() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_KEY);
  },
};

const api = axios.create({
  baseURL,
  headers: { "Content-Type": "application/json" },
});

api.interceptors.request.use((config) => {
  const token = tokenStore.access;
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

let refreshing = null;
let onSessionExpired = () => {};

export const setSessionExpiredHandler = (handler) => {
  onSessionExpired = handler;
};

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;
    const status = error.response?.status;

    if (
      status !== 401 ||
      original?._retried ||
      original?.url?.includes("/auth/")
    ) {
      return Promise.reject(normalizeError(error));
    }

    original._retried = true;

    try {
      refreshing ??= api
        .post("/api/auth/refresh", { refreshToken: tokenStore.refresh })
        .then((res) => {
          tokenStore.set(res.data);
          return res.data.accessToken;
        })
        .finally(() => {
          refreshing = null;
        });

      const newToken = await refreshing;
      original.headers.Authorization = `Bearer ${newToken}`;
      return api(original);
    } catch (refreshError) {
      const refreshStatus = refreshError.response?.status;
      if ([400, 401, 403].includes(refreshStatus)) {
        tokenStore.clear();
        onSessionExpired();
      }
      return Promise.reject(
        normalizeError(refreshStatus ? refreshError : error),
      );
    }
  },
);

function normalizeError(error) {
  const message =
    error.response?.data?.message ??
    (error.response?.status === 429
      ? "Previse zahteva u kratkom roku. Sacekajte minut pa pokusajte ponovo."
      : error.response?.status === 403
        ? "Nemate dozvolu za ovu akciju."
        : error.response?.status === 404
          ? "Trazeni podatak ne postoji."
          : "Server nije dostupan. Proverite da li API radi.");

  return Object.assign(error, { message });
}

export default api;
