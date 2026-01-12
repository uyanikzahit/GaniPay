import React, { useMemo, useRef, useState, useCallback } from "react";
import {
  View,
  Text,
  StyleSheet,
  FlatList,
  useWindowDimensions,
  Pressable,
  Platform,
  Image,
  ViewToken,
} from "react-native";
import { useRouter } from "expo-router";
import { LinearGradient } from "expo-linear-gradient";
import { Colors } from "../theme/colors";

type Slide = {
  key: string;
  title: string;
  subtitle: string;
  iconEmoji: string;
  image?: any; // require(...)
};

export default function WelcomeScreen() {
  const router = useRouter();
  const { width } = useWindowDimensions();
  const [index, setIndex] = useState(0);
  const listRef = useRef<FlatList<Slide>>(null);

  const slides: Slide[] = useMemo(
    () => [
      {
        key: "s1",
        title: "Secure wallet experience",
        subtitle: "Manage your account with confidence and speed.",
        iconEmoji: "🛡️",
        // image: require("../../assets/images/onboarding/secure.png"),
      },
      {
        key: "s2",
        title: "Instant top-up",
        subtitle: "Complete top-up transactions in seconds.",
        iconEmoji: "⚡",
        // image: require("../../assets/images/onboarding/topup.png"),
      },
      {
        key: "s3",
        title: "Easy transfers",
        subtitle: "Fast and trackable wallet-to-wallet transfers.",
        iconEmoji: "🔁",
        // image: require("../../assets/images/onboarding/transfer.png"),
      },
    ],
    []
  );

  // ✅ Dots index'i kesin güncelleyen mekanizma
  const viewabilityConfig = useRef({ viewAreaCoveragePercentThreshold: 60 });

  const onViewableItemsChanged = useRef(
    ({ viewableItems }: { viewableItems: Array<ViewToken> }) => {
      const first = viewableItems?.[0];
      if (first?.index != null) setIndex(first.index);
    }
  );

  const goLogin = useCallback(() => router.push("/(auth)/login"), [router]);
  const goRegister = useCallback(() => router.push("/(auth)/register"), [router]);

  return (
    <View style={styles.container}>
      {/* HERO: daha kısa + boşluk az */}
      <LinearGradient
        colors={[Colors.hero, Colors.hero2]}
        start={{ x: 0.2, y: 0 }}
        end={{ x: 0.8, y: 1 }}
        style={styles.hero}
      >
        <View style={styles.brandRow}>
          <Image
            source={require("../../assets/images/ganipay-logo.png")}
            style={styles.logo}
            resizeMode="contain"
          />

          <View>
            <Text style={styles.brandName}>GaniPay</Text>
            <Text style={styles.brandSub}>Welcome</Text>
          </View>
        </View>
      </LinearGradient>

      {/* BOTTOM CARD */}
      <View style={styles.bottomCard}>
        {/* Slide: illustration + title + subtitle (hepsi beraber kayıyor) */}
        <FlatList
          ref={listRef}
          data={slides}
          keyExtractor={(item) => item.key}
          horizontal
          pagingEnabled
          showsHorizontalScrollIndicator={false}
          renderItem={({ item }) => (
            <View style={[styles.slide, { width }]}>
              <View style={styles.illustrationCard}>
                {item.image ? (
                  <Image source={item.image} style={styles.slideImage} resizeMode="contain" />
                ) : (
                  <Text style={styles.illustrationEmoji}>{item.iconEmoji}</Text>
                )}
              </View>

              <Text style={styles.title}>{item.title}</Text>
              <Text style={styles.subtitle}>{item.subtitle}</Text>
            </View>
          )}
          viewabilityConfig={viewabilityConfig.current}
          onViewableItemsChanged={onViewableItemsChanged.current}
        />

        {/* DOTS: sabit, doğru index ile dolacak */}
        <View style={styles.dotsRow}>
          {slides.map((_, i) => (
            <View
              key={i}
              style={[
                styles.dot,
                i === index ? styles.dotActive : styles.dotIdle,
              ]}
            />
          ))}
        </View>

        {/* Buttons: sabit */}
        <View style={styles.actions}>
          <Pressable
            style={({ pressed }) => [
              styles.btnOutline,
              pressed && { opacity: 0.9 },
            ]}
            onPress={goLogin}
          >
            <Text style={styles.btnOutlineText}>Log In</Text>
          </Pressable>

          <Pressable
            style={({ pressed }) => [
              styles.btnPrimary,
              pressed && { opacity: 0.9 },
            ]}
            onPress={goRegister}
          >
            <Text style={styles.btnPrimaryText}>Sign Up</Text>
          </Pressable>
        </View>
      </View>
    </View>
  );
}

