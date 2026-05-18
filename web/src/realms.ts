import type { DevelopmentUser, RealmConfig, RealmId } from "./types";

export const realmConfigs: RealmConfig[] = [
  {
    id: "n2-users",
    label: "Users realm",
    description: "Tenant-facing development workspace for application users and editors.",
    clientId: "n2-users-frontend",
    menuPath: "/menu/users"
  },
  {
    id: "n2-system",
    label: "System realm",
    description: "Platform administration workspace for system operators and support roles.",
    clientId: "n2-system-admin",
    menuPath: "/menu/system"
  }
];

export const developmentUsers: DevelopmentUser[] = [
  {
    realm: "n2-users",
    username: "admin@example.com",
    label: "Admin",
    description: "tenant-admin on Development Tenant"
  },
  {
    realm: "n2-users",
    username: "editor@example.com",
    label: "Editor",
    description: "editor on Development Tenant"
  },
  {
    realm: "n2-users",
    username: "viewer@example.com",
    label: "Viewer",
    description: "viewer on Development Tenant"
  },
  {
    realm: "n2-system",
    username: "platform-admin@example.com",
    label: "Platform admin",
    description: "platform-admin in the system realm"
  },
  {
    realm: "n2-system",
    username: "support-admin@example.com",
    label: "Support admin",
    description: "support-admin in the system realm"
  },
  {
    realm: "n2-system",
    username: "security-admin@example.com",
    label: "Security admin",
    description: "security-admin in the system realm"
  }
];

export const defaultRealm: RealmId = "n2-users";

export function getRealmConfig(realm: RealmId): RealmConfig {
  const config = realmConfigs.find((item) => item.id === realm);

  if (!config) {
    throw new Error(`Unknown realm: ${realm}`);
  }

  return config;
}

export function getHomePathForRealm(realm: RealmId): string {
  return getRealmConfig(realm).menuPath;
}

export function getDevelopmentUsersForRealm(realm: RealmId): DevelopmentUser[] {
  return developmentUsers.filter((user) => user.realm === realm);
}
