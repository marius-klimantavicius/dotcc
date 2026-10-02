/*
Part of the EDG Compiler Project, under the Apache License v2.0 with LLVM
Exceptions.
See https://edgcpp.org/LICENSE.txt for license information.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
*/

/*

extasm.c -- Scanning and validation of GNU extended asm() statements.

*/

/* Header files common to all files. */
#include "fe_common.h"

#ifdef PCH_PRAGMA_GUARD
/* Mark the end of the sequence of headers subject to precompiled header
   processing. */
#pragma hdrstop
#endif /* ifdef PCH_PRAGMA_GUARD */

#if GNU_EXTENSIONS_ALLOWED

#include "extasm.h"

/* Header files used by files involved in declaration processing. */
#include "decl_hdrs.h"

/* Conditionally open the "edg" namespace. */
BEGIN_EDG_NAMESPACE

struct name_to_reg {
  /* Structure to hold a name-to-register mapping entry. */
  a_const_char      *name;
  a_named_register  reg;
};

/*
Extra named registers, in addition to the canonical names in il_def.h.
*/
STATIC_THREAD struct name_to_reg extra_reg_names[] = {
#if GNU_X86_ASM_EXTENSIONS_ALLOWED
  /* x86 integer registers, common set, other possible names... */
  { "al",  (a_named_register)anr_a  },
  { "bl",  (a_named_register)anr_b  },
  { "cl",  (a_named_register)anr_c  },
  { "dl",  (a_named_register)anr_d  },
  { "ah",  (a_named_register)anr_a  },
  { "bh",  (a_named_register)anr_b  },
  { "ch",  (a_named_register)anr_c  },
  { "dh",  (a_named_register)anr_d  },
  { "eax", (a_named_register)anr_a  },
  { "ebx", (a_named_register)anr_b  },
  { "ecx", (a_named_register)anr_c  },
  { "edx", (a_named_register)anr_d  },
  { "esi", (a_named_register)anr_si },
  { "edi", (a_named_register)anr_di },
  { "ebp", (a_named_register)anr_bp },
  { "esp", (a_named_register)anr_sp },
  { "cc",  (a_named_register)anr_flags },

  /* x86-64 additional integer registers, other possible names... */
  { "rax", (a_named_register)anr_a  },
  { "rbx", (a_named_register)anr_b  },
  { "rcx", (a_named_register)anr_c  },
  { "rdx", (a_named_register)anr_d  },
  { "rsi", (a_named_register)anr_si },
  { "rdi", (a_named_register)anr_di },
  { "rbp", (a_named_register)anr_bp },
  { "rsp", (a_named_register)anr_sp },
  { "sil", (a_named_register)anr_si },
  { "dil", (a_named_register)anr_di },
  { "bpl", (a_named_register)anr_bp },
  { "spl", (a_named_register)anr_sp },
  { "r8b",  (a_named_register)anr_r8  },
  { "r8w",  (a_named_register)anr_r8  },
  { "r8d",  (a_named_register)anr_r8  },
  { "r9b",  (a_named_register)anr_r9  },
  { "r9w",  (a_named_register)anr_r9  },
  { "r9d",  (a_named_register)anr_r9  },
  { "r10b", (a_named_register)anr_r10 },
  { "r10w", (a_named_register)anr_r10 },
  { "r10d", (a_named_register)anr_r10 },
  { "r11b", (a_named_register)anr_r11 },
  { "r11w", (a_named_register)anr_r11 },
  { "r11d", (a_named_register)anr_r11 },
  { "r12b", (a_named_register)anr_r12 },
  { "r12w", (a_named_register)anr_r12 },
  { "r12d", (a_named_register)anr_r12 },
  { "r13b", (a_named_register)anr_r13 },
  { "r13w", (a_named_register)anr_r13 },
  { "r13d", (a_named_register)anr_r13 },
  { "r14b", (a_named_register)anr_r14 },
  { "r14w", (a_named_register)anr_r14 },
  { "r14d", (a_named_register)anr_r14 },
  { "r15b", (a_named_register)anr_r15 },
  { "r15w", (a_named_register)anr_r15 },
  { "r15d", (a_named_register)anr_r15 },

  /* 80387 floating point registers, other possible names... */
  { "st1", (a_named_register)anr_st1 },
  { "st2", (a_named_register)anr_st2 },
  { "st3", (a_named_register)anr_st3 },
  { "st4", (a_named_register)anr_st4 },
  { "st5", (a_named_register)anr_st5 },
  { "st6", (a_named_register)anr_st6 },
  { "st7", (a_named_register)anr_st7 },

  /* numeric register names... */
  { "0",   (a_named_register)anr_a  },
  { "1",   (a_named_register)anr_d  },
  { "2",   (a_named_register)anr_c  },
  { "3",   (a_named_register)anr_b  },
  { "4",   (a_named_register)anr_si },
  { "5",   (a_named_register)anr_di },
  { "6",   (a_named_register)anr_bp },
  { "7",   (a_named_register)anr_sp },
  { "8",   (a_named_register)anr_st },
  { "9",   (a_named_register)anr_st1 },
  { "10",  (a_named_register)anr_st2 },
  { "11",  (a_named_register)anr_st3 },
  { "12",  (a_named_register)anr_st4 },
  { "13",  (a_named_register)anr_st5 },
  { "14",  (a_named_register)anr_st6 },
  { "15",  (a_named_register)anr_st7 },
  /* (Registers 16 through 20 do not map on known registers.  Their primary
     names are therefore just "16", "17, "18", "19", and "20".  No secondary
     names are needed here. */
  { "21",  (a_named_register)anr_f0 },
  { "22",  (a_named_register)anr_f1 },
  { "23",  (a_named_register)anr_f2 },
  { "24",  (a_named_register)anr_f3 },
  { "25",  (a_named_register)anr_f4 },
  { "26",  (a_named_register)anr_f5 },
  { "27",  (a_named_register)anr_f6 },
  { "28",  (a_named_register)anr_f7 },
  { "29",  (a_named_register)anr_mm0 },
  { "30",  (a_named_register)anr_mm1 },
  { "31",  (a_named_register)anr_mm2 },
  { "32",  (a_named_register)anr_mm3 },
  { "33",  (a_named_register)anr_mm4 },
  { "34",  (a_named_register)anr_mm5 },
  { "35",  (a_named_register)anr_mm6 },
  { "36",  (a_named_register)anr_mm7 },
  { "37",  (a_named_register)anr_r8 },
  { "38",  (a_named_register)anr_r9 },
  { "39",  (a_named_register)anr_r10 },
  { "40",  (a_named_register)anr_r11 },
  { "41",  (a_named_register)anr_r12 },
  { "42",  (a_named_register)anr_r13 },
  { "43",  (a_named_register)anr_r14 },
  { "44",  (a_named_register)anr_r15 },
  { "45",  (a_named_register)anr_f8 },
  { "46",  (a_named_register)anr_f9 },
  { "47",  (a_named_register)anr_f10 },
  { "48",  (a_named_register)anr_f11 },
  { "49",  (a_named_register)anr_f12 },
  { "50",  (a_named_register)anr_f13 },
  { "51",  (a_named_register)anr_f14 },
  { "52",  (a_named_register)anr_f15 },
#endif /* GNU_X86_ASM_EXTENSIONS_ALLOWED */
#if ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS
  { "", (a_named_register)anr_unrecognized },
#endif /* ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS */
  { "", (a_named_register)anr_last }
};

