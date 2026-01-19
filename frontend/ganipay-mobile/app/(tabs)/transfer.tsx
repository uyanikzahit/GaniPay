import React, { useEffect, useMemo, useState } from "react";
import { View, Text, StyleSheet, ScrollView, Pressable, TextInput, Alert } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";
import AsyncStorage from "@react-native-async-storage/async-storage";

import { SessionKeys } from "../../constants/storage";
import { TransferPayload } from "./transfer.api";

const BG = "#0B1220";
const CARD = "rgba(255,255,255,0.06)";
const BORDER = "rgba(255,255,255,0.10)";
const GOLD = "rgba(246,195,64,1)";
const MUTED = "rgba(255,255,255,0.60)";
const SOFT = "rgba(255,255,255,0.35)";

const PendingTransferKey = "ganipay.pendingTransfer.v1";

function parseAmount(text: string) {
  const normalized = text.replace(",", ".").replace(/[^0-9.]/g, "");
  const parts = normalized.split(".");
  const cleaned = parts.length <= 2 ? normalized : `${parts[0]}.${parts.slice(1).join("")}`;
  const num = Number(cleaned);
  return { cleanedText: cleaned, value: Number.isFinite(num) ? num : 0 };
}

function safeIdempotencyKey() {
  return `idem_${Date.now()}_${Math.random().toString(16).slice(2)}`;
}
function safeReferenceId() {
  return `ref_${Date.now()}_${Math.random().toString(16).slice(2)}`;
}

function cleanAccountNumber(text: string) {
  return text.replace(/\s+/g, "");
}
function maskAccountNumber(v: string) {
  const s = cleanAccountNumber(v);
  if (s.length <= 10) return s;
  return `${s.slice(0, 6)}…${s.slice(-4)}`;
}

