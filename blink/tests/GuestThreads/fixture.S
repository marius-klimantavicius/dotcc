/* Ordinary Linux x86-64 clone/TLS/futex lifecycle. No C runtime or host driver. */
.global _start
.section .text
_start:
    mov $158, %eax                 /* arch_prctl(ARCH_SET_FS, main_tls) */
    mov $0x1002, %edi
    lea main_tls(%rip), %rsi
    syscall
    test %rax, %rax
    jne .Lfail_tls
    mov $186, %eax                 /* main gettid, preserved across clone */
    syscall
    test %eax, %eax
    jle .Lfail_tid
    mov %eax, %r13d
    mov $56, %eax
    mov $0x7d0f00, %edi             /* actual observed musl clone flags */
    lea child_stack_end(%rip), %rsi
    lea child_tid(%rip), %rdx       /* parent_tid and clear_child_tid share storage */
    mov %rdx, %r10
    lea child_tls(%rip), %r8
    syscall
    test %rax, %rax
    je .Lchild
    js .Lfail_clone
    mov %eax, %r12d
    cmp %r13d, %r12d
    je .Lfail_tid
    cmp child_tid(%rip), %r12d
    jne .Lfail_tid

.Lparent_ready:
    lea ready(%rip), %rdi
    mov $128, %esi                 /* FUTEX_WAIT_PRIVATE, expected zero */
    xor %edx, %edx
    call .Lwait
    cmpl $1, ready(%rip)
    jne .Lparent_ready
    cmp observed_child_tid(%rip), %r12d
    jne .Lfail_tid
    cmpq $0x11112222, %fs:0
    jne .Lfail_tls
    cmpq $17, %fs:8
    jne .Lfail_tls
    cmpq $42, child_tls+8(%rip)
    jne .Lfail_tls
    cmpl $42, shared_value(%rip)
    jne .Lfail_shared
    cmpq $85, child_stack_end-64(%rip)
    jne .Lfail_stack
    mov $1, %eax
    xchg %eax, released(%rip)
    lea released(%rip), %rdi
    call .Lwake_private

.Lparent_clear:
    lea child_tid(%rip), %rdi
    xor %esi, %esi                 /* FUTEX_WAIT: Linux clear-TID wake is NOT PRIVATE */
    mov %r12d, %edx
    call .Lwait
    cmpl $0, child_tid(%rip)
    jne .Lparent_clear
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
    lea child_stack_end(%rip), %rax
    cmp %rax, %rsp
    jne .Lfail_stack
    movq $85, -64(%rsp)
    cmpq $0x55556666, %fs:0
    jne .Lfail_tls
    cmpq $0, %fs:8
    jne .Lfail_tls
    movq $42, %fs:8
    mov $186, %eax
    syscall
    test %eax, %eax
    jle .Lfail_tid
    cmp %r13d, %eax
    je .Lfail_tid
    mov %eax, observed_child_tid(%rip)
    movl $42, shared_value(%rip)
    mov $1, %eax
    xchg %eax, ready(%rip)
    lea ready(%rip), %rdi
    call .Lwake_private
.Lchild_release:
    lea released(%rip), %rdi
    mov $128, %esi
    xor %edx, %edx
    call .Lwait
    cmpl $1, released(%rip)
    jne .Lchild_release
    xor %edi, %edi
    mov $60, %eax                  /* child exit, kernel/Blink clears and wakes TID */
    syscall
    mov $18, %edi                  /* exit must not return */
    jmp .Lexit_group

.Lwait:
    xor %r10d, %r10d               /* no timeout: protocol gates determine progress */
    xor %r8d, %r8d
    xor %r9d, %r9d
    mov $202, %eax
    syscall
    test %rax, %rax
    je .Lwait_return
    cmp $-11, %rax                 /* ordinary changed-value race */
    je .Lwait_return
    cmp $-4, %rax                  /* ordinary interrupted wait: caller rechecks word */
    jne .Lfail_futex
.Lwait_return:
    ret
.Lwake_private:
    mov $202, %eax
    mov $129, %esi                 /* FUTEX_WAKE_PRIVATE */
    mov $1, %edx
    xor %r10d, %r10d
    xor %r8d, %r8d
    xor %r9d, %r9d
    syscall
    test %rax, %rax
    js .Lfail_futex
    ret

.Lfail_tls:
    mov $11, %edi
    jmp .Lexit_group
.Lfail_tid:
    mov $12, %edi
    jmp .Lexit_group
.Lfail_clone:
    mov $13, %edi
    jmp .Lexit_group
.Lfail_shared:
    mov $14, %edi
    jmp .Lexit_group
.Lfail_stack:
    mov $15, %edi
    jmp .Lexit_group
.Lfail_futex:
    mov $16, %edi
    jmp .Lexit_group
.Lfail_write:
    mov $17, %edi
.Lexit_group:
    mov $231, %eax
    syscall
.Lunexpected_return:
    jmp .Lunexpected_return

.section .rodata
success:
    .ascii "guest-threads: tls=isolated shared=42 tid=cleared\n"
success_end:
.section .data
.balign 16
main_tls:
    .quad 0x11112222, 17
child_tls:
    .quad 0x55556666, 0
.section .bss
.balign 4
child_tid: .zero 4
observed_child_tid: .zero 4
ready: .zero 4
released: .zero 4
shared_value: .zero 4
.balign 16
child_stack:
    .zero 65536
child_stack_end:
.section .note.GNU-stack,"",@progbits
