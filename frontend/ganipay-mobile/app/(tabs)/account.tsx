// app/(tabs)/account.tsx
import React, { useEffect, useMemo, useState } from "react";
import { View, Text, StyleSheet, ScrollView, Pressable } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";
import { clearSession, loadSession, StoredSession } from "@/constants/storage";

const BG = "#0B1220";
const CARD = "rgba(255,255,255,0.06)";
const BORDER = "rgba(255,255,255,0.10)";
const GOLD = "rgba(246,195,64,1)";
const MUTED = "rgba(255,255,255,0.60)";
const SOFT = "rgba(255,255,255,0.35)";

function safeStr(v: any): string | null {
  if (v === undefined || v === null) return null;
  const s = String(v).trim();
  return s.length ? s : null;
}

// ✅ ID/Identity mask: 12345678901 -> 1234•••••8901
function maskIdentity(input?: string | null, keepStart = 4, keepEnd = 4) {
  const s = safeStr(input);
  if (!s) return "—";
  const len = s.length;
  if (len <= keepStart + keepEnd) return s;
  const start = s.slice(0, keepStart);
  const end = s.slice(-keepEnd);
  const dots = "•".repeat(Math.max(4, len - keepStart - keepEnd));
  return `${start}${dots}${end}`;
}

function buildFullName(customer: any) {
  const first = safeStr(customer?.firstName ?? customer?.name);
  const last = safeStr(customer?.lastName ?? customer?.surname);
  const full = `${first ?? ""} ${last ?? ""}`.trim();
  return full || "—";
}

function buildEmail(customer: any) {
  return (
    safeStr(customer?.email) ??
    safeStr(customer?.primaryEmail) ??
    safeStr(Array.isArray(customer?.emails) ? customer?.emails?.[0]?.value : null) ??
    null
  );
}

function buildPhone(customer: any) {
  // Telefonu AÇIK istedin → direkt gösteriyoruz
  return (
    safeStr(customer?.phoneNumber) ??
    safeStr(customer?.phone) ??
    safeStr(customer?.gsm) ??
    safeStr(Array.isArray(customer?.phones) ? customer?.phones?.[0]?.value : null) ??
    null
  );
}

function buildAddress(customer: any) {
  const addr =
    customer?.address ??
    customer?.primaryAddress ??
    (Array.isArray(customer?.addresses) ? customer?.addresses?.[0] : null);

  if (!addr) return null;
  if (typeof addr === "string") return safeStr(addr);

  const line1 = safeStr(addr?.addressLine1 ?? addr?.line1 ?? addr?.addressLine);
  const district = safeStr(addr?.district ?? addr?.town);
  const city = safeStr(addr?.city);
  const postal = safeStr(addr?.postalCode ?? addr?.zipCode);

  const parts = [line1, district, city].filter(Boolean) as string[];
  const s = parts.join(", ");
  if (!s) return null;
  return postal ? `${s} ${postal}` : s;
}

function buildTier(customer: any) {
  return safeStr(customer?.segment ?? customer?.tier) ?? "Standard";
}

function buildIdentity(customer: any, session: StoredSession | null) {
  // backend’de identityNumber / nationalId gibi alanlar olabilir
  return (
    safeStr(customer?.identityNumber) ??
    safeStr(customer?.nationalId) ??
    safeStr(customer?.tckn) ??
    safeStr(session?.customerId) ?? // en garanti olan
    null
  );
}

