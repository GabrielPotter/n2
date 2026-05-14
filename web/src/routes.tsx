import CategoryRoundedIcon from "@mui/icons-material/CategoryRounded";
import Inventory2RoundedIcon from "@mui/icons-material/Inventory2Rounded";
import AddBoxRoundedIcon from "@mui/icons-material/AddBoxRounded";
import BadgeRoundedIcon from "@mui/icons-material/BadgeRounded";
import { CategoriesPage } from "./pages/CategoriesPage";
import { CreateObjectPage } from "./pages/CreateObjectPage";
import { ObjectsPage } from "./pages/ObjectsPage";
import { SessionPage } from "./pages/SessionPage";
import type { MenuRouteDefinition, RealmId } from "./types";

export const routeDefinitions: MenuRouteDefinition[] = [
  {
    path: "/catalog/categories",
    title: "Catalog categories",
    description: "Browse the category definitions coming from the gateway catalog endpoints.",
    tag: "Catalog",
    allowedRealms: ["n2-users"],
    icon: CategoryRoundedIcon,
    element: CategoriesPage
  },
  {
    path: "/objects",
    title: "Objects",
    description: "Inspect the current object list returned by the query endpoints.",
    tag: "Query",
    allowedRealms: ["n2-users"],
    icon: Inventory2RoundedIcon,
    element: ObjectsPage
  },
  {
    path: "/objects/create",
    title: "Create object",
    description: "Open the editor workflow and create a new object with the selected catalog mapping.",
    tag: "Editor",
    allowedRealms: ["n2-users"],
    icon: AddBoxRoundedIcon,
    element: CreateObjectPage
  },
  {
    path: "/session",
    title: "Session details",
    description: "Review the active Keycloak-backed session and the mapped authorization claims.",
    tag: "Auth",
    allowedRealms: ["n2-users", "n2-system"],
    icon: BadgeRoundedIcon,
    element: SessionPage
  }
];

export function getMenuRoutesForRealm(realm: RealmId): MenuRouteDefinition[] {
  return routeDefinitions.filter((route) => route.allowedRealms.includes(realm));
}

export const routeLookup = Object.fromEntries(routeDefinitions.map((route) => [route.path, route])) as Record<
  string,
  MenuRouteDefinition
>;
