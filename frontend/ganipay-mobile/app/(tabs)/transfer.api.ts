import { Platform } from "react-native";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { SessionKeys } from "../../constants/storage";

const WEB_BASE_URL = "http://localhost:9080/workflow-api";
const REAL_DEVICE_BASE_URL = "http://192.168.1.5:9080/workflow-api";
const ANDROID_EMU_BASE_URL = "http://10.0.2.2:9080/workflow-api";

function getBaseUrl() {
  if (Platform.OS === "web") return WEB_BASE_URL;
  if (Platform.OS === "android") return ANDROID_EMU_BASE_URL;
  return REAL_DEVICE_BASE_URL;
}

export type TransferPayload = {
  customerId: string;
  accountId: string;
  receiverCustomerId: string;
  amount: number;
  currency: "TRY";
  idempotencyKey: string;
  referenceId: string;
  note?: string;
};

export type TransferApiResponse = {
  success?: boolean;
  status?: "Succeeded" | "Running" | "Failed" | string;
  message?: string;
  correlationId?: string;
  processId?: string;
  processInstanceKey?: string;
  [key: string]: any;
};

async function authHeaders() {
  const token = await AsyncStorage.getItem(SessionKeys.accessToken);
  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

async function readJsonSafe(res: Response) {
  try {
    return (await res.json()) as any;
  } catch {
    return null;
  }
}

export async function startTransfer(payload: TransferPayload): Promise<TransferApiResponse> {
  const baseUrl = getBaseUrl();
  const url = `${baseUrl}/api/v1/transfers/transfer`;

  const res = await fetch(url, {
    method: "POST",
    headers: await authHeaders(),
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(text || `Transfer request failed (${res.status})`);
  }

  const data = (await readJsonSafe(res)) as TransferApiResponse | null;
  return data ?? { success: true, status: "Running", message: "Transfer is being processed." };
}
