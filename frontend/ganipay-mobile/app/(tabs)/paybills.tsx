import React, { useMemo, useState } from "react";
import { View, Text, StyleSheet, ScrollView, Pressable, TextInput, Alert } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";

const BG="#0B1220", CARD="rgba(255,255,255,0.06)", BORDER="rgba(255,255,255,0.10)", GOLD="rgba(246,195,64,1)", MUTED="rgba(255,255,255,0.60)", SOFT="rgba(255,255,255,0.35)";

type Biller = {
  id: string;
  name: string;
  category: "Electricity" | "Water" | "Gas" | "Internet" | "Mobile";
  icon: React.ComponentProps<typeof Ionicons>["name"];
};

export default function PayBillsScreen() {
  const router = useRouter();
  const [search, setSearch] = useState("");

  const billers = useMemo<Biller[]>(
    () => [
      { id: "b1", name: "Electricity Provider", category: "Electricity", icon: "flash-outline" },
      { id: "b2", name: "Water Services", category: "Water", icon: "water-outline" },
      { id: "b3", name: "Natural Gas", category: "Gas", icon: "flame-outline" },
      { id: "b4", name: "Fiber Internet", category: "Internet", icon: "wifi-outline" },
      { id: "b5", name: "Mobile Operator", category: "Mobile", icon: "phone-portrait-outline" },
    ],
    []
  );

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return billers;
    return billers.filter((b) => (b.name + " " + b.category).toLowerCase().includes(q));
  }, [search, billers]);

  const onSelect = (b: Biller) => {
    Alert.alert("Bill payment", `${b.name}\n\nThis flow will be connected to biller integration.`);
  };

  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.container} showsVerticalScrollIndicator={false}>
      <View style={styles.headerRow}>
        <Pressable onPress={() => router.back()} style={({ pressed }) => [styles.backBtn, pressed && { opacity: 0.9 }]}>
          <Ionicons name="chevron-back" size={18} color="rgba(255,255,255,0.85)" />
        </Pressable>

        <View style={{ flex: 1 }}>
          <Text style={styles.title}>Pay bills</Text>
          <Text style={styles.sub}>Utilities & subscriptions</Text>
        </View>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Search provider</Text>
        <Text style={styles.cardHint}>Find a biller by name or category.</Text>

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
        <Text style={styles.cardTitle}>Providers</Text>
        <Text style={styles.cardHint}>Choose a provider to continue.</Text>

        {filtered.map((b) => (
          <Pressable key={b.id} onPress={() => onSelect(b)} style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}>
            <View style={styles.iconWrap}>
              <Ionicons name={b.icon} size={18} color={GOLD} />
            </View>
            <View style={{ flex: 1 }}>
              <Text style={styles.itemTitle}>{b.name}</Text>
              <Text style={styles.itemSub}>{b.category}</Text>
            </View>
            <Ionicons name="chevron-forward" size={16} color={SOFT} />
          </Pressable>
        ))}
      </View>

      <View style={styles.card}>
        <View style={styles.tipRow}>
          <Ionicons name="shield-checkmark-outline" size={18} color={GOLD} />
          <Text style={styles.tipText}>
            Bill payments typically require verification and can be subject to limits and fraud checks.
          </Text>
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

  item:{flexDirection:"row",alignItems:"center",paddingVertical:12,paddingHorizontal:6,borderRadius:14},
  iconWrap:{width:34,height:34,borderRadius:14,alignItems:"center",justifyContent:"center",backgroundColor:"rgba(246,195,64,0.10)",borderWidth:1,borderColor:"rgba(246,195,64,0.22)",marginRight:10},
  itemTitle:{color:"rgba(255,255,255,0.92)",fontSize:13,fontWeight:"900"},
  itemSub:{marginTop:3,color:"rgba(255,255,255,0.55)",fontSize:11.5,fontWeight:"700"},

  tipRow:{borderRadius:14,padding:12,backgroundColor:"rgba(0,0,0,0.18)",borderWidth:1,borderColor:BORDER,flexDirection:"row",alignItems:"center"},
  tipText:{marginLeft:10,color:"rgba(255,255,255,0.78)",fontWeight:"800",fontSize:12,lineHeight:16},
});
