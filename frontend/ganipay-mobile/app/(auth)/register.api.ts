import { Platform } from "react-native";
import Constants from "expo-constants";
import type { RegisterPayload } from "./register.types";

/**
 * Backend Swagger: http://localhost:7253/swagger
 * Endpoint: POST /api/v1/onboarding/register
 *
 * NOT:
 * - Web: localhost çalışır
 * - Android emulator: 10.0.2.2 localhost’a gider
 * - iOS simulator: localhost çalışır (Mac)
 * - Expo Go fiziksel cihaz: PC IP gerekir (örn http://192.168.1.20:7253)
 */
const WEB_BASE_URL = "https://localhost:7253";

// Android emulator için özel localhost
const ANDROID_EMULATOR_BASE_URL = "http://10.0.2.2:7253";

// Fiziksel cihaz için PC IP yaz (wifi aynı olmalı)
const DEVICE_BASE_URL = "http://YOUR_PC_IP:7253";

// İstersen ENV’den de alabilirsin (app.json -> extra.apiBaseUrl)
function getEnvBaseUrl(): string | null {
  const extra = (Constants.expoConfig?.extra ?? {}) as any;
  const v = extra.apiBaseUrl as string | undefined;
  return v && v.trim().length > 0 ? v.trim() : null;
}

export function getBaseUrl(): string {
  const env = getEnvBaseUrl();
  if (env) return env;

  if (Platform.OS === "web") return WEB_BASE_URL;

  // Android emulator case
  // Expo Go + android emulator çoğu zaman ANDROID_EMULATOR_BASE_URL ister.
  if (Platform.OS === "android") return ANDROID_EMULATOR_BASE_URL;

  // iOS simulator + (bazı) physical cihazlar için:
  return DEVICE_BASE_URL;
}

type ApiError = {
  message?: string;
  errors?: Record<string, string[]>;
};

function tryParseJson(text: string): any | null {
  try {
    return JSON.parse(text);
  } catch {
    return null;
  }
}

function buildErrorMessage(status: number, bodyText: string): string {
  // json olabilir (validation vs)
  const maybeJson = tryParseJson(bodyText) as ApiError | null;
  if (maybeJson) {
    if (maybeJson.message) return maybeJson.message;

    // ASP.NET validation: { errors: { Field: ["msg"] } }
    const errs = maybeJson.errors;
    if (errs && typeof errs === "object") {
      const firstKey = Object.keys(errs)[0];
      const firstMsg = firstKey ? errs[firstKey]?.[0] : undefined;
      if (firstMsg) return firstMsg;
    }
  }

  // plain text
  if (bodyText && bodyText.trim().length > 0) return bodyText;

  return `Register failed (${status})`;
}

async function fetchWithTimeout(
  input: RequestInfo,
  init: RequestInit,
  timeoutMs = 15000
) {
  const controller = new AbortController();
  const id = setTimeout(() => controller.abort(), timeoutMs);

  try {
    const res = await fetch(input, { ...init, signal: controller.signal });
    return res;
  } finally {
    clearTimeout(id);
  }
}

export async function register(payload: RegisterPayload) {
  const baseUrl = getBaseUrl();
  const url = `${baseUrl}/api/v1/onboarding/register`;

  const res = await fetchWithTimeout(
    url,
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    },
    20000
  );

  const text = await res.text().catch(() => "");

  if (!res.ok) {
    throw new Error(buildErrorMessage(res.status, text));
  }

  // Swagger 200 OK: json dönüyorsa parse et, değilse text dön.
  const json = tryParseJson(text);
  return json ?? text;
}
