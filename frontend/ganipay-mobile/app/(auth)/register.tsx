import React, { useMemo, useState } from "react";
import {
  View,
  Text,
  StyleSheet,
  Pressable,
  Platform,
  ScrollView,
  KeyboardAvoidingView
} from "react-native";

import { LinearGradient } from "expo-linear-gradient";
import { useRouter } from "expo-router";
import { Alert } from "react-native";
import { Colors } from "../theme/colors";

import { defaults, REGISTER_STEPS, type StepKey, buildRegisterPayload, validateStep } from "./register.types";
import { StepAccount, StepAddress, StepContact, StepReview } from "./register.steps";
import { register as registerApi } from "./register.api";

const GOLD = "rgba(246,195,64,1)";

export default function RegisterScreen() {
  const router = useRouter();
  const [form, setForm] = useState(defaults);
  const [stepIndex, setStepIndex] = useState(0);
  const [loading, setLoading] = useState(false);



  const stepKey: StepKey = REGISTER_STEPS[stepIndex].key;

  const canGoBack = stepIndex > 0;
  const isLast = stepIndex === REGISTER_STEPS.length - 1;

  const onNext = () => {
    const err = validateStep(stepKey, form);
    if (err) return Alert.alert("Check your details", err);
    setStepIndex((p) => Math.min(p + 1, REGISTER_STEPS.length - 1));
  };

  const onBack = () => setStepIndex((p) => Math.max(p - 1, 0));

  const onSubmit = async () => {
    const err = validateStep("account", form) || validateStep("contact", form) || validateStep("address", form);
    if (err) return Alert.alert("Check your details", err);

    try {
      setLoading(true);
      const payload = buildRegisterPayload(form);

      // ✅ API call (CORS/https vs sonra)
      const res = await registerApi(payload);
      console.log("REGISTER RESPONSE:", res);

      Alert.alert("Success", "Account created. Continue to onboarding.");
      // ✅ Approach 1: Register biter → onboarding başlar
      router.replace("/(auth)/onboarding");
    } catch (e: any) {
      Alert.alert("Register failed", e?.message ?? "Unknown error");
    } finally {
      setLoading(false);
    }
  };

  const StepView = useMemo(() => {
    if (stepKey === "account") return <StepAccount form={form} setForm={setForm} />;
    if (stepKey === "contact") return <StepContact form={form} setForm={setForm} />;
    if (stepKey === "address") return <StepAddress form={form} setForm={setForm} />;
    return <StepReview form={form} />;
  }, [stepKey, form]);

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
            onPress={() => router.back()}
            style={({ pressed }) => [styles.backBtn, pressed && { opacity: 0.85 }]}
          >
            <Text style={styles.backText}>←</Text>
          </Pressable>

          <View style={{ flex: 1 }}>
            <Text style={styles.pageTitle}>Sign up</Text>
            <Text style={styles.pageSub}>
              Step {stepIndex + 1} of {REGISTER_STEPS.length} • {REGISTER_STEPS[stepIndex].label}
            </Text>
          </View>
        </View>

        {/* Stepper (icons + label + desc) */}
          <View style={styles.stepperWrap}>
            <View style={styles.stepperIconsRow}>
              {REGISTER_STEPS.map((s, i) => {
                const state = i < stepIndex ? "done" : i === stepIndex ? "active" : "idle";

                return (
                  <React.Fragment key={s.key}>
                    <View
                      style={[
                        styles.stepIcon,
                        state === "done" && styles.stepIconDone,
                        state === "active" && styles.stepIconActive,
                      ]}
                    >
                      <Text
                        style={[
                          styles.stepIconText,
                          state === "active" && styles.stepIconTextActive,
                          state === "done" && styles.stepIconTextDone,
                        ]}
                      >
                        {s.icon}
                      </Text>
                    </View>

                    {i < REGISTER_STEPS.length - 1 && (
                      <View
                        style={[
                          styles.stepConn,
                          i < stepIndex ? styles.stepConnDone : styles.stepConnIdle,
                        ]}
                      />
                    )}
                  </React.Fragment>
                );
              })}
            </View>

            <View style={styles.stepperText}>
              <Text style={styles.stepperTitle}>
                {REGISTER_STEPS[stepIndex].label}
              </Text>
              <Text style={styles.stepperDesc}>
                {REGISTER_STEPS[stepIndex].desc}
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
            keyboardShouldPersistTaps="handled"
            showsVerticalScrollIndicator={false}
          >
            {StepView}
            <View style={{ height: 110 }} />
          </ScrollView>

          {/* Sticky actions */}
          <View style={styles.actionsWrap}>
            <View style={styles.actions}>
              <Pressable
                disabled={!canGoBack || loading}
                onPress={onBack}
                style={({ pressed }) => [
                  styles.btnGhost,
                  (!canGoBack || loading) && { opacity: 0.4 },
                  pressed && { opacity: 0.92 },
                ]}
              >
                <Text style={styles.btnGhostText}>Back</Text>
              </Pressable>

              <Pressable
                disabled={loading}
                onPress={isLast ? onSubmit : onNext}
                style={({ pressed }) => [
                  styles.btnPrimary,
                  loading && { opacity: 0.75 },
                  pressed && { opacity: 0.92 },
                ]}
              >
                <Text style={styles.btnPrimaryText}>
                  {isLast ? (loading ? "Creating..." : "Create account") : "Next"}
                </Text>
              </Pressable>
            </View>
          </View>
        </KeyboardAvoidingView>
      </LinearGradient>
    </View>
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
  // ✅ overflow hidden -> sağa/aşağı kayma yok
  container: { flex: 1, backgroundColor: Colors.bg, overflow: "hidden" },

  hero: {
    flex: 1,
    paddingTop: 22,
    paddingHorizontal: 18,
    overflow: "hidden",
  },

  // ✅ glow taşmaz
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
  backText: {
    color: "rgba(255,255,255,0.92)",
    fontSize: 18,
    fontWeight: "900",
  },
  pageTitle: {
    color: "rgba(255,255,255,0.96)",
    fontSize: 22,
    fontWeight: "900",
  },
  pageSub: {
    color: "rgba(255,255,255,0.70)",
    fontSize: 13,
    marginTop: 4,
  },

  stepperWrap: {
  marginTop: 6,
  marginBottom: 8,
  paddingHorizontal: 2,
},

stepperIconsRow: {
  flexDirection: "row",
  alignItems: "center",
  justifyContent: "center",
},

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

stepIconDone: {
  backgroundColor: "rgba(246,195,64,0.16)",
  borderColor: "rgba(246,195,64,0.30)",
  opacity: 0.95,
},

stepIconText: {
  fontSize: 18,
},

stepIconTextActive: {
  // aktifken daha “premium” gözüksün
  transform: [{ scale: 1.05 }],
},

stepIconTextDone: {
  opacity: 0.95,
},

stepConn: {
  height: 2,
  width: 26,
  marginHorizontal: 10,
  borderRadius: 99,
},

stepConnDone: { backgroundColor: "rgba(246,195,64,0.55)" },
stepConnIdle: { backgroundColor: "rgba(255,255,255,0.14)" },

stepperText: {
  alignItems: "center",
  marginTop: 10,
},

stepperTitle: {
  color: "rgba(255,255,255,0.92)",
  fontSize: 12.5,
  fontWeight: "900",
  letterSpacing: 0.2,
},

stepperDesc: {
  color: "rgba(255,255,255,0.65)",
  fontSize: 12,
  marginTop: 4,
  textAlign: "center",
},

  stepItem: { flexDirection: "row", alignItems: "center" },

  stepDotActive: { backgroundColor: GOLD, width: 22 },
  stepDotDone: { backgroundColor: "rgba(246,195,64,0.75)" },

  stepLineDone: { backgroundColor: "rgba(246,195,64,0.55)" },
  stepLineIdle: { backgroundColor: "rgba(255,255,255,0.14)" },

  scroll: { paddingTop: 10, paddingBottom: 10 },

  actionsWrap: {
    position: "absolute",
    left: 18,
    right: 18,
    bottom: 14,
  },
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
  btnGhostText: {
    color: "rgba(255,255,255,0.92)",
    fontWeight: "900",
    fontSize: 16,
  },

  btnPrimary: {
    flex: 1,
    height: 54,
    borderRadius: 999,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: GOLD,
  },
  btnPrimaryText: {
    color: "#111827",
    fontWeight: "900",
    fontSize: 16,
  },
});