/* The complete map between register names and enumerators, and its size. */
STATIC_THREAD struct name_to_reg
                *regmap;
STATIC_THREAD size_t
                regmap_size;


a_named_register name_to_register(a_const_char  *name)
/*
Given the user-specified name of a register as a string, return
its code number, or anr_invalid if there is no such register.
In the latter case, issues an error.
*/
{
  unsigned int      md, mn = 0, mx = (unsigned int)regmap_size;
  int               comp;
  a_named_register  result = (a_named_register)anr_invalid;
  a_const_char      *name_to_search = name;

#if GNU_X86_ASM_EXTENSIONS_ALLOWED
  if (name[0] == '%') {
    /* Register names are optionally prefixed with the "%" character. */
    ++name_to_search;
  }  /* if */
#endif /* GNU_X86_ASM_EXTENSIONS_ALLOWED */
  /* Binary search regmap array. */
  while (mx > mn) {
    md = (mn + mx) / 2;
    comp = strcmp(name_to_search, regmap[md].name);
    if (comp > 0) {
      mn = md + 1;
    } else if (comp < 0) {
      mx = md;
    } else {
      result = regmap[md].reg;
      break;
    }  /* if */
  }  /* while */
#if ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS
  if (result == (a_named_register)anr_invalid) {
    result = (a_named_register)anr_unrecognized;
  }  /* if */
#endif /* ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS */
  return result;
}  /* name_to_register */

#if !RECORD_RAW_ASM_OPERAND_DESCRIPTIONS

static a_boolean validate_expr_for_constraints(
                                ARG_UNUSED an_expr_node_ptr              expr,
                                ARG_UNUSED an_asm_operand_constraint_ptr cstrt)
/*
Verify that expr can legitimately be used as an asm operand with
constraints given by cstrt.  This code is machine specific and must be
provided by the author of the back end.
*/
{
  return TRUE;
}  /* validate_expr_for_constraints */

#endif /* !RECORD_RAW_ASM_OPERAND_DESCRIPTIONS */

static int find_symbolic_operand(a_const_char        **pc,
                                 an_asm_operand_ptr  operands,
                                 a_label_list_ptr    labels,
                                 a_boolean           is_label,
                                 a_source_position   *diag_pos)
/*
*pc points to a left bracket ('[') that starts a reference to a symbolic asm
operand or label.  Advance the *pc pointer to the matching right bracket (or
the null character terminating the string if there is no right bracket) and
return the position of the indicated operand.  If the indicated operand name
does not correspond to a previous operand or label, issue an error (at the
given source position) and return -1.  operands points to the list of operands
created so far (this routine is sometimes called when that list is still
incomplete).  labels points to a list of labels (if this is an "asm goto"
statement) or NULL.  When is_label is TRUE, the symbolic name is from a
"%l[label]" reference, and thus refers to a label (and not an operand).
*/
{
  int          result = -1, n = 0;
  a_const_char *start;

  check_assertion(**pc == '[');
  start = ++*pc;
  /* Find the end of the symbolic operand name. */
  while (**pc != ']' && **pc != '\0') {
    ++*pc;
  }  /* while */
  if (is_label) {
    /* Look for a label whose name matches the symbolic name. */
    check_assertion_or_expect_error(labels != NULL);
    while (labels != NULL) {
      a_const_char *label_name = labels->label->source_corresp.name;
      if (label_name != NULL) {
        size_t len_from_start = (size_t)((*pc) - start);
        if (strncmp(label_name, start, len_from_start) == 0 &&
            strlen(label_name) == len_from_start) {
          result = n;
          break;
        }  /* if */
      }  /* if */
      labels = labels->next;
      ++n;
    }  /* while */
  } else {
    /* Look for an operand with that name in the list of preceding operands. */
    while (operands != NULL) {
      if (operands->name != NULL) {
        size_t len_from_start = (size_t)((*pc) - start);
        if (strncmp(operands->name, start, len_from_start) == 0 &&
            strlen(operands->name) == len_from_start) {
          result = n;
          break;
        }  /* if */
      }  /* if */
      operands = operands->next;
      ++n;
    }  /* while */
  }  /* if */
  if (result == -1) {
    char saved_char = **pc;
    *(char *)*pc = '\0';
    pos_st_error(ec_invalid_symbolic_asm_operand_name, diag_pos, start);
    *(char *)*pc = saved_char;
  }  /* if */
  return result;
}  /* find_symbolic_operand */


static void validate_symbolic_operand_and_label_references(
                                                   an_asm_entry_ptr  asm_entry)
/*
Traverse the asm string for the given asm_entry and validate any symbolic
operand references of the form "%[<name>]" it contains against the list of
operands.  For an asm goto also check the string for any references to labels,
either by symbolic reference (e.g., "%l[label]) or by argument number
(e.g., "%l0).
*/
{
  a_constant_ptr      asm_string = asm_entry->asm_string;

  if (asm_string->kind == (a_constant_repr_kind)ck_string) {
    size_t       operand_count = 0;
    a_boolean    is_label;
    a_source_position
                 *diag_pos = &asm_entry->source_corresp.decl_position;
    a_const_char *pc = asm_string->variant.string.value;
    while (*pc != '\0') {
      if (pc[0] == '%' && (pc[1] == '[' || (pc[1] != '\0' && pc[2] == '['))) {
        /* We found a "%[" or "%X[" (where X is an output format modifier)
           construct.  Look up the symbolic operand reference that (normally)
           follows.  The call to find_symbol_operand will trigger any needed
           diagnostics. */
        ++pc;
        is_label = FALSE;
        if (*pc != '[')  {
          /* An output format modifier between the '%' and '['. */
          if (*pc == 'l' && asm_entry->is_asm_goto) {
            /* Found "%l[", which introduces a label in an "asm goto".  The
               name that follows must match one of the label arguments. */
            is_label = TRUE;
          }  /* if */
          ++pc;
        }  /* if */
        (void)find_symbolic_operand(&pc, asm_entry->operands,
                                    asm_entry->labels, is_label, diag_pos);
      } else if (pc[0] == '%' && pc[1] == '%') {
        /* Found "%%" which indicates a register that begins with a '%'; just
           skip over it. */
        pc += 2;
      } else if (pc[0] == '%' && pc[1] == 'l' && asm_entry->is_asm_goto) {
        /* Found "%l" (but not "%l["); what follows should be a decimal number
           that corresponds to a valid label argument number. */
        pc += 2;
        if (*pc >= '0' && *pc <= '9') {
          size_t operand_num = 0;
          while (*pc >= '0' && *pc <= '9') {
            operand_num *= 10;
            operand_num += size_t_arg(*pc - '0');
            ++pc;
          }  /* while */
          /* Verify that the (zero-based) operand number refers to a label
             argument that was specified.  The argument number includes
             any input or output operands specified before the labels. */
          if (operand_count == 0) {
            a_label_list_ptr   llp;
            an_asm_operand_ptr aop;
            for (aop = asm_entry->operands; aop != NULL; aop = aop->next) {
              ++operand_count;
            }  /* for */
            for (llp = asm_entry->labels; llp != NULL; llp = llp->next) {
              ++operand_count;
            }  /* for */
          }  /* if */
          if (operand_num >= operand_count) {
            pos_error(ec_label_operand_number_out_of_range, diag_pos);
          }  /* if */
        } else {
          /* No operand number after "%l". */
          pos_error(ec_missing_label_operand_number, diag_pos);
        }  /* if */
      } else {
        ++pc;
      }  /* if */
    }  /* while */
  } else {
    check_assertion(is_error_constant(asm_string));
    expect_error();
  }  /* if */
}  /* validate_symbolic_operand_and_label_references */

