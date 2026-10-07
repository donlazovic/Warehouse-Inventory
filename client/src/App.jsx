import { BrowserRouter, Route, Routes } from "react-router-dom";
import { AuthProvider } from "./auth/AuthContext";
import ProtectedRoute from "./auth/ProtectedRoute";
import { ToastProvider } from "./components/common/Toast";
import AppLayout from "./components/layout/AppLayout";
import CategoriesPage from "./pages/CategoriesPage";
import DashboardPage from "./pages/DashboardPage";
import ForgotPasswordPage from "./pages/ForgotPasswordPage";
import LocationsPage from "./pages/LocationsPage";
import MovementsPage from "./pages/MovementsPage";
import LoginPage from "./pages/LoginPage";
import NotFoundPage from "./pages/NotFoundPage";
import OrdersPage from "./pages/OrdersPage";
import PartnersPage from "./pages/PartnersPage";
import ProductsPage from "./pages/ProductsPage";
import RegisterPage from "./pages/RegisterPage";
import ReportsPage from "./pages/ReportsPage";
import ResetPasswordPage from "./pages/ResetPasswordPage";
import SettingsPage from "./pages/SettingsPage";
import StockPage from "./pages/StockPage";
import UsersPage from "./pages/UsersPage";
import { RealtimeProvider } from "./realtime/RealtimeProvider";
import { ColorModeProvider } from "./ColorMode";

const guarded = (permission, element) => (
  <ProtectedRoute permission={permission}>{element}</ProtectedRoute>
);

export default function App() {
  return (
    <ColorModeProvider>
      <ToastProvider>
        <BrowserRouter>
          <AuthProvider>
            <RealtimeProvider>
            <Routes>
              <Route path="/prijava" element={<LoginPage />} />
              <Route path="/registracija" element={<RegisterPage />} />
              <Route path="/zaboravljena-lozinka" element={<ForgotPasswordPage />} />
              <Route path="/reset-lozinke" element={<ResetPasswordPage />} />

              <Route
                element={
                  <ProtectedRoute>
                    <AppLayout />
                  </ProtectedRoute>
                }
              >
                <Route path="/" element={<DashboardPage />} />
                <Route path="/nalozi" element={guarded("orders.view", <OrdersPage />)} />
                <Route path="/izvestaji" element={guarded("reports.view", <ReportsPage />)} />
                <Route path="/zalihe" element={guarded("stock.view", <StockPage />)} />
                <Route path="/skladista" element={guarded("stock.view", <LocationsPage />)} />
                <Route path="/kretanja" element={guarded("stock.view", <MovementsPage />)} />
                <Route path="/proizvodi" element={guarded("products.view", <ProductsPage />)} />
                <Route path="/kategorije" element={guarded("categories.view", <CategoriesPage />)} />
                <Route path="/dobavljaci" element={guarded("suppliers.view", <PartnersPage kind="supplier" />)} />
                <Route path="/objekti" element={guarded("stores.view", <PartnersPage kind="store" />)} />
                <Route path="/korisnici" element={guarded("users.view", <UsersPage />)} />
                <Route path="/podesavanja" element={<SettingsPage />} />
                <Route path="*" element={<NotFoundPage />} />
              </Route>
            </Routes>
            </RealtimeProvider>
          </AuthProvider>
        </BrowserRouter>
      </ToastProvider>
    </ColorModeProvider>
  );
}
