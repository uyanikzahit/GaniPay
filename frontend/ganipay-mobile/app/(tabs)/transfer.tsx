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

function parseAmount(text: string) {
  const normalized = text.replace(",", ".").replace(/[^0-9.]/g, "");
  const parts = normalized.split(".");
  const cleaned = parts.length <= 2 ? normalized : `${parts[0]}.${parts.slice(1).join("")}`;
  const num = Number(cleaned);
  return { cleanedText: cleaned, value: Number.isFinite(num) ? num : 0 };
}

function maskPhone(phone: string) {
  const clean = phone.replace(/\D/g, "");
  if (clean.length < 7) return phone;
  return `${clean.slice(0, 3)}****${clean.slice(-2)}`;
}

export default function TransferScreen() {
  const router = useRouter();

  const [phone, setPhone] = useState("");
  const [amountText, setAmountText] = useState("");
  const [note, setNote] = useState("");
  const [loading, setLoading] = useState(false);

  const amountParsed = useMemo(() => parseAmount(amountText), [amountText]);
  const amount = amountParsed.value;

  const canSubmit = useMemo(() => {
    if (loading) return false;
    if (phone.replace(/\D/g, "").length < 10) return false;
    if (!amountText.trim() || amount <= 0) return false;
    if (amount > 1_000_000) return false;
    return true;
  }, [loading, phone, amountText, amount]);

  const onSubmit = async () => {
    try {
      setLoading(true);
      await new Promise((r) => setTimeout(r, 650));

      Alert.alert(
        "Transfer created",
        `Receiver: ${maskPhone(phone)}\nAmount: ${amount.toFixed(2)} TRY${note ? `\nNote: ${note}` : ""}`,
        [{ text: "Done", onPress: () => router.push("/(tabs)/wallet") }]
      );

      setPhone("");
      setAmountText("");
      setNote("");
    } catch {
      Alert.alert("Transfer failed", "Please try again.");
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
          <Text style={styles.title}>Transfer</Text>
          <Text style={styles.sub}>Send money instantly</Text>
        </View>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Receiver</Text>
        <Text style={styles.cardHint}>Enter the recipient phone number.</Text>

        <View style={styles.inputRow}>
          <Ionicons name="call-outline" size={18} color={SOFT} />
          <TextInput
            value={phone}
            onChangeText={setPhone}
            placeholder="e.g. 506 000 00 00"
            placeholderTextColor="rgba(255,255,255,0.25)"
            keyboardType="phone-pad"
            style={styles.input}
          />
        </View>

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
              <Text style={styles.primaryBtnText}>Processing...</Text>
            </>
          ) : (
            <>
              <Ionicons name="send-outline" size={18} color="rgba(255,255,255,0.92)" />
              <Text style={styles.primaryBtnText}>Send</Text>
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
