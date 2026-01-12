export type Segment = "STANDARD" | "CORPORATE";
export type AddressType = "HOME";

export type RegisterForm = {
  // Step 1 (Account)
  firstName: string;
  lastName: string;
  segment: Segment;
  password: string;

  // Step 2 (Contact)
  phoneNumber: string;
  email: string;
  nationality: string; // default TR

  // Step 3 (Address)
  addressType: AddressType; // default HOME
  city: string;
  district: string;
  postalCode: string;
  addressLine1: string;

  // Hidden/default
  identityNumber: string;
  currency: string; // default TRY
};

export type RegisterPayload = {
  firstName: string;
  lastName: string;
  birthDate: string; // auto
  nationality: string;
  identityNumber: string;
  segment: Segment;
  phoneNumber: string;
  password: string;
  email: string;
  address: {
    addressType: AddressType;
    city: string;
    district: string;
    postalCode: string;
    addressLine1: string;
  };
  currency: string;
};

export type StepKey = "account" | "contact" | "address" | "review";

export const REGISTER_STEPS: Array<{ key: StepKey; label: string; desc: string; icon: string }> = [
  { key: "account", label: "Account", desc: "Create your wallet profile.", icon: "👤" },
  { key: "contact", label: "Contact", desc: "Enter your contact details.", icon: "📞" },
  { key: "address", label: "Address", desc: "Add your address information.", icon: "📍" },
  { key: "review", label: "Review", desc: "Confirm details before creating.", icon: "🛡️" },
];

export const defaults: RegisterForm = {
  firstName: "",
  lastName: "",
  segment: "STANDARD",
  password: "",

  phoneNumber: "",
  email: "",
  nationality: "TR",

  addressType: "HOME",
  city: "",
  district: "",
  postalCode: "",
  addressLine1: "",

  identityNumber: "",
  currency: "TRY",
};

export function digitsOnly(value: string) {
  return value.replace(/[^\d]/g, "");
}

export function isEmail(value: string) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim());
}

export function buildRegisterPayload(form: RegisterForm): RegisterPayload {
  // ✅ birthDate: kullanıcıdan alınmayacak. Şimdilik sabit.
  const birthDateAuto = "2000-01-15";

  return {
    firstName: form.firstName.trim(),
    lastName: form.lastName.trim(),
    birthDate: birthDateAuto,
    nationality: (form.nationality.trim() || "TR").toUpperCase(),
    identityNumber: form.identityNumber.trim(),
    segment: form.segment,
    phoneNumber: form.phoneNumber.trim(),
    password: form.password,
    email: form.email.trim().toLowerCase(),
    address: {
      addressType: form.addressType,
      city: form.city.trim(),
      district: form.district.trim(),
      postalCode: form.postalCode.trim(),
      addressLine1: form.addressLine1.trim(),
    },
    currency: (form.currency.trim() || "TRY").toUpperCase(),
  };
}

export function validateStep(step: StepKey, form: RegisterForm): string | null {
  if (step === "account") {
    if (form.firstName.trim().length < 2) return "First name is required.";
    if (form.lastName.trim().length < 2) return "Last name is required.";
    if (!form.identityNumber || digitsOnly(form.identityNumber).length < 10)
      return "Identity number looks invalid.";
    if (form.password.length < 6) return "Password must be at least 6 characters.";
  }

  if (step === "contact") {
    if (digitsOnly(form.phoneNumber).length < 10) return "Phone number looks invalid.";
    if (!isEmail(form.email)) return "Please enter a valid email address.";
    if (!form.nationality.trim()) return "Nationality is required.";
  }

  if (step === "address") {
    if (!form.city.trim()) return "City is required.";
    if (!form.district.trim()) return "District is required.";
    if (!form.postalCode.trim()) return "Postal code is required.";
    if (!form.addressLine1.trim()) return "Address line is required.";
  }

  return null;
}
