import { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Checkbox,
  FormControl,
  FormControlLabel,
  InputLabel,
  MenuItem,
  Paper,
  Select,
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
}

type QuestionType = 'Text' | 'TextArea' | 'Select' | 'MultiSelect';

interface QuestionDto {
  id: string;
  permitTypeId: string;
  order: number;
  key: string;
  prompt: string;
  type: QuestionType;
  optionsJson?: string | null;
  required: boolean;
}

const questionTypeOptions: QuestionType[] = ['Text', 'TextArea', 'Select', 'MultiSelect'];

const QuestionsPage: React.FC = () => {
  const queryClient = useQueryClient();
  const [selectedPermit, setSelectedPermit] = useState<string>('');
  const [error, setError] = useState<string | null>(null);
  const [formValues, setFormValues] = useState({
    order: 0,
    key: '',
    prompt: '',
    type: 'Text' as QuestionType,
    optionsJson: '',
    required: true,
  });

  const { data: permitTypes } = useQuery<PermitTypeDto[]>({
    queryKey: ['permit-types-active'],
    queryFn: async () => {
      const response = await apiClient.get('/api/permittypes', { params: { includeInactive: false } });
      return response.data;
    },
  });

  useEffect(() => {
    if (permitTypes && permitTypes.length > 0 && !selectedPermit) {
      setSelectedPermit(permitTypes[0].id);
    }
  }, [permitTypes, selectedPermit]);

  const { data: questions, isLoading: isLoadingQuestions } = useQuery<QuestionDto[]>({
    queryKey: ['questions', selectedPermit],
    enabled: Boolean(selectedPermit),
    queryFn: async () => {
      const response = await apiClient.get(`/api/permittypes/${selectedPermit}/questions`);
      return response.data;
    },
  });

  const createQuestionMutation = useMutation({
    mutationFn: async () => {
      if (!selectedPermit) {
        throw new Error('Select a permit type before adding questions.');
      }
      await apiClient.post(`/api/permittypes/${selectedPermit}/questions`, {
        permitTypeId: selectedPermit,
        order: Number(formValues.order),
        key: formValues.key,
        prompt: formValues.prompt,
        type: formValues.type,
        optionsJson: formValues.optionsJson || null,
        required: formValues.required,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['questions', selectedPermit] });
      setFormValues({ order: 0, key: '', prompt: '', type: 'Text', optionsJson: '', required: true });
      setError(null);
    },
    onError: (err) => {
      console.error(err);
      setError('Unable to save the question. Please review the values and try again.');
    },
  });

  const selectedPermitName = useMemo(
    () => permitTypes?.find((pt) => pt.id === selectedPermit)?.name ?? '?',
    [permitTypes, selectedPermit],
  );

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    if (!formValues.key.trim() || !formValues.prompt.trim()) {
      setError('Key and prompt are required.');
      return;
    }
    createQuestionMutation.mutate();
  };

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
              {permitTypes?.map((pt) => (
                <MenuItem key={pt.id} value={pt.id}>
                  {pt.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <Box component="form" onSubmit={handleSubmit}>
            <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}
              alignItems={{ xs: 'stretch', md: 'flex-end' }}>
              <TextField
                label="Order"
                type="number"
                value={formValues.order}
                onChange={(event) => setFormValues((prev) => ({ ...prev, order: Number(event.target.value) }))}
                sx={{ width: 120 }}
              />
              <TextField
                label="Key"
                value={formValues.key}
                onChange={(event) => setFormValues((prev) => ({ ...prev, key: event.target.value }))}
                required
              />
              <TextField
                label="Prompt"
                value={formValues.prompt}
                onChange={(event) => setFormValues((prev) => ({ ...prev, prompt: event.target.value }))}
                required
                fullWidth
              />
              <FormControl sx={{ minWidth: 160 }}>
                <InputLabel id="question-type-label">Type</InputLabel>
                <Select
                  labelId="question-type-label"
                  label="Type"
                  value={formValues.type}
                  onChange={(event) => setFormValues((prev) => ({ ...prev, type: event.target.value as QuestionType }))}
                >
                  {questionTypeOptions.map((type) => (
                    <MenuItem key={type} value={type}>
                      {type}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              <TextField
                label="Options JSON (for selects)"
                value={formValues.optionsJson}
                onChange={(event) => setFormValues((prev) => ({ ...prev, optionsJson: event.target.value }))}
                placeholder='["Option A", "Option B"]'
                fullWidth
              />
              <FormControlLabel
                control={
                  <Checkbox
                    checked={formValues.required}
                    onChange={(event) => setFormValues((prev) => ({ ...prev, required: event.target.checked }))}
                  />
                }
                label="Required"
              />
              <Button type="submit" variant="contained" disabled={createQuestionMutation.isPending || !selectedPermit}>
                {createQuestionMutation.isPending ? 'Saving...' : 'Add Question'}
              </Button>
            </Stack>
          </Box>
          {error && <Alert severity="error">{error}</Alert>}
        </Stack>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Blueprint ? {selectedPermitName}
        </Typography>
        {isLoadingQuestions ? (
          <Typography>Loading...</Typography>
        ) : (
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Order</TableCell>
                <TableCell>Key</TableCell>
                <TableCell>Prompt</TableCell>
                <TableCell>Type</TableCell>
                <TableCell>Required</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {questions?.map((question) => (
                <TableRow key={question.id} hover>
                  <TableCell>{question.order}</TableCell>
                  <TableCell>{question.key}</TableCell>
                  <TableCell>{question.prompt}</TableCell>
                  <TableCell>{question.type}</TableCell>
                  <TableCell>{question.required ? 'Yes' : 'No'}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </Paper>
    </Stack>
  );
};

export default QuestionsPage;