#if !RECORD_RAW_ASM_OPERAND_DESCRIPTIONS

static an_asm_operand_constraint_kind get_symbolic_matching_constraint(
                                                 a_const_char        **pc,
                                                 an_asm_operand_ptr  operands,
                                                 a_source_position   *diag_pos)
/*
*pc points to a '[' character starting a symbolic operand name.  Advance this
pointer to the matching ']' character (or to the last character in the string
if none is found) and return a "matching constraint kind" for the operand
name indicated.  operands points to the list of operands already created.
Errors are diagnosed at the given position.
*/
{
  an_asm_operand_constraint_kind  result;
  int                             op_num;

  op_num = find_symbolic_operand(pc, operands, (a_label_list_ptr)NULL,
                                 /*is_label=*/FALSE, diag_pos);
  if (op_num < 0) {
    /* An error was already issued. */
    result = (an_asm_operand_constraint_kind)aoc_invalid;
  } else if (op_num > 9) {
    pos_error(ec_match_limit_for_symbolic_asm_operand, diag_pos);
    result = (an_asm_operand_constraint_kind)aoc_invalid;
  } else {
    result = (an_asm_operand_constraint_kind)((int)aoc_match_0 + op_num);
  }  /* if */
  return result;
}  /* get_symbolic_matching_constraint */

#endif /* !RECORD_RAW_ASM_OPERAND_DESCRIPTIONS */

static void process_asm_operand(
                          an_asm_operand_ptr            operand,
                          ARG_UNUSED an_asm_operand_ptr operands,
                          an_expr_node_ptr              expr,
                          a_const_char                  *cstring,
                          a_boolean                     output,
                          ARG_UNUSED int                *number_of_constraints)
