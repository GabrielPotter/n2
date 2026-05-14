import { createTheme } from "@mui/material";

export const appTheme = createTheme({
  palette: {
    mode: "light",
    primary: {
      main: "#15202b"
    },
    secondary: {
      main: "#9b4d2f"
    },
    background: {
      default: "#edf3f8",
      paper: "#ffffff"
    }
  },
  shape: {
    borderRadius: 18
  },
  typography: {
    fontFamily: '"Public Sans", "Segoe UI", sans-serif',
    h2: {
      fontSize: "clamp(2.6rem, 6vw, 4.8rem)",
      lineHeight: 0.95,
      fontWeight: 700
    },
    h5: {
      fontWeight: 650
    },
    h6: {
      fontWeight: 650
    },
    button: {
      textTransform: "none",
      fontWeight: 600
    }
  },
  components: {
    MuiButton: {
      defaultProps: {
        disableElevation: true
      }
    },
    MuiTextField: {
      defaultProps: {
        variant: "outlined"
      }
    }
  }
});
