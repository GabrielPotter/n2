import { Navigate, Route, Routes } from "react-router-dom";
import type { ComponentType } from "react";
import { AppShell } from "../layout/AppShell";
import { routeDefinitions } from "../routes";
import { useAppContext } from "./AppProvider";
import { LoginPage } from "../pages/LoginPage";
import { SystemMenuPage } from "../pages/SystemMenuPage";
import { UsersMenuPage } from "../pages/UsersMenuPage";
import type { RealmId } from "../types";

function ProtectedRoute() {
  const { isAuthenticated } = useAppContext();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <AppShell />;
}

type RealmRouteProps = {
  allowedRealms: RealmId[];
  element: ComponentType;
};

function RealmRoute({ allowedRealms, element: Element }: RealmRouteProps) {
  const { homePath, session } = useAppContext();

  if (!allowedRealms.includes(session.realm)) {
    return <Navigate to={homePath} replace />;
  }

  return <Element />;
}

export function AppRoutes() {
  const { homePath, isAuthenticated } = useAppContext();

  return (
    <Routes>
      <Route path="/login" element={isAuthenticated ? <Navigate to={homePath} replace /> : <LoginPage />} />
      <Route element={<ProtectedRoute />}>
        <Route
          path="/menu/users"
          element={<RealmRoute allowedRealms={["n2-users"]} element={UsersMenuPage} />}
        />
        <Route
          path="/menu/system"
          element={<RealmRoute allowedRealms={["n2-system"]} element={SystemMenuPage} />}
        />
        {routeDefinitions.map((route) => (
          <Route
            key={route.path}
            path={route.path}
            element={<RealmRoute allowedRealms={route.allowedRealms} element={route.element} />}
          />
        ))}
      </Route>
      <Route path="*" element={<Navigate to={isAuthenticated ? homePath : "/login"} replace />} />
    </Routes>
  );
}
