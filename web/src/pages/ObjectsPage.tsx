import { Button, Chip, Stack, Typography } from "@mui/material";
import RefreshRoundedIcon from "@mui/icons-material/RefreshRounded";
import { PageSection } from "../components/PageSection";
import { useAppContext } from "../app/AppProvider";

export function ObjectsPage() {
  const { data, refreshData } = useAppContext();

  return (
    <PageSection
      title="Objects"
      description="Queried objects reflect the current active records returned by the gateway."
      action={
        <Button
          variant="outlined"
          startIcon={<RefreshRoundedIcon />}
          onClick={() => void refreshData()}
          disabled={data.loading}
        >
          {data.loading ? "Refreshing..." : "Refresh"}
        </Button>
      }
      error={data.error}
    >
      <Stack spacing={1.5}>
        {data.objects.map((object) => (
          <Stack
            key={object.id}
            direction={{ xs: "column", md: "row" }}
            spacing={2}
            sx={{
              justifyContent: "space-between",
              p: 2,
              borderRadius: 3,
              border: "1px solid rgba(21, 32, 43, 0.08)",
              backgroundColor: "rgba(246, 248, 251, 0.9)"
            }}
          >
            <div>
              <Typography variant="subtitle1">{object.name}</Typography>
              <Typography variant="body2" color="text.secondary">
                {object.objectKind} | {object.categoryName} | {object.typeName}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {object.id}
              </Typography>
            </div>
            <Chip label={object.status} color={object.status === "active" ? "success" : "default"} variant="outlined" />
          </Stack>
        ))}
        {!data.loading && data.objects.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No objects loaded.
          </Typography>
        ) : null}
      </Stack>
    </PageSection>
  );
}
