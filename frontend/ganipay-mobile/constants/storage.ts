import AsyncStorage from "@react-native-async-storage/async-storage";

export const SessionKeys = {
  accessToken: "ganipay.accessToken",
  refreshToken: "ganipay.refreshToken",
  user: "ganipay.user",
  customerId: "ganipay.customerId",
  accountId: "ganipay.accountId",
  currency: "ganipay.currency",

  // ✅ login response’tan gelen ek datalar
  customer: "ganipay.customer",
  wallets: "ganipay.wallets",
} as const;

export type StoredSession = {
  accessToken?: string | null;
  refreshToken?: string | null;
  customerId?: string | null;
  accountId?: string | null;
  currency?: string | null;

  // JSON saklananlar
  user?: any | null;
  customer?: any | null;
  wallets?: any | null;
};

export async function saveSession(data: StoredSession) {
  const pairs: [string, string][] = [];

  if (data.accessToken !== undefined)
    pairs.push([SessionKeys.accessToken, data.accessToken ?? ""]);

  if (data.refreshToken !== undefined)
    pairs.push([SessionKeys.refreshToken, data.refreshToken ?? ""]);

  if (data.customerId !== undefined)
    pairs.push([SessionKeys.customerId, data.customerId ?? ""]);

  if (data.accountId !== undefined)
    pairs.push([SessionKeys.accountId, data.accountId ?? ""]);

  if (data.currency !== undefined)
    pairs.push([SessionKeys.currency, data.currency ?? ""]);

  if (data.user !== undefined)
    pairs.push([SessionKeys.user, JSON.stringify(data.user ?? null)]);

  if (data.customer !== undefined)
    pairs.push([SessionKeys.customer, JSON.stringify(data.customer ?? null)]);

  if (data.wallets !== undefined)
    pairs.push([SessionKeys.wallets, JSON.stringify(data.wallets ?? null)]);

  if (pairs.length > 0) {
    await AsyncStorage.multiSet(pairs);
  }
}

export async function loadSession(): Promise<StoredSession> {
  const entries = await AsyncStorage.multiGet(Object.values(SessionKeys));
  const map = Object.fromEntries(entries);

  const safeJson = (val?: string | null) => {
    if (!val) return null;
    try {
      return JSON.parse(val);
    } catch {
      return null;
    }
  };

  return {
    accessToken: map[SessionKeys.accessToken] || null,
    refreshToken: map[SessionKeys.refreshToken] || null,
    customerId: map[SessionKeys.customerId] || null,
    accountId: map[SessionKeys.accountId] || null,
    currency: map[SessionKeys.currency] || null,
    user: safeJson(map[SessionKeys.user]),
    customer: safeJson(map[SessionKeys.customer]),
    wallets: safeJson(map[SessionKeys.wallets]),
  };
}

export async function clearSession() {
  await AsyncStorage.multiRemove(Object.values(SessionKeys));
}