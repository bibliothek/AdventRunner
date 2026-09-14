module.exports = {
    content: ["./index.html", "./src/**/*.{vue,js,ts,jsx,tsx}"],
    darkMode: "media",
    theme: {
        extend: {
            fontFamily: {
                sans: [
                    "Outfit",
                    "ui-sans-serif",
                    "system-ui",
                    "-apple-system",
                    "Segoe UI",
                    "Roboto",
                    "Helvetica Neue",
                    "Arial",
                    "sans-serif",
                ],
            },
            colors: {
                ink: {
                    50: "#f5f7fa",
                    100: "#e9edf3",
                    400: "#6b768c",
                    600: "#3f4a5e",
                    700: "#333c4d",
                    800: "#28303d",
                    900: "#1c222c",
                },
                strava: "#fc4c02",
            },
            boxShadow: {
                door: "0 8px 20px -10px rgba(28, 34, 44, 0.45)",
                "door-lift": "0 18px 32px -14px rgba(28, 34, 44, 0.55)",
                soft: "0 20px 45px -28px rgba(28, 34, 44, 0.5)",
            },
            keyframes: {
                "fade-up": {
                    "0%": { opacity: "0", transform: "translateY(14px)" },
                    "100%": { opacity: "1", transform: "translateY(0)" },
                },
                float: {
                    "0%, 100%": { transform: "translateY(0)" },
                    "50%": { transform: "translateY(-8px)" },
                },
                "snow-drift": {
                    "0%": { transform: "translateY(-10%)" },
                    "100%": { transform: "translateY(110%)" },
                },
            },
            animation: {
                "fade-up": "fade-up 0.6s cubic-bezier(0.22, 1, 0.36, 1) both",
                float: "float 5s ease-in-out infinite",
            },
        },
    },
    variants: {
        extend: {},
    },
    plugins: [require("daisyui")],
    daisyui: {
        themes: [
          {
            light: {
              ...require("daisyui/colors/themes")["[data-theme=light]"],
              "primary": "#66cc8a",
              "primary-focus": "#40bf6c",
              "primary-content": "#f9fafb",
              "secondary": "#377cfb",
              "secondary-focus": "#055bfa",
              "secondary-content": "#f9fafb",
              "accent": "#ea5234",
              "accent-focus": "#d43616",
              "accent-content": "#f9fafb",
              "neutral": "#333c4d",
              "neutral-focus": "#1f242e",
              "neutral-content": "#f9fafb",
              "base-100": "#ffffff",
              "base-200": "#f9fafb",
              "base-300": "#f2f2f2",
              "base-content": "#333c4d",
              "info": "#2094f3",
              "success": "#009485",
              "warning": "#ff9900",
              "error": "#ff5724",
            },
          },
        ],
      },
};
