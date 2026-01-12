import { Platform } from "react-native";
import type { RegisterPayload } from "./register.types";

const WEB_BASE_URL = "http://localhost:7253";

// ✅ Mobilde localhost çalışmaz. PC IP yaz.
const MOBILE_BASE_URL = "http://YOUR_PC_IP:7253";

export function getBaseUrl() {
  return Platform.OS === "web" ? WEB_BASE_URL : MOBILE_BASE_URL;
}

export async function register(payload: RegisterPayload) {
  const baseUrl = getBaseUrl();
  const url = `${baseUrl}/api/v1/onboarding/register`;

  const res = await fetch(url, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(text || `Register failed (${res.status})`);
  }

  // Swagger 200 OK - response içeriğini bilmiyoruz, json/text olabilir:
  const contentType = res.headers.get("content-type") || "";
  if (contentType.includes("application/json")) return res.json();
  return res.text();
}
