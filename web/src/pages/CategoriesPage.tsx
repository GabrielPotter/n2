import { Box, Button, List, ListItem, ListItemText, Typography } from "@mui/material";
import RefreshRoundedIcon from "@mui/icons-material/RefreshRounded";
import { PageSection } from "../components/PageSection";
import { useAppContext } from "../app/AppProvider";

export function CategoriesPage() {
  const { data, refreshData } = useAppContext();

  return (
    <PageSection
      title="Catalog categories"
      description="Categories come from the gateway and define the object grouping model."
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
      <List sx={{ display: "grid", gap: 1.5 }}>
        {data.categories.map((category) => (
          <ListItem
            key={category.categoryId}
            sx={{
              borderRadius: 3,
              alignItems: "flex-start",
              border: "1px solid rgba(21, 32, 43, 0.08)",
              backgroundColor: "rgba(246, 248, 251, 0.9)"
            }}
          >
            <ListItemText
              primary={category.name}
              secondary={
                <Box sx={{ mt: 0.75 }}>
                  <Typography variant="body2" color="text.secondary">
                    Object kind: {category.objectKind}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {category.categoryId}
                  </Typography>
                </Box>
              }
            />
          </ListItem>
        ))}
        {!data.loading && data.categories.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No categories loaded.
          </Typography>
        ) : null}
      </List>
    </PageSection>
  );
}
