import api from "./client";

const unwrap = (promise) => promise.then((res) => res.data);

export const authApi = {
  login: (payload) => unwrap(api.post("/api/auth/login", payload)),
  logout: (refreshToken) => unwrap(api.post("/api/auth/logout", { refreshToken })),
  me: () => unwrap(api.get("/api/auth/me")),
};

const crud = (resource) => ({
  list: (params) => unwrap(api.get(`/api/${resource}`, { params })),
  byId: (id) => unwrap(api.get(`/api/${resource}/${id}`)),
  create: (payload) => unwrap(api.post(`/api/${resource}`, payload)),
  update: (id, payload) => unwrap(api.put(`/api/${resource}/${id}`, payload)),
  remove: (id) => unwrap(api.delete(`/api/${resource}/${id}`)),
});

export const productsApi = {
  ...crud("products"),
  toggleFavorite: (id) => unwrap(api.post(`/api/products/${id}/favorite`)),
};

export const categoriesApi = {
  ...crud("categories"),
  tree: () => unwrap(api.get("/api/categories/tree")),
};

export const suppliersApi = crud("suppliers");
export const storesApi = crud("stores");

export const locationsApi = {
  ...crud("storage-locations"),
  lookup: () => unwrap(api.get("/api/storage-locations/lookup")),
};

export const ordersApi = {
  ...crud("orders"),
  kanban: (params) => unwrap(api.get("/api/orders/kanban", { params })),
  changeStatus: (id, payload) => unwrap(api.patch(`/api/orders/${id}/status`, payload)),
};

export const stockApi = {
  list: (params) => unwrap(api.get("/api/stock", { params })),
  movements: (params) => unwrap(api.get("/api/stock/movements", { params })),
  adjust: (payload) => unwrap(api.post("/api/stock/adjust", payload)),
  setLimits: (id, payload) => unwrap(api.put(`/api/stock/${id}/limits`, payload)),
};
