/* SOURCE-AUTHORED / UNRUN. Linux guest SysV ABI, independent of host ABI. */
.text
.globl rep_context_compare
.type rep_context_compare,@function
rep_context_compare:
    mov %rcx,%r9
    mov %rdi,%r8
    mov %rsi,%rdi
    mov %r8,%rsi
    mov %rdx,%rcx
    xor %eax,%eax
    stc
.globl rep_context_site
rep_context_site:
    repe cmpsb
    pushfq
    pop %r8
    cld /* capture edited REP DF, then honor the guest C ABI on return */
    mov %rcx,0(%r9)
    mov %rsi,8(%r9)
    mov %rdi,16(%r9)
    mov %r8,24(%r9)
    ret
.size rep_context_compare,.-rep_context_compare

.globl rep_handler_copy
.type rep_handler_copy,@function
rep_handler_copy:
    mov %rdi,%r8
    mov %rsi,%rdi
    mov %r8,%rsi
    mov %rdx,%rcx
    cld
    rep movsb
    ret
.size rep_handler_copy,.-rep_handler_copy
.section .note.GNU-stack,"",@progbits
