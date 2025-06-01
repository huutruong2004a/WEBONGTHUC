// Wait for DOM to be fully loaded
document.addEventListener('DOMContentLoaded', function () {
    // Mobile menu toggle
    const mobileMenuBtn = document.querySelector('.mobile-menu-btn');
    const navLinks = document.querySelector('.nav-links');

    if (mobileMenuBtn) {
        mobileMenuBtn.addEventListener('click', function () {
            navLinks.classList.toggle('show');
        });
    }

    // Recipe tabs functionality
    const tabLinks = document.querySelectorAll('.tabs-list li a');
    const tabContents = document.querySelectorAll('.tab-content');

    tabLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();

            // Remove active class from all tabs
            tabLinks.forEach(link => {
                link.parentElement.classList.remove('active');
            });

            // Hide all tab contents
            tabContents.forEach(content => {
                content.classList.remove('active');
            });

            // Add active class to clicked tab
            this.parentElement.classList.add('active');

            // Show corresponding tab content
            const targetId = this.getAttribute('href').substring(1);
            document.getElementById(targetId).classList.add('active');
        });
    });

    // Favorite button functionality
    const favoriteButtons = document.querySelectorAll('.favorite-btn');

    favoriteButtons.forEach(button => {
        button.addEventListener('click', function () {
            this.classList.toggle('active');
            const icon = this.querySelector('i');

            if (this.classList.contains('active')) {
                icon.classList.remove('far');
                icon.classList.add('fas');
                icon.style.color = '#f97316';
            } else {
                icon.classList.remove('fas');
                icon.classList.add('far');
                icon.style.color = '';
            }
        });
    });

    // Lazy loading images
    if ('IntersectionObserver' in window) {
        const imgObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    const src = img.getAttribute('data-src');

                    if (src) {
                        img.src = src;
                        img.removeAttribute('data-src');
                    }

                    observer.unobserve(img);
                }
            });
        });

        document.querySelectorAll('img[data-src]').forEach(img => {
            imgObserver.observe(img);
        });
    }

    // Search functionality - REMOVE OR COMMENT OUT THIS ENTIRE BLOCK
    // const searchInput = document.querySelector('.search-box input'); // This selector might target the wrong input now
    // const layoutSearchForm = document.getElementById('layoutSearchForm'); // Use the form ID

    // if (searchInput && layoutSearchForm) { // Check for both input and form
    //     searchInput.addEventListener('keypress', function (e) {
    //         if (e.key === 'Enter') {
    //             // e.preventDefault(); // Prevent default only if we are handling it here, otherwise let the form submit
    //             // const searchTerm = this.value.trim();
    //             // if (searchTerm) {
    //                 // alert(`Searching for: ${searchTerm}`); // Remove alert
    //                 // layoutSearchForm.submit(); // Optionally submit the form via JS if needed
    //             // } else {
    //                 // e.preventDefault(); // Prevent submitting an empty form if Enter is pressed on empty input
    //             // }
    //         }
    //     });
    // }
    // END OF BLOCK TO REMOVE/COMMENT
});

// Authentication page functionality
document.addEventListener('DOMContentLoaded', function () {
    // Password visibility toggle
    const passwordInputs = document.querySelectorAll('input[type="password"]');

    passwordInputs.forEach(input => {
        const parent = input.parentElement;

        // Create toggle button
        const toggleBtn = document.createElement('button');
        toggleBtn.type = 'button';
        toggleBtn.className = 'password-toggle';
        toggleBtn.innerHTML = '<i class="fas fa-eye"></i>';
        toggleBtn.setAttribute('aria-label', 'Toggle password visibility');

        // Add toggle button to input group
        parent.appendChild(toggleBtn);

        // Add event listener
        toggleBtn.addEventListener('click', function () {
            const type = input.getAttribute('type') === 'password' ? 'text' : 'password';
            input.setAttribute('type', type);

            // Toggle icon
            const icon = this.querySelector('i');
            icon.classList.toggle('fa-eye');
            icon.classList.toggle('fa-eye-slash');
        });
    });

    // Mobile user menu toggle
    const userDropdownBtn = document.querySelector('.user-dropdown-btn');
    const userDropdownContent = document.querySelector('.user-dropdown-content');

    if (userDropdownBtn && userDropdownContent) {
        userDropdownBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            userDropdownContent.classList.toggle('show');
        });

        // Close dropdown when clicking outside
        document.addEventListener('click', function () {
            if (userDropdownContent.classList.contains('show')) {
                userDropdownContent.classList.remove('show');
            }
        });
    }
});

document.addEventListener('DOMContentLoaded', function () {
    // Mobile menu toggle (this might be a duplicate if you have it above, ensure only one instance)
    const mobileMenuBtn = document.querySelector('.mobile-menu-btn');
    const navLinksUl = document.querySelector('.nav-links'); // Changed to select the UL

    if (mobileMenuBtn && navLinksUl) {
        mobileMenuBtn.addEventListener('click', function () {
            navLinksUl.classList.toggle('show');
        });
    }

    // Active navigation link based on current URL
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.main-nav .nav-links li a');

    navLinks.forEach(link => {
        link.classList.remove('active'); // Remove active from all first
        let linkPath = link.getAttribute('href');

        // Normalize paths for comparison (e.g., ensure leading/trailing slashes are consistent or handled)
        // For simple comparison:
        // If currentPath is just "/" (homepage), and linkPath is also "/", it's a match.
        // Otherwise, for other pages, check if currentPath starts with the linkPath (if linkPath is not just "/").
        if (linkPath === currentPath) {
            link.classList.add('active');
        } else if (linkPath !== "/" && currentPath.startsWith(linkPath)) {
            // This handles cases like /recipes/details matching /recipes
            // However, be careful if you have similar starting paths e.g. /blog and /blog-archive
            // For more robust matching, you might need to compare segments or use a more specific logic.
            // For now, let's assume direct match or homepage match is sufficient for most cases.
            // If currentPath is /recipes/details and linkPath is /recipes, add active
            if (currentPath.startsWith(linkPath + (linkPath.endsWith('/') ? '' : '/')) || currentPath === linkPath) {
                 // A more specific check to avoid /blog activating for /blog-posts if /blog is a link
                 // This is a simple check, might need refinement based on your exact URL structures and nav links
            }
        }
    });

    // If no specific link matched (e.g., on a sub-page not directly in nav), but we are in a section
    // Example: if on /recipes/details/123, and there's a /recipes link, highlight /recipes
    // This part requires more specific logic based on your site structure.
    // A simplified approach for now:
    if (!document.querySelector('.main-nav .nav-links li a.active')) {
        navLinks.forEach(link => {
            let linkPath = link.getAttribute('href');
            if (linkPath !== "/" && currentPath.startsWith(linkPath)) {
                // Check if the current path starts with the link path, but is longer
                if (currentPath.length > linkPath.length && currentPath.charAt(linkPath.length) === '/') {
                    link.classList.add('active');
                }
            }
        });
    }
     // Special case for homepage if nothing else is active and path is "/"
    if (!document.querySelector('.main-nav .nav-links li a.active') && currentPath === "/"){
        const homeLink = document.querySelector('.main-nav .nav-links li a[href="/"]');
        if(homeLink) homeLink.classList.add('active');
    }
});
