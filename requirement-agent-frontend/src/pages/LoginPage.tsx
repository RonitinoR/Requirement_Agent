import { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { Alert, Box, Button, Card, CardContent, CardHeader, Stack, TextField, Typography } from '@mui/material';
import { apiClient } from '../api/client';
import { useAuth } from '../context/AuthContext';

interface LocationState {
  from?: {
    pathname?: string;
  };
}

const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const state = location.state as LocationState | undefined;
  const redirectTo = state?.from?.pathname ?? '/';

  const { setAuth } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setLoading(true);
    setError(null);

    try {
      // Mock authentication for demo purposes
      if (email === 'admin@example.com' && password === 'Admin123!') {
        setAuth({ 
          token: 'demo-admin-token', 
          role: 'Admin', 
          email: 'admin@example.com' 
        });
        navigate(redirectTo, { replace: true });
        return;
      }
      
      if (email === 'client@example.com' && password === 'Client123!') {
        setAuth({ 
          token: 'demo-client-token', 
          role: 'Client', 
          email: 'client@example.com' 
        });
        navigate(redirectTo, { replace: true });
        return;
      }

      // Try real API if mock credentials don't match
      const response = await apiClient.post('/api/auth/login', { email, password });
      setAuth({ token: response.data.token, role: response.data.role, email: response.data.email });
      navigate(redirectTo, { replace: true });
    } catch (err) {
      console.error(err);
      setError('Login failed. Use demo credentials: admin@example.com / Admin123! or client@example.com / Client123!');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box display="flex" minHeight="100vh" alignItems="center" justifyContent="center" padding={2}>
      <Card sx={{ maxWidth: 420, width: '100%' }}>
        <CardHeader title="Requirement Agent" subheader="Sign in to continue" />
        <CardContent>
          <Box component="form" onSubmit={handleSubmit} noValidate>
            <Stack spacing={2}>
              {error && <Alert severity="error">{error}</Alert>}
              <TextField
                label="Email"
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                required
                fullWidth
              />
              <TextField
                label="Password"
                type="password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                required
                fullWidth
              />
              <Button variant="contained" type="submit" disabled={loading} fullWidth>
                {loading ? 'Signing in...' : 'Sign In'}
              </Button>
              <Typography variant="body2" color="text.secondary" textAlign="center">
                <strong>Demo Credentials:</strong><br />
                Admin: admin@example.com / Admin123!<br />
                Client: client@example.com / Client123!
              </Typography>
            </Stack>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default LoginPage;
