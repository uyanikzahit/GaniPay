import React, { useEffect, useMemo, useState } from "react";
import { View, Text, StyleSheet, ScrollView, Pressable, TextInput, Alert, KeyboardAvoidingView, Platform } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { SessionKeys } from "../../constants/storage";
import type { TopUpPayload } from "./topup.api";

const BG = "#0B1220";
const CARD = "rgba(255,255,255,0.06)";
const BORDER = "rgba(255,255,255,0.10)";
const GOLD = "rgba(246,195,64,1)";
const MUTED = "rgba(255,255,255,0.60)";
const SOFT = "rgba(255,255,255,0.35)";
const DANGER = "rgba(255,71,87,0.85)";

const PendingTopUpKey = "ganipay.pendingTopUp.v1";

function safeIdempotencyKey() {
  const g: any = globalThis as any;
  if (g?.crypto?.randomUUID) return `idem_${g.crypto.randomUUID()}`;
  return `idem_${Date.now()}_${Math.random().toString(16).slice(2)}`;
}

function onlyDigits(s: string) {
  return s.replace(/\D/g, "");
}

function parseAmount(text: string) {
  const normalized = text.replace(",", ".").replace(/[^0-9.]/g, "");
  const parts = normalized.split(".");
  const cleaned = parts.length <= 2 ? normalized : `${parts[0]}.${parts.slice(1).join("")}`;
  const num = Number(cleaned);
  return { cleanedText: cleaned, value: Number.isFinite(num) ? num : 0 };
}

/** Card formatting */
function formatCardNumber(raw: string) {
  const d = onlyDigits(raw).slice(0, 16);
  return d.replace(/(.{4})/g, "$1 ").trim();
}
function isCardNumberOk(raw: string) {
  return onlyDigits(raw).length === 16;
}
function formatExpiry(raw: string) {
  const d = onlyDigits(raw).slice(0, 4);
  if (d.length <= 2) return d;
  return `${d.slice(0, 2)}/${d.slice(2)}`;
}
function isExpiryOk(raw: string) {
  const d = onlyDigits(raw);
  if (d.length !== 4) return false;
  const mm = Number(d.slice(0, 2));
  const yy = Number(d.slice(2, 4));
  if (!Number.isFinite(mm) || !Number.isFinite(yy)) return false;
  if (mm < 1 || mm > 12) return false;

  const now = new Date();
  const curYY = Number(String(now.getFullYear()).slice(2));
  const curMM = now.getMonth() + 1;

  if (yy < curYY) return false;
  if (yy === curYY && mm < curMM) return false;

  return true;
}
function formatCvv(raw: string) {
  return onlyDigits(raw).slice(0, 3);
}
function isCvvOk(raw: string) {
  return onlyDigits(raw).length === 3;
}

/** IBAN (TR prefix fixed, user enters digits only) */
function enforceTrPrefixDigitsOnly(ibanDigits: string) {
  const d = onlyDigits(ibanDigits).slice(0, 24);
  return `TR${d}`;
}
function formatIbanTr(iban: string) {
  const clean = iban.replace(/\s+/g, "").toUpperCase();
  return clean.replace(/(.{4})/g, "$1 ").trim();
}
function isTrIbanOk(iban: string) {
  const clean = iban.replace(/\s+/g, "").toUpperCase();
  if (!clean.startsWith("TR")) return false;
  if (clean.length !== 26) return false;
  return /^\d+$/.test(clean.slice(2));
}

