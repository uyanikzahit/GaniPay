// app/(tabs)/_layout.tsx
import React, { useEffect, useMemo, useState } from "react";
import { Tabs, useRouter } from "expo-router";
import { Ionicons } from "@expo/vector-icons";
import {
  Image,
  Platform,
  Pressable,
  StyleSheet,
  View,
  Modal,
  Text,
  Switch,
} from "react-native";

import { clearSession } from "@/constants/storage";

// ✅ i18n
import { t, type Lang } from "@/constants/i18n";
import { getLang, setLang, getTheme, setTheme } from "@/constants/prefs";

// ✅ theme colors
import { getColors, type ThemeMode } from "@/constants/Colors";

const GOLD = "rgba(246,195,64,1)";

type MenuItem = {
  key: string;
  title: string;
  subtitle?: string;
  icon: React.ComponentProps<typeof Ionicons>["name"];
  tone?: "default" | "danger";
  onPress?: () => void;
};

export default function TabLayout() {
  const router = useRouter();
  const [menuOpen, setMenuOpen] = useState(false);

  // ✅ persisted language
  const [language, setLanguage] = useState<Lang>("EN");

  // ✅ persisted theme
  const [theme, setThemeState] = useState<ThemeMode>("dark");

  // derived colors
  const C = useMemo(() => getColors(theme), [theme]);

  // ✅ load saved settings once
  useEffect(() => {
    let alive = true;
    (async () => {
      const [savedLang, savedTheme] = await Promise.all([getLang(), getTheme()]);
      if (!alive) return;
      setLanguage(savedLang);
      setThemeState(savedTheme);
    })();
    return () => {
      alive = false;
    };
  }, []);

  const toggleLanguage = async () => {
    const next: Lang = language === "EN" ? "TR" : "EN";
    setLanguage(next); // UI instantly
    await setLang(next); // persist + emit
  };

  const toggleTheme = async (isDark: boolean) => {
    const next: ThemeMode = isDark ? "dark" : "light";
    setThemeState(next); // UI instantly
    await setTheme(next); // persist + emit
  };

  const items: MenuItem[] = useMemo(
    () => [
      {
        key: "profile",
        title: t(language, "menu.profile"),
        subtitle: t(language, "menu.profile.sub"),
        icon: "person-circle-outline",
        onPress: () => {
          setMenuOpen(false);
          router.push("/(tabs)/account");
        },
      },
      {
        key: "wallet",
        title: t(language, "menu.wallet"),
        subtitle: t(language, "menu.wallet.sub"),
        icon: "wallet-outline",
        onPress: () => {
          setMenuOpen(false);
          router.push("/(tabs)/wallet");
        },
      },
      {
        key: "limits",
        title: t(language, "menu.limits"),
        subtitle: t(language, "menu.limits.sub"),
        icon: "speedometer-outline",
        onPress: () => {
          setMenuOpen(false);
          router.push("/(tabs)/limits");
        },
      },
      {
        key: "security",
        title: t(language, "menu.security"),
        subtitle: t(language, "menu.security.sub"),
        icon: "shield-checkmark-outline",
        onPress: () => {
          setMenuOpen(false);
          router.push("/(tabs)/security");
        },
      },
      {
        key: "notifications",
        title: t(language, "menu.notifications"),
        subtitle: t(language, "menu.notifications.sub"),
        icon: "notifications-outline",
        onPress: () => {
          setMenuOpen(false);
          router.push("/(tabs)/notifications");
        },
      },
      {
        key: "paybills",
        title: t(language, "menu.paybills"),
        subtitle: t(language, "menu.paybills.sub"),
        icon: "receipt-outline",
        onPress: () => {
          setMenuOpen(false);
          router.push("/(tabs)/paybills");
        },
      },
      {
        key: "partners",
        title: t(language, "menu.partners"),
        subtitle: t(language, "menu.partners.sub"),
        icon: "storefront-outline",
        onPress: () => {
          setMenuOpen(false);
          router.push("/(tabs)/partners");
        },
      },
      {
        key: "legal",
        title: t(language, "menu.legal"),
        subtitle: t(language, "menu.legal.sub"),
        icon: "document-text-outline",
        onPress: () => {
          setMenuOpen(false);
          router.push("/(tabs)/legal");
        },
      },
      {
        key: "about",
        title: t(language, "menu.about"),
        subtitle: t(language, "menu.about.sub"),
        icon: "information-circle-outline",
        onPress: () => {
          setMenuOpen(false);
          router.push("/(tabs)/about");
        },
      },
      {
        key: "logout",
        title: t(language, "menu.logout"),
        subtitle: t(language, "menu.logout.sub"),
        icon: "log-out-outline",
        tone: "danger",
        onPress: async () => {
          setMenuOpen(false);
          await clearSession();
          router.replace("/(auth)/login");
        },
      },
    ],
    [router, language]
  );

  return (
    <>
      <Tabs
        screenOptions={{
          headerShown: true,
          headerStyle: { backgroundColor: C.navBg },
          headerShadowVisible: false,

          headerTitle: "",
          headerLeftContainerStyle: { paddingLeft: 0, marginLeft: 0 },
          headerLeft: () => (
            <View style={styles.headerLeftWrap}>
              <Image
                source={require("../../assets/images/ganipay-logo.png")}
                style={styles.logo}
                resizeMode="contain"
              />
            </View>
          ),

          headerRight: () => (
            <Pressable
              onPress={() => setMenuOpen(true)}
              style={({ pressed }) => [
                styles.menuBtn,
                { backgroundColor: C.card, borderColor: C.border },
                pressed && { opacity: 0.85 },
              ]}
              hitSlop={10}
            >
              <Ionicons name="menu" size={22} color={C.gold} />
            </Pressable>
          ),

          tabBarActiveTintColor: C.gold,
          tabBarInactiveTintColor: C.tabInactive,
          tabBarShowLabel: false,

          tabBarStyle: Platform.select({
            ios: {
              backgroundColor: C.navBg,
              borderTopColor: C.border,
              borderTopWidth: 1,
              height: 86,
              paddingBottom: 18,
              paddingTop: 10,
            },
            default: {
              backgroundColor: C.navBg,
              borderTopColor: C.border,
              borderTopWidth: 1,
              height: 64,
              paddingBottom: 10,
              paddingTop: 6,
            },
          }),
        }}
      >
        <Tabs.Screen
          name="index"
          options={{
            title: t(language, "tabs.home"),
            tabBarIcon: ({ color, size }) => (
              <Ionicons name="home" size={size} color={color} />
            ),
          }}
        />

        <Tabs.Screen
          name="topup"
          options={{
            title: t(language, "tabs.topup"),
            tabBarIcon: ({ color, size }) => (
              <Ionicons name="wallet-outline" size={size} color={color} />
            ),
          }}
        />

        <Tabs.Screen
          name="transfer"
          options={{
            title: t(language, "tabs.transfer"),
            tabBarIcon: ({ color, size }) => (
              <Ionicons name="swap-horizontal-outline" size={size} color={color} />
            ),
          }}
        />

        <Tabs.Screen
          name="account"
          options={{
            title: t(language, "tabs.account"),
            tabBarIcon: ({ color, size }) => (
              <Ionicons name="person-circle-outline" size={size} color={color} />
            ),
          }}
        />

        {/* hidden screens */}
        <Tabs.Screen name="wallet" options={{ href: null }} />
        <Tabs.Screen name="limits" options={{ href: null }} />
        <Tabs.Screen name="security" options={{ href: null }} />
        <Tabs.Screen name="notifications" options={{ href: null }} />
        <Tabs.Screen name="paybills" options={{ href: null }} />
        <Tabs.Screen name="partners" options={{ href: null }} />
        <Tabs.Screen name="legal" options={{ href: null }} />
        <Tabs.Screen name="about" options={{ href: null }} />
        <Tabs.Screen name="3ds" options={{ href: null }} />
        <Tabs.Screen name="topup-result" options={{ href: null }} />
        <Tabs.Screen name="transfer-result" options={{ href: null }} />
        <Tabs.Screen name="transfer-3ds" options={{ href: null }} />
      </Tabs>

      {/* ✅ Premium menu modal */}
      <Modal
        visible={menuOpen}
        transparent
        animationType="fade"
        onRequestClose={() => setMenuOpen(false)}
      >
        <Pressable style={styles.backdrop} onPress={() => setMenuOpen(false)} />

        <View style={styles.sheetWrap} pointerEvents="box-none">
          <View
            style={[
              styles.sheet,
              {
                backgroundColor: theme === "light" ? "rgba(255,255,255,0.96)" : "rgba(10,18,32,0.96)",
                borderColor: C.border,
              },
            ]}
          >
            <View style={styles.sheetHeader}>
              <Text style={[styles.sheetTitle, { color: C.text }]}>{t(language, "menu.title")}</Text>
              <Pressable
                onPress={() => setMenuOpen(false)}
                style={({ pressed }) => [
                  styles.closeBtn,
                  { backgroundColor: C.card, borderColor: C.border },
                  pressed && { opacity: 0.85 },
                ]}
              >
                <Ionicons name="close" size={18} color={C.text} />
              </Pressable>
            </View>

            {/* ✅ Dark mode + Language */}
            <View style={styles.quickRow}>
              <View style={[styles.quickItem, { backgroundColor: C.card, borderColor: C.border }]}>
                <Text style={[styles.quickLabel, { color: C.text }]}>{t(language, "menu.darkMode")}</Text>
                <Switch
                  value={theme === "dark"}
                  onValueChange={toggleTheme}
                  thumbColor={theme === "dark" ? GOLD : undefined}
                  trackColor={{
                    false: theme === "light" ? "rgba(10,18,32,0.18)" : "rgba(255,255,255,0.18)",
                    true: "rgba(246,195,64,0.25)",
                  }}
                />
              </View>

              <Pressable
                onPress={toggleLanguage}
                style={({ pressed }) => [
                  styles.langPill,
                  pressed && { opacity: 0.9 },
                ]}
              >
                <Text style={styles.langText}>
                  {t(language, "menu.language")}: {language}
                </Text>
              </Pressable>
            </View>

            <View style={[styles.sheetDivider, { backgroundColor: C.border }]} />

            {/* ✅ MENU LIST + Wallet altına Topup/Transfer ekle */}
            {items.map((it) => {
              const row = (
                <Pressable
                  key={it.key}
                  onPress={it.onPress}
                  style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}
                >
                  <View
                    style={[
                      styles.itemIconWrap,
                      it.tone === "danger" && styles.itemIconWrapDanger,
                    ]}
                  >
                    <Ionicons
                      name={it.icon}
                      size={18}
                      color={it.tone === "danger" ? "rgba(252,165,165,0.95)" : C.gold}
                    />
                  </View>

                  <View style={{ flex: 1 }}>
                    <Text
                      style={[
                        styles.itemTitle,
                        { color: it.tone === "danger" ? "rgba(252,165,165,0.95)" : C.text },
                      ]}
                    >
                      {it.title}
                    </Text>
                    {!!it.subtitle && (
                      <Text style={[styles.itemSub, { color: C.muted }]}>{it.subtitle}</Text>
                    )}
                  </View>

                  <Ionicons name="chevron-forward" size={16} color={C.soft} />
                </Pressable>
              );

              // ✅ wallet değilse aynen bas
              if (it.key !== "wallet") return row;

              // ✅ wallet ise altına Topup + Transfer ekle (önceki mantık)
              return (
                <View key="wallet-group">
                  {row}

                  {/* ✅ Top up (Wallet altında) */}
                  <Pressable
                    onPress={() => {
                      setMenuOpen(false);
                      router.push("/(tabs)/topup");
                    }}
                    style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}
                  >
                    <View style={styles.itemIconWrap}>
                      <Ionicons name="wallet-outline" size={18} color={C.gold} />
                    </View>

                    <View style={{ flex: 1 }}>
                      <Text style={[styles.itemTitle, { color: C.text }]}>{t(language, "menu.topup")}</Text>
                      <Text style={[styles.itemSub, { color: C.muted }]}>{t(language, "menu.topup.sub")}</Text>
                    </View>

                    <Ionicons name="chevron-forward" size={16} color={C.soft} />
                  </Pressable>

                  {/* ✅ Transfer (Wallet altında) */}
                  <Pressable
                    onPress={() => {
                      setMenuOpen(false);
                      router.push("/(tabs)/transfer");
                    }}
                    style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}
                  >
                    <View style={styles.itemIconWrap}>
                      <Ionicons name="swap-horizontal-outline" size={18} color={C.gold} />
                    </View>

                    <View style={{ flex: 1 }}>
                      <Text style={[styles.itemTitle, { color: C.text }]}>{t(language, "menu.transfer")}</Text>
                      <Text style={[styles.itemSub, { color: C.muted }]}>{t(language, "menu.transfer.sub")}</Text>
                    </View>

                    <Ionicons name="chevron-forward" size={16} color={C.soft} />
                  </Pressable>
                </View>
              );
            })}
          </View>
        </View>
      </Modal>
    </>
  );
}

