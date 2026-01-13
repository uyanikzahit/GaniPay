// app/(tabs)/_layout.tsx
import React, { useMemo, useState } from "react";
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

const NAV_BG = "#0B1220";
const BORDER = "rgba(255,255,255,0.10)";
const GOLD = "rgba(246,195,64,1)";
const INACTIVE = "#98a2b3";

type MenuItem = {
  key: string;
  title: string;
  subtitle?: string;
  icon: React.ComponentProps<typeof Ionicons>["name"];
  tone?: "default" | "danger";
  kind?: "link";
  onPress?: () => void;
};

export default function TabLayout() {
  const router = useRouter();
  const [menuOpen, setMenuOpen] = useState(false);

  // ✅ Local-only toggles (sonra global settings’e bağlarız)
  const [darkMode, setDarkMode] = useState(true);
  const [nightShift, setNightShift] = useState(true);
  const [pushNotifs, setPushNotifs] = useState(true);
  const [language, setLanguage] = useState<"EN" | "TR">("EN");

  const items: MenuItem[] = useMemo(
    () => [
      {
        key: "profile",
        title: "Profile details",
        subtitle: "Personal info, verification status",
        icon: "person-circle-outline",
        onPress: () => {
          setMenuOpen(false);
          router.push("/(tabs)/account");
        },
      },
      {
        key: "wallet",
        title: "Wallet & cards",
        subtitle: "Linked cards, funding sources",
        icon: "card-outline",
        onPress: () => setMenuOpen(false),
      },
      {
        key: "limits",
        title: "Spending limits",
        subtitle: "Daily/Monthly limits, rules",
        icon: "speedometer-outline",
        onPress: () => setMenuOpen(false),
      },
      {
        key: "security",
        title: "Security",
        subtitle: "PIN, biometrics, trusted devices",
        icon: "shield-checkmark-outline",
        onPress: () => setMenuOpen(false),
      },
      {
        key: "devices",
        title: "Trusted devices",
        subtitle: "Manage active sessions",
        icon: "phone-portrait-outline",
        onPress: () => setMenuOpen(false),
      },
      {
        key: "notifications",
        title: "Notifications",
        subtitle: "Push alerts, email preferences",
        icon: "notifications-outline",
        onPress: () => setMenuOpen(false),
      },
      {
        key: "appearance",
        title: "Appearance",
        subtitle: "Theme & readability",
        icon: "color-palette-outline",
        onPress: () => setMenuOpen(false),
      },
      {
        key: "support",
        title: "Help center",
        subtitle: "FAQ, contact support",
        icon: "help-circle-outline",
        onPress: () => setMenuOpen(false),
      },
      {
        key: "report",
        title: "Report a problem",
        subtitle: "Send feedback & logs",
        icon: "bug-outline",
        onPress: () => setMenuOpen(false),
      },
      {
        key: "legal",
        title: "Legal",
        subtitle: "Terms, privacy policy",
        icon: "document-text-outline",
        onPress: () => setMenuOpen(false),
      },
      {
        key: "about",
        title: "About GaniPay",
        subtitle: "Version, build info",
        icon: "information-circle-outline",
        onPress: () => setMenuOpen(false),
      },
      {
        key: "logout",
        title: "Log out",
        subtitle: "End this session",
        icon: "log-out-outline",
        tone: "danger",
        onPress: () => {
          setMenuOpen(false);
          // sonra token temizleyip login’e döneceğiz
          // router.replace("/(auth)/login");
        },
      },
    ],
    [router]
  );

  return (
    <>
      <Tabs
        screenOptions={{
          headerShown: true,
          headerStyle: { backgroundColor: NAV_BG },
          headerShadowVisible: false,

          // ✅ Logo kesin sola: headerLeft içine aldık
          headerTitle: "",
          headerLeft: () => (
            <View style={styles.headerLeftWrap}>
              <Image
                source={require("../../assets/images/ganipay-logo.png")}
                style={styles.logo}
                resizeMode="contain"
              />
            </View>
          ),

          // ✅ Sağ menü butonu
          headerRight: () => (
            <Pressable
              onPress={() => setMenuOpen(true)}
              style={({ pressed }) => [styles.menuBtn, pressed && { opacity: 0.85 }]}
              hitSlop={10}
            >
              <Ionicons name="menu" size={22} color={GOLD} />
            </Pressable>
          ),

          tabBarActiveTintColor: GOLD,
          tabBarInactiveTintColor: INACTIVE,
          tabBarShowLabel: false,

          tabBarStyle: Platform.select({
            ios: {
              backgroundColor: NAV_BG,
              borderTopColor: BORDER,
              borderTopWidth: 1,
              height: 86,
              paddingBottom: 18,
              paddingTop: 10,
            },
            default: {
              backgroundColor: NAV_BG,
              borderTopColor: BORDER,
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
            title: "Home",
            tabBarIcon: ({ color, size }) => (
              <Ionicons name="home" size={size} color={color} />
            ),
          }}
        />
        <Tabs.Screen
          name="topup"
          options={{
            title: "Top Up",
            tabBarIcon: ({ color, size }) => (
              <Ionicons name="wallet-outline" size={size} color={color} />
            ),
          }}
        />
        <Tabs.Screen
          name="transfer"
          options={{
            title: "Transfer",
            tabBarIcon: ({ color, size }) => (
              <Ionicons name="swap-horizontal-outline" size={size} color={color} />
            ),
          }}
        />
        <Tabs.Screen
          name="account"
          options={{
            title: "Account",
            tabBarIcon: ({ color, size }) => (
              <Ionicons name="person-circle-outline" size={size} color={color} />
            ),
          }}
        />
      </Tabs>

      {/* ✅ Premium menu modal */}
      <Modal visible={menuOpen} transparent animationType="fade" onRequestClose={() => setMenuOpen(false)}>
        {/* ✅ Arka plan karartma artırıldı (yazı karışmasını önler) */}
        <Pressable style={styles.backdrop} onPress={() => setMenuOpen(false)} />

        <View style={styles.sheetWrap} pointerEvents="box-none">
          <View style={styles.sheet}>
            <View style={styles.sheetHeader}>
              <Text style={styles.sheetTitle}>Menu</Text>
              <Pressable
                onPress={() => setMenuOpen(false)}
                style={({ pressed }) => [styles.closeBtn, pressed && { opacity: 0.85 }]}
              >
                <Ionicons name="close" size={18} color="rgba(255,255,255,0.85)" />
              </Pressable>
            </View>

            {/* ✅ Quick toggles (premium cüzdan tarzı) */}
            <View style={styles.quickRow}>
              <View style={styles.quickItem}>
                <Text style={styles.quickLabel}>Dark mode</Text>
                <Switch
                  value={darkMode}
                  onValueChange={setDarkMode}
                  thumbColor={darkMode ? GOLD : undefined}
                  trackColor={{ false: "rgba(255,255,255,0.18)", true: "rgba(246,195,64,0.25)" }}
                />
              </View>

              <View style={styles.quickItem}>
                <Text style={styles.quickLabel}>Night shift</Text>
                <Switch
                  value={nightShift}
                  onValueChange={setNightShift}
                  thumbColor={nightShift ? GOLD : undefined}
                  trackColor={{ false: "rgba(255,255,255,0.18)", true: "rgba(246,195,64,0.25)" }}
                />
              </View>
            </View>

            <View style={styles.quickRow}>
              <View style={styles.quickItem}>
                <Text style={styles.quickLabel}>Push alerts</Text>
                <Switch
                  value={pushNotifs}
                  onValueChange={setPushNotifs}
                  thumbColor={pushNotifs ? GOLD : undefined}
                  trackColor={{ false: "rgba(255,255,255,0.18)", true: "rgba(246,195,64,0.25)" }}
                />
              </View>

              <Pressable
                onPress={() => setLanguage((p) => (p === "EN" ? "TR" : "EN"))}
                style={({ pressed }) => [styles.langPill, pressed && { opacity: 0.9 }]}
              >
                <Text style={styles.langText}>Language: {language}</Text>
              </Pressable>
            </View>

            <View style={styles.sheetDivider} />

            <ScrollArea>
              {items.map((it) => (
                <Pressable
                  key={it.key}
                  onPress={it.onPress}
                  style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}
                >
                  <View style={[styles.itemIconWrap, it.tone === "danger" && styles.itemIconWrapDanger]}>
                    <Ionicons
                      name={it.icon}
                      size={18}
                      color={it.tone === "danger" ? "rgba(252,165,165,0.95)" : GOLD}
                    />
                  </View>

                  <View style={{ flex: 1 }}>
                    <Text style={[styles.itemTitle, it.tone === "danger" && styles.itemTitleDanger]}>
                      {it.title}
                    </Text>
                    {!!it.subtitle && <Text style={styles.itemSub}>{it.subtitle}</Text>}
                  </View>

                  <Ionicons name="chevron-forward" size={16} color="rgba(255,255,255,0.35)" />
                </Pressable>
              ))}
            </ScrollArea>
          </View>
        </View>
      </Modal>
    </>
  );
}

// ✅ Modal içeriği uzarsa scroll olsun (web/android/ios)
function ScrollArea({ children }: { children: React.ReactNode }) {
  // Switch’ler var, çok karmaşaya girmeden basit View bırakıyorum.
  // Menü uzarsa zaten sheet yüksekliği yeterli; istersek sonra ScrollView ekleriz.
  return <View>{children}</View>;
}

const styles = StyleSheet.create({
  headerLeftWrap: {
    height: 56,
    justifyContent: "center",
    paddingLeft: 10, // ✅ sol köşeye yakın
  },
  logo: {
    height: 48,
    width: 190,
  },

  menuBtn: {
    width: 44,
    height: 44,
    borderRadius: 16,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.10)",
    marginRight: 10,
  },

  // ✅ Arkayı daha net kararttık (yazı karışmasın)
  backdrop: {
    flex: 1,
    backgroundColor: "rgba(0,0,0,0.70)",
  },

  sheetWrap: {
    position: "absolute",
    top: 56,
    right: 14,
    left: 14,
    alignItems: "flex-end",
  },

  // ✅ Sheet daha opak (arkadaki yazılar gözükmesin)
  sheet: {
    width: "86%",
    borderRadius: 18,
    padding: 12,
    backgroundColor: "rgba(10,18,32,0.96)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.12)",
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
  sheetTitle: {
    color: "rgba(255,255,255,0.92)",
    fontSize: 14,
    fontWeight: "900",
  },
  closeBtn: {
    width: 34,
    height: 34,
    borderRadius: 14,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.10)",
  },

  quickRow: {
    flexDirection: "row",
    gap: 10,
    paddingHorizontal: 6,
    paddingBottom: 10,
  },
  quickItem: {
    flex: 1,
    borderRadius: 14,
    padding: 10,
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.10)",
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
  },
  quickLabel: {
    color: "rgba(255,255,255,0.80)",
    fontSize: 12,
    fontWeight: "900",
  },

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
  langText: {
    color: "rgba(246,195,64,0.95)",
    fontSize: 12,
    fontWeight: "900",
  },

  sheetDivider: {
    height: 1,
    backgroundColor: "rgba(255,255,255,0.10)",
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

  itemTitle: {
    color: "rgba(255,255,255,0.92)",
    fontSize: 13,
    fontWeight: "900",
  },
  itemTitleDanger: {
    color: "rgba(252,165,165,0.95)",
  },
  itemSub: {
    marginTop: 3,
    color: "rgba(255,255,255,0.55)",
    fontSize: 11.5,
    fontWeight: "700",
  },
});
