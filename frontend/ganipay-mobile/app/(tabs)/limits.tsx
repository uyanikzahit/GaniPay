import React, { useMemo, useState } from "react";
import { View, Text, StyleSheet, ScrollView, Pressable, Switch, Alert } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";

const BG = "#0B1220";
const CARD = "rgba(255,255,255,0.06)";
const BORDER = "rgba(255,255,255,0.10)";
const GOLD = "rgba(246,195,64,1)";
const MUTED = "rgba(255,255,255,0.60)";

type LimitRow = {
  key: string;
  title: string;
  subtitle: string;
  cap: number;      // total limit
  used: number;     // used amount
  currency: "TRY";
};

function clamp01(v: number) {
  if (v < 0) return 0;
  if (v > 1) return 1;
  return v;
}

export default function LimitsScreen() {
  const router = useRouter();
  const [spendingLock, setSpendingLock] = useState(false);

  const limits = useMemo<LimitRow[]>(
    () => [
      { key: "daily", title: "Daily limit", subtitle: "Outgoing today", cap: 15000, used: 2350, currency: "TRY" },
      { key: "monthly", title: "Monthly limit", subtitle: "Outgoing this month", cap: 100000, used: 17850, currency: "TRY" },
      { key: "single", title: "Single transfer", subtitle: "Maximum per transfer", cap: 5000, used: 0, currency: "TRY" },
    ],
    []
  );

  const onEdit = () => Alert.alert("Coming soon", "Limit management will be available soon.");

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
            <Text style={styles.cardHint}>See your cap, usage and remaining amount.</Text>
          </View>
          <Pressable onPress={onEdit} style={({ pressed }) => [styles.miniBtn, pressed && { opacity: 0.9 }]}>
            <Ionicons name="create-outline" size={16} color={GOLD} />
            <Text style={styles.miniBtnText}>Edit</Text>
          </Pressable>
        </View>

        {limits.map((l) => {
          const remaining = Math.max(0, l.cap - l.used);
          const ratio = clamp01(l.cap === 0 ? 0 : l.used / l.cap);

          return (
            <View key={l.key} style={styles.limitCard}>
              <View style={styles.limitHeader}>
                <View style={styles.limitIcon}>
                  <Ionicons name="speedometer-outline" size={18} color={GOLD} />
                </View>
                <View style={{ flex: 1 }}>
                  <Text style={styles.limitTitle}>{l.title}</Text>
                  <Text style={styles.limitSub}>{l.subtitle}</Text>
                </View>
                <Text style={styles.limitCap}>₺{l.cap.toLocaleString("tr-TR")}</Text>
              </View>

              <View style={styles.progressTrack}>
                <View style={[styles.progressFill, { width: `${Math.round(ratio * 100)}%` }]} />
              </View>

              <View style={styles.metaRow}>
                <View style={styles.metaItem}>
                  <Text style={styles.metaK}>Used</Text>
                  <Text style={styles.metaV}>₺{l.used.toLocaleString("tr-TR")}</Text>
                </View>

                <View style={styles.metaItem}>
                  <Text style={styles.metaK}>Remaining</Text>
                  <Text style={[styles.metaV, { color: "rgba(255,255,255,0.92)" }]}>
                    ₺{remaining.toLocaleString("tr-TR")}
                  </Text>
                </View>
              </View>
            </View>
          );
        })}
      </View>

      <View style={styles.card}>
        <View style={styles.tipRow}>
          <Ionicons name="information-circle-outline" size={18} color={GOLD} />
          <Text style={styles.tipText}>
            Limits may vary by verification level and risk checks. Remaining amount updates after each transaction.
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

  limitCard: {
    marginTop: 12,
    borderRadius: 16,
    padding: 12,
    backgroundColor: "rgba(0,0,0,0.18)",
    borderWidth: 1,
    borderColor: BORDER,
  },
  limitHeader: { flexDirection: "row", alignItems: "center" },

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
  limitCap: { color: "rgba(255,255,255,0.90)", fontWeight: "900" },

  progressTrack: {
    marginTop: 12,
    height: 10,
    borderRadius: 999,
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: BORDER,
    overflow: "hidden",
  },
  progressFill: {
    height: "100%",
    borderRadius: 999,
    backgroundColor: "rgba(246,195,64,0.35)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.40)",
  },

  metaRow: { flexDirection: "row", marginTop: 12 },
  metaItem: {
    flex: 1,
    borderRadius: 14,
    padding: 10,
    backgroundColor: "rgba(255,255,255,0.04)",
    borderWidth: 1,
    borderColor: BORDER,
    marginRight: 10,
  },
  metaK: { color: MUTED, fontWeight: "900", fontSize: 11.5 },
  metaV: { marginTop: 6, color: GOLD, fontWeight: "900", fontSize: 12 },

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
