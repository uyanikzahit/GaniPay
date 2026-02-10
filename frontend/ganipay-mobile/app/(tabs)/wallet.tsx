// app/(tabs)/wallet.tsx
import React, { useCallback, useEffect, useMemo, useState } from "react";
import { View, Text, StyleSheet, ScrollView, Pressable, Platform } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useRouter, useFocusEffect } from "expo-router";
import * as Clipboard from "expo-clipboard";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { loadSession, StoredSession, SessionKeys } from "@/constants/storage";

const BG = "#0B1220";
const CARD = "rgba(255,255,255,0.06)";
const BORDER = "rgba(255,255,255,0.10)";
const GOLD = "rgba(246,195,64,1)";
const MUTED = "rgba(255,255,255,0.60)";
const SOFT = "rgba(255,255,255,0.35)";

type CurrencyCode = "TRY" | "USD" | "EUR" | string;

type BalanceHistoryItem = {
  id: string;
  accountId: string;
  direction: "debit" | "credit" | string;
  changeAmount: number;
  balanceBefore: number;
  balanceAfter: number;
  currency: CurrencyCode;
  operationType: number;
  referenceId: string;
  createdAt: string;
};

function safeStr(v: any): string | null {
  if (v === undefined || v === null) return null;
  const s = String(v).trim();
  return s.length ? s : null;
}

function currencyLabel(code: CurrencyCode) {
  // ekranda "TR" istedin -> TRY için TR göstereyim
  if (String(code).toUpperCase() === "TRY") return "TR";
  return String(code).toUpperCase();
}

function moneyAbs(amount: number, currency: CurrencyCode) {
  const abs = Math.abs(amount);
  const c = String(currency).toUpperCase();
  const symbol = c === "TRY" ? "₺" : c === "USD" ? "$" : c === "EUR" ? "€" : "";
  return `${symbol}${abs.toFixed(2)}`;
}

function formatDateTime(dt: string) {
  const d = new Date(dt);
  const date = d.toLocaleDateString("tr-TR");
  const time = d.toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" });
  return `${date} • ${time}`;
}

function txKind(direction: string) {
  const d = String(direction).toLowerCase();
  return d === "debit" ? "Outgoing" : "Incoming";
}

// NOTE: enum netleşince kesinleştiririz
function isTopUp(h: BalanceHistoryItem) {
  return h.operationType === 1 || String(h.direction).toLowerCase() === "credit";
}

function txTitle(h: BalanceHistoryItem) {
  return isTopUp(h) ? "Top Up" : "Transfer";
}

function getAccountingBaseUrl() {
  // ✅ Artık accounting'e direkt porttan değil APISIX gateway üzerinden gidiyoruz (9080)
  if (Platform.OS === "web") return "http://localhost:9080";
  if (Platform.OS === "android") return "http://10.0.2.2:9080";
  return "http://192.168.1.5:9080";
}

