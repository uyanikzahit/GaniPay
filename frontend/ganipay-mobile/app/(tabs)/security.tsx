import React, { useState } from "react";
import { View, Text, StyleSheet, ScrollView, Pressable, Switch, Alert } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";

const BG = "#0B1220";
const CARD = "rgba(255,255,255,0.06)";
const BORDER = "rgba(255,255,255,0.10)";
const GOLD = "rgba(246,195,64,1)";
const MUTED = "rgba(255,255,255,0.60)";
const SOFT = "rgba(255,255,255,0.35)";

export default function SecurityScreen() {
  const router = useRouter();

  const [biometric, setBiometric] = useState(true);
  const [deviceLock, setDeviceLock] = useState(true);
  const [highRiskConfirm, setHighRiskConfirm] = useState(true);

  const onAction = (title: string) => Alert.alert(title, "This feature will be available soon.");

  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.container} showsVerticalScrollIndicator={false}>
      <View style={styles.headerRow}>
        <Pressable onPress={() => router.back()} style={({ pressed }) => [styles.backBtn, pressed && { opacity: 0.9 }]}>
          <Ionicons name="chevron-back" size={18} color="rgba(255,255,255,0.85)" />
        </Pressable>

        <View style={{ flex: 1 }}>
          <Text style={styles.title}>Security</Text>
          <Text style={styles.sub}>Protect your account and wallet</Text>
        </View>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Authentication</Text>
        <Text style={styles.cardHint}>Configure how you approve sensitive actions.</Text>

        <View style={styles.toggleRow}>
          <View style={{ flex: 1 }}>
            <Text style={styles.toggleTitle}>Biometric approval</Text>
            <Text style={styles.toggleSub}>Use FaceID/TouchID where supported</Text>
          </View>
          <Switch value={biometric} onValueChange={setBiometric} />
        </View>

        <View style={styles.toggleRow}>
          <View style={{ flex: 1 }}>
            <Text style={styles.toggleTitle}>Device lock</Text>
            <Text style={styles.toggleSub}>Require device authentication</Text>
          </View>
          <Switch value={deviceLock} onValueChange={setDeviceLock} />
        </View>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Transaction safeguards</Text>
        <Text style={styles.cardHint}>Extra confirmations for risky patterns.</Text>

        <View style={styles.toggleRow}>
          <View style={{ flex: 1 }}>
            <Text style={styles.toggleTitle}>High-risk confirmation</Text>
            <Text style={styles.toggleSub}>Ask confirmation for unusual transfers</Text>
          </View>
          <Switch value={highRiskConfirm} onValueChange={setHighRiskConfirm} />
        </View>

        <View style={styles.divider} />

        <Pressable onPress={() => onAction("Trusted devices")} style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}>
          <View style={styles.iconWrap}>
            <Ionicons name="phone-portrait-outline" size={18} color={GOLD} />
          </View>
          <View style={{ flex: 1 }}>
            <Text style={styles.itemTitle}>Trusted devices</Text>
            <Text style={styles.itemSub}>Manage devices linked to your account</Text>
          </View>
          <Ionicons name="chevron-forward" size={16} color={SOFT} />
        </Pressable>

        <Pressable onPress={() => onAction("Change password")} style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}>
          <View style={styles.iconWrap}>
            <Ionicons name="key-outline" size={18} color={GOLD} />
          </View>
          <View style={{ flex: 1 }}>
            <Text style={styles.itemTitle}>Change password</Text>
            <Text style={styles.itemSub}>Update your login credentials</Text>
          </View>
          <Ionicons name="chevron-forward" size={16} color={SOFT} />
        </Pressable>
      </View>

      <View style={styles.card}>
        <View style={styles.tipRow}>
          <Ionicons name="shield-checkmark-outline" size={18} color={GOLD} />
          <Text style={styles.tipText}>Security settings may affect approval steps in transfers and top ups.</Text>
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

  cardTitle: { color: "rgba(255,255,255,0.92)", fontSize: 14, fontWeight: "900" },
  cardHint: { marginTop: 6, color: "rgba(255,255,255,0.55)", fontSize: 11.5, fontWeight: "700", lineHeight: 16 },

  toggleRow: {
    marginTop: 12,
    borderRadius: 16,
    padding: 12,
    backgroundColor: "rgba(0,0,0,0.18)",
    borderWidth: 1,
    borderColor: BORDER,
    flexDirection: "row",
    alignItems: "center",
  },
  toggleTitle: { color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 13 },
  toggleSub: { marginTop: 4, color: MUTED, fontWeight: "700", fontSize: 11.5 },

  divider: { height: 1, backgroundColor: BORDER, marginTop: 16, marginBottom: 12 },

  item: { flexDirection: "row", alignItems: "center", paddingVertical: 12, paddingHorizontal: 6, borderRadius: 14 },
  iconWrap: {
    width: 34,
    height: 34,
    borderRadius: 14,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
    marginRight: 10,
  },
  itemTitle: { color: "rgba(255,255,255,0.92)", fontSize: 13, fontWeight: "900" },
  itemSub: { marginTop: 3, color: "rgba(255,255,255,0.55)", fontSize: 11.5, fontWeight: "700" },

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
