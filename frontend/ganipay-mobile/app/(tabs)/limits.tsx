import React from "react";
import { View, Text, StyleSheet, ScrollView } from "react-native";
import { Ionicons } from "@expo/vector-icons";

const BG = "#0B1220";
const CARD = "rgba(255,255,255,0.06)";
const BORDER = "rgba(255,255,255,0.10)";
const GOLD = "rgba(246,195,64,1)";

export default function LimitsScreen() {
  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.container}>
      <Text style={styles.title}>Spending limits</Text>
      <Text style={styles.sub}>Daily/Monthly limits, rules (mock)</Text>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Daily limits</Text>

        <View style={styles.limitRow}>
          <View style={styles.left}>
            <Ionicons name="swap-horizontal-outline" size={18} color={GOLD} />
            <Text style={styles.label}>Transfers</Text>
          </View>
          <Text style={styles.value}>₺0 / ₺10,000</Text>
        </View>

        <View style={styles.limitRow}>
          <View style={styles.left}>
            <Ionicons name="wallet-outline" size={18} color={GOLD} />
            <Text style={styles.label}>Top ups</Text>
          </View>
          <Text style={styles.value}>₺0 / ₺10,000</Text>
        </View>

        <Text style={styles.hint}>These values are mock. Later we’ll read from Limits service.</Text>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Rules</Text>

        <View style={styles.rule}>
          <Ionicons name="checkmark-circle-outline" size={18} color="rgba(255,255,255,0.75)" />
          <Text style={styles.ruleText}>Additional checks for first-time transfers</Text>
        </View>

        <View style={styles.rule}>
          <Ionicons name="checkmark-circle-outline" size={18} color="rgba(255,255,255,0.75)" />
          <Text style={styles.ruleText}>Risk-based limits for unusual behavior</Text>
        </View>

        <View style={styles.rule}>
          <Ionicons name="checkmark-circle-outline" size={18} color="rgba(255,255,255,0.75)" />
          <Text style={styles.ruleText}>Optional KYC/AML enforcement (future)</Text>
        </View>
      </View>

      <View style={{ height: 26 }} />
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  page: { flex: 1, backgroundColor: BG },
  container: { padding: 16, paddingBottom: 28 },

  title: { color: "rgba(255,255,255,0.92)", fontSize: 20, fontWeight: "900" },
  sub: { marginTop: 6, color: "rgba(255,255,255,0.60)", fontSize: 12.5, fontWeight: "700" },

  card: {
    marginTop: 14,
    borderRadius: 18,
    padding: 14,
    backgroundColor: CARD,
    borderWidth: 1,
    borderColor: BORDER,
  },
  cardTitle: { color: "rgba(255,255,255,0.92)", fontSize: 14, fontWeight: "900" },

  limitRow: { marginTop: 12, flexDirection: "row", justifyContent: "space-between", alignItems: "center" },
  left: { flexDirection: "row", gap: 10, alignItems: "center" },
  label: { color: "rgba(255,255,255,0.80)", fontWeight: "900" },
  value: { color: GOLD, fontWeight: "900" },

  hint: { marginTop: 10, color: "rgba(255,255,255,0.55)", fontSize: 11.5, fontWeight: "700" },

  rule: { marginTop: 12, flexDirection: "row", alignItems: "center", gap: 10 },
  ruleText: { color: "rgba(255,255,255,0.75)", fontWeight: "700", lineHeight: 18 },
});
