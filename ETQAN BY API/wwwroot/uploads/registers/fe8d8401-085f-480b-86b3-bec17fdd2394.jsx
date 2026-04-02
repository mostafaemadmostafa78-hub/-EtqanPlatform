import React, { useState, useRef, useEffect } from "react";
import "./CraftsmanRegister.css";
import { FiEye, FiEyeOff } from "react-icons/fi";
import { motion } from "framer-motion";
import { Link, useNavigate } from "react-router-dom";
import axios from "axios";

// ========================
// Axios Instance
// ========================
const api = axios.create({
  baseURL: "https://localhost:7000", // ← غيّر البورت حسب بيئتك
  headers: { "Content-Type": "application/json" },
});

// ========================
// خريطة المهن: الاسم → JobId  (مطابق لـ Jobs table في الـ DB)
// ========================
const jobsMap = [
  { id: 1,  name: "حداد" },
  { id: 2,  name: "نجارة" },
  { id: 3,  name: "فني تكييفات" },
  { id: 4,  name: "سباكة" },
  { id: 5,  name: "كهرباء" },
  { id: 6,  name: "سيراميك" },
  { id: 7,  name: "فني كاميرات" },
  { id: 8,  name: "عامل بناء" },
  { id: 9,  name: "نقاش" },
  { id: 10, name: "فني غاز" },
  { id: 11, name: "سواق نقل" },
  { id: 12, name: "تكسير وإزالة" },
  { id: 13, name: "الومنتال" },
  { id: 14, name: "منجد" },
  { id: 15, name: "أمن وأنظمة ذكية" },
  { id: 16, name: "محارة" },
  { id: 17, name: "تنظيف" },
  { id: 18, name: "استشارات هندسية" },
  { id: 19, name: "رش مبيدات" },
  { id: 20, name: "صيانة اجهزة كهربائية" },
  { id: 21, name: "فني تركيب دش" },
];

// ========================
// خريطة الحالة الاجتماعية → رقم MaritalStatus Enum في الـ Backend
// ========================
const maritalMap = {
  "أعزب":  0,
  "متزوج": 1,
  "مطلق":  2,
  "أرمل":  3,
};

const maritalList = Object.keys(maritalMap);

