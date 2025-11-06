import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import { setApiClientToken } from '../api/client';

type Role = 'Admin' | 'Client' | null;

interface AuthState {
  token: string | null;
  role: Role;
  email: string | null;
  setAuth: (auth: { token: string; role: Role; email: string }) => void;
  clearAuth: () => void;
}

const AuthContext = createContext<AuthState | undefined>(undefined);

const STORAGE_KEY = 'requirement-agent-auth';

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [token, setToken] = useState<string | null>(null);
  const [role, setRole] = useState<Role>(null);
  const [email, setEmail] = useState<string | null>(null);

  useEffect(() => {
    const stored = window.sessionStorage.getItem(STORAGE_KEY);
    if (stored) {
      try {
        const parsed = JSON.parse(stored) as { token: string; role: Role; email: string };
        setToken(parsed.token);
        setRole(parsed.role);
        setEmail(parsed.email);
        setApiClientToken(parsed.token);
      } catch (error) {
        console.error('Failed to parse stored auth state', error);
        window.sessionStorage.removeItem(STORAGE_KEY);
      }
    }
  }, []);

  const setAuth = ({ token: newToken, role: newRole, email: newEmail }: { token: string; role: Role; email: string }) => {
    setToken(newToken);
    setRole(newRole);
    setEmail(newEmail);
    setApiClientToken(newToken);
    window.sessionStorage.setItem(STORAGE_KEY, JSON.stringify({ token: newToken, role: newRole, email: newEmail }));
  };

  const clearAuth = () => {
    setToken(null);
    setRole(null);
    setEmail(null);
    setApiClientToken(null);
    window.sessionStorage.removeItem(STORAGE_KEY);
  };

  const value = useMemo(
    () => ({ token, role, email, setAuth, clearAuth }),
    [token, role, email],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthState => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }

  return context;
};
