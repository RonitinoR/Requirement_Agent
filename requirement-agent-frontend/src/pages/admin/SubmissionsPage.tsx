import { useState } from 'react';
import {
  Button,
  IconButton,
  InputAdornment,
  MenuItem,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Tooltip,
  Typography,
} from '@mui/material';
import { Clear, Download } from '@mui/icons-material';
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../../api/client';

interface PermitTypeDto {
  id: string;
  name: string;
}

interface SubmissionDto {
  id: string;
  permitTypeId: string;
  permitTypeName: string;
  clientEmail: string;
  projectName: string;
  createdAt: string;
  updatedAt: string;
}

const SubmissionsPage: React.FC = () => {
  const [permitFilter, setPermitFilter] = useState<string>('');
  const [emailFilter, setEmailFilter] = useState<string>('');

  const { data: permitTypes } = useQuery<PermitTypeDto[]>({
    queryKey: ['permit-types'],
    queryFn: async () => {
      const response = await apiClient.get('/api/permittypes', { params: { includeInactive: true } });
      return response.data;
    },
  });

  const { data: submissions, isFetching } = useQuery<SubmissionDto[]>({
    queryKey: ['submissions', permitFilter, emailFilter],
    queryFn: async () => {
      const response = await apiClient.get('/api/submissions', {
        params: {
          permitTypeId: permitFilter || undefined,
          clientEmail: emailFilter || undefined,
        },
      });
      return response.data;
    },
  });

  const handleDownload = async (submissionId: string, path: string, fileName: string) => {
    const response = await apiClient.get(`/api/submissions/${submissionId}/generate/${path}`, {
      responseType: path === 'aipack' ? 'arraybuffer' : 'blob',
    });

    const blob = new Blob([response.data]);
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.click();
    window.URL.revokeObjectURL(url);
  };

  return (
    <Stack spacing={3}>
      <Paper sx={{ p: 3 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} alignItems={{ xs: 'stretch', md: 'flex-end' }}>
          <TextField
            select
            label="Permit Type"
            value={permitFilter}
            onChange={(event) => setPermitFilter(event.target.value)}
            sx={{ minWidth: 220 }}
          >
            <MenuItem value="">All</MenuItem>
            {permitTypes?.map((permit) => (
              <MenuItem key={permit.id} value={permit.id}>
                {permit.name}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            label="Client Email"
            value={emailFilter}
            onChange={(event) => setEmailFilter(event.target.value)}
            InputProps={{
              endAdornment: emailFilter ? (
                <InputAdornment position="end">
                  <IconButton onClick={() => setEmailFilter('')}>
                    <Clear fontSize="small" />
                  </IconButton>
                </InputAdornment>
              ) : undefined,
            }}
          />
          <Button variant="outlined" onClick={() => { setPermitFilter(''); setEmailFilter(''); }}>
            Reset
          </Button>
        </Stack>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Submissions {isFetching ? '(Refreshing...)' : ''}
        </Typography>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Project</TableCell>
              <TableCell>Permit Type</TableCell>
              <TableCell>Client Email</TableCell>
              <TableCell>Created</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {submissions?.map((submission) => (
              <TableRow key={submission.id} hover>
                <TableCell>{submission.projectName}</TableCell>
                <TableCell>{submission.permitTypeName}</TableCell>
                <TableCell>{submission.clientEmail}</TableCell>
                <TableCell>{new Date(submission.createdAt).toLocaleString()}</TableCell>
                <TableCell align="right">
                  <Stack direction="row" spacing={1} justifyContent="flex-end">
                    <Tooltip title="Use Case">
                      <IconButton size="small" onClick={() => handleDownload(submission.id, 'usecase', 'UseCase.md')}>
                        <Download fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="User Stories">
                      <IconButton size="small" onClick={() => handleDownload(submission.id, 'userstories', 'UserStories.md')}>
                        <Download fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Data Dictionary">
                      <IconButton size="small" onClick={() => handleDownload(submission.id, 'datadictionary', 'DataDictionary.csv')}>
                        <Download fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="AI Pack">
                      <IconButton size="small" onClick={() => handleDownload(submission.id, 'aipack', 'AI_Pack.zip')}>
                        <Download fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </Stack>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Paper>
    </Stack>
  );
};

export default SubmissionsPage;
