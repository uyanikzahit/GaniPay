import React, { useMemo, useState } from "react";
import {
  View,
  Text,
  StyleSheet,
  Pressable,
  Platform,
  ScrollView,
  KeyboardAvoidingView,
} from "react-native";
import { LinearGradient } from "expo-linear-gradient";
import { useRouter } from "expo-router";
import { Colors } from "../../theme/colors";

const GOLD = "rgba(246,195,64,1)";

export default function AgreementsScreen() {
  const router = useRouter();

  const [agreeTerms, setAgreeTerms] = useState(false);
  const [agreePrivacy, setAgreePrivacy] = useState(false);
  const [agreeKvkk, setAgreeKvkk] = useState(false);
  const [loading, setLoading] = useState(false);

  const canContinue = useMemo(
    () => agreeTerms && agreePrivacy && agreeKvkk,
    [agreeTerms, agreePrivacy, agreeKvkk]
  );

  const onContinue = async () => {
    if (!canContinue) return;
    try {
      setLoading(true);
      // ✅ MOCK: burada istersen AsyncStorage’a flag yazarsın (şimdilik gerek yok)
      router.replace("/(auth)/onboarding/kyc");
    } finally {
      setLoading(false);
    }
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

        {/* Top */}
        <View style={styles.topBar}>
          <Pressable
            onPress={() => router.back()}
            style={({ pressed }) => [styles.backBtn, pressed && { opacity: 0.85 }]}
          >
            <Text style={styles.backText}>←</Text>
          </Pressable>

          <View style={{ flex: 1 }}>
            <Text style={styles.pageTitle}>Onboarding</Text>
            <Text style={styles.pageSub}>Step 1 of 2 • Agreements</Text>
          </View>
        </View>

        {/* Stepper */}
        <View style={styles.stepperWrap}>
          <View style={styles.stepperIconsRow}>
            <View style={[styles.stepIcon, styles.stepIconActive]}>
              <Text style={[styles.stepIconText, styles.stepIconTextActive]}>📄</Text>
            </View>
            <View style={[styles.stepConn, styles.stepConnDone]} />
            <View style={styles.stepIcon}>
              <Text style={styles.stepIconText}>🧾</Text>
            </View>
          </View>

          <View style={styles.stepperText}>
            <Text style={styles.stepperTitle}>Agreements</Text>
            <Text style={styles.stepperDesc}>
              Review and accept the required terms to continue.
            </Text>
          </View>
        </View>

        {/* Content */}
        <KeyboardAvoidingView
          style={{ flex: 1 }}
          behavior={Platform.OS === "ios" ? "padding" : undefined}
        >
          <ScrollView
            contentContainerStyle={styles.scroll}
            showsVerticalScrollIndicator={false}
          >
            <View style={styles.card}>
              <Text style={styles.cardTitle}>Agreements</Text>
              <Text style={styles.cardSub}>
                We keep it simple: accept the essentials to activate your onboarding.
              </Text>

              <AgreementItem
                title="Terms of Service"
                desc="Usage rules, account responsibilities, and service scope."
                checked={agreeTerms}
                onToggle={() => setAgreeTerms((p) => !p)}
              />

              <AgreementItem
                title="Privacy Policy"
                desc="How we collect, use, and protect your data."
                checked={agreePrivacy}
                onToggle={() => setAgreePrivacy((p) => !p)}
              />

              <AgreementItem
                title="KVKK / Data Consent"
                desc="Local compliance consent for identity and contact verification."
                checked={agreeKvkk}
                onToggle={() => setAgreeKvkk((p) => !p)}
              />

              <View style={{ height: 8 }} />

              <Text style={styles.note}>
                By continuing, you agree to proceed to identity verification (KYC).
              </Text>
            </View>

            <View style={{ height: 110 }} />
          </ScrollView>

          {/* Sticky actions */}
          <View style={styles.actionsWrap}>
            <View style={styles.actions}>
              <Pressable
                onPress={() => router.back()}
                style={({ pressed }) => [
                  styles.btnGhost,
                  pressed && { opacity: 0.92 },
                ]}
              >
                <Text style={styles.btnGhostText}>Back</Text>
              </Pressable>

              <Pressable
                disabled={!canContinue || loading}
                onPress={onContinue}
                style={({ pressed }) => [
                  styles.btnPrimary,
                  (!canContinue || loading) && { opacity: 0.55 },
                  pressed && { opacity: 0.92 },
                ]}
              >
                <Text style={styles.btnPrimaryText}>
                  {loading ? "Continuing..." : "Continue"}
                </Text>
              </Pressable>
            </View>
          </View>
        </KeyboardAvoidingView>
      </LinearGradient>
    </View>
  );
}