export default function AccountScreen() {
  const router = useRouter();
  const [session, setSession] = useState<StoredSession | null>(null);

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

  const vm = useMemo(() => {
    const customer = (session?.customer ?? session?.user ?? {}) as any;

    const fullName = buildFullName(customer);
    const email = buildEmail(customer);
    const phone = buildPhone(customer);
    const address = buildAddress(customer);
    const tier = buildTier(customer);

    const identityRaw = buildIdentity(customer, session);
    const identityMasked = maskIdentity(identityRaw, 4, 4);

    // currency: wallet ekranına koyacağız ama account header’da küçük bilgi olarak göstermek istersen
    const currency = safeStr(session?.currency ?? customer?.currency);

    return {
      fullName,
      email: email ?? "—",
      phone: phone ?? "—",
      address: address ?? "—",
      tier,
      currency,
      identityMasked,
    };
  }, [session]);

  // ✅ Layout’taki gibi: direkt logout
  const onLogout = async () => {
    try {
      await clearSession();
    } finally {
      router.replace("/(auth)/login");
    }
  };

  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.container} showsVerticalScrollIndicator={false}>
      <Text style={styles.title}>Account</Text>
      <Text style={styles.sub}>Profile & settings</Text>

      {/* ✅ Profile card */}
      <View style={styles.card}>
        <View style={styles.profileRow}>
          <View style={styles.avatar}>
            <Ionicons name="person-outline" size={22} color={GOLD} />
          </View>

          <View style={{ flex: 1 }}>
            <Text style={styles.profileName}>{vm.fullName}</Text>
            <Text style={styles.profileSub}>
              {vm.tier} • Verified{vm.currency ? ` • ${vm.currency}` : ""}
            </Text>
          </View>

          <Pressable
            onPress={() => {}}
            style={({ pressed }) => [styles.miniBtn, pressed && { opacity: 0.9 }]}
          >
            <Ionicons name="create-outline" size={16} color={GOLD} />
            <Text style={styles.miniBtnText}>Edit</Text>
          </Pressable>
        </View>

        {/* ✅ Phone (AÇIK) */}
        <View style={styles.infoRow}>
          <Ionicons name="call-outline" size={16} color={SOFT} />
          <Text style={styles.infoText}>{vm.phone}</Text>
        </View>

        {/* ✅ Email */}
        <View style={styles.infoRow}>
          <Ionicons name="mail-outline" size={16} color={SOFT} />
          <Text style={styles.infoText}>{vm.email}</Text>
        </View>

        {/* ✅ Address */}
        <View style={styles.infoRow}>
          <Ionicons name="location-outline" size={16} color={SOFT} />
          <Text style={styles.infoText} numberOfLines={2}>
            {vm.address}
          </Text>
        </View>

        {/* ✅ Identity (MASK’Lİ) */}
        <View style={styles.infoRow}>
          <Ionicons name="shield-outline" size={16} color={SOFT} />
          <Text style={styles.infoText}>{vm.identityMasked}</Text>
        </View>
      </View>

      {/* ✅ Preferences */}
      <View style={styles.card}>
        <Text style={styles.cardTitle}>Preferences</Text>
        <Text style={styles.cardHint}>Manage your authentication, limits and legal settings.</Text>

        <Pressable
          onPress={() => router.push("/(tabs)/security")}
          style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}
        >
          <View style={styles.iconWrap}>
            <Ionicons name="shield-checkmark-outline" size={18} color={GOLD} />
          </View>
          <View style={{ flex: 1 }}>
            <Text style={styles.itemTitle}>Security</Text>
            <Text style={styles.itemSub}>Authentication and safeguards</Text>
          </View>
          <Ionicons name="chevron-forward" size={16} color={SOFT} />
        </Pressable>

        <Pressable
          onPress={() => router.push("/(tabs)/limits")}
          style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}
        >
          <View style={styles.iconWrap}>
            <Ionicons name="speedometer-outline" size={18} color={GOLD} />
          </View>
          <View style={{ flex: 1 }}>
            <Text style={styles.itemTitle}>Spending limits</Text>
            <Text style={styles.itemSub}>Daily and monthly caps</Text>
          </View>
          <Ionicons name="chevron-forward" size={16} color={SOFT} />
        </Pressable>

        <Pressable
          onPress={() => router.push("/(tabs)/legal")}
          style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}
        >
          <View style={styles.iconWrap}>
            <Ionicons name="document-text-outline" size={18} color={GOLD} />
          </View>
          <View style={{ flex: 1 }}>
            <Text style={styles.itemTitle}>Legal</Text>
            <Text style={styles.itemSub}>Terms and privacy policy</Text>
          </View>
          <Ionicons name="chevron-forward" size={16} color={SOFT} />
        </Pressable>
      </View>

      {/* ✅ Logout */}
      <View style={styles.card}>
        <Pressable onPress={onLogout} style={({ pressed }) => [styles.dangerBtn, pressed && { opacity: 0.9 }]}>
          <Ionicons name="log-out-outline" size={18} color="rgba(255,255,255,0.92)" />
          <Text style={styles.dangerText}>Log out</Text>
        </Pressable>
      </View>

      <View style={{ height: 26 }} />
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  page: { flex: 1, backgroundColor: BG },
  container: { padding: 16, paddingBottom: 28 },

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

  profileRow: { flexDirection: "row", alignItems: "center" },
  avatar: {
    width: 42,
    height: 42,
    borderRadius: 18,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
    marginRight: 10,
  },
  profileName: { color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 14 },
  profileSub: { marginTop: 4, color: MUTED, fontWeight: "700", fontSize: 11.5 },

  miniBtn: {
    flexDirection: "row",
    alignItems: "center",
    paddingHorizontal: 10,
    paddingVertical: 6,
    borderRadius: 999,
    backgroundColor: "rgba(246,195,64,0.10)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
  },
  miniBtnText: { marginLeft: 6, color: "rgba(255,255,255,0.86)", fontWeight: "900", fontSize: 11.5 },

  infoRow: { marginTop: 12, flexDirection: "row", alignItems: "center" },
  infoText: { marginLeft: 10, color: "rgba(255,255,255,0.86)", fontWeight: "800", flex: 1 },

  cardTitle: { color: "rgba(255,255,255,0.92)", fontSize: 14, fontWeight: "900" },
  cardHint: { marginTop: 6, color: "rgba(255,255,255,0.55)", fontSize: 11.5, fontWeight: "700", lineHeight: 16 },

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

  dangerBtn: {
    borderRadius: 16,
    paddingVertical: 12,
    paddingHorizontal: 14,
    backgroundColor: "rgba(255,71,87,0.16)",
    borderWidth: 1,
    borderColor: "rgba(255,71,87,0.26)",
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
  },
  dangerText: { marginLeft: 8, color: "rgba(255,255,255,0.92)", fontWeight: "900", fontSize: 13.5 },
});
