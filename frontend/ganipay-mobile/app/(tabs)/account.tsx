import React, { useMemo } from "react";
import { View, Text, StyleSheet, ScrollView, Pressable, Alert } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";

const BG = "#0B1220";
const CARD = "rgba(255,255,255,0.06)";
const BORDER = "rgba(255,255,255,0.10)";
const GOLD = "rgba(246,195,64,1)";
const MUTED = "rgba(255,255,255,0.60)";
const SOFT = "rgba(255,255,255,0.35)";

export default function AccountScreen() {
  const router = useRouter();

  const user = useMemo(
    () => ({
      fullName: "Mehmet Zahit",
      phone: "+90 506 *** ** 00",
      email: "zahit.test@ganipay.io",
      tier: "Standard",
    }),
    []
  );

  const onAction = (title: string) => Alert.alert(title, "This feature will be available soon.");

  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.container} showsVerticalScrollIndicator={false}>
      <Text style={styles.title}>Account</Text>
      <Text style={styles.sub}>Profile & settings</Text>

      <View style={styles.card}>
        <View style={styles.profileRow}>
          <View style={styles.avatar}>
            <Ionicons name="person-outline" size={22} color={GOLD} />
          </View>
          <View style={{ flex: 1 }}>
            <Text style={styles.profileName}>{user.fullName}</Text>
            <Text style={styles.profileSub}>{user.tier} • Verified</Text>
          </View>
          <Pressable onPress={() => onAction("Edit profile")} style={({ pressed }) => [styles.miniBtn, pressed && { opacity: 0.9 }]}>
            <Ionicons name="create-outline" size={16} color={GOLD} />
            <Text style={styles.miniBtnText}>Edit</Text>
          </Pressable>
        </View>

        <View style={styles.infoRow}>
          <Ionicons name="call-outline" size={16} color={SOFT} />
          <Text style={styles.infoText}>{user.phone}</Text>
        </View>

        <View style={styles.infoRow}>
          <Ionicons name="mail-outline" size={16} color={SOFT} />
          <Text style={styles.infoText}>{user.email}</Text>
        </View>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Preferences</Text>
        <Text style={styles.cardHint}>Manage your app preferences and privacy.</Text>

        <Pressable onPress={() => router.push("/(tabs)/security")} style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}>
          <View style={styles.iconWrap}>
            <Ionicons name="shield-checkmark-outline" size={18} color={GOLD} />
          </View>
          <View style={{ flex: 1 }}>
            <Text style={styles.itemTitle}>Security</Text>
            <Text style={styles.itemSub}>Authentication and safeguards</Text>
          </View>
          <Ionicons name="chevron-forward" size={16} color={SOFT} />
        </Pressable>

        <Pressable onPress={() => router.push("/(tabs)/limits")} style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}>
          <View style={styles.iconWrap}>
            <Ionicons name="speedometer-outline" size={18} color={GOLD} />
          </View>
          <View style={{ flex: 1 }}>
            <Text style={styles.itemTitle}>Spending limits</Text>
            <Text style={styles.itemSub}>Daily and monthly caps</Text>
          </View>
          <Ionicons name="chevron-forward" size={16} color={SOFT} />
        </Pressable>

        <Pressable onPress={() => onAction("Legal")} style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}>
          <View style={styles.iconWrap}>
            <Ionicons name="document-text-outline" size={18} color={GOLD} />
          </View>
          <View style={{ flex: 1 }}>
            <Text style={styles.itemTitle}>Legal</Text>
            <Text style={styles.itemSub}>Terms and privacy policy</Text>
          </View>
          <Ionicons name="chevron-forward" size={16} color={SOFT} />
        </Pressable>
      </View>

      <View style={styles.card}>
        <Pressable
          onPress={() => onAction("Log out")}
          style={({ pressed }) => [styles.dangerBtn, pressed && { opacity: 0.9 }]}
        >
          <Ionicons name="log-out-outline" size={18} color="rgba(255,255,255,0.92)" />
          <Text style={styles.dangerText}>Log out</Text>
        </Pressable>
      </View>

      <View style={{ height: 26 }} />
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  page: { flex: 1, backgroundColor: BG },
  container: { padding: 16, paddingBottom: 28 },

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

  profileRow: { flexDirection: "row", alignItems: "center" },
  avatar: {
    width: 42,
    height: 42,
    borderRadius: 18,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
    marginRight: 10,
  },
  profileName: { color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 14 },
  profileSub: { marginTop: 4, color: MUTED, fontWeight: "700", fontSize: 11.5 },

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

  infoRow: { marginTop: 12, flexDirection: "row", alignItems: "center" },
  infoText: { marginLeft: 10, color: "rgba(255,255,255,0.86)", fontWeight: "800" },

  cardTitle: { color: "rgba(255,255,255,0.92)", fontSize: 14, fontWeight: "900" },
  cardHint: { marginTop: 6, color: "rgba(255,255,255,0.55)", fontSize: 11.5, fontWeight: "700", lineHeight: 16 },

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

  dangerBtn: {
    borderRadius: 16,
    paddingVertical: 12,
    paddingHorizontal: 14,
    backgroundColor: "rgba(255,71,87,0.16)",
    borderWidth: 1,
    borderColor: "rgba(255,71,87,0.26)",
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
  },
  dangerText: { marginLeft: 8, color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 13.5 },
});
