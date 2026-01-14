import React, { useMemo, useState } from "react";
import { View, Text, StyleSheet, ScrollView, Pressable, Switch, Alert } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";

const BG = "#0B1220";
const CARD = "rgba(255,255,255,0.06)";
const BORDER = "rgba(255,255,255,0.10)";
const GOLD = "rgba(246,195,64,1)";
const MUTED = "rgba(255,255,255,0.60)";

type LimitRow = { key: string; title: string; subtitle: string; value: number; currency: "TRY" };

export default function LimitsScreen() {
  const router = useRouter();

  const [spendingLock, setSpendingLock] = useState(false);

  const limits = useMemo<LimitRow[]>(
    () => [
      { key: "daily", title: "Daily limit", subtitle: "Maximum outgoing per day", value: 15000, currency: "TRY" },
      { key: "monthly", title: "Monthly limit", subtitle: "Maximum outgoing per month", value: 100000, currency: "TRY" },
      { key: "single", title: "Single transfer", subtitle: "Maximum per transfer", value: 5000, currency: "TRY" },
    ],
    []
  );

  const onEdit = () => {
    Alert.alert("Coming soon", "Limit management will be available soon.");
  };

  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.container} showsVerticalScrollIndicator={false}>
      <View style={styles.headerRow}>
        <Pressable onPress={() => router.back()} style={({ pressed }) => [styles.backBtn, pressed && { opacity: 0.9 }]}>
          <Ionicons name="chevron-back" size={18} color="rgba(255,255,255,0.85)" />
        </Pressable>

        <View style={{ flex: 1 }}>
          <Text style={styles.title}>Spending limits</Text>
          <Text style={styles.sub}>Control your outgoing transactions</Text>
        </View>
      </View>

      <View style={styles.card}>
        <View style={styles.rowBetween}>
          <View style={{ flex: 1 }}>
            <Text style={styles.cardTitle}>Spending lock</Text>
            <Text style={styles.cardHint}>Temporarily block outgoing transfers and payments.</Text>
          </View>
          <Switch value={spendingLock} onValueChange={setSpendingLock} />
        </View>
      </View>

      <View style={styles.card}>
        <View style={styles.rowBetween}>
          <View>
            <Text style={styles.cardTitle}>Your limits</Text>
            <Text style={styles.cardHint}>These limits help you control risk and spending.</Text>
          </View>
          <Pressable onPress={onEdit} style={({ pressed }) => [styles.miniBtn, pressed && { opacity: 0.9 }]}>
            <Ionicons name="create-outline" size={16} color={GOLD} />
            <Text style={styles.miniBtnText}>Edit</Text>
          </Pressable>
        </View>

        {limits.map((l) => (
          <View key={l.key} style={styles.limitRow}>
            <View style={styles.limitIcon}>
              <Ionicons name="speedometer-outline" size={18} color={GOLD} />
            </View>
            <View style={{ flex: 1 }}>
              <Text style={styles.limitTitle}>{l.title}</Text>
              <Text style={styles.limitSub}>{l.subtitle}</Text>
            </View>
            <Text style={styles.limitValue}>₺{l.value.toLocaleString("tr-TR")}</Text>
          </View>
        ))}
      </View>

      <View style={styles.card}>
        <View style={styles.tipRow}>
          <Ionicons name="information-circle-outline" size={18} color={GOLD} />
          <Text style={styles.tipText}>
            Limits may vary based on verification level and risk checks. You can adjust them in settings.
          </Text>
        </View>
      </View>

      <View style={{ height: 26 }} />
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  page: { flex: 1, backgroundColor: BG },
  container: { padding: 16, paddingBottom: 28 },

  headerRow: { flexDirection: "row", alignItems: "center" },
  backBtn: {
    width: 38,
    height: 38,
    borderRadius: 14,
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: BORDER,
    alignItems: "center",
    justifyContent: "center",
    marginRight: 10,
  },

  title: { color: "rgba(255,255,255,0.92)", fontSize: 20, fontWeight: "900" },
  sub: { marginTop: 6, color: MUTED, fontSize: 12.5, fontWeight: "700" },

  card: {
    marginTop: 14,
    borderRadius: 18,
    padding: 14,
    backgroundColor: CARD,
    borderWidth: 1,
    borderColor: BORDER,
  },

  rowBetween: { flexDirection: "row", alignItems: "center", justifyContent: "space-between" },

  cardTitle: { color: "rgba(255,255,255,0.92)", fontSize: 14, fontWeight: "900" },
  cardHint: { marginTop: 6, color: "rgba(255,255,255,0.55)", fontSize: 11.5, fontWeight: "700", lineHeight: 16 },

  miniBtn: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderRadius: 999,
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
  },
  miniBtnText: { marginLeft: 6, color: "rgba(255,255,255,0.86)", fontWeight: "900", fontSize: 11.5 },

  limitRow: {
    marginTop: 12,
    flexDirection: "row",
    alignItems: "center",
    padding: 12,
    borderRadius: 16,
    backgroundColor: "rgba(0,0,0,0.18)",
    borderWidth: 1,
    borderColor: BORDER,
  },
  limitIcon: {
    width: 36,
    height: 36,
    borderRadius: 14,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
    marginRight: 10,
  },
  limitTitle: { color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 13 },
  limitSub: { marginTop: 4, color: MUTED, fontWeight: "700", fontSize: 11.5 },
  limitValue: { color: "rgba(255,255,255,0.90)", fontWeight: "900" },

  tipRow: {
    borderRadius: 14,
    padding: 12,
    backgroundColor: "rgba(0,0,0,0.18)",
    borderWidth: 1,
    borderColor: BORDER,
    flexDirection: "row",
    alignItems: "center",
  },
  tipText: { marginLeft: 10, color: "rgba(255,255,255,0.78)", fontWeight: "800", fontSize: 12, lineHeight: 16 },
});
