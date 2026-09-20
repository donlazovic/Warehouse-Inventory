import AssignmentIcon from "@mui/icons-material/Assignment";
import CategoryIcon from "@mui/icons-material/Category";
import DashboardIcon from "@mui/icons-material/Dashboard";
import HistoryIcon from "@mui/icons-material/History";
import Inventory2Icon from "@mui/icons-material/Inventory2";
import LocalShippingIcon from "@mui/icons-material/LocalShipping";
import StorefrontIcon from "@mui/icons-material/Storefront";
import WarehouseIcon from "@mui/icons-material/Warehouse";

export const navigation = [
  {
    heading: "Pregled",
    items: [{ label: "Kontrolna tabla", to: "/", icon: DashboardIcon, permission: "reports.view" }],
  },
  {
    heading: "Roba",
    items: [
      { label: "Proizvodi", to: "/proizvodi", icon: Inventory2Icon, permission: "products.view" },
      { label: "Kategorije", to: "/kategorije", icon: CategoryIcon, permission: "categories.view" },
      { label: "Zalihe", to: "/zalihe", icon: WarehouseIcon, permission: "stock.view" },
      { label: "Kretanje robe", to: "/kretanja", icon: HistoryIcon, permission: "stock.view" },
    ],
  },
  {
    heading: "Nalozi",
    items: [{ label: "Nalozi", to: "/nalozi", icon: AssignmentIcon, permission: "orders.view" }],
  },
  {
    heading: "Partneri",
    items: [
      { label: "Dobavljaci", to: "/dobavljaci", icon: LocalShippingIcon, permission: "suppliers.view" },
      { label: "Prodajni objekti", to: "/objekti", icon: StorefrontIcon, permission: "stores.view" },
    ],
  },
];
