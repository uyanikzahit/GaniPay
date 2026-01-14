import React, { useMemo, useState } from "react";
import { View, Text, StyleSheet, ScrollView, Pressable, TextInput, Alert } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";

const BG="#0B1220", CARD="rgba(255,255,255,0.06)", BORDER="rgba(255,255,255,0.10)", GOLD="rgba(246,195,64,1)", MUTED="rgba(255,255,255,0.60)", SOFT="rgba(255,255,255,0.35)";

type Partner = {
  id: string;
  name: string;
  category: string;
  cashback: string;
  icon: React.ComponentProps<typeof Ionicons>["name"];
};

export default function PartnersScreen() {
  const router = useRouter();
  const [search, setSearch] = useState("");

  const partners = useMemo<Partner[]>(
    () => [
      { id: "p1", name: "Coffee House", category: "Food & Beverage", cashback: "Up to 5% cashback", icon: "cafe-outline" },
      { id: "p2", name: "Ride & Taxi", category: "Transport", cashback: "Instant discount", icon: "car-outline" },
      { id: "p3", name: "Online Market", category: "Shopping", cashback: "Up to 3% cashback", icon: "bag-outline" },
      { id: "p4", name: "Cinema", category: "Entertainment", cashback: "2-for-1 tickets", icon: "film-outline" },
    ],
    []
  );

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return partners;
    return partners.filter((p) => (p.name + " " + p.category).toLowerCase().includes(q));
  }, [search, partners]);

  const onOpen = (p: Partner) => Alert.alert("Partner details", `${p.name}\n${p.cashback}`);

  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.container} showsVerticalScrollIndicator={false}>
      <View style={styles.headerRow}>
        <Pressable onPress={() => router.back()} style={({ pressed }) => [styles.backBtn, pressed && { opacity: 0.9 }]}>
          <Ionicons name="chevron-back" size={18} color="rgba(255,255,255,0.85)" />
        </Pressable>
        <View style={{ flex: 1 }}>
          <Text style={styles.title}>Partner stores</Text>
          <Text style={styles.sub}>Deals & supported merchants</Text>
        </View>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Search</Text>
        <Text style={styles.cardHint}>Find a partner by name or category.</Text>

        <View style={styles.inputRow}>
          <Ionicons name="search-outline" size={18} color={SOFT} />
          <TextInput
            value={search}
            onChangeText={setSearch}
            placeholder="Search…"
            placeholderTextColor="rgba(255,255,255,0.25)"
            style={styles.input}
          />
        </View>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Featured</Text>
        <Text style={styles.cardHint}>Selected partners and offers.</Text>

        {filtered.map((p) => (
          <Pressable key={p.id} onPress={() => onOpen(p)} style={({ pressed }) => [styles.partnerRow, pressed && { opacity: 0.9 }]}>
            <View style={styles.iconWrap}>
              <Ionicons name={p.icon} size={18} color={GOLD} />
            </View>
            <View style={{ flex: 1 }}>
              <Text style={styles.partnerTitle}>{p.name}</Text>
              <Text style={styles.partnerSub}>{p.category} • {p.cashback}</Text>
            </View>
            <Ionicons name="chevron-forward" size={16} color={SOFT} />
          </Pressable>
        ))}
      </View>

      <View style={styles.card}>
        <View style={styles.tipRow}>
          <Ionicons name="information-circle-outline" size={18} color={GOLD} />
          <Text style={styles.tipText}>Offers may require eligible payment method and can change over time.</Text>
        </View>
      </View>

      <View style={{ height: 26 }} />
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  page:{flex:1, backgroundColor:BG},
  container:{padding:16, paddingBottom:28},

  headerRow:{flexDirection:"row", alignItems:"center"},
  backBtn:{width:38,height:38,borderRadius:14,backgroundColor:"rgba(255,255,255,0.06)",borderWidth:1,borderColor:BORDER,alignItems:"center",justifyContent:"center",marginRight:10},
  title:{color:"rgba(255,255,255,0.92)",fontSize:20,fontWeight:"900"},
  sub:{marginTop:6,color:MUTED,fontSize:12.5,fontWeight:"700"},

  card:{marginTop:14,borderRadius:18,padding:14,backgroundColor:CARD,borderWidth:1,borderColor:BORDER},
  cardTitle:{color:"rgba(255,255,255,0.92)",fontSize:14,fontWeight:"900"},
  cardHint:{marginTop:6,color:"rgba(255,255,255,0.55)",fontSize:11.5,fontWeight:"700",lineHeight:16},

  inputRow:{marginTop:10,borderRadius:14,paddingHorizontal:12,paddingVertical:10,backgroundColor:"rgba(0,0,0,0.18)",borderWidth:1,borderColor:BORDER,flexDirection:"row",alignItems:"center"},
  input:{flex:1,color:"rgba(255,255,255,0.92)",fontWeight:"900",fontSize:14,marginLeft:10,paddingVertical:0},

  partnerRow:{marginTop:12,flexDirection:"row",alignItems:"center",padding:12,borderRadius:16,backgroundColor:"rgba(0,0,0,0.18)",borderWidth:1,borderColor:BORDER},
  iconWrap:{width:36,height:36,borderRadius:14,alignItems:"center",justifyContent:"center",backgroundColor:"rgba(246,195,64,0.10)",borderWidth:1,borderColor:"rgba(246,195,64,0.22)",marginRight:10},
  partnerTitle:{color:"rgba(255,255,255,0.92)",fontWeight:"900",fontSize:13},
  partnerSub:{marginTop:4,color:MUTED,fontWeight:"700",fontSize:11.5},

  tipRow:{borderRadius:14,padding:12,backgroundColor:"rgba(0,0,0,0.18)",borderWidth:1,borderColor:BORDER,flexDirection:"row",alignItems:"center"},
  tipText:{marginLeft:10,color:"rgba(255,255,255,0.78)",fontWeight:"800",fontSize:12,lineHeight:16},
});
