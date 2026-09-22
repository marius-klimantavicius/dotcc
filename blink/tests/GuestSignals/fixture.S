/* Normal Linux user-signal delivery; fixed valid memory and no C runtime. */
.global _start, handler_start_syscall, handler_end_syscall
.section .text
_start:
    xor %r15d, %r15d             /* finite ordinary futex retry count per thread */
    mov $39, %eax                 /* expected sender process and user */
    syscall
    mov %eax, process_id(%rip)
    mov $102, %eax
    syscall
    mov %eax, user_id(%rip)
    mov $186, %eax
    syscall
    mov %eax, main_tid(%rip)
    mov $13, %eax                 /* rt_sigaction(SIGUSR1, &action, 0, 8) */
    mov $10, %edi
    lea action(%rip), %rsi
    xor %edx, %edx
    mov $8, %r10d
    syscall
    test %rax, %rax
    jne .Lfail_action
    mov $200, %eax                /* immediate self tkill */
    mov main_tid(%rip), %edi
    mov $10, %esi
    syscall
    test %rax, %rax
    jne .Lfail_send
    cmpl $1, main_count(%rip)
    jne .Lfail_count
    mov $14, %eax                 /* block before clone; child inherits */
    xor %edi, %edi
    lea usr1_mask(%rip), %rsi
    xor %edx, %edx
    mov $8, %r10d
    syscall
    test %rax, %rax
    jne .Lfail_mask
    mov $56, %eax
    mov $0x7d0f00, %edi
    lea child_stack_end(%rip), %rsi
    lea child_tid(%rip), %rdx
    mov %rdx, %r10
    lea child_tls(%rip), %r8
    syscall
    test %rax, %rax
    je .Lchild
    js .Lfail_clone
    mov %eax, %r12d
.Lparent_ready:
    lea ready(%rip), %rdi
    mov $128, %esi
    xor %edx, %edx
    call .Lwait
    cmpl $1, ready(%rip)
    jne .Lparent_ready
    mov $200, %eax                /* target remains masked until release */
    mov %r12d, %edi
    mov $10, %esi
    syscall
    test %rax, %rax
    jne .Lfail_send
    mov $1, %eax
    xchg %eax, released(%rip)
    lea released(%rip), %rdi
    call .Lwake
.Lparent_clear:
    lea child_tid(%rip), %rdi
    xor %esi, %esi                /* ordinary non-private clear-TID wait */
    mov %r12d, %edx
    call .Lwait
    cmpl $0, child_tid(%rip)
    jne .Lparent_clear
    cmpl $1, main_count(%rip)
    jne .Lfail_count
    cmpl $1, child_count(%rip)
    jne .Lfail_count
    lea success(%rip), %r14
    mov $(success_end-success), %ebp
.Lwrite:
    mov $1, %eax
    mov $1, %edi
    mov %r14, %rsi
    mov %ebp, %edx
    syscall
    test %rax, %rax
    jle .Lfail_write
    cmp %rbp, %rax
    ja .Lfail_write
    add %rax, %r14
    sub %eax, %ebp
    jne .Lwrite
    xor %edi, %edi
    jmp .Lexit_group
.Lchild:
    mov $1, %eax
    xchg %eax, ready(%rip)
    lea ready(%rip), %rdi
    call .Lwake
.Lchild_release:
    lea released(%rip), %rdi
    mov $128, %esi
    xor %edx, %edx
    call .Lwait
    cmpl $1, released(%rip)
    jne .Lchild_release
    cmpl $0, child_count(%rip)
    jne .Lfail_count
    mov $127, %eax                /* prove actual masked pending bit */
    lea pending(%rip), %rdi
    mov $8, %esi
    syscall
    test %rax, %rax
    jne .Lfail_pending
    testq $512, pending(%rip)
    je .Lfail_pending
    mov $14, %eax                 /* SIG_UNBLOCK: recursive upstream delivery */
    mov $1, %edi
    lea usr1_mask(%rip), %rsi
    xor %edx, %edx
    mov $8, %r10d
    syscall
    test %rax, %rax
    jne .Lfail_mask
    cmpl $1, child_count(%rip)
    jne .Lfail_count
    xor %edi, %edi
    mov $60, %eax
    syscall
    mov $29, %edi
    jmp .Lexit_group

