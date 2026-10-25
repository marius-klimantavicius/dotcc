/* Ordinary Linux x86-64 alternate-stack lifecycle; guard page stays untouched. */
.global _start
.equ STACK_SIZE, 16384
.equ GUARD_SIZE, 4096
.section .text
_start:
    mov %rsp, %r12
    mov $0x123456789abcdef0, %r13
    mov $186, %eax                   /* gettid */
    syscall
    mov %eax, main_tid(%rip)
    mov $9, %eax                     /* mmap RW, then protect the low page */
    xor %edi, %edi
    mov $STACK_SIZE, %esi
    mov $3, %edx
    mov $0x22, %r10d
    mov $-1, %r8
    xor %r9d, %r9d
    syscall
    cmp $-4095, %rax
    jae .Lfail_map
    mov %rax, stack_descriptor(%rip)
    mov %rax, %rdi
    mov $GUARD_SIZE, %esi
    xor %edx, %edx
    mov $10, %eax                    /* mprotect(PROT_NONE) */
    syscall
    test %rax, %rax
    jne .Lfail_protect
    mov $131, %eax                   /* query initial disabled stack */
    xor %edi, %edi
    lea queried(%rip), %rsi
    syscall
    test %rax, %rax
    jne .Lfail_query
    testl $2, queried+8(%rip)
    je .Lfail_query
    mov $131, %eax                   /* descriptor INCLUDES the guard */
    lea stack_descriptor(%rip), %rdi
    lea queried(%rip), %rsi
    syscall
    test %rax, %rax
    jne .Lfail_register
    testl $2, queried+8(%rip)
    je .Lfail_query
    call .Lquery_outside
    mov $13, %eax                    /* SIGUSR1, SIGINFO|RESTORER|ONSTACK */
    mov $10, %edi
    lea action(%rip), %rsi
    xor %edx, %edx
    mov $8, %r10d
    syscall
    test %rax, %rax
    jne .Lfail_action
    call .Lsend
    cmpl $1, handled(%rip)
    jne .Lfail_return
    cmp %r12, %rsp
    jne .Lfail_return
    mov $0x123456789abcdef0, %rax
    cmp %rax, %r13
    jne .Lfail_return
    call .Lquery_outside
    call .Lsend                      /* registration remains usable on return */
    cmpl $2, handled(%rip)
    jne .Lfail_return
    cmp %r12, %rsp
    jne .Lfail_return
    mov $0x123456789abcdef0, %rax
    cmp %rax, %r13
    jne .Lfail_return
    call .Lquery_outside
    mov $131, %eax                   /* disable, retaining old descriptor */
    lea disabled(%rip), %rdi
    lea queried(%rip), %rsi
    syscall
    test %rax, %rax
    jne .Lfail_disable
    call .Lcheck_descriptor
    cmpl $0, queried+8(%rip)
    jne .Lfail_disable
    mov $131, %eax
    xor %edi, %edi
    lea queried(%rip), %rsi
    syscall
    test %rax, %rax
    jne .Lfail_disable
    testl $2, queried+8(%rip)
    je .Lfail_disable
    mov $13, %eax                    /* restore SIG_DFL before releasing stack */
    mov $10, %edi
    lea default_action(%rip), %rsi
    xor %edx, %edx
    mov $8, %r10d
    syscall
    test %rax, %rax
    jne .Lfail_action
    mov $11, %eax
    mov stack_descriptor(%rip), %rdi
    mov $STACK_SIZE, %esi
    syscall
    test %rax, %rax
    jne .Lfail_unmap
    lea success(%rip), %rsi
    mov $(success_end-success), %edx
.Lwrite:
    mov $1, %eax
    mov $1, %edi
    syscall
    test %rax, %rax
    jle .Lfail_write
    cmp %rdx, %rax
    ja .Lfail_write
    add %rax, %rsi
    sub %rax, %rdx
    jne .Lwrite
    xor %edi, %edi
    jmp .Lexit

.Lsend:
    mov $200, %eax                   /* ordinary self tkill(SIGUSR1) */
    mov main_tid(%rip), %edi
    mov $10, %esi
    syscall
    test %rax, %rax
    jne .Lfail_send
    ret
.Lquery_outside:
    mov $131, %eax
    xor %edi, %edi
    lea queried(%rip), %rsi
    syscall
    test %rax, %rax
    jne .Lfail_query
    cmpl $0, queried+8(%rip)
    jne .Lfail_query
.Lcheck_descriptor:
    mov stack_descriptor(%rip), %rax
    cmp %rax, queried(%rip)
    jne .Lfail_query
    cmpq $STACK_SIZE, queried+16(%rip)
    jne .Lfail_query
    ret

handler:
    cmp $10, %edi
    jne .Lfail_handler
    cmpl $10, (%rsi)
    jne .Lfail_handler
    mov stack_descriptor(%rip), %rax
    add $GUARD_SIZE, %rax
    cmp %rax, %rsp
    jb .Lfail_handler
    add $(STACK_SIZE-GUARD_SIZE), %rax
    cmp %rax, %rsp
    jae .Lfail_handler
    mov $131, %eax                   /* actual handler observes SS_ONSTACK */
    xor %edi, %edi
    lea queried(%rip), %rsi
    syscall
    test %rax, %rax
    jne .Lfail_handler
    cmpl $1, queried+8(%rip)
    jne .Lfail_handler
    call .Lcheck_descriptor
    inc %r13                        /* sigreturn must restore saved register */
    incl handled(%rip)
    ret
restorer:
    mov $15, %eax                    /* actual rt_sigreturn */
    syscall
    jmp .Lfail_return

.Lfail_map:      mov $21, %edi; jmp .Lexit
.Lfail_protect:  mov $22, %edi; jmp .Lexit
.Lfail_query:    mov $23, %edi; jmp .Lexit
.Lfail_register: mov $24, %edi; jmp .Lexit
.Lfail_action:   mov $25, %edi; jmp .Lexit
.Lfail_send:     mov $26, %edi; jmp .Lexit
.Lfail_handler:  mov $27, %edi; jmp .Lexit
.Lfail_return:   mov $28, %edi; jmp .Lexit
.Lfail_disable:  mov $29, %edi; jmp .Lexit
.Lfail_unmap:    mov $30, %edi; jmp .Lexit
.Lfail_write:    mov $31, %edi
.Lexit:
    mov $231, %eax
    syscall

.section .rodata
success: .ascii "guest-signal-stack: guard=4096 registered=16384 onstack=2 returned=2 disabled=1\n"
success_end:
.section .data
.balign 8
stack_descriptor: .quad 0, 0, STACK_SIZE
disabled: .quad 0, 2, 0
action: .quad handler, 0x0c000004, restorer, 0
default_action: .quad 0, 0, 0, 0
.section .bss
.balign 8
queried: .skip 24
main_tid: .skip 4
handled: .skip 4
.section .note.GNU-stack,"",@progbits
