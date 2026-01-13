// app/(auth)/login.tsx
import React, { useMemo, useState } from "react";
import {
  View,
  Text,
  StyleSheet,
  Pressable,
  Platform,
  ScrollView,
  KeyboardAvoidingView,
  TextInput,
  Alert,
} from "react-native";
import { LinearGradient } from "expo-linear-gradient";
import { useRouter } from "expo-router";

import { Colors } from "../theme/colors";
import { login } from "./login.api";

const GOLD = "rgba(246,195,64,1)";

export default function LoginScreen() {
  const router = useRouter();

  const [phoneNumber, setPhoneNumber] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);

  // ✅ HATA MESAJI (inline)
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // ✅ phone max 11, only digits
  const onChangePhone = (val: string) => {
    const digitsOnly = val.replace(/\D/g, "");
    setPhoneNumber(digitsOnly.slice(0, 11));
  };

  const canSubmit = useMemo(() => {
    // TR numaraları genelde 10/11 — sen max 11 istedin
    return phoneNumber.trim().length >= 10 && password.trim().length >= 3;
  }, [phoneNumber, password]);

  const onSubmit = async () => {
    // ✅ her denemede eski hatayı temizle
    setErrorMessage(null);

    if (!canSubmit) {
      // mevcut yapıyı bozmayalım: istersen burayı da inline yapabiliriz ama dokunmadım
      Alert.alert("Check your details", "Phone number and password are required.");
      return;
    }

    try {
      setLoading(true);

      const res = await login({
        phoneNumber: phoneNumber.trim(),
        password: password.trim(),
        channel: Platform.OS === "web" ? "WEB" : "MOBILE",
        clientVersion: "0.1",
      });

      // ✅ Başarılıysa
      if (res.success === true && res.status === "Succeeded" && res.token) {
        router.replace("/(tabs)");
        return;
      }

      // ✅ Başarısızsa: ekranda göster
      setErrorMessage(res.message || "The password or phone number is incorrect.");
    } catch (e: any) {
      // ✅ 4xx/5xx vb. sistem hataları
      setErrorMessage(e?.message ?? "Unknown error");
    } finally {
      setLoading(false);
    }
  };

  return (
    <View style={styles.container}>
      <LinearGradient
        colors={[Colors.bg, "#0B1220", "#12223E"]}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        style={styles.hero}
      >
        <LinearGradient
          colors={["rgba(246,195,64,0.85)", "rgba(246,195,64,0)"]}
          start={{ x: 0.2, y: 0 }}
          end={{ x: 0.8, y: 1 }}
          style={styles.goldGlow}
        />

        {/* ✅ Top bar (aşağı indirildi) */}
        <View style={styles.topBar}>
          <Pressable
            onPress={() => router.back()}
            style={({ pressed }) => [styles.backBtn, pressed && { opacity: 0.85 }]}
          >
            <Text style={styles.backText}>←</Text>
          </Pressable>

          <View style={{ flex: 1 }}>
            <Text style={styles.pageTitle}>Sign in</Text>
            <Text style={styles.pageSub}>Secure access to your GaniPay wallet.</Text>
          </View>
        </View>

        <KeyboardAvoidingView
          style={{ flex: 1 }}
          behavior={Platform.OS === "ios" ? "padding" : undefined}
        >
          <View style={styles.heroSpacer} />

          <ScrollView
            contentContainerStyle={styles.scroll}
            keyboardShouldPersistTaps="handled"
            showsVerticalScrollIndicator={false}
          >
            <View style={styles.card}>
              <Text style={styles.cardTitle}>Welcome to GaniPay</Text>
              <Text style={styles.cardDesc}>
                Sign in to continue. We’ll securely verify your session in the background.
              </Text>

              <View style={{ height: 14 }} />

              <Text style={styles.label}>Phone number</Text>
              <TextInput
                value={phoneNumber}
                onChangeText={onChangePhone}
                keyboardType="phone-pad"
                placeholder="5XXXXXXXXXX"
                placeholderTextColor="rgba(255,255,255,0.35)"
                style={styles.input}
                maxLength={11} // ✅ ekstra güvenlik
                autoCorrect={false}
                autoCapitalize="none"
              />

              <View style={{ height: 12 }} />

              <Text style={styles.label}>Password</Text>
              <TextInput
                value={password}
                onChangeText={setPassword}
                secureTextEntry
                placeholder="••••••••"
                placeholderTextColor="rgba(255,255,255,0.35)"
                style={styles.input}
                autoCorrect={false}
                autoCapitalize="none"
              />

              {/* ✅ Forgot password (tıklanabilir, yönlendirme yok) */}
              <Pressable
                onPress={() => Alert.alert("Info", "Forgot password?")}
                style={({ pressed }) => [styles.forgotWrap, pressed && { opacity: 0.85 }]}
                hitSlop={10}
              >
                <Text style={styles.forgotText}>Forgot password</Text>
              </Pressable>

              {/* ✅ Yanlış şifre/telefon mesajı burada gözüksün */}
              {errorMessage && <Text style={styles.errorText}>{errorMessage}</Text>}

              <View style={{ height: 10 }} />

              <Text style={styles.hint}>Protected by device & risk checks • Encrypted session</Text>
            </View>

            <View style={{ height: 120 }} />
          </ScrollView>

          <View style={styles.actionsWrap}>
            <View style={styles.actions}>
              <Pressable
                disabled={loading}
                onPress={() => router.replace("/(auth)/register")}
                style={({ pressed }) => [
                  styles.btnGhost,
                  loading && { opacity: 0.4 },
                  pressed && { opacity: 0.92 },
                ]}
              >
                <Text style={styles.btnGhostText}>Sign up</Text>
              </Pressable>

              <Pressable
                disabled={loading || !canSubmit}
                onPress={onSubmit}
                style={({ pressed }) => [
                  styles.btnPrimary,
                  (!canSubmit || loading) && { opacity: 0.6 },
                  pressed && { opacity: 0.92 },
                ]}
              >
                <Text style={styles.btnPrimaryText}>{loading ? "Signing in..." : "Log in"}</Text>
              </Pressable>
            </View>
          </View>
        </KeyboardAvoidingView>
      </LinearGradient>
    </View>
  );
}

