import { Box, Chip } from "@mui/material";
import { NavigationTileGrid } from "../components/NavigationTileGrid";
import { PageSection } from "../components/PageSection";
import { getMenuRoutesForRealm } from "../routes";

export function UsersMenuPage() {
  const menuRoutes = getMenuRoutesForRealm("n2-users");

  return (
    <Box>
      <PageSection
        title="Workspace navigation"
        description="Open tenant-facing catalog, query, editor, and session functions for the users realm."
        action={<Chip label={`${menuRoutes.length} pages`} color="secondary" variant="outlined" />}
      >
        <NavigationTileGrid items={menuRoutes} />
      </PageSection>
    </Box>
  );
}
