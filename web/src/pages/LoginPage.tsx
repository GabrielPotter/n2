import { FormEvent, useState } from "react";
import {
  Alert,
  Box,
  Button,
  Container,
  IconButton,
  InputAdornment,
  Paper,
  Stack,
  MenuItem,
  TextField,
  Typography
} from "@mui/material";
import VisibilityOffRoundedIcon from "@mui/icons-material/VisibilityOffRounded";
import VisibilityRoundedIcon from "@mui/icons-material/VisibilityRounded";
import LoginRoundedIcon from "@mui/icons-material/LoginRounded";
import { useAppContext } from "../app/AppProvider";
import { defaultRealm, getDevelopmentUsersForRealm, getRealmConfig, realmConfigs } from "../realms";
import type { LoginRequest } from "../types";

const initialLoginRequest: LoginRequest = {
  realm: defaultRealm,
  username: "",
  password: "Password123!"
};

export function LoginPage() {
  const { signIn, session, data } = useAppContext();
  const [request, setRequest] = useState<LoginRequest>({
    realm: session.realm || initialLoginRequest.realm,
    username: session.username || initialLoginRequest.username,
    password: initialLoginRequest.password
  });
  const [showPassword, setShowPassword] = useState(false);
  const selectedRealm = getRealmConfig(request.realm);
  const developmentUsers = getDevelopmentUsersForRealm(request.realm);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    try {
      await signIn(request);
    } catch {
      return;
    }
  }

  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "grid",
        placeItems: "center",
        px: 2,
        background:
          "radial-gradient(circle at top, rgba(201, 117, 70, 0.22), transparent 30%), linear-gradient(180deg, #f4eee4 0%, #edf3f8 100%)"
      }}
    >
      <Container maxWidth="lg">
        <Box
          sx={{
            display: "grid",
            gap: 2,
            gridTemplateColumns: { xs: "1fr", lg: "1.15fr 0.85fr" },
            alignItems: "stretch"
          }}
        >
          <Paper
            elevation={0}
            sx={{
              p: { xs: 3, md: 5 },
              borderRadius: 5,
              border: "1px solid rgba(21, 32, 43, 0.08)",
              backgroundColor: "rgba(255,255,255,0.82)",
              backdropFilter: "blur(10px)"
            }}
          >
            <Typography variant="overline" sx={{ color: "secondary.main", letterSpacing: "0.18em" }}>
              Platform Console
            </Typography>
            <Typography variant="h2" sx={{ mt: 1, maxWidth: 560 }}>
              Sign in to the current development workspace
            </Typography>
            <Typography variant="body1" color="text.secondary" sx={{ mt: 2, maxWidth: 520 }}>
              Authentication is handled by Keycloak. After a successful login, the application opens the navigation
              hub that matches the selected realm.
            </Typography>

            <Box component="form" onSubmit={handleSubmit} sx={{ mt: 4, display: "grid", gap: 2 }}>
              {data.error ? <Alert severity="error">{data.error}</Alert> : null}
              <TextField
                select
                label="Realm"
                value={request.realm}
                onChange={(event) =>
                  setRequest((current) => ({
                    ...current,
                    realm: event.target.value as LoginRequest["realm"]
                  }))
                }
                fullWidth
              >
                {realmConfigs.map((realm) => (
                  <MenuItem key={realm.id} value={realm.id}>
                    {realm.label}
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                label="Email"
                value={request.username}
                onChange={(event) => setRequest((current) => ({ ...current, username: event.target.value }))}
                placeholder="admin@example.com"
                autoComplete="username"
                required
                fullWidth
              />
              <TextField
                label="Password"
                type={showPassword ? "text" : "password"}
                value={request.password}
                onChange={(event) => setRequest((current) => ({ ...current, password: event.target.value }))}
                autoComplete="current-password"
                required
                fullWidth
                slotProps={{
                  input: {
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton onClick={() => setShowPassword((current) => !current)} edge="end">
                          {showPassword ? <VisibilityOffRoundedIcon /> : <VisibilityRoundedIcon />}
                        </IconButton>
                      </InputAdornment>
                    )
                  }
                }}
              />
              <Button type="submit" variant="contained" size="large" startIcon={<LoginRoundedIcon />} disabled={session.signingIn}>
                {session.signingIn ? "Signing in..." : "Sign in"}
              </Button>
            </Box>
          </Paper>

          <Paper
            elevation={0}
            sx={{
              p: { xs: 3, md: 4 },
              borderRadius: 5,
              border: "1px solid rgba(21, 32, 43, 0.08)",
              background: "linear-gradient(180deg, rgba(255,255,255,0.92) 0%, rgba(244, 238, 229, 0.9) 100%)"
            }}
          >
            <Typography variant="h5">{selectedRealm.label}</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1.5, mb: 3 }}>
              {selectedRealm.description}
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              Default password: <strong>Password123!</strong>
            </Typography>
            <Stack spacing={1.5}>
              {developmentUsers.map((user) => (
                <Paper
                  key={user.username}
                  elevation={0}
                  sx={{
                    p: 2,
                    borderRadius: 3,
                    border: "1px solid rgba(21, 32, 43, 0.08)",
                    backgroundColor: "rgba(255,255,255,0.72)"
                  }}
                >
                  <Typography variant="subtitle1">{user.label}</Typography>
                  <Typography variant="body2" color="text.secondary">
                    {user.username}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {user.description}
                  </Typography>
                </Paper>
              ))}
            </Stack>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 3 }}>
              Users are loaded from the Keycloak realm configuration under `infra/keycloak/`.
            </Typography>
          </Paper>
        </Box>
      </Container>
    </Box>
  );
}
