import {
  Paper,
  Stack,
  Typography,
} from '@mui/material';

const SubmissionsPage: React.FC = () => {
  return (
    <Stack spacing={3}>
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Client Submissions
        </Typography>
        <Typography color="text.secondary">
          Client submissions will appear here once the database is connected and clients start submitting forms.
        </Typography>
      </Paper>
    </Stack>
  );
};

export default SubmissionsPage;
