import AssessmentIcon from "@mui/icons-material/Assessment";
import AssignmentIcon from "@mui/icons-material/Assignment";
import CategoryIcon from "@mui/icons-material/Category";
import GroupIcon from "@mui/icons-material/Group";
import HistoryIcon from "@mui/icons-material/History";
import HomeIcon from "@mui/icons-material/Home";
import Inventory2Icon from "@mui/icons-material/Inventory2";
import LocalOfferIcon from "@mui/icons-material/LocalOffer";
import LocalShippingIcon from "@mui/icons-material/LocalShipping";
import StorefrontIcon from "@mui/icons-material/Storefront";
import WarehouseIcon from "@mui/icons-material/Warehouse";

export const navigation = [
  {
    items: [
      { label: "Pocetna", to: "/", icon: HomeIcon },
      { label: "Nalozi", to: "/nalozi", icon: AssignmentIcon, permission: "orders.view" },
      { label: "Izvestaji", to: "/izvestaji", icon: AssessmentIcon, permission: "reports.view" },
    ],
  },
  {
    heading: "Zalihe",
    items: [
      { label: "Stanje zaliha", to: "/zalihe", icon: Inventory2Icon, permission: "stock.view" },
      { label: "Skladista", to: "/skladista", icon: WarehouseIcon, permission: "stock.view" },
      { label: "Kretanje robe", to: "/kretanja", icon: HistoryIcon, permission: "stock.view" },
    ],
  },
  {
    heading: "Sifarnici",
    items: [
      { label: "Proizvodi", to: "/proizvodi", icon: LocalOfferIcon, permission: "products.view" },
      { label: "Kategorije", to: "/kategorije", icon: CategoryIcon, permission: "categories.view" },
      { label: "Dobavljaci", to: "/dobavljaci", icon: LocalShippingIcon, permission: "suppliers.view" },
      { label: "Prodajni objekti", to: "/objekti", icon: StorefrontIcon, permission: "stores.view" },
    ],
  },
  {
    heading: "Administracija",
    items: [
      {
        label: "Korisnici i uloge",
        to: "/korisnici",
        icon: GroupIcon,
        permission: "users.view",
        badgeKey: "pendingUsers",
      },
    ],
  },
];
