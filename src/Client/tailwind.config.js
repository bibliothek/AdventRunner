module.exports = {
    purge: ["./index.html", "./src/**/*.{vue,js,ts,jsx,tsx}"],
    darkMode: false, // or 'media' or 'class'
    theme: {
        extend: {},
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