/*
Fill in *operand (a GNU asm operand description) using the cstring constraints
string and the expr expression.  Output is TRUE if the call is for an output
operand.  If RECORD_RAW_ASM_OPERAND_DESCRIPTIONS is FALSE, validate the
semantic consistency of expr, cstring, and output: If an inconsistency is
detected, an error is issued and *operand is set to "error placemarker" values.
operands points to the operands created so far.  If *number_of_constraints is
zero, this is the first operand and *number_of_constraints will be updated to
the actual number of constraints found in the constraint string (and will check
for that number on subsequent invocations).
*/
{
#if RECORD_RAW_ASM_OPERAND_DESCRIPTIONS
  /* When the raw form is recorded, consistency checks are not performed by
     the front end.  (A back end may be in a better position to perform those
     checks.) */
  operand->is_output_operand = output;
  operand->constraints_string = cstring;
  operand->expression = expr;
#else /* !RECORD_RAW_ASM_OPERAND_DESCRIPTIONS */
  an_asm_operand_constraint_ptr  *constraint;
  an_asm_operand_constraint_kind ck;
  an_asm_operand_modifier        modifiers;
  a_boolean                      error_occurred = FALSE;
  a_const_char                   *p;
  int                            constraints = 1;
#if !ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS
  char                           errletter[2];
#endif /* !ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS */

  operand->constraints = NULL;
  if (cstring == NULL || expr == NULL) {
    /* Syntactically invalid -- an error has already been issued.
       N.B. We do not bail out if expr is an error node, because it's
       still possible and useful to validate the constraint string. */
    goto error_return;
  }  /* if */
#if !ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS
  errletter[1] = '\0';
#endif /* !ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS */
  /* Compute modifiers.  Note that the "+" and "=" modifiers must appear at the
     beginning of the constraint string; other modifiers are handled as
     "constraints" in the loop below. */
  modifiers = (an_asm_operand_modifier)aom_invalid;
  for (p = cstring; *p != '\0'; p++) {
    switch (*p) {
      /* Modifiers valid only at beginning of constraint string. */
      case '=':
        modifiers = (an_asm_operand_modifier)(modifiers | aom_output);
        break;
      case '+':
        modifiers = (an_asm_operand_modifier)(modifiers | aom_modify);
        break;
      default:
        goto done_with_modifiers;
    }  /* switch */
  }  /* for */
done_with_modifiers:
  if (modifiers == (an_asm_operand_modifier)aom_invalid) {
    /* No modifiers on an input operand. */
    modifiers = (an_asm_operand_modifier)aom_input;
  }  /* if */
  if (*p == '\0') {
    pos_error(ec_missing_constraint_letter, &operand->position);
    goto error_return;
  }  /* if */
  constraint = &operand->constraints;
  /*lint --e{850} p modified in loop */
  for (; *p != '\0'; p++) {
#if GNU_X86_ASM_EXTENSIONS_ALLOWED
    a_const_char *cond_code = NULL;
#endif /* GNU_X86_ASM_EXTENSIONS_ALLOWED */
    /* The next thing in the string should be a constraint letter. */
    ck = (an_asm_operand_constraint_kind)aoc_invalid;
    switch (*p) {
      /* A constraint separator (multiple alternative constraint case). */
      case ',':
        ck = (an_asm_operand_constraint_kind)aoc_end_of_constraint;
        constraints++;
        break;
      /* Modifiers that can appear multiple times in a constraint string. */
      case '&':
        ck = (an_asm_operand_constraint_kind)aoc_mod_earlyclobber;
        break;
      case '%':
        ck = (an_asm_operand_constraint_kind)aoc_mod_commutative_ops;
        break;
      case '#':
        ck = (an_asm_operand_constraint_kind)aoc_mod_ignore;
        break;
      case '*':
        ck = (an_asm_operand_constraint_kind)aoc_mod_ignore_char;
        break;
      case '?':
        ck = (an_asm_operand_constraint_kind)aoc_mod_disparage_slightly;
        break;
      case '!':
        ck = (an_asm_operand_constraint_kind)aoc_mod_disparage_severely;
        break;
      /* Machine independent constraints - miscellaneous. */
      case 'X':
        ck = (an_asm_operand_constraint_kind)aoc_any;     
        break;
      case 'g': 
        ck = (an_asm_operand_constraint_kind)aoc_general; 
        break;
      case '0': 
        ck = (an_asm_operand_constraint_kind)aoc_match_0; 
        break;
      case '1': 
        ck = (an_asm_operand_constraint_kind)aoc_match_1; 
        break;
      case '2': 
        ck = (an_asm_operand_constraint_kind)aoc_match_2; 
        break;
      case '3':
        ck = (an_asm_operand_constraint_kind)aoc_match_3; 
        break;
      case '4': 
        ck = (an_asm_operand_constraint_kind)aoc_match_4; 
        break;
      case '5': 
        ck = (an_asm_operand_constraint_kind)aoc_match_5; 
        break;
      case '6': 
        ck = (an_asm_operand_constraint_kind)aoc_match_6; 
        break;
      case '7': 
        ck = (an_asm_operand_constraint_kind)aoc_match_7; 
        break;
      case '8': 
        ck = (an_asm_operand_constraint_kind)aoc_match_8; 
        break;
      case '9': 
        ck = (an_asm_operand_constraint_kind)aoc_match_9; 
        break;
      case '[':
        ck = get_symbolic_matching_constraint(&p, operands,
                                              &operand->position);
        error_occurred = (ck == (an_asm_operand_constraint_kind)aoc_invalid);
        break;
      /* Registers */
      case 'r': 
        ck = (an_asm_operand_constraint_kind)aoc_reg_integer; 
        break;
      case 'f': 
        ck = (an_asm_operand_constraint_kind)aoc_reg_float;   
        break;
      /* Memory */
      case 'm': 
        ck = (an_asm_operand_constraint_kind)aoc_mem_any;
        break;
      case 'p': 
        ck = (an_asm_operand_constraint_kind)aoc_mem_load;
        break;
      case 'o': 
        ck = (an_asm_operand_constraint_kind)aoc_mem_offset;
        break;
      case 'V': 
        ck = (an_asm_operand_constraint_kind)aoc_mem_nonoffset;
        break;
      case '<': 
        ck = (an_asm_operand_constraint_kind)aoc_mem_autoinc;
        break;
      case '>': 
        ck = (an_asm_operand_constraint_kind)aoc_mem_autodec;
        break;
      /* Immediates */
      case 'i':
        ck = (an_asm_operand_constraint_kind)aoc_imm_int;    
        break;
      case 'n':
        ck = (an_asm_operand_constraint_kind)aoc_imm_number; 
        break;
      case 's':
        ck = (an_asm_operand_constraint_kind)aoc_imm_symbol; 
        break;
      case 'E':
        ck = (an_asm_operand_constraint_kind)aoc_imm_float;  
        break;
      case 'F':
        ck = (an_asm_operand_constraint_kind)aoc_imm_float;  
        break;
#if GNU_X86_ASM_EXTENSIONS_ALLOWED
      /* x86 specific constraints - registers */
      case 'a':
        ck = (an_asm_operand_constraint_kind)aoc_reg_a;         
        break;
      case 'b':
        ck = (an_asm_operand_constraint_kind)aoc_reg_b;         
        break;
      case 'c':
        ck = (an_asm_operand_constraint_kind)aoc_reg_c;         
        break;
      case 'd':
        ck = (an_asm_operand_constraint_kind)aoc_reg_d;         
        break;
      case 'S':
        ck = (an_asm_operand_constraint_kind)aoc_reg_si;        
        break;
      case 'D':
        ck = (an_asm_operand_constraint_kind)aoc_reg_di;        
        break;
      case 'R':
        ck = (an_asm_operand_constraint_kind)aoc_reg_legacy;    
        break;
      case 'q':
        ck = (an_asm_operand_constraint_kind)aoc_reg_q;         
        break;
      case 'Q':
        ck = (an_asm_operand_constraint_kind)aoc_reg_Q;         
        break;
      case 'A':
        ck = (an_asm_operand_constraint_kind)aoc_reg_ad;        
        break;
      case 't':
        ck = (an_asm_operand_constraint_kind)aoc_reg_float_tos; 
        break;
      case 'u':
        ck = (an_asm_operand_constraint_kind)aoc_reg_float_second;
        break;
      case 'x':
        ck = (an_asm_operand_constraint_kind)aoc_reg_sse;       
        break;
      case 'Y':
        ck = (an_asm_operand_constraint_kind)aoc_reg_sse2;      
        break;
      case 'y':
        ck = (an_asm_operand_constraint_kind)aoc_reg_mmx;       
        break;
      /* Immediates */
      case 'I':
        ck = (an_asm_operand_constraint_kind)aoc_imm_short_shift;
        break;
      case 'J':
        ck = (an_asm_operand_constraint_kind)aoc_imm_long_shift;
        break;
      case 'M':
        ck = (an_asm_operand_constraint_kind)aoc_imm_lea_shift; 
        break;
      case 'K':
        ck = (an_asm_operand_constraint_kind)aoc_imm_signed8;   
        break;
      case 'N':
        ck = (an_asm_operand_constraint_kind)aoc_imm_unsigned8; 
        break;
      case 'L':
        ck = (an_asm_operand_constraint_kind)aoc_imm_and_zext;  
        break;
      case 'G':
        ck = (an_asm_operand_constraint_kind)aoc_imm_80387;     
        break;
      case 'H':
        ck = (an_asm_operand_constraint_kind)aoc_imm_sse;       
        break;
      case 'e':
        ck = (an_asm_operand_constraint_kind)aoc_imm_sext32;    
        break;
      case 'Z':
        ck = (an_asm_operand_constraint_kind)aoc_imm_zext32;    
        break;
      case '@':
        /* "=@ccCOND" is a special case of "=" that allows you to query the
           result of a condition code at the end of your assembly statement.
           Capture COND in a string so it can be used by a back end. */
        if (p[1] == 'c' && p[2] == 'c') {
          ck = aoc_cc;
          cond_code = &p[3];
          /* Skip over the rest of the string. */
          for (p=p+3; *p != '\0'; p++) {}
        } else {
          /* Not something we understand. */
          goto unrecognized;
        }  /* if */
        break;
#endif /* GNU_X86_ASM_EXTENSIONS_ALLOWED */
      default:
#if GNU_X86_ASM_EXTENSIONS_ALLOWED
unrecognized:
#endif /* GNU_X86_ASM_EXTENSIONS_ALLOWED */
#if !ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS
        errletter[0] = *p;
        pos_st_error(ispunct((unsigned char)*p) ? 
                     ec_bad_asm_constraint_modifier : 
                     ec_bad_asm_constraint_letter,
                     &operand->position, errletter);
        error_occurred = TRUE;
#endif /* !ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS */
        break;
    }  /* switch */
    if (ck == aoc_invalid) {
      if (*p == '\0') {
        /* Reached the end of the string; nothing more to do. */
        break;
      }  /* if */
    } else {
      /* Create a new constraint and add it to the list.  */
      *constraint = alloc_asm_operand_constraint(ck);
#if GNU_X86_ASM_EXTENSIONS_ALLOWED
      if (cond_code != NULL) {
        /* Copy any condition code into the IL. */
        (*constraint)->cond_code =
                         (a_const_char *)alloc_il((size_t)(p - cond_code + 1));
        (void)strcpy((char *)(*constraint)->cond_code, cond_code);
        /* We're done parsing the string. */
        break;
      }  /* if */
#endif /* GNU_X86_ASM_EXTENSIONS_ALLOWED */
      constraint = &(*constraint)->next;
    }  /* if */
  }  /* for */
  if (error_occurred) {
    goto error_return;
  }  /* if */
  /* Semantic validation. */
  if (output && !(modifiers & (an_asm_operand_modifier)aom_output)) {
    pos_error(ec_asm_output_must_have_output_mod, &operand->position);
    goto error_return;
  } else if (!output && (modifiers & (an_asm_operand_modifier)aom_output)) {
    pos_error(ec_asm_input_must_not_have_output_mod, &operand->position);
    goto error_return;
  }  /* if */
  if (*number_of_constraints == 0) {
    /* The number of constraints in each constraint string has not yet
       been established; set it now. */
    *number_of_constraints = constraints;
  } else if (*number_of_constraints != constraints &&
             *number_of_constraints != -1) {
    /* Each multi-alternative constraint string must have the same number of
       constraints. */
    pos_error(ec_constraint_number_mismatch, &operand->position);
    /* Use -1 so we don't get multiple instances of the same error. */
    *number_of_constraints = -1;
    goto error_return;
  }  /* if */
  if (validate_expr_for_constraints(expr, operand->constraints)) {
    operand->expression = expr;
    operand->modifiers = modifiers;
  } else {
error_return:
    operand->expression = error_node();
    operand->modifiers = (an_asm_operand_modifier)aom_invalid;
  }  /* if */
#endif /* RECORD_RAW_ASM_OPERAND_DESCRIPTIONS */
}  /* process_asm_operand */


