/* Ordinary static Linux clone/clear-TID lifecycle; no runtime or native driver.
 * c: managed capacity (main + two held children, next clone must be EAGAIN).
 * n: native capacity witness (same protocol, skips managed-only policy check).
 * b: cancellation with both workers awaiting futexes. j: main exits first.
 * Other arguments run 96 sequential children under a two-worker owner limit.
 */
.global _start
.section .text
_start:
    cmpq $2, (%rsp)
    jne .Lfail
    mov 16(%rsp), %rax
    movzbl (%rax), %r15d
    cmp $'c', %r15d
    je .Lcapacity
    cmp $'n', %r15d
    je .Lcapacity
    cmp $'b', %r15d
    je .Lblocked
    cmp $'j', %r15d
    je .Ljoin
    jmp .Lchurn

.Lcapacity:
    lea slot1(%rip), %r12
    call .Lspawn
    call .Lready
    lea slot2(%rip), %r12
    call .Lspawn
    call .Lready
    cmp $'n', %r15d
    je .Lrelease_capacity
    lea slot3(%rip), %r12
    call .Lclone
    cmp $-11, %rax                 /* managed owner cap, not host exhaustion */
    jne .Lfail
.Lrelease_capacity:
    lea slot1(%rip), %r12
    call .Lrelease
    lea slot2(%rip), %r12
    call .Lrelease
    lea slot1(%rip), %r12
    call .Lcleared
    lea slot2(%rip), %r12
    call .Lcleared

.Lchurn:
    mov $96, %r14d
.Lround:
    lea slot1(%rip), %r12
    call .Lspawn
    call .Lready
    call .Lrelease
    call .Lcleared
    dec %r14d
    jne .Lround
    lea success(%rip), %rsi
    mov $(success_end-success), %edx
    call .Lwrite
    xor %edi, %edi
    jmp .Lexit_group

.Lblocked:
    lea slot1(%rip), %r12
    call .Lspawn
    call .Lready
    lea blocked(%rip), %rsi
    mov $(blocked_end-blocked), %edx
    call .Lwrite
.Lblock_forever:
    lea never(%rip), %rdi
    mov $128, %esi
    xor %edx, %edx
    call .Lwait
    jmp .Lblock_forever

.Ljoin:
    mov $186, %eax
    syscall
    mov %eax, main_tid(%rip)
    mov $218, %eax                /* child waits for main's clear-TID, no delay */
    lea main_tid(%rip), %rdi
    syscall
    cmp main_tid(%rip), %eax
    jne .Lfail
    lea slot1(%rip), %r12
    call .Lspawn
    call .Lready
    call .Lrelease
    mov $7, %edi                  /* main ThreadExit; child is final exit(0) */
    mov $60, %eax
    syscall
    jmp .Lfail

.Lspawn:
    mov $5000, %ebx               /* retry only while owner cleanup finishes */
.Lretry:
    movq $0, (%r12)               /* tid + ready */
    movq $0, 8(%r12)              /* release + observed tid */
    call .Lclone
    test %rax, %rax
    jg .Lspawned
    cmp $-11, %rax
    jne .Lfail
    mov $35, %eax                 /* progress without spinning guest CPU */
    lea retry_delay(%rip), %rdi
    xor %esi, %esi
    syscall
    test %rax, %rax
    jne .Lfail
    dec %ebx
    jne .Lretry
    jmp .Lfail
.Lspawned:
    cmp (%r12), %eax              /* CLONE_PARENT_SETTID already published */
    jne .Lfail
    ret
.Lclone:
    mov $56, %eax
    mov $0x7d0f00, %edi
    mov 16(%r12), %rsi
    mov %r12, %rdx
    mov %r12, %r10
    lea 24(%r12), %r8
    syscall
    test %rax, %rax
    je .Lchild
    ret

.Lchild:
    mov $186, %eax
    syscall
    cmp (%r12), %eax              /* also catches execution before publication */
    jne .Lfail
    mov %eax, 12(%r12)
    cmpq $1234, %fs:0
    jne .Lfail
    mov $1, %eax
    xchg %eax, 4(%r12)
    lea 4(%r12), %rdi
    call .Lwake
.Lchild_wait:
    cmpl $1, 8(%r12)
    je .Lchild_exit
    lea 8(%r12), %rdi
    mov $128, %esi
    xor %edx, %edx
    call .Lwait
    jmp .Lchild_wait
.Lchild_exit:
    cmp $'j', %r15d
    jne .Lexit_child
.Lwait_main:
    mov main_tid(%rip), %edx
    test %edx, %edx
    je .Lexit_child
    lea main_tid(%rip), %rdi
    xor %esi, %esi
    call .Lwait
    jmp .Lwait_main
.Lexit_child:
    xor %edi, %edi
    mov $60, %eax
    syscall
    jmp .Lfail

.Lready:
    cmpl $1, 4(%r12)
    je .Lcheck_ready
    lea 4(%r12), %rdi
    mov $128, %esi
    xor %edx, %edx
    call .Lwait
    jmp .Lready
.Lcheck_ready:
    mov (%r12), %eax
    test %eax, %eax
    jle .Lfail
    cmp 12(%r12), %eax
    jne .Lfail
    ret
.Lrelease:
    mov $1, %eax
    xchg %eax, 8(%r12)
    lea 8(%r12), %rdi
    jmp .Lwake
.Lcleared:
    mov (%r12), %edx
    test %edx, %edx
    je .Lreturn
    mov %r12, %rdi
    xor %esi, %esi                /* clear-TID uses shared FUTEX_WAKE */
    call .Lwait
    jmp .Lcleared
.Lreturn:
    ret
.Lwait:
    xor %r10d, %r10d
    xor %r8d, %r8d
    xor %r9d, %r9d
    mov $202, %eax
    syscall
    test %rax, %rax
    je .Lreturn
    cmp $-11, %rax
    je .Lreturn
    cmp $-4, %rax
    je .Lreturn
    jmp .Lfail
.Lwake:
    mov $202, %eax
    mov $129, %esi
    mov $1, %edx
    syscall
    test %rax, %rax
    js .Lfail
    ret
.Lwrite:
    mov $1, %eax
    mov $1, %edi
    syscall
    cmp %rdx, %rax
    jne .Lfail
    ret
.Lfail:
    mov $13, %edi
.Lexit_group:
    mov $231, %eax
    syscall
    jmp .Lfail

.section .rodata
success: .ascii "guest-thread-churn: 96 cleared, capacity recovered\n"
success_end:
blocked: .ascii "guest-thread-churn: cancel ready\n"
blocked_end:
retry_delay: .quad 0, 1000000
.section .data
.balign 16
slot1: .long 0, 0, 0, 0
    .quad stack1_end, 1234, 0
.balign 16
slot2: .long 0, 0, 0, 0
    .quad stack2_end, 1234, 0
.balign 16
slot3: .long 0, 0, 0, 0
    .quad stack3_end, 1234, 0
.section .bss
.balign 16
never: .zero 16
main_tid: .zero 16
stack1: .zero 65536
stack1_end:
stack2: .zero 65536
stack2_end:
stack3: .zero 65536
stack3_end:
.section .note.GNU-stack,"",@progbits
