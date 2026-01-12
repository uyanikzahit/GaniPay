import React, { useMemo, useRef, useState } from "react";
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
  image?: any;
  emoji: string;
};

const GOLD = "rgba(246,195,64,1)";
const GOLD_DARK = "rgba(214,160,35,1)";

export default function WelcomeScreen() {
  const router = useRouter();
  const { width } = useWindowDimensions();
  const [index, setIndex] = useState(0);
  const listRef = useRef<FlatList<Slide>>(null);

  const slides: Slide[] = useMemo(
    () => [
      {
        key: "s1",
        title: "Secure wallet, built for speed",
        subtitle: "Your money is protected with bank-grade security, smart verification, and encrypted sessions. Move confidently—every action is designed to be fast, clear, and safe, so you always know what’s happening in your wallet.",
        emoji: "🛡️",
      },
      {
        key: "s2",
        title: "Instant top-ups",
        subtitle: "Add funds in seconds and follow each step with transparent status updates. Whether you top up once or regularly, GaniPay keeps every movement traceable—so you stay in control of your balance, anytime.",
        emoji: "⚡",
      },
      {
        key: "s3",
        title: "Easy transfers",
        subtitle: "Send money wallet-to-wallet with a smooth, premium flow. Clear confirmations, trackable results, and security checks behind the scenes—so transfers feel effortless while staying protected end-to-end.",
        emoji: "🔁",
      },
    ],
    []
  );

  const viewabilityConfig = useRef({ viewAreaCoveragePercentThreshold: 60 });
  const onViewableItemsChanged = useRef(
    ({ viewableItems }: { viewableItems: Array<ViewToken> }) => {
      const first = viewableItems?.[0];
      if (first?.index != null) setIndex(first.index);
    }
  );

  // ✅ Ekran padding’iyle uyumlu gerçek içerik genişliği
  const contentWidth = Math.max(0, width - 36);

  return (
    <View style={styles.container}>
      <LinearGradient
        colors={[Colors.bg, "#0B1220", "#12223E"]}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 1 }}
        style={styles.hero}
      >
        {/* Gold highlight */}
        <LinearGradient
          colors={["rgba(246,195,64,0.85)", "rgba(246,195,64,0)"]}
          start={{ x: 0.2, y: 0 }}
          end={{ x: 0.8, y: 1 }}
          style={styles.goldGlow}
        />

        {/* Brand (aşağı alındı) */}
        <View style={styles.brandRow}>
          <View style={styles.brandChip}>
            <Image
              source={require("../../assets/images/ganipay-logo.png")}
              style={styles.logo}
              resizeMode="contain"
            />
          </View>

          <View style={{ flex: 1 }}>
            <Text style={styles.brandSub}>Premium wallet experience</Text>
          </View>
        </View>

        {/* Slider */}
        <View style={styles.sliderWrap}>
          <FlatList
            ref={listRef}
            data={slides}
            keyExtractor={(item) => item.key}
            horizontal
            pagingEnabled
            showsHorizontalScrollIndicator={false}
            viewabilityConfig={viewabilityConfig.current}
            onViewableItemsChanged={onViewableItemsChanged.current}
            // ✅ Padding’i FlatList’e veriyoruz, ortalama tam oturuyor
            contentContainerStyle={styles.listContent}
            renderItem={({ item }) => (
              // ✅ Slide genişliği padding düşülmüş hali (ortada kalır)
              <View style={[styles.slide, { width: contentWidth }]}>
                <View style={styles.glassCard}>
                  <View style={styles.iconRing}>
                    <LinearGradient
                      colors={[
                        "rgba(255,255,255,0.20)",
                        "rgba(255,255,255,0.06)",
                      ]}
                      start={{ x: 0, y: 0 }}
                      end={{ x: 1, y: 1 }}
                      style={styles.iconRingInner}
                    >
                      {item.image ? (
                        <Image
                          source={item.image}
                          style={styles.featureIcon}
                          resizeMode="contain"
                        />
                      ) : (
                        <Text style={styles.featureEmoji}>{item.emoji}</Text>
                      )}
                    </LinearGradient>
                  </View>

                  {/* ✅ biraz küçültüldü */}
                  <Text style={styles.title}>{item.title}</Text>
                  <Text style={styles.subtitle}>{item.subtitle}</Text>
                </View>
              </View>
            )}
          />
        </View>

        {/* Dots */}
        <View style={styles.dotsRow}>
          {slides.map((_, i) => (
            <View
              key={i}
              style={[styles.dot, i === index ? styles.dotActive : styles.dotIdle]}
            />
          ))}
        </View>

        {/* Actions */}
        <View style={styles.actions}>
          <Pressable
            style={({ pressed }) => [styles.btnGhost, pressed && { opacity: 0.92 }]}
            onPress={() => router.push("/(auth)/login")}
          >
            <Text style={styles.btnGhostText}>Log In</Text>
          </Pressable>

          <Pressable
            style={({ pressed }) => [
              styles.btnPrimary,
              pressed && { opacity: 0.92 },
            ]}
            onPress={() => router.push("/(auth)/register")}
          >
            <Text style={styles.btnPrimaryText}>Sign Up</Text>
          </Pressable>
        </View>

        <View style={{ height: 14 }} />
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

  // ✅ Brand aşağı alındı (mobile’da daha iyi)
  brandRow: {
    flexDirection: "row",
    alignItems: "center",
    gap: 12,
    marginTop: 18, // eskiden 6 idi
    marginBottom: 10,
  },

  brandChip: {
    width: 72,
    height: 72,
    borderRadius: 18,
    backgroundColor: "transparent",
    borderWidth: 0,
    alignItems: "center",
    justifyContent: "center",
  },

  // ✅ Logo daha makul (120 çok büyüktü)
  logo: {
    width: 120,
    height: 120,
  },

  brandSub: {
    color: "rgba(255,255,255,0.75)",
    fontSize: 12.5,
    marginTop: 2,
  },

  sliderWrap: {
    flex: 1,
    justifyContent: "flex-start",
    marginTop: 8,
  },

  // ✅ FlatList padding burada yönetiliyor (ortalamayı düzeltir)
  listContent: {
    paddingHorizontal: 0,
  },

  // ✅ Slide artık içerik genişliğinde (width-36) ve tam ortada
  slide: {
    alignItems: "center",
    justifyContent: "flex-start",
  },

  // ✅ Yazı kutusu biraz küçültüldü (daha kompakt)
  glassCard: {
    width: "100%",
    borderRadius: 28,
    paddingVertical: 22, // 28 -> 22
    paddingHorizontal: 16, // 18 -> 16
    backgroundColor: "rgba(255,255,255,0.10)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.16)",
    minHeight: 460, // 520 -> 460 (daha kompakt)
    justifyContent: "flex-start",
    ...shadow,
  },

  // ✅ Icon biraz küçült
  iconRing: {
    alignSelf: "center",
    width: 96, // 112 -> 96
    height: 96,
    borderRadius: 28,
    backgroundColor: "rgba(246,195,64,0.18)",
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.22)",
    alignItems: "center",
    justifyContent: "center",
    marginBottom: 14,
    marginTop: 6,
  },
  iconRingInner: {
    width: 76, // 88 -> 76
    height: 76,
    borderRadius: 22,
    alignItems: "center",
    justifyContent: "center",
  },

  featureIcon: { width: 48, height: 48 },
  featureEmoji: { fontSize: 36 }, // 42 -> 36

  // ✅ Başlık/alt yazı biraz küçült
  title: {
    color: "rgba(255,255,255,0.96)",
    fontSize: 24, // 28 -> 24
    fontWeight: "900",
    textAlign: "center",
    marginTop: 2,
    marginBottom: 10,
    lineHeight: 30,
  },
  subtitle: {
    color: "rgba(255,255,255,0.75)",
    fontSize: 14.5, // 15.6 -> 14.5
    lineHeight: 21,
    textAlign: "center",
    paddingHorizontal: 6,
  },

  dotsRow: {
    flexDirection: "row",
    justifyContent: "center",
    gap: 8,
    marginTop: 14,
    marginBottom: 12,
  },
  dot: { width: 8, height: 8, borderRadius: 999 },
  dotActive: { backgroundColor: GOLD, width: 22 },
  dotIdle: { backgroundColor: "rgba(255,255,255,0.28)" },

  actions: {
    flexDirection: "row",
    gap: 12,
    marginTop: 2,
  },

  btnGhost: {
    flex: 1,
    height: 56,
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
    height: 56,
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
