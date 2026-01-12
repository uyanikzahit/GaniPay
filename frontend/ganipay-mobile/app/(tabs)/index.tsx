import React from "react";
import { View, Text, StyleSheet, TouchableOpacity, ScrollView } from "react-native";
import { Ionicons } from "@expo/vector-icons";

export default function HomeScreen() {
  const userName = "Ilyas";
  const balance = "₺0.00";

  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.container}>
      {/* Header Card */}
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

      {/* 4 Action Tiles */}
      <View style={styles.actionGrid}>
        <ActionTile title="Top Up" icon="wallet-outline" variant="teal" onPress={() => {}} />
        <ActionTile title="Transfer" icon="swap-horizontal-outline" variant="blue" onPress={() => {}} />
        <ActionTile title="Pay Bills" icon="receipt-outline" variant="purple" disabled />
        <ActionTile title="Partner Stores" icon="storefront-outline" variant="orange" disabled />
      </View>

      {/* Last 3 Transactions */}
      <View style={styles.sectionHeader}>
        <Text style={styles.sectionTitle}>Last Transactions</Text>
        <Text style={styles.sectionLink}>View all</Text>
      </View>

      <TxItem title="Top Up" amount="+ ₺200.00" meta="Today • Pending" />
      <TxItem title="Transfer" amount="- ₺120.00" meta="Today • Completed" />
      <TxItem title="Top Up" amount="+ ₺50.00" meta="Yesterday • Completed" />
    </ScrollView>
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
      style={styles.actionTile}
      onPress={onPress}
      disabled={disabled}
      activeOpacity={0.85}
    >
      <View style={[styles.iconBubble, { backgroundColor: v.bubbleBg }]}>
        <Ionicons name={icon} size={24} color={v.icon} />
      </View>

      <Text style={styles.actionText}>{title}</Text>

    </TouchableOpacity>
  );
}

const variantStyles = {
  teal: { bubbleBg: "#E9FBF7", icon: "#2DB7A3" },
  blue: { bubbleBg: "#EEF4FF", icon: "#2563EB" },
  purple: { bubbleBg: "#F4F1FF", icon: "#6D28D9" },
  orange: { bubbleBg: "#FFF3E8", icon: "#EA580C" },
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

const styles = StyleSheet.create({
  page: { flex: 1, backgroundColor: "#F6F7F9" },
  container: { padding: 16, paddingBottom: 28 },

  headerCard: {
    backgroundColor: "#297967",
    borderRadius: 16,
    padding: 16,
    marginBottom: 14,
  },
  headerRow: { flexDirection: "row", justifyContent: "space-between" },
  welcome: { color: "white", opacity: 0.92 },
  name: { color: "white", fontSize: 20, fontWeight: "800", marginTop: 2 },
  balanceLabel: { color: "white", opacity: 0.92 },
  balance: { color: "white", fontSize: 18, fontWeight: "800", marginTop: 2 },
  subHint: { marginTop: 10, color: "white", opacity: 0.75, fontSize: 12 },

  actionGrid: { flexDirection: "row", flexWrap: "wrap", gap: 12, marginTop: 6, marginBottom: 6 },

  actionTile: {
    backgroundColor: "white",
    borderRadius: 14,
    paddingVertical: 16,
    paddingHorizontal: 14,
    width: "48%",
    alignItems: "center",
    justifyContent: "center",
    borderWidth: 1,
    borderColor: "#EEF0F3",
    position: "relative",
  },
  iconBubble: {
    width: 44,
    height: 44,
    borderRadius: 22,
    alignItems: "center",
    justifyContent: "center",
    marginBottom: 10,
  },
  actionText: { fontWeight: "800", fontSize: 13 },

  badge: {
    position: "absolute",
    top: 10,
    right: 10,
    fontSize: 10,
    fontWeight: "900",
    color: "#98A2B3",
  },

  sectionHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginTop: 14,
  },
  sectionTitle: { fontSize: 16, fontWeight: "900" },
  sectionLink: { color: "#2DB7A3", fontWeight: "800" },

  txItem: {
    backgroundColor: "white",
    borderRadius: 12,
    padding: 14,
    marginTop: 10,
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    borderWidth: 1,
    borderColor: "#EEF0F3",
  },
  txTitle: { fontWeight: "900" },
  txMeta: { color: "#667085", fontSize: 12, fontWeight: "600" },

  txAmount: { fontWeight: "900" },
  positive: { color: "#12B76A" },
  negative: { color: "#F04438" },
});
