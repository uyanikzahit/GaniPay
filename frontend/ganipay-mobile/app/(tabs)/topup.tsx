import React, { useMemo, useState } from "react";
import { View, Text, StyleSheet, ScrollView, Pressable, TextInput, Alert } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";

const BG = "#0B1220";
const CARD = "rgba(255,255,255,0.06)";
const BORDER = "rgba(255,255,255,0.10)";
const GOLD = "rgba(246,195,64,1)";
const MUTED = "rgba(255,255,255,0.60)";
const SOFT = "rgba(255,255,255,0.35)";

type TopupRequest = {
  customerId: string;
  accountId: string;
  amount: number;
  currency: string;
  idempotencyKey: string;
  referenceId: string; // UI'da IBAN gibi gösteriyoruz
};

function safeIdempotencyKey() {
  return `idem_${Date.now()}_${Math.random().toString(16).slice(2)}`;
}

function parseAmount(text: string) {
  const normalized = text.replace(",", ".").replace(/[^0-9.]/g, "");
  const parts = normalized.split(".");
  const cleaned = parts.length <= 2 ? normalized : `${parts[0]}.${parts.slice(1).join("")}`;
  const num = Number(cleaned);
  return { cleanedText: cleaned, value: Number.isFinite(num) ? num : 0 };
}

function digits(n: number) {
  let out = "";
  for (let i = 0; i < n; i++) out += Math.floor(Math.random() * 10).toString();
  return out;
}

function makeMockTrIban() {
  // TR + 2 check digit + 5 bank + 1 reserve + 16 account-like digits = 26 chars total (TR dahil 26)
  // Bu checksum doğrulamalı değil; UI mock için "IBAN gibi" görünür.
  const check = digits(2);
  const bank = digits(5);
  const reserve = digits(1);
  const account = digits(16);
  return `TR${check}${bank}${reserve}${account}`;
}

function formatIban(ibanRaw: string) {
  const clean = ibanRaw.replace(/\s+/g, "").toUpperCase().replace(/[^A-Z0-9]/g, "");
  // 4'lü gruplar
  return clean.replace(/(.{4})/g, "$1 ").trim();
}

function cleanIbanInput(text: string) {
  return text.toUpperCase().replace(/[^A-Z0-9]/g, "");
}

function isValidIbanShape(ibanRaw: string) {
  const clean = ibanRaw.replace(/\s+/g, "").toUpperCase();
  // MVP: TR ile başlasın ve min uzunluk sağlasın
  if (!clean.startsWith("TR")) return false;
  if (clean.length < 24) return false; // TR IBAN 26 ama kullanıcı yarım girebilir diye 24 altını kapatalım
  if (clean.length > 34) return false;
  return true;
}

