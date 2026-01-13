import React, { useMemo, useState } from "react";
import { View, Text, StyleSheet, Pressable, Platform, ScrollView } from "react-native";
import { LinearGradient } from "expo-linear-gradient";
import { useRouter } from "expo-router";
import { Colors } from "../../theme/colors"; // eğer path farklıysa düzelt

const GOLD = "rgba(246,195,64,1)";

type KycItemKey = "doc" | "selfie" | "address" | "consent";

export default function KycScreen() {
  const router = useRouter();

  const [checks, setChecks] = useState<Record<KycItemKey, boolean>>({
    doc: false,
    selfie: false,
    address: false,
    consent: false,
  });

  const allDone = useMemo(
    () => Object.values(checks).every(Boolean),
    [checks]
  );

  const toggle = (k: KycItemKey) => setChecks((p) => ({ ...p, [k]: !p[k] }));

  const onBack = () => router.back(); // agreements’a geri döner
  const onComplete = () => {
    // ✅ MVP: KYC tamam → login
    router.replace("/(auth)/login");
  };

  return (
    <View style={styles.container}>
      <LinearGradient
        colors={[Colors.bg, "#0B1220", "#12223E"]}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        style={styles.hero}
      >
        <LinearGradient
          colors={["rgba(246,195,64,0.85)", "rgba(246,195,64,0)"]}
          start={{ x: 0.2, y: 0 }}
          end={{ x: 0.8, y: 1 }}
          style={styles.goldGlow}
        />

        {/* Top bar */}
        <View style={styles.topBar}>
          <Pressable
            onPress={onBack}
            style={({ pressed }) => [styles.backBtn, pressed && { opacity: 0.85 }]}
          >
            <Text style={styles.backText}>←</Text>
          </Pressable>

          <View style={{ flex: 1 }}>
            <Text style={styles.pageTitle}>Onboarding</Text>
            <Text style={styles.pageSub}>Step 2 of 2 • KYC</Text>
          </View>
        </View>

        {/* Mini stepper */}
        <View style={styles.stepperWrap}>
          <View style={styles.stepperIconsRow}>
            <View style={[styles.stepIcon, styles.stepIconDone]}>
              <Text style={styles.stepIconText}>📄</Text>
            </View>
            <View style={[styles.stepConn, styles.stepConnDone]} />
            <View style={[styles.stepIcon, styles.stepIconActive]}>
              <Text style={styles.stepIconText}>🛡️</Text>
            </View>
          </View>

          <View style={styles.stepperText}>
            <Text style={styles.stepperTitle}>KYC Verification</Text>
            <Text style={styles.stepperDesc}>
              Complete the checks to activate wallet access. (Mock)
            </Text>
          </View>
        </View>

        <ScrollView
          contentContainerStyle={styles.scroll}
          showsVerticalScrollIndicator={false}
        >
          <View style={styles.card}>
            <Text style={styles.cardTitle}>Identity check</Text>
            <Text style={styles.cardDesc}>
              This is an MVP mock flow. In production, these steps would be verified by a KYC provider.
            </Text>

            <KycRow
              title="Upload identity document"
              desc="Front/back photo of your ID (mock)."
              checked={checks.doc}
              onPress={() => toggle("doc")}
            />
            <KycRow
              title="Selfie verification"
              desc="Quick selfie match (mock)."
              checked={checks.selfie}
              onPress={() => toggle("selfie")}
            />
            <KycRow
              title="Confirm address details"
              desc="We’ll use your registration address (mock)."
              checked={checks.address}
              onPress={() => toggle("address")}
            />
            <KycRow
              title="Compliance consent"
              desc="Consent for risk & screening checks (mock)."
              checked={checks.consent}
              onPress={() => toggle("consent")}
            />

            <Text style={styles.note}>
              After completion you’ll be redirected to login.
            </Text>
          </View>

          <View style={{ height: 120 }} />
        </ScrollView>

        {/* Sticky actions */}
        <View style={styles.actionsWrap}>
          <View style={styles.actions}>
            <Pressable
              onPress={onBack}
              style={({ pressed }) => [styles.btnGhost, pressed && { opacity: 0.92 }]}
            >
              <Text style={styles.btnGhostText}>Back</Text>
            </Pressable>

            <Pressable
              disabled={!allDone}
              onPress={onComplete}
              style={({ pressed }) => [
                styles.btnPrimary,
                !allDone && { opacity: 0.45 },
                pressed && { opacity: 0.92 },
              ]}
            >
              <Text style={styles.btnPrimaryText}>Complete KYC</Text>
            </Pressable>
          </View>
        </View>
      </LinearGradient>
    </View>
  );
}

