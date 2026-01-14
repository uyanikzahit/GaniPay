import React, { useMemo } from "react";
import { View, Text, StyleSheet, ScrollView, Pressable, Alert } from "react-native";
import { Ionicons } from "@expo/vector-icons";
import { useRouter } from "expo-router";

const BG="#0B1220", CARD="rgba(255,255,255,0.06)", BORDER="rgba(255,255,255,0.10)", GOLD="rgba(246,195,64,1)", MUTED="rgba(255,255,255,0.60)", SOFT="rgba(255,255,255,0.35)";

export default function AboutScreen() {
  const router = useRouter();

  const info = useMemo(
    () => ({
      appName: "GaniPay",
      version: "2.9.0",
      build: "1029",
      environment: "Development",
    }),
    []
  );

  const onAction = (t: string) => Alert.alert(t, "This item will be connected soon.");

  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.container} showsVerticalScrollIndicator={false}>
      <View style={styles.headerRow}>
        <Pressable onPress={() => router.back()} style={({ pressed }) => [styles.backBtn, pressed && { opacity: 0.9 }]}>
          <Ionicons name="chevron-back" size={18} color="rgba(255,255,255,0.85)" />
        </Pressable>
        <View style={{ flex: 1 }}>
          <Text style={styles.title}>About {info.appName}</Text>
          <Text style={styles.sub}>Version, build info</Text>
        </View>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>App information</Text>
        <Text style={styles.cardHint}>Build and environment details.</Text>

        <View style={styles.kvRow}>
          <Text style={styles.k}>Version</Text>
          <Text style={styles.v}>{info.version}</Text>
        </View>
        <View style={styles.kvRow}>
          <Text style={styles.k}>Build</Text>
          <Text style={styles.v}>{info.build}</Text>
        </View>
        <View style={styles.kvRow}>
          <Text style={styles.k}>Environment</Text>
          <Text style={styles.v}>{info.environment}</Text>
        </View>
      </View>

      <View style={styles.card}>
        <Text style={styles.cardTitle}>Support</Text>
        <Text style={styles.cardHint}>Get help and contact support.</Text>

        <Pressable onPress={() => onAction("Help center")} style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}>
          <View style={styles.iconWrap}>
            <Ionicons name="help-circle-outline" size={18} color={GOLD} />
          </View>
          <View style={{ flex: 1 }}>
            <Text style={styles.itemTitle}>Help center</Text>
            <Text style={styles.itemSub}>FAQs and guides</Text>
          </View>
          <Ionicons name="chevron-forward" size={16} color={SOFT} />
        </Pressable>

        <Pressable onPress={() => onAction("Contact support")} style={({ pressed }) => [styles.item, pressed && { opacity: 0.9 }]}>
          <View style={styles.iconWrap}>
            <Ionicons name="mail-outline" size={18} color={GOLD} />
          </View>
          <View style={{ flex: 1 }}>
            <Text style={styles.itemTitle}>Contact support</Text>
            <Text style={styles.itemSub}>Open a support request</Text>
          </View>
          <Ionicons name="chevron-forward" size={16} color={SOFT} />
        </Pressable>
      </View>

      <View style={styles.card}>
        <View style={styles.tipRow}>
          <Ionicons name="shield-checkmark-outline" size={18} color={GOLD} />
          <Text style={styles.tipText}>GaniPay uses secure communication and follows standard security practices.</Text>
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

  kvRow:{marginTop:10,flexDirection:"row",justifyContent:"space-between"},
  k:{color:MUTED,fontWeight:"800"},
  v:{color:"rgba(255,255,255,0.92)",fontWeight:"900"},

  item:{flexDirection:"row",alignItems:"center",paddingVertical:12,paddingHorizontal:6,borderRadius:14},
  iconWrap:{width:34,height:34,borderRadius:14,alignItems:"center",justifyContent:"center",backgroundColor:"rgba(246,195,64,0.10)",borderWidth:1,borderColor:"rgba(246,195,64,0.22)",marginRight:10},
  itemTitle:{color:"rgba(255,255,255,0.92)",fontSize:13,fontWeight:"900"},
  itemSub:{marginTop:3,color:"rgba(255,255,255,0.55)",fontSize:11.5,fontWeight:"700"},

  tipRow:{borderRadius:14,padding:12,backgroundColor:"rgba(0,0,0,0.18)",borderWidth:1,borderColor:BORDER,flexDirection:"row",alignItems:"center"},
  tipText:{marginLeft:10,color:"rgba(255,255,255,0.78)",fontWeight:"800",fontSize:12,lineHeight:16},
});