function CraftsmanRegister() {
  const navigate = useNavigate();

  const jobDropdownRef     = useRef(null);
  const maritalDropdownRef = useRef(null);

  // ── States ──
  const [username,        setUsername]        = useState("");
  const [email,           setEmail]           = useState("");
  const [age,             setAge]             = useState("");
  const [maritalStatus,   setMaritalStatus]   = useState("");
  const [nationalId,      setNationalId]      = useState("");
  const [phone,           setPhone]           = useState("");
  const [selectedJob,     setSelectedJob]     = useState(null); // { id, name }
  const [jobSearchTerm,   setJobSearchTerm]   = useState("");

  const [password,        setPassword]        = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  const [emailError,          setEmailError]          = useState("");
  const [showPassword,        setShowPassword]        = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [showJobDropdown,     setShowJobDropdown]     = useState(false);
  const [showMarital,         setShowMarital]         = useState(false);
  const [isFocused,           setIsFocused]           = useState(false);

  // حالة الـ API
  const [isLoading,  setIsLoading]  = useState(false);
  const [apiError,   setApiError]   = useState("");
  const [apiSuccess, setApiSuccess] = useState("");

  // ── إغلاق الـ Dropdowns عند الضغط بره ──
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (jobDropdownRef.current && !jobDropdownRef.current.contains(event.target))
        setShowJobDropdown(false);
      if (maritalDropdownRef.current && !maritalDropdownRef.current.contains(event.target))
        setShowMarital(false);
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const filteredJobs = jobsMap.filter((j) => j.name.includes(jobSearchTerm));

  // ── Handlers ──
  const handleUsernameChange = (e) => {
    const val = e.target.value;
    if (/^[a-zA-Z\u0600-\u06FF\s]*$/.test(val)) setUsername(val);
  };

  const handleEmailChange = (e) => {
    const val = e.target.value.trim();
    setEmail(val);
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    setEmailError(val && !emailRegex.test(val) ? "البريد الإلكتروني غير صالح" : "");
  };

  const handleNationalIdChange = (e) => {
    const val = e.target.value;
    if (/^\d*$/.test(val) && val.length <= 14) setNationalId(val);
  };

  const handlePhoneChange = (e) => {
    const val = e.target.value;
    if (!/^\d*$/.test(val) || val.length > 11) return;
    setPhone(val);
  };

  // ── Password rules ──
  const rules = {
    firstCapital: /^[A-Z]/,
    specialChar:  /[!@#$%^&*()/\\]/,
    minLength:    /.{8,}/,
  };
  const checkRule = (rule) => rule.test(password);

  // ── Validation ──
  const isPhoneValid =
    phone.length === 11 &&
    phone[0] === "0" &&
    ["010", "011", "012", "015"].includes(phone.substring(0, 3));

  const passwordsNotMatch = confirmPassword.length > 0 && password !== confirmPassword;

  const isFormValid =
    username.trim()   !== "" &&
    email.trim()      !== "" &&
    emailError        === "" &&
    age               !== "" &&
    Number(age)       >= 18  &&
    maritalStatus     !== "" &&
    nationalId.length === 14 &&
    selectedJob       !== null &&
    isPhoneValid &&
    password          !== "" &&
    confirmPassword   !== "" &&
    password          === confirmPassword &&
    checkRule(rules.firstCapital) &&
    checkRule(rules.specialChar)  &&
    checkRule(rules.minLength);

  // ── Submit → Axios POST ──
  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!isFormValid) return;

    setIsLoading(true);
    setApiError("");
    setApiSuccess("");

    // مطابق تماماً لـ RegisterArtisanDto
    const payload = {
      fullname:      username.trim(),            // dto.Fullname
      email:         email.trim(),               // dto.Email
      phoneNumber:   phone,                      // dto.phoneNumber
      age:           Number(age),                // dto.Age
      maritalStatus: maritalMap[maritalStatus],  // dto.MaritalStatus → رقم Enum
      nationalId:    nationalId,                 // dto.NationalId
      jobId:         selectedJob.id,             // dto.JobId
      password:      password,                   // dto.Password
    };

    console.log("📤 Payload المرسل:", payload);

    try {
      const { data } = await api.post("/api/ArtisanAccount/register", payload);

      // ✅ نجاح
      setApiSuccess(data.message || "تم إنشاء الحساب بنجاح");
      setTimeout(() => navigate("/CompanyOTP", { state: { userId: data.userId } }), 1500);

    } catch (err) {
      console.error("❌ Register error:", err);

      if (err.response) {
        const body = err.response.data;
        console.error("📥 Response من الـ Backend:", body);

        let errorMsg = "حدث خطأ أثناء التسجيل";
        if (body?.message) {
          errorMsg = body.message;
        } else if (body?.errors) {
          errorMsg = Object.values(body.errors).flat().join(" | ");
        } else if (typeof body === "string") {
          errorMsg = body;
        }

        setApiError(`(${err.response.status}) ${errorMsg}`);
      } else if (err.request) {
        setApiError("تعذّر الاتصال بالخادم — تأكد أن الـ Backend شغّال وأن CORS مفعّل");
      } else {
        setApiError(`خطأ غير متوقع: ${err.message}`);
      }
    } finally {
      setIsLoading(false);
    }
  };

  // ─────────────────────────────────────────────
  return (
    <div className="craftsman-page-container">

      <motion.div
        className="form-section"
        initial={{ y: 60, opacity: 0 }}
        animate={{ y: 0, opacity: 1 }}
        transition={{ duration: 1, ease: "easeOut", delay: 0.4 }}
      >
        <div className="form-fields">

          <form onSubmit={handleSubmit}>

            <h1>مرحباً بك</h1>
            <h3>قم بإنشاء حسابك لبدء استخدام الخدمة</h3>

            {/* رسائل الـ API */}
            {apiError   && <p className="error-msg api-error">{apiError}</p>}
            {apiSuccess && <p className="success-msg">{apiSuccess}</p>}

            <div className="fields-row">

              {/* اسم المستخدم */}
              <div className="field-container">
                <input
                  type="text"
                  placeholder="اسم المستخدم"
                  value={username}
                  onChange={handleUsernameChange}
                />
              </div>

              {/* البريد الإلكتروني */}
              <div className="field-container">
                <input
                  type="email"
                  placeholder="البريد الإلكتروني"
                  value={email}
                  onChange={handleEmailChange}
                />
                {emailError && <p className="error-msg">{emailError}</p>}
              </div>

              {/* العمر */}
              <div className="field-container">
                <input
                  type="number"
                  placeholder="العمر"
                  value={age}
                  onChange={(e) => setAge(e.target.value)}
                  min="18"
                />
                {age && Number(age) < 18 && (
                  <p className="error-msg">يجب ألا يقل العمر عن 18 سنة</p>
                )}
              </div>

              {/* الحالة الاجتماعية */}
              <div className="field-container marital-dropdown" ref={maritalDropdownRef}>
                <input
                  type="text"
                  placeholder="الحالة الاجتماعية"
                  value={maritalStatus}
                  readOnly
                  onClick={() => setShowMarital(!showMarital)}
                />
                {showMarital && (
                  <ul className="dropdown-list">
                    {maritalList.map((item, index) => (
                      <li
                        key={index}
                        onClick={() => { setMaritalStatus(item); setShowMarital(false); }}
                      >
                        {item}
                      </li>
                    ))}
                  </ul>
                )}
              </div>

              {/* الرقم القومي */}
              <div className="field-container">
                <input
                  type="text"
                  placeholder="الرقم القومي"
                  value={nationalId}
                  onChange={handleNationalIdChange}
                />
                {nationalId.length > 0 && nationalId.length < 14 && (
                  <p className="error-msg">الرقم القومي يجب أن يكون 14 رقم</p>
                )}
              </div>

              {/* المهنة */}
              <div className="field-container" ref={jobDropdownRef}>
                <input
                  type="text"
                  placeholder="المهنة"
                  value={selectedJob ? selectedJob.name : jobSearchTerm}
                  onChange={(e) => {
                    setJobSearchTerm(e.target.value);
                    setSelectedJob(null);
                    setShowJobDropdown(true);
                  }}
                  onFocus={() => setShowJobDropdown(true)}
                />
                {showJobDropdown && jobSearchTerm && (
                  <ul className="dropdown-list">
                    {filteredJobs.length > 0 ? (
                      filteredJobs.map((j) => (
                        <li
                          key={j.id}
                          onClick={() => {
                            setSelectedJob(j);
                            setJobSearchTerm("");
                            setShowJobDropdown(false);
                          }}
                        >
                          {j.name}
                        </li>
                      ))
                    ) : (
                      <li className="no-result">لا توجد نتائج</li>
                    )}
                  </ul>
                )}
              </div>

              {/* رقم الهاتف */}
              <div className="field-container phone-field">
                <input
                  type="text"
                  placeholder="رقم الهاتف"
                  value={phone}
                  onChange={handlePhoneChange}
                  required
                />
                {((phone.length > 0 && phone.length < 11) ||
                  phone[0] !== "0" ||
                  (phone.length >= 3 &&
                    !["010", "011", "012", "015"].includes(phone.substring(0, 3)))) && (
                  <ul className="phone-errors">
                    {phone.length > 0 && phone.length < 11 && (
                      <li>رقم الهاتف يجب أن يكون 11 رقم</li>
                    )}
                    {phone.length >= 1 && phone[0] !== "0" && (
                      <li>رقم الهاتف يجب أن يبدأ بالرقم 0</li>
                    )}
                    {phone.length >= 3 &&
                      !["010", "011", "012", "015"].includes(phone.substring(0, 3)) && (
                        <li>رقم الهاتف يجب أن يبدأ بـ 010 أو 011 أو 012 أو 015</li>
                    )}
                  </ul>
                )}
              </div>

              {/* كلمة السر */}
              <div className="field-container password-field-container" style={{ position: "relative" }}>
                <input
                  type={showPassword ? "text" : "password"}
                  placeholder="كلمة السر"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  onFocus={() => setIsFocused(true)}
                  onBlur={() => setIsFocused(false)}
                />
                <span className="eye" onClick={() => setShowPassword(!showPassword)}>
                  {showPassword ? <FiEye /> : <FiEyeOff />}
                </span>
                {isFocused && (
                  <div className="password-rules" style={{ position: "absolute", top: "100%", left: 0, width: "100%" }}>
                    <ul>
                      <li style={{ color: checkRule(rules.firstCapital) ? "rgb(114,114,243)" : "rgb(235,138,138)" }}>
                        يجب ان يكون أول حرف Capital
                      </li>
                      <li style={{ color: checkRule(rules.specialChar) ? "rgb(114,114,243)" : "rgb(235,138,138)" }}>
                        يجب أن يحتوي على !@#$%
                      </li>
                      <li style={{ color: checkRule(rules.minLength) ? "rgb(114,114,243)" : "rgb(235,138,138)" }}>
                        يجب ان يكون على الأقل 8 أحرف
                      </li>
                    </ul>
                  </div>
                )}
              </div>

              {/* تأكيد كلمة السر */}
              <div className="field-container confirm-password-field-container">
                <input
                  type={showConfirmPassword ? "text" : "password"}
                  placeholder="تأكيد كلمة السر"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                />
                <span className="eye" onClick={() => setShowConfirmPassword(!showConfirmPassword)}>
                  {showConfirmPassword ? <FiEye /> : <FiEyeOff />}
                </span>
                {passwordsNotMatch && (
                  <p className="password-error-msg">يجب أن تكون كلمة السر مطابقة</p>
                )}
              </div>

            </div>

            {/* زرار تسجيل */}
            <button
              type="submit"
              className={`link-button ${(!isFormValid || isLoading) ? "disabled" : ""}`}
              disabled={!isFormValid || isLoading}
              style={{ opacity: (!isFormValid || isLoading) ? 0.5 : 1 }}
            >
              {isLoading ? "جاري التسجيل..." : "تسجيل"}
            </button>

            <h4 className="h4-craftsman-login">
              هل لديك حساب ؟ <Link to="/Login" className="Link">تسجيل الدخول</Link>
            </h4>

          </form>
        </div>

        <div className="image">
          <motion.img
            src="/images/Frame 18.svg"
            initial={{ x: "20%", y: 0, opacity: 0 }}
            animate={{ x: 0, y: [0, -10, 0], opacity: 1 }}
            transition={{
              x:       { duration: 1.8, ease: "easeOut" },
              y:       { duration: 3, ease: "easeInOut", repeat: Infinity },
              opacity: { duration: 1.8, ease: "easeOut" },
            }}
          />
        </div>

      </motion.div>
    </div>
  );
}

export default CraftsmanRegister;
