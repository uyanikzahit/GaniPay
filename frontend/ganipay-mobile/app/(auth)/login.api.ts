// app/(auth)/login.api.ts
import { Platform } from "react-native";
import { saveSession } from "@/constants/storage";

const WEB_BASE_URL = "https://localhost:7253";

// ✅ Mobile (HTTP) — Aspire üzerinden açtığın port
const REAL_DEVICE_BASE_URL = "http://192.168.1.5:5210";
const ANDROID_EMU_BASE_URL = "http://10.0.2.2:5210"; // Android emulator

export function getBaseUrl() {
  if (Platform.OS === "web") return WEB_BASE_URL;
  if (Platform.OS === "android") return ANDROID_EMU_BASE_URL;
  return REAL_DEVICE_BASE_URL; // ios + real device
}

export type LoginPayload = {
  phoneNumber: string;
  password: string;
  ipAddress?: string;
  deviceId?: string;
  channel?: "WEB" | "MOBILE";
  clientVersion?: string;
};

export type LoginApiResponse = {
  success: boolean;
  status: "Succeeded" | "Running" | "Failed" | string;
  message?: string;
  token?: string;
  correlationId?: string;

  // backend’den gelenler
  customerId?: string;
  customer?: {
    firstName?: string;
    lastName?: string;
    [key: string]: any;
  };
  wallets?: {
    accounts?: Array<{
      accountId: string;
      balance: number;
      currency: string;
    }>;
  };

  [key: string]: any;
};

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

async function fetchWithTimeout(
  url: string,
  options: RequestInit,
  timeoutMs = 60000
) {
  const controller = new AbortController();
  const id = setTimeout(() => controller.abort(), timeoutMs);

  try {
    return await fetch(url, { ...options, signal: controller.signal });
  } finally {
    clearTimeout(id);
  }
}

async function postJson<T>(url: string, body: any): Promise<T> {
  const res = await fetchWithTimeout(
    url,
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
    15000
  );

  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(text || `Request failed (${res.status})`);
  }

  const data = (await readJsonSafe(res)) as T | null;
  if (!data) throw new Error("Empty response");
  return data;
}

// ✅ login sonucu alınana kadar bekleyen helper (polling)
async function pollLoginResult(baseUrl: string, correlationId: string) {
  const maxAttempts = 30;
  const delayMs = 1000;

  for (let i = 0; i < maxAttempts; i++) {
    const url = `${baseUrl}/api/v1/auth/login/result/${encodeURIComponent(
      correlationId
    )}`;

    const res = await fetch(url, { method: "GET" });

    if (res.ok) {
      const data = (await readJsonSafe(res)) as LoginApiResponse | null;
      return data ?? { success: false, status: "Failed" };
    }

    if (res.status === 404) {
      await sleep(delayMs);
      continue;
    }

    const text = await res.text().catch(() => "");
    return {
      success: false,
      status: "Failed",
      message: text,
    };
  }

  return {
    success: false,
    status: "Running",
    message: "Login is still being processed.",
  };
}

// ✅ ASIL LOGIN
export async function login(
  payload: LoginPayload
): Promise<LoginApiResponse> {
  const baseUrl = getBaseUrl();
  const url = `${baseUrl}/api/v1/auth/login`;

  const first = await postJson<LoginApiResponse>(url, payload);

  // 1️⃣ Direkt succeeded
  if (first.success && first.status === "Succeeded" && first.token) {
    await saveLoginSession(first);
    return first;
  }

  // 2️⃣ Running → poll
  if (first.status === "Running" && first.correlationId) {
    const result = await pollLoginResult(baseUrl, first.correlationId);

    if (result.success && result.status === "Succeeded" && result.token) {
      await saveLoginSession(result);
    }

    return result;
  }

  return first;
}

// 🔐 Session kaydetme helper
async function saveLoginSession(res: LoginApiResponse) {
  const account = res.wallets?.accounts?.[0];

  await saveSession({
    accessToken: res.token!,
    user: res.customer,
    customerId: res.customerId,
    accountId: account?.accountId,
    currency: account?.currency,
  });
}