import { CssBaseline, ThemeProvider } from "@mui/material";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import { AuthProvider } from "./auth/AuthContext";
import ProtectedRoute from "./auth/ProtectedRoute";
import { ToastProvider } from "./components/common/Toast";
import AppLayout from "./components/layout/AppLayout";
import CategoriesPage from "./pages/CategoriesPage";
import DashboardPage from "./pages/DashboardPage";
import LocationsPage from "./pages/LocationsPage";
import LoginPage from "./pages/LoginPage";
import NotFoundPage from "./pages/NotFoundPage";
import OrdersPage from "./pages/OrdersPage";
import PartnersPage from "./pages/PartnersPage";
import ProductsPage from "./pages/ProductsPage";
import theme from "./theme";

export default function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <ToastProvider>
        <BrowserRouter>
          <AuthProvider>
            <Routes>
              <Route path="/prijava" element={<LoginPage />} />

              <Route
                element={
                  <ProtectedRoute>
                    <AppLayout />
                  </ProtectedRoute>
                }
              >
                <Route path="/" element={<DashboardPage />} />

                <Route
                  path="/nalozi"
                  element={
                    <ProtectedRoute permission="orders.view">
                      <OrdersPage />
                    </ProtectedRoute>
                  }
                />

                <Route
                  path="/skladista"
                  element={
                    <ProtectedRoute permission="stock.view">
                      <LocationsPage />
                    </ProtectedRoute>
                  }
                />

                <Route
                  path="/proizvodi"
                  element={
                    <ProtectedRoute permission="products.view">
                      <ProductsPage />
                    </ProtectedRoute>
                  }
                />

                <Route
                  path="/kategorije"
                  element={
                    <ProtectedRoute permission="categories.view">
                      <CategoriesPage />
                    </ProtectedRoute>
                  }
                />

                <Route
                  path="/dobavljaci"
                  element={
                    <ProtectedRoute permission="suppliers.view">
                      <PartnersPage kind="supplier" />
                    </ProtectedRoute>
                  }
                />

                <Route
                  path="/objekti"
                  element={
                    <ProtectedRoute permission="stores.view">
                      <PartnersPage kind="store" />
                    </ProtectedRoute>
                  }
                />

                <Route path="*" element={<NotFoundPage />} />
              </Route>
            </Routes>
          </AuthProvider>
        </BrowserRouter>
      </ToastProvider>
    </ThemeProvider>
  );
}
