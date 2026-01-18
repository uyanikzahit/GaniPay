import React, { useEffect } from "react";
import { View, Text, StyleSheet, Pressable } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useLocalSearchParams, useRouter } from "expo-router";

const BG = "#0B1220";
const CARD = "rgba(255,255,255,0.06)";
const BORDER = "rgba(255,255,255,0.10)";
const GOLD = "rgba(246,195,64,1)";
const MUTED = "rgba(255,255,255,0.60)";
const DANGER = "rgba(255,71,87,0.85)";

export default function TopUpResultScreen() {
  const router = useRouter();
  const params = useLocalSearchParams<{ status?: string; message?: string }>();

  const status = params.status === "success" ? "success" : "failed";
  const isSuccess = status === "success";



  return (
    <View style={styles.page}>
      <View style={styles.card}>
        <Ionicons
          name={isSuccess ? "checkmark-circle-outline" : "close-circle-outline"}
          size={64}
          color={isSuccess ? GOLD : DANGER}
        />

        <Text style={styles.title}>{isSuccess ? "Top up completed" : "Top up failed"}</Text>

        <Text style={styles.message}>
          {params.message ??
            (isSuccess
              ? "Top up successful. Redirecting to home..."
              : "Something went wrong. Please try again.")}
        </Text>

        <Pressable
          onPress={() => router.replace(isSuccess ? "/(tabs)" : "/(tabs)/topup")}
          style={styles.primaryBtn}
        >
          <Text style={styles.primaryText}>{isSuccess ? "Go to Home" : "Try Again"}</Text>
        </Pressable>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  page: {
    flex: 1,
    backgroundColor: BG,
    alignItems: "center",
    justifyContent: "center",
    padding: 16,
  },
  card: {
    width: "100%",
    borderRadius: 20,
    padding: 20,
    backgroundColor: CARD,
    borderWidth: 1,
    borderColor: BORDER,
    alignItems: "center",
  },
  title: {
    marginTop: 14,
    fontSize: 18,
    fontWeight: "900",
    color: "rgba(255,255,255,0.95)",
  },
  message: {
    marginTop: 8,
    textAlign: "center",
    color: MUTED,
    fontWeight: "700",
    fontSize: 13,
  },
  primaryBtn: {
    marginTop: 18,
    borderRadius: 16,
    paddingVertical: 12,
    paddingHorizontal: 20,
    backgroundColor: "rgba(246,195,64,0.22)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.35)",
  },
  primaryText: {
    color: "rgba(255,255,255,0.95)",
    fontWeight: "900",
  },
});
