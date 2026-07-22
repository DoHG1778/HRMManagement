document.addEventListener("DOMContentLoaded", function () {
    const sidebar = document.getElementById("app-sidebar");
    const sidebarNav = document.getElementById("sidebar-nav");
    const storageKey = "hrm-sidebar-scroll-top";

    const scrollTarget = sidebarNav || sidebar;

    if (scrollTarget) {
        const savedPosition = sessionStorage.getItem(storageKey);

        if (savedPosition !== null) {
            requestAnimationFrame(function () {
                scrollTarget.scrollTop = Number(savedPosition) || 0;
            });
        }

        const saveScrollPosition = function () {
            sessionStorage.setItem(
                storageKey,
                String(scrollTarget.scrollTop)
            );
        };

        scrollTarget.addEventListener(
            "scroll",
            saveScrollPosition,
            { passive: true }
        );

        window.addEventListener(
            "beforeunload",
            saveScrollPosition
        );

        document
            .querySelectorAll(".sidebar-nav a.nav-item[href]")
            .forEach(function (link) {
                link.addEventListener("click", function () {
                    saveScrollPosition();
                    // Do not call preventDefault().
                    // The browser must continue navigating normally.
                });
            });
    }
});
