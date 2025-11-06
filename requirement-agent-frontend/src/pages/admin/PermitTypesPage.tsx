import { useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../../api/client';

interface PermitTypeDto {
  id: string;
  name: string;
  description?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

const PermitTypesPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [formValues, setFormValues] = useState({ name: '', description: '' });
  const [error, setError] = useState<string | null>(null);

  // Mock data for demo
  const mockData: PermitTypeDto[] = [
    {
      id: '1',
      name: 'Adopt-A-Highway',
      description: 'Highway adoption and maintenance program',
      isActive: true,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    },
    {
      id: '2', 
      name: 'Building Permit',
      description: 'Commercial and residential building permits',
      isActive: true,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    }
  ];

  const { data, isLoading } = useQuery<PermitTypeDto[]>({
    queryKey: ['permit-types'],
    queryFn: async () => {
      try {
        const response = await apiClient.get('/api/permittypes', { params: { includeInactive: true } });
        return response.data;
      } catch (error) {
        // Return mock data if API fails
        console.log('Using mock data for demo');
        return mockData;
      }
    },
  });

  const createMutation = useMutation({
    mutationFn: async () => {
      await apiClient.post('/api/permittypes', {
        name: formValues.name,
        description: formValues.description || null,
        isActive: true,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['permit-types'] });
      setFormValues({ name: '', description: '' });
      setError(null);
    },
    onError: (err) => {
      console.error(err);
      setError('Unable to create permit type. Please verify the information and try again.');
    },
  });

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    if (!formValues.name.trim()) {
      setError('Name is required.');
      return;
    }
    createMutation.mutate();
  };

  return (
    <Stack spacing={4}>
      <Box component={Paper} padding={3}>
        <Typography variant="h6" gutterBottom>
          Add Permit Type
        </Typography>
        <Box component="form" onSubmit={handleSubmit}>
          <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} alignItems={{ xs: 'stretch', md: 'flex-end' }}>
            <TextField
              label="Name"
              value={formValues.name}
              onChange={(event) => setFormValues((prev) => ({ ...prev, name: event.target.value }))}
              required
              sx={{ minWidth: 220 }}
            />
            <TextField
              label="Description"
              value={formValues.description}
              onChange={(event) => setFormValues((prev) => ({ ...prev, description: event.target.value }))}
              fullWidth
            />
            <Button type="submit" variant="contained" disabled={createMutation.isPending}>
              {createMutation.isPending ? 'Saving...' : 'Save'}
            </Button>
          </Stack>
        </Box>
        {error && (
          <Alert severity="error" sx={{ mt: 2 }}>
            {error}
          </Alert>
        )}
      </Box>

      <Box component={Paper} padding={3}>
        <Typography variant="h6" gutterBottom>
          Permit Types
        </Typography>
        {isLoading ? (
          <Typography>Loading...</Typography>
        ) : (
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Description</TableCell>
                <TableCell>Status</TableCell>
                <TableCell>Updated</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.map((item) => (
                <TableRow key={item.id} hover>
                  <TableCell>{item.name}</TableCell>
                  <TableCell>{item.description ?? '—'}</TableCell>
                  <TableCell>{item.isActive ? 'Active' : 'Inactive'}</TableCell>
                  <TableCell>{new Date(item.updatedAt).toLocaleString()}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </Box>
    </Stack>
  );
};

export default PermitTypesPage;