const shadow = Platform.select({
  ios: {
    shadowColor: "#000",
    shadowOpacity: 0.22,
    shadowRadius: 18,
    shadowOffset: { width: 0, height: 12 },
  },
  android: { elevation: 10 },
  web: { boxShadow: "0px 14px 40px rgba(0,0,0,0.28)" } as any,
});

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.bg, overflow: "hidden" },

  hero: {
    flex: 1,
    paddingTop: 22,
    paddingHorizontal: 18,
    overflow: "hidden",
  },

  heroSpacer: {
    height: 64, // kartı aşağı iter (istersen 60-100 arası oyna)
  },

  goldGlow: {
    position: "absolute",
    top: -30,
    left: 0,
    right: 0,
    height: 250,
    borderBottomLeftRadius: 220,
    borderBottomRightRadius: 220,
    opacity: 0.9,
    transform: [{ scaleX: 1.15 }],
  },

  // ✅ TopBar’ı aşağı aldık
  topBar: {
    flexDirection: "row",
    alignItems: "flex-start",
    gap: 12,
    marginTop: 34, // ⬅️ önce 12’ydi
    marginBottom: 10,
  },

  backBtn: {
    width: 40,
    height: 40,
    borderRadius: 14,
    backgroundColor: "rgba(255,255,255,0.10)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.16)",
    alignItems: "center",
    justifyContent: "center",
  },
  backText: {
    color: "rgba(255,255,255,0.92)",
    fontSize: 18,
    fontWeight: "900",
  },
  pageTitle: {
    color: "rgba(255,255,255,0.96)",
    fontSize: 22,
    fontWeight: "900",
  },
  pageSub: {
    color: "rgba(255,255,255,0.70)",
    fontSize: 13,
    marginTop: 4,
  },

  scroll: { paddingTop: 10, paddingBottom: 10 },

  card: {
    marginTop: 0,
    borderRadius: 22,
    padding: 16,
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.10)",
    ...shadow,
  },

  cardTitle: {
    color: "rgba(255,255,255,0.92)",
    fontSize: 18,
    fontWeight: "900",
  },
  cardDesc: {
    color: "rgba(255,255,255,0.65)",
    fontSize: 12.5,
    marginTop: 6,
    lineHeight: 18,
  },

  label: {
    color: "rgba(255,255,255,0.72)",
    fontSize: 12,
    fontWeight: "800",
    marginBottom: 6,
  },

  input: {
    height: 52,
    borderRadius: 16,
    paddingHorizontal: 14,
    color: "rgba(255,255,255,0.92)",
    backgroundColor: "rgba(0,0,0,0.18)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.10)",
    fontSize: 14,
    fontWeight: "700",
  },

  // ✅ Forgot password
  forgotWrap: {
    alignSelf: "flex-end",
    marginTop: 10,
  },
  forgotText: {
    color: "rgba(246,195,64,0.95)",
    fontSize: 12.5,
    fontWeight: "900",
  },

  // ✅ Inline error text
  errorText: {
    marginTop: 10,
    color: "rgba(252,165,165,0.95)",
    fontSize: 12.5,
    fontWeight: "700",
  },

  hint: {
    color: "rgba(255,255,255,0.55)",
    fontSize: 11.5,
    marginTop: 8,
  },

  actionsWrap: {
    position: "absolute",
    left: 18,
    right: 18,
    bottom: 14,
  },
  actions: {
    flexDirection: "row",
    gap: 12,
    padding: 12,
    borderRadius: 20,
    backgroundColor: "rgba(255,255,255,0.06)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.10)",
    ...shadow,
  },

  btnGhost: {
    flex: 1,
    height: 54,
    borderRadius: 999,
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.35)",
    backgroundColor: "rgba(246,195,64,0.10)",
    alignItems: "center",
    justifyContent: "center",
  },
  btnGhostText: {
    color: "rgba(255,255,255,0.92)",
    fontWeight: "900",
    fontSize: 16,
  },

  btnPrimary: {
    flex: 1,
    height: 54,
    borderRadius: 999,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: GOLD,
  },
  btnPrimaryText: {
    color: "#111827",
    fontWeight: "900",
    fontSize: 16,
  },
});