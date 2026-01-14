import React, { useMemo, useState } from "react";
import { View, Text, StyleSheet, ScrollView, Pressable } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";

const BG = "#0B1220";
const CARD = "rgba(255,255,255,0.06)";
const BORDER = "rgba(255,255,255,0.10)";
const GOLD = "rgba(246,195,64,1)";
const MUTED = "rgba(255,255,255,0.60)";
const SOFT = "rgba(255,255,255,0.35)";

type CurrencyCode = "TRY" | "USD" | "EUR";
type FundingSourceType = "CARD" | "BANK";

type WalletMock = {
  currency: CurrencyCode;
  available: number;
  blocked: number;
  balances: Array<{ currency: CurrencyCode; available: number; blocked: number }>;
};

type FundingSourceMock = {
  id: string;
  type: FundingSourceType;
  title: string;
  subtitle: string;
  isPrimary?: boolean;
  status: "ACTIVE" | "PENDING" | "DISABLED";
};

type ActivityMock = {
  id: string;
  title: string;
  subtitle: string;
  amount: number; // + incoming, - outgoing
  currency: CurrencyCode;
  status: "COMPLETED" | "PENDING" | "FAILED";
  icon: React.ComponentProps<typeof Ionicons>["name"];
};

type Tone = "good" | "warn" | "bad";

function formatMoney(amount: number, currency: CurrencyCode) {
  const sign = amount < 0 ? "-" : "";
  const abs = Math.abs(amount);

  const symbol = currency === "TRY" ? "₺" : currency === "USD" ? "$" : "€";
  return `${sign}${symbol}${abs.toFixed(2)}`;
}

function toTone(status: "ACTIVE" | "PENDING" | "DISABLED" | "COMPLETED" | "FAILED"): Tone {
  if (status === "ACTIVE" || status === "COMPLETED") return "good";
  if (status === "PENDING") return "warn";
  return "bad";
}

function statusPillStyle(tone: Tone) {
  const borderColor =
    tone === "good"
      ? "rgba(46,213,115,0.30)"
      : tone === "warn"
      ? "rgba(246,195,64,0.25)"
      : "rgba(255,71,87,0.25)";

  const backgroundColor =
    tone === "good"
      ? "rgba(46,213,115,0.10)"
      : tone === "warn"
      ? "rgba(246,195,64,0.10)"
      : "rgba(255,71,87,0.10)";

  return {
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderRadius: 999,
    borderWidth: 1,
    borderColor,
    backgroundColor,
  } as const;
}

function statusPillTextStyle() {
  return {
    color: "rgba(255,255,255,0.85)",
    fontWeight: "900",
    fontSize: 11,
    letterSpacing: 0.2,
  } as const;
}