function AgreementItem({
  title,
  desc,
  checked,
  onToggle,
}: {
  title: string;
  desc: string;
  checked: boolean;
  onToggle: () => void;
}) {
  return (
    <Pressable onPress={onToggle} style={({ pressed }) => [styles.item, pressed && { opacity: 0.92 }]}>
      <View style={[styles.checkbox, checked && styles.checkboxOn]}>
        <Text style={[styles.checkText, checked && styles.checkTextOn]}>
          {checked ? "✓" : ""}
        </Text>
      </View>

      <View style={{ flex: 1 }}>
        <Text style={styles.itemTitle}>{title}</Text>
        <Text style={styles.itemDesc}>{desc}</Text>
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
  stepIconActive: {
    backgroundColor: "rgba(246,195,64,0.22)",
    borderColor: "rgba(246,195,64,0.45)",
  },
  stepIconText: { fontSize: 18 },
  stepIconTextActive: { transform: [{ scale: 1.05 }] },

  stepConn: { height: 2, width: 26, marginHorizontal: 10, borderRadius: 99 },
  stepConnDone: { backgroundColor: "rgba(246,195,64,0.55)" },

  stepperText: { alignItems: "center", marginTop: 10 },
  stepperTitle: { color: "rgba(255,255,255,0.92)", fontSize: 12.5, fontWeight: "900", letterSpacing: 0.2 },
  stepperDesc: { color: "rgba(255,255,255,0.65)", fontSize: 12, marginTop: 4, textAlign: "center" },

  scroll: { paddingTop: 10, paddingBottom: 10 },

  card: {
    borderRadius: 26,
    paddingVertical: 18,
    paddingHorizontal: 16,
    backgroundColor: "rgba(255,255,255,0.08)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.14)",
    ...shadow,
  },
  cardTitle: { color: "rgba(255,255,255,0.96)", fontSize: 18, fontWeight: "900" },
  cardSub: { color: "rgba(255,255,255,0.72)", fontSize: 12.5, marginTop: 6, marginBottom: 12, lineHeight: 18 },

  item: {
    flexDirection: "row",
    gap: 12,
    paddingVertical: 12,
    paddingHorizontal: 12,
    borderRadius: 16,
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.10)",
    marginTop: 10,
  },
  checkbox: {
    width: 22,
    height: 22,
    borderRadius: 7,
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.22)",
    backgroundColor: "rgba(0,0,0,0.18)",
    alignItems: "center",
    justifyContent: "center",
  },
  checkboxOn: {
    borderColor: "rgba(246,195,64,0.55)",
    backgroundColor: "rgba(246,195,64,0.18)",
  },
  checkText: { color: "rgba(255,255,255,0.0)", fontWeight: "900", fontSize: 14 },
  checkTextOn: { color: "rgba(255,255,255,0.92)" },

  itemTitle: { color: "rgba(255,255,255,0.92)", fontSize: 13.5, fontWeight: "900" },
  itemDesc: { color: "rgba(255,255,255,0.66)", fontSize: 12, marginTop: 4, lineHeight: 17 },

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

  btnPrimary: {
    flex: 1,
    height: 54,
    borderRadius: 999,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: GOLD,
  },
  btnPrimaryText: { color: "#111827", fontWeight: "900", fontSize: 16 },
});
