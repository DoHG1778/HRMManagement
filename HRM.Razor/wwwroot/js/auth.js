/* HRM.Razor - Authentication Interactive Script */

document.addEventListener('DOMContentLoaded', function () {
    // 1. Password Visibility Toggle
    const toggleButtons = document.querySelectorAll('.btn-toggle-password');
    toggleButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            const targetId = this.getAttribute('data-target');
            const input = document.getElementById(targetId);
            if (!input) return;

            const icon = this.querySelector('i');
            if (input.type === 'password') {
                input.type = 'text';
                if (icon) {
                    icon.classList.remove('bi-eye');
                    icon.classList.add('bi-eye-slash');
                }
            } else {
                input.type = 'password';
                if (icon) {
                    icon.classList.remove('bi-eye-slash');
                    icon.classList.add('bi-eye');
                }
            }
        });
    });

    // 2. Realtime Password Strength Checklist Validator
    const newPasswordInput = document.getElementById('NewPassword');
    const currentPasswordInput = document.getElementById('CurrentPassword');

    if (newPasswordInput) {
        const checkLength = document.getElementById('check-length');
        const checkComplexity = document.getElementById('check-complexity');
        const checkDifferent = document.getElementById('check-different');

        function validatePasswordChecklist() {
            const val = newPasswordInput.value;
            const currentVal = currentPasswordInput ? currentPasswordInput.value : '';

            // 1. Length >= 8
            if (checkLength) {
                if (val.length >= 8) {
                    checkLength.classList.add('valid');
                    checkLength.querySelector('i').className = 'bi bi-check-circle-fill';
                } else {
                    checkLength.classList.remove('valid');
                    checkLength.querySelector('i').className = 'bi bi-circle';
                }
            }

            // 2. Upper, lower, digit, special character
            if (checkComplexity) {
                const hasUpper = /[A-Z]/.test(val);
                const hasLower = /[a-z]/.test(val);
                const hasDigit = /[0-9]/.test(val);
                const hasSpecial = /[^A-Za-z0-9]/.test(val);

                if (hasUpper && hasLower && hasDigit && hasSpecial) {
                    checkComplexity.classList.add('valid');
                    checkComplexity.querySelector('i').className = 'bi bi-check-circle-fill';
                } else {
                    checkComplexity.classList.remove('valid');
                    checkComplexity.querySelector('i').className = 'bi bi-circle';
                }
            }

            // 3. Different from current password
            if (checkDifferent) {
                if (val.length > 0 && val !== currentVal) {
                    checkDifferent.classList.add('valid');
                    checkDifferent.querySelector('i').className = 'bi bi-check-circle-fill';
                } else {
                    checkDifferent.classList.remove('valid');
                    checkDifferent.querySelector('i').className = 'bi bi-circle';
                }
            }
        }

        newPasswordInput.addEventListener('input', validatePasswordChecklist);
        if (currentPasswordInput) {
            currentPasswordInput.addEventListener('input', validatePasswordChecklist);
        }
    }

    // 3. Disable submit button and show spinner on form submission
    const authForms = document.querySelectorAll('.auth-form');
    authForms.forEach(form => {
        form.addEventListener('submit', function () {
            if ($(this).valid && !$(this).valid()) {
                return;
            }
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                const spinner = submitBtn.querySelector('.spinner-border');
                if (spinner) {
                    spinner.classList.remove('d-none');
                }
            }

            const overlay = document.getElementById('app-loading-overlay');
            if (overlay) {
                overlay.style.display = 'flex';
            }
        });
    });
});
