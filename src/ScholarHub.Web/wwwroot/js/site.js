// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('input[type="password"]').forEach(function (input) {
        var container = input.closest('.form-floating') || input.parentElement;
        if (!container || container.querySelector('.sh-password-toggle')) {
            return;
        }

        var btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'sh-password-toggle';
        btn.setAttribute('aria-label', 'Show password');
        btn.innerHTML = '<i class="bi bi-eye"></i>';
        container.appendChild(btn);

        btn.addEventListener('click', function () {
            var willShow = input.type === 'password';
            input.type = willShow ? 'text' : 'password';
            btn.innerHTML = willShow ? '<i class="bi bi-eye-slash"></i>' : '<i class="bi bi-eye"></i>';
            btn.setAttribute('aria-label', willShow ? 'Hide password' : 'Show password');
        });
    });
});
