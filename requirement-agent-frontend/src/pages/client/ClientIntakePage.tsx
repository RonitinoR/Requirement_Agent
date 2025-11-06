import {
  Alert,
  Box,
  Card,
  CardContent,
  CardHeader,
  Stack,
  Typography,
} from '@mui/material';

const ClientIntakePage: React.FC = () => {
  return (
    <Box py={4}>
      <Stack spacing={3}>
        <Typography variant="h4">Client Intake</Typography>

        <Card>
          <CardHeader title="Welcome" subheader="Select a permit type to begin" />
          <CardContent>
            <Alert severity="info">
              The intake form will appear here once permit types and questions are configured by an administrator.
            </Alert>
          </CardContent>
        </Card>
      </Stack>
    </Box>
  );
};

export default ClientIntakePage;
