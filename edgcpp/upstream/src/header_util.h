/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

header_util.h -- General utility components (mostly templates) intended for use
in both headers and compilation units.  Utilities declared here are guaranteed
to be free from dependence on undefined entities in the front end.

*/

#ifndef EDG_HEADER_UTIL_H
#define EDG_HEADER_UTIL_H 1

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

/*
A generic utility for saving and automatically restoring the original value of
a variable whose value is temporarily being changed.
*/
template<typename a_Var_type>
struct Value_saver {
  INLINE Value_saver(a_Var_type * const var_to_be_saved);
  INLINE Value_saver(a_Var_type * const var_to_be_saved,
                     const a_Var_type   &new_value);
  INLINE ~Value_saver();

  INLINE void restore_now() const;
private:
  a_Var_type *const
                saved_var;
                        /* A pointer to the variable where the saved value
                           should be restored upon destruction. */
  a_Var_type
                saved_value;
                        /* The value captured during construction to be
                           restored upon destruction. */
};  /* Value_saver */


template<typename a_Var_type>
Value_saver<a_Var_type>::Value_saver(a_Var_type * const var_to_be_saved)
  : saved_var(var_to_be_saved), saved_value(*var_to_be_saved)
/*
Save the current value of the given variable (var_to_be_saved).  The saved
value will be restored upon destruction.
*/
{
}  /* Value_saver::Value_saver */


template<typename a_Var_type>
Value_saver<a_Var_type>::Value_saver(a_Var_type * const var_to_be_saved,
                                     const a_Var_type   &new_value)
  : Value_saver(var_to_be_saved)
/*
Save the current value of the given variable (var_to_be_saved).  Then, update
the variable's value to the given new value.  The saved value will be restored
upon destruction.
*/
{
  *saved_var = new_value;
}  /* Value_saver::Value_saver */


template<typename a_Var_type>
Value_saver<a_Var_type>::~Value_saver()
/*
Restore the variable given during construction to its saved state.
*/
{
  this->restore_now();
}  /* Value_saver::~Value_saver */


template<typename a_Var_type>
void Value_saver<a_Var_type>::restore_now() const
/*
Immediately restore the variable given during construction to its saved state.
*/
{
  *saved_var = saved_value;
}  /* Value_saver::restore_now */


/*
An implementation of an "optional" type.  This type allows representing
a value that may or may not be present and is thus "optional."

Before dereferencing to retrieve the value, consuming code should check that
the optional has a stored value (via a call to "has_value").  When checking is
enabled, this contract is strictly enforced to ensure that the calling logic
doesn't accidentally forget a check.
*/
template<typename a_Value_type>
struct Opt {
  INLINE Opt() : storing_value(FALSE)
    {}
  INLINE Opt(const a_Value_type &value)
    : storing_value(TRUE), stored_value(value)
    {}
  INLINE Opt(a_Value_type &&value)
    : storing_value(TRUE), stored_value(static_cast<a_Value_type &&>(value))
    {}
  INLINE Opt(const Opt<a_Value_type> &other);
  INLINE Opt(Opt<a_Value_type> &&other);
  INLINE ~Opt();
  INLINE a_boolean has_value() const;
  /* Value retrieval functions. */
  INLINE a_Value_type* operator->();
  INLINE const a_Value_type* operator->() const;
  INLINE a_Value_type& operator*();
  INLINE const a_Value_type& operator*() const;
  /* Value update functions. */
  INLINE Opt<a_Value_type>& operator=(const a_Value_type &value);
  INLINE Opt<a_Value_type>& operator=(a_Value_type &&value);
  INLINE Opt<a_Value_type>& operator=(const Opt<a_Value_type> &other);
  INLINE Opt<a_Value_type>& operator=(Opt<a_Value_type> &&other);
  INLINE void clear();
private:
  a_boolean     storing_value;
                        /* TRUE if there is a value stored, FALSE otherwise. */
#ifdef UNION_AS_STRUCT
/* Workaround for union-as-struct build issue. */
#undef union
#endif /* ifdef UNION_AS_STRUCT */
  union {
    a_Value_type
                stored_value;
                        /* The value stored.  Represented as a union so the
                           value can be uninitialized, and construction and
                           destruction are manually managed. */
#ifdef UNION_AS_STRUCT
#define union struct
#endif /* ifdef UNION_AS_STRUCT */
  };
#if CHECKING
  a_boolean     value_presence_checked = FALSE;
                        /* TRUE if there was a call to has_value for this
                           instance of Opt, FALSE otherwise. */
#endif /* CHECKING */
};


template<typename a_Value_type>
Opt<a_Value_type>::Opt(const Opt<a_Value_type> &other)
  : storing_value(other.storing_value)
