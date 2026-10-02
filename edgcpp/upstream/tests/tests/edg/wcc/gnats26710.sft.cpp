//options:--c++20 --incognito

consteval bool foo() { return true; }

consteval bool edg() {
#ifdef __EDG__
  static_assert(false, "BOOM!");
#endif /* __EDG__ */
  return true;
}  /* edg */

consteval bool edg_constexpr() {
#ifdef __EDG_CONSTEXPR_ENABLED__
  static_assert(false, "BOOM!");
#endif /* __EDG_CONSTEXPR_ENABLED__ */
  return true;
}  /* edg_constexpr */

consteval bool edg_version() {
#ifdef __EDG_VERSION__
  static_assert(false, "BOOM!");
#endif /* __EDG_VERSION__ */
  return true;
}  /* edg_version */

consteval bool edg_size_type() {
#ifdef __EDG_SIZE_TYPE__
  static_assert(false, "BOOM!");
#endif /* __EDG_SIZE_TYPE__ */
  return true;
}  /* edg_size_type */

consteval bool edg_ptrdiff_type() {
#ifdef __EDG_PTRDIFF_TYPE__
  static_assert(false, "BOOM!");
#endif /* __EDG_PTRDIFF_TYPE__ */
  return true;
}  /* edg_ptrdiff_type */
