// app/(tabs)/dashboard.api.ts
import { Platform } from "react-native";
import AsyncStorage from "@react-native-async-storage/async-storage";

// Eğer sende path alias varsa "@/constants/storage" kullanabilirsin.
// Yoksa index.tsx içinden "../../constants/storage" ile import edeceğiz.
import { SessionKeys } from "../../constants/storage";

const WEB_ACCOUNTING_BASE_URL = "http://localhost:5103";
// Mobile kısmını sonra açacağız (şimdilik web)
// const REAL_DEVICE_ACCOUNTING_BASE_URL = "http://192.168.1.5:5103";

function getAccountingBaseUrl() {
  if (Platform.OS === "web") return WEB_ACCOUNTING_BASE_URL;
  // şimdilik web dışı da localhost kalsın, sonra mobile IP'ye çeviririz
  return WEB_ACCOUNTING_BASE_URL;
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
  createdAt: string; // ISO
};

export async function getCustomerBalance(customerId: string, currency = "TRY") {
  const baseUrl = getAccountingBaseUrl();
  const url = `${baseUrl}/api/accounting/customers/${encodeURIComponent(customerId)}/balance?currency=${encodeURIComponent(
    currency
  )}`;

  const res = await fetch(url, { method: "GET", headers: await authHeaders() });
  if (!res.ok) throw new Error(await res.text().catch(() => `Balance failed (${res.status})`));

  const data = (await readJsonSafe(res)) as BalanceResponse | null;
  if (!data) throw new Error("Empty balance response");
  return data;
}

export async function getAccountBalanceHistory(accountId: string) {
  const baseUrl = getAccountingBaseUrl();
  const url = `${baseUrl}/api/accounting/accounts/${encodeURIComponent(accountId)}/balance-history`;

  const res = await fetch(url, { method: "GET", headers: await authHeaders() });
  if (!res.ok) throw new Error(await res.text().catch(() => `History failed (${res.status})`));

  const data = (await readJsonSafe(res)) as BalanceHistoryItem[] | null;
  return data ?? [];
}
