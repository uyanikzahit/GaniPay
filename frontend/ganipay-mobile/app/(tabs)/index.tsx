// app/(tabs)/index.tsx
import React, { useEffect, useState } from "react";
import { View, Text, StyleSheet, TouchableOpacity, ScrollView, Platform } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { LinearGradient } from "expo-linear-gradient";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { Colors } from "../theme/colors";
import { SessionKeys } from "../../constants/storage";

export default function HomeScreen() {
  // 🔽 SADECE BURASI DİNAMİK YAPILDI
  const [userName, setUserName] = useState<string>("—");
  const [balance, setBalance] = useState<string>("₺0.00");

  useEffect(() => {
    (async () => {
      try {
        const userRaw = await AsyncStorage.getItem(SessionKeys.user);
        const walletsRaw = await AsyncStorage.getItem(SessionKeys.accountId);
        const currency = (await AsyncStorage.getItem(SessionKeys.currency)) || "TRY";

        if (userRaw) {
          const user = JSON.parse(userRaw);
          const fullName = `${user.firstName ?? ""} ${user.lastName ?? ""}`.trim();
          if (fullName) setUserName(fullName);
        }

        // şimdilik login response’tan gelen balance
        // (sonra transfer/topup sonrası API’den güncellenecek)
        if (walletsRaw) {
          // backend response’unda balance varsa burası çalışır
          // yoksa default kalır
          const wallet = JSON.parse(walletsRaw);
          if (wallet?.balance !== undefined) {
            const formatted = `${currency === "TRY" ? "₺" : ""}${Number(wallet.balance).toFixed(2)}`;
            setBalance(formatted);
          }
        }
      } catch (e) {
        console.warn("HomeScreen session read error:", e);
      }
    })();
  }, []);
  // 🔼 SADECE BURASI DİNAMİK YAPILDI

  return (
    <View style={styles.container}>
      <LinearGradient
        colors={[Colors.bg, "#0B1220", "#12223E"]}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        style={styles.hero}
      >
        <LinearGradient
          colors={["rgba(246,195,64,0.55)", "rgba(246,195,64,0)"]}
          start={{ x: 0.15, y: 0 }}
          end={{ x: 0.85, y: 1 }}
          style={styles.goldGlow}
        />

        <ScrollView contentContainerStyle={styles.content} showsVerticalScrollIndicator={false}>
          <View style={styles.headerCard}>
            <View style={styles.headerRow}>
              <View>
                <Text style={styles.welcome}>Welcome,</Text>
                <Text style={styles.name}>{userName}</Text>
              </View>

              <View style={{ alignItems: "flex-end" }}>
                <Text style={styles.balanceLabel}>Balance</Text>
                <Text style={styles.balance}>{balance}</Text>
              </View>
            </View>

            <Text style={styles.subHint}>Your wallet is ready for quick actions.</Text>
          </View>

          <View style={styles.actionGrid}>
            <ActionTile title="Top Up" icon="wallet-outline" variant="teal" onPress={() => {}} />
            <ActionTile title="Transfer" icon="swap-horizontal-outline" variant="blue" onPress={() => {}} />
            <ActionTile title="Pay Bills" icon="receipt-outline" variant="purple" disabled />
            <ActionTile title="Partner Stores" icon="storefront-outline" variant="orange" disabled />
          </View>

          <View style={styles.sectionHeader}>
            <Text style={styles.sectionTitle}>Last Transactions</Text>
            <Text style={styles.sectionLink}>View all</Text>
          </View>

          <TxItem title="Top Up" amount="+ ₺200.00" meta="Today • Pending" />
          <TxItem title="Transfer" amount="- ₺120.00" meta="Today • Completed" />
          <TxItem title="Top Up" amount="+ ₺50.00" meta="Yesterday • Completed" />

          <View style={{ height: 28 }} />
        </ScrollView>
      </LinearGradient>
    </View>
  );
}

function ActionTile({
  title,
  icon,
  onPress,
  disabled,
  variant,
}: {
  title: string;
  icon: React.ComponentProps<typeof Ionicons>["name"];
  onPress?: () => void;
  disabled?: boolean;
  variant: "teal" | "blue" | "purple" | "orange";
}) {
  const v = variantStyles[variant];

  return (
    <TouchableOpacity
      style={[styles.actionTile, disabled && styles.actionTileDisabled]}
      onPress={onPress}
      disabled={disabled}
      activeOpacity={0.85}
    >
      <View style={[styles.iconBubble, { backgroundColor: v.bubbleBg, borderColor: v.bubbleBorder }]}>
        <Ionicons name={icon} size={24} color={v.icon} />
      </View>

      <Text style={[styles.actionText, disabled && styles.disabledText]}>{title}</Text>
    </TouchableOpacity>
  );
}

