import { Navigate, Route, Routes } from 'react-router-dom';
import RequireAuth from './components/RequireAuth';
import { useAuth } from './context/AuthContext';
import LoginPage from './pages/LoginPage';
import AdminLayout from './pages/admin/AdminLayout';
import PermitTypesPage from './pages/admin/PermitTypesPage';
import QuestionsPage from './pages/admin/QuestionsPage';
import SubmissionsPage from './pages/admin/SubmissionsPage';
import ClientIntakePage from './pages/client/ClientIntakePage';

const App: React.FC = () => {
  const { role } = useAuth();

  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route element={<RequireAuth allowedRoles={['Admin']} />}>
        <Route path="/admin" element={<AdminLayout />}>
          <Route index element={<Navigate to="permit-types" replace />} />
          <Route path="permit-types" element={<PermitTypesPage />} />
          <Route path="questions" element={<QuestionsPage />} />
          <Route path="submissions" element={<SubmissionsPage />} />
        </Route>
      </Route>

      <Route element={<RequireAuth allowedRoles={['Client', 'Admin']} />}>
        <Route path="/client" element={<ClientIntakePage />} />
      </Route>

      <Route path="/" element={<Navigate to={role === 'Admin' ? '/admin' : '/client'} replace />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};

export default App;