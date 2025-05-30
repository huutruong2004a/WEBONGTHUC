// JavaScript cải tiến cho User Navigation
document.addEventListener('DOMContentLoaded', function () {
    const userDropdown = document.querySelector('.user-dropdown');
    const dropdownContent = document.querySelector('.user-dropdown-content');
    const dropdownBtn = document.querySelector('.user-dropdown-btn');

    let isDropdownOpen = false;
    let hoverTimeout;

    if (userDropdown && dropdownContent && dropdownBtn) {
        // Click để toggle dropdown (alternative method)
        dropdownBtn.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            if (isDropdownOpen) {
                closeDropdown();
            } else {
                openDropdown();
            }
        });

        // Hover để mở dropdown
        userDropdown.addEventListener('mouseenter', function () {
            clearTimeout(hoverTimeout);
            openDropdown();
        });

        // Delay khi rời khỏi dropdown
        userDropdown.addEventListener('mouseleave', function () {
            hoverTimeout = setTimeout(() => {
                closeDropdown();
            }, 200); // 200ms delay
        });

        // Giữ dropdown mở khi hover vào content
        dropdownContent.addEventListener('mouseenter', function () {
            clearTimeout(hoverTimeout);
        });

        dropdownContent.addEventListener('mouseleave', function () {
            hoverTimeout = setTimeout(() => {
                closeDropdown();
            }, 200);
        });

        // Xử lý nested dropdown
        const nestedDropdowns = document.querySelectorAll('.user-dropdown-content .dropdown');
        nestedDropdowns.forEach(nested => {
            const nestedMenu = nested.querySelector('.dropdown-menu');
            if (nestedMenu) {
                let nestedTimeout;

                nested.addEventListener('mouseenter', function () {
                    clearTimeout(nestedTimeout);
                    nestedMenu.style.display = 'block';
                });

                nested.addEventListener('mouseleave', function () {
                    nestedTimeout = setTimeout(() => {
                        nestedMenu.style.display = 'none';
                    }, 150);
                });

                nestedMenu.addEventListener('mouseenter', function () {
                    clearTimeout(nestedTimeout);
                });

                nestedMenu.addEventListener('mouseleave', function () {
                    nestedTimeout = setTimeout(() => {
                        nestedMenu.style.display = 'none';
                    }, 150);
                });
            }
        });
    }

    function openDropdown() {
        if (dropdownContent) {
            dropdownContent.style.display = 'block';
            isDropdownOpen = true;
        }
    }

    function closeDropdown() {
        if (dropdownContent) {
            dropdownContent.style.display = 'none';
            isDropdownOpen = false;

            // Đóng tất cả nested dropdowns
            const nestedMenus = document.querySelectorAll('.user-dropdown-content .dropdown-menu');
            nestedMenus.forEach(menu => {
                menu.style.display = 'none';
            });
        }
    }

    // Đóng dropdown khi click bên ngoài
    document.addEventListener('click', function (event) {
        if (userDropdown && !userDropdown.contains(event.target)) {
            closeDropdown();
        }
    });

    // Đóng dropdown khi nhấn ESC
    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
            closeDropdown();
        }
    });

    // Xử lý form submit trong dropdown
    const logoutForm = document.querySelector('.user-dropdown-content form');
    if (logoutForm) {
        logoutForm.addEventListener('submit', function (e) {
            // Cho phép form submit bình thường
            console.log('Logging out...');
        });
    }

    // Mobile menu toggle
    if (window.innerWidth <= 576) {
        const navbarNav = document.querySelector('.user-nav .navbar-nav');
        if (navbarNav) {
            const toggleBtn = document.createElement('button');
            toggleBtn.innerHTML = '<i class="fas fa-bars"></i>';
            toggleBtn.className = 'mobile-toggle-btn';
            toggleBtn.style.cssText = `
                display: block;
                background: #ff9f43;
                border: none;
                color: white;
                padding: 8px 12px;
                border-radius: 6px;
                margin-left: 10px;
            `;

            toggleBtn.addEventListener('click', function () {
                navbarNav.classList.toggle('show');
            });

            userDropdown.parentNode.insertBefore(toggleBtn, navbarNav);
        }
    }
});