/*
Machine-specific tables used by validate_operands_and_clobbers.
*/
#if !RECORD_RAW_ASM_OPERAND_DESCRIPTIONS
typedef struct single_register_constraint {
  /* Structure to hold a constraint-to-register mapping. */
  an_asm_operand_constraint_kind cons;
  a_named_register               reg;
} single_register_constraint;


STATIC_THREAD single_register_constraint single_register_constraints[] = {
#if GNU_X86_ASM_EXTENSIONS_ALLOWED
  { (an_asm_operand_constraint_kind)aoc_reg_a, (a_named_register)anr_a },
  { (an_asm_operand_constraint_kind)aoc_reg_b, (a_named_register)anr_b },
  { (an_asm_operand_constraint_kind)aoc_reg_c, (a_named_register)anr_c },
  { (an_asm_operand_constraint_kind)aoc_reg_d, (a_named_register)anr_d },
  { (an_asm_operand_constraint_kind)aoc_reg_si, (a_named_register)anr_si },
  { (an_asm_operand_constraint_kind)aoc_reg_di, (a_named_register)anr_di },
  /* 't' and 'u' (x86 reg stack) are not included in this list,
     because the rules are not properly handled by the generic code
     below.  Machine-specific code must be written to handle them. */
#endif /* GNU_X86_ASM_EXTENSIONS_ALLOWED */
  { (an_asm_operand_constraint_kind)aoc_last, (a_named_register)anr_last }
};

#endif /* !RECORD_RAW_ASM_OPERAND_DESCRIPTIONS */

STATIC_THREAD a_named_register fixed_registers[] = {
#if GNU_X86_ASM_EXTENSIONS_ALLOWED
  (a_named_register)anr_bp, (a_named_register)anr_sp,
#endif /* GNU_X86_ASM_EXTENSIONS_ALLOWED */
  (a_named_register)anr_last
};


