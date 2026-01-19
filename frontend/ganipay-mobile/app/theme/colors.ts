// app/theme/colors.ts

export type ThemeMode = "dark" | "light";

export const DarkTheme = {
  bg: "#0B1220",        // navy (premium)
  surface: "#FFFFFF",
  muted: "#64748B",
  text: "#0F172A",
  primary: "#16A34A",
  primaryDark: "#15803D",
  border: "#E5E7EB",
  hero: "#F6C340",
  hero2: "#FDE68A",
} as const;

export const LightTheme = {
  bg: "#F5F7FB",        // light background
  surface: "#FFFFFF",
  muted: "#475569",
  text: "#0B1220",
  primary: "#16A34A",
  primaryDark: "#15803D",
  border: "rgba(0,0,0,0.08)",
  hero: "#F6C340",
  hero2: "#FDE68A",
} as const;

// ✅ mevcut kod kırılmasın diye "Colors" dark default kalsın
export const Colors = DarkTheme;

export function getThemeColors(mode: ThemeMode) {
  return mode === "light" ? LightTheme : DarkTheme;
}
