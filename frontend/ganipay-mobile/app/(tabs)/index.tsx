import React from "react";
import { View, Text, StyleSheet, TouchableOpacity, ScrollView } from "react-native";

export default function HomeScreen() {
  const userName = "İlyas";
  const balance = "0,00 ₺";

  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.container}>
      {/* Üst Kart */}
      <View style={styles.headerCard}>
        <View style={styles.headerRow}>
          <View>
            <Text style={styles.welcome}>Hoş Geldin,</Text>
            <Text style={styles.name}>{userName}</Text>
          </View>

          <View style={{ alignItems: "flex-end" }}>
            <Text style={styles.balanceLabel}>Bakiye</Text>
            <Text style={styles.balance}>{balance}</Text>
          </View>
        </View>

        <View style={styles.quickRow}>
          <QuickButton title="Para Transferi" />
          <QuickButton title="Para Yükle" />
          <QuickButton title="Fatura Öde" />
          <QuickButton title="İşlemler" />
        </View>
      </View>

      {/* Grid Kartlar */}
      <View style={styles.grid}>
        <GridCard title="Para Transferi" />
        <GridCard title="Fatura Öde" />
        <GridCard title="Para Yükle" />
        <GridCard title="Hesap Detayları" />
      </View>

      {/* Son İşlemler */}
      <View style={styles.sectionHeader}>
        <Text style={styles.sectionTitle}>Son İşlemler</Text>
        <Text style={styles.sectionLink}>Tümünü Gör</Text>
      </View>

      <View style={styles.txItem}>
        <Text style={styles.txTitle}>Para Yükleme</Text>
        <Text style={styles.txAmount}>+200,00 ₺</Text>
      </View>

      <View style={styles.txItem}>
        <Text style={styles.txTitle}>Transfer</Text>
        <Text style={styles.txAmount}>-120,00 ₺</Text>
      </View>
    </ScrollView>
  );
}

function QuickButton({ title }: { title: string }) {
  return (
    <TouchableOpacity style={styles.quickBtn} onPress={() => {}}>
      <Text style={styles.quickText}>{title}</Text>
    </TouchableOpacity>
  );
}

function GridCard({ title }: { title: string }) {
  return (
    <TouchableOpacity style={styles.gridCard} onPress={() => {}}>
      <Text style={styles.gridTitle}>{title}</Text>
    </TouchableOpacity>
  );
}

const styles = StyleSheet.create({
  page: { flex: 1, backgroundColor: "#f6f7f9" },
  container: { padding: 16, paddingBottom: 28 },

  headerCard: {
    backgroundColor: "#2db7a3",
    borderRadius: 16,
    padding: 16,
    marginBottom: 14,
  },
  headerRow: { flexDirection: "row", justifyContent: "space-between" },
  welcome: { color: "white", opacity: 0.9 },
  name: { color: "white", fontSize: 20, fontWeight: "800", marginTop: 2 },
  balanceLabel: { color: "white", opacity: 0.9 },
  balance: { color: "white", fontSize: 18, fontWeight: "800", marginTop: 2 },

  quickRow: { flexDirection: "row", justifyContent: "space-between", marginTop: 14, gap: 8 },
  quickBtn: { backgroundColor: "rgba(255,255,255,0.2)", padding: 10, borderRadius: 12, flex: 1 },
  quickText: { color: "white", fontSize: 12, fontWeight: "700", textAlign: "center" },

  grid: { flexDirection: "row", flexWrap: "wrap", gap: 12, marginBottom: 10 },
  gridCard: { backgroundColor: "white", borderRadius: 14, padding: 16, width: "48%" },
  gridTitle: { fontWeight: "800" },

  sectionHeader: { flexDirection: "row", justifyContent: "space-between", alignItems: "center", marginTop: 8 },
  sectionTitle: { fontSize: 16, fontWeight: "900" },
  sectionLink: { color: "#2db7a3", fontWeight: "800" },

  txItem: {
    backgroundColor: "white",
    borderRadius: 12,
    padding: 14,
    marginTop: 10,
    flexDirection: "row",
    justifyContent: "space-between",
  },
  txTitle: { fontWeight: "800" },
  txAmount: { fontWeight: "900" },
});