void validate_operands_and_clobbers(an_asm_entry_ptr  asm_entry)
/*
Validate the given asm entry.  The machine-independent portion of this code
simply checks that no duplicates appear in the clobbers list, no single-
register constraint is used when the corresponding register is clobbered,
and no un-clobberable registers appear in the clobber list.  It also
verifies that symbolic operand names appearing in the asm string itself do
refer to actual operands.  Back end authors should augment this code to do
complete validation of the lists; it is easy to write an asm statement with
unsatisfiable register allocation requirements that does not trip over any
of the machine-independent checks.

Note that this function never modifies the operands or clobbers lists,
even if they are invalid.
*/
{
#if RECORD_RAW_ASM_OPERAND_DESCRIPTIONS
  /* Only raw operand descriptions are recorded: We do not attempt to
     understand the meaning of those descriptions and therefore we do not
     diagnose semantic errors in them.  We can however diagnose duplicate
     clobbers and attempts to clobber fixed registers. */
  a_byte                     regs_clobbered[(int)anr_last];
  a_named_register_list_ptr  clobber, clobbers = asm_entry->clobbers;
  int                        i;
  a_named_register           r;

  memzero((char*)regs_clobbered, sizeof regs_clobbered);
  for (clobber = clobbers; clobber != NULL; clobber = clobber->next) {
    r = clobber->reg;
#if ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS
    /* Ignore entries for unrecognized registers. */
    if (r == (a_named_register)anr_unrecognized) continue;
#endif /* ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS */
    if (r != (a_named_register)anr_invalid && regs_clobbered[(int)r] == 1) {
      /* Test clobbered == 1 so the diagnostic is issued at most once per
         register. */
      pos_st_warning(ec_register_clobbered_twice,
                     &asm_entry->source_corresp.decl_position,
                     named_register_names[(int)r]);
    }  /* if */
    ++regs_clobbered[(int)r];
  }  /* for */
  for (i = 0; fixed_registers[i] != (a_named_register)anr_last; i++) {
    r = fixed_registers[i];
#if ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS
    /* Ignore entries for unrecognized registers. */
    if (r == (a_named_register)anr_unrecognized) continue;
#endif /* ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS */
    if (regs_clobbered[(int)r]) {
      pos_st_error(ec_fixed_register_clobbered,
                   &asm_entry->source_corresp.decl_position,
                   named_register_names[(int)r]);
    }  /* if */
  }  /* for */
#else /* !RECORD_RAW_ASM_OPERAND_DESCRIPTIONS */
  /* Asm operand descriptions are parsed and can therefore be diagnosed for
     consistency. */
  /* For multi-alternative constraints, each set of constraints needs its
     own check.  The local data structures here assume a maximum number of
     constraints (which matches GNU's limit); no checking is done on
     constraints over this limit (a diagnostic is issued). */
#define MAX_CONSTRAINTS 30 /* The maximum number of checked constraints. */
  a_byte                        regs_clobbered[(int)anr_last][MAX_CONSTRAINTS];
  a_byte                        regs_used_in[(int)anr_last][MAX_CONSTRAINTS];
  a_byte                        regs_used_out[(int)anr_last][MAX_CONSTRAINTS];
  an_asm_operand_ptr            aop;
  a_named_register_list_ptr     clobber, clobbers = asm_entry->clobbers;
  an_asm_operand_constraint_ptr c;
  int                           i, constraint, constraint_limit;
  a_named_register              r;

  memzero((char*)regs_clobbered, sizeof regs_clobbered);
  memzero((char*)regs_used_in, sizeof regs_used_in);
  memzero((char*)regs_used_out, sizeof regs_used_out);
  for (aop = asm_entry->operands; aop != NULL; aop = aop->next) {
    for (i = 0;
         single_register_constraints[i].cons !=
                                      (an_asm_operand_constraint_kind)aoc_last;
         i++) {
      a_boolean  input =
                    (aop->modifiers & (an_asm_operand_modifier)aom_input) != 0;
      a_boolean  output =
                   (aop->modifiers & (an_asm_operand_modifier)aom_output) != 0;
      constraint = 0;
      for (c = aop->constraints; c != NULL; c = c->next) {
        if (c->kind == (an_asm_operand_constraint_kind)aoc_end_of_constraint) {
          /* This is a multiple alternative constraint string; each set of
             constraints is checked independently of the others. */
          constraint++;
          if (constraint >= MAX_CONSTRAINTS) {
            /* The number of constraints exceeds the space allocated for
               constraint checking; don't check the remaining constraints. */
            pos_diagnostic(es_discretionary_error, ec_too_many_constraints,
                           &aop->position);
            goto clobber_check;
          }  /* if */
        } else if (c->kind ==
                        (an_asm_operand_constraint_kind)aoc_mod_earlyclobber) {
          if (asm_entry->number_of_constraints == 1 && !clang_mode) {
            /* Indicate that the register is clobbered.  Clang doesn't appear
               to do any earlyclobber checks. */
            input = output = TRUE;
          } else {
            /* GNU seems to be inconsistent about issuing errors related to
               earlyclobbers in multi-alternative constraints.  For example,
               it issues one error in this case:

                 asm("foo %1,%0;" : "=&a,&b" (out) : "b,b" (in)); // Okay
                 asm("foo %1,%0;" : "=&a,&b" (out) : "a,a" (in)); // Error
                 asm("foo %1,%0;" : "=&b,&a" (out) : "b,b" (in)); // Okay
                 asm("foo %1,%0;" : "=&b,&a" (out) : "a,a" (in)); // Okay

               Until a better understanding of GNU's behavior is attained,
               suppress any error checking related to earlyclobbers when
               multi-alternative constraints are used. */
          }  /* if */
        } else if (c->kind == single_register_constraints[i].cons) {
          r = single_register_constraints[i].reg;
#if ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS
          /* Ignore entries for unrecognized registers. */
          if (r == (a_named_register)anr_unrecognized) continue;
#endif /* ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS */
          /* Test used == 1 so the error is issued once per register. */
          if (r != (a_named_register)anr_invalid &&
              ((input && regs_used_in[(int)r][constraint] == 1) ||
               (output && regs_used_out[(int)r][constraint] == 1))) {
            pos_st_error(ec_register_used_twice, &aop->position,
                         named_register_names[(int)r]);
          }  /* if */
          if (input) ++regs_used_in[(int)r][constraint];
          if (output) ++regs_used_out[(int)r][constraint];
        }  /* if */
      }  /* for */
    }  /* for */
  }  /* for */
clobber_check:
  /* Do the requisite checking for each set of constraints (up to the max). */
  constraint_limit = (int)asm_entry->number_of_constraints;
  if (constraint_limit == 0) {
    /* If there were no input/output operands, we still need to do constraint
       checking on the clobbers operand. */
    constraint_limit = 1;
  } else if (constraint_limit > MAX_CONSTRAINTS) {
    /* Constrain the check to the max (a diagnostic has already been
       issued). */
    constraint_limit = MAX_CONSTRAINTS;
  }  /* if */
  for (constraint = 0; constraint < constraint_limit; constraint++) {
    for (clobber = clobbers; clobber != NULL; clobber = clobber->next) {
      r = clobber->reg;
#if ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS
      /* Ignore entries for unrecognized registers. */
      if (r == (a_named_register)anr_unrecognized) continue;
#endif /* ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS */
      if ((regs_used_in[(int)r][constraint] ||
           regs_used_out[(int)r][constraint]) &&
          !regs_clobbered[(int)r][constraint]) {
        /* Test used and not clobbered so the error is issued at most once per
           register. */
        pos_st_error(ec_register_used_and_clobbered,
                     &asm_entry->source_corresp.decl_position,
                     named_register_names[(int)r]);
      } else if (r != (a_named_register)anr_invalid &&
                 regs_clobbered[(int)r][constraint] == 1) {
        /* Test clobbered == 1 so the diagnostic is issued at most once per
           register. */
        pos_st_warning(ec_register_clobbered_twice,
                       &asm_entry->source_corresp.decl_position,
                       named_register_names[(int)r]);
      }  /* if */
      ++regs_clobbered[(int)r][constraint];
    }  /* for */
    for (i = 0; fixed_registers[i] != (a_named_register)anr_last; i++) {
      r = fixed_registers[i];
#if ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS
      /* Ignore entries for unrecognized registers. */
      if (r == (a_named_register)anr_unrecognized) continue;
#endif /* ACCEPT_UNRECOGNIZED_GNU_ASM_OPERANDS */
      if (regs_used_in[(int)r][constraint] ||
          regs_used_out[(int)r][constraint]) {
        pos_st_error(ec_fixed_register_used,
                     &asm_entry->source_corresp.decl_position,
                     named_register_names[(int)r]);
      } else if (regs_clobbered[(int)r][constraint]) {
        pos_st_error(ec_fixed_register_clobbered,
                     &asm_entry->source_corresp.decl_position,
                     named_register_names[(int)r]);
      }  /* if */
    }  /* for */
  }  /* for */
#undef MAX_CONSTRAINTS
#endif /* RECORD_RAW_ASM_OPERAND_DESCRIPTIONS */
  validate_symbolic_operand_and_label_references(asm_entry);
}  /* validate_operands_and_clobbers */


static void asm_operand(an_asm_operand_ptr operand,
                        an_asm_operand_ptr operands,
                        a_boolean          output,
                        a_boolean          *seen_tok_colon_colon,
                        int                *number_of_constraints)
