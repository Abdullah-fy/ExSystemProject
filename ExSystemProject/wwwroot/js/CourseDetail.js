window.onload = function () {
    const toastEl = document.querySelector('.toast');
    if (toastEl) {
        new bootstrap.Toast(toastEl).show();
    }
};