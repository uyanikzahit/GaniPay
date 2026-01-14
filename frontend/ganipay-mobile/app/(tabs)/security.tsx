import React from "react";
import { View, Text, StyleSheet, ScrollView, Pressable, Alert } from "react-native";
import { Ionicons } from "@expo/vector-icons";

const BG = "#0B1220";
const CARD = "rgba(255,255,255,0.06)";
const BORDER = "rgba(255,255,255,0.10)";
const GOLD = "rgba(246,195,64,1)";

export default function SecurityScreen() {
  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.container}>
      <Text style={styles.title}>Security</Text>
      <Text style={styles.sub}>PIN, biometrics, sign-in options (mock)</Text>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Sign-in protection</Text>

        <Pressable
          onPress={() => Alert.alert("Mock", "Change PIN (not implemented yet).")}
          style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}
        >
          <View style={styles.iconWrap}>
            <Ionicons name="key-outline" size={18} color={GOLD} />
          </View>
          <View style={{ flex: 1 }}>
            <Text style={styles.itemTitle}>Change PIN</Text>
            <Text style={styles.itemSub}>Update your wallet PIN</Text>
          </View>
          <Ionicons name="chevron-forward" size={16} color="rgba(255,255,255,0.35)" />
        </Pressable>

        <Pressable
          onPress={() => Alert.alert("Mock", "Biometrics (not implemented yet).")}
          style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}
        >
          <View style={styles.iconWrap}>
            <Ionicons name="finger-print-outline" size={18} color={GOLD} />
          </View>
          <View style={{ flex: 1 }}>
            <Text style={styles.itemTitle}>Biometrics</Text>
            <Text style={styles.itemSub}>Face ID / Touch ID preferences</Text>
          </View>
          <Ionicons name="chevron-forward" size={16} color="rgba(255,255,255,0.35)" />
        </Pressable>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Device trust</Text>
        <Text style={styles.cardHint}>
          In the login workflow we run device/risk checks. This screen will later list trusted devices.
        </Text>
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
  cardHint: { marginTop: 8, color: "rgba(255,255,255,0.55)", fontSize: 11.5, fontWeight: "700", lineHeight: 16 },

  item: { flexDirection: "row", alignItems: "center", gap: 10, paddingVertical: 12, paddingHorizontal: 6, borderRadius: 14 },
  iconWrap: {
    width: 34,
    height: 34,
    borderRadius: 14,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
  },
  itemTitle: { color: "rgba(255,255,255,0.92)", fontSize: 13, fontWeight: "900" },
  itemSub: { marginTop: 3, color: "rgba(255,255,255,0.55)", fontSize: 11.5, fontWeight: "700" },
});
