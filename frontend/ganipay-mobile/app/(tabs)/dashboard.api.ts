// app/(tabs)/dashboard.api.ts
import { Platform } from "react-native";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { SessionKeys } from "../../constants/storage";

// ✅ Artık Accounting'e direkt portla değil, APISIX üzerinden gidiyoruz.
const WEB_GATEWAY_BASE_URL = "http://localhost:9080";
const ANDROID_EMU_GATEWAY_BASE_URL = "http://10.0.2.2:9080";
const REAL_DEVICE_GATEWAY_BASE_URL = "http://192.168.1.5:9080"; // PC IP + 9080

function getGatewayBaseUrl() {
  if (Platform.OS === "web") return WEB_GATEWAY_BASE_URL;
  if (Platform.OS === "android") return ANDROID_EMU_GATEWAY_BASE_URL;
  return REAL_DEVICE_GATEWAY_BASE_URL;
}

async function authHeaders() {
  const token = await AsyncStorage.getItem(SessionKeys.accessToken);
  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

async function readJsonSafe(res: Response) {
  try {
    return await res.json();
  } catch {
    return null;
  }
}

export type BalanceResponse = {
  accountId: string;
  customerId: string;
  currency: string;
  balance: number;
};

export type BalanceHistoryItem = {
  id: string;
  accountId: string;
  direction: "credit" | "debit" | string;
  changeAmount: number;
  balanceBefore: number;
  balanceAfter: number;
  currency: string;
  operationType?: string;
  referenceId?: string;
  createdAt: string;
};

export async function getCustomerBalance(customerId: string, currency = "TRY") {
  const baseUrl = getGatewayBaseUrl();
  // ✅ APISIX route prefix: /accounting-api
  const url =
    `${baseUrl}/accounting-api/api/accounting/customers/${encodeURIComponent(customerId)}` +
    `/balance?currency=${encodeURIComponent(currency)}`;

  const res = await fetch(url, { method: "GET", headers: await authHeaders() });
  if (!res.ok) throw new Error(await res.text().catch(() => `Balance failed (${res.status})`));

  const data = (await readJsonSafe(res)) as BalanceResponse | null;
  if (!data) throw new Error("Empty balance response");
  return data;
}

export async function getAccountBalanceHistory(accountId: string) {
  const baseUrl = getGatewayBaseUrl();
  const url = `${baseUrl}/accounting-api/api/accounting/accounts/${encodeURIComponent(accountId)}/balance-history`;

  const res = await fetch(url, { method: "GET", headers: await authHeaders() });
  if (!res.ok) throw new Error(await res.text().catch(() => `History failed (${res.status})`));

  const data = (await readJsonSafe(res)) as BalanceHistoryItem[] | null;
  return data ?? [];
}
