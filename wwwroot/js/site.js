// Basic toast / notification system for XP and streak events

window.learnEarnToast = (function () {
    function ensureContainer() {
        let c = document.getElementById('learnEarn-toast-container');
        if (!c) {
            c = document.createElement('div');
            c.id = 'learnEarn-toast-container';
            c.style.position = 'fixed';
            c.style.right = '1.5rem';
            c.style.bottom = '1.5rem';
            c.style.zIndex = '1080';
            document.body.appendChild(c);
        }
        return c;
    }

    function show(message, type) {
        const container = ensureContainer();
        const wrapper = document.createElement('div');
        wrapper.className = 'learnEarn-toast shadow-lg mb-2';
        wrapper.innerHTML = `
<div class="toast-body">
  <div class="fw-semibold mb-1">${type === 'quiz' ? 'Quiz updated' : 'Lesson updated'}</div>
  <div>${message}</div>
</div>`;
        container.appendChild(wrapper);
        setTimeout(() => {
            wrapper.classList.add('show');
        }, 10);
        setTimeout(() => {
            wrapper.classList.remove('show');
            setTimeout(() => wrapper.remove(), 300);
        }, 4200);
    }

    return { show };
})();

// Auto-trigger toasts from data attributes rendered on the page (TempData)
document.addEventListener('DOMContentLoaded', function () {
    const el = document.querySelector('[data-xp-toast]');
    if (el && window.learnEarnToast) {
        const msg = el.getAttribute('data-xp-toast');
        const kind = el.getAttribute('data-xp-kind') || 'lesson';
        if (msg) {
            window.learnEarnToast.show(msg, kind);
        }
    }
});
