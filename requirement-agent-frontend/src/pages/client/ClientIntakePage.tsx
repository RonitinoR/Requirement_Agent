import { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CardHeader,
  Checkbox,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import { useMutation, useQuery } from '@tanstack/react-query';
import { apiClient } from '../../api/client';
import { useAuth } from '../../context/AuthContext';

type QuestionType = 'Text' | 'TextArea' | 'Select' | 'MultiSelect';

interface PermitTypeDto {
  id: string;
  name: string;
}

interface QuestionDto {
  id: string;
  order: number;
  key: string;
  prompt: string;
  type: QuestionType;
  optionsJson?: string | null;
  required: boolean;
}

const ClientIntakePage: React.FC = () => {
  const { email: authenticatedEmail } = useAuth();
  const [permitTypeId, setPermitTypeId] = useState<string>('');
  const [clientEmail, setClientEmail] = useState(authenticatedEmail ?? '');
  const [projectName, setProjectName] = useState('');
  const [answers, setAnswers] = useState<Record<string, unknown>>({});
  const [selectedFiles, setSelectedFiles] = useState<FileList | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const { data: permitTypes } = useQuery<PermitTypeDto[]>({
    queryKey: ['permit-types-client'],
    queryFn: async () => {
      const response = await apiClient.get('/api/permittypes');
      return response.data;
    },
  });

  useEffect(() => {
    if (permitTypes && permitTypes.length > 0 && !permitTypeId) {
      setPermitTypeId(permitTypes[0].id);
    }
  }, [permitTypes, permitTypeId]);

  const { data: questions } = useQuery<QuestionDto[]>({
    queryKey: ['client-questions', permitTypeId],
    enabled: Boolean(permitTypeId),
    queryFn: async () => {
      const response = await apiClient.get(`/api/permittypes/${permitTypeId}/questions`);
      return response.data;
    },
  });

  const submissionMutation = useMutation({
    mutationFn: async () => {
      if (!permitTypeId) {
        throw new Error('Please select a permit type.');
      }

      const submissionResponse = await apiClient.post('/api/submissions', {
        permitTypeId,
        clientEmail,
        projectName,
        answersJson: JSON.stringify(answers),
      });

      const submissionId = submissionResponse.data.id ?? submissionResponse.data?.value?.id;

      if (selectedFiles && selectedFiles.length > 0) {
        for (const file of Array.from(selectedFiles)) {
          const formData = new FormData();
          formData.append('file', file);
          await apiClient.post(`/api/submissions/${submissionId}/uploads`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
          });
        }
      }

      return submissionId as string;
    },
    onSuccess: (submissionId) => {
      setSuccessMessage(`Submission ${submissionId} saved successfully.`);
      setErrorMessage(null);
      setAnswers({});
      setProjectName('');
      setSelectedFiles(null);
    },
    onError: (error) => {
      console.error(error);
      setSuccessMessage(null);
      setErrorMessage('Unable to submit at this time. Please review the form and try again.');
    },
  });

  const handleAnswerChange = (key: string, value: unknown) => {
    setAnswers((prev) => ({ ...prev, [key]: value }));
  };

  const parseOptions = (optionsJson?: string | null): string[] => {
    if (!optionsJson) {
      return [];
    }
    try {
      const parsed = JSON.parse(optionsJson);
      return Array.isArray(parsed) ? (parsed as string[]) : [];
    } catch (error) {
      console.warn('Failed to parse options JSON', error);
      return [];
    }
  };

  const renderField = (question: QuestionDto) => {
    const value = answers[question.key] ?? (question.type === 'MultiSelect' ? [] : '');
    const options = parseOptions(question.optionsJson);

    switch (question.type) {
      case 'TextArea':
        return (
          <TextField
            multiline
            minRows={3}
            label={question.prompt}
            value={value as string}
            onChange={(event) => handleAnswerChange(question.key, event.target.value)}
            required={question.required}
            fullWidth
          />
        );
      case 'Select':
        return (
          <TextField
            select
            label={question.prompt}
            value={(value as string) || ''}
            onChange={(event) => handleAnswerChange(question.key, event.target.value)}
            required={question.required}
            fullWidth
          >
            {options.map((option) => (
              <MenuItem key={option} value={option}>
                {option}
              </MenuItem>
            ))}
          </TextField>
        );
      case 'MultiSelect':
        return (
          <FormControl fullWidth>
            <InputLabel>{question.prompt}</InputLabel>
            <Select<string[]>
              multiple
              value={(value as string[]) ?? []}
              label={question.prompt}
              onChange={(event) => {
                const selected = event.target.value as string[];
                handleAnswerChange(question.key, selected);
              }}
              renderValue={(selected) => (selected as string[]).join(', ')}
            >
              {options.map((option) => (
                <MenuItem key={option} value={option}>
                  <Checkbox checked={((value as string[]) ?? []).includes(option)} sx={{ mr: 1 }} />
                  <Typography>{option}</Typography>
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        );
      default:
        return (
          <TextField
            label={question.prompt}
            value={(value as string) ?? ''}
            onChange={(event) => handleAnswerChange(question.key, event.target.value)}
            required={question.required}
            fullWidth
          />
        );
    }
  };

  const selectedPermitName = useMemo(
    () => permitTypes?.find((pt) => pt.id === permitTypeId)?.name ?? '',
    [permitTypes, permitTypeId],
  );

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    setSuccessMessage(null);
    setErrorMessage(null);
    submissionMutation.mutate();
  };

  return (
    <Box py={4}>
      <Stack spacing={3}>
        <Typography variant="h4">Client Intake</Typography>

        <Paper sx={{ p: 3 }}>
          <Stack direction={{ xs: 'column', md: 'row' }} spacing={2} alignItems={{ xs: 'stretch', md: 'flex-end' }}>
            <TextField
              select
              label="Permit Type"
              value={permitTypeId}
              onChange={(event) => setPermitTypeId(event.target.value)}
              sx={{ minWidth: 240 }}
            >
              {permitTypes?.map((pt) => (
                <MenuItem key={pt.id} value={pt.id}>
                  {pt.name}
                </MenuItem>
              ))}
            </TextField>
            <TextField
              label="Client Email"
              value={clientEmail}
              onChange={(event) => setClientEmail(event.target.value)}
              type="email"
              required
            />
            <TextField
              label="Project Name"
              value={projectName}
              onChange={(event) => setProjectName(event.target.value)}
              required
            />
          </Stack>
        </Paper>

        {questions && questions.length > 0 ? (
          <Box component="form" onSubmit={handleSubmit}>
            <Card>
              <CardHeader title={selectedPermitName} subheader="Answer the questions below" />
              <CardContent>
                <Stack spacing={3}>
                  {questions
                    .slice()
                    .sort((a, b) => a.order - b.order)
                    .map((question) => (
                      <Box key={question.id}>{renderField(question)}</Box>
                    ))}
                  <Stack spacing={1}>
                    <Typography variant="subtitle1">Supporting Files</Typography>
                    <input
                      type="file"
                      multiple
                      onChange={(event) => setSelectedFiles(event.target.files)}
                    />
                  </Stack>
                  {successMessage && <Alert severity="success">{successMessage}</Alert>}
                  {errorMessage && <Alert severity="error">{errorMessage}</Alert>}
                  <Button type="submit" variant="contained" disabled={submissionMutation.isPending}>
                    {submissionMutation.isPending ? 'Submitting...' : 'Submit Intake'}
                  </Button>
                </Stack>
              </CardContent>
            </Card>
          </Box>
        ) : (
          <Alert severity="info">Select a permit type to load its questions.</Alert>
        )}
      </Stack>
    </Box>
  );
};

export default ClientIntakePage;