handler:
    cmp $10, %edi
    jne .Lfail_info
    cmpl $10, (%rsi)
    jne .Lfail_info
    cmpl $-6, 8(%rsi)             /* SI_TKILL, actual sender PID/UID */
    jne .Lfail_info
    mov process_id(%rip), %eax
    cmp %eax, 16(%rsi)
    jne .Lfail_info
    mov user_id(%rip), %eax
    cmp %eax, 20(%rsi)
    jne .Lfail_info
    mov $186, %eax
handler_start_syscall:
    syscall
    mov %eax, %r13d
    xor %r9d, %r9d
    mov $4096, %r8d
.Lhandler_loop:
    add $3, %r9d
    dec %r8d
    jne .Lhandler_loop
    cmp $12288, %r9d
    jne .Lfail_count
    mov $186, %eax
handler_end_syscall:
    syscall
    cmp %r13d, %eax
    jne .Lfail_info
    cmp main_tid(%rip), %eax
    jne .Lchild_handler
    incl main_count(%rip)
    ret
.Lchild_handler:
    cmp child_tid(%rip), %eax
    jne .Lfail_info
    incl child_count(%rip)
    ret
restorer:
    mov $15, %eax                 /* actual rt_sigreturn */
    syscall
    mov $30, %edi
    jmp .Lexit_group
.Lwait:
    inc %r15d
    cmp $32, %r15d
    ja .Lfail_futex
    xor %r10d, %r10d
    xor %r8d, %r8d
    xor %r9d, %r9d
    mov $202, %eax
    syscall
    test %rax, %rax
    je .Lwait_return
    cmp $-11, %rax
    je .Lwait_return
    cmp $-4, %rax
    jne .Lfail_futex
.Lwait_return:
    ret
.Lwake:
    mov $202, %eax
    mov $129, %esi
    mov $1, %edx
    xor %r10d, %r10d
    xor %r8d, %r8d
    xor %r9d, %r9d
    syscall
    test %rax, %rax
    js .Lfail_futex
    ret
.Lfail_action:  mov $21, %edi; jmp .Lexit_group
.Lfail_send:    mov $22, %edi; jmp .Lexit_group
.Lfail_count:   mov $23, %edi; jmp .Lexit_group
.Lfail_mask:    mov $24, %edi; jmp .Lexit_group
.Lfail_clone:   mov $25, %edi; jmp .Lexit_group
.Lfail_pending: mov $26, %edi; jmp .Lexit_group
.Lfail_write:   mov $27, %edi; jmp .Lexit_group
.Lfail_info:    mov $28, %edi; jmp .Lexit_group
.Lfail_futex:   mov $31, %edi
.Lexit_group:
    mov $231, %eax
    syscall
.Lunexpected_return:
    jmp .Lunexpected_return
.section .rodata
success: .ascii "guest-signals: self=1 child=1 sender=checked pending=checked\n"
success_end:
.section .data
.balign 8
action: .quad handler, 0x04000004, restorer, 0 /* SA_RESTORER|SA_SIGINFO */
usr1_mask: .quad 512
child_tls: .quad 0, 0
.section .bss
.balign 8
pending: .zero 8
process_id: .zero 4
user_id: .zero 4
main_tid: .zero 4
child_tid: .zero 4
main_count: .zero 4
child_count: .zero 4
ready: .zero 4
released: .zero 4
.balign 16
child_stack: .zero 65536
child_stack_end:
.section .note.GNU-stack,"",@progbits
