import { Alert, Box, Paper, Typography } from "@mui/material";
import { PropsWithChildren, ReactNode } from "react";

type PageSectionProps = PropsWithChildren<{
  title: string;
  description?: string;
  action?: ReactNode;
  error?: string;
}>;

export function PageSection({ title, description, action, error, children }: PageSectionProps) {
  return (
    <Paper
      elevation={0}
      sx={{
        p: { xs: 2.5, md: 3 },
        borderRadius: 4,
        border: "1px solid rgba(21, 32, 43, 0.08)",
        backgroundColor: "rgba(255, 255, 255, 0.82)",
        backdropFilter: "blur(10px)"
      }}
    >
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          gap: 2,
          alignItems: { xs: "flex-start", sm: "center" },
          flexDirection: { xs: "column", sm: "row" },
          mb: 2.5
        }}
      >
        <Box>
          <Typography variant="h5">{title}</Typography>
          {description ? (
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.75 }}>
              {description}
            </Typography>
          ) : null}
        </Box>
        {action}
      </Box>
      {error ? (
        <Alert severity="error" sx={{ mb: 2.5 }}>
          {error}
        </Alert>
      ) : null}
      {children}
    </Paper>
  );
}
