import React from "react";
import { Tabs } from "expo-router";
import { Ionicons } from "@expo/vector-icons";
import { Platform } from "react-native";

export default function TabLayout() {
  return (
    <Tabs
      screenOptions={{
        headerShown: true,
        tabBarActiveTintColor: "#2db7a3",
        tabBarInactiveTintColor: "#98a2b3",

        // Yazıları kaldırıyoruz (sadece ikon)
        tabBarShowLabel: false,

        tabBarStyle: Platform.select({
          ios: { height: 86, paddingBottom: 18, paddingTop: 10 },
          default: { height: 64, paddingBottom: 10, paddingTop: 6 },
        }),
      }}
    >
      {/* 1) Ana Sayfa */}
      <Tabs.Screen
        name="index"
        options={{
          title: "GaniPay",
          tabBarIcon: ({ color, size }) => (
            <Ionicons name="home" size={size} color={color} />
          ),
        }}
      />
{/* 2) Para Yükle */}
<Tabs.Screen
  name="topup"
  options={{
    title: "Para Yükle",
    tabBarIcon: ({ color, size }) => (
      <Ionicons name="wallet-outline" size={size} color={color} />
    ),
  }}
/>

{/* 3) Para Transferi */}
<Tabs.Screen
  name="transfer"
  options={{
    title: "Para Gönder",
    tabBarIcon: ({ color, size }) => (
      <Ionicons name="swap-horizontal-outline" size={size} color={color} />
    ),
  }}
/>

{/* 4) Hesap */}
<Tabs.Screen
  name="account"
  options={{
    title: "Hesap",
    tabBarIcon: ({ color, size }) => (
      <Ionicons name="person-circle-outline" size={size} color={color} />
    ),
  }}
/>
    </Tabs>
  );
}
