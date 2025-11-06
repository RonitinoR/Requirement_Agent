import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

type AllowedRole = 'Admin' | 'Client';

interface Props {
  allowedRoles?: AllowedRole[];
}

const RequireAuth: React.FC<Props> = ({ allowedRoles }) => {
  const { token, role } = useAuth();
  const location = useLocation();

  if (!token) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (allowedRoles && role && !allowedRoles.includes(role)) {
    return <Navigate to="/client" replace />;
  }

  return <Outlet />;
};

export default RequireAuth;
