// constants/workflowBase.ts
import { Platform } from "react-native";
import Constants from "expo-constants";

/**
 * DEFAULT (güvenli): Workflow API'ye DIRECT gider
 * - Web: https://localhost:7253
 * - Android emulator: http://10.0.2.2:5210
 * - Real device: http://192.168.1.5:5210
 *
 * İstersen APISIX/Gateway açarsın:
 * - extra.useGateway: true
 * - extra.gatewayBaseUrl: http://192.168.1.5:9080 (real device)
 *   veya web için http://localhost:9080
 *
 * Gateway açıkken prefix: /workflow-api
 */

type Extra = {
  useGateway?: boolean | string;
  gatewayBaseUrl?: string;
  webDirectBaseUrl?: string;
  deviceDirectBaseUrl?: string;
};

function getExtra(): Extra {
  return (Constants.expoConfig?.extra ?? {}) as any;
}

function asBool(v: unknown): boolean {
  if (typeof v === "boolean") return v;
  if (typeof v === "string") return v.toLowerCase() === "true";
  return false;
}

/** ✅ Senin çalışan DIRECT base mantığın */
export function getWorkflowDirectBaseUrl() {
  const extra = getExtra();

  const WEB_DIRECT = extra.webDirectBaseUrl?.trim() || "https://localhost:7253";
  const REAL_DEVICE_DIRECT = extra.deviceDirectBaseUrl?.trim() || "http://192.168.1.5:5210";
  const ANDROID_EMU_DIRECT = "http://10.0.2.2:5210";

  if (Platform.OS === "web") return WEB_DIRECT;
  if (Platform.OS === "android") return ANDROID_EMU_DIRECT;
  return REAL_DEVICE_DIRECT;
}

/** ✅ Gateway base */
export function getGatewayBaseUrl() {
  const extra = getExtra();

  // app.json extra.gatewayBaseUrl ile override edilebilir
  const env = (extra.gatewayBaseUrl ?? "").trim();
  if (env) return env;

  // default gateway değerleri (istersen değiştirirsin)
  const WEB_GATEWAY = "http://localhost:9080";
  const ANDROID_EMU_GATEWAY = "http://10.0.2.2:9080";
  const REAL_DEVICE_GATEWAY = "http://192.168.1.5:9080";

  if (Platform.OS === "web") return WEB_GATEWAY;
  if (Platform.OS === "android") return ANDROID_EMU_GATEWAY;
  return REAL_DEVICE_GATEWAY;
}

/**
 * ✅ BÜTÜN api.ts dosyalarının kullanacağı tek fonksiyon.
 * Varsayılan DIRECT döner (bozmaz).
 * Sadece extra.useGateway=true ise APISIX'e döner.
 */
export function getWorkflowApiBaseUrl() {
  const extra = getExtra();
  const useGateway = asBool(extra.useGateway);

  if (!useGateway) {
    // ✅ senin çalışan eski düzen
    return getWorkflowDirectBaseUrl();
  }

  // ✅ APISIX üzerinden erişim
  return `${getGatewayBaseUrl()}/workflow-api`;
}
