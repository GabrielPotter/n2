import type { ComponentType } from "react";

export type RealmId = "n2-users" | "n2-system";

export type RealmConfig = {
  id: RealmId;
  label: string;
  description: string;
  clientId: string;
  menuPath: string;
};

export type DevelopmentUser = {
  realm: RealmId;
  username: string;
  label: string;
  description: string;
};

export type CatalogCategory = {
  categoryId: string;
  objectKind: string;
  name: string;
};

export type CatalogCategoriesResponse = {
  service: string;
  categories: CatalogCategory[];
};

export type CatalogType = {
  typeId: string;
  categoryId: string;
  name: string;
};

export type CatalogTypesResponse = {
  service: string;
  types: CatalogType[];
};

export type QueryObject = {
  id: string;
  name: string;
  objectKind: string;
  categoryId: string;
  categoryName: string;
  typeId: string;
  typeName: string;
  status: string;
};

export type QueryObjectsResponse = {
  service: string;
  objects: QueryObject[];
};

export type CreateObjectRequest = {
  name: string;
  categoryId: string;
  typeId: string;
};

export type CreateObjectResponse = {
  service: string;
  object: QueryObject;
};

export type KeycloakTokenResponse = {
  access_token: string;
  expires_in: number;
  refresh_expires_in: number;
  token_type: string;
  scope?: string;
};

export type SessionUser = {
  realm: RealmId;
  username: string;
  subject: string;
  tenantId?: string;
  roles: string[];
  authzVersion?: number;
  expiresAtUtc: string;
};

export type LoginRequest = {
  realm: RealmId;
  username: string;
  password: string;
};

export type AppSessionState = {
  realm: RealmId;
  username: string;
  password: string;
  accessToken: string;
  user?: SessionUser;
  signingIn: boolean;
};

export type AppDataState = {
  categories: CatalogCategory[];
  types: CatalogType[];
  objects: QueryObject[];
  error: string;
  loading: boolean;
  creatingObject: boolean;
};

export type AppContextValue = {
  session: AppSessionState;
  data: AppDataState;
  isAuthenticated: boolean;
  homePath: string;
  signIn: (request: LoginRequest) => Promise<void>;
  signOut: () => void;
  refreshData: () => Promise<void>;
  submitObject: (request: CreateObjectRequest) => Promise<void>;
};

export type MenuRouteDefinition = {
  path: string;
  title: string;
  description: string;
  tag: string;
  allowedRealms: RealmId[];
  icon: ComponentType;
  element: ComponentType;
};