const styles = StyleSheet.create({
  headerLeftWrap: {
    height: 56,
    justifyContent: "center",
    paddingLeft: 0,
    marginLeft: -90,
  },
  logo: {
    height: 62,
    width: 260,
  },

  menuBtn: {
    width: 44,
    height: 44,
    borderRadius: 16,
    alignItems: "center",
    justifyContent: "center",
    borderWidth: 1,
    marginRight: 10,
  },

  backdrop: { flex: 1, backgroundColor: "rgba(0,0,0,0.70)" },

  sheetWrap: {
    position: "absolute",
    top: 56,
    right: 14,
    left: 14,
    alignItems: "flex-end",
  },

  sheet: {
    width: "86%",
    borderRadius: 18,
    padding: 12,
    borderWidth: 1,
    overflow: "hidden",
  },

  sheetHeader: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    paddingHorizontal: 6,
    paddingTop: 4,
    paddingBottom: 10,
  },
  sheetTitle: { fontSize: 14, fontWeight: "900" },

  closeBtn: {
    width: 34,
    height: 34,
    borderRadius: 14,
    alignItems: "center",
    justifyContent: "center",
    borderWidth: 1,
  },

  quickRow: { flexDirection: "row", gap: 10, paddingHorizontal: 6, paddingBottom: 10 },
  quickItem: {
    flex: 1,
    borderRadius: 14,
    padding: 10,
    borderWidth: 1,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
  },
  quickLabel: { fontSize: 12, fontWeight: "900" },

  langPill: {
    flex: 1,
    borderRadius: 14,
    padding: 10,
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
    alignItems: "center",
    justifyContent: "center",
  },
  langText: { color: "rgba(246,195,64,0.95)", fontSize: 12, fontWeight: "900" },

  sheetDivider: {
    height: 1,
    marginVertical: 8,
  },

  item: {
    flexDirection: "row",
    alignItems: "center",
    gap: 10,
    paddingVertical: 10,
    paddingHorizontal: 6,
    borderRadius: 14,
  },

  itemIconWrap: {
    width: 34,
    height: 34,
    borderRadius: 14,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
  },
  itemIconWrapDanger: {
    backgroundColor: "rgba(252,165,165,0.10)",
    borderColor: "rgba(252,165,165,0.22)",
  },

  itemTitle: { fontSize: 13, fontWeight: "900" },
  itemSub: { marginTop: 3, fontSize: 11.5, fontWeight: "700" },
});
