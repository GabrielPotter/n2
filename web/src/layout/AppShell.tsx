import { AppBar, Box, Button, Chip, Container, Stack, Toolbar, Typography } from "@mui/material";
import ArrowBackRoundedIcon from "@mui/icons-material/ArrowBackRounded";
import LogoutRoundedIcon from "@mui/icons-material/LogoutRounded";
import GridViewRoundedIcon from "@mui/icons-material/GridViewRounded";
import { Outlet, useLocation, useNavigate } from "react-router-dom";
import { routeLookup } from "../routes";
import { useAppContext } from "../app/AppProvider";
import { getRealmConfig } from "../realms";

export function AppShell() {
  const location = useLocation();
  const navigate = useNavigate();
  const { homePath, session, signOut } = useAppContext();
  const currentRoute = routeLookup[location.pathname];
  const realmConfig = getRealmConfig(session.realm);
  const isMenu = location.pathname.startsWith("/menu/");

  return (
    <Box
      sx={{
        minHeight: "100vh",
        background:
          "radial-gradient(circle at top left, rgba(201, 117, 70, 0.16), transparent 28%), linear-gradient(180deg, #f6f1e8 0%, #eef2f7 100%)"
      }}
    >
      <AppBar
        position="sticky"
        color="transparent"
        elevation={0}
        sx={{
          backdropFilter: "blur(18px)",
          borderBottom: "1px solid rgba(21, 32, 43, 0.08)"
        }}
      >
        <Toolbar sx={{ minHeight: 80, gap: 2 }}>
          {isMenu ? (
            <GridViewRoundedIcon />
          ) : (
            <Button
              color="inherit"
              startIcon={<ArrowBackRoundedIcon />}
              onClick={() => navigate(homePath)}
              sx={{ borderRadius: 999 }}
            >
              Back to menu
            </Button>
          )}
          <Box sx={{ flexGrow: 1 }}>
            <Typography variant="overline" sx={{ color: "secondary.main", letterSpacing: "0.18em" }}>
              Platform Console
            </Typography>
            <Typography variant="h5">{isMenu ? realmConfig.label : currentRoute?.title ?? "Workspace"}</Typography>
          </Box>
          <Stack spacing={0.5} sx={{ alignItems: "flex-end", display: { xs: "none", md: "flex" } }}>
            <Typography variant="body2">{session.user?.username}</Typography>
            <Stack direction="row" spacing={1} useFlexGap sx={{ flexWrap: "wrap", justifyContent: "flex-end" }}>
              <Chip size="small" label={`Realm: ${realmConfig.id}`} variant="outlined" />
              {session.user?.tenantId ? (
                <Chip size="small" label={`Tenant: ${session.user.tenantId}`} variant="outlined" />
              ) : null}
              <Chip size="small" label={`Roles: ${session.user?.roles.join(", ") || "none"}`} variant="outlined" />
              {typeof session.user?.authzVersion === "number" ? (
                <Chip size="small" label={`Authz: ${String(session.user.authzVersion)}`} variant="outlined" />
              ) : null}
            </Stack>
          </Stack>
          <Button color="inherit" startIcon={<LogoutRoundedIcon />} onClick={signOut} sx={{ borderRadius: 999 }}>
            Sign out
          </Button>
        </Toolbar>
      </AppBar>

      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Outlet />
      </Container>
    </Box>
  );
}