function KycRow({
  title,
  desc,
  checked,
  onPress,
}: {
  title: string;
  desc: string;
  checked: boolean;
  onPress: () => void;
}) {
  return (
    <Pressable onPress={onPress} style={({ pressed }) => [styles.row, pressed && { opacity: 0.94 }]}>
      <View style={[styles.tick, checked && styles.tickOn]}>
        <Text style={styles.tickText}>{checked ? "✓" : ""}</Text>
      </View>

      <View style={{ flex: 1 }}>
        <Text style={styles.rowTitle}>{title}</Text>
        <Text style={styles.rowDesc}>{desc}</Text>
      </View>
    </Pressable>
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
  container: { flex: 1, backgroundColor: Colors.bg, overflow: "hidden" },
  hero: { flex: 1, paddingTop: 22, paddingHorizontal: 18, overflow: "hidden" },

  goldGlow: {
    position: "absolute",
    top: -30,
    left: 0,
    right: 0,
    height: 250,
    borderBottomLeftRadius: 220,
    borderBottomRightRadius: 220,
    opacity: 0.9,
    transform: [{ scaleX: 1.15 }],
  },

  topBar: {
    flexDirection: "row",
    alignItems: "flex-start",
    gap: 12,
    marginTop: 12,
    marginBottom: 10,
  },
  backBtn: {
    width: 40,
    height: 40,
    borderRadius: 14,
    backgroundColor: "rgba(255,255,255,0.10)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.16)",
    alignItems: "center",
    justifyContent: "center",
  },
  backText: { color: "rgba(255,255,255,0.92)", fontSize: 18, fontWeight: "900" },
  pageTitle: { color: "rgba(255,255,255,0.96)", fontSize: 22, fontWeight: "900" },
  pageSub: { color: "rgba(255,255,255,0.70)", fontSize: 13, marginTop: 4 },

  stepperWrap: { marginTop: 6, marginBottom: 8, paddingHorizontal: 2 },
  stepperIconsRow: { flexDirection: "row", alignItems: "center", justifyContent: "center" },

  stepIcon: {
    width: 40,
    height: 40,
    borderRadius: 14,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "rgba(255,255,255,0.08)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.14)",
  },
  stepIconActive: { backgroundColor: "rgba(246,195,64,0.22)", borderColor: "rgba(246,195,64,0.45)" },
  stepIconDone: { backgroundColor: "rgba(246,195,64,0.16)", borderColor: "rgba(246,195,64,0.30)" },
  stepIconText: { fontSize: 18 },

  stepConn: { height: 2, width: 26, marginHorizontal: 10, borderRadius: 99 },
  stepConnDone: { backgroundColor: "rgba(246,195,64,0.55)" },

  stepperText: { alignItems: "center", marginTop: 10 },
  stepperTitle: { color: "rgba(255,255,255,0.92)", fontSize: 12.5, fontWeight: "900" },
  stepperDesc: { color: "rgba(255,255,255,0.65)", fontSize: 12, marginTop: 4, textAlign: "center" },

  scroll: { paddingTop: 10, paddingBottom: 10 },

  card: {
    marginTop: 10,
    borderRadius: 22,
    padding: 16,
    backgroundColor: "rgba(255,255,255,0.08)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.14)",
    ...shadow,
  },
  cardTitle: { color: "rgba(255,255,255,0.95)", fontSize: 18, fontWeight: "900" },
  cardDesc: { color: "rgba(255,255,255,0.70)", marginTop: 6, lineHeight: 18, fontSize: 12.5 },

  row: {
    marginTop: 12,
    borderRadius: 16,
    padding: 14,
    flexDirection: "row",
    gap: 12,
    alignItems: "center",
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.10)",
  },
  tick: {
    width: 22,
    height: 22,
    borderRadius: 7,
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.35)",
    backgroundColor: "rgba(246,195,64,0.10)",
    alignItems: "center",
    justifyContent: "center",
  },
  tickOn: { backgroundColor: "rgba(246,195,64,0.28)", borderColor: "rgba(246,195,64,0.55)" },
  tickText: { color: "rgba(255,255,255,0.95)", fontWeight: "900" },

  rowTitle: { color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 13.5 },
  rowDesc: { color: "rgba(255,255,255,0.65)", marginTop: 3, fontSize: 12 },

  note: { color: "rgba(255,255,255,0.55)", fontSize: 11.5, marginTop: 12 },

  actionsWrap: { position: "absolute", left: 18, right: 18, bottom: 14 },
  actions: {
    flexDirection: "row",
    gap: 12,
    padding: 12,
    borderRadius: 20,
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.10)",
    ...shadow,
  },

  btnGhost: {
    flex: 1,
    height: 54,
    borderRadius: 999,
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.35)",
    backgroundColor: "rgba(246,195,64,0.10)",
    alignItems: "center",
    justifyContent: "center",
  },
  btnGhostText: { color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 16 },

  btnPrimary: { flex: 1, height: 54, borderRadius: 999, alignItems: "center", justifyContent: "center", backgroundColor: GOLD },
  btnPrimaryText: { color: "#111827", fontWeight: "900", fontSize: 16 },
});