/*
Scan a single asm-statement operand, writing it into the structure pointed to
by operand.  The syntax is

   string-literal ( expression )

optionally preceded by a symbolic name specifier of the form

   [ identifier ]

operands points to the list of operands created so far and output is TRUE
if we're scanning an output operand.  seen_tok_colon_colon maintains
state information for get_token_with_colon_separation.  *number_of_constraints
is used to ensure that the number of constraints in a multi-alternative
constraint string are the same across all input and output specifications of a
particular asm statement.
*/
{
  a_const_char     *constraint_string = NULL;
  an_expr_node_ptr expr = NULL;

  db_enter(4, "asm_operand");
  add_stop_token(tok_comma);
  add_stop_token(tok_colon);
  add_stop_token(tok_colon_colon);
  operand->position = pos_curr_token;
  if (curr_token == tok_lbracket) {
    /* Presumably a named operand.  The next token must be an identifier. */
    (void)get_token_with_colon_separation(seen_tok_colon_colon);
    add_stop_token(tok_rbracket);
    if (curr_token != tok_identifier) {
      syntax_error(ec_exp_identifier);
    } else {
      /* Record the identifier as the name of the operand. */
      a_symbol_header  *sym_hdr = locator_for_curr_id.symbol_header;
      operand->name = alloc_il(sym_hdr->identifier_length+1);
      (void)strcpy(operand->name, sym_hdr->identifier);
      (void)get_token_with_colon_separation(seen_tok_colon_colon);
    }  /* if */
    if (curr_token != tok_rbracket) {
      syntax_error(ec_exp_rbracket);
    }  /* if */
    (void)get_token_with_colon_separation(seen_tok_colon_colon);
    remove_stop_token(tok_rbracket);
  }  /* if */
  if (curr_token != tok_string_literal) {
    syntax_error(ec_exp_string_literal);
  } else if (!is_normal_character_kind(const_for_curr_token.character_kind)) {
    syntax_error(ec_wide_string_invalid_in_asm);
  } else {
    constraint_string = const_for_curr_token.variant.string.value;
    /* Advance past string literal. */
    (void)get_token_with_colon_separation(seen_tok_colon_colon);
    if (curr_token == tok_lparen) {
      a_boolean  input = !output, is_memory_operand = FALSE;
      /* Note that in this context (i.e., scanning an expression),
         treat "::" as tok_colon_colon; it may be a namespace delimiter. */
      (void)get_token();
      if (constraint_string != NULL) {
        /* Take a peek at the constraint string to determine how to
           scan the operand expression (the constraint string is examined
           in more detail by process_asm_operand below). */
        a_const_char *cp;
        if (output) {
          /* A '+' in the constraint string of an output operand indicates a
             read-modify-write instruction; i.e., the operand is first an input
             operand and then an output operand. */
          input = (strchr(constraint_string, '+') != NULL);
        }  /* if */
        for (cp = constraint_string; *cp != '\0'; cp++) {
          /* The constraints below indicate some form of a memory operand
             (meaning that the operand should not be converted). */
          if (*cp == 'm' || *cp == 'o' || *cp == 'v' || *cp == 'g' ||
              *cp == '>' || *cp == '<') {
            is_memory_operand = TRUE;
            break;
          }  /* if */
        }  /* for */
      }  /* if */
      add_stop_token(tok_rparen);
      expr = scan_asm_operand_expression(output, input, is_memory_operand);
      if (curr_token == tok_rparen) {
        (void)get_token_with_colon_separation(seen_tok_colon_colon);
      } else if (is_error_node(expr)) {
        flush_to_closing_paren();
        (void)required_token(tok_rparen, ec_exp_rparen);
      } else {
        syntax_error(ec_exp_rparen);
      }  /* if */
      remove_stop_token(tok_rparen);
    } else {
      syntax_error(ec_exp_lparen);
    }  /* if */
  }  /* if */
  process_asm_operand(operand, operands, expr, constraint_string, output,
                      number_of_constraints);
  remove_stop_token(tok_comma);
  remove_stop_token(tok_colon);
  remove_stop_token(tok_colon_colon);
  db_exit();
}  /* asm_operand */


an_asm_operand_ptr asm_operands_spec(a_boolean *seen_tok_colon_colon,
                                     int       *number_of_constraints)
/*
Parse and validate a list of asm-statement operands.  This handles both input
and output operands.  The list is returned as a sequence of an_asm_operand
entries.  *number_of_constraints is used to ensure that the number of
constraints in a multi-alternative constraint string are the same across
all input and output specifications of a particular asm statement.

On entry, curr_token is the leading colon of the operands specification; on
exit, it is the leading colon of the clobbers specification, or the close
parenthesis if there are no clobbers.

The syntax is

    : [operand [, operand...]]   // outputs
   [: [operand [, operand...]]]  // inputs

Operand lists and clobbers can be empty, leading to cases where
two adjacent colons are parsed (in C++) as a single tok_colon_colon.  To
get around this case (without undue complexity), use
get_token_with_colon_separation rather than get_token to return two
tok_colon tokens rather than a single tok_colon_colon.  See
get_token_with_colon_separation for a description of seen_tok_colon_colon.
*/
{
  a_boolean          output = TRUE;
  an_asm_operand_ptr operands = NULL;
  an_asm_operand_ptr *p_operands = &operands;

  db_enter(3, "asm_operands_spec");
  check_assertion(curr_token == tok_colon);
  report_gnu_extension_if_needed(&pos_curr_token,
                                 ec_asm_operand_spec_is_gnu_extension);
  /* Skip initial :. */
  (void)get_token_with_colon_separation(seen_tok_colon_colon);
  /* If the output list is empty, we'll be at another colon. */
  if (curr_token == tok_colon) {
    output = FALSE;
    (void)get_token_with_colon_separation(seen_tok_colon_colon);
  }  /* if */
  while (curr_token == tok_string_literal || curr_token == tok_lbracket) {
    *p_operands = alloc_asm_operand();
    asm_operand(*p_operands, operands, output, seen_tok_colon_colon,
                number_of_constraints);
    p_operands = &(*p_operands)->next;
    /* Next must be a comma, colon, or right paren. */
    if (curr_token == tok_colon) {
      if (output) {
        /* End of output list; consume colon and continue. */
        output = FALSE;
        (void)get_token_with_colon_separation(seen_tok_colon_colon);
      }  /* if */
    } else if (curr_token == tok_comma) {
      (void)get_token_with_colon_separation(seen_tok_colon_colon);
      if (curr_token != tok_string_literal && curr_token != tok_lbracket) {
        syntax_error(ec_exp_asm_operand);
      }  /* if */
    }  /* if */
  }  /* while */
  db_exit();
  return operands;
}  /* asm_operands_spec */


