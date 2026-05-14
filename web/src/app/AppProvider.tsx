import { PropsWithChildren, useEffect, useState } from "react";
import { createContext, useContext } from "react";
import {
  createObject,
  getCatalogCategories,
  getCatalogTypes,
  getQueryObjects,
  loginWithPassword,
  readSessionUser
} from "../api";
import { defaultRealm, getHomePathForRealm } from "../realms";
import type {
  AppContextValue,
  AppDataState,
  AppSessionState,
  CreateObjectRequest,
  LoginRequest
} from "../types";

const authStorageKey = "n2.dev.auth";

const initialDataState: AppDataState = {
  categories: [],
  types: [],
  objects: [],
  error: "",
  loading: false,
  creatingObject: false
};

const initialSessionState: AppSessionState = {
  realm: defaultRealm,
  username: "",
  password: "Password123!",
  accessToken: "",
  user: undefined,
  signingIn: false
};

const AppContext = createContext<AppContextValue | null>(null);

function readStoredSession(): Partial<AppSessionState> {
  const stored = window.localStorage.getItem(authStorageKey);

  if (!stored) {
    return {};
  }

  try {
    return JSON.parse(stored) as Partial<AppSessionState>;
  } catch {
    return {};
  }
}

export function AppProvider({ children }: PropsWithChildren) {
  const storedSession = readStoredSession();
  const [session, setSession] = useState<AppSessionState>({
    ...initialSessionState,
    realm: storedSession.realm ?? defaultRealm,
    username: storedSession.username ?? "",
    accessToken: storedSession.accessToken ?? "",
    user: storedSession.user
  });
  const [data, setData] = useState<AppDataState>(initialDataState);

  useEffect(() => {
    window.localStorage.setItem(
      authStorageKey,
        JSON.stringify({
        realm: session.realm,
        username: session.username,
        accessToken: session.accessToken,
        user: session.user
      })
    );
  }, [session.accessToken, session.realm, session.user, session.username]);

  async function refreshData() {
    if (!session.accessToken) {
      setData(initialDataState);
      return;
    }

    if (session.realm !== "n2-users") {
      setData(initialDataState);
      return;
    }

    setData((current) => ({ ...current, loading: true, error: "" }));

    try {
      const [categoryResponse, typeResponse, objectResponse] = await Promise.all([
        getCatalogCategories(session.accessToken),
        getCatalogTypes(session.accessToken),
        getQueryObjects(session.accessToken)
      ]);

      setData({
        categories: categoryResponse.categories,
        types: typeResponse.types,
        objects: objectResponse.objects,
        error: "",
        loading: false,
        creatingObject: false
      });
    } catch (error) {
      const message = error instanceof Error ? error.message : "Request failed.";

      setData((current) => ({
        ...current,
        error: message,
        loading: false
      }));

      if (message.includes("401") || message.includes("Unauthorized")) {
        signOut();
      }
    }
  }

  useEffect(() => {
    void refreshData();
  }, [session.accessToken]);

  async function signIn(request: LoginRequest) {
    setSession((current) => ({
      ...current,
      realm: request.realm,
      username: request.username,
      password: request.password,
      signingIn: true
    }));
    setData((current) => ({ ...current, error: "" }));

    try {
      const token = await loginWithPassword(request.realm, request.username, request.password);
      const user = readSessionUser(request.realm, request.username, token.access_token);

      setSession((current) => ({
        ...current,
        realm: request.realm,
        username: request.username,
        password: request.password,
        accessToken: token.access_token,
        user,
        signingIn: false
      }));
    } catch (error) {
      setSession((current) => ({ ...current, signingIn: false }));
      setData((current) => ({
        ...current,
        error: error instanceof Error ? error.message : "Login failed."
      }));
      throw error;
    }
  }

  function signOut() {
    window.localStorage.removeItem(authStorageKey);
    setSession(initialSessionState);
    setData(initialDataState);
  }

  async function submitObject(request: CreateObjectRequest) {
    if (!session.accessToken) {
      throw new Error("No active session.");
    }

    setData((current) => ({ ...current, creatingObject: true, error: "" }));

    try {
      await createObject(request, session.accessToken);
      await refreshData();
    } catch (error) {
      setData((current) => ({
        ...current,
        creatingObject: false,
        error: error instanceof Error ? error.message : "Create object failed."
      }));
      throw error;
    }
  }

  const value: AppContextValue = {
    session,
    data,
    isAuthenticated: Boolean(session.accessToken),
    homePath: getHomePathForRealm(session.realm),
    signIn,
    signOut,
    refreshData,
    submitObject
  };

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>;
}

export function useAppContext() {
  const context = useContext(AppContext);

  if (!context) {
    throw new Error("App context is not available.");
  }

  return context;
}