/*
Copy-construct an optional from another optional.
*/
{
  /* A value was stored, copy it. */
  if (storing_value) {
    ::new (&stored_value) a_Value_type(other.stored_value);
  }  /* if */
}  /* Opt */


template<typename a_Value_type>
Opt<a_Value_type>::Opt(Opt<a_Value_type> &&other)
  : storing_value(other.storing_value)
/*
Move-construct an optional from another optional.
*/
{
  /* A value was stored, move it. */
  if (storing_value) {
    ::new (&stored_value) a_Value_type(
                             static_cast<a_Value_type &&>(other.stored_value));
  }  /* if */
}  /* Opt */


template<typename a_Value_type>
Opt<a_Value_type>::~Opt()
/*
Destruct an optional invoking the destructor for the stored value if there is
one.
*/
{
  /* A value was stored, make sure its destructor is invoked. */
  if (storing_value) {
    stored_value.~a_Value_type();
  }  /* if */
}  /* ~Opt */


template<typename a_Value_type>
a_boolean Opt<a_Value_type>::has_value() const
/*
Return TRUE if this optional is storing a value, otherwise return FALSE.
*/
{
#if CHECKING
  /* In checking builds break constness to record that the presence of a value
     was checked for before being accessed. */
  const_cast<Opt<a_Value_type>*>(this)->value_presence_checked = TRUE;
#endif /* CHECKING */
  return storing_value;
}  /* has_value */


template<typename a_Value_type>
a_Value_type* Opt<a_Value_type>::operator->()
/*
This function is only valid when the Opt is not empty.  A pointer to the stored
value is returned.
*/
{
  /* Check that the caller previously checked for a value. */
  check_assertion_str(value_presence_checked, "missing call to has_value");
  /* Check that a value is present. */
  check_assertion_str(storing_value, "the optional was empty");
  return &stored_value;
}  /* operator-> */


template<typename a_Value_type>
const a_Value_type* Opt<a_Value_type>::operator->() const
/*
This function is only valid when the Opt is not empty.  A pointer to the stored
value is returned.
*/
{
  /* Check that the caller previously checked for a value. */
  check_assertion_str(value_presence_checked, "missing call to has_value");
  /* Check that a value is present. */
  check_assertion_str(storing_value, "the optional was empty");
  return &stored_value;
}  /* operator-> */


template<typename a_Value_type>
a_Value_type& Opt<a_Value_type>::operator*()
/*
This function is only valid when the Opt is not empty.  A reference to the
stored value is returned.
*/
{
  /* Check that the caller previously checked for a value. */
  check_assertion_str(value_presence_checked, "missing call to has_value");
  /* Check that a value is present. */
  check_assertion_str(storing_value, "the optional was empty");
  return stored_value;
}  /* operator* */


template<typename a_Value_type>
const a_Value_type& Opt<a_Value_type>::operator*() const
/*
This function is only valid when the Opt is not empty.  A reference to the
stored value is returned.
*/
{
  /* Check that the caller previously checked for a value. */
  check_assertion_str(value_presence_checked, "missing call to has_value");
  /* Check that a value is present. */
  check_assertion_str(storing_value, "the optional was empty");
  return stored_value;
}  /* operator* */


template<typename a_Value_type>
Opt<a_Value_type>& Opt<a_Value_type>::operator=(const a_Value_type &value)
/*
Store the given value as the new stored value via a copy.  The updated optional
is returned.
*/
{
  /* If a value was stored previously, use the value type's normal copy
     assignment operator; otherwise, use placement new to initialize the memory
     and update the storage flag. */
  if (storing_value) {
    stored_value = value;
  } else {
    storing_value = TRUE;
    ::new (&stored_value) a_Value_type(value);
  }  /* if */
  return *this;
}  /* operator= */


template<typename a_Value_type>
Opt<a_Value_type>& Opt<a_Value_type>::operator=(a_Value_type &&value)
/*
Store the given value as the new stored value via a move.  The updated optional
is returned.
*/
{
  /* If a value was stored previously, use the value type's normal move
     assignment operator; otherwise, use placement new to initialize the memory
     and update the storage flag. */
  if (storing_value) {
    stored_value = static_cast<a_Value_type &&>(value);
  } else {
    storing_value = TRUE;
    ::new (&stored_value) a_Value_type(static_cast<a_Value_type &&>(value));
  }  /* if */
  return *this;
}  /* operator= */


template<typename a_Value_type>
Opt<a_Value_type>& Opt<a_Value_type>::operator=(const Opt<a_Value_type> &other)
/*
Store the given value as the new stored value via a copy.  The updated optional
is returned.
*/
{
  /* If the copied optional has a value, invoke this optional's copy assignment
     operator with the value to be stored, to copy the value; otherwise, invoke
     clear to remove any current value. */
  if (other.storing_value) {
    *this = other.stored_value;
  } else {
    this->clear();
  }  /* if */
#if CHECKING
  /* Reset the "checked" status as the value has been updated by an operation
     where the previous answer to has_value may have changed. */
  value_presence_checked = FALSE;
#endif /* CHECKING */
  return *this;
}  /* operator= */


