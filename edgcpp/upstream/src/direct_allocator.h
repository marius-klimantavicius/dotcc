/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

direct_allocator.c -- Code for the direct allocator.

*/

#ifndef EDG_DIRECT_ALLOCATOR_H
#define EDG_DIRECT_ALLOCATOR_H 1

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

/*
An allocator that directly interfaces with the C malloc, realloc, and free
functions.
*/
template<typename an_Elem>
struct Direct_allocator {
  typedef an_Elem an_elem;
  typedef Allocation<an_elem> an_allocation;
  typedef Direct_allocator<an_elem> an_allocator;
  typedef Direct_allocator<an_elem> a_deallocator;
  /* Allocator concept. */
  inline static auto alloc(size_t n) -> an_allocation;
  inline static auto replace_alloc(an_allocation  a,
                                   size_t         new_capacity,
                                   size_t         n_to_move)
                     -> an_allocation;
  static auto move_alloc(ARG_UNUSED an_allocator  &src,
                         an_allocation            src_alloc,
                         ARG_UNUSED size_t        n_to_move) -> an_allocation
    { return src_alloc; }
  inline static void dealloc(an_allocation allocation);
  /* Deleter concept. */
  inline static void delete_object(an_Elem **elem);
};  /* Direct_allocator */


template<typename an_Elem>
inline auto Direct_allocator<an_Elem>::alloc(size_t n) -> an_allocation
/*
Allocate at least n elements of type an_Elem and return the resulting
allocation (which reflects the actual number of allocated elements).
*/
{
  size_t n_bytes = n * sizeof(an_elem);

  return an_allocation{(an_elem*)malloc(n_bytes), n_bytes};
}  /* Direct_allocator::alloc */


template<typename an_Elem>
inline auto Direct_allocator<an_Elem>::replace_alloc(
                                                    an_allocation a,
                                                    size_t        new_capacity,
                                                    size_t        n_to_move)
            -> an_allocation
/*
Replace the given allocation -- which was allocated by the same allocator -- by
a new one with at least new_capacity elements.  The first n_to_move elements in
the original allocation must be initialized and are moved to the start of the
new allocation.
*/
{
  size_t   n_bytes = new_capacity * sizeof(an_elem);
  an_elem  *old_start = a.start,
           *new_start = (an_elem*)malloc(n_bytes);
  for (size_t k = 0; k < n_to_move; ++k) {
    construct(new_start + k, move_from(old_start + k));
    destroy(old_start + k);
  }  /* for */
  free(old_start);
  return an_allocation{ new_start, n_bytes };
}  /* Direct_allocator::replace_alloc */


template<typename an_Elem>
inline void Direct_allocator<an_Elem>::dealloc(an_allocation a)
/*
Release the given allocation, which was allocated by the same allocator.
The caller is responsible for ensuring the allocation contains no live
objects.
*/
{
  /* Note that free will correctly handle a null allocation. */
  free((void*)a.start);
}  /* Direct_allocator::dealloc */


template<typename an_Object, typename ...an_Arg_pack>
inline an_Object *new_direct(an_Arg_pack ...args)
/*
Allocate via malloc and construct an object of type an_Object with the
constructor arguments specified by args.  Return a pointer to the object.
*/
{
  an_Object *p = Direct_allocator<an_Object>::alloc(1).start;
  construct(p, fwd<an_Arg_pack>(args)...);
  return p;
}  /* new_direct */


template<typename an_Object>
inline void delete_direct(an_Object **p)
/*
Destroy and delete an object of type an_Object that was allocated directly via
malloc.  The value of *p will be set to NULL.
*/
{
  if (*p != NULL) {
    destroy(*p);

    Allocation<an_Object> alloc = {*p, sizeof(an_Object)};
    Direct_allocator<an_Object>::dealloc(alloc);
    *p = NULL;
  }  /* if */
}  /* delete_direct */


template<typename an_Elem>
void Direct_allocator<an_Elem>::delete_object(an_Elem **elem)
/*
Destroy and delete an object of type an_Elem that was allocated directly via
malloc.  The value of *p will be set to NULL.
*/
{
  delete_direct(elem);
}  /* Direct_allocator::delete_object */

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* EDG_DIRECT_ALLOCATOR_H */

