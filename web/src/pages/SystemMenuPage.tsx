import { Box, Chip } from "@mui/material";
import { NavigationTileGrid } from "../components/NavigationTileGrid";
import { PageSection } from "../components/PageSection";
import { getMenuRoutesForRealm } from "../routes";

export function SystemMenuPage() {
  const menuRoutes = getMenuRoutesForRealm("n2-system");

  return (
    <Box>
      <PageSection
        title="System navigation"
        description="This realm opens the platform administration workspace instead of the tenant-facing users menu."
        action={<Chip label={`${menuRoutes.length} pages`} color="secondary" variant="outlined" />}
      >
        <NavigationTileGrid items={menuRoutes} />
      </PageSection>
    </Box>
  );
}
