import React from "react";
import { View, Text, StyleSheet, TextInput, Pressable } from "react-native";
import { LinearGradient } from "expo-linear-gradient";
import { Colors } from "../theme/colors";
import type { RegisterForm, Segment } from "./register.types";
import { digitsOnly } from "./register.types";

const GOLD = "rgba(246,195,64,1)";

type Props = {
  form: RegisterForm;
  setForm: React.Dispatch<React.SetStateAction<RegisterForm>>;
};

function Field(props: {
  label: string;
  value: string;
  onChangeText?: (v: string) => void;
  placeholder?: string;
  keyboardType?: any;
  secureTextEntry?: boolean;
  autoCapitalize?: any;
  editable?: boolean;
  maxLength?: number;
}) {
  const {
    label,
    value,
    onChangeText,
    placeholder,
    keyboardType,
    secureTextEntry,
    autoCapitalize,
    editable = true,
    maxLength,
  } = props;

  return (
    <View style={s.field}>
      <Text style={s.label}>{label}</Text>
      <TextInput
        value={value}
        onChangeText={onChangeText}
        placeholder={placeholder}
        placeholderTextColor="rgba(255,255,255,0.35)"
        keyboardType={keyboardType}
        secureTextEntry={secureTextEntry}
        autoCapitalize={autoCapitalize}
        editable={editable}
        maxLength={maxLength}
        style={[s.input, !editable && { opacity: 0.7 }]}
      />
    </View>
  );
}

export function StepAccount({ form, setForm }: Props) {
  const set = (k: keyof RegisterForm, v: any) => setForm((p) => ({ ...p, [k]: v }));

  return (
    <View style={s.card}>
      <Text style={s.title}>Create your account</Text>
      <Text style={s.subTitle}>
        Start with your basics. Security comes standard.
      </Text>

      <View style={s.segmentRow}>
        <SegmentPill
          active={form.segment === "STANDARD"}
          text="Individual"
          onPress={() => set("segment", "STANDARD")}
        />
        <SegmentPill
          active={form.segment === "CORPORATE"}
          text="Corporate"
          onPress={() => set("segment", "CORPORATE")}
        />
      </View>

      <View style={s.row2}>
        <Field
          label="First name"
          value={form.firstName}
          onChangeText={(v) => set("firstName", v)}
          placeholder="Mehmet"
        />
        <Field
          label="Last name"
          value={form.lastName}
          onChangeText={(v) => set("lastName", v)}
          placeholder="Zahit"
        />
      </View>

      <Field
        label="Identity number"
        value={form.identityNumber}
        onChangeText={(v) => set("identityNumber", digitsOnly(v))}
        placeholder="11111187111"
        keyboardType="number-pad"
        maxLength={11}
      />

      <Field
        label="Password"
        value={form.password}
        onChangeText={(v) => set("password", v)}
        placeholder="••••••••"
        secureTextEntry
      />

      <Text style={s.hint}>
        Birth date is set automatically during registration.
      </Text>
    </View>
  );
}

export function StepContact({ form, setForm }: Props) {
  const set = (k: keyof RegisterForm, v: any) => setForm((p) => ({ ...p, [k]: v }));

  return (
    <View style={s.card}>
      <Text style={s.title}>Contact details</Text>
      <Text style={s.subTitle}>
        We’ll use this to secure your access and keep you updated.
      </Text>

      <Field
        label="Phone number"
        value={form.phoneNumber}
        onChangeText={(v) => set("phoneNumber", digitsOnly(v))}
        placeholder="50628368000"
        keyboardType="phone-pad"
        maxLength={12}
      />

      <Field
        label="Email"
        value={form.email}
        onChangeText={(v) => set("email", v)}
        placeholder="zahit.test@ganipay.io"
        keyboardType="email-address"
        autoCapitalize="none"
      />

      <Field
        label="Nationality"
        value={form.nationality}
        onChangeText={(v) => set("nationality", v.toUpperCase())}
        placeholder="TR"
        maxLength={2}
      />
    </View>
  );
}

