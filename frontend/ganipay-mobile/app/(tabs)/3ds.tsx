import React, { useEffect, useMemo, useState } from "react";
import { View, Text, StyleSheet, Pressable, ActivityIndicator, TextInput, Alert } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { startTopUp, TopUpPayload } from "./topup.api";

const BG = "#0B1220";
const CARD = "rgba(255,255,255,0.06)";
const BORDER = "rgba(255,255,255,0.10)";
const GOLD = "rgba(246,195,64,1)";
const MUTED = "rgba(255,255,255,0.60)";
const SOFT = "rgba(255,255,255,0.35)";
const DANGER = "rgba(255,71,87,0.85)";

const PendingTopUpKey = "ganipay.pendingTopUp.v1";

function sleep(ms: number) {
  return new Promise((r) => setTimeout(r, ms));
}

function maskIban(iban: string) {
  const c = (iban || "").replace(/\s+/g, "");
  if (c.length < 10) return c;
  return `${c.slice(0, 6)} **** **** ${c.slice(-4)}`;
}

function onlyDigits(s: string) {
  return s.replace(/\D/g, "");
}

export default function ThreeDSScreen() {
  const router = useRouter();

  const [payload, setPayload] = useState<TopUpPayload | null>(null);
  const [loading, setLoading] = useState(true);

  // UI states
  const [phase, setPhase] = useState<"challenge" | "verifying">("challenge");
  const [otp, setOtp] = useState("");
  const [attempts, setAttempts] = useState(0);
  const [secondsLeft, setSecondsLeft] = useState(60);
  const [sending, setSending] = useState(false);
  const [verifying, setVerifying] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Load pending payload
  useEffect(() => {
    (async () => {
      try {
        const raw = await AsyncStorage.getItem(PendingTopUpKey);
        if (!raw) {
          setPayload(null);
          return;
        }
        setPayload(JSON.parse(raw));
      } catch {
        setPayload(null);
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  // Countdown timer for resend
  useEffect(() => {
    if (loading) return;
    if (!payload) return;
    if (phase !== "challenge") return;
    if (secondsLeft <= 0) return;

    const id = setInterval(() => setSecondsLeft((s) => Math.max(0, s - 1)), 1000);
    return () => clearInterval(id);
  }, [loading, payload, phase, secondsLeft]);

  const summary = useMemo(() => {
    if (!payload) return null;
    return {
      amount: payload.amount?.toFixed?.(2) ?? String(payload.amount),
      currency: payload.currency,
      ref: maskIban(payload.referenceId),
    };
  }, [payload]);

  const otpOk = useMemo(() => onlyDigits(otp).length === 6, [otp]);
  const canResend = useMemo(() => secondsLeft === 0 && !sending && phase === "challenge", [secondsLeft, sending, phase]);

  const resendCode = async () => {
    if (!canResend) return;

    setSending(true);
    setError(null);

    // Simülasyon: bankaya kod yeniden gönderiliyor gibi beklet
    await sleep(900);
    setSecondsLeft(60);
    setSending(false);

    Alert.alert("Code sent", "A new verification code has been sent.");
  };

  const cancel = async () => {
    // pending’i temizle
    await AsyncStorage.removeItem(PendingTopUpKey).catch(() => {});
    router.back();
  };

  const approve = async () => {
    if (!payload) return;

    setError(null);

    // Simülasyon: gerçekçi OTP doğrulama
    // Demo’da “123456” doğru olsun (istersen değiştirirsin)
    const code = onlyDigits(otp);

    if (!otpOk) {
      setError("Please enter the 6-digit code.");
      return;
    }

    setAttempts((a) => a + 1);

    // 3 denemeden sonra kilitle (simülasyon)
    if (attempts >= 2) {
      setError("Too many attempts. Please request a new code.");
      return;
    }

    setPhase("verifying");
    setVerifying(true);

    try {
      // Banka doğrulama beklemesi (UI gerçekçilik)
      await sleep(1200);

      if (code !== "123456") {
        setPhase("challenge");
        setVerifying(false);
        setError("Invalid code. Please try again.");
        return;
      }

      // ✅ OTP doğruysa: gerçek TopUp workflow çağrısı
      const result = await startTopUp(payload);

      await AsyncStorage.removeItem(PendingTopUpKey).catch(() => {});

      if (result.success && result.status === "Succeeded") {
        Alert.alert("Success", result.message || "Top up successful.", [
          { text: "Done", onPress: () => router.replace("/(tabs)/wallet") },
        ]);
        return;
      }

      Alert.alert("Failed", result.message || "Top up failed. Please try again.", [
        { text: "Back", onPress: () => router.replace("/(tabs)/topup") },
      ]);
    } catch (e: any) {
      Alert.alert("Failed", e?.message || "Top up failed. Please try again.", [
        { text: "Back", onPress: () => router.replace("/(tabs)/topup") },
      ]);
    } finally {
      setVerifying(false);
    }
  };

  if (loading) {
    return (
      <View style={[styles.page, styles.center]}>
        <ActivityIndicator />
        <Text style={styles.mutedText}>Preparing 3D Secure...</Text>
      </View>
    );
  }

  if (!payload || !summary) {
    return (
      <View style={[styles.page, styles.center, { padding: 18 }]}>
        <Text style={styles.h1}>No pending payment</Text>
        <Text style={[styles.mutedText, { textAlign: "center" }]}>
          Go back and start the top up again.
        </Text>

        <Pressable onPress={() => router.replace("/(tabs)/topup")} style={({ pressed }) => [styles.secondaryBtn, pressed && { opacity: 0.9 }]}>
          <Ionicons name="arrow-back-outline" size={18} color="rgba(255,255,255,0.92)" />
          <Text style={styles.secondaryBtnText}>Back to Top up</Text>
        </Pressable>
      </View>
    );
  }

  return (
    <View style={styles.page}>
      {/* Header */}
      <View style={styles.header}>
        <View style={styles.badge}>
          <Ionicons name="shield-checkmark-outline" size={18} color={GOLD} />
        </View>
        <Text style={styles.title}>3D Secure Verification</Text>
        <Text style={styles.sub}>
          Enter the one-time code sent by your bank to confirm this payment.
        </Text>
      </View>

      {/* Summary */}
      <View style={styles.card}>
        <View style={styles.summaryRow}>
          <Text style={styles.label}>Amount</Text>
          <Text style={styles.value}>{summary.amount} {summary.currency}</Text>
        </View>
        <View style={styles.summaryRow}>
          <Text style={styles.label}>Reference</Text>
          <Text style={styles.value}>{summary.ref}</Text>
        </View>
        <View style={[styles.summaryRow, { marginBottom: 0 }]}>
          <Text style={styles.label}>Status</Text>
          <Text style={styles.value}>{phase === "verifying" ? "Verifying..." : "Waiting for code"}</Text>
        </View>
      </View>

      {/* Challenge */}
      <View style={styles.card}>
        <Text style={styles.bankTitle}>GaniBank Secure</Text>
        <Text style={styles.bankSub}>
          We’ve sent a 6-digit code to your phone. (Demo code: 123456)
        </Text>

        <View style={[styles.otpRow, error ? { borderColor: "rgba(255,71,87,0.35)" } : null]}>
          <Ionicons name="chatbubble-ellipses-outline" size={18} color={SOFT} />
          <TextInput
            value={onlyDigits(otp)}
            onChangeText={(t) => {
              setError(null);
              setOtp(onlyDigits(t).slice(0, 6));
            }}
            placeholder="••••••"
            placeholderTextColor="rgba(255,255,255,0.25)"
            keyboardType="number-pad"
            style={styles.otpInput}
            maxLength={6}
            editable={!verifying}
          />
          <View style={[styles.timerPill, secondsLeft === 0 && { borderColor: "rgba(246,195,64,0.35)" }]}>
            <Text style={styles.timerText}>{secondsLeft > 0 ? `00:${String(secondsLeft).padStart(2, "0")}` : "Resend"}</Text>
          </View>
        </View>

        {error ? <Text style={styles.errorText}>{error}</Text> : null}

        <View style={styles.actionsRow}>
          <Pressable
            onPress={approve}
            disabled={verifying}
            style={({ pressed }) => [
              styles.primaryBtn,
              verifying && { opacity: 0.55 },
              pressed && !verifying && { opacity: 0.9 },
            ]}
          >
            {verifying ? (
              <>
                <ActivityIndicator />
                <Text style={styles.primaryText}>Verifying...</Text>
              </>
            ) : (
              <>
                <Ionicons name="checkmark-circle-outline" size={18} color="rgba(255,255,255,0.92)" />
                <Text style={styles.primaryText}>Confirm</Text>
              </>
            )}
          </Pressable>

          <Pressable
            onPress={cancel}
            disabled={verifying}
            style={({ pressed }) => [styles.cancelBtn, pressed && { opacity: 0.9 }, verifying && { opacity: 0.55 }]}
          >
            <Ionicons name="close-circle-outline" size={18} color="rgba(255,255,255,0.80)" />
            <Text style={styles.cancelText}>Cancel</Text>
          </Pressable>
        </View>

        <View style={styles.helperRow}>
          <Text style={styles.helperText}>Didn’t receive the code?</Text>
          <Pressable onPress={resendCode} disabled={!canResend}>
            <Text style={[styles.linkText, !canResend && { opacity: 0.55 }]}>
              {sending ? "Sending..." : "Resend code"}
            </Text>
          </Pressable>
        </View>
      </View>

      {/* Footer note */}
      <View style={styles.card}>
        <View style={styles.tipRow}>
          <Ionicons name="lock-closed-outline" size={18} color={GOLD} />
          <Text style={styles.tipText}>
            This is a simulation. In production, this screen is typically provided by the bank or payment gateway.
          </Text>
        </View>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  page: { flex: 1, backgroundColor: BG, padding: 16 },
  center: { justifyContent: "center", alignItems: "center" },

  h1: { color: "rgba(255,255,255,0.90)", fontWeight: "900", fontSize: 16 },
  mutedText: { color: MUTED, marginTop: 10, fontWeight: "800" },

  header: { alignItems: "center", marginTop: 8 },
  badge: {
    width: 44, height: 44, borderRadius: 16,
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1, borderColor: "rgba(246,195,64,0.22)",
    alignItems: "center", justifyContent: "center",
  },
  title: { marginTop: 12, color: "rgba(255,255,255,0.92)", fontSize: 18, fontWeight: "900" },
  sub: { marginTop: 6, color: MUTED, fontSize: 12.5, fontWeight: "700", textAlign: "center", lineHeight: 17 },

  card: {
    marginTop: 14,
    borderRadius: 18,
    padding: 14,
    backgroundColor: CARD,
    borderWidth: 1,
    borderColor: BORDER,
  },

  summaryRow: { flexDirection: "row", justifyContent: "space-between", alignItems: "center", marginBottom: 10 },
  label: { color: MUTED, fontWeight: "800", fontSize: 12 },
  value: { color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 12.5 },

  bankTitle: { color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 14 },
  bankSub: { marginTop: 6, color: MUTED, fontWeight: "700", fontSize: 12, lineHeight: 16 },

  otpRow: {
    marginTop: 12,
    borderRadius: 14,
    paddingHorizontal: 12,
    paddingVertical: 10,
    backgroundColor: "rgba(0,0,0,0.18)",
    borderWidth: 1,
    borderColor: BORDER,
    flexDirection: "row",
    alignItems: "center",
  },
  otpInput: {
    flex: 1,
    minWidth: 0,
    marginLeft: 10,
    color: "rgba(255,255,255,0.92)",
    fontWeight: "900",
    fontSize: 16,
    letterSpacing: 2,
    paddingVertical: 0,
  },
  timerPill: {
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderRadius: 999,
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
    marginLeft: 10,
  },
  timerText: { color: "rgba(255,255,255,0.85)", fontWeight: "900", fontSize: 11.5 },

  errorText: { marginTop: 8, color: DANGER, fontWeight: "800", fontSize: 11.5 },

  actionsRow: { marginTop: 12 },
  primaryBtn: {
    borderRadius: 16,
    paddingVertical: 12,
    paddingHorizontal: 14,
    backgroundColor: "rgba(246,195,64,0.22)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.35)",
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
  },
  primaryText: { marginLeft: 8, color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 13.5 },

  cancelBtn: {
    marginTop: 10,
    borderRadius: 16,
    paddingVertical: 12,
    paddingHorizontal: 14,
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: BORDER,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
  },
  cancelText: { marginLeft: 8, color: "rgba(255,255,255,0.82)", fontWeight: "900", fontSize: 13.5 },

  helperRow: { marginTop: 12, flexDirection: "row", justifyContent: "space-between", alignItems: "center" },
  helperText: { color: MUTED, fontWeight: "800", fontSize: 12 },
  linkText: { color: "rgba(246,195,64,0.95)", fontWeight: "900", fontSize: 12 },

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

  secondaryBtn: {
    marginTop: 14,
    borderRadius: 16,
    paddingVertical: 12,
    paddingHorizontal: 14,
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: BORDER,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
  },
  secondaryBtnText: { marginLeft: 8, color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 13.5 },
});