const shadow = Platform.select({
  ios: {
    shadowColor: "#000",
    shadowOpacity: 0.14,
    shadowRadius: 18,
    shadowOffset: { width: 0, height: 10 },
  },
  android: { elevation: 7 },
  web: { boxShadow: "0px 10px 30px rgba(0,0,0,0.14)" } as any,
});

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.bg },

  // ✅ Sarı alanı kısalttık (boşluk gitti)
  hero: {
    height: 118, // sabit yükseklik -> boş sarı alan kalmaz
    paddingTop: 22,
    paddingHorizontal: 18,
    paddingBottom: 14,
    justifyContent: "flex-start",
  },

  brandRow: {
    flexDirection: "row",
    alignItems: "center",
    gap: 12,
    marginTop: 2,
  },

  // ✅ Logo büyüdü
  logo: {
    width: 72,
    height: 72,
  },

  brandName: { fontSize: 20, fontWeight: "900", color: "#111827" },
  brandSub: { fontSize: 12.5, color: "#374151", marginTop: 2 },

  bottomCard: {
    flex: 1,
    backgroundColor: Colors.surface,
    borderTopLeftRadius: 30,
    borderTopRightRadius: 30,
    overflow: "hidden",
    marginTop: -18,
    paddingTop: 18,
    paddingBottom: 22,
  },

  slide: {
    paddingHorizontal: 22,
    alignItems: "center",
    justifyContent: "flex-start",
    paddingTop: 6,
  },

  illustrationCard: {
    width: 240,
    height: 240,
    borderRadius: 28,
    backgroundColor: "rgba(255,255,255,0.72)",
    alignItems: "center",
    justifyContent: "center",
    marginBottom: 18,
    ...shadow,
  },

  // premium görsel için alan
  slideImage: {
    width: 170,
    height: 170,
  },

  illustrationEmoji: { fontSize: 84 },

  title: {
    fontSize: 26,
    fontWeight: "900",
    color: Colors.text,
    textAlign: "center",
    marginBottom: 10,
  },
  subtitle: {
    fontSize: 15.5,
    lineHeight: 22,
    color: Colors.muted,
    textAlign: "center",
    paddingHorizontal: 10,
  },

  dotsRow: {
    alignSelf: "center",
    flexDirection: "row",
    justifyContent: "center",
    gap: 8,
    marginTop: 10,
    marginBottom: 12,
  },
  dot: { width: 8, height: 8, borderRadius: 999 },
  dotActive: { backgroundColor: Colors.primary, width: 22 },
  dotIdle: { backgroundColor: "#CBD5E1" },

  actions: {
    flexDirection: "row",
    gap: 12,
    paddingHorizontal: 18,
    marginTop: 6,
  },
  btnOutline: {
    flex: 1,
    height: 54,
    borderRadius: 999,
    borderWidth: 1,
    borderColor: Colors.border,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: "#FFFFFF",
  },
  btnOutlineText: { fontWeight: "900", fontSize: 16, color: Colors.text },

  btnPrimary: {
    flex: 1,
    height: 54,
    borderRadius: 999,
    alignItems: "center",
    justifyContent: "center",
    backgroundColor: Colors.primary,
  },
  btnPrimaryText: { fontWeight: "900", fontSize: 16, color: "#FFFFFF" },
});