export function StepAddress({ form, setForm }: Props) {
  const set = (k: keyof RegisterForm, v: any) => setForm((p) => ({ ...p, [k]: v }));

  return (
    <View style={s.card}>
      <Text style={s.title}>Address</Text>
      <Text style={s.subTitle}>
        Needed for compliance and account verification.
      </Text>

      <View style={s.row2}>
        <Field
          label="City"
          value={form.city}
          onChangeText={(v) => set("city", v)}
          placeholder="Istanbul"
        />
        <Field
          label="District"
          value={form.district}
          onChangeText={(v) => set("district", v)}
          placeholder="Kadikoy"
        />
      </View>

      <View style={s.row2}>
        <Field
          label="Postal code"
          value={form.postalCode}
          onChangeText={(v) => set("postalCode", v)}
          placeholder="34710"
          keyboardType="number-pad"
        />
        <Field label="Address type" value="HOME" editable={false} />
      </View>

      <Field
        label="Address line"
        value={form.addressLine1}
        onChangeText={(v) => set("addressLine1", v)}
        placeholder="Test Mah. Test Sok. No:1"
      />

      <View style={s.row2}>
        <Field
          label="Currency"
          value={form.currency}
          onChangeText={(v) => set("currency", v.toUpperCase())}
          placeholder="TRY"
          maxLength={3}
        />
        <View style={{ flex: 1 }} />
      </View>
    </View>
  );
}

export function StepReview({ form }: { form: RegisterForm }) {
  const summary = [
    { k: "Name", v: `${form.firstName} ${form.lastName}`.trim() },
    { k: "Segment", v: form.segment },
    { k: "Phone", v: form.phoneNumber },
    { k: "Email", v: form.email },
    { k: "Nationality", v: form.nationality },
    { k: "City", v: form.city },
    { k: "District", v: form.district },
    { k: "Postal code", v: form.postalCode },
    { k: "Address", v: form.addressLine1 },
    { k: "Currency", v: form.currency },
  ];

  return (
    <View style={s.card}>
      <Text style={s.title}>Review</Text>
      <Text style={s.subTitle}>
        Please confirm your details before creating your account.
      </Text>

      <View style={s.summaryBox}>
        {summary.map((it) => (
          <View key={it.k} style={s.summaryRow}>
            <Text style={s.summaryKey}>{it.k}</Text>
            <Text style={s.summaryVal} numberOfLines={2}>
              {it.v || "-"}
            </Text>
          </View>
        ))}
      </View>

      <Text style={s.hint}>
        By creating an account, you’ll continue to onboarding (KYC & agreements).
      </Text>
    </View>
  );
}

function SegmentPill(props: { active: boolean; text: string; onPress: () => void }) {
  return (
    <Pressable onPress={props.onPress} style={({ pressed }) => [
      s.segmentPill,
      props.active && s.segmentPillActive,
      pressed && { opacity: 0.92 },
    ]}>
      <Text style={[s.segmentText, props.active && s.segmentTextActive]}>
        {props.text}
      </Text>
    </Pressable>
  );
}

const s = StyleSheet.create({
  card: {
    borderRadius: 28,
    padding: 16,
    backgroundColor: "rgba(255,255,255,0.10)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.16)",
  },

  title: {
    color: "rgba(255,255,255,0.96)",
    fontSize: 22,
    fontWeight: "900",
  },
  subTitle: {
    color: "rgba(255,255,255,0.70)",
    fontSize: 13,
    marginTop: 6,
    marginBottom: 14,
    lineHeight: 18,
  },

  segmentRow: { flexDirection: "row", gap: 10, marginBottom: 10 },
  segmentPill: {
    flex: 1,
    height: 44,
    borderRadius: 999,
    borderWidth: 1,
    borderColor: "rgba(246,195,64,0.25)",
    backgroundColor: "rgba(246,195,64,0.08)",
    alignItems: "center",
    justifyContent: "center",
  },
  segmentPillActive: {
    backgroundColor: GOLD,
    borderColor: "rgba(255,255,255,0.12)",
  },
  segmentText: { color: "rgba(255,255,255,0.86)", fontWeight: "900" },
  segmentTextActive: { color: "#111827" },

  row2: { flexDirection: "row", gap: 10 },

  field: { flex: 1, marginBottom: 12 },
  label: { color: "rgba(255,255,255,0.70)", fontSize: 12, marginBottom: 6 },
  input: {
    height: 48,
    borderRadius: 16,
    paddingHorizontal: 12,
    color: "rgba(255,255,255,0.92)",
    backgroundColor: "rgba(255,255,255,0.08)",
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.14)",
  },

  hint: {
    color: "rgba(255,255,255,0.55)",
    fontSize: 12,
    lineHeight: 18,
    marginTop: 6,
  },

  summaryBox: {
    marginTop: 6,
    borderRadius: 18,
    borderWidth: 1,
    borderColor: "rgba(255,255,255,0.14)",
    backgroundColor: "rgba(255,255,255,0.06)",
    padding: 12,
  },
  summaryRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    gap: 12,
    paddingVertical: 6,
  },
  summaryKey: { color: "rgba(255,255,255,0.65)", fontSize: 12 },
  summaryVal: { color: "rgba(255,255,255,0.92)", fontSize: 12, fontWeight: "800", flex: 1, textAlign: "right" },
});
