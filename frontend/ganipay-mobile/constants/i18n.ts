// constants/i18n.ts
export type Lang = "EN" | "TR";

type Dict = Record<string, string>;

const en: Dict = {
  // MENU
  "menu.title": "Menu",
  "menu.darkMode": "Dark mode",
  "menu.language": "Language",
  "menu.profile": "Profile details",
  "menu.profile.sub": "Personal info, verification status",
  "menu.wallet": "Wallet",
  "menu.wallet.sub": "Funding sources & balances",
  "menu.topup": "Top up",
  "menu.topup.sub": "Add money to your wallet",
  "menu.transfer": "Transfer",
  "menu.transfer.sub": "Send money instantly",
  "menu.limits": "Spending limits",
  "menu.limits.sub": "Daily/Monthly limits, rules",
  "menu.security": "Security",
  "menu.security.sub": "PIN, biometrics, sign-in options",
  "menu.notifications": "Notifications",
  "menu.notifications.sub": "Alerts & preferences",
  "menu.paybills": "Pay bills",
  "menu.paybills.sub": "Utilities & subscriptions",
  "menu.partners": "Partner stores",
  "menu.partners.sub": "Deals & supported merchants",
  "menu.legal": "Legal",
  "menu.legal.sub": "Terms, privacy policy",
  "menu.about": "About GaniPay",
  "menu.about.sub": "Version, build info",
  "menu.logout": "Log out",
  "menu.logout.sub": "End this session",

  "tabs.home": "Home",
  "tabs.topup": "Top Up",
  "tabs.transfer": "Transfer",
  "tabs.account": "Account",

  // HOME (index.tsx’e göre tamamlayacağız)
  "home.welcome": "Welcome,",
  "home.quickActions": "Quick actions",
  "home.lastTransactions": "Last Transactions",
  "home.viewAll": "View all",
};

const tr: Dict = {
  // MENU
  "menu.title": "Menü",
  "menu.darkMode": "Karanlık mod",
  "menu.language": "Dil",
  "menu.profile": "Profil bilgileri",
  "menu.profile.sub": "Kişisel bilgiler, doğrulama durumu",
  "menu.wallet": "Cüzdan",
  "menu.wallet.sub": "Kaynaklar & bakiyeler",
  "menu.topup": "Para yükle",
  "menu.topup.sub": "Cüzdanına para ekle",
  "menu.transfer": "Transfer",
  "menu.transfer.sub": "Anında para gönder",
  "menu.limits": "Harcamа limitleri",
  "menu.limits.sub": "Günlük/Aylık limitler, kurallar",
  "menu.security": "Güvenlik",
  "menu.security.sub": "PIN, biyometri, giriş seçenekleri",
  "menu.notifications": "Bildirimler",
  "menu.notifications.sub": "Uyarılar & tercihler",
  "menu.paybills": "Fatura öde",
  "menu.paybills.sub": "Abonelikler & faturalar",
  "menu.partners": "Partner mağazalar",
  "menu.partners.sub": "Kampanyalar & anlaşmalı iş yerleri",
  "menu.legal": "Yasal",
  "menu.legal.sub": "Şartlar, gizlilik politikası",
  "menu.about": "GaniPay hakkında",
  "menu.about.sub": "Sürüm, yapı bilgisi",
  "menu.logout": "Çıkış yap",
  "menu.logout.sub": "Oturumu sonlandır",

  // HOME (index.tsx’e göre tamamlayacağız)
  "home.welcome": "Hoş geldin,",
  "home.quickActions": "Hızlı işlemler",
  "home.lastTransactions": "Son işlemler",
  "home.viewAll": "Tümünü gör",

  "tabs.home": "Ana Sayfa",
  "tabs.topup": "Para Yükle",
  "tabs.transfer": "Transfer",
  "tabs.account": "Hesap",
};

export function t(lang: Lang, key: string) {
  const table = lang === "TR" ? tr : en;
  return table[key] ?? key; // eksikse key gösterir (debug için iyi)
}
