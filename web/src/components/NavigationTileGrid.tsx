import { ArrowForwardRounded as ArrowForwardRoundedIcon } from "@mui/icons-material";
import { Box, Card, CardActionArea, Chip, Stack, Typography } from "@mui/material";
import { useNavigate } from "react-router-dom";
import type { MenuRouteDefinition } from "../types";

type NavigationTileGridProps = {
  items: MenuRouteDefinition[];
};

export function NavigationTileGrid({ items }: NavigationTileGridProps) {
  const navigate = useNavigate();

  return (
    <Box
      sx={{
        display: "grid",
        gridTemplateColumns: {
          xs: "1fr",
          sm: "repeat(2, minmax(0, 1fr))",
          lg: "repeat(3, minmax(0, 1fr))"
        },
        gap: 2
      }}
    >
      {items.map((item) => (
        <Card
          key={item.path}
          elevation={0}
          sx={{
            borderRadius: 4,
            border: "1px solid rgba(21, 32, 43, 0.08)",
            background: "linear-gradient(180deg, rgba(255,255,255,0.96) 0%, rgba(242,246,251,0.94) 100%)"
          }}
        >
          <CardActionArea onClick={() => navigate(item.path)} sx={{ p: 3, height: "100%" }}>
            <Stack spacing={2} sx={{ height: "100%" }}>
              <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                <Box
                  sx={{
                    width: 52,
                    height: 52,
                    borderRadius: 3,
                    display: "grid",
                    placeItems: "center",
                    backgroundColor: "rgba(155, 77, 47, 0.12)",
                    color: "secondary.main"
                  }}
                >
                  <item.icon />
                </Box>
                <ArrowForwardRoundedIcon color="action" />
              </Box>
              <Box>
                <Typography variant="h6">{item.title}</Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                  {item.description}
                </Typography>
              </Box>
              <Box sx={{ mt: "auto" }}>
                <Chip label={item.tag} size="small" variant="outlined" />
              </Box>
            </Stack>
          </CardActionArea>
        </Card>
      ))}
    </Box>
  );
}
