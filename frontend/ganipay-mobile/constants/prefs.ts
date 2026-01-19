// constants/prefs.ts
import AsyncStorage from "@react-native-async-storage/async-storage";
import type { Lang } from "./i18n";

const KEY_LANG = "ganipay.lang";

export async function getLang(): Promise<Lang> {
  const v = await AsyncStorage.getItem(KEY_LANG);
  return v === "TR" ? "TR" : "EN";
}

export async function setLang(lang: Lang) {
  await AsyncStorage.setItem(KEY_LANG, lang);
}