export default function WalletScreen() {
  const router = useRouter();

  const [session, setSession] = useState<StoredSession | null>(null);
  const [history, setHistory] = useState<BalanceHistoryItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [copyOk, setCopyOk] = useState(false);

  useEffect(() => {
    let alive = true;
    (async () => {
      const s = await loadSession();
      if (!alive) return;
      setSession(s);
    })();
    return () => {
      alive = false;
    };
  }, []);

  const wallet = useMemo(() => {
    const wallets = (session?.wallets ?? {}) as any;
    const accounts: any[] = Array.isArray(wallets?.accounts) ? wallets.accounts : [];

    const accountId = safeStr(session?.accountId) ?? safeStr(accounts?.[0]?.accountId) ?? null;
    const currency = (safeStr(session?.currency) ?? safeStr(accounts?.[0]?.currency) ?? "TRY") as CurrencyCode;

    const bal = accounts?.[0]?.balance;
    const available = typeof bal === "number" ? bal : 0;

    const customer = (session?.customer ?? session?.user ?? {}) as any;
    const fullName = `${safeStr(customer?.firstName) ?? ""} ${safeStr(customer?.lastName) ?? ""}`.trim() || "GaniPay User";

    return { accountId, currency, available, fullName };
  }, [session]);

  const fetchHistory = useCallback(async () => {
    if (!wallet.accountId) {
      setHistory([]);
      return;
    }

    setLoading(true);
    try {
      const baseUrl = getAccountingBaseUrl();
      // ✅ APISIX route prefix eklendi: /accounting-api
      const url = `${baseUrl}/accounting-api/api/accounting/accounts/${encodeURIComponent(wallet.accountId)}/balance-history`;

      // ✅ Bearer token (Authorize varsa şart)
      const token = await AsyncStorage.getItem(SessionKeys.accessToken);

      const res = await fetch(url, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          ...(token ? { Authorization: `Bearer ${token}` } : {}),
        },
      });

      if (!res.ok) {
        // burada 404 vs olunca direkt boş gösterelim (UI bozulmasın)
        setHistory([]);
        return;
      }

      const data = (await res.json()) as BalanceHistoryItem[];
      const arr = Array.isArray(data) ? data : [];
      arr.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
      setHistory(arr);
    } catch {
      setHistory([]);
    } finally {
      setLoading(false);
    }
  }, [wallet.accountId]);

  // ✅ ekrana her dönüşte (TopUp/Transfer sonrası) otomatik yenile
  useFocusEffect(
    useCallback(() => {
      if (wallet.accountId) fetchHistory();
    }, [wallet.accountId, fetchHistory])
  );

  useEffect(() => {
    if (wallet.accountId) fetchHistory();
  }, [wallet.accountId, fetchHistory]);

  const latestBalance = useMemo(() => {
    if (history.length > 0) return history[0].balanceAfter;
    return wallet.available;
  }, [history, wallet.available]);

  const onCopyWalletNumber = useCallback(async () => {
    if (!wallet.accountId) return;
    await Clipboard.setStringAsync(wallet.accountId);
    setCopyOk(true);
    setTimeout(() => setCopyOk(false), 900);
  }, [wallet.accountId]);

  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.container} showsVerticalScrollIndicator={false}>
      {/* HEADER */}
      <View style={styles.headerRow}>
        <View>
          <Text style={styles.title}>Wallet</Text>
          <Text style={styles.sub}>Balances & wallet activity</Text>
        </View>

        <Pressable
          onPress={() => router.push("/(tabs)/account")}
          style={({ pressed }) => [styles.pillBtn, pressed && { opacity: 0.9 }]}
        >
          <Text style={styles.pillBtnText}>Account</Text>
          <Ionicons name="chevron-forward" size={14} color={SOFT} />
        </Pressable>
      </View>

      {/* HERO (refresh kaldırıldı) */}
      <View style={[styles.card, styles.heroCard]}>

        {/* Wallet number + copy */}
        <View style={{ marginTop: 14 }}>
          <Text style={styles.label}>Wallet number</Text>

          <View style={styles.numberRow}>
            <Text style={styles.walletNumber} numberOfLines={1} ellipsizeMode="middle">
              {wallet.accountId ?? "—"}
            </Text>

            <Pressable
              onPress={onCopyWalletNumber}
              disabled={!wallet.accountId}
              style={({ pressed }) => [
                styles.copyBtn,
                !wallet.accountId && { opacity: 0.5 },
                pressed && { opacity: 0.9 },
              ]}
              hitSlop={10}
            >
              <Ionicons name={copyOk ? "checkmark" : "copy-outline"} size={16} color={copyOk ? "rgba(46,213,115,0.95)" : "rgba(255,255,255,0.85)"} />
              <Text style={styles.copyText}>{copyOk ? "Copied" : "Copy"}</Text>
            </Pressable>
          </View>
        </View>

        {/* Balance yukarı + currency pill */}
        <View style={styles.balanceRow}>
          <View>
            <Text style={styles.label}>Balance</Text>
            <Text style={styles.balanceValue}>
              {moneyAbs(latestBalance, wallet.currency)}
            </Text>
          </View>

          <View style={styles.currencyPill}>
            <Text style={styles.currencyPillK}>Currency</Text>
            <Text style={styles.currencyPillV}>{currencyLabel(wallet.currency)}</Text>
          </View>
        </View>

        {/* küçük sync text (buton yok) */}
        <Text style={styles.syncText}>{loading ? "Syncing…" : "Up to date"}</Text>
      </View>

      {/* TRANSACTIONS (dashboard tonları) */}
      <View style={styles.txHeader}>
        <Text style={styles.txTitle}>Last Transactions</Text>
        {/* view all kaldırıldı */}
      </View>

      {history.length === 0 ? (
        <View style={[styles.card, { padding: 14 }]}>
          <Text style={{ color: "rgba(255,255,255,0.85)", fontWeight: "900" }}>
            {loading ? "Loading…" : "No transactions yet"}
          </Text>
          <Text style={{ marginTop: 6, color: MUTED, fontWeight: "700", fontSize: 12 }}>
            Your Top Up and Transfer movements will appear here.
          </Text>
        </View>
      ) : (
        history.map((h) => {
          const out = String(h.direction).toLowerCase() === "debit";
          const title = txTitle(h);
          const signed = out ? -Math.abs(h.changeAmount) : Math.abs(h.changeAmount);

          // ✅ istediğin kural:
          // TopUp -> accountId
          // Transfer -> referenceId
          const idLine = title === "Top Up" ? `Account ${h.accountId}` : `Ref ${h.referenceId}`;

          return (
            <View key={h.id} style={styles.txCard}>
              <View style={styles.txTop}>
                <Text style={[styles.txName, out ? styles.txNameOut : styles.txNameIn]}>{title}</Text>
                <Text style={[styles.txAmount, out ? styles.txAmountOut : styles.txAmountIn]}>
                  {out ? "-" : "+"} {moneyAbs(signed, h.currency)}
                </Text>
              </View>

              <Text style={styles.txSub} numberOfLines={1} ellipsizeMode="tail">
                {formatDateTime(h.createdAt)} • {txKind(h.direction)}
              </Text>

              {/* ✅ id tek satır, parçalı görünmesin */}
              <Text style={styles.txId} numberOfLines={1} ellipsizeMode="tail">
                {idLine}
              </Text>
            </View>
          );
        })
      )}

      <View style={{ height: 26 }} />
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  page: { flex: 1, backgroundColor: BG },
  container: { padding: 16, paddingBottom: 28 },

  headerRow: { flexDirection: "row", alignItems: "center", justifyContent: "space-between" },
  title: { color: "rgba(255,255,255,0.92)", fontSize: 20, fontWeight: "900" },
  sub: { marginTop: 6, color: MUTED, fontSize: 12.5, fontWeight: "700" },

  pillBtn: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderRadius: 999,
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: BORDER,
  },
  pillBtnText: { marginRight: 4, color: "rgba(255,255,255,0.80)", fontWeight: "900", fontSize: 11.5 },

  card: {
    marginTop: 14,
    borderRadius: 18,
    padding: 14,
    backgroundColor: CARD,
    borderWidth: 1,
    borderColor: BORDER,
  },

  heroCard: { padding: 16 },

  welcomeSmall: { color: "rgba(255,255,255,0.70)", fontWeight: "800", fontSize: 12 },
  welcomeName: { marginTop: 6, color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 18 },
  heroHint: { marginTop: 6, color: MUTED, fontWeight: "700", fontSize: 12.2 },

  label: { color: MUTED, fontWeight: "800", fontSize: 11.5 },

  numberRow: { marginTop: 8, flexDirection: "row", alignItems: "center", gap: 10 },
  walletNumber: { flex: 1, color: "rgba(255,255,255,0.90)", fontWeight: "900", fontSize: 12.5 },

  copyBtn: {
    flexDirection: "row",
    alignItems: "center",
    gap: 6,
    paddingHorizontal: 10,
    paddingVertical: 8,
    borderRadius: 999,
    backgroundColor: "rgba(0,0,0,0.18)",
    borderWidth: 1,
    borderColor: BORDER,
  },
  copyText: { color: "rgba(255,255,255,0.85)", fontWeight: "900", fontSize: 11.5 },

  // ✅ balance yukarı alınmış düzen
  balanceRow: {
    marginTop: 14,
    flexDirection: "row",
    alignItems: "flex-end",
    justifyContent: "space-between",
    gap: 12,
  },
  balanceValue: { marginTop: 6, color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 22 },

  currencyPill: {
    minWidth: 120,
    borderRadius: 16,
    paddingVertical: 10,
    paddingHorizontal: 12,
    backgroundColor: "rgba(0,0,0,0.18)",
    borderWidth: 1,
    borderColor: BORDER,
    alignItems: "flex-start",
  },
  currencyPillK: { color: MUTED, fontWeight: "800", fontSize: 11.5 },
  currencyPillV: { marginTop: 6, color: GOLD, fontWeight: "900", fontSize: 14.5 },

  syncText: { marginTop: 10, color: "rgba(255,255,255,0.55)", fontWeight: "800", fontSize: 11.5 },

  txHeader: { marginTop: 16, marginBottom: 4, flexDirection: "row", alignItems: "center", justifyContent: "space-between" },
  txTitle: { color: "rgba(255,255,255,0.92)", fontSize: 14, fontWeight: "900" },

  // ✅ dashboard tonları (koyu card + yeşil/kırmızı text)
  txCard: {
    marginTop: 12,
    borderRadius: 16,
    padding: 14,
    backgroundColor: "rgba(0,0,0,0.18)",
    borderWidth: 1,
    borderColor: BORDER,
  },
  txTop: { flexDirection: "row", alignItems: "center", justifyContent: "space-between" },

  txName: { fontWeight: "900", fontSize: 13.5 },
  txNameIn: { color: "rgba(46,213,115,0.95)" }, // ✅ yeşil başlık (topup)
  txNameOut: { color: "rgba(255,71,87,0.95)" }, // ✅ kırmızı başlık (transfer-out)

  txAmount: { fontWeight: "900", fontSize: 13.5 },
  txAmountIn: { color: "rgba(46,213,115,0.95)" },
  txAmountOut: { color: "rgba(255,71,87,0.95)" },

  txSub: { marginTop: 8, color: "rgba(255,255,255,0.70)", fontWeight: "700", fontSize: 11.5 },
  txId: { marginTop: 6, color: "rgba(255,255,255,0.60)", fontWeight: "800", fontSize: 11.5 },
});
