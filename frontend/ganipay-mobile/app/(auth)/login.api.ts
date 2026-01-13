// app/(auth)/login.api.ts
import { Platform } from "react-native";

const WEB_BASE_URL = "https://localhost:7253";
const ANDROID_EMU_BASE_URL = "https://10.0.2.2:7253";
const IOS_SIM_BASE_URL = "https://localhost:7253";
const REAL_DEVICE_BASE_URL = "https://YOUR_PC_IP:7253";

export function getBaseUrl() {
  if (Platform.OS === "web") return WEB_BASE_URL;
  if (Platform.OS === "android") return ANDROID_EMU_BASE_URL;
  if (Platform.OS === "ios") return IOS_SIM_BASE_URL;
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
};

async function readJsonSafe(res: Response) {
  try {
    return (await res.json()) as any;
  } catch {
    return null;
  }
}

export async function login(payload: LoginPayload): Promise<LoginApiResponse> {
  const url = `${getBaseUrl()}/api/v1/auth/login`;

  const res = await fetch(url, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  // ✅ 4xx/5xx ise direkt patlat
  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(text || `Login failed (${res.status})`);
  }

  const data = (await readJsonSafe(res)) as LoginApiResponse | null;
  if (!data) throw new Error("Login failed: empty response");

  // ✅ 202 bile olsa success:false ise UI’da hata göstereceğiz
 return data;
}
