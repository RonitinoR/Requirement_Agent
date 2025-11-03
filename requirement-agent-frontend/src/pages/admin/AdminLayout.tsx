import { Logout } from '@mui/icons-material';
import { AppBar, Box, Button, Container, IconButton, Stack, Toolbar, Typography } from '@mui/material';
import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

const adminNavItems = [
  { label: 'Permit Types', path: '/admin/permit-types' },
  { label: 'Questions', path: '/admin/questions' },
  { label: 'Submissions', path: '/admin/submissions' },
];

const AdminLayout: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { clearAuth, email } = useAuth();

  const handleLogout = () => {
    clearAuth();
    navigate('/login', { replace: true });
  };

  return (
    <Box minHeight="100vh" display="flex" flexDirection="column">
      <AppBar position="static" color="primary">
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>
            Requirement Agent Admin
          </Typography>
          <Stack direction="row" spacing={1} alignItems="center">
            <Typography variant="body2" color="inherit">
              {email}
            </Typography>
            <IconButton color="inherit" onClick={handleLogout} size="small">
              <Logout />
            </IconButton>
          </Stack>
        </Toolbar>
      </AppBar>

      <Box component="nav" bgcolor="background.paper" borderBottom={1} borderColor="divider">
        <Container>
          <Stack direction="row" spacing={2} py={2}>
            {adminNavItems.map((item) => {
              const isActive = location.pathname.startsWith(item.path);
              return (
                <Button
                  key={item.path}
                  onClick={() => navigate(item.path)}
                  variant={isActive ? 'contained' : 'text'}
                  color={isActive ? 'primary' : 'inherit'}
                >
                  {item.label}
                </Button>
              );
            })}
            <Button onClick={() => navigate('/client')} color="inherit">
              Client Intake
            </Button>
          </Stack>
        </Container>
      </Box>

      <Container sx={{ flexGrow: 1, py: 4 }}>
        <Outlet />
      </Container>
    </Box>
  );
};

export default AdminLayout;
