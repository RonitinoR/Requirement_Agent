import { useState } from 'react';
import {
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  TextField,
  Typography,
} from '@mui/material';

const QuestionsPage: React.FC = () => {
  const [selectedPermit, setSelectedPermit] = useState<string>('');

  return (
    <Stack spacing={4}>
      <Paper sx={{ p: 3 }}>
        <Stack spacing={2}>
          <Typography variant="h6">Configure Questions</Typography>
          <FormControl sx={{ minWidth: 240 }}>
            <InputLabel id="permit-select-label">Permit Type</InputLabel>
            <Select
              labelId="permit-select-label"
              label="Permit Type"
              value={selectedPermit}
              onChange={(event) => setSelectedPermit(event.target.value)}
            >
              <MenuItem value="">Select a permit type first</MenuItem>
            </Select>
          </FormControl>

          <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} alignItems={{ xs: 'stretch', md: 'flex-end' }}>
            <TextField label="Order" type="number" sx={{ width: 120 }} />
            <TextField label="Key" required />
            <TextField label="Prompt" required fullWidth />
            <FormControl sx={{ minWidth: 160 }}>
              <InputLabel>Type</InputLabel>
              <Select label="Type">
                <MenuItem value="Text">Text</MenuItem>
                <MenuItem value="TextArea">TextArea</MenuItem>
                <MenuItem value="Select">Select</MenuItem>
                <MenuItem value="MultiSelect">MultiSelect</MenuItem>
              </Select>
            </FormControl>
            <Button variant="contained" disabled>
              Add Question
            </Button>
          </Stack>
        </Stack>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Questions Preview
        </Typography>
        <Typography color="text.secondary">
          Create permit types first, then add questions to them.
        </Typography>
      </Paper>
    </Stack>
  );
};

export default QuestionsPage;
