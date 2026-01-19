// constants/Colors.ts
export type ThemeMode = "dark" | "light";

const GOLD = "rgba(246,195,64,1)";

export type ThemeColors = {
  mode: ThemeMode;

  // Core surfaces
  bg: string;
  navBg: string;
  card: string;
  border: string;

  // Text
  text: string;
  muted: string;
  soft: string;

  // Accent
  gold: string;

  // Tabs
  tabInactive: string;
};

export const DarkColors: ThemeColors = {
  mode: "dark",
  bg: "#0B1220",
  navBg: "#0B1220",
  card: "rgba(255,255,255,0.06)",
  border: "rgba(255,255,255,0.10)",

  text: "rgba(255,255,255,0.92)",
  muted: "rgba(255,255,255,0.60)",
  soft: "rgba(255,255,255,0.35)",

  gold: GOLD,
  tabInactive: "#98a2b3",
};

export const LightColors: ThemeColors = {
  mode: "light",
  bg: "#F5F7FB",
  navBg: "#FFFFFF",
  card: "rgba(0,0,0,0.03)",
  border: "rgba(0,0,0,0.08)",

  text: "rgba(10,18,32,0.95)",
  muted: "rgba(10,18,32,0.60)",
  soft: "rgba(10,18,32,0.35)",

  gold: GOLD,
  tabInactive: "rgba(10,18,32,0.40)",
};

export function getColors(mode: ThemeMode): ThemeColors {
  return mode === "light" ? LightColors : DarkColors;
}
