import { Box, Chip, Typography } from "@mui/material";
import { PageSection } from "../components/PageSection";
import { useAppContext } from "../app/AppProvider";

export function SessionPage() {
  const { session } = useAppContext();
  const user = session.user;

  return (
    <PageSection
      title="Session details"
      description="This page shows the application fields derived from the active Keycloak access token."
    >
      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: { xs: "1fr", md: "repeat(2, minmax(0, 1fr))" },
          gap: 2
        }}
      >
        <SessionField label="Realm" value={user?.realm ?? "-"} />
        <SessionField label="Username" value={user?.username ?? "-"} />
        <SessionField label="Tenant ID" value={user?.tenantId ?? "-"} />
        <SessionField label="Subject" value={user?.subject ?? "-"} />
        <SessionField label="Authorization version" value={String(user?.authzVersion ?? "-")} />
        <SessionField label="Expires at" value={user?.expiresAtUtc ?? "-"} />
      </Box>
      <Box sx={{ mt: 3, display: "flex", flexWrap: "wrap", gap: 1 }}>
        {(user?.roles ?? []).map((role) => (
          <Chip key={role} label={role} color="secondary" variant="outlined" />
        ))}
      </Box>
    </PageSection>
  );
}

type SessionFieldProps = {
  label: string;
  value: string;
};

function SessionField({ label, value }: SessionFieldProps) {
  return (
    <Box
      sx={{
        p: 2.25,
        borderRadius: 3,
        border: "1px solid rgba(21, 32, 43, 0.08)",
        backgroundColor: "rgba(246, 248, 251, 0.9)"
      }}
    >
      <Typography variant="overline" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="body1" sx={{ mt: 1 }}>
        {value}
      </Typography>
    </Box>
  );
}
