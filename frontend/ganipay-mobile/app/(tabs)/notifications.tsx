import React, { useMemo, useState } from "react";
import { View, Text, StyleSheet, ScrollView, Pressable, Switch, Alert } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";

const BG = "#0B1220";
const CARD = "rgba(255,255,255,0.06)";
const BORDER = "rgba(255,255,255,0.10)";
const GOLD = "rgba(246,195,64,1)";
const MUTED = "rgba(255,255,255,0.60)";
const SOFT = "rgba(255,255,255,0.35)";

type NotiPreview = {
  id: string;
  title: string;
  desc: string;
  time: string;
  icon: React.ComponentProps<typeof Ionicons>["name"];
  tone: "info" | "warn" | "success";
};

export default function NotificationsScreen() {
  const router = useRouter();

  const [push, setPush] = useState(true);
  const [security, setSecurity] = useState(true);
  const [transactions, setTransactions] = useState(true);
  const [marketing, setMarketing] = useState(false);

  const previews = useMemo<NotiPreview[]>(
    () => [
      { id: "n1", title: "Security alert", desc: "New sign-in detected from a device.", time: "Today • 12:41", icon: "shield-checkmark-outline", tone: "warn" },
      { id: "n2", title: "Top up completed", desc: "Your wallet was funded successfully.", time: "Yesterday • 19:06", icon: "checkmark-circle-outline", tone: "success" },
      { id: "n3", title: "Weekly summary", desc: "Your spending overview is ready.", time: "Mon • 09:00", icon: "stats-chart-outline", tone: "info" },
    ],
    []
  );

  const pillStyle = (tone: NotiPreview["tone"]) => {
    const borderColor =
      tone === "success" ? "rgba(46,213,115,0.30)" : tone === "warn" ? "rgba(246,195,64,0.25)" : "rgba(52,152,219,0.25)";
    const backgroundColor =
      tone === "success" ? "rgba(46,213,115,0.10)" : tone === "warn" ? "rgba(246,195,64,0.10)" : "rgba(52,152,219,0.10)";
    return { borderColor, backgroundColor };
  };

  const onOpenCenter = () => Alert.alert("Notification center", "Full notification center will be available soon.");

  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.container} showsVerticalScrollIndicator={false}>
      <View style={styles.headerRow}>
        <Pressable onPress={() => router.back()} style={({ pressed }) => [styles.backBtn, pressed && { opacity: 0.9 }]}>
          <Ionicons name="chevron-back" size={18} color="rgba(255,255,255,0.85)" />
        </Pressable>

        <View style={{ flex: 1 }}>
          <Text style={styles.title}>Notifications</Text>
          <Text style={styles.sub}>Alerts & preferences</Text>
        </View>

        <Pressable onPress={onOpenCenter} style={({ pressed }) => [styles.miniBtn, pressed && { opacity: 0.9 }]}>
          <Ionicons name="notifications-outline" size={16} color={GOLD} />
          <Text style={styles.miniBtnText}>Center</Text>
        </Pressable>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Preferences</Text>
        <Text style={styles.cardHint}>Choose what you want to receive.</Text>

        <View style={styles.toggleRow}>
          <View style={{ flex: 1 }}>
            <Text style={styles.toggleTitle}>Push notifications</Text>
            <Text style={styles.toggleSub}>General updates and reminders</Text>
          </View>
          <Switch value={push} onValueChange={setPush} />
        </View>

        <View style={styles.toggleRow}>
          <View style={{ flex: 1 }}>
            <Text style={styles.toggleTitle}>Security alerts</Text>
            <Text style={styles.toggleSub}>Login, device and risk alerts</Text>
          </View>
          <Switch value={security} onValueChange={setSecurity} />
        </View>

        <View style={styles.toggleRow}>
          <View style={{ flex: 1 }}>
            <Text style={styles.toggleTitle}>Transaction updates</Text>
            <Text style={styles.toggleSub}>Top up / transfer status changes</Text>
          </View>
          <Switch value={transactions} onValueChange={setTransactions} />
        </View>

        <View style={styles.toggleRow}>
          <View style={{ flex: 1 }}>
            <Text style={styles.toggleTitle}>Partner offers</Text>
            <Text style={styles.toggleSub}>Campaigns and promotions</Text>
          </View>
          <Switch value={marketing} onValueChange={setMarketing} />
        </View>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Recent</Text>
        <Text style={styles.cardHint}>Latest updates from your account.</Text>

        {previews.map((p) => (
          <View key={p.id} style={styles.previewRow}>
            <View style={styles.previewIcon}>
              <Ionicons name={p.icon} size={18} color={GOLD} />
            </View>

            <View style={{ flex: 1 }}>
              <View style={styles.previewTop}>
                <Text style={styles.previewTitle}>{p.title}</Text>
                <View style={[styles.pill, pillStyle(p.tone)]}>
                  <Text style={styles.pillText}>{p.tone.toUpperCase()}</Text>
                </View>
              </View>
              <Text style={styles.previewDesc}>{p.desc}</Text>
              <Text style={styles.previewTime}>{p.time}</Text>
            </View>
          </View>
        ))}
      </View>

      <View style={styles.card}>
        <View style={styles.tipRow}>
          <Ionicons name="information-circle-outline" size={18} color={GOLD} />
          <Text style={styles.tipText}>Security and transaction alerts are recommended for safer wallet usage.</Text>
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

  card: { marginTop: 14, borderRadius: 18, padding: 14, backgroundColor: CARD, borderWidth: 1, borderColor: BORDER },
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

  previewRow: { marginTop: 12, flexDirection: "row", alignItems: "flex-start" },
  previewIcon: {
    width: 36,
    height: 36,
    borderRadius: 14,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
    marginRight: 10,
    marginTop: 2,
  },
  previewTop: { flexDirection: "row", alignItems: "center", justifyContent: "space-between" },
  previewTitle: { color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 13 },
  previewDesc: { marginTop: 4, color: MUTED, fontWeight: "700", fontSize: 11.5, lineHeight: 16 },
  previewTime: { marginTop: 6, color: "rgba(255,255,255,0.45)", fontWeight: "800", fontSize: 11 },

  pill: { paddingHorizontal: 10, paddingVertical: 6, borderRadius: 999, borderWidth: 1 },
  pillText: { color: "rgba(255,255,255,0.85)", fontWeight: "900", fontSize: 11, letterSpacing: 0.2 },

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
