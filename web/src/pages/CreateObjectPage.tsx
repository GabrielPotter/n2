import { FormEvent, useMemo, useState } from "react";
import { Alert, Box, Button, MenuItem, Stack, TextField } from "@mui/material";
import AddCircleRoundedIcon from "@mui/icons-material/AddCircleRounded";
import { PageSection } from "../components/PageSection";
import { useAppContext } from "../app/AppProvider";
import type { CreateObjectRequest } from "../types";

const initialForm: CreateObjectRequest = {
  name: "",
  categoryId: "",
  typeId: ""
};

export function CreateObjectPage() {
  const { data, submitObject } = useAppContext();
  const [form, setForm] = useState<CreateObjectRequest>(initialForm);
  const [successMessage, setSuccessMessage] = useState("");

  const filteredTypes = useMemo(
    () => data.types.filter((type) => type.categoryId === form.categoryId),
    [data.types, form.categoryId]
  );

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    try {
      await submitObject(form);
      setForm(initialForm);
      setSuccessMessage("Object created successfully.");
    } catch {
      return;
    }
  }

  return (
    <PageSection
      title="Create object"
      description="This page sends the object creation request through the gateway to the editor service."
      error={data.error}
    >
      <Box component="form" onSubmit={(event) => void handleSubmit(event)} sx={{ maxWidth: 560 }}>
        <Stack spacing={2}>
          {successMessage ? <Alert severity="success">{successMessage}</Alert> : null}
          <TextField
            label="Name"
            value={form.name}
            onChange={(event) => {
              setSuccessMessage("");
              setForm((current) => ({ ...current, name: event.target.value }));
            }}
            required
            fullWidth
          />
          <TextField
            select
            label="Category"
            value={form.categoryId}
            onChange={(event) => {
              setSuccessMessage("");
              const categoryId = event.target.value;
              const nextTypeId = data.types.find((type) => type.categoryId === categoryId)?.typeId ?? "";
              setForm((current) => ({ ...current, categoryId, typeId: nextTypeId }));
            }}
            required
            fullWidth
          >
            {data.categories.map((category) => (
              <MenuItem key={category.categoryId} value={category.categoryId}>
                {category.name} ({category.objectKind})
              </MenuItem>
            ))}
          </TextField>
          <TextField
            select
            label="Type"
            value={form.typeId}
            onChange={(event) => {
              setSuccessMessage("");
              setForm((current) => ({ ...current, typeId: event.target.value }));
            }}
            required
            fullWidth
          >
            {filteredTypes.map((type) => (
              <MenuItem key={type.typeId} value={type.typeId}>
                {type.name}
              </MenuItem>
            ))}
          </TextField>
          <Button type="submit" variant="contained" startIcon={<AddCircleRoundedIcon />} disabled={data.creatingObject}>
            {data.creatingObject ? "Creating..." : "Create object"}
          </Button>
        </Stack>
      </Box>
    </PageSection>
  );
}
