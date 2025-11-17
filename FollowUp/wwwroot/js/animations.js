/* ========================================
   FollowUp 交互动画脚本
   提供高级动画效果的 JavaScript 支持
   ======================================== */

// 全局动画配置
window.FollowUpAnimations = {
    // 卡片 3D 倾斜效果
    init3DTilt: function (selector) {
        const cards = document.querySelectorAll(selector || '.card-3d-tilt');

        cards.forEach(card => {
            card.addEventListener('mousemove', (e) => {
                const rect = card.getBoundingClientRect();
                const x = e.clientX - rect.left;
                const y = e.clientY - rect.top;

                const centerX = rect.width / 2;
                const centerY = rect.height / 2;

                const rotateX = (y - centerY) / 10;
                const rotateY = (centerX - x) / 10;

                card.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateZ(10px)`;
            });

            card.addEventListener('mouseleave', () => {
                card.style.transform = 'perspective(1000px) rotateX(0) rotateY(0) translateZ(0)';
            });
        });
    },

    // 数字滚动动画（备用，优先使用 Blazor 组件内置）
    animateCounter: function (element, targetValue, duration = 1000) {
        if (!element) return;

        const start = 0;
        const increment = targetValue / (duration / 16);
        let current = start;

        const timer = setInterval(() => {
            current += increment;
            if (current >= targetValue) {
                current = targetValue;
                clearInterval(timer);
            }

            if (Number.isInteger(targetValue)) {
                element.textContent = Math.floor(current);
            } else {
                element.textContent = current.toFixed(1);
            }
        }, 16);
    },

    // 涟漪效果（用于按钮）
    addRippleEffect: function (selector) {
        const buttons = document.querySelectorAll(selector || '.ripple-btn');

        buttons.forEach(button => {
            button.addEventListener('click', function (e) {
                const ripple = document.createElement('span');
                const rect = this.getBoundingClientRect();

                const size = Math.max(rect.width, rect.height);
                const x = e.clientX - rect.left - size / 2;
                const y = e.clientY - rect.top - size / 2;

                ripple.style.cssText = `
                    position: absolute;
                    border-radius: 50%;
                    background: rgba(255, 255, 255, 0.4);
                    width: ${size}px;
                    height: ${size}px;
                    left: ${x}px;
                    top: ${y}px;
                    transform: scale(0);
                    animation: rippleEffect 0.6s ease-out;
                    pointer-events: none;
                `;

                this.style.position = 'relative';
                this.style.overflow = 'hidden';
                this.appendChild(ripple);

                setTimeout(() => ripple.remove(), 600);
            });
        });

        // 添加涟漪动画关键帧
        if (!document.querySelector('#ripple-keyframes')) {
            const style = document.createElement('style');
            style.id = 'ripple-keyframes';
            style.textContent = `
                @keyframes rippleEffect {
                    to {
                        transform: scale(4);
                        opacity: 0;
                    }
                }
            `;
            document.head.appendChild(style);
        }
    },

    // 滚动触发动画
    initScrollAnimations: function () {
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-in');
                    observer.unobserve(entry.target);
                }
            });
        }, observerOptions);

        document.querySelectorAll('.scroll-animate').forEach(el => {
            observer.observe(el);
        });
    },

    // 打字机效果
    typeWriter: function (element, text, speed = 50) {
        if (!element) return;

        let i = 0;
        element.textContent = '';

        function type() {
            if (i < text.length) {
                element.textContent += text.charAt(i);
                i++;
                setTimeout(type, speed);
            }
        }

        type();
    },

    // 粒子背景效果（医疗主题）
    initParticles: function (canvasId) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        const ctx = canvas.getContext('2d');
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;

        const particles = [];
        const particleCount = 50;

        for (let i = 0; i < particleCount; i++) {
            particles.push({
                x: Math.random() * canvas.width,
                y: Math.random() * canvas.height,
                radius: Math.random() * 3 + 1,
                speedX: Math.random() * 0.5 - 0.25,
                speedY: Math.random() * 0.5 - 0.25,
                opacity: Math.random() * 0.5 + 0.2
            });
        }

        function animate() {
            ctx.clearRect(0, 0, canvas.width, canvas.height);

            particles.forEach(p => {
                p.x += p.speedX;
                p.y += p.speedY;

                if (p.x < 0 || p.x > canvas.width) p.speedX *= -1;
                if (p.y < 0 || p.y > canvas.height) p.speedY *= -1;

                ctx.beginPath();
                ctx.arc(p.x, p.y, p.radius, 0, Math.PI * 2);
                ctx.fillStyle = `rgba(24, 183, 151, ${p.opacity})`;
                ctx.fill();
            });

            requestAnimationFrame(animate);
        }

        animate();

        window.addEventListener('resize', () => {
            canvas.width = window.innerWidth;
            canvas.height = window.innerHeight;
        });
    },

    // 进度环动画
    animateProgressRing: function (element, percentage) {
        if (!element) return;

        const radius = 45;
        const circumference = 2 * Math.PI * radius;
        const offset = circumference - (percentage / 100) * circumference;

        element.style.strokeDasharray = circumference;
        element.style.strokeDashoffset = circumference;

        setTimeout(() => {
            element.style.transition = 'stroke-dashoffset 1.5s ease-out';
            element.style.strokeDashoffset = offset;
        }, 100);
    },

    // 通知滑入
    showNotification: function (message, type = 'info', duration = 3000) {
        const notification = document.createElement('div');
        notification.className = `notification notification-${type} notification-slide`;
        notification.textContent = message;

        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 16px 24px;
            border-radius: 8px;
            color: white;
            font-weight: 500;
            z-index: 9999;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        `;

        const colors = {
            info: '#4299E1',
            success: '#48BB78',
            warning: '#ED8936',
            error: '#F56565'
        };

        notification.style.backgroundColor = colors[type] || colors.info;

        document.body.appendChild(notification);

        setTimeout(() => {
            notification.classList.add('notification-fade-out');
            setTimeout(() => notification.remove(), 350);
        }, duration);
    },

    // 页面切换过渡
    pageTransition: function (callback) {
        const content = document.querySelector('.main-content');
        if (!content) {
            if (callback) callback();
            return;
        }

        content.style.opacity = '0';
        content.style.transform = 'translateY(20px)';

        setTimeout(() => {
            if (callback) callback();

            content.style.transition = 'all 0.35s ease-out';
            content.style.opacity = '1';
            content.style.transform = 'translateY(0)';
        }, 100);
    },

    // 初始化所有动画
    initAll: function () {
        this.init3DTilt();
        this.addRippleEffect();
        this.initScrollAnimations();

        console.log('FollowUp Animations initialized');
    }
};

// 自动初始化（可选）
document.addEventListener('DOMContentLoaded', () => {
    // 延迟初始化，确保 Blazor 渲染完成
    setTimeout(() => {
        window.FollowUpAnimations.initAll();
    }, 500);
});

