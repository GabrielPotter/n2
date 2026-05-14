import { BrowserRouter } from "react-router-dom";
import { CssBaseline, ThemeProvider } from "@mui/material";
import { AppProvider } from "./app/AppProvider";
import { AppRoutes } from "./app/AppRoutes";
import { appTheme } from "./theme";

export default function App() {
  return (
    <ThemeProvider theme={appTheme}>
      <CssBaseline />
      <BrowserRouter>
        <AppProvider>
          <AppRoutes />
        </AppProvider>
      </BrowserRouter>
    </ThemeProvider>
  );
}
