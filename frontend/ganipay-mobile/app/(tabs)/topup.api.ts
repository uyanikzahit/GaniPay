// app/(tabs)/topup.api.ts
import { getBaseUrl } from "../(auth)/login.api";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { SessionKeys } from "../../constants/storage";

export type TopUpPayload = {
  customerId: string;
  accountId: string;
  amount: number;
  currency: string; // "TRY"
  idempotencyKey: string;
  referenceId: string;
};

export type TopUpApiResponse = {
  success: boolean;
  status?: "Succeeded" | "Running" | "Failed" | string;
  message?: string;

  correlationId?: string;

  // backend farklı alanlar dönebilir diye esnek bıraktım
  [key: string]: any;
};

// 🔧 Swagger’a göre gerekirse sadece burayı değiştir
const TOPUP_START_PATH = "/api/v1/topup";
const TOPUP_RESULT_PATH = (correlationId: string) =>
  `/api/v1/topup/result/${encodeURIComponent(correlationId)}`;

async function readJsonSafe(res: Response) {
  try {
    return (await res.json()) as any;
  } catch {
    return null;
  }
}

function sleep(ms: number) {
  return new Promise((r) => setTimeout(r, ms));
}

async function fetchWithTimeout(url: string, options: RequestInit, timeoutMs = 60000) {
  const controller = new AbortController();
  const id = setTimeout(() => controller.abort(), timeoutMs);

  try {
    return await fetch(url, { ...options, signal: controller.signal });
  } finally {
    clearTimeout(id);
  }
}

async function authHeaders() {
  const token = await AsyncStorage.getItem(SessionKeys.accessToken);
  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

async function postJson<T>(url: string, body: any): Promise<T> {
  const res = await fetchWithTimeout(
    url,
    {
      method: "POST",
      headers: await authHeaders(),
      body: JSON.stringify(body),
    },
    20000
  );

  const data = (await readJsonSafe(res)) as T | null;

  if (!res.ok) {
    const text = await res.text().catch(() => "");
    // mümkünse backend mesajını döndür
    const msg = (data as any)?.message || text || `Request failed (${res.status})`;
    throw new Error(msg);
  }

  if (!data) throw new Error("Empty response");
  return data;
}

async function pollTopUpResult(baseUrl: string, correlationId: string): Promise<TopUpApiResponse> {
  const maxAttempts = 45;
  const delayMs = 1000;

  for (let i = 0; i < maxAttempts; i++) {
    const url = `${baseUrl}${TOPUP_RESULT_PATH(correlationId)}`;
    const res = await fetch(url, { method: "GET", headers: await authHeaders() });

    if (res.ok) {
      const data = (await readJsonSafe(res)) as TopUpApiResponse | null;
      return data ?? { success: false, status: "Failed", message: "Empty response" };
    }

    // login’deki gibi 404 ise biraz bekle devam et
    if (res.status === 404) {
      await sleep(delayMs);
      continue;
    }

    const text = await res.text().catch(() => "");
    return { success: false, status: "Failed", message: text || `Result failed (${res.status})` };
  }

  return {
    success: false,
    status: "Running",
    message: "Top up is still being processed.",
  };
}

/**
 * ✅ TopUp akışını başlatır.
 * - Direct succeeded ise direkt döner
 * - Running ise correlationId ile result polling yapar
 */
export async function startTopUp(payload: TopUpPayload): Promise<TopUpApiResponse> {
  const baseUrl = getBaseUrl();
  const url = `${baseUrl}${TOPUP_START_PATH}`;

  const first = await postJson<TopUpApiResponse>(url, payload);

  // 1) Direkt succeeded
  if (first.success && first.status === "Succeeded") return first;

  // 2) Running → poll
  if ((first.status === "Running" || !first.success) && first.correlationId) {
    const result = await pollTopUpResult(baseUrl, first.correlationId);
    return result;
  }

  // 3) Her ihtimale karşı aynen döndür
  return first;
}
