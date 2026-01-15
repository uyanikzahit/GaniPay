// app/(auth)/login.api.ts
import { Platform } from "react-native";

const WEB_BASE_URL = "https://localhost:7253";

// ✅ Mobile (HTTP) — Aspire üzerinden açtığın port
const REAL_DEVICE_BASE_URL = "http://192.168.1.5:5210";
const ANDROID_EMU_BASE_URL = "http://10.0.2.2:5210"; // ✅ Android emulator => PC localhost

export function getBaseUrl() {
  if (Platform.OS === "web") return WEB_BASE_URL;
  if (Platform.OS === "android") return ANDROID_EMU_BASE_URL;
  // ios + diğer cihazlar
  return REAL_DEVICE_BASE_URL;
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

// ✅ Timeout’lu fetch (RN’de çok kritik)
async function fetchWithTimeout(url: string, options: RequestInit, timeoutMs = 60000) {
  const controller = new AbortController();
  const id = setTimeout(() => controller.abort(), timeoutMs);

  try {
    const res = await fetch(url, { ...options, signal: controller.signal });
    return res;
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
  const url = `${baseUrl}/api/v1/auth/login/result`;

  const maxAttempts = 12; // ✅ 12 deneme
  const delayMs = 700;    // ✅ daha hızlı
  for (let i = 0; i < maxAttempts; i++) {
    try {
      const data = await postJson<LoginApiResponse>(url, { correlationId });

      if (data.success === true && data.status === "Succeeded" && data.token) return data;
      if (data.status === "Failed" || data.success === false) return data;

      await sleep(delayMs);
    } catch (e) {
      // ✅ result endpoint’e anlık erişememe vs: devam et ama sonsuza kilitlenme
      await sleep(delayMs);
    }
  }

  return {
    success: false,
    status: "Running",
    message: "Login is still being processed. Please try again.",
    correlationId,
  } as LoginApiResponse;
}

export async function login(payload: LoginPayload): Promise<LoginApiResponse> {
  const baseUrl = getBaseUrl();
  const url = `${baseUrl}/api/v1/auth/login`;

  const first = await postJson<LoginApiResponse>(url, payload);

  // 1) Succeeded geldiyse direkt dön
  if (first.success === true && first.status === "Succeeded" && first.token) return first;

  // 2) Running geldiyse correlationId ile result poll et
  if (first.status === "Running" && first.correlationId) {
    return await pollLoginResult(baseUrl, first.correlationId);
  }

  // 3) Failed vs.
  return first;
}