export default function WalletScreen() {
  const router = useRouter();
  const [hideAmounts, setHideAmounts] = useState(false);

  const wallet: WalletMock = useMemo(
    () => ({
      currency: "TRY",
      available: 2450.75,
      blocked: 120.0,
      balances: [
        { currency: "TRY", available: 2450.75, blocked: 120.0 },
        { currency: "USD", available: 80.5, blocked: 0 },
        { currency: "EUR", available: 12.0, blocked: 0 },
      ],
    }),
    []
  );

  const fundingSources: FundingSourceMock[] = useMemo(
    () => [
      {
        id: "fs_1",
        type: "CARD",
        title: "Primary Card",
        subtitle: "**** 2451 • Visa",
        isPrimary: true,
        status: "ACTIVE",
      },
      {
        id: "fs_2",
        type: "BANK",
        title: "Bank Account",
        subtitle: "TR** **** **** 6800",
        status: "PENDING",
      },
    ],
    []
  );

  const activity: ActivityMock[] = useMemo(
    () => [
      {
        id: "a_1",
        title: "Top up",
        subtitle: "Card • Primary Card",
        amount: +500,
        currency: "TRY",
        status: "COMPLETED",
        icon: "add-circle-outline",
      },
      {
        id: "a_2",
        title: "Transfer to Mehmet",
        subtitle: "Wallet to Wallet",
        amount: -120,
        currency: "TRY",
        status: "COMPLETED",
        icon: "swap-horizontal-outline",
      },
      {
        id: "a_3",
        title: "Transfer to Ali",
        subtitle: "Wallet to Wallet",
        amount: -75,
        currency: "TRY",
        status: "PENDING",
        icon: "time-outline",
      },
    ],
    []
  );

  const total = wallet.available + wallet.blocked;
  const masked = (value: string) => (hideAmounts ? "••••" : value);

  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.container} showsVerticalScrollIndicator={false}>
      <View style={styles.headerRow}>
        <View>
          <Text style={styles.title}>Wallet</Text>
          <Text style={styles.sub}>Balances, sources & activity (mock)</Text>
        </View>

        <Pressable
          onPress={() => setHideAmounts((p) => !p)}
          style={({ pressed }) => [styles.eyeBtn, pressed && { opacity: 0.9 }]}
        >
          <Ionicons name={hideAmounts ? "eye-off-outline" : "eye-outline"} size={18} color="rgba(255,255,255,0.85)" />
        </Pressable>
      </View>

      {/* HERO BALANCE */}
      <View style={[styles.card, styles.heroCard]}>
        <View style={styles.heroTop}>
          <View style={styles.heroBadge}>
            <Ionicons name="shield-checkmark-outline" size={14} color={GOLD} />
            <Text style={styles.heroBadgeText}>Secure Wallet</Text>
          </View>

          <Pressable
            onPress={() => router.push("/(tabs)/account")}
            style={({ pressed }) => [styles.miniLink, pressed && { opacity: 0.9 }]}
          >
            <Text style={styles.miniLinkText}>Details</Text>
            <Ionicons name="chevron-forward" size={14} color={SOFT} />
          </Pressable>
        </View>

        <Text style={styles.heroLabel}>Total balance</Text>
        <Text style={styles.heroValue}>{masked(formatMoney(total, wallet.currency))}</Text>

        <View style={styles.heroStatsRow}>
          <View style={styles.statPill}>
            <Text style={styles.statK}>Available</Text>
            <Text style={styles.statV}>{masked(formatMoney(wallet.available, wallet.currency))}</Text>
          </View>

          <View style={styles.statPill}>
            <Text style={styles.statK}>Blocked</Text>
            <Text style={styles.statV}>{masked(formatMoney(wallet.blocked, wallet.currency))}</Text>
          </View>
        </View>

        <View style={styles.divider} />

        <Text style={styles.cardTitle}>Currency breakdown</Text>
        <Text style={styles.cardHint}>Mock multi-currency view (MVP-ready)</Text>

        {wallet.balances.map((b) => {
          const t = b.available + b.blocked;
          return (
            <View key={b.currency} style={styles.breakRow}>
              <View style={styles.currencyChip}>
                <Text style={styles.currencyChipText}>{b.currency}</Text>
              </View>

              <View style={{ flex: 1 }}>
                <Text style={styles.breakMain}>{masked(formatMoney(t, b.currency))}</Text>
                <Text style={styles.breakSub}>
                  {hideAmounts
                    ? "••••"
                    : `Avail ${formatMoney(b.available, b.currency)} • Blocked ${formatMoney(b.blocked, b.currency)}`}
                </Text>
              </View>
            </View>
          );
        })}
      </View>

      {/* QUICK ACTIONS */}
      <View style={styles.card}>
        <Text style={styles.cardTitle}>Quick actions</Text>
        <Text style={styles.cardHint}>Instant actions for daily usage</Text>

        <Pressable
          onPress={() => router.push("/(tabs)/topup")}
          style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}
        >
          <View style={styles.iconWrap}>
            <Ionicons name="wallet-outline" size={18} color={GOLD} />
          </View>
          <View style={{ flex: 1 }}>
            <Text style={styles.itemTitle}>Top up</Text>
            <Text style={styles.itemSub}>Add money to your wallet</Text>
          </View>
          <Ionicons name="chevron-forward" size={16} color={SOFT} />
        </Pressable>

        <Pressable
          onPress={() => router.push("/(tabs)/transfer")}
          style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}
        >
          <View style={styles.iconWrap}>
            <Ionicons name="swap-horizontal-outline" size={18} color={GOLD} />
          </View>
          <View style={{ flex: 1 }}>
            <Text style={styles.itemTitle}>Transfer</Text>
            <Text style={styles.itemSub}>Send money instantly</Text>
          </View>
          <Ionicons name="chevron-forward" size={16} color={SOFT} />
        </Pressable>
      </View>

      {/* FUNDING SOURCES */}
      <View style={styles.card}>
        <View style={styles.sectionHeaderRow}>
          <View>
            <Text style={styles.cardTitle}>Funding sources</Text>
            <Text style={styles.cardHint}>Cards & bank accounts (mock)</Text>
          </View>

          <Pressable
            onPress={() => router.push("/(tabs)/security")}
            style={({ pressed }) => [styles.miniLink, pressed && { opacity: 0.9 }]}
          >
            <Text style={styles.miniLinkText}>Manage</Text>
            <Ionicons name="chevron-forward" size={14} color={SOFT} />
          </Pressable>
        </View>

        {fundingSources.map((fs) => {
          const icon = fs.type === "CARD" ? "card-outline" : "business-outline";
          const tone = toTone(fs.status);
          return (
            <View key={fs.id} style={styles.sourceRow}>
              <View style={styles.sourceIcon}>
                <Ionicons name={icon} size={18} color={GOLD} />
              </View>

              <View style={{ flex: 1 }}>
                <View style={styles.sourceTop}>
                  <Text style={styles.sourceTitle}>{fs.title}</Text>

                  {fs.isPrimary ? (
                    <View style={styles.primaryPill}>
                      <Ionicons name="star" size={12} color={GOLD} />
                      <Text style={styles.primaryPillText}>Primary</Text>
                    </View>
                  ) : (
                    <View style={statusPillStyle(tone)}>
                      <Text style={statusPillTextStyle()}>{fs.status}</Text>
                    </View>
                  )}
                </View>

                <Text style={styles.sourceSub}>{fs.subtitle}</Text>
              </View>

              <Ionicons name="chevron-forward" size={16} color={SOFT} />
            </View>
          );
        })}
      </View>

      {/* RECENT ACTIVITY */}
      <View style={styles.card}>
        <View style={styles.sectionHeaderRow}>
          <View>
            <Text style={styles.cardTitle}>Recent activity</Text>
            <Text style={styles.cardHint}>Last wallet movements (mock)</Text>
          </View>

          <View style={styles.smallPill}>
            <Ionicons name="time-outline" size={12} color="rgba(255,255,255,0.75)" />
            <Text style={styles.smallPillText}>Live later</Text>
          </View>
        </View>

        {activity.map((a) => {
          const tone = toTone(a.status);
          const isOut = a.amount < 0;

          return (
            <View key={a.id} style={styles.activityRow}>
              <View style={styles.activityIcon}>
                <Ionicons name={a.icon} size={18} color={isOut ? "rgba(255,255,255,0.85)" : GOLD} />
              </View>

              <View style={{ flex: 1 }}>
                <View style={styles.activityTop}>
                  <Text style={styles.activityTitle}>{a.title}</Text>
                  <View style={statusPillStyle(tone)}>
                    <Text style={statusPillTextStyle()}>{a.status}</Text>
                  </View>
                </View>
                <Text style={styles.activitySub}>{a.subtitle}</Text>
              </View>

              <Text style={[styles.activityAmount, isOut ? styles.outAmount : styles.inAmount]}>
                {hideAmounts ? "••••" : formatMoney(a.amount, a.currency)}
              </Text>
            </View>
          );
        })}
      </View>

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

  eyeBtn: {
    width: 38,
    height: 38,
    borderRadius: 14,
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: BORDER,
    alignItems: "center",
    justifyContent: "center",
  },

  card: {
    marginTop: 14,
    borderRadius: 18,
    padding: 14,
    backgroundColor: CARD,
    borderWidth: 1,
    borderColor: BORDER,
  },

  heroCard: { padding: 16 },
  heroTop: { flexDirection: "row", alignItems: "center", justifyContent: "space-between" },

  heroBadge: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderRadius: 999,
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
  },
  heroBadgeText: { marginLeft: 6, color: "rgba(255,255,255,0.80)", fontWeight: "900", fontSize: 11.5 },

  heroLabel: { marginTop: 14, color: MUTED, fontSize: 12, fontWeight: "800" },
  heroValue: { marginTop: 6, color: "rgba(255,255,255,0.92)", fontSize: 30, fontWeight: "900" },

  heroStatsRow: { flexDirection: "row", marginTop: 12 },
  statPill: {
    flex: 1,
    borderRadius: 16,
    paddingVertical: 10,
    paddingHorizontal: 12,
    backgroundColor: "rgba(0,0,0,0.18)",
    borderWidth: 1,
    borderColor: BORDER,
    marginRight: 10,
  },
  statK: { color: MUTED, fontWeight: "800", fontSize: 11.5 },
  statV: { marginTop: 6, color: GOLD, fontWeight: "900", fontSize: 14.5 },

  divider: { height: 1, backgroundColor: BORDER, marginTop: 14, marginBottom: 12 },

  cardTitle: { color: "rgba(255,255,255,0.92)", fontSize: 14, fontWeight: "900" },
  cardHint: { marginTop: 6, color: "rgba(255,255,255,0.55)", fontSize: 11.5, fontWeight: "700", lineHeight: 16 },

  breakRow: { flexDirection: "row", alignItems: "center", paddingVertical: 10 },
  currencyChip: {
    width: 46,
    height: 34,
    borderRadius: 14,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: BORDER,
    marginRight: 10,
  },
  currencyChipText: { color: "rgba(255,255,255,0.85)", fontWeight: "900", fontSize: 12 },
  breakMain: { color: "rgba(255,255,255,0.90)", fontWeight: "900", fontSize: 13.5 },
  breakSub: { marginTop: 3, color: MUTED, fontWeight: "700", fontSize: 11.5 },

  item: { flexDirection: "row", alignItems: "center", paddingVertical: 12, paddingHorizontal: 6, borderRadius: 14 },
  iconWrap: {
    width: 34,
    height: 34,
    borderRadius: 14,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
    marginRight: 10,
  },
  itemTitle: { color: "rgba(255,255,255,0.92)", fontSize: 13, fontWeight: "900" },
  itemSub: { marginTop: 3, color: "rgba(255,255,255,0.55)", fontSize: 11.5, fontWeight: "700" },

  sectionHeaderRow: { flexDirection: "row", alignItems: "center", justifyContent: "space-between" },

  miniLink: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderRadius: 999,
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: BORDER,
  },
  miniLinkText: { marginRight: 4, color: "rgba(255,255,255,0.80)", fontWeight: "900", fontSize: 11.5 },

  sourceRow: {
    marginTop: 12,
    flexDirection: "row",
    alignItems: "center",
    padding: 12,
    borderRadius: 16,
    backgroundColor: "rgba(0,0,0,0.18)",
    borderWidth: 1,
    borderColor: BORDER,
  },
  sourceIcon: {
    width: 36,
    height: 36,
    borderRadius: 14,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
    marginRight: 10,
  },
  sourceTop: { flexDirection: "row", alignItems: "center", justifyContent: "space-between" },
  sourceTitle: { color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 13 },
  sourceSub: { marginTop: 4, color: MUTED, fontWeight: "700", fontSize: 11.5 },

  primaryPill: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderRadius: 999,
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
  },
  primaryPillText: { marginLeft: 5, color: "rgba(255,255,255,0.85)", fontWeight: "900", fontSize: 11.5 },

  smallPill: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderRadius: 999,
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: BORDER,
  },
  smallPillText: { marginLeft: 6, color: "rgba(255,255,255,0.78)", fontWeight: "900", fontSize: 11.5 },

  activityRow: { marginTop: 12, flexDirection: "row", alignItems: "center", paddingVertical: 6 },
  activityIcon: {
    width: 36,
    height: 36,
    borderRadius: 14,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "rgba(0,0,0,0.18)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.18)",
    marginRight: 10,
  },
  activityTop: { flexDirection: "row", alignItems: "center", justifyContent: "space-between" },
  activityTitle: { color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 13 },
  activitySub: { marginTop: 4, color: MUTED, fontWeight: "700", fontSize: 11.5 },

  activityAmount: { fontWeight: "900", fontSize: 13.5, marginLeft: 10 },
  inAmount: { color: GOLD },
  outAmount: { color: "rgba(255,255,255,0.86)" },
});