export default function TopupScreen() {
  const router = useRouter();

  // Session
  const [sessionCustomerId, setSessionCustomerId] = useState<string | null>(null);
  const [sessionAccountId, setSessionAccountId] = useState<string | null>(null);
  const [sessionCurrency, setSessionCurrency] = useState<string>("TRY");
  const [sessionReady, setSessionReady] = useState(false);

  // Form
  const [amountText, setAmountText] = useState("");
  const [cardNumber, setCardNumber] = useState("");
  const [expiry, setExpiry] = useState("");
  const [cvv, setCvv] = useState("");
  const [cardholder, setCardholder] = useState("");
  const [ibanDigits, setIbanDigits] = useState(""); // user types digits only
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    (async () => {
      try {
        const customerId = await AsyncStorage.getItem(SessionKeys.customerId);
        const accountId = await AsyncStorage.getItem(SessionKeys.accountId);
        const currency = await AsyncStorage.getItem(SessionKeys.currency);

        setSessionCustomerId(customerId);
        setSessionAccountId(accountId);
        if (currency) setSessionCurrency(currency);
      } finally {
        setSessionReady(true);
      }
    })();
  }, []);

  const amountParsed = useMemo(() => parseAmount(amountText), [amountText]);
  const amount = amountParsed.value;

  const cardOk = useMemo(() => isCardNumberOk(cardNumber), [cardNumber]);
  const expiryOk = useMemo(() => isExpiryOk(expiry), [expiry]);
  const cvvOk = useMemo(() => isCvvOk(cvv), [cvv]);
  const holderOk = useMemo(() => cardholder.trim().length >= 4, [cardholder]);

  const ibanRaw = useMemo(() => enforceTrPrefixDigitsOnly(ibanDigits), [ibanDigits]);
  const ibanFormatted = useMemo(() => formatIbanTr(ibanRaw), [ibanRaw]);
  const ibanOk = useMemo(() => isTrIbanOk(ibanRaw), [ibanRaw]);

  const canSubmit = useMemo(() => {
    if (loading) return false;
    if (!sessionReady) return false;
    if (!sessionCustomerId || !sessionAccountId) return false;

    if (!amountText.trim()) return false;
    if (amount <= 0) return false;
    if (amount > 1_000_000) return false;

    if (!cardOk || !expiryOk || !cvvOk || !holderOk) return false;

    // IBAN burada "referenceId" için kullanılacak → zorunlu tutuyorum
    if (!ibanOk) return false;

    return true;
  }, [loading, sessionReady, sessionCustomerId, sessionAccountId, amountText, amount, cardOk, expiryOk, cvvOk, holderOk, ibanOk]);

  const onConfirm = async () => {
    if (!sessionCustomerId || !sessionAccountId) {
      Alert.alert("Session missing", "Please login again.");
      router.replace("/(auth)/login");
      return;
    }

    const payload: TopUpPayload = {
      customerId: sessionCustomerId,
      accountId: sessionAccountId,
      amount: Number(amount.toFixed(2)),
      currency: sessionCurrency || "TRY",
      idempotencyKey: safeIdempotencyKey(),
      referenceId: ibanRaw.replace(/\s+/g, ""),
    };

    // 3DS ekranına geçebilmek için pending payload saklıyoruz
    try {
      setLoading(true);
      await AsyncStorage.setItem(PendingTopUpKey, JSON.stringify(payload));
      // 3D Secure simülasyon ekranına git
      router.push("/(tabs)/3ds");
    } catch {
      Alert.alert("Error", "Unable to continue. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <KeyboardAvoidingView style={{ flex: 1, backgroundColor: BG }} behavior={Platform.OS === "ios" ? "padding" : undefined}>
      <ScrollView style={styles.page} contentContainerStyle={styles.container} keyboardShouldPersistTaps="handled" showsVerticalScrollIndicator={false}>
        {/* Header */}
        <View style={styles.headerRow}>
          <Pressable onPress={() => router.back()} style={({ pressed }) => [styles.backBtn, pressed && { opacity: 0.9 }]}>
            <Ionicons name="chevron-back" size={18} color="rgba(255,255,255,0.85)" />
          </Pressable>

          <View style={{ flex: 1, minWidth: 0 }}>
            <Text style={styles.title}>Top up</Text>
            <Text style={styles.sub}>Add money using your card</Text>
          </View>
        </View>

        {/* Amount */}
        <View style={styles.card}>
          <Text style={styles.cardTitle}>Amount</Text>
          <Text style={styles.cardHint}>Enter how much you want to add.</Text>

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
              <Text style={styles.currencyText}>{sessionCurrency}</Text>
            </View>
          </View>

          <Text style={[styles.helper, { marginTop: 8 }]}>
            {amount > 0 ? `You will add ${amount.toFixed(2)} ${sessionCurrency}` : "Enter a valid amount"}
          </Text>
        </View>

        {/* Payment method */}
        <View style={styles.card}>
          <View style={styles.sectionHeader}>
            <Text style={styles.cardTitle}>Payment method</Text>
            <View style={styles.securePill}>
              <Ionicons name="lock-closed-outline" size={14} color={GOLD} />
              <Text style={styles.secureText}>3D Secure</Text>
            </View>
          </View>

          <Text style={styles.cardHint}>
            Card details are used for simulation only and are not stored.
          </Text>

          {/* Card number */}
          <View style={[styles.inputRow, cardNumber.length > 0 && !cardOk ? styles.inputRowDanger : null]}>
            <Ionicons name="card-outline" size={18} color={SOFT} />
            <TextInput
              value={formatCardNumber(cardNumber)}
              onChangeText={(t) => setCardNumber(onlyDigits(t).slice(0, 16))}
              placeholder="1234 5678 9012 3456"
              placeholderTextColor="rgba(255,255,255,0.25)"
              keyboardType="number-pad"
              style={styles.input}
              maxLength={19}
            />
            <View style={styles.brandPill}>
              <Text style={styles.brandText}>VISA / MC</Text>
            </View>
          </View>
          {cardNumber.length > 0 && !cardOk ? <Text style={[styles.error, { marginTop: 8 }]}>Card number must be 16 digits.</Text> : null}

          {/* Expiry + CVV */}
          <View style={styles.row}>
            <View style={[styles.rowItem, expiry.length > 0 && !expiryOk ? styles.rowItemDanger : null]}>
              <Ionicons name="calendar-outline" size={18} color={SOFT} />
              <TextInput
                value={formatExpiry(expiry)}
                onChangeText={(t) => setExpiry(onlyDigits(t).slice(0, 4))}
                placeholder="MM/YY"
                placeholderTextColor="rgba(255,255,255,0.25)"
                keyboardType="number-pad"
                style={styles.rowInput}
                maxLength={5}
              />
            </View>

            <View style={[styles.rowItem, cvv.length > 0 && !cvvOk ? styles.rowItemDanger : null]}>
              <Ionicons name="key-outline" size={18} color={SOFT} />
              <TextInput
                value={formatCvv(cvv)}
                onChangeText={(t) => setCvv(onlyDigits(t).slice(0, 3))}
                placeholder="CVV"
                placeholderTextColor="rgba(255,255,255,0.25)"
                keyboardType="number-pad"
                secureTextEntry
                style={styles.rowInput}
                maxLength={3}
              />
            </View>
          </View>
          {(expiry.length > 0 && !expiryOk) ? <Text style={[styles.error, { marginTop: 8 }]}>Invalid expiry date.</Text> : null}
          {(cvv.length > 0 && !cvvOk) ? <Text style={[styles.error, { marginTop: 8 }]}>CVV must be 3 digits.</Text> : null}

          {/* Cardholder */}
          <View style={[styles.inputRow, cardholder.length > 0 && !holderOk ? styles.inputRowDanger : null]}>
            <Ionicons name="person-outline" size={18} color={SOFT} />
            <TextInput
              value={cardholder}
              onChangeText={setCardholder}
              placeholder="Name on card"
              placeholderTextColor="rgba(255,255,255,0.25)"
              autoCapitalize="words"
              style={styles.input}
            />
          </View>
          {cardholder.length > 0 && !holderOk ? <Text style={[styles.error, { marginTop: 8 }]}>Enter the name on the card.</Text> : null}

          <View style={styles.divider} />

          {/* IBAN */}
          <Text style={styles.cardTitle}>Destination IBAN</Text>
          <Text style={styles.cardHint}>We use IBAN as the referenceId for the top up operation.</Text>

          <View style={styles.ibanWrap}>
            <View style={styles.ibanPrefix}>
              <Text style={styles.ibanPrefixText}>TR</Text>
            </View>

            <TextInput
              value={ibanFormatted.slice(2).trimStart()}
              onChangeText={(t) => setIbanDigits(onlyDigits(t).slice(0, 24))}
              placeholder="00 0000 0000 0000 0000 0000 00"
              placeholderTextColor="rgba(255,255,255,0.20)"
              keyboardType="number-pad"
              style={styles.ibanInput}
              maxLength={24 + 6}
            />
          </View>

            <Text style={[styles.helper, { marginTop: 8 }]}>Format: TR + 24 digits</Text>

          <Pressable
            onPress={onConfirm}
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
                <Text style={styles.primaryBtnText}>Continuing...</Text>
              </>
            ) : (
              <>
                <Ionicons name="shield-checkmark-outline" size={18} color="rgba(255,255,255,0.92)" />
                <Text style={styles.primaryBtnText}>Continue to 3D Secure</Text>
              </>
            )}
          </Pressable>

          {sessionReady && (!sessionCustomerId || !sessionAccountId) ? (
            <Text style={[styles.error, { marginTop: 10 }]}>Session not found. Please login again.</Text>
          ) : null}
        </View>

        {/* Note */}
        <View style={styles.card}>
          <View style={styles.tipRow}>
            <Ionicons name="lock-closed-outline" size={18} color={GOLD} />
            <Text style={styles.tipText}>
              Duplicate protection is enabled using an idempotency key. You’ll receive the final result after 3D Secure.
            </Text>
          </View>
        </View>

        <View style={{ height: 26 }} />
      </ScrollView>
    </KeyboardAvoidingView>
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

  sectionHeader: { flexDirection: "row", alignItems: "center", justifyContent: "space-between" },
  securePill: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderRadius: 999,
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
  },
  secureText: { marginLeft: 6, color: "rgba(255,255,255,0.86)", fontWeight: "900", fontSize: 11.5 },

  brandPill: {
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderRadius: 999,
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: BORDER,
    marginLeft: 10,
  },
  brandText: { color: "rgba(255,255,255,0.70)", fontWeight: "900", fontSize: 11 },

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
  inputRowDanger: { borderColor: "rgba(255,71,87,0.35)" },
  input: {
    flex: 1,
    minWidth: 0,
    color: "rgba(255,255,255,0.92)",
    fontWeight: "900",
    fontSize: 14,
    marginLeft: 10,
    paddingVertical: 0,
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
  error: { color: DANGER, fontWeight: "800", fontSize: 11.5 },

  row: { flexDirection: "row", marginTop: 12 },
  rowItem: {
    flex: 1,
    minWidth: 0,
    borderRadius: 14,
    paddingHorizontal: 12,
    paddingVertical: 10,
    backgroundColor: "rgba(0,0,0,0.18)",
    borderWidth: 1,
    borderColor: BORDER,
    flexDirection: "row",
    alignItems: "center",
  },
  rowItemDanger: { borderColor: "rgba(255,71,87,0.35)" },
  rowInput: {
    flex: 1,
    minWidth: 0,
    marginLeft: 10,
    color: "rgba(255,255,255,0.92)",
    fontWeight: "900",
    fontSize: 14,
    paddingVertical: 0,
  },

  // spacing between expiry & cvv without gap (stable on all RN)
  // we'll apply by wrapping in row and using marginRight on first
  // (implemented via inline? no → do here)
  divider: { height: 1, backgroundColor: BORDER, marginTop: 16, marginBottom: 14 },

  ibanWrap: {
    marginTop: 10,
    borderRadius: 14,
    backgroundColor: "rgba(0,0,0,0.18)",
    borderWidth: 1,
    borderColor: BORDER,
    flexDirection: "row",
    alignItems: "center",
    overflow: "hidden",
  },
  ibanDanger: { borderColor: "rgba(255,71,87,0.35)" },
  ibanPrefix: {
    paddingHorizontal: 14,
    paddingVertical: 12,
    borderRightWidth: 1,
    borderRightColor: "rgba(246,195,64,0.25)",
    backgroundColor: "rgba(246,195,64,0.08)",
  },
  ibanPrefixText: { color: "rgba(255,255,255,0.92)", fontWeight: "900", letterSpacing: 1 },
  ibanInput: {
    flex: 1,
    minWidth: 0,
    paddingHorizontal: 12,
    paddingVertical: 12,
    color: "rgba(255,255,255,0.92)",
    fontWeight: "900",
    fontSize: 14,
  },

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
