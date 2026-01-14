import AsyncStorage from "@react-native-async-storage/async-storage";

export const SessionKeys = {
  accessToken: "ganipay.accessToken",
  refreshToken: "ganipay.refreshToken",
  user: "ganipay.user",
  customerId: "ganipay.customerId",
  accountId: "ganipay.accountId",
  currency: "ganipay.currency",
} as const;

export async function clearSession() {
  await AsyncStorage.multiRemove(Object.values(SessionKeys));
}
