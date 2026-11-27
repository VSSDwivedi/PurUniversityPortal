/**
 * Purbanchal University Examination Portal
 * Common JavaScript for Admin and Student Dashboards
 */

(function () {
    'use strict';

    function initDashboardSidebar() {
        var sidebar = document.getElementById('dashboardSidebar');
        var sidebarToggle = document.getElementById('sidebarToggle');
        var sidebarCloseBtn = document.getElementById('sidebarCloseBtn');
        var backdrop = document.getElementById('sidebarBackdrop');

        if (!sidebar) return;

        function openSidebar() {
            sidebar.classList.add('sidebar-open');
            if (backdrop) backdrop.classList.add('show');
            document.body.style.overflow = 'hidden';
        }

        function closeSidebar() {
            sidebar.classList.remove('sidebar-open');
            if (backdrop) backdrop.classList.remove('show');
            document.body.style.overflow = '';
        }

        if (sidebarToggle) {
            sidebarToggle.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                if (sidebar.classList.contains('sidebar-open')) {
                    closeSidebar();
                } else {
                    openSidebar();
                }
            });
        }

        if (sidebarCloseBtn) {
            sidebarCloseBtn.addEventListener('click', function (e) {
                e.preventDefault();
                closeSidebar();
            });
        }

        if (backdrop) {
            backdrop.addEventListener('click', function () {
                closeSidebar();
            });
        }

        // Close on ESC key
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && sidebar.classList.contains('sidebar-open')) {
                closeSidebar();
            }
        });

        // Close sidebar when navigating via nav-links on mobile (< 992px)
        var navLinks = sidebar.querySelectorAll('.nav-link');
        navLinks.forEach(function (link) {
            link.addEventListener('click', function () {
                if (window.innerWidth < 992) {
                    closeSidebar();
                }
            });
        });

        // Auto close if window resized to desktop
        window.addEventListener('resize', function () {
            if (window.innerWidth >= 992 && sidebar.classList.contains('sidebar-open')) {
                closeSidebar();
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initDashboardSidebar);
    } else {
        initDashboardSidebar();
    }
})();
