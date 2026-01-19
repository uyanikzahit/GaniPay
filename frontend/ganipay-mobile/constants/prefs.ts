// constants/prefs.ts
import AsyncStorage from "@react-native-async-storage/async-storage";
import { DeviceEventEmitter } from "react-native";
import type { Lang } from "./i18n";
import type { ThemeMode } from "./Colors";

const KEY_LANG = "ganipay.lang";
const KEY_THEME = "ganipay.theme";

// ✅ event name (home + menu dinleyecek)
export const LANG_CHANGED = "ganipay.lang.changed";
export const THEME_CHANGED = "ganipay.theme.changed";

export async function getLang(): Promise<Lang> {
  const v = await AsyncStorage.getItem(KEY_LANG);
  return v === "TR" ? "TR" : "EN";
}

export async function setLang(lang: Lang) {
  await AsyncStorage.setItem(KEY_LANG, lang);
  DeviceEventEmitter.emit(LANG_CHANGED, lang);
}

// ✅ theme
export async function getTheme(): Promise<ThemeMode> {
  const v = await AsyncStorage.getItem(KEY_THEME);
  return v === "light" ? "light" : "dark"; // default dark
}

export async function setTheme(mode: ThemeMode) {
  await AsyncStorage.setItem(KEY_THEME, mode);
  DeviceEventEmitter.emit(THEME_CHANGED, mode);
}
