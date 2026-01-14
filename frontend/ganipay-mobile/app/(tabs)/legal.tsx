import React, { useMemo } from "react";
import { View, Text, StyleSheet, ScrollView, Pressable, Alert } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";

const BG="#0B1220", CARD="rgba(255,255,255,0.06)", BORDER="rgba(255,255,255,0.10)", GOLD="rgba(246,195,64,1)", MUTED="rgba(255,255,255,0.60)", SOFT="rgba(255,255,255,0.35)";

type Doc = {
  id: string;
  title: string;
  subtitle: string;
  icon: React.ComponentProps<typeof Ionicons>["name"];
};

export default function LegalScreen() {
  const router = useRouter();

  const docs = useMemo<Doc[]>(
    () => [
      { id: "d1", title: "Terms of Service", subtitle: "User agreement and service rules", icon: "document-text-outline" },
      { id: "d2", title: "Privacy Policy", subtitle: "How we process and protect personal data", icon: "lock-closed-outline" },
      { id: "d3", title: "Fees & Limits", subtitle: "Pricing, limits and important disclosures", icon: "pricetag-outline" },
      { id: "d4", title: "KYC & Verification", subtitle: "Identity verification requirements", icon: "id-card-outline" },
    ],
    []
  );

  const onOpen = (d: Doc) => Alert.alert(d.title, "This document will open as a web view / PDF in the next step.");

  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.container} showsVerticalScrollIndicator={false}>
      <View style={styles.headerRow}>
        <Pressable onPress={() => router.back()} style={({ pressed }) => [styles.backBtn, pressed && { opacity: 0.9 }]}>
          <Ionicons name="chevron-back" size={18} color="rgba(255,255,255,0.85)" />
        </Pressable>
        <View style={{ flex: 1 }}>
          <Text style={styles.title}>Legal</Text>
          <Text style={styles.sub}>Terms, privacy, policy</Text>
        </View>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Documents</Text>
        <Text style={styles.cardHint}>Read the key documents related to your wallet usage.</Text>

        {docs.map((d) => (
          <Pressable key={d.id} onPress={() => onOpen(d)} style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}>
            <View style={styles.iconWrap}>
              <Ionicons name={d.icon} size={18} color={GOLD} />
            </View>
            <View style={{ flex: 1 }}>
              <Text style={styles.itemTitle}>{d.title}</Text>
              <Text style={styles.itemSub}>{d.subtitle}</Text>
            </View>
            <Ionicons name="chevron-forward" size={16} color={SOFT} />
          </Pressable>
        ))}
      </View>

      <View style={styles.card}>
        <View style={styles.tipRow}>
          <Ionicons name="information-circle-outline" size={18} color={GOLD} />
          <Text style={styles.tipText}>Legal content is provided for transparency and compliance requirements.</Text>
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

  item:{flexDirection:"row",alignItems:"center",paddingVertical:12,paddingHorizontal:6,borderRadius:14},
  iconWrap:{width:34,height:34,borderRadius:14,alignItems:"center",justifyContent:"center",backgroundColor:"rgba(246,195,64,0.10)",borderWidth:1,borderColor:"rgba(246,195,64,0.22)",marginRight:10},
  itemTitle:{color:"rgba(255,255,255,0.92)",fontSize:13,fontWeight:"900"},
  itemSub:{marginTop:3,color:"rgba(255,255,255,0.55)",fontSize:11.5,fontWeight:"700"},

  tipRow:{borderRadius:14,padding:12,backgroundColor:"rgba(0,0,0,0.18)",borderWidth:1,borderColor:BORDER,flexDirection:"row",alignItems:"center"},
  tipText:{marginLeft:10,color:"rgba(255,255,255,0.78)",fontWeight:"800",fontSize:12,lineHeight:16},
});