template<typename a_Value_type>
Opt<a_Value_type>& Opt<a_Value_type>::operator=(Opt<a_Value_type> &&other)
/*
Store the given value as the new stored value via a copy.  The updated optional
is returned.
*/
{
  /* If the copied optional has a value, invoke this optional's move assignment
     operator with the value to be stored, to move the value; otherwise, invoke
     clear to remove any current value. */
  if (other.storing_value) {
    *this = static_cast<a_Value_type &&>(other.stored_value);
  } else {
    this->clear();
  }  /* if */
#if CHECKING
  /* Reset the "checked" status as the value has been updated by an operation
     where the previous answer to has_value may have changed. */
  value_presence_checked = FALSE;
#endif /* CHECKING */
  return *this;
}  /* operator= */


template<typename a_Value_type>
void Opt<a_Value_type>::clear()
/*
Reset the optional to an empty state.
*/
{
  /* A value was stored, make sure its destructor is invoked. */
  if (storing_value) {
    stored_value.~a_Value_type();
  }  /* if */
  storing_value = FALSE;
#if CHECKING
  /* Reset the "checked" status as the value has been updated by an operation
     where the previous answer to has_value may have changed. */
  value_presence_checked = FALSE;
#endif /* CHECKING */
}  /* clear */


/*
A struct representing a const char*-based string of a given length.  The
lifetime of the represented string must exceed the lifetime of the string view
object.
*/
struct a_string_view {
  INLINE a_string_view()
    : str_start(""), str_length(0)
    {}
  INLINE a_string_view(a_const_char *start_val)
    : str_start((check_assertion(start_val != NULL), start_val)),
      str_length(strlen(start_val))
    {}
  INLINE a_string_view(a_const_char *start_val, size_t length_val)
    : str_start(start_val), str_length(length_val)
    { check_assertion(start_val != NULL); }

  INLINE a_const_char* start() const
    { return this->str_start; }
  INLINE size_t length() const
    { return this->str_length; }
private:
  a_const_char  *str_start;
                        /* The start of the string. */
  size_t        str_length;
                        /* The length of the string (not including the null
                           terminator for null-terminated strings). */
};  /* a_string_view */


INLINE a_boolean operator==(const a_string_view str1,
                            const a_string_view str2)
/*
Return TRUE if the string represented by str1 is the same as the string
represented by str2.  If both handles contain empty strings then the "strings"
are considered the same.
*/
{
  a_boolean result;

  if (str1.length() != str2.length()) {
    result = FALSE;
  } else {
    result = (strncmp(str1.start(), str2.start(), str1.length()) == 0);
  }  /* if */
  return result;
}  /* operator== */


INLINE a_boolean operator!=(const a_string_view str1,
                            const a_string_view str2)
/*
Return TRUE if the string represented by str1 is not the same as the string
represented by str2.  If both handles contain empty strings then the "strings"
are considered the same.
*/
{
  return !(str1 == str2);
}  /* operator!= */


INLINE a_boolean operator<(const a_string_view str1,
                           const a_string_view str2)
/*
Return TRUE if the string represented by str1 comes before str in
lexicographical order; otherwise, return FALSE.
*/
{
  a_boolean result = FALSE;
  size_t    max_len = str1.length();

  if (str2.length() < max_len) {
    max_len = str2.length();
  }  /* if */

  int cmp_result = strncmp(str1.start(), str2.start(), max_len);
  if (cmp_result < 0) {
    result = TRUE;
  } else if (cmp_result == 0) {
    if (str1.length() < str2.length()) {
      result = TRUE;
    }  /* if */
  }  /* if */
  return result;
}  /* operator< */


/*
Pack expand the given void type expression with each expanded expression
guaranteed to evaluate at runtime in the order of expansion.

This macro uses a braced-init-list as the means of expansion (for the well
defined left-to-right evaluation order of braced-init-list [dcl.init.list]).

As the braced-init-list must initialize something, the following expansion
defines an array (unused and thus discarded by the optimizer), where each array
element is initialized by a comma expression that evaluates the corresponding
pack element and then has a constant zero-valued result.

An additional 0u value is appended to gracefully handle empty packs.
*/
#define PACK_EXPAND_VOID_EXPR(expr) \
  { LOCAL_UNUSED unsigned _[] = { ( (expr), 0u)..., 0u }; }

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* ifndef EDG_HEADER_UTIL_H */

