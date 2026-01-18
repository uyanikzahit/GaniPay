import { Platform } from "react-native";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { SessionKeys } from "../../constants/storage";

// Workflow API base URL (login.api.ts ile uyumlu)
const WEB_BASE_URL = "https://localhost:7253";
const REAL_DEVICE_BASE_URL = "http://192.168.1.5:5210";
const ANDROID_EMU_BASE_URL = "http://10.0.2.2:5210";

function getBaseUrl() {
  if (Platform.OS === "web") return WEB_BASE_URL;
  if (Platform.OS === "android") return ANDROID_EMU_BASE_URL;
  return REAL_DEVICE_BASE_URL;
}

export type TopUpPayload = {
  customerId: string;
  accountId: string;
  amount: number;
  currency: "TRY";
  idempotencyKey: string;
  referenceId: string;
};

export type TopUpApiResponse = {
  success?: boolean;
  status?: string;
  message?: string;
  correlationId?: string;
  [key: string]: any;
};

async function readJsonSafe(res: Response) {
  try {
    return await res.json();
  } catch {
    return null;
  }
}

async function authHeaders() {
  const token = await AsyncStorage.getItem(SessionKeys.accessToken);
  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

// ✅ SADECE BU ÇAĞRI
export async function startTopUp(payload: TopUpPayload): Promise<TopUpApiResponse> {
  const baseUrl = getBaseUrl();
  const url = `${baseUrl}/api/v1/payments/topup`;

  const res = await fetch(url, {
    method: "POST",
    headers: await authHeaders(),
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(text || `TopUp request failed (${res.status})`);
  }

  const data = (await readJsonSafe(res)) as TopUpApiResponse | null;
  return data ?? {};
}
