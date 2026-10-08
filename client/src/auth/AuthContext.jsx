import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import { authApi } from "../api/endpoints";
import { setSessionExpiredHandler, tokenStore } from "../api/client";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  const signOut = useCallback(async () => {
    const refreshToken = tokenStore.refresh;
    if (refreshToken) {
      try {
        await authApi.logout(refreshToken);
      } catch {}
    }
    tokenStore.clear();
    setUser(null);
  }, []);

  useEffect(() => {
    setSessionExpiredHandler(() => setUser(null));
  }, []);

  useEffect(() => {
    if (!tokenStore.access) {
      setLoading(false);
      return;
    }

    authApi
      .me()
      .then(setUser)
      .catch((err) => {
        if ([400, 401, 403].includes(err.response?.status)) tokenStore.clear();
      })
      .finally(() => setLoading(false));
  }, []);

  const signIn = useCallback(async (email, password) => {
    const result = await authApi.login({ email, password });
    tokenStore.set(result);
    setUser(result.user);
    return result.user;
  }, []);

  const applySession = useCallback((result) => {
    tokenStore.set(result);
    setUser(result.user);
  }, []);

  const value = useMemo(
    () => ({
      user,
      loading,
      signIn,
      signOut,
      updateUser: setUser,
      applySession,
      can: (permission) => Boolean(user?.permissions?.includes(permission)),
    }),
    [user, loading, signIn, signOut, applySession],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth mora biti unutar AuthProvider-a.");
  return context;
}