export default function TopupScreen() {
  const router = useRouter();

  // ✅ Hidden alanlar (sonra login/session'dan gelecek)
  const hidden = useMemo(
    () => ({
      customerId: "mock-customer-id",
      accountId: "mock-account-id",
      currency: "TRY",
    }),
    []
  );

  const [amountText, setAmountText] = useState("");
  const [ibanRaw, setIbanRaw] = useState(() => makeMockTrIban()); // ekrana girince otomatik dolu
  const [loading, setLoading] = useState(false);

  const amountParsed = useMemo(() => parseAmount(amountText), [amountText]);
  const amount = amountParsed.value;

  const ibanFormatted = useMemo(() => formatIban(ibanRaw), [ibanRaw]);
  const ibanOk = useMemo(() => isValidIbanShape(ibanRaw), [ibanRaw]);

  const canSubmit = useMemo(() => {
    if (loading) return false;
    if (!amountText.trim()) return false;
    if (amount <= 0) return false;
    if (amount > 1_000_000) return false;
    if (!ibanOk) return false;
    return true;
  }, [loading, amountText, amount, ibanOk]);

  const regenerateIban = () => setIbanRaw(makeMockTrIban());

  const onSubmit = async () => {
    const req: TopupRequest = {
      customerId: hidden.customerId,
      accountId: hidden.accountId,
      amount: Number(amount.toFixed(2)),
      currency: hidden.currency,
      idempotencyKey: safeIdempotencyKey(),
      referenceId: ibanRaw.replace(/\s+/g, "").toUpperCase(),
    };

    try {
      setLoading(true);

      // MOCK NETWORK (sonra gerçek request burada olacak)
      await new Promise((r) => setTimeout(r, 650));

      Alert.alert(
        "Top up request created",
        `Amount: ${req.amount.toFixed(2)} ${req.currency}\nIBAN: ${formatIban(req.referenceId)}`,
        [
          {
            text: "Done",
            onPress: () => router.push("/(tabs)/wallet"),
          },
        ]
      );

      setAmountText("");
      setIbanRaw(makeMockTrIban());
    } catch {
      Alert.alert("Top up failed", "Please try again.");
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

        <View style={{ flex: 1 }}>
          <Text style={styles.title}>Top up</Text>
          <Text style={styles.sub}>Add money to your wallet</Text>
        </View>
      </View>

      {/* FORM */}
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
            <Text style={styles.currencyText}>{hidden.currency}</Text>
          </View>
        </View>

        <Text style={[styles.helper, { marginTop: 8 }]}>
          {amount > 0 ? `You will add ${amount.toFixed(2)} ${hidden.currency}` : "Enter a valid amount"}
        </Text>

        <View style={styles.divider} />

        <View style={styles.ibanHeader}>
          <Text style={styles.cardTitle}>Destination IBAN</Text>

          <Pressable onPress={regenerateIban} style={({ pressed }) => [styles.ghostBtn, pressed && { opacity: 0.9 }]}>
            <Ionicons name="refresh-outline" size={16} color={GOLD} />
            <Text style={styles.ghostBtnText}>New</Text>
          </Pressable>
        </View>

        <Text style={styles.cardHint}>Funds will be deposited to this IBAN.</Text>

        <View style={[styles.inputRow, !ibanOk && { borderColor: "rgba(255,71,87,0.35)" }]}>
          <Ionicons name="business-outline" size={18} color={SOFT} />
          <TextInput
            value={ibanFormatted}
            onChangeText={(t) => setIbanRaw(cleanIbanInput(t))}
            placeholder="TR00 0000 0000 0000 0000 0000 00"
            placeholderTextColor="rgba(255,255,255,0.25)"
            autoCapitalize="characters"
            style={styles.input}
            maxLength={34 + 8} // boşluk payı
          />
        </View>

        {!ibanOk ? (
          <Text style={[styles.helper, { marginTop: 8, color: "rgba(255,71,87,0.85)" }]}>Please enter a valid IBAN format.</Text>
        ) : (
          <Text style={[styles.helper, { marginTop: 8 }]}>Example: TRxx xxxx xxxx xxxx xxxx xxxx xx</Text>
        )}

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
              <Text style={styles.primaryBtnText}>Processing...</Text>
            </>
          ) : (
            <>
              <Ionicons name="checkmark-circle-outline" size={18} color="rgba(255,255,255,0.92)" />
              <Text style={styles.primaryBtnText}>Confirm top up</Text>
            </>
          )}
        </Pressable>
      </View>

      {/* NOTE */}
      <View style={styles.card}>
        <View style={styles.tipRow}>
          <Ionicons name="lock-closed-outline" size={18} color={GOLD} />
          <Text style={styles.tipText}>
            This action is protected against duplicate processing. You’ll see a receipt after the operation completes.
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

  ibanHeader: { flexDirection: "row", alignItems: "center", justifyContent: "space-between" },

  ghostBtn: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderRadius: 999,
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
  },
  ghostBtnText: { marginLeft: 6, color: "rgba(255,255,255,0.86)", fontWeight: "900", fontSize: 11.5 },

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