a_named_register_list_ptr asm_clobbers_spec(a_boolean *seen_tok_colon_colon)
/*
Parse and validate a list of asm-statement clobbers.  Returns the list of
registers clobbered.

The syntax is

   string-literal [, string-literal ...]

Operand lists and clobbers can be empty, leading to cases where
two adjacent colons are parsed (in C++) as a single tok_colon_colon.  To
get around this case (without undue complexity), use
get_token_with_colon_separation rather than get_token to return two
tok_colon tokens rather than a single tok_tolon_colon.  See
get_token_with_colon_separation for a description of seen_tok_colon_colon.
*/
{
  /* There is no hard limit on the number of clobbers. */
  a_named_register           reg;
  int                        nparsed = 0;
  a_const_char               *name;
  a_named_register_list_ptr  first_reg = NULL, last_reg = NULL;

  db_enter(3, "asm_clobbers_spec");
  if (curr_token == tok_colon) {
    (void)get_token_with_colon_separation(seen_tok_colon_colon);
    while (curr_token == tok_string_literal) {
      nparsed++;
      name = const_for_curr_token.variant.string.value;
      if (strcmp(name, "memory") == 0) {
        /* The string "memory" can appear in place of a register name.  */
        reg = (a_named_register)anr_memory;
      } else if (strcmp(name, "cc") == 0) {
        pos_warning(ec_cc_clobber_ignored, &error_position);
        goto skip_item;
      } else {
        reg = name_to_register(name);
      }  /* if */
      if (reg == (a_named_register)anr_invalid) {
        pos_st_error(ec_bad_reg_name, &pos_curr_token, name);
      } else {
        /* Add this register to our list. */
        if (first_reg == NULL) {
          first_reg = last_reg = alloc_named_register_list();
        } else {
          check_assertion(last_reg != NULL);
          last_reg->next = alloc_named_register_list();
          last_reg = last_reg->next;
        }  /* if */
        last_reg->reg = reg;
      }  /* if */
skip_item:
      /* Advance past the string literal. */
      (void)get_token_with_colon_separation(seen_tok_colon_colon);
      /* The next token must be a comma or a right parenthesis. */
      if (curr_token == tok_comma) {
        (void)get_token_with_colon_separation(seen_tok_colon_colon);
        if (curr_token != tok_string_literal) {
          syntax_error(ec_exp_asm_clobber);
        }  /* if */
      }  /* if */
    }  /* while */
    /* GNU C versions prior to 4.5 treat an empty clobbers list with a colon as
       a syntax error.  We can parse it correctly, so it's semantic for us.
       Don't issue this error if we saw anything other than a colon immediately
       followed by a right parenthesis.  In the "asm goto" case, a fourth
       colon follows the clobber list. */
    if (curr_token != tok_rparen && curr_token != tok_colon) {
      syntax_error(ec_exp_rparen);
    } else if (nparsed == 0 && gcc_mode && gnu_version < 40500) {
      pos_error(ec_empty_clobbers_list, &pos_curr_token);
    }  /* if */
  }  /* if */
  db_exit();
  return first_reg;
}  /* asm_clobbers_spec */


a_label_list_ptr asm_labels_spec(a_boolean *seen_tok_colon_colon)
/*
Parse and validate a list of asm-statement labels that appear after the
fourth colon in an "asm goto" statement and return the list.

The syntax is

   label [, label ...]

Operand lists and clobbers can be empty, leading to cases where
two adjacent colons are parsed (in C++) as a single tok_colon_colon.  To
get around this case (without undue complexity), use
get_token_with_colon_separation rather than get_token to return two
tok_colon tokens rather than a single tok_tolon_colon.  See
get_token_with_colon_separation for a description of seen_tok_colon_colon.
*/
{
  /* There is no hard limit on the number of labels. */
  int               nparsed = 0;
  a_label_ptr       label;
  a_label_list_ptr  first_label = NULL, last_label = NULL;

  db_enter(3, "asm_labels_spec");
  if (curr_token == tok_colon) {
    (void)get_token_with_colon_separation(seen_tok_colon_colon);
    while (curr_token == tok_identifier) {
      nparsed++;
      label = scan_label(/*is_definition=*/FALSE, /*is_declaration=*/FALSE);
      check_assertion(label != NULL);
      /* Add this label to our list. */
      if (first_label == NULL) {
        first_label = last_label = alloc_label_list();
      } else {
        check_assertion(last_label != NULL);
        last_label->next = alloc_label_list();
        last_label = last_label->next;
      }  /* if */
      last_label->label = label;
      /* The next token must be a comma or a right parenthesis. */
      if (curr_token == tok_comma) {
        (void)get_token_with_colon_separation(seen_tok_colon_colon);
        if (curr_token != tok_identifier) {
          syntax_error(ec_exp_asm_label);
        }  /* if */
      }  /* if */
    }  /* while */
    /* Make sure at least one label was specified. */
    if (curr_token != tok_rparen) {
      syntax_error(ec_exp_rparen);
    } else if (nparsed == 0) {
      pos_error(ec_exp_asm_label, &pos_curr_token);
    }  /* if */
  } else {
    syntax_error(ec_exp_colon);
  }  /* if */
  db_exit();
  return first_label;
}  /* asm_labels_spec */


static a_boolean compare_n2r(name_to_reg const  &x,
                             name_to_reg const  &y)
/*
Return TRUE if the register name of x comes before that of y.
*/
{
  return strcmp(x.name, y.name) < 0;
}  /* compare_n2r */


void extasm_one_time_init(void)
/*
Do one-time initialization of variables related to the processing of
extended asm statements.
*/
{
  int i;
#if CHECKING
  /* Check that the table of mode names is correctly initialized. */
  if (named_register_names[(int)anr_last] == NULL ||
      strcmp(named_register_names[(int)anr_last], "last") != 0) {
    internal_error(
      "extasm_one_time_init: named_register_names: bad init");
  }  /* if */
#if !RECORD_RAW_ASM_OPERAND_DESCRIPTIONS
  /* Ditto the table of constraint letters. */
  if (asm_operand_constraint_letters[(int)aoc_last] != '~') {
    internal_error(
      "extasm_one_time_init: asm_operand_constraint_letters: bad init");
  }  /* if */
#endif /* !RECORD_RAW_ASM_OPERAND_DESCRIPTIONS */
#endif /* CHECKING */
  /* Set up the complete regmap table, including both the official and
     extra register names.  regmap does not include entries for
     anr_invalid or anr_last. */
  regmap_size = (int)anr_last - 1;
  regmap_size += (sizeof(extra_reg_names) /
                  sizeof(struct name_to_reg)) - 1;  /*lint !e845*/
  regmap = (struct name_to_reg *)alloc_general(
                         (sizeof_t)(regmap_size * sizeof(struct name_to_reg)));
  /* Start with i = 1 since anr_invalid is not copied. */
  for (i = 1; i < (int)anr_last; i++) { /*lint !e681*/
    regmap[i-1].name = named_register_names[i];
    regmap[i-1].reg = (a_named_register)i;
  }  /* for */
  /* Copy the extra register information. */
  (void)memcpy((char*)&regmap[(int)anr_last-1], (char*)extra_reg_names,
               size_t_arg(sizeof(extra_reg_names) -
                                                  sizeof(struct name_to_reg)));
  /* name_to_register requires that regmap be sorted. */
  sort(regmap, regmap + regmap_size, compare_n2r);
  /* Save variables from extasm.h and extasm.c that are needed for
     precompiled headers */
  if (precompiled_header_processing_required) {
    STATIC_THREAD a_pch_saved_variable saved_vars[] = {
      pch_saved_var_array_terminating_elem()
    };
    register_pch_saved_variables(saved_vars);
  }  /* if */
}  /* extasm_one_time_init */

/* Conditionally close the "edg" namespace. */
END_EDG_NAMESPACE

#endif /* GNU_EXTENSIONS_ALLOWED */

