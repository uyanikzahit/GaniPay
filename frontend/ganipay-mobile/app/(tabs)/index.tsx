// app/(tabs)/index.tsx
import React, { useCallback, useMemo, useState, useEffect } from "react";
import { View, Text, StyleSheet, TouchableOpacity, ScrollView, Platform } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { LinearGradient } from "expo-linear-gradient";
import { useRouter, useFocusEffect } from "expo-router";




import AsyncStorage from "@react-native-async-storage/async-storage";
import { Colors } from "../theme/colors";
import { SessionKeys } from "../../constants/storage";
import { getCustomerBalance, getAccountBalanceHistory } from "./dashboard.api";

type DashboardUser = {
  firstName?: string;
  lastName?: string;
};

type BalanceDto = {
  accountId?: string;
  customerId?: string;
  currency?: string;
  balance?: number;
};

type BalanceHistoryItem = {
  id?: string;
  accountId?: string;
  direction?: "credit" | "debit" | string;
  amount?: number;
  currency?: string;
  createdAt?: string;
  operationType?: string | number;
  referenceId?: string;
};

function formatMoney(amount: number, currency = "TRY") {
  const symbol = currency === "TRY" ? "₺" : currency === "USD" ? "$" : currency + " ";
  const safe = Number.isFinite(amount) ? amount : 0;
  const formatted = safe.toLocaleString("tr-TR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  return `${symbol}${formatted}`;
}

function formatTxMeta(dateIso?: string, direction?: string) {
  if (!dateIso) return direction ? direction : "";
  const d = new Date(dateIso);
  const date = d.toLocaleDateString("tr-TR");
  const time = d.toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" });

  const dirLabel =
    direction === "credit" ? "Incoming" : direction === "debit" ? "Outgoing" : direction ? String(direction) : "";

  return `${date} • ${time}${dirLabel ? ` • ${dirLabel}` : ""}`;
}

function mapTxTitle(tx: BalanceHistoryItem) {
  if (tx.direction === "credit") return "Top Up";
  if (tx.direction === "debit") return "Transfer";
  return "Transaction";
}

export default function HomeScreen() {
  const router = useRouter();

  const [initialLoading, setInitialLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [user, setUser] = useState<DashboardUser>({});
  const [balanceDto, setBalanceDto] = useState<BalanceDto>({});
  const [history, setHistory] = useState<BalanceHistoryItem[]>([]);
  const [error, setError] = useState<string | null>(null);

const load = useCallback(async (opts?: { silent?: boolean }) => {
  const silent = opts?.silent === true;

  try {
    setError(null);

    // ✅ İlk açılışta loading göster, geri dönüşte gösterme
    if (silent) setRefreshing(true);
    else setInitialLoading(true);

    const [customerId, currency, accountIdFromSession, userJson] = await AsyncStorage.multiGet([
      SessionKeys.customerId,
      SessionKeys.currency,
      SessionKeys.accountId,
      SessionKeys.user,
    ]).then((arr) => arr.map((x) => x[1] ?? ""));

    if (userJson) {
      try {
        const u = JSON.parse(userJson) as DashboardUser;
        // ✅ sessiz yenilemede de isim hemen kalsın
        setUser({ firstName: u?.firstName, lastName: u?.lastName });
      } catch {}
    }

    if (!customerId) throw new Error("customerId bulunamadı. Login sonrası SessionKeys.customerId set edildi mi?");

    const cur = (currency || "TRY").toUpperCase();

    const balanceRes = await getCustomerBalance(customerId, cur);

    const accountId = balanceRes?.accountId || accountIdFromSession;
    if (!accountId) throw new Error("accountId bulunamadı. Balance response accountId dönmüyor mu?");

    setBalanceDto({
      accountId,
      customerId: balanceRes?.customerId || customerId,
      currency: (balanceRes?.currency || cur).toUpperCase(),
      balance: typeof balanceRes?.balance === "number" ? balanceRes.balance : Number(balanceRes?.balance ?? 0),
    });

    const historyRes = await getAccountBalanceHistory(accountId);

    const mapped: BalanceHistoryItem[] = (historyRes ?? []).map((x: any) => ({
      id: x?.id,
      accountId: x?.accountId,
      direction: x?.direction,
      amount: typeof x?.changeAmount === "number" ? x.changeAmount : Number(x?.changeAmount ?? x?.amount ?? 0),
      currency: x?.currency,
      createdAt: x?.createdAt,
      operationType: x?.operationType,
      referenceId: x?.referenceId,
    }));

    setHistory(Array.isArray(mapped) ? mapped : []);
  } catch (e: any) {
    setError(e?.message ?? "Dashboard load failed");
  } finally {
    if (silent) setRefreshing(false);
    else setInitialLoading(false);
  }
}, []);



useEffect(() => {
  load({ silent: false });
}, [load]);

  useFocusEffect(
    useCallback(() => {
      // ✅ geri gelince UI flicker yok, arkada yenile
      load({ silent: true });
    }, [load])
  );

  const fullName = useMemo(() => {
    const fn = (user.firstName ?? "").trim();
    const ln = (user.lastName ?? "").trim();
    const name = `${fn} ${ln}`.trim();
    return name || "User";
  }, [user.firstName, user.lastName]);

  const balanceText = useMemo(() => {
    const amount = balanceDto.balance ?? 0;
    const cur = balanceDto.currency ?? "TRY";
    return formatMoney(amount, cur);
  }, [balanceDto.balance, balanceDto.currency]);

  const last3 = useMemo(() => {
    const sorted = [...history].sort((a, b) => {
      const ad = a?.createdAt ? new Date(a.createdAt).getTime() : 0;
      const bd = b?.createdAt ? new Date(b.createdAt).getTime() : 0;
      return bd - ad;
    });
    return sorted.slice(0, 3);
  }, [history]);

  return (
    <View style={styles.container}>
      <LinearGradient
        colors={[Colors.bg, "#0B1220", "#12223E"]}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        style={styles.hero}
      >
        <LinearGradient
          colors={["rgba(246,195,64,0.55)", "rgba(246,195,64,0)"]}
          start={{ x: 0.15, y: 0 }}
          end={{ x: 0.85, y: 1 }}
          style={styles.goldGlow}
        />

        <ScrollView contentContainerStyle={styles.content} showsVerticalScrollIndicator={false}>
          <View style={styles.headerCard}>
            <View style={styles.headerRow}>
              <View>
                <Text style={styles.welcome}>Welcome,</Text>
                <Text style={styles.name}>{initialLoading ? "Loading..." : fullName}</Text>
              </View>

              <View style={{ alignItems: "flex-end" }}>
                <Text style={styles.balanceLabel}>Balance</Text>
                <Text style={styles.balance}>{initialLoading ? "…" : balanceText}</Text>
              </View>
            </View>

            <Text style={styles.subHint}>{error ? error : "Your wallet is ready for quick actions."}</Text>
          </View>

          <View style={styles.actionGrid}>
            <ActionTile title="Top Up" icon="wallet-outline" variant="teal" onPress={() => router.push("/topup")} />
            <ActionTile
              title="Transfer"
              icon="swap-horizontal-outline"
              variant="blue"
              onPress={() => router.push("/transfer")}
            />
            {/* ✅ artık disabled değil -> yönlendirme var */}
            <ActionTile
              title="Pay Bills"
              icon="receipt-outline"
              variant="purple"
              onPress={() => router.push("/paybills")}
            />
            <ActionTile
              title="Partner Stores"
              icon="storefront-outline"
              variant="orange"
              onPress={() => router.push("/partners")}
            />
          </View>

          <View style={styles.sectionHeader}>
            <Text style={styles.sectionTitle}>Last Transactions</Text>
            <Text style={styles.sectionLink} onPress={() => router.push("/wallet")}>
              View all
            </Text>
          </View>

          {last3.length === 0 ? (
            <View style={styles.txItem}>
              <View style={{ gap: 4 }}>
                <Text style={styles.txTitle}>No transactions yet</Text>
                <Text style={styles.txMeta}>Your recent activity will appear here.</Text>
              </View>
              <Text style={styles.txAmount}> </Text>
            </View>
          ) : (
            last3.map((tx, idx) => {
              const title = mapTxTitle(tx);
              const isCredit = String(tx.direction).toLowerCase() === "credit";

              const amount = typeof tx.amount === "number" ? tx.amount : Number(tx.amount ?? 0);
              const cur = (tx.currency ?? balanceDto.currency ?? "TRY").toUpperCase();
              const amountText = `${isCredit ? "+" : "-"} ${formatMoney(Math.abs(amount), cur)}`;

              const meta = formatTxMeta(tx.createdAt, tx.direction);

              return (
                <TxItem
                  key={tx.id ?? String(idx)}
                  title={title}
                  amount={amountText}
                  meta={meta}
                  titleTone={isCredit ? "positive" : "negative"}
                />
              );
            })
          )}

          <View style={{ height: 28 }} />
        </ScrollView>
      </LinearGradient>
    </View>
  );
}

function ActionTile({
  title,
  icon,
  onPress,
  disabled,
  variant,
}: {
  title: string;
  icon: React.ComponentProps<typeof Ionicons>["name"];
  onPress?: () => void;
  disabled?: boolean;
  variant: "teal" | "blue" | "purple" | "orange";
}) {
  const v = variantStyles[variant];

  return (
    <TouchableOpacity
      style={[styles.actionTile, disabled && styles.actionTileDisabled]}
      onPress={onPress}
      disabled={disabled}
      activeOpacity={0.85}
    >
      <View style={[styles.iconBubble, { backgroundColor: v.bubbleBg, borderColor: v.bubbleBorder }]}>
        <Ionicons name={icon} size={24} color={v.icon} />
      </View>

      <Text style={[styles.actionText, disabled && styles.disabledText]}>{title}</Text>
    </TouchableOpacity>
  );
}

const variantStyles = {
  teal: { bubbleBg: "rgba(45,183,163,0.14)", bubbleBorder: "rgba(45,183,163,0.22)", icon: "rgba(45,183,163,1)" },
  blue: { bubbleBg: "rgba(37,99,235,0.14)", bubbleBorder: "rgba(37,99,235,0.22)", icon: "rgba(37,99,235,1)" },
  purple: { bubbleBg: "rgba(109,40,217,0.14)", bubbleBorder: "rgba(109,40,217,0.22)", icon: "rgba(109,40,217,1)" },
  orange: { bubbleBg: "rgba(234,88,12,0.14)", bubbleBorder: "rgba(234,88,12,0.22)", icon: "rgba(234,88,12,1)" },
} as const;

function TxItem({
  title,
  amount,
  meta,
  titleTone,
}: {
  title: string;
  amount: string;
  meta: string;
  titleTone?: "positive" | "negative";
}) {
  const isPositive = amount.trim().startsWith("+");

  return (
    <View style={styles.txItem}>
      <View style={{ gap: 4 }}>
        <Text
          style={[
            styles.txTitle,
            titleTone === "positive" ? styles.txTitlePositive : null,
            titleTone === "negative" ? styles.txTitleNegative : null,
          ]}
        >
          {title}
        </Text>
        <Text style={styles.txMeta}>{meta}</Text>
      </View>

      <Text style={[styles.txAmount, isPositive ? styles.positive : styles.negative]}>{amount}</Text>
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
  container: { flex: 1, backgroundColor: Colors.bg },

  hero: {
    flex: 1,
    paddingHorizontal: 16,
    paddingTop: 14,
    overflow: "hidden",
  },

  goldGlow: {
    position: "absolute",
    top: -50,
    left: 0,
    right: 0,
    height: 280,
    borderBottomLeftRadius: 240,
    borderBottomRightRadius: 240,
    opacity: 0.9,
    transform: [{ scaleX: 1.2 }],
  },

  content: { paddingBottom: 28 },

  headerCard: {
    borderRadius: 18,
    padding: 16,
    marginBottom: 14,
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.10)",
    ...shadow,
  },
  headerRow: { flexDirection: "row", justifyContent: "space-between" },
  welcome: { color: "rgba(255,255,255,0.80)", fontWeight: "800" },
  name: { color: "rgba(255,255,255,0.96)", fontSize: 20, fontWeight: "900", marginTop: 2 },
  balanceLabel: { color: "rgba(255,255,255,0.72)", fontWeight: "800" },
  balance: { color: "rgba(255,255,255,0.96)", fontSize: 18, fontWeight: "900", marginTop: 2 },
  subHint: { marginTop: 10, color: "rgba(255,255,255,0.62)", fontSize: 12, fontWeight: "700" },

  actionGrid: { flexDirection: "row", flexWrap: "wrap", gap: 12, marginTop: 6, marginBottom: 6 },

  actionTile: {
    borderRadius: 16,
    paddingVertical: 16,
    paddingHorizontal: 14,
    width: "48%",
    alignItems: "center",
    justifyContent: "center",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.10)",
    backgroundColor: "rgba(255,255,255,0.06)",
    position: "relative",
    ...shadow,
  },
  actionTileDisabled: { opacity: 1 },

  iconBubble: {
    width: 46,
    height: 46,
    borderRadius: 23,
    alignItems: "center",
    justifyContent: "center",
    marginBottom: 10,
    borderWidth: 1,
  },
  actionText: { fontWeight: "900", fontSize: 13, color: "rgba(255,255,255,0.92)" },
  disabledText: { color: "rgba(255,255,255,0.92)" },

  sectionHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginTop: 14,
  },
  sectionTitle: { fontSize: 16, fontWeight: "900", color: "rgba(255,255,255,0.92)" },
  sectionLink: { color: "rgba(246,195,64,0.95)", fontWeight: "900" },

  txItem: {
    borderRadius: 14,
    padding: 14,
    marginTop: 10,
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.10)",
    backgroundColor: "rgba(255,255,255,0.06)",
    ...shadow,
  },
  txTitle: { fontWeight: "900", color: "rgba(255,255,255,0.92)" },
  // ✅ title rengi: Top Up yeşil, Transfer kırmızı
  txTitlePositive: { color: "rgba(46,213,115,0.95)" },
  txTitleNegative: { color: "rgba(255,71,87,0.95)" },

  txMeta: { color: "rgba(255,255,255,0.60)", fontSize: 12, fontWeight: "700" },

  txAmount: { fontWeight: "900", color: "rgba(255,255,255,0.92)" },
  positive: { color: "rgba(46,213,115,0.95)" },
  negative: { color: "rgba(255,71,87,0.95)" },
});
