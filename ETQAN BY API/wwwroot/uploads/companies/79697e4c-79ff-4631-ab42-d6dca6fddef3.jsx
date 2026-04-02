import React, { useState, useRef, useEffect } from "react";
import "./CraftsmanRegister.css";
import { FiEye, FiEyeOff } from "react-icons/fi";
import { motion } from "framer-motion";
import { Link, useNavigate } from "react-router-dom";

function CraftsmanRegister() {
  const navigate = useNavigate();

  const defaultJobs = [
    { id: 1,  name: "حداد" }, { id: 2,  name: "نجارة" }, { id: 3,  name: "فني تكييفات" },
    { id: 4,  name: "سباكة" }, { id: 5,  name: "كهرباء" }, { id: 6,  name: "سيراميك" },
    { id: 7,  name: "فني كاميرات" }, { id: 8,  name: "عامل بناء" }, { id: 9,  name: "نقاش" },
    { id: 10, name: "فني غاز" }, { id: 11, name: "سواق نقل" }, { id: 12, name: "تكسير وإزالة" },
    { id: 13, name: "الومنتال" }, { id: 14, name: "منجد" }, { id: 15, name: "أمن وأنظمة ذكية" },
    { id: 16, name: "محارة" }, { id: 17, name: "تنظيف" }, { id: 18, name: "استشارات هندسية" },
    { id: 19, name: "رش مبيدات" }, { id: 20, name: "صيانة اجهزة كهربائية" }, { id: 21, name: "فني تركيب دش" },
  ];

  const [jobsList, setJobsList] = useState(defaultJobs);

  useEffect(() => {
    fetch("http://localhost:5036/api/ArtisanAccount/GetJobs")
      .then((res) => res.json())
      .then((data) => setJobsList(data))
      .catch((err) => console.error("فشل جلب المهن، هيشتغل بالقائمة الافتراضية:", err));
  }, []);

  const maritalList = ["أعزب", "متزوج", "مطلق", "أرمل"];
  const maritalMap  = { "أعزب": 1, "متزوج": 2, "مطلق": 3, "أرمل": 4 };

  const jobDropdownRef     = useRef(null);
  const maritalDropdownRef = useRef(null);

  const [username,        setUsername]        = useState("");
  const [email,           setEmail]           = useState("");
  const [age,             setAge]             = useState("");
  const [maritalStatus,   setMaritalStatus]   = useState("");
  const [nationalId,      setNationalId]      = useState("");
  const [phone,           setPhone]           = useState("");
  const [selectedJob,     setSelectedJob]     = useState("");
  const [selectedJobId,   setSelectedJobId]   = useState(null);
  const [jobSearchTerm,   setJobSearchTerm]   = useState("");
  const [password,        setPassword]        = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [emailError,          setEmailError]          = useState("");
  const [showPassword,        setShowPassword]        = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [showJobDropdown, setShowJobDropdown] = useState(false);
  const [showMarital,     setShowMarital]     = useState(false);
  const [isFocused,       setIsFocused]       = useState(false);
  const [loading,         setLoading]         = useState(false);

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

  const filteredJobs = jobsList.filter((j) => j.name.includes(jobSearchTerm));

  const handleUsernameChange = (e) => {
    const val = e.target.value;
    if (/^[a-zA-Z\u0600-\u06FF\s]*$/.test(val)) setUsername(val);
  };

  const handleEmailChange = (e) => {
    const val = e.target.value.trim();
    setEmail(val);
    if (val && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val)) setEmailError("البريد الإلكتروني غير صالح");
    else setEmailError("");
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

  const rules = { firstCapital: /^[A-Z]/, specialChar: /[!@#$%^&*()/\\]/, minLength: /.{8,}/ };
  const checkRule = (rule) => rule.test(password);

  const passwordsNotMatch = confirmPassword.length > 0 && password !== confirmPassword;

  const isPhoneValid =
    phone.length === 11 && phone[0] === "0" &&
    ["010", "011", "012", "015"].includes(phone.substring(0, 3));

  const isFormValid =
    username.trim()    !== "" &&
    email.trim()       !== "" &&
    emailError         === "" &&
    age                !== "" &&
    Number(age)        >= 18  &&
    maritalStatus      !== "" &&
    nationalId.length  === 14 &&
    selectedJob.trim() !== "" &&
    isPhoneValid &&
    password           !== "" &&
    confirmPassword    !== "" &&
    password === confirmPassword &&
    checkRule(rules.minLength);

  const handleRegister = async (e) => {
    e.preventDefault();
    if (!isFormValid || loading) return;

    setLoading(true);

    const registerData = {
      fullname:      username,
      email:         email,
      password:      password,
      phoneNumber:   phone,
      age:           parseInt(age),
      nationalId:    nationalId,
      maritalStatus: maritalMap[maritalStatus],
      jobId:         selectedJobId,
    };

    try {
      const response = await fetch("http://localhost:5036/api/ArtisanAccount/register-step1-send-otp", {
        method:  "POST",
        headers: { "Content-Type": "application/json" },
        body:    JSON.stringify(registerData),
      });

      const result = await response.json();

      if (response.ok) {
        // ✅ بنبعت email + registerData عشان CraftsmanCode يقدر يعمل Resend
        navigate("/CraftsmanCode", {
          state: {
            email:        email,
            registerData: registerData,
          },
        });
      } else {
        alert(result.message || "حدث خطأ أثناء التسجيل");
      }
    } catch (error) {
      alert("تعذر الاتصال بالخادم، تأكد من تشغيل الـ API");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="craftsman-page-container">
      <motion.div
        className="form-section"
        initial={{ y: 60, opacity: 0 }}
        animate={{ y: 0, opacity: 1 }}
        transition={{ duration: 1, ease: "easeOut", delay: 0.4 }}
      >
        <div className="form-fields">
          <form onSubmit={handleRegister}>
            <h1>مرحباً بك</h1>
            <h3>قم بإنشاء حسابك لبدء استخدام الخدمة</h3>

            <div className="fields-row">

              <div className="field-container">
                <input type="text" placeholder="اسم المستخدم" value={username} onChange={handleUsernameChange} />
              </div>

              <div className="field-container">
                <input type="email" placeholder="البريد الإلكتروني" value={email} onChange={handleEmailChange} />
                {emailError && <p className="error-msg">{emailError}</p>}
              </div>

              <div className="field-container">
                <input type="number" placeholder="العمر" value={age} onChange={(e) => setAge(e.target.value)} min="18" />
              </div>

              <div className="field-container marital-dropdown" ref={maritalDropdownRef}>
                <input type="text" placeholder="الحالة الاجتماعية" value={maritalStatus} readOnly onClick={() => setShowMarital(!showMarital)} />
                {showMarital && (
                  <ul className="dropdown-list">
                    {maritalList.map((item, index) => (
                      <li key={index} onClick={() => { setMaritalStatus(item); setShowMarital(false); }}>{item}</li>
                    ))}
                  </ul>
                )}
              </div>

              <div className="field-container">
                <input type="text" placeholder="الرقم القومي" value={nationalId} onChange={handleNationalIdChange} />
              </div>

              <div className="field-container" ref={jobDropdownRef}>
                <input
                  type="text"
                  placeholder="المهنة"
                  value={selectedJob || jobSearchTerm}
                  onFocus={() => setShowJobDropdown(true)}
                  onChange={(e) => { setJobSearchTerm(e.target.value); setSelectedJob(""); setSelectedJobId(null); setShowJobDropdown(true); }}
                />
                {showJobDropdown && (
                  <ul className="dropdown-list">
                    {filteredJobs.map((j, index) => (
                      <li key={index} onClick={() => { setSelectedJob(j.name); setSelectedJobId(j.id); setJobSearchTerm(""); setShowJobDropdown(false); }}>{j.name}</li>
                    ))}
                  </ul>
                )}
              </div>

              <div className="field-container">
                <input type="text" placeholder="رقم الهاتف" value={phone} onChange={handlePhoneChange} />
              </div>

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
                  <div className="password-rules" style={{ position: "absolute", top: "100%", left: 0, width: "100%", zIndex: 10 }}>
                    <ul>
                      <li style={{ color: checkRule(rules.firstCapital) ? "rgb(114,114,243)" : "rgb(235,138,138)" }}>يجب ان يكون أول حرف Capital</li>
                      <li style={{ color: checkRule(rules.specialChar)  ? "rgb(114,114,243)" : "rgb(235,138,138)" }}>يجب أن يحتوي على !@#$%</li>
                      <li style={{ color: checkRule(rules.minLength)    ? "rgb(114,114,243)" : "rgb(235,138,138)" }}>يجب ان يكون على الأقل 8 أحرف</li>
                    </ul>
                  </div>
                )}
              </div>

              <div className="field-container confirm-password-field-container" style={{ position: "relative" }}>
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
                  <p className="password-error-msg" style={{ color: "rgb(235,138,138)", fontSize: "12px" }}>
                    يجب أن تكون كلمة السر مطابقة
                  </p>
                )}
              </div>

            </div>

            <button
              type="submit"
              disabled={!isFormValid || loading}
              className={`link-button ${!isFormValid || loading ? "disabled" : ""}`}
              style={{ opacity: !isFormValid || loading ? 0.5 : 1, cursor: loading ? "wait" : "pointer" }}
            >
              {loading ? "جاري التسجيل..." : "تسجيل"}
            </button>

            <h4 className="h4-craftsman-login">
              هل لديك حساب ؟ <Link to="/Login" className="Link">تسجيل الدخول</Link>
            </h4>
          </form>
        </div>

        <div className="image">
          <motion.img
            src="/images/Frame 18.svg"
            initial={{ x: "20%", opacity: 0 }}
            animate={{ x: 0, y: [0, -10, 0], opacity: 1 }}
            transition={{ x: { duration: 1.8 }, y: { duration: 3, repeat: Infinity }, opacity: { duration: 1.8 } }}
          />
        </div>
      </motion.div>
    </div>
  );
}

export default CraftsmanRegister;
