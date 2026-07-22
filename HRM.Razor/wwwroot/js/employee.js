/* HRM.Razor - Employee Management Script (UC05 & UC06) */

document.addEventListener('DOMContentLoaded', function () {
    // 1. Status Change Modal Logic (UC06)
    const btnOpenModal = document.getElementById('btn-open-status-modal');
    const modalOverlay = document.getElementById('status-change-modal');
    const btnCancelModal = document.getElementById('btn-close-status-modal');
    const statusSelect = document.getElementById('TargetStatusSelect');
    const targetStatusHidden = document.getElementById('TargetStatusHidden');
    const targetStatusText = document.getElementById('target-status-text');

    if (btnOpenModal && modalOverlay && statusSelect) {
        btnOpenModal.addEventListener('click', function (e) {
            e.preventDefault();
            const selectedStatus = statusSelect.value;
            if (!selectedStatus) return;

            if (targetStatusHidden) {
                targetStatusHidden.value = selectedStatus;
            }

            if (targetStatusText) {
                const textMap = {
                    'ACTIVE': 'Đang làm việc',
                    'ON_LEAVE': 'Đang nghỉ',
                    'RESIGNED': 'Đã nghỉ việc',
                    'TERMINATED': 'Đã chấm dứt'
                };
                targetStatusText.textContent = textMap[selectedStatus] || selectedStatus;
            }

            // Mở Modal nếu trạng thái là RESIGNED hoặc TERMINATED
            if (selectedStatus === 'RESIGNED' || selectedStatus === 'TERMINATED') {
                modalOverlay.classList.add('show');
            } else {
                // Đổi trạng thái trực tiếp nếu là ACTIVE hoặc ON_LEAVE
                const form = document.getElementById('status-change-form');
                if (form) form.submit();
            }
        });
    }

    if (btnCancelModal && modalOverlay) {
        btnCancelModal.addEventListener('click', function () {
            modalOverlay.classList.remove('show');
        });

        modalOverlay.addEventListener('click', function (e) {
            if (e.target === modalOverlay) {
                modalOverlay.classList.remove('show');
            }
        });
    }

    // 2. Client-side Date Check (DateOfBirth < HireDate)
    const dobInput = document.getElementById('Input_DateOfBirth');
    const hireDateInput = document.getElementById('Input_HireDate');

    if (dobInput && hireDateInput) {
        function validateDates() {
            if (dobInput.value && hireDateInput.value) {
                if (new Date(dobInput.value) >= new Date(hireDateInput.value)) {
                    hireDateInput.setCustomValidity('Ngày vào làm phải sau ngày sinh.');
                } else {
                    hireDateInput.setCustomValidity('');
                }
            }
        }

        dobInput.addEventListener('change', validateDates);
        hireDateInput.addEventListener('change', validateDates);
    }
});
