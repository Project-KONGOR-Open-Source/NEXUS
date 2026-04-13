let documentClickHandler = null;

export function initialise() {
    documentClickHandler = function (event) {
        document.querySelectorAll("details.portal-nav-disclosure").forEach(function (details) {
            if (!details.contains(event.target)) {
                details.removeAttribute("open");
            }
        });
    };

    document.addEventListener("click", documentClickHandler);
}

export function dispose() {
    if (documentClickHandler !== null) {
        document.removeEventListener("click", documentClickHandler);
        documentClickHandler = null;
    }
}