export default function TransferScreen() {
  const router = useRouter();

  const [sessionCustomerId, setSessionCustomerId] = useState<string | null>(null);
  const [sessionAccountId, setSessionAccountId] = useState<string | null>(null);
  const [sessionReady, setSessionReady] = useState(false);

  // UI (receiverCustomerId = accountNumber)
  const [accountNumber, setAccountNumber] = useState("");
  const [amountText, setAmountText] = useState("");
  const [note, setNote] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    (async () => {
      try {
        const cid = await AsyncStorage.getItem(SessionKeys.customerId);
        const aid = await AsyncStorage.getItem(SessionKeys.accountId);
        setSessionCustomerId(cid);
        setSessionAccountId(aid);
      } finally {
        setSessionReady(true);
      }
    })();
  }, []);

  const amountParsed = useMemo(() => parseAmount(amountText), [amountText]);
  const amount = amountParsed.value;

  const receiverOk = useMemo(
    () => cleanAccountNumber(accountNumber).length >= 12, // GUID gibi değerler için
    [accountNumber]
  );

  const canSubmit = useMemo(() => {
    if (!sessionReady) return false;
    if (loading) return false;
    if (!sessionCustomerId || !sessionAccountId) return false;
    if (!receiverOk) return false;
    if (!amountText.trim() || amount <= 0) return false;
    if (amount > 1_000_000) return false;
    return true;
  }, [sessionReady, loading, sessionCustomerId, sessionAccountId, receiverOk, amountText, amount]);

  const onSubmit = async () => {
    if (!sessionCustomerId || !sessionAccountId) {
      Alert.alert("Session missing", "Please login again.");
      router.replace("/(auth)/login");
      return;
    }

    const payload: TransferPayload = {
      customerId: sessionCustomerId,
      accountId: sessionAccountId,
      receiverCustomerId: cleanAccountNumber(accountNumber),
      amount: Number(amount.toFixed(2)),
      currency: "TRY",
      idempotencyKey: safeIdempotencyKey(),
      referenceId: safeReferenceId(),
      ...(note.trim() ? { note: note.trim() } : {}),
    };

    try {
      setLoading(true);

      // ✅ Transfer API çağrısı burada YOK.
      // ✅ Sadece 3DS ekranına gidebilmek için payload’ı saklıyoruz.
      await AsyncStorage.setItem(PendingTransferKey, JSON.stringify(payload));

      // ✅ Form sıfırla (3DS’e geçince topup gibi temiz kalsın)
      setAccountNumber("");
      setAmountText("");
      setNote("");

      // ✅ 3DS sayfasına yönlendir
      router.push("/(tabs)/transfer-3ds");
    } catch {
      Alert.alert("Error", "Unable to continue. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.container} showsVerticalScrollIndicator={false}>
      <View style={styles.headerRow}>
        <Pressable onPress={() => router.back()} style={({ pressed }) => [styles.backBtn, pressed && { opacity: 0.9 }]}>
          <Ionicons name="chevron-back" size={18} color="rgba(255,255,255,0.85)" />
        </Pressable>

        <View style={{ flex: 1, minWidth: 0 }}>
          <Text style={styles.title}>Transfer</Text>
          <Text style={styles.sub}>Send money instantly</Text>
        </View>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Receiver</Text>
        <Text style={styles.cardHint}>Paste recipient account number (demo: receiverCustomerId).</Text>

        <View style={styles.inputRow}>
          <Ionicons name="key-outline" size={18} color={SOFT} />
          <TextInput
            value={accountNumber}
            onChangeText={(t) => setAccountNumber(cleanAccountNumber(t))}
            placeholder="Paste account number"
            placeholderTextColor="rgba(255,255,255,0.25)"
            autoCapitalize="none"
            autoCorrect={false}
            style={styles.input}
          />
        </View>

        <Text style={[styles.helper, { marginTop: 8 }]}>
          {receiverOk ? `Preview: ${maskAccountNumber(accountNumber)}` : "Enter a valid account number"}
        </Text>

        <View style={styles.divider} />

        <Text style={styles.cardTitle}>Amount</Text>
        <Text style={styles.cardHint}>Select how much to send.</Text>

        <View style={styles.inputRow}>
          <Ionicons name="cash-outline" size={18} color={SOFT} />
          <TextInput
            value={amountText}
            onChangeText={(t) => setAmountText(parseAmount(t).cleanedText)}
            placeholder="0.00"
            placeholderTextColor="rgba(255,255,255,0.25)"
            keyboardType="decimal-pad"
            style={styles.input}
          />
          <View style={styles.currencyPill}>
            <Text style={styles.currencyText}>TRY</Text>
          </View>
        </View>

        <Text style={[styles.helper, { marginTop: 8 }]}>
          {amount > 0 ? `You will send ${amount.toFixed(2)} TRY` : "Enter a valid amount"}
        </Text>

        <View style={styles.divider} />

        <Text style={styles.cardTitle}>Note</Text>
        <Text style={styles.cardHint}>Optional description for your transfer.</Text>

        <View style={styles.inputRow}>
          <Ionicons name="create-outline" size={18} color={SOFT} />
          <TextInput
            value={note}
            onChangeText={setNote}
            placeholder="e.g. dinner"
            placeholderTextColor="rgba(255,255,255,0.25)"
            style={styles.input}
            maxLength={60}
          />
        </View>

        <Pressable
          onPress={onSubmit}
          disabled={!canSubmit}
          style={({ pressed }) => [
            styles.primaryBtn,
            !canSubmit && { opacity: 0.45 },
            pressed && canSubmit && { opacity: 0.9 },
          ]}
        >
          {loading ? (
            <>
              <Ionicons name="time-outline" size={18} color="rgba(255,255,255,0.92)" />
              <Text style={styles.primaryBtnText}>Continue...</Text>
            </>
          ) : (
            <>
              <Ionicons name="shield-checkmark-outline" size={18} color="rgba(255,255,255,0.92)" />
              <Text style={styles.primaryBtnText}>Continue to 3D Secure</Text>
            </>
          )}
        </Pressable>
      </View>

      <View style={styles.card}>
        <View style={styles.tipRow}>
          <Ionicons name="shield-checkmark-outline" size={18} color={GOLD} />
          <Text style={styles.tipText}>
            Transfers can be subject to balance, limit and security checks. You’ll see a receipt after completion.
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

  cardTitle: { color: "rgba(255,255,255,0.92)", fontSize: 14, fontWeight: "900" },
  cardHint: { marginTop: 6, color: "rgba(255,255,255,0.55)", fontSize: 11.5, fontWeight: "700", lineHeight: 16 },

  inputRow: {
    marginTop: 10,
    borderRadius: 14,
    paddingHorizontal: 12,
    paddingVertical: 10,
    backgroundColor: "rgba(0,0,0,0.18)",
    borderWidth: 1,
    borderColor: BORDER,
    flexDirection: "row",
    alignItems: "center",
  },
  input: {
    flex: 1,
    color: "rgba(255,255,255,0.92)",
    fontWeight: "900",
    fontSize: 14,
    marginLeft: 10,
    paddingVertical: 0,
    minWidth: 0,
  },

  currencyPill: {
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderRadius: 999,
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
    marginLeft: 10,
  },
  currencyText: { color: "rgba(255,255,255,0.85)", fontWeight: "900", fontSize: 11.5 },

  helper: { color: MUTED, fontWeight: "700", fontSize: 11.5 },

  divider: { height: 1, backgroundColor: BORDER, marginTop: 16, marginBottom: 14 },

  primaryBtn: {
    marginTop: 16,
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
  primaryBtnText: { marginLeft: 8, color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 13.5 },

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