const variantStyles = {
  teal: { bubbleBg: "rgba(45,183,163,0.14)", bubbleBorder: "rgba(45,183,163,0.22)", icon: "rgba(45,183,163,1)" },
  blue: { bubbleBg: "rgba(37,99,235,0.14)", bubbleBorder: "rgba(37,99,235,0.22)", icon: "rgba(37,99,235,1)" },
  purple: { bubbleBg: "rgba(109,40,217,0.14)", bubbleBorder: "rgba(109,40,217,0.22)", icon: "rgba(109,40,217,1)" },
  orange: { bubbleBg: "rgba(234,88,12,0.14)", bubbleBorder: "rgba(234,88,12,0.22)", icon: "rgba(234,88,12,1)" },
} as const;

function TxItem({ title, amount, meta }: { title: string; amount: string; meta: string }) {
  const isPositive = amount.trim().startsWith("+");
  return (
    <View style={styles.txItem}>
      <View style={{ gap: 4 }}>
        <Text style={styles.txTitle}>{title}</Text>
        <Text style={styles.txMeta}>{meta}</Text>
      </View>
      <Text style={[styles.txAmount, isPositive ? styles.positive : styles.negative]}>{amount}</Text>
    </View>
  );
}

const shadow = Platform.select({
  ios: {
    shadowColor: "#000",
    shadowOpacity: 0.22,
    shadowRadius: 18,
    shadowOffset: { width: 0, height: 12 },
  },
  android: { elevation: 10 },
  web: { boxShadow: "0px 14px 40px rgba(0,0,0,0.28)" } as any,
});

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.bg },

  hero: {
    flex: 1,
    paddingHorizontal: 16,
    paddingTop: 14,
    overflow: "hidden",
  },

  goldGlow: {
    position: "absolute",
    top: -50,
    left: 0,
    right: 0,
    height: 280,
    borderBottomLeftRadius: 240,
    borderBottomRightRadius: 240,
    opacity: 0.9,
    transform: [{ scaleX: 1.2 }],
  },

  content: { paddingBottom: 28 },

  headerCard: {
    borderRadius: 18,
    padding: 16,
    marginBottom: 14,
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.10)",
    ...shadow,
  },
  headerRow: { flexDirection: "row", justifyContent: "space-between" },
  welcome: { color: "rgba(255,255,255,0.80)", fontWeight: "800" },
  name: { color: "rgba(255,255,255,0.96)", fontSize: 20, fontWeight: "900", marginTop: 2 },
  balanceLabel: { color: "rgba(255,255,255,0.72)", fontWeight: "800" },
  balance: { color: "rgba(255,255,255,0.96)", fontSize: 18, fontWeight: "900", marginTop: 2 },
  subHint: { marginTop: 10, color: "rgba(255,255,255,0.62)", fontSize: 12, fontWeight: "700" },

  actionGrid: { flexDirection: "row", flexWrap: "wrap", gap: 12, marginTop: 6, marginBottom: 6 },

  actionTile: {
    borderRadius: 16,
    paddingVertical: 16,
    paddingHorizontal: 14,
    width: "48%",
    alignItems: "center",
    justifyContent: "center",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.10)",
    backgroundColor: "rgba(255,255,255,0.06)",
    position: "relative",
    ...shadow,
  },
  actionTileDisabled: { opacity: 1 },

  iconBubble: {
    width: 46,
    height: 46,
    borderRadius: 23,
    alignItems: "center",
    justifyContent: "center",
    marginBottom: 10,
    borderWidth: 1,
  },
  actionText: { fontWeight: "900", fontSize: 13, color: "rgba(255,255,255,0.92)" },
  disabledText: { color: "rgba(255,255,255,0.92)" },

  sectionHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginTop: 14,
  },
  sectionTitle: { fontSize: 16, fontWeight: "900", color: "rgba(255,255,255,0.92)" },
  sectionLink: { color: "rgba(246,195,64,0.95)", fontWeight: "900" },

  txItem: {
    borderRadius: 14,
    padding: 14,
    marginTop: 10,
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.10)",
    backgroundColor: "rgba(255,255,255,0.06)",
    ...shadow,
  },
  txTitle: { fontWeight: "900", color: "rgba(255,255,255,0.92)" },
  txMeta: { color: "rgba(255,255,255,0.60)", fontSize: 12, fontWeight: "700" },

  txAmount: { fontWeight: "900", color: "rgba(255,255,255,0.92)" },
  positive: { color: "rgba(134, 239, 172, 0.95)" },
  negative: { color: "rgba(252, 165, 165, 0.95)" },
});
