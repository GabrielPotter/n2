import type {
  CatalogCategoriesResponse,
  CatalogTypesResponse,
  CreateObjectRequest,
  CreateObjectResponse,
  KeycloakTokenResponse,
  QueryObjectsResponse,
  RealmId,
  SessionUser
} from "./types";
import { getRealmConfig } from "./realms";

const gatewayBaseUrl = "http://localhost:5100";
const keycloakBaseUrl = "http://localhost:8081";

async function readError(response: Response): Promise<string> {
  const contentType = response.headers.get("content-type") ?? "";

  if (contentType.includes("application/json")) {
    const payload = (await response.json()) as { title?: string; detail?: string; message?: string; error?: string };
    return payload.detail ?? payload.title ?? payload.message ?? payload.error ?? `Request failed with ${response.status}`;
  }

  const text = await response.text();
  return text || `Request failed with ${response.status}`;
}

async function getJson<T>(path: string, accessToken: string): Promise<T> {
  const response = await fetch(`${gatewayBaseUrl}${path}`, {
    headers: {
      Authorization: `Bearer ${accessToken}`
    }
  });

  if (!response.ok) {
    throw new Error(await readError(response));
  }

  return (await response.json()) as T;
}

async function postJson<TRequest, TResponse>(path: string, body: TRequest, accessToken: string): Promise<TResponse> {
  const response = await fetch(`${gatewayBaseUrl}${path}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${accessToken}`
    },
    body: JSON.stringify(body)
  });

  if (!response.ok) {
    throw new Error(await readError(response));
  }

  return (await response.json()) as TResponse;
}

export async function loginWithPassword(realm: RealmId, username: string, password: string): Promise<KeycloakTokenResponse> {
  const realmConfig = getRealmConfig(realm);
  const body = new URLSearchParams({
    grant_type: "password",
    client_id: realmConfig.clientId,
    username,
    password
  });

  const response = await fetch(`${keycloakBaseUrl}/realms/${realm}/protocol/openid-connect/token`, {
    method: "POST",
    headers: {
      "Content-Type": "application/x-www-form-urlencoded"
    },
    body: body.toString()
  });

  if (!response.ok) {
    throw new Error(await readError(response));
  }

  return (await response.json()) as KeycloakTokenResponse;
}

export function getCatalogCategories(accessToken: string): Promise<CatalogCategoriesResponse> {
  return getJson<CatalogCategoriesResponse>("/api/v1/catalog/categories", accessToken);
}

export function getCatalogTypes(accessToken: string): Promise<CatalogTypesResponse> {
  return getJson<CatalogTypesResponse>("/api/v1/catalog/types", accessToken);
}

export function getQueryObjects(accessToken: string): Promise<QueryObjectsResponse> {
  return getJson<QueryObjectsResponse>("/api/v1/query/objects", accessToken);
}

export function createObject(request: CreateObjectRequest, accessToken: string): Promise<CreateObjectResponse> {
  return postJson<CreateObjectRequest, CreateObjectResponse>("/api/v1/editor/object", request, accessToken);
}

type JwtPayload = {
  sub?: string;
  preferred_username?: string;
  email?: string;
  tenant_id?: string;
  tenant_name?: string;
  authz_version?: string | number;
  roles?: string[] | string;
  exp?: number;
};

function decodeJwtPayload(token: string): JwtPayload {
  const [, payload] = token.split(".");

  if (!payload) {
    throw new Error("Access token is malformed.");
  }

  const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
  const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, "=");

  return JSON.parse(window.atob(padded)) as JwtPayload;
}

export function readSessionUser(realm: RealmId, username: string, accessToken: string): SessionUser {
  const payload = decodeJwtPayload(accessToken);
  const roles = Array.isArray(payload.roles)
    ? payload.roles
    : typeof payload.roles === "string" && payload.roles.length > 0
      ? [payload.roles]
      : [];
  const authzVersion =
    payload.authz_version === undefined
      ? undefined
      : typeof payload.authz_version === "number"
      ? payload.authz_version
      : Number.parseInt(payload.authz_version ?? "0", 10);

  if (!payload.sub) {
    throw new Error("Keycloak access token is missing required application claims.");
  }

  if (realm === "n2-users" && (!payload.tenant_name || !Number.isFinite(authzVersion))) {
    throw new Error("Keycloak access token is missing required application claims.");
  }

  return {
    realm,
    username: username || payload.email || payload.preferred_username || payload.sub,
    subject: payload.sub,
    tenantId: payload.tenant_id,
    tenantName: payload.tenant_name,
    roles,
    authzVersion,
    expiresAtUtc: payload.exp ? new Date(payload.exp * 1000).toISOString() : ""
  };
}
